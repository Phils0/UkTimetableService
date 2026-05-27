# Station Group Search in UkTimetableService — Plan

## Goal

Add support for Silverrail station group codes (e.g. `GB@LO`, `GB@MA`, `GB^PH`) to `/api/timetable/departures/{origin}/{date}` and `/api/timetable/arrivals/{dest}/{date}` (with their `?to=` / `?from=` query parameters), so that callers can search for departures from/to "London (all)" or "Manchester (all)" without enumerating member CRS codes themselves. Existing CRS-only behaviour stays bit-for-bit identical.

The work spans **two repos**:
- The **core** `UkTimetableService` — gains a generic, vendor-neutral "station groups" capability with a configurable per-group dedup strategy.
- The Seatfrog **wrapper** `uk-timetable-service` — gains a small Dockerfile step that produces the group-data file from Silverrail's `locations.json` at build time and sets the dedup-strategy switch from an Unleash flag.

Splitting the work this way keeps the core repo **client-agnostic** — it has no knowledge of Silverrail's schema, Seatfrog URLs, or our delimiter conventions. The core just consumes a small neutral JSON file. Anything Silverrail-specific lives in the wrapper, where it belongs.

## Where the work lives

In the **core repo** (`/Users/rob/repos/UkTimetableService`, `phils0/UkTimetableService`, `net8` branch) for the feature itself, with a small companion change in `/Users/rob/repos/uk-timetable-service` (the wrapper's Dockerfile) to produce the group-data file. Rationale for the core/wrapper split:

- The wrapper has no app code, no plugin DLL output, and the core's `EnableCustomPlugins` flag is off in production. Doing it entirely in the wrapper would require either turning on the plugin system (substantial change) or building a separate proxy service (much larger architectural change).
- The skills-output README of the wrapper explicitly disclaims app logic: "This service IS NOT responsible for the timetable API application code (owned by phils0/UkTimetableService)."
- The natural extension points (`ILocationData`, `IFilterFactory`, controllers) all live in the core.
- We want the core changes to remain client-agnostic so they can plausibly be merged upstream — hence the neutral schema and disk-based loading described below.

---

## Reference-data design (the neutral schema)

The core repo owns a small, vendor-neutral file format that captures the **only** things it needs to know about groups: members, and (optionally) a priority ordering used for dedup overrides.

**`Data/station-groups.json`** (lives alongside `Data/stations.xml`, `Data/tocs.xml`, `Data/<timetable>.zip` in the cloned core source tree):

```json
{
  "groups": [
    { "code": "GB@LO", "members": ["EUS", "KGX", "LST", "CTK", "..."] },
    { "code": "GB@MA", "members": ["MAN", "MCV", "MCO"], "priorities": ["MAN"] },
    { "code": "GB#AC", "members": ["ACC", "AML", "SAT"] },
    { "code": "GB#RE", "members": ["RDG", "RDW"], "priorities": ["RDG"] },
    { "code": "GB@BH", "members": ["BMO", "BSW", "BHM"] },
    { "code": "GB@BR", "members": ["BRI", "BPW"], "priorities": ["BRI"] }
  ]
}
```

Field semantics:
- `code` — the group code as it appears in inbound requests (e.g. path or `?to=` parameter). Case-insensitive at lookup time.
- `members` — CRS codes of stations belonging to the group. Order is not significant. Case-insensitive.
- `priorities` *(optional)* — ordered list of CRS codes that should be preferred during dedup, **first one matching a service's stops wins**. May be a subset of `members`. Omit the field entirely (or supply an empty array) to mean "no priority overrides — use the configured heuristic".

**Invariants** (validated at load time, warn-and-skip on violation):
- Every CRS in `priorities` must also be in `members`. References to non-members are logged and dropped.
- `code` values are unique.
- All `members` are non-empty strings.

**No equal-priority support.** Within a group, priority is a strict total order. If a genuine tie-case emerges later, the schema can extend to nested arrays (`"priorities": [["A"], ["B", "C"]]`) — but every current group is either strictly ordered or has no priorities at all, so we keep the flat-array form for readability.

### How the wrapper produces the file

The wrapper's Dockerfile adds a build step that fetches Silverrail's `locations.json`, projects it into the neutral shape, and merges in the priority overrides (which Seatfrog owns):

```dockerfile
ARG LOCATIONS_JSON_URL=https://reference.seatfrog.com/locations.json
COPY config/station-group-priorities.json /tmp/priorities.json
RUN apt-get update && apt-get install -y --no-install-recommends jq curl \
 && curl -fsSL "$LOCATIONS_JSON_URL" \
    | jq --slurpfile p /tmp/priorities.json '{
        groups: [
          .locations[]
          | select(.isInputGroup == true)
          | { code: .code, members: .members }
          | . + ( $p[0][.code] // {} )
        ]
      }' \
    > app/UkTimetableService/Timetable.Web/Data/station-groups.json
```

(Illustrative — exact `jq` filter verified against the real `locations.json` during implementation.)

`config/station-group-priorities.json` is a tiny Seatfrog-owned file in the wrapper repo, e.g.:

```json
{
  "GB#RE": { "priorities": ["RDG"] },
  "GB@BR": { "priorities": ["BRI"] },
  "GB@MA": { "priorities": ["MAN"] }
}
```

This keeps Seatfrog's editorial overrides in version control under the wrapper repo, separate from the upstream Silverrail data. The wrapper rebuilds hourly via `*/60 * * * *` cron in `build.yml`, so upstream `locations.json` changes propagate automatically; priority changes propagate on PR merge.

The initial overrides list is deliberately **minimal** — only the groups whose preferred station has been confirmed via Silverrail-behaviour testing are included. New entries are added as evidence accumulates (see "Remaining open data questions" below). The priorities file is a data change, not a code change, so additions go through a small PR review and are picked up by the next hourly build with no service redeploy.

**If `station-groups.json` is absent at runtime**, the feature is silently disabled — group code lookups return 404 (`LocationNotFound`) and the service starts normally, logging "station-groups.json not found at {path} — station group search disabled". Matches the optional-file handling pattern for other reference files.

---

## Deduplication strategy

When a service calls at multiple group members (e.g. a train stopping MCO → MCV → MAN within `GB@MA`), the naïve expansion produces N duplicate results. The deduplication strategy collapses those duplicates per service using a **priority-override pass** followed by a **journey heuristic fallback**.

### Algorithm

For each `(RetailServiceId, Service.On)` group of same-service candidates:

1. **Priority pass.** If the resolved group has a non-empty `priorities` list, walk it in order. For each priority CRS, look for a candidate whose relevant stop (origin stop for origin-side groups; destination stop for destination-side groups) is at that CRS. The first match wins.
2. **Heuristic fallback.** If no priority matched (or `priorities` was empty/absent), apply the configured `JourneyHeuristic`:

   | Heuristic | Origin group (path param) | Destination group (`?to=` / `?from=`) |
   |---|---|---|
   | `Longest` | Earliest departure within group (train traverses more of the group) | Latest arrival within group |
   | `Shortest` | Latest departure within group | Earliest arrival within group |

The heuristic is a single configuration value (`StationGroupDeduplicationStrategy` in `appsettings.json`, value `Longest` or `Shortest`). One choice applies globally for the running instance.

### Why configurable?

There's a genuine product question about which heuristic produces a better customer experience. The longest-journey view shows the "main" terminus (often the canonical one for a city); shortest-journey shows the first opportunity to alight (potentially more convenient for a passenger heading to a less-central area). Until we have data, we want to be able to switch — and the wrapper's existing `scripts/update_appsettings` step can patch this value from an Unleash flag at build time. The hourly rebuild cadence means a flag flip propagates within ~1 hour, no code change required.

### Why not configurable per group?

Considered and rejected. The priorities list already covers every case where the global heuristic produces the "wrong" station — if a group has a station that should win regardless of journey length, you encode it in `priorities`. There's no plausible case where a group has no clear preferred station yet still needs the opposite heuristic from what the rest of the service uses. The longest/shortest choice is closer to a property of the user's preference (consistent across cities) than a property of any particular group.

If a counter-example ever emerges, the strategy interface already takes the heuristic as a parameter, so adding per-group config is a small extension — but we shouldn't build it speculatively.

### Why priority overrides on top?

Pure heuristics miss cases where there's a clear editorial preference that doesn't follow from "longest" / "shortest" journey alone. For example:
- **Reading (`GB#RE`)**: passengers expect "Reading" to mean RDG — the heritage main-line station — not RDW (Reading West). Silverrail-behaviour testing confirmed RDG is the preferred station regardless of journey direction or whether the train terminates there. Setting `priorities: ["RDG"]` guarantees this regardless of which heuristic is configured.
- **Bristol (`GB@BR`)**: BRI (Temple Meads, central) is the preferred station; BPW (Parkway, on the outskirts) is a destination most customers would only choose deliberately. Silverrail-behaviour testing confirmed BRI wins in every observed pairing. Setting `priorities: ["BRI"]` codifies this.
- **Manchester (`GB@MA`)**: MAN (Piccadilly) is the canonical Manchester terminus. Silverrail-behaviour testing showed MAN is consistently displayed over MCO (Oxford Road) when a service calls at both, **even when that gives a shorter journey**. Setting `priorities: ["MAN"]` codifies this — this is exactly the kind of case where the longest-journey heuristic alone would produce the wrong answer.

For groups where Silverrail behaviour matches what the heuristic alone would produce — Birmingham (`GB@BH`) is a clear example: testing showed no direct services call at both BHM and the smaller stations, and services that do call at both BMO and BSW use longest-journey — no `priorities` entry is needed. The heuristic does the right thing on its own.

For small/suburban groups like Acton (`GB#AC`) and Ardrossan (`GB#AR`), the heuristic also takes full responsibility. Those are cases where the member stations are largely interchangeable from a customer's perspective, so neither "longest" nor "shortest" produces a controversial answer.

**Approach**: start with the priorities we have evidence for, and add new entries as customer signal or further testing reveals them. The priorities file is a data change, not a code change.

### Why not the UK Routeing Guide?

The Routeing Guide's "Station Group" concept with a designated "main station" (G01 → EUS, G20 → MAN, etc.) was considered, and Brain MCP queries confirmed the data is well-defined. But on reflection: routeing groups exist for ticket validity and route-map calculation, not for timetable display. The Routeing Guide groups also only cluster routeing points, leaving suburban members (e.g. ACC, SRA-as-part-of-London) without coverage and requiring a fallback anyway. We chose to handle the override behaviour ourselves through the explicit `priorities` list, which is editorially controlled and applies uniformly across all groups Seatfrog cares about. This decision is reversible — the `priorities` data could be populated from Routeing Guide data in future if we change our minds.

---

## Implementation

### New types (in `Timetable/` project)

| File | Purpose |
|---|---|
| `StationGroup.cs` | Immutable record: `Code`, `Members` (IReadOnlySet<string>), `Priorities` (IReadOnlyList<string>?). Case-insensitive set/list. |
| `IStationGroupResolver.cs` | `bool TryGet(string code, out StationGroup group)` — single case-insensitive lookup |
| `StationGroupResolver.cs` | Implementation. In-memory `Dictionary<string, StationGroup>` (case-insensitive comparer) populated at startup |
| `JourneyHeuristic.cs` | Enum: `Longest`, `Shortest` |
| `IStationGroupDeduplicator.cs` | `Deduplicate(IEnumerable<ResolvedServiceStop>, IReadOnlySet<Station>, bool groupIsDestination, string groupCode = null)` |
| `StationGroupDeduplicator.cs` | Single implementation: priority-pass then heuristic fallback. `JourneyHeuristic` injected via ctor |

> **No `IsGroupShaped(string)` method.** The core has no opinion about what group codes "look like" — it just tries an exact-match (case-insensitive) lookup against the loaded data. See "Validation & error mapping" below.

### Modified types

| File | Change |
|---|---|
| `Timetable/ResolvedServiceStop.cs` | Add `bool GoesToAnyOf(IReadOnlySet<Station>)` and `bool ComesFromAnyOf(IReadOnlySet<Station>)`. Each does a **single** backward/forward scan of the service's stops, checking set membership at each stop. Same `O(S)` cost as the single-destination variants. |
| `Timetable/GatherFilterFactory.cs` | Add new `IFilterFactory` overloads: `DeparturesGoTo(IReadOnlySet<Station>)` and `ArrivalsComeFrom(IReadOnlySet<Station>)`. Implementations delegate to `GoesToAnyOf` / `ComesFromAnyOf`. Backward-compatible. |
| `Timetable.Web/Controllers/ArrivalDeparturesControllerBase.cs` | Inject `IStationGroupResolver`. Modify `CreateFilter(SearchRequest, TocFilter)` to try CRS (case-insensitive) first, then group, then warn-and-drop. Add new abstract `CreateFilter(IReadOnlySet<Station>)`. Add `ResolveStations` helper. Normalise location input to a canonical case before lookup. |
| `Timetable.Web/Controllers/DeparturesController.cs` | Inject `IStationGroupResolver` + `IStationGroupDeduplicator`. Implement multi-station filter override. Add group dispatch in `Departures` and `FullDayDepartures` (try CRS first, then group). Add `FullDayGroupDepartures` and `WindowedGroupDepartures` helpers (sequential loop over members, dedup, then trim if windowed). |
| `Timetable.Web/Controllers/ArrivalsController.cs` | Same structural changes, mirrored. |
| `Timetable.Web/Configuration.cs` | Add `StationGroupsFile` (path) and `StationGroupDeduplicationStrategy` (enum string) properties. |
| `Timetable.Web/appsettings.json` | Add `"StationGroupsFile": "Data/station-groups.json"` and `"StationGroupDeduplicationStrategy": "Longest"`. |
| `Timetable.Web/Loaders/StationGroupsLoader.cs` *(new)* | Reads JSON file from disk. Returns `IReadOnlyDictionary<string, StationGroup>`. File absent → returns empty dictionary and logs Information. Malformed → throws (fail-fast). Validates invariants (priorities are member-CRS, codes unique). |
| `Timetable.Web/ServiceConfiguration/SetData.cs` | Add `Configuration` ctor param. New `LoadGroupResolver()` invoked synchronously during startup. Register `IStationGroupResolver`. |
| `Timetable.Web/ServiceConfiguration/Singletons.cs` | Read `StationGroupDeduplicationStrategy` from configuration; register `IStationGroupDeduplicator` with the corresponding `JourneyHeuristic` value. |
| `Timetable.Web/ConfigurationFinder.cs` | Pass `config` through to `SetData` constructor. |

### Wrapper changes

| File | Change |
|---|---|
| `uk-timetable-service/Dockerfile` | Add build step that fetches `locations.json`, merges with `config/station-group-priorities.json`, and projects into `Data/station-groups.json`. |
| `uk-timetable-service/config/station-group-priorities.json` *(new)* | Seatfrog-owned editorial overrides for specific groups (initial set: Reading, Bristol, Manchester). New entries added as Silverrail-behaviour testing or customer signal confirms them. |
| `uk-timetable-service/scripts/update_appsettings` | Extend to also set `StationGroupDeduplicationStrategy` from an Unleash flag fetched at build time (or default if flag unavailable). |

### Request flow

`GET /api/timetable/departures/GB@LO/2026-05-13?to=GB@MA&fullDay=true&includeStops=true`

```
DeparturesController.Departures
  → TryGetStation("GB@LO") → false (not a CRS in the timetable)
  → resolver.TryGet("GB@LO", out originGroup) → true (case-insensitive)
  → ResolveStations(originGroup.Members) → IReadOnlySet<Station>
  → FullDayGroupDepartures(originMembers, originGroup, request, …)
       │
       ├─ CreateFilter(request, tocFilter)            // ?to=GB@MA case
       │    → TryGetStation("GB@MA") → false
       │    → resolver.TryGet("GB@MA", out destGroup) → true
       │    → ResolveStations(destGroup.Members) → IReadOnlySet<Station>
       │    → CreateFilter(stations) → _filters.DeparturesGoTo(stations)
       │      └ filter = s => s.Where(svc => svc.GoesToAnyOf(destinations))
       │
       ├─ foreach (var member in origin members):   // sequential loop, see "Performance"
       │    (status, stops) = _timetable.AllDepartures(member.ThreeLetterCode, onDate, filter, boundary)
       │    if success: allStops.AddRange(stops)
       │
       ├─ deduped = _deduplicator.Deduplicate(allStops, originMembers, groupIsDestination=false, "GB@LO")
       │    // Priority pass first (originGroup.Priorities), then JourneyHeuristic fallback
       │
       ├─ (windowed mode only) deduped = deduped.OrderBy(time).Take(before+after).ToArray()
       │
       └─ Process(...) maps deduped → FoundServiceResponse with concrete at/to CRS codes
```

For a destination-only group (`location=EUS`, `to=GB@MA`): no group loop over the origin (single `AllDepartures` call), but the destination filter set selects services going to any Manchester member via `GoesToAnyOf`, and the deduplicator collapses any service that calls at multiple Manchester stops. `groupIsDestination=true`, `groupCode="GB@MA"`.

For arrivals: mirrored, with `ArrivalsComeFrom(IReadOnlySet<Station>)` for `?from=` groups and the same dispatch in `ArrivalsController`.

### The destination-side iteration is not multiplicative

A naive multi-destination filter does `s.Where(svc => destinations.Any(d => svc.GoesTo(d)))` — `O(N_dest × S)` per candidate. Instead, push the set check inside a single scan:

```csharp
public bool GoesToAnyOf(IReadOnlySet<Station> destinations) { ... }
return s.Where(svc => svc.GoesToAnyOf(destinations));
```

`GoesToAnyOf` does **one** backward scan of arrivals — same shape as `GoesTo` today — and at each arrival checks `destinations.Contains(arrival.Station)` (O(1) set membership). One arrivals scan per service regardless of |destinations|. Symmetric `ComesFromAnyOf` for arrivals. The method deterministically records the matching stop most useful for downstream dedup (e.g. for a destination group, the latest-arrival match it encounters during the backward scan).

### Validation & error mapping

Flow: try CRS (case-insensitive), then try group (case-insensitive), then 404. No format detection.

| Input | Outcome |
|---|---|
| Valid CRS in path/query, in master data | Existing CRS path, unchanged |
| Valid CRS-shaped but not in master data | 404 (existing `LocationNotFound`) |
| Anything else (any length, any chars) that resolves via `resolver.TryGet` | Group path → 200 |
| Anything else not resolvable as group either | 404 `LocationNotFound` |

This is the same error model the core uses today — no introduction of `400 Bad Request` for malformed origin codes.

**URL encoding.** Group codes contain characters reserved in URLs — `#` is the fragment delimiter, `%` is the escape character itself, `@` and `^` are reserved in some contexts. Clients **must** percent-encode group codes when constructing URLs (e.g. `GB%23RE` for `GB#RE`, `GB%40LO` for `GB@LO`). ASP.NET Core's default route and query binding decodes these automatically, so the action method receives the canonical form (`GB#RE`) without any custom handling needed. A controller test covers this explicitly to guard against regressions if any code path is later changed to read `HttpContext.Request.Path` / `.QueryString` directly (which would expose the raw, undecoded form).

**Origin and destination both being the same group** (e.g. `GB@LO → GB@LO`) is **permitted** and behaves naturally. The timetable service doesn't impose product-level constraints — it's a generic timetable lookup. The natural request flow handles it correctly: each origin member is queried, services going to any destination member are filtered in (via `GoesToAnyOf`), and the deduplicator collapses any service calling at multiple members. For most cities there are few or no direct intra-city services in CIF data, so results will usually be small or empty. Where they do exist (e.g. cross-London services via Thameslink / Elizabeth Line), they're real journeys and the service returns them. Product-level restrictions (e.g. "auction search doesn't sell intra-city") belong upstream in the consumer.

### Reference-data loader (`StationGroupsLoader`)

- Reads the file at `Configuration.StationGroupsFile` using `System.Text.Json`.
- File absent → returns empty dictionary, logs Information ("station-groups.json not found at {path} — station group search disabled"). Service starts normally.
- File present but malformed → throws → startup fails (fail-fast).
- Validates invariants (priorities are subset of members, codes unique). Violations logged and skipped.
- One-shot read at startup, held in memory until process restart. No HTTP.
- Lives in `Timetable.Web/Loaders/` alongside `CifLoader`.

### Performance estimate

Worst-case query: London origin (N₁=18 members) → Manchester destination (N_dest=3), full day:
- 18 sequential `AllDepartures` calls, each ≈ today's single-CRS cost (~20-40ms in production)
- Per-candidate filter is `GoesToAnyOf` — **one** stops scan regardless of N_dest
- Dedup: priority lookup + heuristic over ~3,600 candidates ≈ ~1-2 ms

Estimated wall-clock: **~400-700ms** sequential. Inside the 500ms p95 target for most cases; the upper end (London during peak hours) sits on the borderline. Sequential is the chosen starting point — measure under realistic load before considering optimisations. See "Future optimisations" below.

### Tests

- `Timetable.Test/StationGroupResolverTest.cs` — case-insensitive lookups present/absent, empty dictionary, invariant violations
- `Timetable.Test/GoesToAnyOfTest.cs` — single match, no match, multiple-matches-in-set, deterministic `FoundToStop` selection
- `Timetable.Test/GatherFilterGroupTest.cs` — multi-station filter wiring
- `Timetable.Test/StationGroupDeduplicatorTest.cs` — priority hit (single, ordered, partial), priority miss → heuristic, both heuristics for origin and destination, dedup by RSID+date, ties, empty input, single candidate
- `Timetable.Web.Test/Controllers/GroupDeparturesControllerTest.cs` — group origin, group dest, both, unknown group → 404, regular CRS unchanged, dedup wired, windowed-trim applied
- `Timetable.Web.Test/Controllers/GroupArrivalsControllerTest.cs` — symmetric
- `Timetable.Web.Test/Loaders/StationGroupsLoaderTest.cs` — parse valid file, missing-file returns empty + logs, malformed file throws, invariant warnings
- `Timetable.Web.Test/ServiceConfiguration/{Singletons,SetData}Test.cs` — DI registration assertions for both heuristic values

---

## Resolved decisions (previously open questions)

These were debated during planning and have now been settled:

| Decision | Resolution |
|---|---|
| Dedup approach | Single `StationGroupDeduplicator` with priority-pass override + configurable `Longest`/`Shortest` heuristic. The UK Routeing Guide "main station" approach was considered and rejected — its rules apply to ticket validity, not timetable display. |
| Shortest vs Longest as default | Both implementations shipped behind a config switch. `Longest` as the initial default; Unleash flag (set at build time by the wrapper) selects between them globally. |
| Source of priority overrides | Seatfrog-owned editorial file in the wrapper repo (`config/station-group-priorities.json`), merged into the neutral group file at build time. Keeps core client-agnostic. |
| Origin-loop strategy | Sequential. Simplest, makes correctness easier to verify. Parallel and precomputed alternatives noted under "Future optimisations" if perf becomes a concern. |
| Unknown `?to=` / `?from=` group code | Keep existing behaviour: warn-and-drop the filter (matches handling of unknown CRS in `?to=`). |
| Windowed-mode group queries | Trim merged-and-deduped results back to `before+after` after sorting by departure/arrival time. Preserves the windowed contract. |
| Case sensitivity | Case-insensitive everywhere — group code lookups, CRS lookups (this addresses a pre-existing inconsistency in the service). |
| File-absent behaviour | Feature silently disabled, log Information at startup. Same pattern as other optional reference files. |
| 400 vs 404 for malformed input | Stay with 404 (matches existing behaviour). No new validation introduced. |
| Origin and destination both being the same group | Permitted. Timetable service is generic; product-level constraints belong upstream. Natural request flow handles it correctly. |
| Per-group heuristic configuration | Not needed. Priority list already covers cases where global heuristic is wrong. Strategy interface already takes the heuristic as a parameter, so per-group support remains a small future extension if a counter-example emerges. |

---

## Remaining open data questions

These are content/editorial questions about the priorities data and the group set itself, not code-design questions:

1. **Initial `priorities` list — which groups need overrides?** Confirmed via Silverrail-behaviour testing so far:
   - `GB#RE` (Reading): `["RDG"]` — RDG wins in every observed pairing.
   - `GB@BR` (Bristol): `["BRI"]` — BRI wins in every observed pairing where a service calls at both BRI and BPW.
   - `GB@MA` (Manchester): `["MAN"]` — MAN wins over MCO when a service calls at both, **even when that gives a shorter journey**. No service was found calling at all three of MAN, MCV and MCO (Google suggests they exist but are rare), so the priority list of just `["MAN"]` is sufficient — fallback handles any remaining pairings.
   - `GB@BH` (Birmingham): **no priority needed** — testing showed no direct services calling at both BHM and the smaller stations (BMO/BSW), and BMO/BSW pairings use longest-journey. The global heuristic produces the correct result on its own.

   Other major groups (`GB@LO`, `GB@GL`, `GB@LP`, `GB@CW`, etc.) haven't been individually tested. We don't need to know them all upfront — the priorities file is data, additions are cheap, and the global `Longest` heuristic is a reasonable default for groups we haven't validated yet. Recommend adding entries as customer signal or further testing reveals them.

2. **Equal-priority stations** — not needed for any current group. If a future group needs ties, the schema extends to nested arrays (`"priorities": [["A"], ["B", "C"]]`). Flagging only so this isn't a surprise later.

---

## Future optimisations (not in initial scope)

The plan defaults to a sequential origin-loop strategy. Two faster alternatives, to be revisited if production measurements show the sequential loop overruns the 500ms p95 target:

| Option | Detail | Pros | Cons |
|---|---|---|---|
| **Parallel via `Task.WhenAll`** | All member lookups are independent, in-memory, side-effect-free. | Brings wall-clock back to roughly a single-CRS query (~30-50ms) on multi-core hosts. Small contained change. | Adds CPU-bound parallelism in request threads (slightly more thread-pool pressure under burst load). |
| **Precomputed merged group boards at startup** | When timetable data finishes loading, walk each group and build a merged `SortedList<Time, IService[]>` per group code. Add `ILocationData.TryGetGroupTimetable`. | Per-request cost matches a single-CRS query (~30ms) regardless of group size — strictly fastest. | Group concept leaks slightly into the data layer. Modest extra memory (services referenced not copied). More work upfront. |

The deduplicator, filter, and resolver designs are unchanged across all three options — only the merge step changes — so switching later is a contained refactor.

---

## Out of scope (confirmed)

- Webservices changes (the previous plan's Part 2 — handled in a future task once the timetable service is shipped)
- AuctionEngine
- Limitation #2 (multi-leg / interchange searches like NRW → EDB via PBO)
- `/api/timetable/toc/{toc}/{date}` and `/api/timetable/service/{retailServiceId}/{date}` (these resolve to specific stations naturally and don't need group support)
- Parallel/concurrent member-station queries (see "Future optimisations")
- Runtime HTTP fetching of reference data (the wrapper bakes the file at build time)
- Runtime feature-flag toggling of the dedup heuristic (build-time only via Unleash; rebuild required to switch)

---

## Build sequence

**Phase 1 — Domain & resolver** (no controllers, no IO)
- `StationGroup` record, `JourneyHeuristic` enum
- `IStationGroupResolver`, `StationGroupResolver` (case-insensitive)
- `IStationGroupDeduplicator` interface, `StationGroupDeduplicator` implementation (priority pass + heuristic fallback)
- `GoesToAnyOf` / `ComesFromAnyOf` on `ResolvedServiceStop` + tests
- `IFilterFactory` multi-station overloads + tests
- Deduplicator tests covering priority and both heuristics

**Phase 2 — Loader & DI**
- `StationGroupsLoader` (disk read, invariant validation) + tests
- `Configuration.StationGroupsFile` and `StationGroupDeduplicationStrategy`, `appsettings.json` keys
- `SetData.LoadGroupResolver()`, register `IStationGroupResolver`
- `Singletons.cs` register `IStationGroupDeduplicator` reading strategy from config
- Update `ConfigurationFinder`
- Update existing DI tests

**Phase 3 — Controllers**
- `ArrivalDeparturesControllerBase` — new abstract, new ctor param, case-insensitive normalisation, modify `CreateFilter`
- `DeparturesController` — group dispatch (try CRS → try group → 404), helpers, windowed-trim, ctor wiring
- `ArrivalsController` — mirrored
- Group controller tests
- Manual smoke test: existing CRS query returns identical response to pre-change; new group query returns sensible result for both heuristics

**Phase 4 — Wrapper Dockerfile + priorities file**
- Add `config/station-group-priorities.json` with the agreed initial overrides (Reading, Bristol, Manchester; see open data question 1)
- Extend Dockerfile build step: fetch `locations.json`, merge with priorities file, write to `Data/station-groups.json` inside the cloned core source tree
- Extend `scripts/update_appsettings` (or add a sibling script) to set `StationGroupDeduplicationStrategy` from an Unleash flag at build time
- Adjust the cache-bust ARG so changes to upstream `locations.json` re-trigger this step
- Smoke test the produced image locally: `docker build` → `docker run` → curl a group query with each heuristic

**Phase 5 — Deployment**
- Wrapper changes deploy through the existing GitHub Actions workflow (UAT first, then prod)
- Verify the produced image contains `Data/station-groups.json` with the expected overrides
- Verify the service logs "Loaded N station groups" at startup and which heuristic is registered

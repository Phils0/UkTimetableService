using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Timetable.Web.IntegrationTest
{
    /// <summary>
    /// Checks that an overnight train which departs just after midnight shows up on a station's
    /// full-day departures board for the right day - from every station it calls at, not just some.
    ///
    /// The example is the overnight Newcastle -> Liverpool Lime St service C24988 (TP5161). It
    /// leaves Newcastle at 21:02 and runs through the night, departing Manchester Victoria (MCV)
    /// at 00:01 and Manchester Oxford Road (MCO) at 00:24 on 23-Jun-2026. So a full-day search for
    /// departures to Liverpool on 23-Jun, from either MCV or MCO, should return this train.
    ///
    /// It used to go missing from MCV's board (but not MCO's): the train's time at MCV shifts by a
    /// few minutes from one night to the next (00:01 one night, 00:04 the next), and the step that
    /// re-attributes an after-midnight departure to the night it actually started compared on the
    /// clock time alone - so the shifted time failed to line up and the departure was dropped. MCO
    /// kept it only because its time there (00:24) happens not to change between those nights.
    ///
    /// The test pins both stations to "present" so that drop can't come back. It loads the real
    /// production timetable (RJTTF870) and calls the ordinary departures endpoint directly.
    /// </summary>
    [Collection("Rjttf870")]
    public class OvernightBoardAsymmetryTest
    {
        private const string OvernightServiceUid = "C24988";
        private readonly Rjttf870Fixture _fixture;

        public OvernightBoardAsymmetryTest(Rjttf870Fixture fixture)
        {
            _fixture = fixture;
        }

        [SkippableTheory]
        [InlineData("MCO", "2026-06-23")] // departs MCO 00:24 - its time here is the same each night
        [InlineData("MCV", "2026-06-23")] // departs MCV 00:01 - its time here shifts night to night
        public async Task OvernightService_C24988_IsReachableFromManchesterMember(string origin, string date)
        {
            Skip.IfNot(_fixture.Available,
                $"{Rjttf870Fixture.ArchiveFileName} not present in Data/ (it is gitignored) - skipping.");

            var (status, body) = await GetFullDayDepartures(origin, date, "LIV");

            status.Should().BeTrue("departures {0} -> LIV on {1} should return 200", origin, date);
            body.Should().Contain(OvernightServiceUid,
                "overnight service {0} (TP5161) departs {1} for Liverpool in the small hours of {2} " +
                "and must appear on the full-day board", OvernightServiceUid, origin, date);
        }

        private async Task<(bool ok, string body)> GetFullDayDepartures(string origin, string date, string destination)
        {
            var client = _fixture.Host.GetTestClient();
            var url = $"/api/Timetable/departures/{origin}/{date}?to={destination}&fullDay=true&includeStops=true";
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, body);
        }
    }
}

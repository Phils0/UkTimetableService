using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Timetable
{
    public class AssociationDictionary :
        Dictionary<string, SortedList<(StpIndicator indicator, ICalendar calendar), Association>>
    {
        private readonly ILogger _logger;

        public AssociationDictionary(int capacity, ILogger logger) : base(capacity)
        {
            _logger = logger;
        }

        public ResolvedAssociation[] Resolve(string timetableUid, DateTime on, string retailServiceId)
        {
            var resolvedAssociations = new List<ResolvedAssociation>();
            foreach (var versions in this.Values)
            {
                var isCancelled = false;
                foreach (var association in versions.Values)
                {
                    if (association.AppliesOn(on))
                    {
                        if (association.IsCancelled())
                            isCancelled = true;
                        else
                        {
                            var other = association.GetOtherService(timetableUid);
                            var otherDate = ResolveDate(on, association.DateIndicator, association.IsMain(timetableUid));
                            if (other != null && other.TryResolveOn(otherDate, out var resolved, false))
                            {
                                if (!resolved.HasRetailServiceId(retailServiceId))
                                    _logger.Information(
                                        "Resolved association {resolved} has mismatched RetailServiceIds {resolvedRsId} instead of {retailServiceId}",
                                        resolved, resolved.Details.NrsRetailServiceId, retailServiceId);

                                resolvedAssociations.Add(
                                    new ResolvedAssociation(association, on, isCancelled, resolved));
                            }
                            else
                            {
                                _logger.Warning(
                                    "Association {association} has unresolved service {other} on {otherDate}",
                                    association, other, otherDate.ToYMD());
                            }

                            break;
                        }
                    }
                }
            }

            return resolvedAssociations.ToArray();
        }

        private DateTime ResolveDate(DateTime onDate, AssociationDateIndicator indicator, bool isMain)
        {
            switch (indicator)
            {
                case AssociationDateIndicator.Standard:
                    return onDate;
                case AssociationDateIndicator.NextDay:
                    return isMain ? onDate.AddDays(1) : onDate.AddDays(-1);
                case AssociationDateIndicator.PreviousDay:
                    return isMain ? onDate.AddDays(-1) : onDate.AddDays(1);
                default:
                    throw new ArgumentException($"Unhandled DateIndicator value {indicator}", nameof(indicator));
            }
        }
        public bool Add(Association association, bool isMain, Service service)
        {
            var uid = isMain ? association.Associated.TimetableUid : association.Main.TimetableUid;
            if (!TryGetValue(uid, out var values))
            {
                values = new SortedList<(StpIndicator indicator, ICalendar calendar), Association>(StpDescendingComparer.Instance);
                Add(uid, values);
            }

            try
            {
                values.Add((association.StpIndicator, association.Calendar), association);
                return true;
            }
            catch (ArgumentException)
            {
                // Can have 2 associations that are different but for the same services, see tests
                if (values.TryGetValue((association.StpIndicator, association.Calendar), out var duplicate))
                {
                    if (TryResolveAssociation(out var added))
                        return added;
                    
                    RemoveDuplicateAssociation();
                    return false;
                }
                throw;

                bool TryResolveAssociation(out bool addedAssociation)
                {
                    addedAssociation = false;
                    var key = (association.StpIndicator, association.Calendar);
                    if(!service.TryGetSchedule(key, out var schedule))
                        return false;
                    
                    if (duplicate.HasConsistentLocation(schedule, isMain))
                    {
                        _logger.Warning("Has Duplicate Association {association}.  Original association has consistent location and is used.", association);
                        return true;
                    }
                    if (association.HasConsistentLocation(schedule, isMain))
                    {
                        _logger.Warning("Has Duplicate Association {duplicate}.  New association has consistent location and is used.", duplicate);
                        values[key] = association;
                        addedAssociation = true;
                        return true;
                    }
                    return false;
                }

                void RemoveDuplicateAssociation()
                {
                    _logger.Warning("Removing Duplicate Associations {association} {duplicate}", association,duplicate);
                    values.Remove((association.StpIndicator, association.Calendar));
                    if (!values.Any())
                        Remove(uid);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Xunit;
using static Timetable.Test.StationGroupSearchFixtures;

namespace Timetable.Test
{
    public class ServiceCandidateGroupingTest
    {
        // Trivial canonical picker: just keep the first row of each run. Lets these tests focus purely on the
        // grouping/identity behaviour, with no selection logic involved.
        private static ResolvedServiceStop First(IReadOnlyList<ResolvedServiceStop> rows) => rows[0];

        [Fact]
        public void EmptyInputReturnsEmpty()
        {
            var result = ServiceCandidateGrouping.PickOnePerService(Array.Empty<ResolvedServiceStop>(), First);

            Assert.Empty(result);
        }

        [Fact]
        public void CollapsesRowsOfTheSameRunToOne()
        {
            var service = LondonAllToManchesterPiccadilly();
            var atKgx = new ResolvedServiceStop(service, service.Details.Locations[0]);
            var atStp = new ResolvedServiceStop(service, service.Details.Locations[1]);

            var result = ServiceCandidateGrouping.PickOnePerService(new[] { atKgx, atStp }, First);

            Assert.Single(result);
        }

        [Fact]
        public void InvokesChooseCanonicalOncePerRun()
        {
            var service = LondonAllToManchesterPiccadilly();
            var atKgx = new ResolvedServiceStop(service, service.Details.Locations[0]);
            var atStp = new ResolvedServiceStop(service, service.Details.Locations[1]);
            var calls = 0;

            ServiceCandidateGrouping.PickOnePerService(new[] { atKgx, atStp }, rows => { calls++; return rows[0]; });

            Assert.Equal(1, calls);
        }

        [Fact]
        public void KeepsDifferentServiceRunsSeparate()
        {
            var serviceA = LondonAllToManchesterPiccadilly(uid: "L11111");
            var serviceB = LondonAllToManchesterPiccadilly(uid: "L22222");
            var atA = new ResolvedServiceStop(serviceA, serviceA.Details.Locations[0]);
            var atB = new ResolvedServiceStop(serviceB, serviceB.Details.Locations[0]);

            var result = ServiceCandidateGrouping.PickOnePerService(new[] { atA, atB }, First);

            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void SameRetailServiceIdDifferentTimetableUidAreKeptSeparate()
        {
            // Two distinct runs (different TimetableUid) that share a RetailServiceId - e.g. split portions.
            // They must NOT be collapsed: identity is TimetableUid + date, not RetailServiceId.
            var serviceA = LondonAllToManchesterPiccadilly(uid: "A11111");
            var serviceB = LondonAllToManchesterPiccadilly(uid: "B22222");
            ((CifSchedule)serviceA.Details).RetailServiceId = "VT9999";
            ((CifSchedule)serviceB.Details).RetailServiceId = "VT9999";
            var atA = new ResolvedServiceStop(serviceA, serviceA.Details.Locations[0]);
            var atB = new ResolvedServiceStop(serviceB, serviceB.Details.Locations[0]);

            var result = ServiceCandidateGrouping.PickOnePerService(new[] { atA, atB }, First);

            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void DropsRunsWhereChooseCanonicalReturnsNull()
        {
            var service = LondonAllToManchesterPiccadilly();
            var atKgx = new ResolvedServiceStop(service, service.Details.Locations[0]);

            var result = ServiceCandidateGrouping.PickOnePerService(new[] { atKgx }, _ => null);

            Assert.Empty(result);
        }
    }
}
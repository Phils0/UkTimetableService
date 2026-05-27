using Timetable.Test.Data;

namespace Timetable.Test
{
    /// <summary>
    /// Shared station and service fixtures for the station-group search tests
    /// (<see cref="ServiceCandidateGroupingTest"/>, <see cref="CanonicalStopSelectorTest"/> and
    /// <see cref="StationGroupStopOptimiserTest"/>). The routes are contrived to model the GB@LO / GB@MA groups
    /// discussed in planning. Each station access returns a fresh <see cref="Station"/> so LocationTimetable
    /// state can't leak across tests.
    /// </summary>
    internal static class StationGroupSearchFixtures
    {
        public static Station Euston => TestStations.Create("EUS");
        public static Station KingsCross => TestStations.Create("KGX");
        public static Station StPancras => TestStations.Create("STP");
        public static Station ManchesterPiccadilly => TestStations.Create("MAN");
        public static Station ManchesterVictoria => TestStations.Create("MCV");
        public static Station ManchesterOxfordRoad => TestStations.Create("MCO");

        // Euston (10:00) -> MCO (11:00) -> MCV (11:10) -> Manchester Piccadilly (11:20, destination)
        public static ResolvedService EustonToManchesterAll(string uid = "M12345")
        {
            var ten = TestSchedules.Ten;
            var stops = new[]
            {
                (ScheduleLocation)TestScheduleLocations.CreateOrigin(Euston, ten),
                TestScheduleLocations.CreateStop(ManchesterOxfordRoad, ten.AddMinutes(60)),
                TestScheduleLocations.CreateStop(ManchesterVictoria, ten.AddMinutes(70)),
                TestScheduleLocations.CreateDestination(ManchesterPiccadilly, ten.AddMinutes(80))
            };
            return TestSchedules.CreateService(timetableId: uid, stops: stops);
        }

        // King's Cross (10:00) -> St Pancras (10:05) -> Manchester Piccadilly (11:20, destination)
        // Contrived but useful: lets us test origin-group multiplicity when a service has public
        // departures at two London-area members in a row.
        public static ResolvedService LondonAllToManchesterPiccadilly(string uid = "L12345")
        {
            var ten = TestSchedules.Ten;
            var stops = new[]
            {
                (ScheduleLocation)TestScheduleLocations.CreateOrigin(KingsCross, ten),
                TestScheduleLocations.CreateStop(StPancras, ten.AddMinutes(5)),
                TestScheduleLocations.CreateDestination(ManchesterPiccadilly, ten.AddMinutes(80))
            };
            return TestSchedules.CreateService(timetableId: uid, stops: stops);
        }

        // King's Cross (10:00) -> St Pancras (10:05) -> MCO (11:00) -> MCV (11:10) -> Manchester Piccadilly (11:20)
        // Multiplicity on both ends: two London origins and three Manchester destinations.
        public static ResolvedService LondonAllToManchesterAll(string uid = "B12345")
        {
            var ten = TestSchedules.Ten;
            var stops = new[]
            {
                (ScheduleLocation)TestScheduleLocations.CreateOrigin(KingsCross, ten),
                TestScheduleLocations.CreateStop(StPancras, ten.AddMinutes(5)),
                TestScheduleLocations.CreateStop(ManchesterOxfordRoad, ten.AddMinutes(60)),
                TestScheduleLocations.CreateStop(ManchesterVictoria, ten.AddMinutes(70)),
                TestScheduleLocations.CreateDestination(ManchesterPiccadilly, ten.AddMinutes(80))
            };
            return TestSchedules.CreateService(timetableId: uid, stops: stops);
        }
    }
}
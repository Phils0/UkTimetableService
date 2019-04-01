namespace Timetable.Test.Data
{
    public static class TestData
    {
        public static readonly Timetable.Data Instance = new Timetable.Data(
            new []
            {
                TestLocations.Surbiton,
                TestLocations.WaterlooMain,
                TestLocations.WaterlooWindsor
            });
    }
}
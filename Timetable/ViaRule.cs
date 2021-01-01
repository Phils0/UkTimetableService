namespace Timetable
{
    public class ViaRule
    {
        public Station At { get; set; } = Station.NotSet;
        public Location Destination { get; set; } = Location.NotSet;
        public Location Location1 { get; set; } = Location.NotSet;
        public Location Location2 { get; set; } = Location.NotSet;
        public string Text { get; set; }
        public bool HasLocation2 => !Location2.Equals(Location.NotSet);
        
        public bool IsSatisfied(Schedule schedule)
        {
            if(!schedule.Destination.Location.Equals(Destination))
                return false;

            var foundAt = false;
            var foundLocation1 = false;
            
            foreach (var location in schedule.Locations)
            {
                if (foundAt)
                {
                    if (foundLocation1)
                    {
                        if (location.IsStopAt(Location2))
                            return true;
                    }
                    else
                    {
                        if (location.IsStopAt(Location1))
                        {
                            if (Location2.Equals(Location.NotSet))
                                return true;
                            
                            foundLocation1 = true;                   
                        }
                    }
                }
                else
                {
                    if (location.IsStopAt(At))
                        foundAt = true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return HasLocation2
                ? $"{At} To:{Destination.Tiploc} Loc1-{Location1.Tiploc} Loc2-{Location2.Tiploc}"
                : $"{At} To:{Destination.Tiploc} Loc1-{Location1.Tiploc}";
        }
    }
}
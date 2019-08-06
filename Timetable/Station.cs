using System;
using System.Collections.Generic;
using Serilog;

namespace Timetable
{
    /// <summary>
    /// Station, aggregate of locations from the Master Station List
    /// </summary>
    public class Station : IEquatable<Station>
    {
        /// <summary>
        /// Location CRS code
        /// </summary>
        public string ThreeLetterCode => Main.ThreeLetterCode;

        /// <summary>
        /// National Location Code - 4 character code
        /// </summary>
        public string Nlc => Main.Nlc?.Substring(0, 4);
        
        /// <summary>
        /// Main Location
        /// </summary>
        public Location Main { get; private set; } = Location.NotSet;
        
        /// <summary>
        /// Tiplocs
        /// </summary>
        public ISet<Location> Locations { get; } = new HashSet<Location>();
        
        /// <summary>
        /// Timetable for Station
        /// </summary>
        public LocationTimetable Timetable { get; }

        public Station()
        {
            Timetable = new LocationTimetable(this);
        }

        public bool Equals(Station other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ThreeLetterCode.Equals(other.ThreeLetterCode);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Station) obj);
        }

        public override int GetHashCode()
        {
            return Main.GetHashCode();
        }

        /// <summary>
        /// Add a location to the station
        /// </summary>
        /// <param name="location"></param>
        public void Add(Location location)
        {
            Locations.Add(location);
            location.Station = this;
            if (!location.IsSubsidiary)
            {
                /// Should never happen, but if we have 2 locations, last one wins
                if(!Main.Equals(Location.NotSet))
                    Log.Warning("Overriding main location {original} with {replacement}", Main, location);
                
                Main = location;               
            }
        }

        /// <summary>
        /// Add a service stop to the station
        /// </summary>
        /// <param name="stop">Service stop</param>
        public void Add(ScheduleLocation stop)
        {
            Timetable.AddService(stop);
        }
        
        public override string ToString()
        {
            return  String.IsNullOrEmpty(ThreeLetterCode) ? "Not Set" : $"{ThreeLetterCode}";
        }
    }
}

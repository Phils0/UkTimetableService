﻿using System;
using System.Collections.Generic;
using Serilog;

namespace Timetable
{
    /// <summary>
    /// Station, aggregate of locations from the Master Station List
    /// </summary>
    public class Station : IEquatable<Station>
    {
        public static Station NotSet = new Station();
        
        /// <summary>
        /// Location CRS code
        /// </summary>
        public string ThreeLetterCode => Main.ThreeLetterCode;

        /// <summary>
        /// National Location Code
        /// </summary>
        public string Nlc { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Coordinates
        /// </summary>
        public Coordinates Coordinates { get; set; }
        
        /// <summary>
        /// Main Location
        /// </summary>
        public Location Main { get; private set; } = Location.NotSet;
        
        /// <summary>
        /// Tiplocs
        /// </summary>
        public ISet<Location> Locations { get; } = new HashSet<Location>();
        
        /// <summary>
        /// The toc that runs the station
        /// </summary>
        public Toc StationOperator { get; set; } = Toc.Unknown;
        
        /// <summary>
        /// The tocs that have services that stop here
        /// </summary>
        public ISet<Toc> TocServices { get; } = new HashSet<Toc>();
        
        /// <summary>
        /// Timetable for Station
        /// </summary>
        internal LocationTimetable Timetable { get; }

        /// <summary>
        /// Rules to add Via Text
        /// </summary>
        internal ViaRules ViaTextRules { get; } = new ViaRules();

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
        public void AddMasterStationLocation(Location location)
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
        /// Add a location to the station
        /// </summary>
        /// <param name="location"></param>
        public void AddCifLocation(Location location)
        {
            Locations.Add(location);
            location.Station = this;
            if (Main.Equals(Location.NotSet))
            {
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
            if(stop.IsAdvertised())
                TocServices.Add(stop.Schedule.Operator);
        }

        /// <summary>
        /// Add a Darwin via text rule
        /// </summary>
        /// <param name="rule">Via text rule</param>
        public void Add(ViaRule rule)
        {
            if (!this.Equals(rule.At))
                throw new ArgumentException($"Via Rule is not for {this}: {rule}");

            ViaTextRules.AddRule(rule);
        }
        
        public string GetViaText(ISchedule schedule)
        {
            return ViaTextRules.GetViaText(schedule);
        }
        
        public override string ToString()
        {
            return  String.IsNullOrEmpty(ThreeLetterCode) ? "Not Set" : $"{ThreeLetterCode}";
        }
    }
}

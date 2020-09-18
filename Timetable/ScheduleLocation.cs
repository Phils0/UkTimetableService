using System;
using System.Collections.Generic;
using Serilog.Context;

namespace Timetable
{
    public enum PublicStop
    {
        NotSet,
        Yes,        // SetDown and PickUp
        PickUpOnly,
        SetDownOnly,
        Request,
        No
    }
    
    public interface IStop
    {
        Location Location { get; }
        int Sequence { get; }

        bool IsStop(Location at, int sequence);
    }
    
    public abstract class ScheduleLocation
    {
        public Location Location { get; set; }

        public int Sequence { get; set; }

        public string Platform { get; set; }

        public Activities Activities { get; set; }
        
        public PublicStop AdvertisedStop => Activities.AdvertisedStop;
        
        public Schedule Schedule { get; private set; }

        public void SetParent(Schedule schedule)
        {
            Schedule = schedule;
            Schedule.AddLocation(this);
            Station.Add(this);
        }

        public Service Service => Schedule.Service;

        public Station Station => Location.Station;
        
        public int Id { get; set; }

        public abstract void AddDay(Time start);

        public abstract bool IsStopAt(StopSpecification spec);

        protected bool IsStopAt(Station at)
        {
            return Station.Equals(at);
        }
        
        internal bool HasAdvertisedTime(bool useDepartures)
        {
            switch (AdvertisedStop)
            {
                case PublicStop.Yes:
                case PublicStop.Request:
                    return true;
                case PublicStop.No:
                    return false;
                case PublicStop.PickUpOnly:
                    return useDepartures;
                case PublicStop.SetDownOnly:
                    return !useDepartures;
                default:
                    return false;
            }
        }
        
        internal bool IsAdvertised()
            => HasAdvertisedTime(true) || HasAdvertisedTime(false);
        
        public bool IsStop(Location at, int sequence)
        {
            return Location.Equals(at) && Sequence == sequence;
        }

        internal bool IsMainConsistent(AssociationCategory category) => category.IsJoin()
            ? Activities.IsTrainJoin
            : category.IsSplit() && Activities.IsTrainSplit; 
        
        internal bool IsAssociatedConsistent(AssociationCategory category) => category.IsJoin()
            ? Activities.IsDestination
            : category.IsSplit() && Activities.IsOrigin; 
        
        public override string ToString()
        {
            return Sequence > 1 ? 
                $"{Location}+{Sequence}" :
                $"{Location}";           
        }
    }
}
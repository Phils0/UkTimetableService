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
    
    public abstract class ScheduleLocation
    {
        public Location Location { get; set; }

        public int Sequence { get; set; }

        public string Platform { get; set; }

        public ISet<string> Activities { get; set; }

        public void UpdateAdvertisedStop()
        {
            if (Activities == null)
                return;
            
            if (Activities.Contains(Activity.StopNotAdvertised))
            {
                AdvertisedStop = PublicStop.No;
                return;
            }
            
            if (Activities.Contains(Activity.PassengerStop))
            {
                AdvertisedStop = PublicStop.Yes;
            }
            else if (Activities.Contains(Activity.TrainBegins) ||
                     Activities.Contains(Activity.PickUpOnlyStop))
            {
                AdvertisedStop = PublicStop.PickUpOnly;                           
            }
            else if (Activities.Contains(Activity.TrainFinishes) ||
                     Activities.Contains(Activity.SetDownOnlyStop))
            {
                AdvertisedStop = PublicStop.SetDownOnly;                           
            }
            else if (Activities.Contains(Activity.RequestStop))
            {
                AdvertisedStop = PublicStop.Request;                           
            }
            else
            {
                AdvertisedStop = PublicStop.No;
            }
        }

        public PublicStop AdvertisedStop { get; private set; } = PublicStop.NotSet;
        
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

        public bool IsStopAt(Station at)
        {
            return Station.Equals(at);
        }
        
        public bool HasAdvertisedTime(bool useDepartures)
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
        
        public bool IsStop(Location at, int sequence)
        {
            return Location.Equals(at) && Sequence == sequence;
        }

        public override string ToString()
        {
            return Sequence > 1 ? 
                $"{Location}+{Sequence}" :
                $"{Location}";           
        }
    }
}
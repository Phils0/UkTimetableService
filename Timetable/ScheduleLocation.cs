using System;
using System.Collections.Generic;

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

        public bool IsStop(Station at, int sequence)
        {
            return Station.Equals(at) && Sequence == sequence;
        }

        public override string ToString()
        {
            return Sequence > 1 ? 
                $"{Location}+{Sequence}" :
                $"{Location}";           
        }
    }
}
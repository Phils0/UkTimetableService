using System;
using System.Collections.Generic;

namespace Timetable
{
    public enum StopType
    {
        NotSet,
        Normal,        // SetDown and PickUp
        PickUpOnly,
        SetDownOnly,
        Request,
        NotAPublicStop
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
                AdvertisedStop = StopType.Normal;
            }
            else if (Activities.Contains(Activity.TrainBegins) ||
                     Activities.Contains(Activity.PickUpOnlyStop))
            {
                AdvertisedStop = StopType.PickUpOnly;                           
            }
            else if (Activities.Contains(Activity.TrainFinishes) ||
                     Activities.Contains(Activity.SetDownOnlyStop))
            {
                AdvertisedStop = StopType.SetDownOnly;                           
            }
            else if (Activities.Contains(Activity.RequestStop))
            {
                AdvertisedStop = StopType.Request;                           
            }
            else
            {
                AdvertisedStop = StopType.NotAPublicStop;
            }
        }

        public StopType AdvertisedStop { get; private set; } = StopType.NotSet;
        
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

        public abstract bool IsStopAt(Station location, Time time);

        public override string ToString()
        {
            return Sequence > 1 ? 
                $"{Location}+{Sequence}" :
                $"{Location}";           
        }
    }
}
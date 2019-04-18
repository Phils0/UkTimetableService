using System;
using System.Collections.Generic;
using System.Linq;

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
    
    public interface IScheduleLocation
    {
        Location Location { get; }
        int Sequence { get; }     
        StopType AdvertisedStop { get; }       
        Schedule Parent { get; }        
        /// <summary>
        /// Unique Id to allow deterministic ordering
        /// </summary>
        int Id { get; }
    }

    public abstract class ScheduleLocation : IScheduleLocation
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
        
        public Schedule Parent { get; set; }
        
        public int Id { get; set; }

        public abstract void AddDay(Time start);  
    }
}
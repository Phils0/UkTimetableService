using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public class ResolvedServiceStop
    {
        public ResolvedService Service { get; }
        public ScheduleLocation Stop { get; }
        public ScheduleLocation FoundToStop { get; private set; } = null;
        public ScheduleLocation FoundFromStop { get; private set; } = null;

        public IncludedAssociation Association { get; private set; } = IncludedAssociation.NoAssociation;
        
        public DateTime On => Service.On;

        public Toc Operator => Service.Details.Operator;
        
        public ResolvedServiceStop(ResolvedService service, ScheduleLocation stop)
        {
            Service = service;
            Stop = stop;
        }

        public override string ToString()
        {
            return $"{Service} {Stop}";
        }

        public bool IsNextDay(bool useDeparture)
        {
            if (useDeparture)
            {
                var departure = Stop as IDeparture;
                return departure?.IsNextDay ?? false;
            }
            
            var arrival = Stop as IArrival;
            return arrival?.IsNextDay ?? false;
        }
        
        public bool GoesTo(Station destination)
        {
            bool found;
            ScheduleLocation to;
            IncludedAssociation association = IncludedAssociation.NoAssociation;
            var departureTime = ((IDeparture) Stop).Time;
            
            (found, to) = GoesTo(destination, departureTime);  // Check our service
            
            if (!found && Service is ResolvedServiceWithAssociations withAssociations)
                (found, to, association) = AssociationGoesTo(withAssociations);    // Check associations

            if (found)
            {
                FoundToStop = to;
                Association = association;
            }
            
            return found;
            
            (bool success, ScheduleLocation to, IncludedAssociation association) AssociationGoesTo(ResolvedServiceWithAssociations service)
            {
                foreach (var association in service.Associations)
                {
                    if (association.IsJoin)
                    {
                        var foundInAssociated = JoinGoesTo(association);
                        if (foundInAssociated.success)
                            return foundInAssociated;
                    }

                    if(association.IsSplit)
                    {
                        var foundInAssociated = SplitGoesTo(association);
                        if (foundInAssociated.success)
                            return foundInAssociated;
                    }                 
                }
                return (false, null, IncludedAssociation.NoAssociation);
            }

            (bool success, ScheduleLocation to, IncludedAssociation association) JoinGoesTo(ResolvedAssociation association)
            {
                if (association.IsMain(Service.TimetableUid))
                    return (false, null, null);  // We're on the main, fail as join not possible
                
                var main = association.Details.Main;
                var joinStop = association.AssociatedService.GetStop(main.AtLocation, main.Sequence);
                var (success, to) = joinStop.GoesTo(destination, departureTime);
                return (success, 
                    to, 
                    success ? new IncludedAssociation(association.AssociatedService.TimetableUid)  : IncludedAssociation.NoAssociation);
            }
            
            (bool success, ScheduleLocation to, IncludedAssociation association) SplitGoesTo(ResolvedAssociation association)
            {
                if (!association.IsMain(Service.TimetableUid))
                    return (false, null, null);  // We're already on the split so cannot go to the main
                
                var main = association.Details.Main;
                var splitStop = Service.GetStop(main.AtLocation, main.Sequence);
                if( splitStop.IsBefore(departureTime, true))
                    return (false, null, null);  // We're past the split stop
                
                var firstStop = association.AssociatedService.Origin;
                var (success, to) = firstStop.GoesTo(destination, departureTime);
                return (success, 
                    to, 
                    success ? new IncludedAssociation(association.AssociatedService.TimetableUid)  : IncludedAssociation.NoAssociation);
            }
        }
        
        private (bool success, ScheduleLocation to) GoesTo(Station destination, Time departureTime)
        {
            foreach (var arrival in  Service.Details.Arrivals.Reverse())
            {
                // If arrival before Stop departure got to found stop so know that it does not go to location
                if (arrival.Time.IsBefore(departureTime))
                    return (false, null);

                if (destination.Equals(arrival.Station))
                {
                    return (true, (ScheduleLocation) arrival);
                }
            }

            return (false, null);
        }

        private bool IsBefore(Time time, bool useDepartures)
        {
            var thisTime = useDepartures ? ((IDeparture) Stop).Time : ((IArrival) Stop).Time;
            return thisTime.IsBefore(time);
        }
        
        public bool ComesFrom(Station origin)
        {
            var arrivalTime = ((IArrival) Stop).Time;
            foreach (var departure in Service.Details.Departures)
            {
                // If arrival at Stop before departure got to found stop so know that it does not come from location
                if (arrivalTime.IsBefore(departure.Time))
                    return false;

                if (origin.Equals(departure.Station))
                {
                    FoundFromStop = (ScheduleLocation) departure;
                    return true;
                }
            }

            return false;
        }
    }
}
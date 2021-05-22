using System;
using System.Collections.Generic;
using System.Linq;

namespace Timetable
{
    public class ResolvedServiceStop
    {
        public ResolvedService Service { get; }
        public ResolvedStop Stop { get; }
        public ResolvedStop FoundToStop { get; private set; } = null;
        public ResolvedStop FoundFromStop { get; private set; } = null;

        public IncludedAssociation Association { get; private set; } = IncludedAssociation.NoAssociation;
        
        public DateTime On => Service.On;

        public Toc Operator => Service.Details.Operator;
        public string ViaText { get; private set; }
        
        public ResolvedServiceStop(ResolvedService service, ScheduleLocation stop)
        {
            Service = service;
            Stop = new ResolvedStop(stop, service.On);
            ViaText = Stop.Stop.Station.GetViaText(Service.Details);
        }

        public override string ToString()
        {
            return $"{Service} {Stop.Stop}";
        }

        public bool IsNextDay(bool useDeparture)
        {
            if (useDeparture)
            {
                var departure = StopDeparture;
                return departure?.IsNextDay ?? false;
            }
            
            var arrival = StopArrival;
            return arrival?.IsNextDay ?? false;
        }

        private bool HasDeparture => (Stop.Stop as IDeparture)?.IsPublic ?? false;
        private bool HasArrival => (Stop.Stop as IArrival)?.IsPublic ?? false;
        
        private IDeparture StopDeparture => ((IDeparture) Stop.Stop);
        private IArrival StopArrival => ((IArrival) Stop.Stop);
        
        private ResolvedStop ResolveStop(IArrival location) => new ResolvedStop((ScheduleLocation) location, On);
        private ResolvedStop ResolveStop(IDeparture location) => new ResolvedStop((ScheduleLocation) location, On);
        
        public bool GoesTo(Station destination)
        {
            bool found;
            ResolvedStop to;
            IncludedAssociation association = IncludedAssociation.NoAssociation;
            var departureTime = StopDeparture.Time;
            
            (found, to) = GoesTo(destination, departureTime);  // Check our service
            
            if (!found && Service is ResolvedServiceWithAssociations withAssociations)
                (found, to, association) = AssociationGoesTo(destination, departureTime, withAssociations);    // Check associations

            if (found)
            {
                FoundToStop = to;
                Association = association;
            }
            
            return found;
        }
        
        private (bool success, ResolvedStop to) GoesTo(Station destination, Time departureTime)
        {
            foreach (var arrival in  Service.Details.Arrivals.Reverse())
            {
                // If arrival before Stop departure got to found stop so know that it does not go to location
                if (arrival.Time.IsBefore(departureTime))
                    return (false, null);

                if (destination.Equals(arrival.Station))
                {
                    return (true, ResolveStop(arrival));
                }
            }

            return (false, null);
        }

        private (bool success, ResolvedStop to, IncludedAssociation association) AssociationGoesTo(Station destination, Time departureTime, ResolvedServiceWithAssociations service)
        {
            foreach (var association in service.Associations.Where(a => !a.IsCancelled))
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
            
            
            (bool success, ResolvedStop to, IncludedAssociation association) JoinGoesTo(ResolvedAssociation association)
            {
                if (association.IsMain(Service.TimetableUid))
                    return (false, null, null);  // We're on the main, fail as join not possible
                
                var main = association.Details.Main;
                var joinStop = association.AssociatedService.GetStop(main.AtLocation, main.Sequence);
                var joinDeparture = joinStop.StopDeparture;
                var (success, to) = joinStop.GoesTo(destination, joinDeparture.Time);
                return (success, 
                    to, 
                    success ? new IncludedAssociation(association.AssociatedService.TimetableUid)  : IncludedAssociation.NoAssociation);
            }
            
            (bool success, ResolvedStop to, IncludedAssociation association) SplitGoesTo(ResolvedAssociation association)
            {
                if (!association.IsMain(Service.TimetableUid))
                    return (false, null, null);  // We're already on the split so cannot go to the main
                
                var main = association.Details.Main;
                var splitStop = Service.GetStop(main.AtLocation, main.Sequence);
                if( splitStop.IsBefore(departureTime, true))
                    return (false, null, null);  // We're past the split stop
                
                var firstStop = association.AssociatedService.Origin;
                var splitDeparture = firstStop.StopDeparture;
                var (success, to) = firstStop.GoesTo(destination, splitDeparture.Time);
                return (success, 
                    to, 
                    success ? new IncludedAssociation(association.AssociatedService.TimetableUid)  : IncludedAssociation.NoAssociation);
            }
        }
        
        private bool IsBefore(Time time, bool useDepartures)
        {
            var thisTime = useDepartures && HasDeparture ? StopDeparture.Time : StopArrival.Time;
            return thisTime.IsBefore(time);
        }
        
        public bool ComesFrom(Station origin)
        {
            bool found;
            ResolvedStop from;
            IncludedAssociation association = IncludedAssociation.NoAssociation;
            var arrivalTime = StopArrival.Time;
            
            (found, from) = ComesFrom(origin, arrivalTime);  // Check our service
            
            if (!found && Service is ResolvedServiceWithAssociations withAssociations)
                (found, from, association) = AssociationComesFrom(origin, arrivalTime, withAssociations);    // Check associations

            if (found)
            {
                FoundFromStop = from;
                Association = association;
            }
            
            return found;
        }
        
        private (bool success, ResolvedStop to) ComesFrom(Station origin, Time arrivalTime)
        {
            foreach (var departure in  Service.Details.Departures)
            {
                // If arrival at Stop before departure got to found stop so know that it does not come from location
                if (arrivalTime.IsBefore(departure.Time))
                    return (false, null);

                if (origin.Equals(departure.Station))
                {
                    return (true, ResolveStop(departure));
                }
            }

            return (false, null);
        }
        
        private (bool success, ResolvedStop to, IncludedAssociation association) AssociationComesFrom(Station origin, Time arrivalTime, ResolvedServiceWithAssociations service)
        {
            foreach (var association in service.Associations.Where(a => !a.IsCancelled))
            {
                if (association.IsJoin)
                {
                    var foundInAssociated = JoinComesFrom(association);
                    if (foundInAssociated.success)
                        return foundInAssociated;
                }

                if(association.IsSplit)
                {
                    var foundInAssociated = SplitComesFrom(association);
                    if (foundInAssociated.success)
                        return foundInAssociated;
                }                 
            }
            return (false, null, IncludedAssociation.NoAssociation);
            
            (bool success, ResolvedStop to, IncludedAssociation association) JoinComesFrom(ResolvedAssociation association)
            {
                if (association.IsAssociated(Service.TimetableUid))
                    return (false, null, null);  // We're already on the join so cannot come from main
                
                var main = association.Details.Main;
                var joinStop = Service.GetStop(main.AtLocation, main.Sequence);
                if( joinStop.IsAfter(arrivalTime, true))
                    return (false, null, null);  // We're past the join stop
                
                var lastStop = association.AssociatedService.Destination;
                var joinArrives = lastStop.StopArrival;
                var (success, to) = lastStop.ComesFrom(origin, joinArrives.Time);
                return (success, 
                    to, 
                    success ? new IncludedAssociation(association.AssociatedService.TimetableUid)  : IncludedAssociation.NoAssociation);
            }
            
            (bool success, ResolvedStop to, IncludedAssociation association) SplitComesFrom(ResolvedAssociation association)
            {
                if (association.IsMain(Service.TimetableUid))
                    return (false, null, null);  // We're on the main, fail as split not possible
                
                var main = association.Details.Main;
                var splitStop = association.AssociatedService.GetStop(main.AtLocation, main.Sequence);
                var splitArrival = splitStop.StopArrival;
                var (success, to) = splitStop.ComesFrom(origin, splitArrival.Time);
                return (success, 
                    to, 
                    success ? new IncludedAssociation(association.AssociatedService.TimetableUid)  : IncludedAssociation.NoAssociation);
            }
        }
        
        private bool IsAfter(Time time, bool useDepartures)
        {
            var thisTime = useDepartures && HasDeparture ? StopDeparture.Time : StopArrival.Time;
            return time.IsBefore(thisTime);
        }
    }
}
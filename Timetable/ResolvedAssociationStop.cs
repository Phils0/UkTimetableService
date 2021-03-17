namespace Timetable
{
    public class ResolvedAssociationStop
    {
        public ResolvedStop Stop { get; }
        public ResolvedStop AssociatedServiceStop { get; }
        public bool IsBroken => (Stop == null || AssociatedServiceStop == null);

        public ResolvedAssociationStop(ResolvedStop stop, ResolvedStop associatedServiceStop)
        {
            Stop = stop;
            AssociatedServiceStop = associatedServiceStop;
        }
        
    }
}
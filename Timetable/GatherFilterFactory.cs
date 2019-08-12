using System;

namespace Timetable
{
    public interface IFilterFactory
    {
        GatherFilterFactory.GatherFilter NoFilter { get; }
        GatherFilterFactory.GatherFilter DeparturesGoTo(Station destination);
        GatherFilterFactory.GatherFilter ArrivalsComeFrom(Station origin);
    }

    public class GatherFilterFactory : IFilterFactory
    {
        public delegate bool GatherFilter(ResolvedServiceStop service);
        
        public static readonly GatherFilter NoFilter = (s => true);

        GatherFilter IFilterFactory.NoFilter => NoFilter;
        
        public GatherFilter DeparturesGoTo(Station destination)
        {
            return (s => s.GoesTo(destination));
        }
        
        public GatherFilter ArrivalsComeFrom(Station origin)
        {
            return (s => s.ComesFrom(origin));
        }
    }
}
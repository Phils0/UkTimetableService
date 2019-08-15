using System;

namespace Timetable
{
    public interface IFilterFactory
    {
        GatherConfiguration.GatherFilter NoFilter { get; }
        GatherConfiguration.GatherFilter DeparturesGoTo(Station destination);
        GatherConfiguration.GatherFilter ArrivalsComeFrom(Station origin);
    }

    public class GatherFilterFactory : IFilterFactory
    {
        public static readonly GatherConfiguration.GatherFilter NoFilter = (s => true);

        GatherConfiguration.GatherFilter IFilterFactory.NoFilter => NoFilter;
        
        public GatherConfiguration.GatherFilter DeparturesGoTo(Station destination)
        {
            return (s => s.GoesTo(destination));
        }
        
        public GatherConfiguration.GatherFilter ArrivalsComeFrom(Station origin)
        {
            return (s => s.ComesFrom(origin));
        }
    }
}
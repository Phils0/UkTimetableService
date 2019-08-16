using System;
using System.Linq;

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
        public static readonly GatherConfiguration.GatherFilter NoFilter = (s => s);

        GatherConfiguration.GatherFilter IFilterFactory.NoFilter => NoFilter;
        
        public GatherConfiguration.GatherFilter DeparturesGoTo(Station destination)
        {
            return (s => s.Where(service => service.GoesTo(destination)));
        }
        
        public GatherConfiguration.GatherFilter ArrivalsComeFrom(Station origin)
        {
            return (s => s.Where(service => service.ComesFrom(origin)));
        }
    }
}
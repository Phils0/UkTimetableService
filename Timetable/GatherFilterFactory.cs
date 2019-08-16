using System;
using System.Linq;

namespace Timetable
{
    public interface IFilterFactory
    {
        GatherConfiguration.GatherFilter NoFilter { get; }
        GatherConfiguration.GatherFilter DeparturesGoTo(Station destination);
        GatherConfiguration.GatherFilter ArrivalsComeFrom(Station origin);
        GatherConfiguration.GatherFilter ProvidedByToc(string tocs, GatherConfiguration.GatherFilter innerFilter);
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
        
        public GatherConfiguration.GatherFilter ProvidedByToc(string tocs, GatherConfiguration.GatherFilter innerFilter)
        {
            return (s => innerFilter(s).
                Where(service => tocs.Contains(service.Details.Operator.Code)));
        }
    }
}
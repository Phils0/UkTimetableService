using System;
using System.Linq;
using Serilog;

namespace Timetable
{
    public interface IFilterFactory
    {
        GatherConfiguration.GatherFilter NoFilter { get; }
        GatherConfiguration.GatherFilter DeparturesGoTo(Station destination);
        GatherConfiguration.GatherFilter ArrivalsComeFrom(Station origin);
        GatherConfiguration.GatherFilter ProvidedByToc(TocFilter filter, GatherConfiguration.GatherFilter innerFilter);
    }

    public class GatherFilterFactory : IFilterFactory
    {
        public ILogger Log { get; }

        public GatherFilterFactory(ILogger log)
        {
            Log = log;
        }
        
        public static readonly GatherConfiguration.GatherFilter NoFilter = (s => s);

        GatherConfiguration.GatherFilter IFilterFactory.NoFilter => NoFilter;

        public GatherConfiguration.GatherFilter DeparturesGoTo(Station destination)
        {
            return (s => s.Where(SuppressExceptions(service => service.GoesTo(destination))));
        }

        private Func<ResolvedServiceStop, bool> SuppressExceptions(Func<ResolvedServiceStop, bool> inner)
        {
            return s =>
            {
                try
                {
                    return inner.Invoke(s);
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Error resolving stop {s}");
                    return false;
                }
            };
        }

        public GatherConfiguration.GatherFilter ArrivalsComeFrom(Station origin)
        {
            return (s => s.Where(SuppressExceptions(service => service.ComesFrom(origin))));
        }
        
        public GatherConfiguration.GatherFilter ProvidedByToc(TocFilter filter, GatherConfiguration.GatherFilter innerFilter)
        {
            return filter.NoFilter ? 
                innerFilter :
                (s => innerFilter(s).Where(SuppressExceptions(service => filter.IsValid(service.Operator))));
        }
    }
}
using System.Collections.Generic;
using AutoMapper;
using Serilog;

namespace Timetable.Web.Mapping
{
    public class TocConverter : IValueConverter<string, Toc>
    {
        private readonly TocLookup _lookup;

        public TocConverter(TocLookup lookup)
        {
            _lookup = lookup;
        }
              
        public Toc Convert(string source, ResolutionContext context)
        {
            return _lookup.Find(source);
        }
    }
}
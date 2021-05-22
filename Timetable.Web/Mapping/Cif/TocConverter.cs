using System;
using System.Collections.Generic;
using AutoMapper;

namespace Timetable.Web.Mapping.Cif
{
    public class TocConverter : IValueConverter<string, Toc>
    {
        public Toc Convert(string source, ResolutionContext context)
        {
            try
            {
                var lookup = context.Items["Tocs"] as TocLookup;
                return Convert(source, lookup);
            }
            catch (KeyNotFoundException ex)
            {
                throw new ArgumentException("Add TocLookup to options using key \"Tocs\" when call Map", ex);
            }

        }
        
        public Toc Convert(string source, TocLookup lookup)
        {
            return lookup.FindOrAdd(source);
        }
    }
}
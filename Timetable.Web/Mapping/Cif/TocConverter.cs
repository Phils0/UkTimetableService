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
                return lookup.Find(source);
            }
            catch (KeyNotFoundException ke)
            {
                throw new ArgumentException("Add TocLookup to options using key \"Tocs\"", ke);
            }
        }
    }
}
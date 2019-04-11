using System;
using System.Collections.Generic;
using AutoMapper;
using Serilog;

namespace Timetable.Web.Mapping
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
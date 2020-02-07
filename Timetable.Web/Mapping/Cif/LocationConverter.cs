using System;
using System.Collections.Generic;
using AutoMapper;

namespace Timetable.Web.Mapping.Cif
{
    public class LocationsConverter : IValueConverter<string, Location>
    {         
        public Location Convert(string source, ResolutionContext context)
        {
            try
            {
                var lookup = context.Items["Locations"] as ILocationData;
                
                if(lookup.TryGetLocation(source, out Location location))
                    return location;

                return null;    //TODO Fix.  Add to lookup if 
            }
            catch (KeyNotFoundException ke)
            {
                throw new ArgumentException("Add LocationData to options using key \"Locations\"", ke);
            }
        }
    }
}
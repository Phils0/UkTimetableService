using System;
using System.Collections.Generic;
using AutoMapper;

namespace Timetable.Web.Mapping.Cif
{
    internal class ActivitiesConverter: IValueConverter<string, Activities>
    {
        private Dictionary<string, Activities> _lookup = new Dictionary<string, Activities>(256);
        
        public Activities Convert(string source, ResolutionContext context)
        {
            if (!_lookup.TryGetValue(source, out Activities activities))
            {
                activities = AddValue(source);
            }
            return activities;
        }

        private Activities AddValue(string source)
        {
            var activities = new Activities(source);
            _lookup.Add(source, activities);
            return activities;
        }
    }
}
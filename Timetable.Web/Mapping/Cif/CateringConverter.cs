using System;
using System.Linq;
using AutoMapper;
using Serilog;

namespace Timetable.Web.Mapping.Cif
{
    internal class CateringConverter: IValueConverter<string, Catering>
    {
        private readonly ILogger _logger;

        internal CateringConverter(ILogger logger)
        {
            _logger = logger;
        }
        
        public Catering Convert(string source, ResolutionContext context)
        {
            source = source ?? string.Empty;
            return source.ToCharArray().Aggregate(Catering.None, (acc, c) => acc | ToCatering(c));
        }

        private Catering ToCatering(char c)
        {
            switch (c)
            {
               case 'C':
                   return Catering.Buffet;
               case 'F':
                   return Catering.FirstRestaurant;               
               case 'H':
                   return Catering.HotFood;               
               case 'M':
                   return Catering.FirstClass;               
               case 'R':
                   return Catering.Restaurant;               
               case 'T':
                   return Catering.Trolley;
               default:
                   throw new ArgumentOutOfRangeException($"Unknown catering value: {c}");
            }
        }
    }
}
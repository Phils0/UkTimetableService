using System;
using AutoMapper;
using Serilog;

namespace Timetable.Web.Mapping.Cif
{
    public class FromCifProfile : Profile
    {
        private const string PassengerAssociation = "P";
        
        public FromCifProfile()
        {
            CreateMap<CifParser.RdgRecords.Station, Timetable.Location>().
                ForPath(d =>d.Coordinates.Eastings, o => o.MapFrom(s => s.East)).
                ForPath(d => d.Coordinates.Northings, o => o.MapFrom(s => s.North)).
                ForPath(d => d.Coordinates.IsEstimate, o => o.MapFrom(s => s.PositionIsEstimated)).
                ForMember(d => d.Station, o => o.Ignore()).
                ForMember(d => d.Nlc, o => o.Ignore()).
                ForMember(d => d.IsActive, o => o.Ignore());
            
            CreateMap<CifParser.Records.TiplocInsertAmend, Timetable.Location>().
                ForMember(d => d.Tiploc, o => o.MapFrom(s => s.Code)).
                ForMember(d => d.ThreeLetterCode, o => o.MapFrom(s => s.ThreeLetterCode)).
                ForMember(d => d.Nlc, o => o.MapFrom(s => s.Nalco)).
                ForMember(d => d.Name, o => o.MapFrom(s => s.Description)).
                ForMember(d => d.InterchangeStatus, o => o.Ignore()).
                ForMember(d => d.Coordinates, o => o.Ignore()).
                ForMember(d => d.Station, o => o.Ignore()).
                ForMember(d => d.IsActive, o => o.MapFrom(s => false));
                       
            // Schedule records
            var cateringConverter = new CateringConverter(Log.Logger);
            CreateMap<CifParser.Records.ScheduleDetails, Timetable.CifSchedule>()
                .ForMember(d => d.Calendar, o => o.ConvertUsing(new CalendarConverter(), s => s))
                .ForMember(d => d.Service, o => o.Ignore())
                .ForMember(d => d.Properties, o => o.Ignore())
                .ForMember(d => d.Operator, o => o.Ignore())
                .ForMember(d => d.Locations, o => o.Ignore())
                .ForMember(d => d.Arrivals, o => o.Ignore())
                .ForMember(d => d.Departures, o => o.Ignore());
            CreateMap<CifParser.Records.ScheduleDetails, Timetable.CifScheduleProperties>()
                .DisableCtorValidation()
                .ForMember(d => d.Catering, o => o.ConvertUsing(cateringConverter))
                .ForMember(d => d.RetailServiceId, o => o.Ignore());

            var locationConverter = new LocationsConverter();
            CreateMap<CifParser.Records.Association, Association>()
                .ConstructUsing(a => new Association(Log.Logger))
                .ForMember(d => d.Main, o => o.MapFrom(s => AssociationConverter.ConvertMain(s)))
                .ForMember(d => d.Associated, o => o.MapFrom(s => AssociationConverter.ConvertAssociated(s)))
                .ForMember(d => d.Calendar, o => o.ConvertUsing(new CalendarConverter(), s => s))
                .ForMember(d => d.AtLocation, o => o.ConvertUsing(locationConverter, s => s.Location))
                .ForMember(d => d.Category, o => o.MapFrom(s => AssociationConverter.ConvertCategory(s.Category)))
                .ForMember(d => d.DateIndicator, o => o.MapFrom(s => AssociationConverter.ConvertDateIndicator(s.DateIndicator)))
                .ForMember(d => d.IsPassenger, o => o.MapFrom(s => PassengerAssociation.Equals(s.AssociationType)))
                .AfterMap((o, d) => AssociationConverter.SetServiceLocations(d));
            
            CreateMap<TimeSpan, Time>()
                .ConvertUsing(t => new Time(t));
            
            var activitiesConverter = new ActivitiesConverter();
            // Schedule location records
            CreateMap<CifParser.Records.OriginLocation, ScheduleStop>()
                .ForMember(d => d.Arrival, o => o.MapFrom(s => Time.NotValid))
                .ForMember(d => d.WorkingArrival, o => o.MapFrom(s => Time.NotValid))
                .ForMember(d => d.Departure, o => o.MapFrom(s => s.PublicDeparture))
                .ForMember(d => d.Location, o => o.ConvertUsing(locationConverter, s => s.Location))
                .ForMember(d => d.Activities, o => o.ConvertUsing(activitiesConverter, s => s.Activities))
                .ForMember(d => d.AdvertisedStop, o => o.Ignore())
                .ForMember(d => d.Schedule, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore());
            CreateMap<CifParser.Records.IntermediateLocation, ScheduleStop>()
                .ForMember(d => d.Arrival, o => o.MapFrom(s => s.PublicArrival))
                .ForMember(d => d.Departure, o => o.MapFrom(s => s.PublicDeparture))
                .ForMember(d => d.Location, o => o.ConvertUsing(locationConverter, s => s.Location))
                .ForMember(d => d.Activities, o => o.ConvertUsing(activitiesConverter, s => s.Activities))
                .ForMember(d => d.AdvertisedStop, o => o.Ignore())
                .ForMember(d => d.Schedule, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore());
            CreateMap<CifParser.Records.IntermediateLocation, SchedulePass>()
                .ForMember(d => d.Location, o => o.ConvertUsing(locationConverter, s => s.Location))
                .ForMember(d => d.PassesAt, o => o.MapFrom(s => s.WorkingPass))
                .ForMember(d => d.Activities, o => o.ConvertUsing(activitiesConverter, s => s.Activities))
                .ForMember(d => d.AdvertisedStop, o => o.Ignore())
                .ForMember(d => d.Schedule, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore());
            CreateMap<CifParser.Records.TerminalLocation, ScheduleStop>()
                .ForMember(d => d.Arrival, o => o.MapFrom(s => s.PublicArrival))
                .ForMember(d => d.Departure, o => o.MapFrom(s => Time.NotValid))
                .ForMember(d => d.WorkingDeparture, o => o.MapFrom(s => Time.NotValid))
                .ForMember(d => d.Location, o => o.ConvertUsing(locationConverter, s => s.Location))
                .ForMember(d => d.Activities, o => o.ConvertUsing(activitiesConverter, s => s.Activities))
                .ForMember(d => d.AdvertisedStop, o => o.Ignore())
                .ForMember(d => d.Schedule, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore());

            var scheduleConverter = new ScheduleConverter(new TocConverter(), Log.Logger);
            CreateMap<CifParser.Schedule, Timetable.CifSchedule>()
                .ConvertUsing(scheduleConverter);            
        }

    }
}
using System;
using System.Linq;

namespace Timetable.Test.Data
{
    public static class TestAssociations
    {
        public static Association CreateAssociation(
            string mainUid = "X12345",
            string associatedUid = "A98765",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null,
            Location location = null,
            AssociationCategory category = AssociationCategory.Join,
            AssociationDateIndicator dateIndicator = AssociationDateIndicator.Standard)
        {
            var association = new Association()
            {
                MainTimetableUid = mainUid,
                AssociatedTimetableUid = associatedUid,
                StpIndicator = indicator,
                Category = category,
                DateIndicator = dateIndicator,
                Calendar = calendar ?? TestSchedules.EverydayAugust2019,
                AtLocation = location ?? TestLocations.Surbiton
            };

            return association;
        }
        
        public static Association CreateAssociationWithServices(
            Service mainService = null,
            Service associatedService = null,
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null,
            Location location = null,
            AssociationCategory category = AssociationCategory.Join,
            AssociationDateIndicator dateIndicator = AssociationDateIndicator.Standard,
            string associatedUid = "A98765" 
            )
        {
            mainService = mainService ?? TestSchedules.CreateScheduleWithService("X12345").Service;
            associatedService = associatedService ?? TestSchedules.CreateScheduleWithService(associatedUid).Service;
            
            var association = CreateAssociation(mainService.TimetableUid, associatedService.TimetableUid, indicator, calendar, location, category,
                dateIndicator);
            
            association.AddToService(mainService, true);
            association.AddToService(associatedService, false);        
            
            return association;
        }
    }
}
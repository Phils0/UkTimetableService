using System;
using System.Linq;

namespace Timetable.Test.Data
{
    public static class TestAssociations
    {
        public static Association CreateAssociation(string mainUid = "X12345",
            string associatedUid = "A98765",
            StpIndicator indicator = StpIndicator.Permanent,
            ICalendar calendar = null,
            Location location = null,
            AssociationCategory category = AssociationCategory.Join,
            AssociationDateIndicator dateIndicator = AssociationDateIndicator.Standard,
            Service service = null)
        {
            var assocation = new Association()
            {
                MainTimetableUid = mainUid,
                AssociatedTimetableUid = associatedUid,
                StpIndicator = indicator,
                Category = category,
                DateIndicator = dateIndicator,
                Calendar = calendar ?? TestSchedules.EverydayAugust2019,
                AtLocation = location ?? TestLocations.Surbiton
            };

            if (service != null)
            {
                bool isMain = service.TimetableUid == mainUid;
                assocation.AddToService(service, isMain);
            }
            
            return assocation;
        }
    }
}
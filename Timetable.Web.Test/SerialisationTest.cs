using System;
using System.Text.Json;
using Timetable.Web.Model;
using Xunit;

namespace Timetable.Web.Test
{
    public class SerialisationTest
    {

        [Fact]
        public void SerialiseFoundServiceItem()
        {
            var response = new FoundServiceResponse()
            {
                Request = new SearchRequest() { Location = "EDB" },
                GeneratedAt = DateTime.Now,
                Services = new FoundServiceItem[]
                {
                    CreateServiceItem(),
                }
            };
            
            var jsonString = JsonSerializer.Serialize(response);
            Assert.Contains("X12345", jsonString);
        }

        private static Model.FoundServiceItem CreateServiceItem()
        {
            return new Model.FoundServiceItem()
            {
                At = new Model.ScheduledStop()
                {
                    Departure = new DateTime(2019, 12,10, 10, 0, 0),
                    Location = new LocationId()
                    {
                        Tiploc = "EDINBUR",
                        ThreeLetterCode = "EDB"
                    }
                },
                Service = new Model.Service()
                {
                    TimetableUid = "X12345",
                    RetailServiceId = "VT987600",
                    NrsRetailServiceId = "VT9876"
                }
            };
        }
        
        [Fact]
        public void SerialiseFoundSummaryItem()
        {
            var response = new FoundSummaryResponse()
            {
                Request = new SearchRequest() { Location = "EDB" },
                GeneratedAt = DateTime.Now,
                Services = new FoundSummaryItem[]
                {
                    CreateSummaryItem(),
                }
            };
            
            var jsonString = JsonSerializer.Serialize(response);
            Assert.Contains("X12345", jsonString);
        }
        
        private static Model.FoundSummaryItem CreateSummaryItem()
        {
            return new Model.FoundSummaryItem()
            {
                At = new Model.ScheduledStop()
                {
                    Departure = new DateTime(2019, 12,10, 10, 0, 0),
                    Location = new LocationId()
                    {
                        Tiploc = "EDINBUR",
                        ThreeLetterCode = "EDB"
                    }
                },
                Service = new Model.ServiceSummary()
                {
                    TimetableUid = "X12345",
                    RetailServiceId = "VT987600",
                    NrsRetailServiceId = "VT9876"
                }
            };
        }
    }
}
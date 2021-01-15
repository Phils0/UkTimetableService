using System;
using AutoMapper;
using Timetable.Test.Data;
using Xunit;

namespace Timetable.Web.Test.Mapping.Cif
{
    public class FromCifProfileAssociationTest
    {
        private static readonly MapperConfiguration FromCifProfileConfiguration =
            FromCifProfileLocationsTest.FromCifProfileConfiguration;

        [Fact]
        public void ValidMapping()
        {
            FromCifProfileConfiguration.AssertConfigurationIsValid();
        }

        [Fact]
        public void MapMainTimetableUid()
        {
            var output = MapAssociation();
            Assert.Equal("X12345", output.Main.TimetableUid);
        }
        
        [Fact]
        public void MapMainSequence()
        {
            var output = MapAssociation();
            Assert.Equal(1, output.Main.Sequence);
        }
        
        public static Association MapAssociation(CifParser.Records.Association input = null)
        {
            input = input ?? Test.Cif.TestAssociations.CreateAssociation();
            var mapper = FromCifProfileConfiguration.CreateMapper();
            return mapper.Map<CifParser.Records.Association, Timetable.Association>(input, o =>
            {
                o.Items.Add("Locations", TestData.Locations);
            });
        }
       
        [Fact]
        public void MapAssociatedTimetableUid()
        {
            var output = MapAssociation();
            Assert.Equal("A98765", output.Associated.TimetableUid);
        }
        
        [Fact]
        public void MapAssociatedSequence()
        {
            var output = MapAssociation();
            Assert.Equal(2, output.Associated.Sequence);
        }
        
        [Theory]
        [InlineData(CifParser.Records.StpIndicator.P, StpIndicator.Permanent)]
        [InlineData(CifParser.Records.StpIndicator.O, StpIndicator.Override)]
        [InlineData(CifParser.Records.StpIndicator.N, StpIndicator.New)]
        [InlineData(CifParser.Records.StpIndicator.C, StpIndicator.Cancelled)]
        public void MapStpIndicator(CifParser.Records.StpIndicator input, StpIndicator expected)
        {
            var association = Test.Cif.TestAssociations.CreateAssociation(stp: input);

            var output = MapAssociation(association);

            Assert.Equal(expected, output.StpIndicator);
        }
        
        [Fact]
        public void MapCalendar()
        {
            var output = MapAssociation();
            var calendar = output.Calendar as Calendar;
            Assert.Equal(new DateTime(2019, 8, 1), calendar.RunsFrom);
            Assert.Equal(new DateTime(2019, 8, 31), calendar.RunsTo);
            Assert.Equal(DaysFlag.Weekdays, calendar.DayMask);
            Assert.Equal(BankHolidayRunning.RunsOnBankHoliday, calendar.BankHolidays);
        }

        [Fact]
        public void MapReusesExistingCalendar()
        {
            var mapper = FromCifProfileConfiguration.CreateMapper();

            var output1 = MapAssociation();
            var output2 = MapAssociation();

            Assert.NotSame(output1, output2);
            Assert.Same(output1.Calendar, output2.Calendar);
        }
        
        [Fact]
        public void MapAtLocation()
        {
            var output = MapAssociation();
            Assert.Equal("SUR", output.AtLocation.ThreeLetterCode);
        }
        
        [Theory]
        [InlineData("JJ", AssociationCategory.Join)]
        [InlineData("VV", AssociationCategory.Split)]
        [InlineData("NP", AssociationCategory.NextPrevious)]
        [InlineData("LK", AssociationCategory.Linked)]
        [InlineData("", AssociationCategory.None)]
        [InlineData(null, AssociationCategory.None)]
        public void MapCategory(string input, AssociationCategory expected)
        {
            var association = Test.Cif.TestAssociations.CreateAssociation();
            association.Category = input;
            var output = MapAssociation(association);
            
            Assert.Equal(expected, output.Category);
        }

        [Theory]
        [InlineData("S", AssociationDateIndicator.Standard)]
        [InlineData("P", AssociationDateIndicator.PreviousDay)]
        [InlineData("N", AssociationDateIndicator.NextDay)]
        [InlineData("", AssociationDateIndicator.None)]
        [InlineData(null, AssociationDateIndicator.None)]
        public void MapDateIndicator(string input, AssociationDateIndicator expected)
        {
            var association = Test.Cif.TestAssociations.CreateAssociation();
            association.DateIndicator = input;
            var output = MapAssociation(association);
            
            Assert.Equal(expected, output.DateIndicator);
        }

        [Theory]
        [InlineData("O",false)]
        [InlineData("P", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void MapIsPublic(string input, bool expected)
        {
            var association = Test.Cif.TestAssociations.CreateAssociation(type: input);
            var output = MapAssociation(association);
            
            Assert.Equal(expected, output.IsPassenger);
        }

    }
}
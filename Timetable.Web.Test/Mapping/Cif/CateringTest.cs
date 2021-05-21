using NSubstitute;
using Serilog;
using Timetable.Web.Mapping.Cif;
using Xunit;

namespace Timetable.Web.Test.Mapping.Cif
{
    public class CateringTest
    {
        public static TheoryData<string, Catering> TestCatering =>
            new TheoryData<string, Catering>()
            {
                {"C", Catering.Buffet},            // C Buffet Service
                {"F", Catering.FirstRestaurant},   // F Restaurant Car available for First Class passengers
                {"H", Catering.HotFood},           // H Service of hot food available
                {"M", Catering.FirstClass},        // M Meal included for First Class passengers
                {"R", Catering.Restaurant},        // R Restaurant
                {"T", Catering.Trolley},           // T Trolley Service
                {"MT", Catering.FirstClass | Catering.Trolley},
                {"", Catering.None}
            };
        
        [Theory]
        [MemberData(nameof(TestCatering))]
        public void Convert(string input, Catering expected)
        {
            var converter = new CateringConverter(Substitute.For<ILogger>());
            var catering = converter.Convert(input, null);

            Assert.Equal(expected, catering);
        }
    }
}
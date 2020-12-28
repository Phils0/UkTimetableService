using System.Threading;
using System.Threading.Tasks;
using Timetable.Web.Loaders;
using Xunit;

namespace Timetable.Web.Test.Loaders
{
    public class NopLoaderTest
    {
        [Fact]
        public async Task LoadEmptyRealtime()
        {
            var data = new Timetable.Data();
            var loader = new NopLoader();
            data = await loader.EnrichReferenceDataAsync(data,  CancellationToken.None);
            
            Assert.NotNull(data.Darwin);
            Assert.Empty(data.Darwin.CancelReasons);    
            Assert.Empty(data.Darwin.LateRunningReasons);   
            Assert.Empty(data.Darwin.Sources);  
        }
    }
}
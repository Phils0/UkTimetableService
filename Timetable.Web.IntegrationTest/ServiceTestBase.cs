using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Timetable.Web.IntegrationTest
{
    public abstract class ServiceTestBase
    {
        private readonly WebServiceFixture _fixture;

        protected IHost Host => _fixture.Host;
        public ILogger Logger => _fixture.Logger;

        public ServiceTestBase(WebServiceFixture fixture)
        {
            _fixture = fixture;  
        }
       
    }
}
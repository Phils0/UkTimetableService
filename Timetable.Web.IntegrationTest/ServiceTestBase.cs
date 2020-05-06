using System;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Timetable.Web.IntegrationTest
{
    public abstract class ServiceTestBase
    {
        protected IHost Host { get;}

        public ServiceTestBase(WebServiceFixture fixture)
        {
            Host = fixture.Host;  
        }
       
    }
}
using System.IO;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Timetable.Web.ServiceConfiguration;
using Xunit;

namespace Timetable.Web.Test.ServiceConfiguration
{
    public class ExceptionHandlerTest
    {
        [Fact]
        public async void ErrorResponseSet()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var handler = new ExceptionHandler();

            await handler.HandleUncaughtException(context);

            context.Response.StatusCode.Should().Be(500);
            context.Response.ContentType.Should().Be("application/json");
            
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var body = reader.ReadToEnd();
            body.Should().Be("{ \"code\":500, \"reason\":\"Internal Server Error\" }");
        }
    }
}
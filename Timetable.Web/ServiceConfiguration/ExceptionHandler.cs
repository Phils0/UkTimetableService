using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Timetable.Web.Plugin;

namespace Timetable.Web.ServiceConfiguration
{
    internal class ExceptionHandler : IPlugin
    {
        public ILogger Logger { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Do nothing
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errApp => errApp.Run(HandleUncaughtException));
            }
        }
        internal async Task HandleUncaughtException(HttpContext context)
        {
            var exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();
            var path = exceptionHandlerPathFeature?.Path;
            var error = exceptionHandlerPathFeature?.Error;
            Log.Error(error, "Unhandled error: {path}", path);

            var statusCode = (int) HttpStatusCode.InternalServerError;
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                string.Format(
                    "{{ \"code\":{0}, \"reason\":\"{1}\" }}",
                    statusCode,
                    ReasonPhrases.GetReasonPhrase(statusCode)));
        }

        public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
        {
            // Do nothing
        }
}

}
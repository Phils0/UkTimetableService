﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Timetable.Web
{
    public class Startup
    {
        private static readonly TimeSpan Timeout = new TimeSpan(0, 2, 0);
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var factory = new Factory(Factory.MapperConfiguration, Configuration, Log.Logger);
            var data = LoadData(factory);

            services.
                AddSingleton<Data>(data).
                AddSingleton<IMapper>(factory.CreateMapper()).
                AddSingleton<IReference>(factory.CreateReferenceService(data)).
                AddSwaggerGen(ConfigureSwagger).
                AddMvc().
                SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        private static Data LoadData(Factory factory)
        {
            var config = factory.Configuration;
            
            try
            {
                    var loader = factory.CreateDataLoader();
                    var loaderTask = loader.LoadAsync(CancellationToken.None);
                    
                    var loaded = Task.WaitAll(new[]
                    {
                        loaderTask
                    }, Timeout);
        
                    if (!loaded)
                        throw new InvalidDataException($"Timeout loading file: {config.TimetableArchiveFile}");
        
                    return loaderTask.Result;
            }
            catch (Exception e)
            {
                Log.Fatal(e,"Timetable not loaded: {file}", config.TimetableArchiveFile);
                throw;
            }

        }

        private void ConfigureSwagger(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v0", new Info {Title = "Seatbox", Version = "v0"});

            var controllerAssembly = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var controllerPath = Path.Combine(AppContext.BaseDirectory, controllerAssembly);
            options.IncludeXmlComments(controllerPath);
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(AddExceptionHandler);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v0/swagger.json", "Timetable V0"); });

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseStatusCodePages();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
        
        private void AddExceptionHandler(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();
                var path = exceptionHandlerPathFeature?.Path;
                var error = exceptionHandlerPathFeature?.Error;
                Log.Error(error, "Unhandled error: {path}", path);

                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
            });
        }
    }
}
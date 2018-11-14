using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Serilog.Events;

namespace Checkout.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var logDirInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "checkout-api", "logs"));
            if (!logDirInfo.Exists)
            {
                logDirInfo.Create();
            }
            const string LogFilename = "{Date}.checkout-api.log";
            var logPath = Path.Combine(logDirInfo.FullName, LogFilename);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(
                    pathFormat: logPath,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                .CreateLogger();

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddMemoryCache();

            // swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Checkout API",
                    Version = "v1"
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

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
                app.UseHsts();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = httpReq.Host.Value);
                c.RouteTemplate = "api/swagger/{documentName}/swagger.json";
            });


            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api/swagger";
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Netpips API v1");
            });

            // CORS
            app.UseCors(builder => builder
                .AllowAnyMethod()
                .AllowAnyHeader()
            );
            app.UseRequestLocalization(builder => { builder.DefaultRequestCulture = new RequestCulture(Program.EnUsCulture); });

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

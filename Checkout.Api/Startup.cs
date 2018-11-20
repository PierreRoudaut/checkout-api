using Checkout.Api.Carts.Services;
using Checkout.Api.Core.Models;
using Checkout.Api.Hubs;
using Checkout.Api.Products.Models;
using Checkout.Api.Products.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.IO;

namespace Checkout.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            const string LogFilename = "{Date}.checkout-api.log";
            var logPath = Path.Combine(Program.LogsDirInfo.FullName, LogFilename);
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
            services.AddMemoryCache();
            services.AddCors();

            //services.AddRouting(options => options.LowercaseUrls = true);
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
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ProductImageService>();
            services.AddScoped<CartService>();
            services.AddScoped<NotifyHub>();

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(
                options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Converters = new List<JsonConverter> { new StringEnumConverter() };
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Serve medialibrary folder as static
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                FileProvider = new PhysicalFileProvider(Program.PublicFilesDirInfo.FullName),
                RequestPath = "/api/public",
                OnPrepareResponse = ctx =>
                {
                    Log.Logger.Information("Serving static file: " + ctx.File.Name);
                }
            });


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
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "v1");
            });

            // CORS
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );

            app.UseRequestLocalization(builder => { builder.DefaultRequestCulture = new RequestCulture(Program.EnUsCulture); });

            //app.UseHsts();
            //app.UseHttpsRedirection();

            app.UseSignalR(routes =>
            {
                routes.MapHub<NotifyHub>("/hub");
            });

            app.UseMvc();
        }
    }
}

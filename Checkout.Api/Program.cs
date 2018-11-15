using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Checkout.Api
{
    public class Program
    {
        public static CultureInfo EnUsCulture = new CultureInfo("en-US");
        public static string AppHome = Path.Combine(Path.GetTempPath(), "checkout-api");
        public static DirectoryInfo LogsDirInfo;
        public static DirectoryInfo AppDirInfo;
        public static DirectoryInfo PublicFilesDirInfo;


        public static void Main(string[] args)
        {
            if (System.Diagnostics.Debugger.IsAttached == false)
            {
                System.Diagnostics.Debugger.Launch();
            }

            // Main app folder
            AppDirInfo = new DirectoryInfo(AppHome);
            if (!AppDirInfo.Exists)
            {
                AppDirInfo.Create();
            }

            // logs folder
            LogsDirInfo = new DirectoryInfo(Path.Combine(AppHome, "logs"));
            if (!LogsDirInfo.Exists)
            {
                LogsDirInfo.Create();
            }

            // public files folder
            PublicFilesDirInfo = new DirectoryInfo(Path.Combine(AppHome, "files"));
            if (!PublicFilesDirInfo.Exists)
            {
                PublicFilesDirInfo.Create();
            }


            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseStartup<Startup>();

    }
}

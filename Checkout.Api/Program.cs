using System.Globalization;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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
        public static DirectoryInfo ProductImagesDirInfo;

        public static void SetupAppDirectories()
        {
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
            PublicFilesDirInfo = new DirectoryInfo(Path.Combine(AppHome, "public"));
            if (!PublicFilesDirInfo.Exists)
            {
                PublicFilesDirInfo.Create();
            }

            ProductImagesDirInfo = PublicFilesDirInfo.CreateSubdirectory("images").CreateSubdirectory("products");
        }

        public static void Main(string[] args)
        {
            SetupAppDirectories();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseStartup<Startup>();

    }
}

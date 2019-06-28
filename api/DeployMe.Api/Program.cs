using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace DeployMe.Api
{
    public static class Program
    {
        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
    }
}

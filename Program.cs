using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microcosm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // This is here due to a bug in ASP.NET
            if (!Directory.Exists("wwwroot"))
            {
                Directory.CreateDirectory("wwwroot");
            }
            File.WriteAllText("wwwroot/index.html", "<!DOCTYPE html><html><head><title>System Ready</title></head><body><p>System Ready</p></body></html>");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(options =>
                    {
                        options.Limits.MaxRequestBodySize = long.MaxValue;
                    });
                });
    }
}

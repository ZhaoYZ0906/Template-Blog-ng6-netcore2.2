using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using BlogDemo.Infrastructure.Database;
using Serilog;
using Serilog.Events;

namespace BlogDemo.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(Path.Combine("logs", "log.txt"), rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            using (var _scope = host.Services.CreateScope())
            {
                var service = _scope.ServiceProvider;
                var LoggerFactory = service.GetRequiredService<ILoggerFactory>();

                try
                {
                    var context = service.GetRequiredService<MyContext>();
                    MyContextSeed.SeedAsync(context,LoggerFactory).Wait();
                }
                catch (Exception e)
                {
                    var log = LoggerFactory.CreateLogger<Program>();
                    log.LogError(e, "初始化数据失败!!!!!");
                }
                
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup(typeof(StartupDevelopment).GetTypeInfo().Assembly.FullName)
            .UseSerilog();
    }
}

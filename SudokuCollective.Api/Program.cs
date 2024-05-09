using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SudokuCollective.Api
{
    /// <summary>
    /// Program Class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main method which is the solutions entry point.
        /// </summary>
        public static void Main(string[] args)
        {
            // Add logger to Program to aid in debugging remote hosts...
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
                builder.AddEventSourceLogger();
            });

            var _logger = loggerFactory.CreateLogger<Program>();

            try
            {
                CreateHostBuilder(args)
                    .ConfigureWebHost(configure =>
                    {
                        configure.ConfigureKestrel(options =>
                        {
                            options.Limits.MaxConcurrentConnections = null;
                            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
                            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                            options.Limits.Http2.KeepAlivePingDelay = TimeSpan.FromSeconds(30);
                            options.Limits.Http2.KeepAlivePingTimeout = TimeSpan.FromMinutes(1);
                        });
                    })
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(message: ex.Message);
            }
        }

        /// <summary>
        /// A method which starts the rest api.
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(
                        hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

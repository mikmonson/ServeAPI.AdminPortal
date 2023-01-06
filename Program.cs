using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using AdminPortal.Data;
using AdminPortal.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdminPortal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<EdgeDBContext>();
                    InitialData.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .UseUrls("http://0.0.0.0:5100", "https://0.0.0.0:5101")                  
                    .ConfigureKestrel(serverOptions =>
                     {
                         serverOptions.Limits.MaxConcurrentConnections = 100;
                         serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
                         serverOptions.Limits.MaxRequestBodySize = 15 * 1024 * 1024;
                         serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                         serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
                         //serverOptions.Listen(IPAddress.Any, 443);
                         //serverOptions.Listen(IPAddress.Loopback, 5001);

                         serverOptions.ConfigureHttpsDefaults(opt =>
                         {
                             opt.ServerCertificate = new X509Certificate2(FilePath.cert_path+FilePath.certfile, FilePath.certkey);
                             opt.SslProtocols = SslProtocols.Tls12;
                         });

                     });
                });
    }
}

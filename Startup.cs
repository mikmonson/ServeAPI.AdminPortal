using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore; // пространство имен EntityFramework
using AdminPortal.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Diagnostics;
using AdminPortal.Data;

namespace AdminPortal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            FilePath.cert_path= configuration.GetValue<string>("cert_path");
            FilePath.device_path = configuration.GetValue<string>("device_path");
            FilePath.template_path = configuration.GetValue<string>("template_path");
            FilePath.path_separator = configuration.GetValue<string>("path_separator");
            FilePath.file_path = configuration.GetValue<string>("file_path");
            FilePath.certfile = configuration.GetValue<string>("certfile");
            FilePath.certkey = configuration.GetValue<string>("certkey");
            Debug.WriteLine("Settings have been loaded");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("DBConnection");
            //services.AddDbContext<EdgeDBContext>(options => options.UseSqlServer(connection));
            services.AddDbContext<EdgeDBContext>(options => options.UseMySql(connection));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                    options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                });

            services.AddControllersWithViews();
            services.Configure<FormOptions>(options =>
            {
                // Set the limit to 4 MB
                options.MultipartBodyLengthLimit = 10885760;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Admin/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Admin}/{action=Index}/{id?}");
            });
        }
    }
}

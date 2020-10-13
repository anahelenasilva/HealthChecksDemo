using BlazorDemo.Data;
using BlazorDemo.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace BlazorDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            //Configura Health Checks
            services.AddHealthChecks()
                .AddCheck<ResponseTimeHealthCheck>("Network speed test", null, new[] { "service" })
                .AddCheck("Database", () => HealthCheckResult.Healthy("The check of the database worked."), new[] { "database", "sql" });

            //HealthCheckResult.Degraded = there is a problem but the application is still working

            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<ResponseTimeHealthCheck>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                //it says that the application is running
                endpoints.MapHealthChecks("/quickhealth", new HealthCheckOptions
                {
                    Predicate = _ => false //the _ is a variable that you dont care about
                });

                endpoints.MapHealthChecks("/health/services", new HealthCheckOptions
                {
                    Predicate = reg => reg.Tags.Contains("service"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
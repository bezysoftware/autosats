using AutoSats.Data;
using AutoSats.Execution;
using AutoSats.Execution.Services;
using AutoSats.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace AutoSats
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
            var connectionString = Configuration.GetConnectionString("AutoSatsDatabase");

            DbInitializer.InitializeQuartzDatabase(connectionString);

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddDbContext<SatsContext>(x =>
            {
                x.UseSqlite(connectionString);
                x.EnableSensitiveDataLogging();
            });
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UsePersistentStore(x =>
                {
                    x.UseProperties = true;
                    x.UseSQLite(connectionString);
                    x.UseJsonSerializer();
                });

                q.AddJob<ExchangeJob>(ExecutionConsts.ExchangeJobKey, x => x.StoreDurably());
            });

            services.AddBitcoinRPC();
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<IExchangeService, ExchangeService>();
            services.AddScoped<IExchangeScheduler, ExchangeScheduler>();
            services.AddScoped<IExchangeScheduleRunner, ExchangeScheduleRunner>();
            services.AddScoped<IWalletService, WalletService>();

            services.Configure<RazorPagesOptions>(options => options.RootDirectory = "/Views/Pages");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SatsContext db)
        {
            db.Database.Migrate();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}

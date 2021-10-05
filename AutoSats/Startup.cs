using AutoSats.Configuration;
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
using System.Linq;

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
                    // use Microsoft.Data.Sqlite.Core instead of System.Data.Sqlite.Core
                    // todo: replace with built-in extension method once released: https://github.com/quartznet/quartznet/pull/1275
                    x.SetProperty($"quartz.dataSource.{SchedulerBuilder.AdoProviderOptions.DefaultDataSourceName}.provider", "SQLite-Microsoft");
                });

                q.AddJob<ExchangeJob>(ExecutionConsts.ExchangeJobKey, x => x.StoreDurably());
            });

            services.AddBitcoinRPC();
            services.AddAutoMapper(typeof(Startup));
            services.AddAntDesign();
            services.AddHttpClient();

            var exchanges = Configuration
                .GetSection("Exchanges")
                .Get<ExchangeOptions[]>()
                .OrderBy(x => x.Name)
                .ToArray()
                .AsEnumerable();

            services.AddSingleton(exchanges); 
            services.AddTransient<IExchangeService, ExchangeService>();
            services.AddScoped<IExchangeAPIProvider, ExchangeAPIProvider>(); 
            services.AddScoped<IExchangeServiceFactory, ExchangeServiceFactory>();
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

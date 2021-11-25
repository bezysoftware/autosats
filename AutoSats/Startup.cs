using AutoSats.Configuration;
using AutoSats.Execution;
using AutoSats.Execution.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AutoSats;

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
        var walletType = Configuration.GetSection("Wallet").GetValue<WalletType>("Type");

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

        services.AddWalletServices(walletType);
        services.AddAutoMapper(typeof(Startup));
        services.AddAntDesign();
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "AutoSats";
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

        services.AddAuthorization(x =>
        {
            if (string.IsNullOrEmpty(Configuration.GetSection("Application").GetValue<string>("Password")))
            {
                    // no authorization
                    x.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build();
            }
        });

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
        services.AddSingleton<ILoginService, LoginService>();

        services.Configure<RazorPagesOptions>(options => options.RootDirectory = "/Views/Pages");
        services.Configure<ApplicationOptions>(Configuration.GetSection("Application"));
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
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}

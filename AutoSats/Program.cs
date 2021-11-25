using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(x => x.UseStartup<Startup>())
    .Build()
    .Run();
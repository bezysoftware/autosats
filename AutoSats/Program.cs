Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(x => x.UseStartup<Startup>())
    .Build()
    .Run();
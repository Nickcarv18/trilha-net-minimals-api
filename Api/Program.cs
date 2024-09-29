using trilha_net_minimals_api;

IHostBuilder CreateHostBuilder(string[] args){
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => 
        {
            webBuilder.UseStartup<Startup>();
        });
}

CreateHostBuilder(args).Build().Run();
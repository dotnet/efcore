using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetHostingPortableApp
{
    public class Startup
    {
        private IConfiguration _config;
        public Startup(IApplicationEnvironment app)
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(app.ApplicationBasePath)
                .AddJsonFile("config.json")
                .Build();
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkSqlite()
                .AddDbContext<TestContext>(o => o.UseSqlite(_config["Sqlite"]));
        }

        public void Configure(IApplicationBuilder app)
        {

        }
        
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseDefaultHostingConfiguration(args)
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
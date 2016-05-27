using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryUsingSqlite;

namespace AspNetHostingPortableApp
{
    public class Startup
    {
        private IConfiguration _config;
        public Startup(IHostingEnvironment app)
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(app.ContentRootPath)
                .AddJsonFile("config.json")
                .Build();
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<TestContext>(o => o.UseSqlite(_config["TestContext"]))
                .AddDbContext<LibraryContext>(o => o.UseSqlite(_config["LibraryContext"], b => b.MigrationsAssembly("AspNetApp")));
        }

        public void Configure(IApplicationBuilder app)
        {

        }
        
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
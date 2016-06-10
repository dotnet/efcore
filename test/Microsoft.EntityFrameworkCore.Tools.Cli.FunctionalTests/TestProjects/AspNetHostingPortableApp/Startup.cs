using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetHostingPortableApp
{
    public class Startup
    {
        private IConfiguration _config;
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("config.json");

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            _config = builder.Build();
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<TestContext>(o => o.UseSqlite(_config["TestContext"]));
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
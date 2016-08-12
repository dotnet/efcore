using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DesktopClassLibrary;
using NetStandardClassLibrary;

namespace DesktopStartupApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<DesktopContext>(o => o.UseSqlite("Filename=./desktop.db"))
                .AddDbContext<NetStandardContext>(o => o.UseSqlite("Filename=./netstandard.db",
                    b => b.MigrationsAssembly("DesktopStartupApp")));
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
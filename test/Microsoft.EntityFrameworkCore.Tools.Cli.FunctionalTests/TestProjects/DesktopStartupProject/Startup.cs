using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using DesktopClassLibrary;

namespace DesktopStartupProject
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DesktopContext>(o => o.UseSqlite("Filename=./desktop.db"));
            
            // Exercises assembly dependency conflict resolution 
            JsonConvert.SerializeObject(new object());
        }
    }
}
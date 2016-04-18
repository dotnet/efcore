using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LibraryWithTools
{
    public class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var o = new object();
            JsonConvert.SerializeObject(o);
            options.UseSqlite("Filename=./library.db");
        }
        
        public DbSet<Blog> Blogs { get; set; }
    }
    
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
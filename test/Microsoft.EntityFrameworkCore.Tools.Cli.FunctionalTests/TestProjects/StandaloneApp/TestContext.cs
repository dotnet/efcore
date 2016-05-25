using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace PortableApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }

    public class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var o = new object();
            JsonConvert.SerializeObject(o);
            options.UseSqlite("Filename=./test.db");
        }
        
        public DbSet<Blog> Blogs { get; set; }
    }
    
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
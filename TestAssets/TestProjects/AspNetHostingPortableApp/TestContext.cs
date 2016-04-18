using Microsoft.EntityFrameworkCore;

namespace AspNetHostingPortableApp
{
    public class TestContext : DbContext
    {
        public TestContext(DbContextOptions o) :base(o) { }
        
        public DbSet<Blog> Blogs { get; set; }
    }
    
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
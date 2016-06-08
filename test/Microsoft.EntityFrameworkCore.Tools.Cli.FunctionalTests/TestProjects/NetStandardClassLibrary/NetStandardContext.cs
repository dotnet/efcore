using Microsoft.EntityFrameworkCore;

namespace NetStandardClassLibrary
{
    public class NetStandardContext : DbContext
    {
        public NetStandardContext(DbContextOptions<NetStandardContext> options) : base (options) { }

        public DbSet<Blog> Blogs { get; set; }
    }

    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
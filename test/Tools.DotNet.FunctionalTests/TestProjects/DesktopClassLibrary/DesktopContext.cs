using Microsoft.EntityFrameworkCore;

namespace DesktopClassLibrary
{
    public class DesktopContext : DbContext
    {
        public DesktopContext(DbContextOptions<DesktopContext> options) : base (options) { }

        public DbSet<Blog> Blogs { get; set; }
    }

    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
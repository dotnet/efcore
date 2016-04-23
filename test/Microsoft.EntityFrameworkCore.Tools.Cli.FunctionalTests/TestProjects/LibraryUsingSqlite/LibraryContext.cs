using Microsoft.EntityFrameworkCore;

namespace LibraryUsingSqlite
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        { }
        
        public DbSet<Blog> Blogs { get; set; }
    }
    
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
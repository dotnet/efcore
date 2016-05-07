using Microsoft.EntityFrameworkCore;

namespace DesktopClassLibrary
{
    public class DesktopContext : DbContext
    {
        public DesktopContext(DbContextOptions<DesktopContext> options) : base (options) { }
    }
}
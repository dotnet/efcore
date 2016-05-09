using Microsoft.EntityFrameworkCore;

namespace NetStandardClassLibrary
{
    public class NetStandardContext : DbContext
    {
        public NetStandardContext(DbContextOptions<NetStandardContext> options) : base (options) { }
    }
}
#if !(NET451 && DRIVER_NOT_SIGNED)
using Microsoft.EntityFrameworkCore.Annotations;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    [MongoDatabase(database: "testdb")]
    public class TestMongoDbContext : DbContext
    {
        public TestMongoDbContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<SimpleRecord> SimpleRecords { get; private set; }

        public DbSet<ComplexRecord> ComplexRecords { get; private set; }

        public DbSet<RootType> RootTypes { get; private set; }
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)
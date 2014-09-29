namespace DbContextPerfTests
{
    using Model;

    public class DbContextPerfTests : DbContextPerfTestsBase
    {
        public void DbContextQuery()
        {
            DbContextQuery(() => new AdvWorksDbContext(ConnectionString, ServiceProvider, Options));
        }

        public void DbContextQueryNoTracking()
        {
            DbContextQueryNoTracking(() => new AdvWorksDbContext(ConnectionString, ServiceProvider, Options));
        }

        public void DbContextInsert()
        {
            DbContextInsert(() => new AdvWorksDbContext(ConnectionString, ServiceProvider, Options));
        }

        public void DbContextDeleteSetup()
        {
            DbContextDeleteSetup(() => new AdvWorksDbContext(ConnectionString, ServiceProvider, Options));
        }

        public void DbContextDelete()
        {
            DbContextDelete(() => new AdvWorksDbContext(ConnectionString, ServiceProvider, Options));
        }

        public void DbContextQueryWithThreadsNoTracking()
        {
            DbContextQueryWithThreadsNoTracking(() => new AdvWorksDbContext(ConnectionString, ServiceProvider, Options));
        }

        public void DbContextUpdateSetup()
        {
            DbContextUpdateSetup(() => new AdvWorksDbContext(ConnectionString, ServiceProvider, Options));
        }

        public void DbContextUpdate()
        {
            DbContextUpdate(() => new AdvWorksDbContext(ConnectionString, ServiceProvider, Options));
        }
    }
}
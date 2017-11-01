using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesInMemoryWithSensitiveDataLoggingFixture : UpdatesInMemoryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging();
    }
}
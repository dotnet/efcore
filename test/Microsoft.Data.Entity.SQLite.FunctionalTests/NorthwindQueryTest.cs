using System;
using Microsoft.Data.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class NorthwindQueryTest : NorthwindQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        // TODO: SQLite doesn't support TOP. Use LIMIT
        #region TOP

        public override void Select_scalar_primitive_after_take()
        {
        }

        public override void SelectMany_correlated_subquery_hard()
        {
        }

        public override void Take_simple()
        {
        }

        public override void Take_simple_projection()
        {
        }

        public override void Take_with_single()
        {
        }

        public override void Where_primitive()
        {
        }

        #endregion

        private readonly NorthwindQueryFixture _fixture;

        public NorthwindQueryTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }

    public class NorthwindQueryFixture : NorthwindQueryFixtureBase, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly TestDatabase _testDatabase;

        public NorthwindQueryFixture()
        {
            _testDatabase = TestDatabase.Northwind();
            _options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseSQLite(_testDatabase.Connection.ConnectionString);
        }

        public DbContext CreateContext()
        {
            return new DbContext(_options);
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
        }
    }
}

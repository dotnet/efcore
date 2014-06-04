using System;
using Microsoft.Data.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class NorthwindQueryTest : NorthwindQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        // TODO: Blocked by #293
        #region 293

        public override void Join_customers_orders()
        {
        }

        public override void Join_customers_orders_select()
        {
        }

        public override void OrderBy_Distinct()
        {
        }

        public override void OrderBy_Join()
        {
        }

        public override void OrderBy_multiple()
        {
        }

        public override void OrderBy_scalar_primitive()
        {
        }

        public override void OrderBy_Select()
        {
        }

        public override void OrderBy_SelectMany()
        {
        }

        public override void OrderBy_ThenBy()
        {
        }

        public override void OrderByDescending()
        {
        }

        public override void OrderByDescending_ThenBy()
        {
        }

        public override void OrderByDescending_ThenByDescending()
        {
        }

        public override void Select_nested_collection()
        {
        }

        public override void Select_nested_collection_deep()
        {
        }

        public override void Select_nested_collection_in_anonymous_type()
        {
        }

        public override void SelectMany_cartesian_product_with_ordering()
        {
        }

        #endregion

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

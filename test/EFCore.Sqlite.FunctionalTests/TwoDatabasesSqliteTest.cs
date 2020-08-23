// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class TwoDatabasesSqliteTest : TwoDatabasesTestBase, IClassFixture<TwoDatabasesSqliteTest.TwoDatabasesFixture>
    {
        public TwoDatabasesSqliteTest(TwoDatabasesFixture fixture)
            : base(fixture)
        {
        }

        protected new TwoDatabasesFixture Fixture
            => (TwoDatabasesFixture)base.Fixture;

        protected override DbContextOptionsBuilder CreateTestOptions(
            DbContextOptionsBuilder optionsBuilder,
            bool withConnectionString = false)
            => withConnectionString
                ? optionsBuilder.UseSqlite(DummyConnectionString)
                : optionsBuilder.UseSqlite();

        protected override TwoDatabasesWithDataContext CreateBackingContext(string databaseName)
            => new TwoDatabasesWithDataContext(Fixture.CreateOptions(SqliteTestStore.Create(databaseName)));

        protected override string DummyConnectionString { get; } = "DataSource=DummyDatabase";

        public class TwoDatabasesFixture : ServiceProviderFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;
        }
    }
}

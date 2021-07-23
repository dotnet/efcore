// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class TwoDatabasesSqlServerTest : TwoDatabasesTestBase, IClassFixture<SqlServerFixture>
    {
        public TwoDatabasesSqlServerTest(SqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected new SqlServerFixture Fixture
            => (SqlServerFixture)base.Fixture;

        protected override DbContextOptionsBuilder CreateTestOptions(
            DbContextOptionsBuilder optionsBuilder,
            bool withConnectionString = false)
            => withConnectionString
                ? optionsBuilder.UseSqlServer(DummyConnectionString)
                : optionsBuilder.UseSqlServer();

        protected override TwoDatabasesWithDataContext CreateBackingContext(string databaseName)
            => new(Fixture.CreateOptions(SqlServerTestStore.Create(databaseName)));

        protected override string DummyConnectionString { get; } = "Database=DoesNotExist";
    }
}

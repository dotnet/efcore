// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            => new TwoDatabasesWithDataContext(Fixture.CreateOptions(SqlServerTestStore.Create(databaseName)));

        protected override string DummyConnectionString { get; } = "Database=DoesNotExist";
    }
}

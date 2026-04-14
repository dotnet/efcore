// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class TwoDatabasesSqlServerTest(SqlServerFixture fixture) : TwoDatabasesTestBase(fixture), IClassFixture<SqlServerFixture>
{
    protected new SqlServerFixture Fixture
        => (SqlServerFixture)base.Fixture;

    protected override DbContextOptionsBuilder CreateTestOptions(
        DbContextOptionsBuilder optionsBuilder,
        bool withConnectionString = false,
        bool withNullConnectionString = false)
        => withConnectionString
            ? withNullConnectionString
                ? optionsBuilder.UseSqlServer((string)null)
                : optionsBuilder.UseSqlServer(DummyConnectionString)
            : optionsBuilder.UseSqlServer();

    protected override TwoDatabasesWithDataContext CreateBackingContext(string databaseName)
        => new(Fixture.CreateOptions(SqlServerTestStore.Create(databaseName)));

    protected override string DummyConnectionString
        => "Database=DoesNotExist";
}

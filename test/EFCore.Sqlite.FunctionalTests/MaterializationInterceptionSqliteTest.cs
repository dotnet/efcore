// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore;

public class MaterializationInterceptionSqliteTest :
    MaterializationInterceptionTestBase<MaterializationInterceptionSqliteTest.SqliteLibraryContext>
{
    public override async Task Intercept_query_materialization_with_owned_types_projecting_collection(bool async, bool usePooling)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Intercept_query_materialization_with_owned_types_projecting_collection(async, usePooling)))
            .Message);

    public class SqliteLibraryContext(DbContextOptions options) : LibraryContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity30244>().OwnsMany(e => e.Settings, b => b.ToJson());
        }
    }

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;
}

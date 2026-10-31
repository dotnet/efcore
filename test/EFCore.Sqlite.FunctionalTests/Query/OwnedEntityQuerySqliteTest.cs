// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedEntityQuerySqliteTest(NonSharedFixture fixture) : OwnedEntityQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
        => base.AddNonSharedOptions(builder.ConfigureWarnings(b => b.Ignore(SqliteEventId.CompositeKeyWithValueGeneration)));

    public override async Task Owned_collection_select_many_without_result_selector(bool async)
    => Assert.Equal(
        SqliteStrings.ApplyNotSupported,
        (await Assert.ThrowsAsync<InvalidOperationException>(()
            => base.Owned_collection_select_many_without_result_selector(async))).Message);

    public override async Task Owned_collection_select_many_without_result_selector_owned_in_separate_table(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Owned_collection_select_many_without_result_selector_owned_in_separate_table(async))).Message);
}

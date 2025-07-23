// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public class OwnedNavigationsCollectionSqliteTest(OwnedNavigationsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedNavigationsCollectionRelationalTestBase<OwnedNavigationsSqliteFixture>(fixture, testOutputHelper)
{
    // TODO: #36296
    public override Task Index_column()
        => Assert.ThrowsAsync<SqliteException>(() => base.Index_column());
}

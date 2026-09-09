// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocNavigationsQuerySqliteTest : AdHocNavigationsQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override async Task Projection_with_multiple_includes_and_subquery_with_set_operation()
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Projection_with_multiple_includes_and_subquery_with_set_operation())).Message);

        AssertSql();
    }

    public override async Task Let_multiple_references_with_reference_to_outer()
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Let_multiple_references_with_reference_to_outer())).Message);

        AssertSql();
    }

    public override async Task SelectMany_and_collection_in_projection_in_FirstOrDefault()
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_and_collection_in_projection_in_FirstOrDefault())).Message);

        AssertSql();
    }
}

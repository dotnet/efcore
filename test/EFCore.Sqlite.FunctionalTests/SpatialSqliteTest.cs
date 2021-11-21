// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

[SpatialiteRequired]
public class SpatialSqliteTest : SpatialTestBase<SpatialSqliteFixture>
{
    public SpatialSqliteTest(SpatialSqliteFixture fixture)
        : base(fixture)
    {
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}

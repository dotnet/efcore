// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

[ConditionalClass(typeof(SqlServerTestEnvironment), nameof(SqlServerTestEnvironment.IsSqlClrSupported))]
public class SpatialSqlServerTest(SpatialSqlServerFixture fixture) : SpatialTestBase<SpatialSqlServerFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class TPTInheritanceQuerySqliteTest : TPTInheritanceQueryTestBase<TPTInheritanceQuerySqliteFixture>
{
    public TPTInheritanceQuerySqliteTest(TPTInheritanceQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class IncompleteMappingInheritanceQuerySqliteTest(
    IncompleteMappingInheritanceQuerySqliteFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPHInheritanceQueryTestBase<IncompleteMappingInheritanceQuerySqliteFixture>(fixture, testOutputHelper)
{
    public override Task Can_insert_update_delete()
        // Test from InheritanceSqliteTest causes transaction failure. We only need to test it once.
        => Task.CompletedTask;
}

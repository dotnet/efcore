// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class GearsOfWarFromSqlQuerySqlServerTest : GearsOfWarFromSqlQueryTestBase<GearsOfWarQuerySqlServerFixture>
{
    public GearsOfWarFromSqlQuerySqlServerTest(GearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public override void From_sql_queryable_simple_columns_out_of_order()
    {
        base.From_sql_queryable_simple_columns_out_of_order();

        Assert.Equal(
            """
SELECT "Id", "Name", "IsAutomatic", "AmmunitionType", "OwnerFullName", "SynergyWithId" FROM "Weapons" ORDER BY "Name"
""",
            Sql);
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private string Sql
        => Fixture.TestSqlLoggerFactory.Sql;
}

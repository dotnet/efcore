// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query;

public class RawSqlQuerySqlServerTests : RawSqlQueryTests
{
    protected override string StoredProcedureCreationScript
        => @"CREATE PROCEDURE dbo.SearchProducts
                    @minPrice decimal(18, 2),
                    @maxPrice decimal(18, 2)
                AS
                BEGIN
                    SELECT * FROM dbo.Products WHERE CurrentPrice >= @minPrice AND CurrentPrice <= @maxPrice
                END";

    protected override OrdersFixtureBase CreateFixture()
        => new OrdersSqlServerFixture("Perf_Query_RawSql");
}

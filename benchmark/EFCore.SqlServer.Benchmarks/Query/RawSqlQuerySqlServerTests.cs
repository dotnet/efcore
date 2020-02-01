// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
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
        {
            return new OrdersSqlServerFixture("Perf_Query_RawSql");
        }
    }
}

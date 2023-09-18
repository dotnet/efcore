// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query;

public class RawSqlQuerySqliteTests : RawSqlQueryTests
{
    protected override string StoredProcedureCreationScript
        => @"";

    // TODO: Define stored procedure creation script
    public override Task StoredProcedure()
        => base.StoredProcedure();

    protected override OrdersFixtureBase CreateFixture()
        => new OrdersSqliteFixture("Perf_Query_RawSql");
}

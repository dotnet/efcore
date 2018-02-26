// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    public class RawSqlQuerySqliteTests : RawSqlQueryTests
    {
        protected override string StoredProcedureCreationScript
            => @"";

        // TODO: Define stored procedure creation script
        public override Task StoredProcedure()
        {
            return base.StoredProcedure();
        }

        protected override OrdersFixtureBase CreateFixture()
        {
            return new OrdersFixture("Perf_Query_RawSql");
        }
    }
}

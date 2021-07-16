// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    public class QueryCompilationSqliteTests : QueryCompilationTests
    {
        public override IServiceCollection AddProviderServices(IServiceCollection services)
        {
            return services.AddEntityFrameworkSqlite();
        }

        public override OrdersFixtureBase CreateFixture()
        {
            return new OrdersSqliteFixture("Perf_Query_Compilation");
        }
    }
}

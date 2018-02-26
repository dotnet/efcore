// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    public class QueryCompilationSqlServerTests : QueryCompilationTests
    {
        public override IServiceCollection AddProviderServices(IServiceCollection services)
        {
            return services.AddEntityFrameworkSqlServer();
        }

        public override OrdersFixtureBase CreateFixture()
        {
            return new OrdersFixture("Perf_Query_Compilation");
        }
    }
}

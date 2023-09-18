// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

#pragma warning disable CA1034 // Nested types should not be visible

namespace Microsoft.EntityFrameworkCore.Benchmarks.UpdatePipeline;

public class SimpleUpdatePipelineSqlServerTests : SimpleUpdatePipelineTests
{
    public class Insert : InsertBase
    {
        public override OrdersFixtureBase CreateFixture()
            => new OrdersSqlServerFixture("Perf_UpdatePipeline_Simple");
    }

    public class Update : UpdateBase
    {
        public override OrdersFixtureBase CreateFixture()
            => new OrdersSqlServerFixture("Perf_UpdatePipeline_Simple");
    }

    public class Delete : DeleteBase
    {
        public override OrdersFixtureBase CreateFixture()
            => new OrdersSqlServerFixture("Perf_UpdatePipeline_Simple");
    }

    public class Mixed : MixedBase
    {
        public override OrdersFixtureBase CreateFixture()
            => new OrdersSqlServerFixture("Perf_UpdatePipeline_Simple");
    }
}

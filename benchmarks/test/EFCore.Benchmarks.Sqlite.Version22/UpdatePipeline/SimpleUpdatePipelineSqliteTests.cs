// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

#pragma warning disable CA1034 // Nested types should not be visible

namespace Microsoft.EntityFrameworkCore.Benchmarks.UpdatePipeline
{
    public class SimpleUpdatePipelineSqliteTests : SimpleUpdatePipelineTests
    {
        public class Insert : InsertBase
        {
            public override OrdersFixtureBase CreateFixture()
            {
                return new OrdersFixture("Perf_UpdatePipeline_Simple");
            }
        }

        public class Update : UpdateBase
        {
            public override OrdersFixtureBase CreateFixture()
            {
                return new OrdersFixture("Perf_UpdatePipeline_Simple");
            }
        }

        public class Delete : DeleteBase
        {
            public override OrdersFixtureBase CreateFixture()
            {
                return new OrdersFixture("Perf_UpdatePipeline_Simple");
            }
        }

        public class Mixed : MixedBase
        {
            public override OrdersFixtureBase CreateFixture()
            {
                return new OrdersFixture("Perf_UpdatePipeline_Simple");
            }
        }
    }
}

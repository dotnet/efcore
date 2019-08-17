// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class ConcurrencyDetectorCosmosTest : ConcurrencyDetectorTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public ConcurrencyDetectorCosmosTest(NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Any_logs_concurrent_access_nonasync()
        {
            return base.Any_logs_concurrent_access_nonasync();
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Any_logs_concurrent_access_async()
        {
            return base.Any_logs_concurrent_access_async();
        }
    }
}

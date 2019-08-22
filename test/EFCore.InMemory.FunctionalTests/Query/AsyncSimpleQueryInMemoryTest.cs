// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncSimpleQueryInMemoryTest : AsyncSimpleQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public AsyncSimpleQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue #16963 Set Operation")]
        public override Task Concat_dbset()
        {
            return Task.CompletedTask;
        }

        [ConditionalFact(Skip = "Issue #16963 Set Operation")]
        public override Task Concat_simple()
        {
            return Task.CompletedTask;
        }

        [ConditionalFact(Skip = "Issue #16963 Set Operation")]
        public override Task Concat_non_entity()
        {
            return Task.CompletedTask;
        }

        [ConditionalFact(Skip = "Issue #16963 Set Operation")]
        public override Task Except_non_entity()
        {
            return Task.CompletedTask;
        }

        [ConditionalFact(Skip = "Issue #16963 Set Operation")]
        public override Task Intersect_non_entity()
        {
            return Task.CompletedTask;
        }

        [ConditionalFact(Skip = "Issue #16963 Set Operation")]
        public override Task Union_non_entity()
        {
            return Task.CompletedTask;
        }
    }
}

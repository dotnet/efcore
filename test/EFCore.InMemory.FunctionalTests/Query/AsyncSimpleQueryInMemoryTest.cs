// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        // InMemory can throw server side exception
        public override Task Average_on_nav_subquery_in_projection()
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Average_on_nav_subquery_in_projection());
        }
    }
}

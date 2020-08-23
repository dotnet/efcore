// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindKeylessEntitiesQueryInMemoryTest : NorthwindKeylessEntitiesQueryTestBase<
        NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public NorthwindKeylessEntitiesQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
            ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        // mapping to view not supported on InMemory
        public override void KeylessEntity_by_database_view()
        {
        }

        public override void Entity_mapped_to_view_on_right_side_of_join()
        {
        }

        public override async Task KeylessEntity_with_included_nav(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.KeylessEntity_with_included_nav(async));
        }
    }
}

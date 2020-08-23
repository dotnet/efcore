// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindChangeTrackingQueryInMemoryTest : NorthwindChangeTrackingQueryTestBase<
        NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public NorthwindChangeTrackingQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        protected override NorthwindContext CreateNoTrackingContext()
            => new NorthwindInMemoryContext(
                new DbContextOptionsBuilder(Fixture.CreateOptions())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);
    }
}

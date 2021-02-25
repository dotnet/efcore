// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSelectQueryInMemoryTest : NorthwindSelectQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public NorthwindSelectQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
            ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "Issue#17536")]
        public override Task SelectMany_correlated_with_outer_3(bool async)
        {
            return base.SelectMany_correlated_with_outer_3(async);
        }
    }
}

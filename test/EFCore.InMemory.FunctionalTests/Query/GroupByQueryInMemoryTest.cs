// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GroupByQueryInMemoryTest : GroupByQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public GroupByQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
            // ReSharper disable once UnusedParameter.Local
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalFact(Skip = "See issue #9591")]
        public override void Select_Distinct_GroupBy()
        {
            base.Select_Distinct_GroupBy();
        }
    }
}

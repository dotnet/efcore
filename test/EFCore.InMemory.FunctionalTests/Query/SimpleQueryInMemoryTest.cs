// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable RedundantOverridenMember
// ReSharper disable ConvertMethodToExpressionBody
namespace Microsoft.EntityFrameworkCore.Query
{
    public class SimpleQueryInMemoryTest : SimpleQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public SimpleQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [Fact(Skip = "See issue #9591")]
        public override void Select_Distinct_GroupBy()
        {
            base.Select_Distinct_GroupBy();
        }
    }
}
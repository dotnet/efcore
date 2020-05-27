// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindStringIncludeQueryInMemoryTest : NorthwindStringIncludeQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public NorthwindStringIncludeQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Include_collection_with_last_no_orderby(bool async)
        {
            return base.Include_collection_with_last_no_orderby(async);
        }
    }
}

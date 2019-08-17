// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeInMemoryTest : IncludeTestBase<IncludeInMemoryFixture>
    {
        public IncludeInMemoryTest(IncludeInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override void Include_collection_with_client_filter(bool useString)
        {
        }
    }
}

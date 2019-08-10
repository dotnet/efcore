// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncGearsOfWarQueryInMemoryTest : AsyncGearsOfWarQueryTestBase<GearsOfWarQueryInMemoryFixture>
    {
        public AsyncGearsOfWarQueryInMemoryTest(GearsOfWarQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue#16963")]
        public override Task GroupBy_Select_sum()
        {
            return base.GroupBy_Select_sum();
        }
    }
}

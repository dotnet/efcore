// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    [MonoVersionCondition(Min = "4.2.0", SkipReason = "Async queries will not work on Mono < 4.2.0 due to differences in the IQueryable interface")]
    public class AsyncQueryInMemoryTest : AsyncQueryTestBase<NorthwindQueryInMemoryFixture>
    {
        public AsyncQueryInMemoryTest(NorthwindQueryInMemoryFixture fixture)
            : base(fixture)
        {
        }
    }
}

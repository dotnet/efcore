// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit.Abstractions;

// ReSharper disable RedundantOverridenMember
// ReSharper disable ConvertMethodToExpressionBody

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class QueryInMemoryTest : QueryTestBase<NorthwindQueryInMemoryFixture>
    {
        public QueryInMemoryTest(NorthwindQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }
    }
}

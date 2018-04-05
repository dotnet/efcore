// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryFilterFuncletizationInMemoryTest
        : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationInMemoryTest.QueryFilterFuncletizationInMemoryFixture>
    {
        public QueryFilterFuncletizationInMemoryTest(
            QueryFilterFuncletizationInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        public class QueryFilterFuncletizationInMemoryFixture : QueryFilterFuncletizationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;
        }
    }
}

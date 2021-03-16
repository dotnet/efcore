// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class ConcurrencyDetectorEnabledCosmosTest : ConcurrencyDetectorEnabledTestBase<
        ConcurrencyDetectorEnabledCosmosTest.ConcurrencyDetectorCosmosFixture>
    {
        public ConcurrencyDetectorEnabledCosmosTest(ConcurrencyDetectorCosmosFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Any(bool async)
            => base.Any(async);

        public class ConcurrencyDetectorCosmosFixture : ConcurrencyDetectorFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => CosmosTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;
        }
    }
}

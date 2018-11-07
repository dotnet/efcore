// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeOneToOneOracleTest : IncludeOneToOneTestBase<IncludeOneToOneOracleTest.OneToOneQueryOracleFixture>
    {
        public IncludeOneToOneOracleTest(OneToOneQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        public class OneToOneQueryOracleFixture : OneToOneQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
        }
    }
}

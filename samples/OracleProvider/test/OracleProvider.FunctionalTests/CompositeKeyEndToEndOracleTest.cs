// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class CompositeKeyEndToEndOracleTest : CompositeKeyEndToEndTestBase<CompositeKeyEndToEndOracleTest.CompositeKeyEndToEndOracleFixture>
    {
        public CompositeKeyEndToEndOracleTest(CompositeKeyEndToEndOracleFixture fixture)
            : base(fixture)
        {
        }

        public class CompositeKeyEndToEndOracleFixture : CompositeKeyEndToEndFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;
        }
    }
}

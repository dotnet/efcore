// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class TableSplittingOracleTest : TableSplittingTestBase
    {
        public TableSplittingOracleTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact(Skip = "Issue: 11910")]
        public override void Can_use_with_redundant_relationships()
        {
            base.Can_use_with_redundant_relationships();
        }

        protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;
    }
}

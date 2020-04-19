// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindIncludeQuerySqliteTest : NorthwindIncludeQueryTestBase<IncludeSqliteFixture>
    {
        public NorthwindIncludeQuerySqliteTest(IncludeSqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        [SqliteVersionCondition(Min = "3.8.8", SkipReason = "Distinct & Order By gives incorrect result in older version of Sqlite.")]
        public override void Include_collection_order_by_non_key_with_take(bool useString)
        {
            base.Include_collection_order_by_non_key_with_take(useString);
        }

        // Sqlite does not support Apply operations
        public override void Include_collection_with_cross_apply_with_filter(bool useString)
        {
        }

        public override void Include_collection_with_outer_apply_with_filter(bool useString)
        {
        }
    }
}

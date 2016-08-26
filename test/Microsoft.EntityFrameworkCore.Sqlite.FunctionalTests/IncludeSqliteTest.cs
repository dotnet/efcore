// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests.Utilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class IncludeSqliteTest : IncludeTestBase<NorthwindQuerySqliteFixture>
    {
        public IncludeSqliteTest(NorthwindQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        [SqliteVersionCondition(Min = "3.8.8", SkipReason = "Distinct & Order By gives incorrect result in older version of Sqlite.")]
        public override void Include_collection_order_by_non_key_with_take(bool useString)
        {
            base.Include_collection_order_by_non_key_with_take(useString);
        }
    }
}

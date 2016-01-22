// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit.Abstractions;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class IncludeSqliteTest : IncludeTestBase<NorthwindQuerySqliteFixture>
    {
        public IncludeSqliteTest(NorthwindQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }
    }
}

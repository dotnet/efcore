// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class IncludeAsyncSqlServerTest : IncludeAsyncTestBase<NorthwindQuerySqlServerFixture>
    {
        public IncludeAsyncSqlServerTest(NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)] // Test does not pass on SqlServer 2008. TODO: See issue#7160
        public override Task Include_duplicate_reference()
        {
            return base.Include_duplicate_reference();
        }
    }
}

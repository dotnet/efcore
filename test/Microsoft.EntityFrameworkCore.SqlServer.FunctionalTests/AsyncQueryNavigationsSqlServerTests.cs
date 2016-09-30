// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class AsyncQueryNavigationsSqlServerTests : AsyncQueryNavigationsTestBase<NorthwindQuerySqlServerFixture>
    {
        public AsyncQueryNavigationsSqlServerTests(NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            // TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }
    }
}

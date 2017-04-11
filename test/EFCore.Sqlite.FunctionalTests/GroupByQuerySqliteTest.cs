// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;
using Xunit.Abstractions;

#if !NET452
using System.Threading;
#endif
namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class GroupByQuerySqliteTest : GroupByQueryTestBase<NorthwindQuerySqliteFixture>
    {
        public GroupByQuerySqliteTest(NorthwindQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }
    }
}

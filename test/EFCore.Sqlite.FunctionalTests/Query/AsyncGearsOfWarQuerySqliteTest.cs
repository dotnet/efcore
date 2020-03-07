// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncGearsOfWarQuerySqliteTest : AsyncGearsOfWarQueryTestBase<GearsOfWarQuerySqliteFixture>
    {
        public AsyncGearsOfWarQuerySqliteTest(GearsOfWarQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }
    }
}

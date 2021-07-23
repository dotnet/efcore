﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSplitIncludeNoTrackingQuerySqlServerTest : NorthwindSplitIncludeNoTrackingQueryTestBase<
        NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public NorthwindSplitIncludeNoTrackingQuerySqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}

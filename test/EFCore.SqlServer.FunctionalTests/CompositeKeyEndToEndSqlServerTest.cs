﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class CompositeKeyEndToEndSqlServerTest : CompositeKeyEndToEndTestBase<
        CompositeKeyEndToEndSqlServerTest.CompositeKeyEndToEndSqlServerFixture>
    {
        public CompositeKeyEndToEndSqlServerTest(CompositeKeyEndToEndSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class CompositeKeyEndToEndSqlServerFixture : CompositeKeyEndToEndFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;
        }
    }
}

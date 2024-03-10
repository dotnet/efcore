// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class CompositeKeyEndToEndSqlServerTest(CompositeKeyEndToEndSqlServerTest.CompositeKeyEndToEndSqlServerFixture fixture) : CompositeKeyEndToEndTestBase<
    CompositeKeyEndToEndSqlServerTest.CompositeKeyEndToEndSqlServerFixture>(fixture)
{
    public class CompositeKeyEndToEndSqlServerFixture : CompositeKeyEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}

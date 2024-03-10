// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class StoreGeneratedSqlServerTest(StoreGeneratedSqlServerTest.StoreGeneratedSqlServerFixture fixture) : StoreGeneratedSqlServerTestBase<StoreGeneratedSqlServerTest.StoreGeneratedSqlServerFixture>(fixture)
{
    public class StoreGeneratedSqlServerFixture : StoreGeneratedSqlServerFixtureBase
    {
        protected override string StoreName
            => "StoreGeneratedTest";
    }
}

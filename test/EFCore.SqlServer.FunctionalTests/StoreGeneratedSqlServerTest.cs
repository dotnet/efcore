// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class StoreGeneratedSqlServerTest : StoreGeneratedSqlServerTestBase<StoreGeneratedSqlServerTest.StoreGeneratedSqlServerFixture>
{
    public StoreGeneratedSqlServerTest(StoreGeneratedSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class StoreGeneratedSqlServerFixture : StoreGeneratedSqlServerFixtureBase
    {
        protected override string StoreName
            => "StoreGeneratedTest";
    }
}

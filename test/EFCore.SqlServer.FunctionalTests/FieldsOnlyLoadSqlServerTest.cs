// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class FieldsOnlyLoadSqlServerTest(FieldsOnlyLoadSqlServerTest.FieldsOnlyLoadSqlServerFixture fixture) : FieldsOnlyLoadTestBase<FieldsOnlyLoadSqlServerTest.FieldsOnlyLoadSqlServerFixture>(fixture)
{
    public class FieldsOnlyLoadSqlServerFixture : FieldsOnlyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}

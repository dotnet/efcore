// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class FieldsOnlyLoadSqlServerTest : FieldsOnlyLoadTestBase<FieldsOnlyLoadSqlServerTest.FieldsOnlyLoadSqlServerFixture>
{
    public FieldsOnlyLoadSqlServerTest(FieldsOnlyLoadSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class FieldsOnlyLoadSqlServerFixture : FieldsOnlyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}

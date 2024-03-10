// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class FieldsOnlyLoadSqliteTest(FieldsOnlyLoadSqliteTest.FieldsOnlyLoadSqliteFixture fixture) : FieldsOnlyLoadTestBase<FieldsOnlyLoadSqliteTest.FieldsOnlyLoadSqliteFixture>(fixture)
{
    public class FieldsOnlyLoadSqliteFixture : FieldsOnlyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}

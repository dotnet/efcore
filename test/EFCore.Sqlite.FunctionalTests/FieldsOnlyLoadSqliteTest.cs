// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class FieldsOnlyLoadSqliteTest : FieldsOnlyLoadTestBase<FieldsOnlyLoadSqliteTest.FieldsOnlyLoadSqliteFixture>
{
    public FieldsOnlyLoadSqliteTest(FieldsOnlyLoadSqliteFixture fixture)
        : base(fixture)
    {
    }

    public class FieldsOnlyLoadSqliteFixture : FieldsOnlyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}

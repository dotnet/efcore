// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ComplexTypesTrackingSqliteTest : ComplexTypesTrackingRelationalTestBase<ComplexTypesTrackingSqliteTest.SqliteFixture>
{
    public ComplexTypesTrackingSqliteTest(SqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    public class SqliteFixture : RelationalFixtureBase, ITestSqlLoggerFactory
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}

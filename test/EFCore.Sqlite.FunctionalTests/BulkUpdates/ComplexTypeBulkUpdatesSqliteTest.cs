// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class ComplexTypeBulkUpdatesSqliteTest : ComplexTypeBulkUpdatesTestBase<
    ComplexTypeBulkUpdatesSqliteTest.ComplexTypeBulkUpdatesSqliteFixture>
{
    public ComplexTypeBulkUpdatesSqliteTest(ComplexTypeBulkUpdatesSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    public class ComplexTypeBulkUpdatesSqliteFixture : ComplexTypeBulkUpdatesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}

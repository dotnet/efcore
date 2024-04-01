// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public class ComplexTypeBulkUpdatesSqliteTest(ComplexTypeBulkUpdatesSqliteTest.ComplexTypeBulkUpdatesSqliteFixture fixture, ITestOutputHelper testOutputHelper) : ComplexTypeBulkUpdatesTestBase<
    ComplexTypeBulkUpdatesSqliteTest.ComplexTypeBulkUpdatesSqliteFixture>(fixture, testOutputHelper)
{
    public class ComplexTypeBulkUpdatesSqliteFixture : ComplexTypeBulkUpdatesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class CompositeKeyEndToEndSqliteTest : CompositeKeyEndToEndTestBase<
    CompositeKeyEndToEndSqliteTest.CompositeKeyEndToEndSqliteFixture>
{
    public CompositeKeyEndToEndSqliteTest(CompositeKeyEndToEndSqliteFixture fixture)
        : base(fixture)
    {
    }

    public override Task Can_use_generated_values_in_composite_key_end_to_end()
        // Not supported on Sqlite
        => Task.CompletedTask;

    public class CompositeKeyEndToEndSqliteFixture : CompositeKeyEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class CompositeKeyEndToEndSqliteTest : CompositeKeyEndToEndTestBase<
        CompositeKeyEndToEndSqliteTest.CompositeKeyEndToEndSqliteFixture>
    {
        public CompositeKeyEndToEndSqliteTest(CompositeKeyEndToEndSqliteFixture fixture)
            : base(fixture)
        {
        }

        public override Task Can_use_generated_values_in_composite_key_end_to_end()
        {
            // Not supported on Sqlite
            return Task.CompletedTask;
        }

        public class CompositeKeyEndToEndSqliteFixture : CompositeKeyEndToEndFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;
        }
    }
}

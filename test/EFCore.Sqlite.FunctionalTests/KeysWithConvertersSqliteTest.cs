// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class KeysWithConvertersSqliteTest : KeysWithConvertersTestBase<KeysWithConvertersSqliteTest.KeysWithConvertersSqliteFixture>
    {
        public KeysWithConvertersSqliteTest(KeysWithConvertersSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class KeysWithConvertersSqliteFixture : KeysWithConvertersFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => builder.UseSqlite(b => b.MinBatchSize(1));
        }
    }
}

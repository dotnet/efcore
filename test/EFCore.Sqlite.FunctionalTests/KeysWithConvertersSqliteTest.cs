// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

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

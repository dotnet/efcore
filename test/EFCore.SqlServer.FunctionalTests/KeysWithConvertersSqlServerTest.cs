// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class KeysWithConvertersSqlServerTest : KeysWithConvertersTestBase<
    KeysWithConvertersSqlServerTest.KeysWithConvertersSqlServerFixture>
{
    public KeysWithConvertersSqlServerTest(KeysWithConvertersSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class KeysWithConvertersSqlServerFixture : KeysWithConvertersFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder.UseSqlServer(b => b.MinBatchSize(1));
    }
}

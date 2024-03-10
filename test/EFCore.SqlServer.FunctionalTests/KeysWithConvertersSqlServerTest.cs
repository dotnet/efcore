// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class KeysWithConvertersSqlServerTest(KeysWithConvertersSqlServerTest.KeysWithConvertersSqlServerFixture fixture) : KeysWithConvertersTestBase<
    KeysWithConvertersSqlServerTest.KeysWithConvertersSqlServerFixture>(fixture)
{
    public class KeysWithConvertersSqlServerFixture : KeysWithConvertersFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder.UseSqlServer(b => b.MinBatchSize(1));
    }
}

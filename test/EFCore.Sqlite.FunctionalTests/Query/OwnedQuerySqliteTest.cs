// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class OwnedQuerySqliteTest(OwnedQuerySqliteTest.OwnedQuerySqliteFixture fixture) : OwnedQueryRelationalTestBase<OwnedQuerySqliteTest.OwnedQuerySqliteFixture>(fixture)
{
    public class OwnedQuerySqliteFixture : RelationalOwnedQueryFixture
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder.ConfigureWarnings(b => b.Ignore(SqliteEventId.CompositeKeyWithValueGeneration)));

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}

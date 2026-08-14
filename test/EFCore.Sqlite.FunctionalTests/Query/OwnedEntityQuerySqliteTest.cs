// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class OwnedEntityQuerySqliteTest : OwnedEntityQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder.ConfigureWarnings(b => b.Ignore(SqliteEventId.CompositeKeyWithValueGeneration)));
}

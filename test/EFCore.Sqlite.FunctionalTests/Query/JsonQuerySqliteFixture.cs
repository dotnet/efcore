// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonQuerySqliteFixture : JsonQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;


    // issue #26708
    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder.ConfigureWarnings(b => b.Ignore(SqliteEventId.CompositeKeyWithValueGeneration)));
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

public class SqliteApiConsistencyTest : ApiConsistencyTestBase<SqliteApiConsistencyTest.SqliteApiConsistencyFixture>
{
    public SqliteApiConsistencyTest(SqliteApiConsistencyFixture fixture)
        : base(fixture)
    {
    }

    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkSqlite();

    protected override Assembly TargetAssembly
        => typeof(SqliteRelationalConnection).Assembly;

    public class SqliteApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } = new()
        {
            typeof(SqliteServiceCollectionExtensions),
            typeof(SqliteDbContextOptionsBuilderExtensions),
            typeof(SqliteDbContextOptionsBuilder),
            typeof(SqlitePropertyBuilderExtensions)
        };
    }
}

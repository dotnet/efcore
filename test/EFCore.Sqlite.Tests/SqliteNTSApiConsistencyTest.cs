// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class SqliteNTSApiConsistencyTest(SqliteNTSApiConsistencyTest.SqliteNTSApiConsistencyFixture fixture) : ApiConsistencyTestBase<SqliteNTSApiConsistencyTest.SqliteNTSApiConsistencyFixture>(fixture)
{
    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkSqliteNetTopologySuite();

    protected override Assembly TargetAssembly
        => typeof(SqliteNetTopologySuiteServiceCollectionExtensions).Assembly;

    public class SqliteNTSApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } =
            [typeof(SqliteNetTopologySuiteDbContextOptionsBuilderExtensions), typeof(SqliteNetTopologySuiteServiceCollectionExtensions)];
    }
}

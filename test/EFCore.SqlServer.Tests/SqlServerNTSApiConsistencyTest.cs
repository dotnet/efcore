// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class SqlServerNTSApiConsistencyTest : ApiConsistencyTestBase<SqlServerNTSApiConsistencyTest.SqlServerNTSApiConsistencyFixture>
{
    public SqlServerNTSApiConsistencyTest(SqlServerNTSApiConsistencyFixture fixture)
        : base(fixture)
    {
    }

    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkSqlServerNetTopologySuite();

    protected override Assembly TargetAssembly
        => typeof(SqlServerNetTopologySuiteServiceCollectionExtensions).Assembly;

    public class SqlServerNTSApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } = new()
        {
            typeof(SqlServerNetTopologySuiteDbContextOptionsBuilderExtensions),
            typeof(SqlServerNetTopologySuiteServiceCollectionExtensions)
        };
    }
}

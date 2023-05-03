// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore;

public class SqlServerHierarchyIdApiConsistencyTest : ApiConsistencyTestBase<SqlServerHierarchyIdApiConsistencyTest.SqlServerHierarchyIdApiConsistencyFixture>
{
    public SqlServerHierarchyIdApiConsistencyTest(SqlServerHierarchyIdApiConsistencyFixture fixture)
        : base(fixture)
    {
    }

    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkSqlServerHierarchyId();

    protected override Assembly TargetAssembly
        => typeof(SqlServerHierarchyIdServiceCollectionExtensions).Assembly;

    public class SqlServerHierarchyIdApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } = new()
        {
            typeof(SqlServerHierarchyIdDbContextOptionsBuilderExtensions),
            typeof(SqlServerHierarchyIdServiceCollectionExtensions)
        };
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore;

public class SqlServerHierarchyIdApiConsistencyTest(SqlServerHierarchyIdApiConsistencyTest.SqlServerHierarchyIdApiConsistencyFixture fixture) : ApiConsistencyTestBase<
    SqlServerHierarchyIdApiConsistencyTest.SqlServerHierarchyIdApiConsistencyFixture>(fixture)
{
    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkSqlServerHierarchyId();

    protected override Assembly TargetAssembly
        => typeof(SqlServerHierarchyIdServiceCollectionExtensions).Assembly;

    public class SqlServerHierarchyIdApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } =
            [typeof(SqlServerHierarchyIdDbContextOptionsBuilderExtensions), typeof(SqlServerHierarchyIdServiceCollectionExtensions)];
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore;

public class SqlServerAbstractionsApiConsistencyTest(SqlServerAbstractionsApiConsistencyTest.SqlServerAbstractionsApiConsistencyFixture fixture) : ApiConsistencyTestBase<
    SqlServerAbstractionsApiConsistencyTest.SqlServerAbstractionsApiConsistencyFixture>(fixture)
{
    protected override void AddServices(ServiceCollection serviceCollection)
    {
    }

    protected override Assembly TargetAssembly
        => typeof(HierarchyId).Assembly;

    public class SqlServerAbstractionsApiConsistencyFixture : ApiConsistencyFixtureBase;
}

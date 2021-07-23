// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteNTSApiConsistencyTest : ApiConsistencyTestBase<SqliteNTSApiConsistencyTest.SqliteNTSApiConsistencyFixture>
    {
        public SqliteNTSApiConsistencyTest(SqliteNTSApiConsistencyFixture fixture)
            : base(fixture)
        {
        }

        protected override void AddServices(ServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqliteNetTopologySuite();

        protected override Assembly TargetAssembly
            => typeof(SqliteNetTopologySuiteServiceCollectionExtensions).Assembly;

        public class SqliteNTSApiConsistencyFixture : ApiConsistencyFixtureBase
        {
            public override HashSet<Type> FluentApiTypes { get; } = new()
            {
                typeof(SqliteNetTopologySuiteDbContextOptionsBuilderExtensions),
                typeof(SqliteNetTopologySuiteServiceCollectionExtensions)
            };
        }
    }
}

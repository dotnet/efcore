// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            public override bool TryGetProviderOptionsDelegate(out Action<DbContextOptionsBuilder> configureOptions)
            {
                configureOptions = b => SqliteTestHelpers.Instance.UseProviderOptions(b);

                return true;
            }

            public override HashSet<Type> FluentApiTypes { get; } = new HashSet<Type>
            {
                typeof(SqliteNetTopologySuiteDbContextOptionsBuilderExtensions),
                typeof(SqliteNetTopologySuiteServiceCollectionExtensions)
            };
        }
    }
}

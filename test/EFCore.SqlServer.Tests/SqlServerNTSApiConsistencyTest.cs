// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
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
            public override bool TryGetProviderOptionsDelegate(out Action<DbContextOptionsBuilder> configureOptions)
            {
                configureOptions = b => SqlServerTestHelpers.Instance.UseProviderOptions(b);

                return true;
            }

            public override HashSet<Type> FluentApiTypes { get; } = new HashSet<Type>
            {
                typeof(SqlServerNetTopologySuiteDbContextOptionsBuilderExtensions),
                typeof(SqlServerNetTopologySuiteServiceCollectionExtensions)
            };
        }
    }
}

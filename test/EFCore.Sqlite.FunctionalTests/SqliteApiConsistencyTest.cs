// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
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
            public override bool TryGetProviderOptionsDelegate(out Action<DbContextOptionsBuilder> configureOptions)
            {
                configureOptions = b => SqliteTestHelpers.Instance.UseProviderOptions(b);

                return true;
            }

            public override HashSet<Type> FluentApiTypes { get; } = new HashSet<Type>
            {
                typeof(SqliteServiceCollectionExtensions),
                typeof(SqliteDbContextOptionsBuilderExtensions),
                typeof(SqliteDbContextOptionsBuilder),
                typeof(SqlitePropertyBuilderExtensions)
            };
        }
    }
}

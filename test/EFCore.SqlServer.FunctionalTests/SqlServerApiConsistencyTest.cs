// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerApiConsistencyTest : ApiConsistencyTestBase<SqlServerApiConsistencyTest.SqlServerApiConsistencyFixture>
    {
        public SqlServerApiConsistencyTest(SqlServerApiConsistencyFixture fixture)
            : base(fixture)
        {
        }

        protected override void AddServices(ServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqlServer();

        protected override Assembly TargetAssembly
            => typeof(SqlServerConnection).Assembly;

        public class SqlServerApiConsistencyFixture : ApiConsistencyFixtureBase
        {
            public override bool TryGetProviderOptionsDelegate(out Action<DbContextOptionsBuilder> configureOptions)
            {
                configureOptions = b => SqlServerTestHelpers.Instance.UseProviderOptions(b);

                return true;
            }

            public override HashSet<Type> FluentApiTypes { get; } = new HashSet<Type>
            {
                typeof(SqlServerDbContextOptionsBuilder),
                typeof(SqlServerDbContextOptionsExtensions),
                typeof(SqlServerMigrationBuilderExtensions),
                typeof(SqlServerIndexBuilderExtensions),
                typeof(SqlServerKeyBuilderExtensions),
                typeof(SqlServerModelBuilderExtensions),
                typeof(SqlServerPropertyBuilderExtensions),
                typeof(SqlServerEntityTypeBuilderExtensions),
                typeof(SqlServerServiceCollectionExtensions)
            };

            public override
                List<(Type Type, Type ReadonlyExtensions, Type MutableExtensions, Type ConventionExtensions, Type
                    ConventionBuilderExtensions)> MetadataExtensionTypes { get; }
                = new List<(Type, Type, Type, Type, Type)>
                {
                    (typeof(IModel), typeof(SqlServerModelExtensions), typeof(SqlServerModelExtensions),
                        typeof(SqlServerModelExtensions), typeof(SqlServerModelBuilderExtensions)),
                    (typeof(IEntityType), typeof(SqlServerEntityTypeExtensions), typeof(SqlServerEntityTypeExtensions),
                        typeof(SqlServerEntityTypeExtensions), typeof(SqlServerEntityTypeBuilderExtensions)),
                    (typeof(IKey), typeof(SqlServerKeyExtensions), typeof(SqlServerKeyExtensions), typeof(SqlServerKeyExtensions),
                        typeof(SqlServerKeyBuilderExtensions)),
                    (typeof(IProperty), typeof(SqlServerPropertyExtensions), typeof(SqlServerPropertyExtensions),
                        typeof(SqlServerPropertyExtensions), typeof(SqlServerPropertyBuilderExtensions)),
                    (typeof(IIndex), typeof(SqlServerIndexExtensions), typeof(SqlServerIndexExtensions),
                        typeof(SqlServerIndexExtensions), typeof(SqlServerIndexBuilderExtensions))
                };
        }
    }
}

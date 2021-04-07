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
            public override HashSet<Type> FluentApiTypes { get; } = new()
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
                List<(Type Type,
                    Type ReadonlyExtensions,
                    Type MutableExtensions,
                    Type ConventionExtensions,
                    Type ConventionBuilderExtensions,
                    Type RuntimeExtensions)> MetadataExtensionTypes { get; }
                = new()
                {
                    (
                        typeof(IReadOnlyModel),
                        typeof(SqlServerModelExtensions),
                        typeof(SqlServerModelExtensions),
                        typeof(SqlServerModelExtensions),
                        typeof(SqlServerModelBuilderExtensions),
                        null
                    ),
                    (
                        typeof(IReadOnlyEntityType),
                        typeof(SqlServerEntityTypeExtensions),
                        typeof(SqlServerEntityTypeExtensions),
                        typeof(SqlServerEntityTypeExtensions),
                        typeof(SqlServerEntityTypeBuilderExtensions),
                        null
                    ),
                    (
                        typeof(IReadOnlyKey),
                        typeof(SqlServerKeyExtensions),
                        typeof(SqlServerKeyExtensions),
                        typeof(SqlServerKeyExtensions),
                        typeof(SqlServerKeyBuilderExtensions),
                        null
                    ),
                    (
                        typeof(IReadOnlyProperty),
                        typeof(SqlServerPropertyExtensions),
                        typeof(SqlServerPropertyExtensions),
                        typeof(SqlServerPropertyExtensions),
                        typeof(SqlServerPropertyBuilderExtensions),
                        null
                    ),
                    (
                        typeof(IReadOnlyIndex),
                        typeof(SqlServerIndexExtensions),
                        typeof(SqlServerIndexExtensions),
                        typeof(SqlServerIndexExtensions),
                        typeof(SqlServerIndexBuilderExtensions),
                        null
                    )
                };
        }
    }
}

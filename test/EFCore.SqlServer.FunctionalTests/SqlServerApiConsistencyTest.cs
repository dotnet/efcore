// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

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
            typeof(SqlServerServiceCollectionExtensions),
            typeof(SqlServerDbFunctionsExtensions),
            typeof(OwnedNavigationTemporalPeriodPropertyBuilder),
            typeof(OwnedNavigationTemporalTableBuilder),
            typeof(OwnedNavigationTemporalTableBuilder<,>),
            typeof(TemporalPeriodPropertyBuilder),
            typeof(TemporalTableBuilder),
            typeof(TemporalTableBuilder<>)
        };

        public override
            Dictionary<Type,
                (Type ReadonlyExtensions,
                Type MutableExtensions,
                Type ConventionExtensions,
                Type ConventionBuilderExtensions,
                Type RuntimeExtensions)> MetadataExtensionTypes { get; }
            = new()
            {
                {

                    typeof(IReadOnlyModel),
                    (
                        typeof(SqlServerModelExtensions),
                        typeof(SqlServerModelExtensions),
                        typeof(SqlServerModelExtensions),
                        typeof(SqlServerModelBuilderExtensions),
                        null
                    )
                },
                {

                    typeof(IReadOnlyEntityType),
                    (
                        typeof(SqlServerEntityTypeExtensions),
                        typeof(SqlServerEntityTypeExtensions),
                        typeof(SqlServerEntityTypeExtensions),
                        typeof(SqlServerEntityTypeBuilderExtensions),
                        null
                    )
                },
                {
                    typeof(IReadOnlyKey),
                    (
                        typeof(SqlServerKeyExtensions),
                        typeof(SqlServerKeyExtensions),
                        typeof(SqlServerKeyExtensions),
                        typeof(SqlServerKeyBuilderExtensions),
                        null
                    )
                },
                {

                    typeof(IReadOnlyProperty),
                    (
                        typeof(SqlServerPropertyExtensions),
                        typeof(SqlServerPropertyExtensions),
                        typeof(SqlServerPropertyExtensions),
                        typeof(SqlServerPropertyBuilderExtensions),
                        null
                    )
                    },
                {

                    typeof(IReadOnlyIndex),
                    (
                        typeof(SqlServerIndexExtensions),
                        typeof(SqlServerIndexExtensions),
                        typeof(SqlServerIndexExtensions),
                        typeof(SqlServerIndexBuilderExtensions),
                        null
                    )
                }
            };

        protected override void Initialize()
        {
            GenericFluentApiTypes.Add(typeof(TemporalTableBuilder), typeof(TemporalTableBuilder<>));
            GenericFluentApiTypes.Add(typeof(OwnedNavigationTemporalTableBuilder), typeof(OwnedNavigationTemporalTableBuilder<,>));

            MirrorTypes.Add(typeof(TemporalTableBuilder), typeof(OwnedNavigationTemporalTableBuilder));
            MirrorTypes.Add(typeof(TemporalTableBuilder<>), typeof(OwnedNavigationTemporalTableBuilder<,>));
            MirrorTypes.Add(typeof(TemporalPeriodPropertyBuilder), typeof(OwnedNavigationTemporalPeriodPropertyBuilder));

            base.Initialize();
        }
    }
}

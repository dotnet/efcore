// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqliteApiConsistencyTest(SqliteApiConsistencyTest.SqliteApiConsistencyFixture fixture) : ApiConsistencyTestBase<SqliteApiConsistencyTest.SqliteApiConsistencyFixture>(fixture)
{
    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkSqlite();

    protected override Assembly TargetAssembly
        => typeof(SqliteRelationalConnection).Assembly;

    public class SqliteApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } =
        [
            typeof(SqliteServiceCollectionExtensions),
            typeof(SqliteDbContextOptionsBuilderExtensions),
            typeof(SqliteDbContextOptionsBuilder),
            typeof(SqlitePropertyBuilderExtensions)
        ];

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
                    typeof(IReadOnlyProperty), (
                        typeof(SqlitePropertyExtensions),
                        typeof(SqlitePropertyExtensions),
                        typeof(SqlitePropertyExtensions),
                        typeof(SqlitePropertyBuilderExtensions),
                        null
                    )
                }
            };

        protected override void Initialize()
        {
            MirrorTypes.Add(typeof(SqlitePropertyBuilderExtensions), typeof(SqliteComplexTypePropertyBuilderExtensions));

            base.Initialize();
        }
    }
}

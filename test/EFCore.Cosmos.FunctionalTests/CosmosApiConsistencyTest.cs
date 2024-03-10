// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class CosmosApiConsistencyTest(CosmosApiConsistencyTest.CosmosApiConsistencyFixture fixture) : ApiConsistencyTestBase<CosmosApiConsistencyTest.CosmosApiConsistencyFixture>(fixture)
{
    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkCosmos();

    protected override Assembly TargetAssembly
        => typeof(CosmosDatabaseWrapper).Assembly;

    public class CosmosApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } =
        [
            typeof(CosmosPrimitiveCollectionBuilderExtensions),
            typeof(CosmosModelBuilderExtensions),
            typeof(CosmosPropertyBuilderExtensions),
            typeof(CosmosServiceCollectionExtensions),
            typeof(CosmosDbContextOptionsExtensions),
            typeof(CosmosDbContextOptionsBuilder)
        ];

        public override
            Dictionary<Type, (Type ReadonlyExtensions,
                Type MutableExtensions,
                Type ConventionExtensions,
                Type ConventionBuilderExtensions,
                Type RuntimeExtensions)> MetadataExtensionTypes
        {
            get;
        }
            = new()
            {
                {
                    typeof(IReadOnlyModel), (
                        typeof(CosmosModelExtensions),
                        typeof(CosmosModelExtensions),
                        typeof(CosmosModelExtensions),
                        typeof(CosmosModelBuilderExtensions),
                        null
                    )
                },
                {
                    typeof(IReadOnlyEntityType), (
                        typeof(CosmosEntityTypeExtensions),
                        typeof(CosmosEntityTypeExtensions),
                        typeof(CosmosEntityTypeExtensions),
                        typeof(CosmosEntityTypeBuilderExtensions),
                        null
                    )
                },
                {
                    typeof(IReadOnlyProperty), (
                        typeof(CosmosPropertyExtensions),
                        typeof(CosmosPropertyExtensions),
                        typeof(CosmosPropertyExtensions),
                        typeof(CosmosPropertyBuilderExtensions),
                        null
                    )
                }
            };
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class CosmosApiConsistencyTest : ApiConsistencyTestBase<CosmosApiConsistencyTest.CosmosApiConsistencyFixture>
    {
        public CosmosApiConsistencyTest(CosmosApiConsistencyFixture fixture)
            : base(fixture)
        {
        }

        protected override void AddServices(ServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkCosmos();

        protected override Assembly TargetAssembly
            => typeof(CosmosDatabaseWrapper).Assembly;

        public class CosmosApiConsistencyFixture : ApiConsistencyFixtureBase
        {
            public override HashSet<Type> FluentApiTypes { get; } = new()
            {
                typeof(CosmosModelBuilderExtensions),
                typeof(CosmosPropertyBuilderExtensions),
                typeof(CosmosServiceCollectionExtensions),
                typeof(CosmosDbContextOptionsExtensions),
                typeof(CosmosDbContextOptionsBuilder)
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
                        typeof(CosmosModelExtensions),
                        typeof(CosmosModelExtensions),
                        typeof(CosmosModelExtensions),
                        typeof(CosmosModelBuilderExtensions),
                        null
                    ),
                    (
                        typeof(IReadOnlyProperty),
                        typeof(CosmosPropertyExtensions),
                        typeof(CosmosPropertyExtensions),
                        typeof(CosmosPropertyExtensions),
                        typeof(CosmosPropertyBuilderExtensions),
                        null
                    )
                };
        }
    }
}

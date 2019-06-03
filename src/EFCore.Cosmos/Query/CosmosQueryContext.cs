// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query
{
    public class CosmosQueryContext : QueryContext
    {
        public CosmosQueryContext(
            [NotNull] QueryContextDependencies dependencies,
            [NotNull] CosmosClientWrapper cosmosClient)
            : base(dependencies)
        {
            CosmosClient = cosmosClient;
        }

        public CosmosClientWrapper CosmosClient { get; }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query
{
    public class CosmosQueryContext : QueryContext
    {
        public CosmosQueryContext(
            [NotNull] QueryContextDependencies dependencies,
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] CosmosClient cosmosClient)
            : base(dependencies, queryBufferFactory)
        {
            CosmosClient = cosmosClient;
        }

        public CosmosClient CosmosClient { get; }
    }
}

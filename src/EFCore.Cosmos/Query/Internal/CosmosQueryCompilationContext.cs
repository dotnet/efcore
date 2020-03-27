// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public class CosmosQueryCompilationContext : QueryCompilationContext
    {
        public virtual string PartitionKey { get; internal set; }

        public CosmosQueryCompilationContext(
            [NotNull] QueryCompilationContextDependencies dependencies, bool async)
            : base(dependencies, async)
        {

        }
    }
}

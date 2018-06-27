// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Internal
{
    public class CosmosSqlQueryContextFactory : QueryContextFactory
    {
        public CosmosSqlQueryContextFactory([NotNull] QueryContextDependencies dependencies)
               : base(dependencies)
        {
        }

        public override QueryContext Create()
        {
            return new QueryContext(Dependencies, CreateQueryBuffer);
        }
    }
}

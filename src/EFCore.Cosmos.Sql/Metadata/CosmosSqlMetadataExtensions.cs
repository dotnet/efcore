// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata
{
    public static class CosmosSqlMetadataExtensions
    {
        public static ICosmosSqlEntityTypeAnnotations CosmosSql(this IEntityType entityType)
            => new CosmosSqlEntityTypeAnnotations(entityType);

        public static CosmosSqlEntityTypeAnnotations CosmosSql(this IMutableEntityType entityType)
            => (CosmosSqlEntityTypeAnnotations)CosmosSql((IEntityType)entityType);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata.Internal
{
    public static class EntityTypeExtensions
    {
        public static bool IsDocumentRoot(this IEntityType entityType)
            => !entityType.IsOwned();
    }
}

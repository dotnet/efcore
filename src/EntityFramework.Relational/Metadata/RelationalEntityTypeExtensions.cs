// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public static class RelationalEntityTypeExtensions
    {
        public static IEnumerable<IForeignKey> GetForeignKeysInHierarchy([NotNull] this IEntityType entityType)
            => entityType.GetDeclaredForeignKeys().Concat(entityType.GetDerivedTypes().SelectMany(t => t.GetDeclaredForeignKeys()));

        public static IEnumerable<IIndex> GetIndexesInHierarchy([NotNull] this IEntityType entityType)
            => entityType.GetDeclaredIndexes().Concat(entityType.GetDerivedTypes().SelectMany(t => t.GetDeclaredIndexes()));

        public static IEnumerable<IProperty> GetPropertiesInHierarchy([NotNull] this IEntityType entityType)
            => entityType.GetDeclaredProperties().Concat(entityType.GetDerivedTypes().SelectMany(t => t.GetDeclaredProperties()));
    }
}

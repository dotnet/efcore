// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public interface IRelationalMetadataExtensionsAccessor
    {
        ISharedRelationalEntityTypeExtensions For([NotNull] IEntityType entityType);
        ISharedRelationalForeignKeyExtensions For([NotNull] IForeignKey foreignKey);
        ISharedRelationalIndexExtensions For([NotNull] IIndex index);
        ISharedRelationalKeyExtensions For([NotNull] IKey key);
        ISharedRelationalModelExtensions For([NotNull] IModel model);
        ISharedRelationalPropertyExtensions For([NotNull] IProperty property);
    }
}

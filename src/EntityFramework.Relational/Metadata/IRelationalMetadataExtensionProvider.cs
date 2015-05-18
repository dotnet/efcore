// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public interface IRelationalMetadataExtensionProvider
    {
        IRelationalEntityTypeExtensions Extensions([NotNull] IEntityType entityType);
        IRelationalForeignKeyExtensions Extensions([NotNull] IForeignKey foreignKey);
        IRelationalIndexExtensions Extensions([NotNull] IIndex index);
        IRelationalKeyExtensions Extensions([NotNull] IKey key);
        IRelationalModelExtensions Extensions([NotNull] IModel model);
        IRelationalPropertyExtensions Extensions([NotNull] IProperty property);
    }
}

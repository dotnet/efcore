// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public interface IRelationalMetadataExtensionProvider
    {
        IRelationalModelExtensions Extensions([NotNull] IModel model);
        IRelationalEntityTypeExtensions Extensions([NotNull] IEntityType entityType);
        IRelationalPropertyExtensions Extensions([NotNull] IProperty property);
        IRelationalKeyExtensions Extensions([NotNull] IKey key);
        IRelationalForeignKeyExtensions Extensions([NotNull] IForeignKey foreignKey);
        IRelationalIndexExtensions Extensions([NotNull] IIndex index);
        RelationalNameBuilder NameBuilder { get; }
    }
}

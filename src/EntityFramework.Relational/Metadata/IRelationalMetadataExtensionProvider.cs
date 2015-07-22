// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IRelationalMetadataExtensionProvider
    {
        IRelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType);
        IRelationalForeignKeyAnnotations GetAnnotations([NotNull] IForeignKey foreignKey);
        IRelationalIndexAnnotations GetAnnotations([NotNull] IIndex index);
        IRelationalKeyAnnotations GetAnnotations([NotNull] IKey key);
        IRelationalModelAnnotations GetAnnotations([NotNull] IModel model);
        IRelationalPropertyAnnotations GetAnnotations([NotNull] IProperty property);
    }
}

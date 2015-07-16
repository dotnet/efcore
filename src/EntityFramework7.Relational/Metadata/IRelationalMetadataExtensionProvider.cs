// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IRelationalMetadataExtensionProvider
    {
        IRelationalEntityTypeAnnotations Extensions([NotNull] IEntityType entityType);
        IRelationalForeignKeyAnnotations Extensions([NotNull] IForeignKey foreignKey);
        IRelationalIndexAnnotations Extensions([NotNull] IIndex index);
        IRelationalKeyAnnotations Extensions([NotNull] IKey key);
        IRelationalModelAnnotations Extensions([NotNull] IModel model);
        IRelationalPropertyAnnotations Extensions([NotNull] IProperty property);
    }
}

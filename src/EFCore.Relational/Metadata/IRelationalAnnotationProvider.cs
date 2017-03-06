// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IRelationalAnnotationProvider
    {
        IRelationalEntityTypeAnnotations For([NotNull] IEntityType entityType);
        IRelationalForeignKeyAnnotations For([NotNull] IForeignKey foreignKey);
        IRelationalIndexAnnotations For([NotNull] IIndex index);
        IRelationalKeyAnnotations For([NotNull] IKey key);
        IRelationalModelAnnotations For([NotNull] IModel model);
        IRelationalPropertyAnnotations For([NotNull] IProperty property);
    }
}

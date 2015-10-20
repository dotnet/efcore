// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public static class ScaffoldingMetadataExtensions
    {
        public static ScaffoldingPropertyAnnotations RelationalDesign([NotNull] this IProperty property)
            => new ScaffoldingPropertyAnnotations(Check.NotNull(property, nameof(property)), ScaffoldingAnnotationNames.AnnotationPrefix);

        public static ScaffoldingForeignKeyAnnotations RelationalDesign([NotNull] this IForeignKey foreignKey)
          => new ScaffoldingForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), ScaffoldingAnnotationNames.AnnotationPrefix);

        public static ScaffoldingEntityTypeAnnotations RelationalDesign([NotNull] this IEntityType entityType)
        => new ScaffoldingEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), ScaffoldingAnnotationNames.AnnotationPrefix);
    }
}

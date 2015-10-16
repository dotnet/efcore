// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Metadata
{
    public static class RelationalDesignMetadataExtensions
    {
        public static RelationalDesignPropertyAnnotations RelationalDesign([NotNull] this IProperty property)
            => new RelationalDesignPropertyAnnotations(Check.NotNull(property, nameof(property)), RelationalDesignAnnotationNames.AnnotationPrefix);

        public static RelationalDesignForeignKeyAnnotations RelationalDesign([NotNull] this IForeignKey foreignKey)
          => new RelationalDesignForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), RelationalDesignAnnotationNames.AnnotationPrefix);

        public static RelationalDesignEntityTypeAnnotations RelationalDesign([NotNull] this IEntityType entityType)
        => new RelationalDesignEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), RelationalDesignAnnotationNames.AnnotationPrefix);
    }
}

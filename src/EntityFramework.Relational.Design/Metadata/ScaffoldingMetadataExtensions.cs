// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Microsoft.Data.Entity.Scaffolding.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity.Metadata
{
    public static class ScaffoldingMetadataExtensions
    {
        public static ScaffoldingModelAnnotations Scaffolding([NotNull] this IModel model)
            => new ScaffoldingModelAnnotations(Check.NotNull(model, nameof(model)), ScaffoldingAnnotationNames.AnnotationPrefix);

        public static ScaffoldingForeignKeyAnnotations Scaffolding([NotNull] this IForeignKey foreignKey)
            => new ScaffoldingForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), ScaffoldingAnnotationNames.AnnotationPrefix);
    }
}

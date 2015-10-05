// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalReferenceCollectionBuilderExtensions
    {
        public static ReferenceCollectionBuilder HasConstraintName(
            [NotNull] this ReferenceCollectionBuilder referenceCollectionBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceCollectionBuilder.Metadata.Relational().Name = name;

            return referenceCollectionBuilder;
        }

        public static ReferenceCollectionBuilder<TEntity, TRelatedEntity> HasConstraintName<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceCollectionBuilder<TEntity, TRelatedEntity>)HasConstraintName(
                (ReferenceCollectionBuilder)referenceCollectionBuilder, name);
    }
}

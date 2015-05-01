// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class RelationalCollectionReferenceBuilderExtensions
    {
        public static CollectionReferenceBuilder Name(
            [NotNull] this CollectionReferenceBuilder collectionReferenceBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(collectionReferenceBuilder, nameof(collectionReferenceBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            collectionReferenceBuilder.Metadata.Relational().Name = name;

            return collectionReferenceBuilder;
        }

        public static CollectionReferenceBuilder<TEntity, TRelatedEntity> Name<TEntity, TRelatedEntity>(
            [NotNull] this CollectionReferenceBuilder<TEntity, TRelatedEntity> collectionReferenceBuilder,
            [CanBeNull] string name)
            where TEntity : class
            => (CollectionReferenceBuilder<TEntity, TRelatedEntity>)Name(
                (CollectionReferenceBuilder)collectionReferenceBuilder, name);
    }
}

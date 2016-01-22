// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public static class RelationalReferenceReferenceBuilderExtensions
    {
        public static ReferenceReferenceBuilder HasConstraintName(
            [NotNull] this ReferenceReferenceBuilder referenceReferenceBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceReferenceBuilder.Metadata.Relational().Name = name;

            return referenceReferenceBuilder;
        }

        public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasConstraintName<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceReferenceBuilder<TEntity, TRelatedEntity>)HasConstraintName(
                (ReferenceReferenceBuilder)referenceReferenceBuilder, name);
    }
}

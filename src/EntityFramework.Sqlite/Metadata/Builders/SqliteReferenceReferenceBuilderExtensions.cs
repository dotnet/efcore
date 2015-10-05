// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqliteReferenceReferenceBuilderExtensions
    {
        public static ReferenceReferenceBuilder ForSqliteHasConstraintName(
            [NotNull] this ReferenceReferenceBuilder builder,
            [CanBeNull] string name)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(name, nameof(name));

            builder.Metadata.Sqlite().Name = name;

            return builder;
        }

        public static ReferenceReferenceBuilder<TEntity, TReferencedEntity> ForSqliteHasConstraintName<TEntity, TReferencedEntity>(
            [NotNull] this ReferenceReferenceBuilder<TEntity, TReferencedEntity> builder,
            [CanBeNull] string name)
            where TEntity : class
            where TReferencedEntity : class
            => (ReferenceReferenceBuilder<TEntity, TReferencedEntity>)((ReferenceReferenceBuilder)builder).ForSqliteHasConstraintName(name);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class RelationalMetadataExtensions
    {
        public static RelationalPropertyAnnotations Relational([NotNull] this IMutableProperty property)
            => new RelationalPropertyAnnotations(Check.NotNull(property, nameof(property)), null);

        public static RelationalEntityTypeAnnotations Relational([NotNull] this IMutableEntityType entityType)
            => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), null);

        public static RelationalKeyAnnotations Relational([NotNull] this IMutableKey key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)), null);

        public static RelationalIndexAnnotations Relational([NotNull] this IMutableIndex index)
            => new RelationalIndexAnnotations(Check.NotNull(index, nameof(index)), null);

        public static RelationalForeignKeyAnnotations Relational([NotNull] this IMutableForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), null);

        public static RelationalModelAnnotations Relational([NotNull] this IMutableModel model)
            => new RelationalModelAnnotations(Check.NotNull(model, nameof(model)), null);
    }
}

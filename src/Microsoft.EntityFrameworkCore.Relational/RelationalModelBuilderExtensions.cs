// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalModelBuilderExtensions
    {
        public static RelationalSequenceBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return new RelationalSequenceBuilder(
                modelBuilder.Model.Relational().GetOrAddSequence(name, schema));
        }

        public static ModelBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
            => modelBuilder.HasSequence(name, null, builderAction);

        public static ModelBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(HasSequence(modelBuilder, name, schema));

            return modelBuilder;
        }

        public static RelationalSequenceBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Type clrType,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var sequence = modelBuilder.Model.Relational().GetOrAddSequence(name, schema);
            sequence.ClrType = clrType;

            return new RelationalSequenceBuilder(sequence);
        }

        public static ModelBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Type clrType,
            [NotNull] string name,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
            => modelBuilder.HasSequence(clrType, name, null, builderAction);

        public static ModelBuilder HasSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Type clrType,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(HasSequence(modelBuilder, clrType, name, schema));

            return modelBuilder;
        }

        public static RelationalSequenceBuilder HasSequence<T>(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var sequence = modelBuilder.Model.Relational().GetOrAddSequence(name, schema);
            sequence.ClrType = typeof(T);

            return new RelationalSequenceBuilder(sequence);
        }

        public static ModelBuilder HasSequence<T>(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
            => modelBuilder.HasSequence<T>(name, null, builderAction);

        public static ModelBuilder HasSequence<T>(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(HasSequence<T>(modelBuilder, name, schema));

            return modelBuilder;
        }

        public static ModelBuilder HasDefaultSchema(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(schema, nameof(schema));

            modelBuilder.Model.Relational().DefaultSchema = schema;

            return modelBuilder;
        }
    }
}

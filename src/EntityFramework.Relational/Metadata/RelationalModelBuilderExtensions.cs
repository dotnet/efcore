// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalModelBuilderExtensions
    {
        public static RelationalSequenceBuilder Sequence(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => new RelationalSequenceBuilder(
                Check.NotNull(modelBuilder, nameof(modelBuilder)).Model.Relational()
                    .GetOrAddSequence(
                        Check.NullButNotEmpty(name, nameof(name)),
                        Check.NullButNotEmpty(schema, nameof(schema))));

        public static ModelBuilder Sequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(Sequence(modelBuilder));

            return modelBuilder;
        }

        public static ModelBuilder Sequence(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(Sequence(modelBuilder, name));

            return modelBuilder;
        }

        public static ModelBuilder Sequence(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(Sequence(modelBuilder, name, schema));

            return modelBuilder;
        }
    }
}

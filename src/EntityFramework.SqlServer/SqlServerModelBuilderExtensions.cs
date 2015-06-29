// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class SqlServerModelBuilderExtensions
    {
        public static RelationalSequenceBuilder SqlServerSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return new RelationalSequenceBuilder(
                modelBuilder.Model.SqlServer().GetOrAddSequence(name, schema),
                s => modelBuilder.Model.SqlServer().AddOrReplaceSequence(s));
        }

        public static ModelBuilder SqlServerSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
            => modelBuilder.SqlServerSequence(name, null, builderAction);

        public static ModelBuilder SqlServerSequence(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Action<RelationalSequenceBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(SqlServerSequence(modelBuilder, name, schema));

            return modelBuilder;
        }

        public static ModelBuilder UseSqlServerSequenceHiLo(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var model = modelBuilder.Model;
            var sequence = model.SqlServer().GetOrAddSequence(name, schema);

            model.SqlServer().IdentityStrategy = SqlServerIdentityStrategy.SequenceHiLo;
            model.SqlServer().DefaultSequenceName = sequence.Name;
            model.SqlServer().DefaultSequenceSchema = sequence.Schema;

            return modelBuilder;
        }

        public static ModelBuilder UseSqlServerIdentityColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.SqlServer().IdentityStrategy = SqlServerIdentityStrategy.IdentityColumn;
            property.SqlServer().DefaultSequenceName = null;
            property.SqlServer().DefaultSequenceSchema = null;

            return modelBuilder;
        }
    }
}

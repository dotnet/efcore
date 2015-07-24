// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Sqlite.Metadata.Internal
{
    public static class SqliteInternalMetadataBuilderExtensions
    {
        public static RelationalModelBuilderAnnotations Sqlite(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalModelBuilderAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);

        public static RelationalPropertyBuilderAnnotations Sqlite(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalPropertyBuilderAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);

        public static RelationalEntityTypeBuilderAnnotations Sqlite(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeBuilderAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);

        public static RelationalKeyBuilderAnnotations Sqlite(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalKeyBuilderAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);

        public static RelationalIndexBuilderAnnotations Sqlite(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalIndexBuilderAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);

        public static RelationalForeignKeyBuilderAnnotations Sqlite(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);
    }
}

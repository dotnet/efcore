// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class SqliteInternalMetadataBuilderExtensions
    {
        public static RelationalModelBuilderAnnotations Sqlite(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalModelBuilderAnnotations(builder, configurationSource, SqliteFullAnnotationNames.Instance);

        public static RelationalPropertyBuilderAnnotations Sqlite(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalPropertyBuilderAnnotations(builder, configurationSource, SqliteFullAnnotationNames.Instance);

        public static RelationalEntityTypeBuilderAnnotations Sqlite(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeBuilderAnnotations(builder, configurationSource, SqliteFullAnnotationNames.Instance);

        public static RelationalKeyBuilderAnnotations Sqlite(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalKeyBuilderAnnotations(builder, configurationSource, SqliteFullAnnotationNames.Instance);

        public static RelationalIndexBuilderAnnotations Sqlite(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalIndexBuilderAnnotations(builder, configurationSource, SqliteFullAnnotationNames.Instance);

        public static RelationalForeignKeyBuilderAnnotations Sqlite(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource, SqliteFullAnnotationNames.Instance);
    }
}

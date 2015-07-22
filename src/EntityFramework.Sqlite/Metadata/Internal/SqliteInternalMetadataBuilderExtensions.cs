// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Sqlite.Metadata.Internal
{
    public static class SqliteInternalMetadataBuilderExtensions
    {
        public static RelationalModelAnnotations Sqlite(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalModelAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);

        public static RelationalPropertyAnnotations Sqlite(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalPropertyAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);

        public static RelationalEntityTypeAnnotations Sqlite(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);

        public static RelationalKeyAnnotations Sqlite(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalKeyAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);

        public static RelationalIndexAnnotations Sqlite(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalIndexAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);

        public static RelationalForeignKeyAnnotations Sqlite(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyAnnotations(builder, configurationSource, SqliteAnnotationNames.Prefix);
    }
}

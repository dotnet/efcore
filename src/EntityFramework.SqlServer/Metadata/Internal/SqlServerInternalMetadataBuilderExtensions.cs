// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.SqlServer.Metadata.Internal
{
    public static class SqlServerInternalMetadataBuilderExtensions
    {
        public static SqlServerModelAnnotations SqlServer(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerModelAnnotations(builder, configurationSource);

        public static SqlServerPropertyAnnotations SqlServer(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerPropertyAnnotations(builder, configurationSource);

        public static RelationalEntityTypeAnnotations SqlServer(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeAnnotations(builder, configurationSource, SqlServerAnnotationNames.Prefix);

        public static SqlServerKeyAnnotations SqlServer(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerKeyAnnotations(builder, configurationSource);

        public static SqlServerIndexAnnotations SqlServer(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerIndexAnnotations(builder, configurationSource);

        public static RelationalForeignKeyAnnotations SqlServer(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyAnnotations(builder, configurationSource, SqlServerAnnotationNames.Prefix);
    }
}

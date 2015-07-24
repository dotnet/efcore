// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.SqlServer.Metadata.Internal
{
    public static class SqlServerInternalMetadataBuilderExtensions
    {
        public static SqlServerModelBuilderAnnotations SqlServer(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerModelBuilderAnnotations(builder, configurationSource);

        public static SqlServerPropertyBuilderAnnotations SqlServer(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerPropertyBuilderAnnotations(builder, configurationSource);

        public static RelationalEntityTypeBuilderAnnotations SqlServer(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeBuilderAnnotations(builder, configurationSource, SqlServerAnnotationNames.Prefix);

        public static SqlServerKeyBuilderAnnotations SqlServer(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerKeyBuilderAnnotations(builder, configurationSource);

        public static SqlServerIndexBuilderAnnotations SqlServer(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerIndexBuilderAnnotations(builder, configurationSource);

        public static RelationalForeignKeyBuilderAnnotations SqlServer(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource, SqlServerAnnotationNames.Prefix);
    }
}

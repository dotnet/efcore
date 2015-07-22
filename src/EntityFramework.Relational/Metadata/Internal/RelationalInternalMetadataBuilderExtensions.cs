// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class RelationalInternalMetadataBuilderExtensions
    {
        public static RelationalModelAnnotations Relational(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalModelAnnotations(builder, configurationSource, null);

        public static RelationalPropertyAnnotations Relational(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalPropertyAnnotations(builder, configurationSource, null);

        public static RelationalEntityTypeAnnotations Relational(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeAnnotations(builder, configurationSource, null);

        public static RelationalKeyAnnotations Relational(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalKeyAnnotations(builder, configurationSource, null);

        public static RelationalIndexAnnotations Relational(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalIndexAnnotations(builder, configurationSource, null);

        public static RelationalForeignKeyAnnotations Relational(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyAnnotations(builder, configurationSource, null);
    }
}

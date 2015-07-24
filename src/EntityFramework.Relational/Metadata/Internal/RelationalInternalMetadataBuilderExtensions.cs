// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class RelationalInternalMetadataBuilderExtensions
    {
        public static RelationalModelBuilderAnnotations Relational(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalModelBuilderAnnotations(builder, configurationSource, null);

        public static RelationalPropertyBuilderAnnotations Relational(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalPropertyBuilderAnnotations(builder, configurationSource, null);

        public static RelationalEntityTypeBuilderAnnotations Relational(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeBuilderAnnotations(builder, configurationSource, null);

        public static RelationalKeyBuilderAnnotations Relational(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalKeyBuilderAnnotations(builder, configurationSource, null);

        public static RelationalIndexBuilderAnnotations Relational(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalIndexBuilderAnnotations(builder, configurationSource, null);

        public static RelationalForeignKeyBuilderAnnotations Relational(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource, null);
    }
}

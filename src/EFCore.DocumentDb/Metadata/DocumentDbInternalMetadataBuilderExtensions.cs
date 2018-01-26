// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public static class DocumentDbInternalMetadataBuilderExtensions
    {
        public static DocumentDbModelBuilderAnnotations DocumentDb(
            this InternalModelBuilder internalBuilder,
            ConfigurationSource configurationSource)
            => new DocumentDbModelBuilderAnnotations(internalBuilder, configurationSource);
        public static DocumentDbEntityTypeBuilderAnnotations DocumentDb(
            this InternalEntityTypeBuilder internalBuilder,
            ConfigurationSource configurationSource)
            => new DocumentDbEntityTypeBuilderAnnotations(internalBuilder, configurationSource);
        public static DocumentDbPropertyBuilderAnnotations DocumentDb(
            this InternalPropertyBuilder internalBuilder,
            ConfigurationSource configurationSource)
            => new DocumentDbPropertyBuilderAnnotations(internalBuilder, configurationSource);
        public static DocumentDbKeyBuilderAnnotations DocumentDb(
            this InternalKeyBuilder internalBuilder,
            ConfigurationSource configurationSource)
            => new DocumentDbKeyBuilderAnnotations(internalBuilder, configurationSource);
        public static DocumentDbIndexBuilderAnnotations DocumentDb(
            this InternalIndexBuilder internalBuilder,
            ConfigurationSource configurationSource)
            => new DocumentDbIndexBuilderAnnotations(internalBuilder, configurationSource);
        public static DocumentDbForeignKeyBuilderAnnotations DocumentDb(
            this InternalRelationshipBuilder internalBuilder,
            ConfigurationSource configurationSource)
            => new DocumentDbForeignKeyBuilderAnnotations(internalBuilder, configurationSource);
    }
}

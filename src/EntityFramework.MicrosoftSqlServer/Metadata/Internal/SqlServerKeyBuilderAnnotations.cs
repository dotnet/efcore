// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class SqlServerKeyBuilderAnnotations : SqlServerKeyAnnotations
    {
        public SqlServerKeyBuilderAnnotations(
            [NotNull] InternalKeyBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource, SqlServerAnnotationNames.Prefix))
        {
        }

        public new virtual bool Name([CanBeNull] string value) => SetName(value);

        public virtual bool Clustered(bool value)
        {
            var annotationsBuilder = (RelationalAnnotationsBuilder)Annotations;
            var internalBuilder = (InternalKeyBuilder)annotationsBuilder.EntityTypeBuilder;
            var indexBuilder = internalBuilder.Metadata.DeclaringEntityType.Builder
                .HasIndex(internalBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            return new SqlServerIndexBuilderAnnotations(indexBuilder, annotationsBuilder.ConfigurationSource).Clustered(value);
        }
    }
}

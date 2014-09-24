// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class SqlServerMetadataExtensions
    {
        public static class Annotations
        {
            public const string ValueGeneration = "SqlServerValueGeneration";
            public const string SequenceBlockSize = "SqlServerSequenceBlockSize";
            public const string SequenceName = "SqlServerSequenceName";
            public const string Sequence = "Sequence";
            public const string Identity = "Identity";
        }

        public static TPropertyBuilder GenerateValuesUsingSequence<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            CheckPropertyTypeForSequence(propertyBuilder);

            SetSequenceAnnotation(propertyBuilder.Metadata);
            propertyBuilder.Metadata.ValueGeneration = ValueGeneration.OnAdd;

            return (TPropertyBuilder)propertyBuilder;
        }

        public static TEntityBuilder GenerateValuesUsingSequence<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            SetSequenceAnnotation(entityBuilder.Metadata);

            return (TEntityBuilder)entityBuilder;
        }

        public static TEntityBuilder GenerateValuesUsingSequence<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            SetSequenceAnnotation(entityBuilder.Metadata);

            return (TEntityBuilder)entityBuilder;
        }

        public static TModelBuilder GenerateValuesUsingSequence<TModelBuilder>(
            [NotNull] this IModelBuilder<TModelBuilder> modelBuilder)
            where TModelBuilder : IModelBuilder<TModelBuilder>
        {
            Check.NotNull(modelBuilder, "modelBuilder");

            SetSequenceAnnotation(modelBuilder.Metadata);

            return (TModelBuilder)modelBuilder;
        }

        public static TPropertyBuilder GenerateValuesUsingSequence<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder,
            [NotNull] string sequenceName,
            int blockSize)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotEmpty(sequenceName, "sequenceName");
            CheckPropertyTypeForSequence(propertyBuilder);
            CheckBlockSize(blockSize);

            SetSequenceAnnotation(propertyBuilder.Metadata, sequenceName, blockSize);
            propertyBuilder.Metadata.ValueGeneration = ValueGeneration.OnAdd;

            return (TPropertyBuilder)propertyBuilder;
        }

        public static TEntityBuilder GenerateValuesUsingSequence<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder,
            [NotNull] string sequenceName,
            int blockSize)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");
            Check.NotEmpty(sequenceName, "sequenceName");
            CheckBlockSize(blockSize);

            SetSequenceAnnotation(entityBuilder.Metadata, sequenceName, blockSize);

            return (TEntityBuilder)entityBuilder;
        }

        public static TEntityBuilder GenerateValuesUsingSequence<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder,
            [NotNull] string sequenceName,
            int blockSize)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");
            Check.NotEmpty(sequenceName, "sequenceName");
            CheckBlockSize(blockSize);

            SetSequenceAnnotation(entityBuilder.Metadata, sequenceName, blockSize);

            return (TEntityBuilder)entityBuilder;
        }

        public static TModelBuilder GenerateValuesUsingSequence<TModelBuilder>(
            [NotNull] this IModelBuilder<TModelBuilder> modelBuilder,
            [NotNull] string sequenceName,
            int blockSize)
            where TModelBuilder : IModelBuilder<TModelBuilder>
        {
            Check.NotNull(modelBuilder, "modelBuilder");
            Check.NotEmpty(sequenceName, "sequenceName");
            CheckBlockSize(blockSize);

            SetSequenceAnnotation(modelBuilder.Metadata, sequenceName, blockSize);

            return (TModelBuilder)modelBuilder;
        }

        public static TPropertyBuilder GenerateValuesUsingIdentity<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            var propertyType = propertyBuilder.Metadata.PropertyType;
            if (!propertyType.IsInteger()
                || propertyType == typeof(byte))
            {
                throw new ArgumentException(Strings.FormatIdentityBadType(
                    propertyBuilder.Metadata.Name, propertyBuilder.Metadata.EntityType.Name, propertyType.Name));
            }

            SetIdentityAnnotation(propertyBuilder.Metadata);
            propertyBuilder.Metadata.ValueGeneration = ValueGeneration.OnAdd;

            return (TPropertyBuilder)propertyBuilder;
        }

        public static TEntityBuilder GenerateValuesUsingIdentity<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            SetIdentityAnnotation(entityBuilder.Metadata);

            return (TEntityBuilder)entityBuilder;
        }

        public static TEntityBuilder GenerateValuesUsingIdentity<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            SetIdentityAnnotation(entityBuilder.Metadata);

            return (TEntityBuilder)entityBuilder;
        }

        public static TModelBuilder GenerateValuesUsingIdentity<TModelBuilder>(
            [NotNull] this IModelBuilder<TModelBuilder> modelBuilder)
            where TModelBuilder : IModelBuilder<TModelBuilder>
        {
            Check.NotNull(modelBuilder, "modelBuilder");

            SetIdentityAnnotation(modelBuilder.Metadata);

            return (TModelBuilder)modelBuilder;
        }

        private static void CheckPropertyTypeForSequence<TPropertyBuilder>(IPropertyBuilder<TPropertyBuilder> propertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            if (!propertyBuilder.Metadata.PropertyType.IsInteger())
            {
                throw new ArgumentException(Strings.FormatSequenceBadType(
                    propertyBuilder.Metadata.Name, propertyBuilder.Metadata.EntityType.Name, propertyBuilder.Metadata.PropertyType.Name));
            }
        }

        private static void CheckBlockSize(int blockSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException("blockSize", Strings.SequenceBadBlockSize);
            }
        }

        private static void SetSequenceAnnotation(MetadataBase metadata)
        {
            metadata[Annotations.ValueGeneration] = Annotations.Sequence;
            metadata[Annotations.SequenceName] = null;
            metadata[Annotations.SequenceBlockSize] = null;
        }

        private static void SetSequenceAnnotation(MetadataBase metadata, string sequenceName, int blockSize)
        {
            metadata[Annotations.ValueGeneration] = Annotations.Sequence;
            metadata[Annotations.SequenceName] = sequenceName;
            metadata[Annotations.SequenceBlockSize] = blockSize.ToString(CultureInfo.InvariantCulture);
        }

        private static void SetIdentityAnnotation(MetadataBase metadata)
        {
            metadata[Annotations.ValueGeneration] = Annotations.Identity;
            metadata[Annotations.SequenceName] = null;
            metadata[Annotations.SequenceBlockSize] = null;
        }
    }
}

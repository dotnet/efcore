// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IMutableEntityType)" />.
    /// </summary>
    public class RelationalEntityTypeAnnotations : IRelationalEntityTypeAnnotations
    {
        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IEntityType" />.
        /// </summary>
        /// <param name="entityType"> The <see cref="IEntityType" /> to use. </param>
        public RelationalEntityTypeAnnotations(
            [NotNull] IEntityType entityType)
            : this(new RelationalAnnotations(entityType))
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IEntityType" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IEntityType" /> to annotate.
        /// </param>
        protected RelationalEntityTypeAnnotations(
            [NotNull] RelationalAnnotations annotations) => Annotations = annotations;

        /// <summary>
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IEntityType" /> to annotate.
        /// </summary>
        protected virtual RelationalAnnotations Annotations { get; }

        /// <summary>
        ///     The <see cref="IEntityType" /> to annotate.
        /// </summary>
        protected virtual IEntityType EntityType => (IEntityType)Annotations.Metadata;

        /// <summary>
        ///     Gets a <see cref="RelationalModelAnnotations" /> instance for the given <see cref="IModel" />
        ///     maintaining the <see cref="RelationalAnnotations" /> semantics being used by this instance to
        ///     control setting annotations by convention.
        /// </summary>
        /// <param name="model"> The <see cref="IModel" /> to annotate. </param>
        /// <returns> A new <see cref="RelationalModelAnnotations" /> instance. </returns>
        protected virtual RelationalModelAnnotations GetAnnotations([NotNull] IModel model)
            => new RelationalModelAnnotations(model);

        /// <summary>
        ///     Gets a <see cref="RelationalEntityTypeAnnotations" /> instance for the given <see cref="IEntityType" />
        ///     maintaining the <see cref="RelationalAnnotations" /> semantics being used by this instance to
        ///     control setting annotations by convention.
        /// </summary>
        /// <param name="entityType"> The <see cref="IEntityType" /> to annotate. </param>
        /// <returns> A new <see cref="RelationalEntityTypeAnnotations" /> instance. </returns>
        protected virtual RelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new RelationalEntityTypeAnnotations(entityType);

        /// <summary>
        ///     The name of the table to which the entity type is mapped..
        /// </summary>
        public virtual string TableName
        {
            get => EntityType.BaseType != null
                ? GetAnnotations(EntityType.RootType()).TableName
                : ((string)Annotations.Metadata[RelationalAnnotationNames.TableName]
                   ?? GetDefaultTableName());

            [param: CanBeNull] set => SetTableName(value);
        }

        private string GetDefaultTableName()
            => ConstraintNamer.Truncate(
                EntityType.HasDefiningNavigation()
                ? $"{GetAnnotations(EntityType.DefiningEntityType).TableName}_{EntityType.DefiningNavigationName}"
                : EntityType.ShortName(),
                null,
                EntityType.Model.GetMaxIdentifierLength());

        /// <summary>
        ///     Attempts to set the <see cref="TableName" /> using the semantics of the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetTableName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.TableName,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     The database schema that contains the mapped table.
        /// </summary>
        public virtual string Schema
        {
            get => EntityType.BaseType != null
                ? GetAnnotations(EntityType.RootType()).Schema
                : ((string)Annotations.Metadata[RelationalAnnotationNames.Schema]
                   ?? GetDefaultSchema());

            [param: CanBeNull] set => SetSchema(value);
        }

        private string GetDefaultSchema()
            => EntityType.HasDefiningNavigation()
                ? GetAnnotations(EntityType.DefiningEntityType).Schema
                : GetAnnotations(EntityType.Model).DefaultSchema;

        /// <summary>
        ///     Attempts to set the <see cref="Schema" /> using the semantics of the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.Schema,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     The <see cref="IProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        public virtual IProperty DiscriminatorProperty
        {
            get
            {
                if (EntityType.BaseType != null)
                {
                    return GetAnnotations(EntityType.RootType()).DiscriminatorProperty;
                }

                var propertyName = (string)Annotations.Metadata[RelationalAnnotationNames.DiscriminatorProperty];

                return propertyName == null ? null : EntityType.FindProperty(propertyName);
            }
            [param: CanBeNull] set => SetDiscriminatorProperty(value);
        }

        /// <summary>
        ///     Finds the <see cref="IProperty" /> set to be used for a discriminator on this type without
        ///     traversing to base types.
        /// </summary>
        /// <returns> The property found, or <c>null</c> if no matching property was found. </returns>
        protected virtual IProperty GetNonRootDiscriminatorProperty()
        {
            var propertyName = (string)Annotations.Metadata[RelationalAnnotationNames.DiscriminatorProperty];

            return propertyName == null ? null : EntityType.FindProperty(propertyName);
        }

        /// <summary>
        ///     Attempts to set the <see cref="DiscriminatorProperty" /> using the semantics of
        ///     the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetDiscriminatorProperty([CanBeNull] IProperty value)
            => SetDiscriminatorProperty(value, DiscriminatorProperty?.ClrType);

        /// <summary>
        ///     Attempts to set the <see cref="DiscriminatorProperty" /> using the semantics of
        ///     the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <param name="oldDiscriminatorType"> The type that was previously being used for discriminator values. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetDiscriminatorProperty([CanBeNull] IProperty value, [CanBeNull] Type oldDiscriminatorType)
        {
            if (value != null)
            {
                if (EntityType != EntityType.RootType())
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DiscriminatorPropertyMustBeOnRoot(EntityType.DisplayName()));
                }

                if (value.DeclaringEntityType != EntityType)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DiscriminatorPropertyNotFound(value.Name, EntityType.DisplayName()));
                }
            }

            if (value == null
                || value.ClrType != oldDiscriminatorType)
            {
                foreach (var derivedType in EntityType.GetDerivedTypesInclusive())
                {
                    GetAnnotations(derivedType).RemoveDiscriminatorValue();
                }
            }

            return Annotations.SetAnnotation(
                RelationalAnnotationNames.DiscriminatorProperty,
                value?.Name);
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the currently set <see cref="DiscriminatorProperty" />.
        /// </summary>
        /// <returns> The <see cref="ConfigurationSource" /> or <c>null</c> if no discriminator property has been set. </returns>
        protected virtual ConfigurationSource? GetDiscriminatorPropertyConfigurationSource()
            => (EntityType as EntityType)
                ?.FindAnnotation(RelationalAnnotationNames.DiscriminatorProperty)
                ?.GetConfigurationSource();

        /// <summary>
        ///     The discriminator value to use.
        /// </summary>
        public virtual object DiscriminatorValue
        {
            get => Annotations.Metadata[RelationalAnnotationNames.DiscriminatorValue];
            [param: CanBeNull] set => SetDiscriminatorValue(value);
        }

        /// <summary>
        ///     Attempts to set the <see cref="DiscriminatorValue" /> using the semantics of
        ///     the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetDiscriminatorValue([CanBeNull] object value)
        {
            if (value != null
                && DiscriminatorProperty == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.NoDiscriminatorForValue(EntityType.DisplayName(), EntityType.RootType().DisplayName()));
            }

            if (value != null
                && !DiscriminatorProperty.ClrType.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DiscriminatorValueIncompatible(
                        value, DiscriminatorProperty.Name, DiscriminatorProperty.ClrType));
            }

            return Annotations.SetAnnotation(RelationalAnnotationNames.DiscriminatorValue, value);
        }

        /// <summary>
        ///     Attempts to remove the <see cref="DiscriminatorValue" /> using the semantics of
        ///     the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        protected virtual bool RemoveDiscriminatorValue()
            => Annotations.RemoveAnnotation(RelationalAnnotationNames.DiscriminatorValue);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the currently set <see cref="DiscriminatorValue" />.
        /// </summary>
        /// <returns> The <see cref="ConfigurationSource" /> or <c>null</c> if no discriminator value has been set. </returns>
        protected virtual ConfigurationSource? GetDiscriminatorValueConfigurationSource()
            => (EntityType as EntityType)
                ?.FindAnnotation(RelationalAnnotationNames.DiscriminatorValue)
                ?.GetConfigurationSource();
    }
}

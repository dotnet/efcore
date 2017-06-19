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
    public class RelationalEntityTypeAnnotations : IRelationalEntityTypeAnnotations
    {
        public RelationalEntityTypeAnnotations(
            [NotNull] IEntityType entityType)
            : this(new RelationalAnnotations(entityType))
        {
        }

        protected RelationalEntityTypeAnnotations(
            [NotNull] RelationalAnnotations annotations) => Annotations = annotations;

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IEntityType EntityType => (IEntityType)Annotations.Metadata;

        protected virtual RelationalModelAnnotations GetAnnotations([NotNull] IModel model)
            => new RelationalModelAnnotations(model);

        protected virtual RelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new RelationalEntityTypeAnnotations(entityType);

        public virtual string TableName
        {
            get => EntityType.BaseType != null
                ? GetAnnotations(EntityType.RootType()).TableName
                : ((string)Annotations.Metadata[RelationalAnnotationNames.TableName]
                   ?? GetDefaultTableName());

            [param: CanBeNull] set => SetTableName(value);
        }

        private string GetDefaultTableName()
            => EntityType.HasDefiningNavigation()
                ? $"{GetAnnotations(EntityType.DefiningEntityType).TableName}_{EntityType.DefiningNavigationName}"
                : EntityType.ShortName();

        protected virtual bool SetTableName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.TableName,
                Check.NullButNotEmpty(value, nameof(value)));

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

        protected virtual bool SetSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.Schema,
                Check.NullButNotEmpty(value, nameof(value)));

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

        protected virtual IProperty GetNonRootDiscriminatorProperty()
        {
            var propertyName = (string)Annotations.Metadata[RelationalAnnotationNames.DiscriminatorProperty];

            return propertyName == null ? null : EntityType.FindProperty(propertyName);
        }

        protected virtual bool SetDiscriminatorProperty([CanBeNull] IProperty value)
            => SetDiscriminatorProperty(value, DiscriminatorProperty?.ClrType);

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
                    GetAnnotations(derivedType).DiscriminatorValue = null;
                }
            }

            return Annotations.SetAnnotation(
                RelationalAnnotationNames.DiscriminatorProperty,
                value?.Name);
        }

        protected virtual ConfigurationSource? GetDiscriminatorPropertyConfigurationSource()
            => (EntityType as EntityType)
                ?.FindAnnotation(RelationalAnnotationNames.DiscriminatorProperty)
                ?.GetConfigurationSource();

        public virtual object DiscriminatorValue
        {
            get => Annotations.Metadata[RelationalAnnotationNames.DiscriminatorValue];
            [param: CanBeNull] set => SetDiscriminatorValue(value);
        }

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

        protected virtual ConfigurationSource? GetDiscriminatorValueConfigurationSource()
            => (EntityType as EntityType)
                ?.FindAnnotation(RelationalAnnotationNames.DiscriminatorValue)
                ?.GetConfigurationSource();
    }
}

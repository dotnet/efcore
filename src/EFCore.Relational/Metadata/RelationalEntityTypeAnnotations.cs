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
        protected readonly RelationalFullAnnotationNames ProviderFullAnnotationNames;

        public RelationalEntityTypeAnnotations(
            [NotNull] IEntityType entityType,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
            : this(new RelationalAnnotations(entityType), providerFullAnnotationNames)
        {
        }

        protected RelationalEntityTypeAnnotations(
            [NotNull] RelationalAnnotations annotations,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
        {
            Annotations = annotations;
            ProviderFullAnnotationNames = providerFullAnnotationNames;
        }

        protected virtual RelationalAnnotations Annotations { get; }
        protected virtual IEntityType EntityType => (IEntityType)Annotations.Metadata;

        protected virtual RelationalModelAnnotations GetAnnotations([NotNull] IModel model)
            => new RelationalModelAnnotations(model, ProviderFullAnnotationNames);

        protected virtual RelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new RelationalEntityTypeAnnotations(entityType, ProviderFullAnnotationNames);

        public virtual string TableName
        {
            get
            {
                if (EntityType.BaseType != null)
                {
                    var rootType = EntityType.RootType();
                    return GetAnnotations(rootType).TableName;
                }

                return (string)Annotations.GetAnnotation(
                           RelationalFullAnnotationNames.Instance.TableName,
                           ProviderFullAnnotationNames?.TableName)
                       ?? EntityType.ShortName();
            }
            [param: CanBeNull] set { SetTableName(value); }
        }

        protected virtual bool SetTableName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.TableName,
                ProviderFullAnnotationNames?.TableName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string Schema
        {
            get
            {
                if (EntityType.BaseType != null)
                {
                    var rootType = EntityType.RootType();
                    return GetAnnotations(rootType).Schema;
                }

                return (string)Annotations.GetAnnotation(
                           RelationalFullAnnotationNames.Instance.Schema,
                           ProviderFullAnnotationNames?.Schema)
                       ?? GetAnnotations((IMutableModel)EntityType.Model).DefaultSchema;
            }
            [param: CanBeNull] set { SetSchema(value); }
        }

        protected virtual bool SetSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.Schema,
                ProviderFullAnnotationNames?.Schema,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual IProperty DiscriminatorProperty
        {
            get
            {
                if (EntityType.BaseType != null)
                {
                    var rootType = EntityType.RootType();
                    return GetAnnotations(rootType).DiscriminatorProperty;
                }

                var propertyName = (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.DiscriminatorProperty,
                    ProviderFullAnnotationNames?.DiscriminatorProperty);

                return propertyName == null ? null : EntityType.FindProperty(propertyName);
            }
            [param: CanBeNull] set { SetDiscriminatorProperty(value); }
        }

        protected virtual IProperty GetNonRootDiscriminatorProperty()
        {
            var propertyName = (string)Annotations.GetAnnotation(
                RelationalFullAnnotationNames.Instance.DiscriminatorProperty,
                ProviderFullAnnotationNames?.DiscriminatorProperty);

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
                RelationalFullAnnotationNames.Instance.DiscriminatorProperty,
                ProviderFullAnnotationNames?.DiscriminatorProperty,
                value?.Name);
        }

        protected virtual ConfigurationSource? GetDiscriminatorPropertyConfigurationSource()
        {
            var entityType = EntityType as EntityType;
            var annotation = (ProviderFullAnnotationNames == null ? null : entityType?.FindAnnotation(ProviderFullAnnotationNames?.DiscriminatorProperty))
                             ?? entityType?.FindAnnotation(RelationalFullAnnotationNames.Instance.DiscriminatorProperty);
            return annotation?.GetConfigurationSource();
        }

        public virtual object DiscriminatorValue
        {
            get
            {
                return Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.DiscriminatorValue,
                    ProviderFullAnnotationNames?.DiscriminatorValue);
            }
            [param: CanBeNull] set { SetDiscriminatorValue(value); }
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
                throw new InvalidOperationException(RelationalStrings.DiscriminatorValueIncompatible(
                    value, DiscriminatorProperty.Name, DiscriminatorProperty.ClrType));
            }

            return Annotations.SetAnnotation
            (RelationalFullAnnotationNames.Instance.DiscriminatorValue,
                ProviderFullAnnotationNames?.DiscriminatorValue,
                value);
        }

        protected virtual ConfigurationSource? GetDiscriminatorValueConfigurationSource()
        {
            var entityType = EntityType as EntityType;
            var annotation = (ProviderFullAnnotationNames == null ? null : entityType?.FindAnnotation(ProviderFullAnnotationNames?.DiscriminatorValue))
                             ?? entityType?.FindAnnotation(RelationalFullAnnotationNames.Instance.DiscriminatorValue);
            return annotation?.GetConfigurationSource();
        }
    }
}

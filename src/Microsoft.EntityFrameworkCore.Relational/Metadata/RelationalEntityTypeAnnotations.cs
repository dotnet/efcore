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

        public virtual string TableName
        {
            get
            {
                var rootType = EntityType.RootType();
                var rootAnnotations = new RelationalAnnotations(rootType);

                return (string)rootAnnotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.TableName,
                    ProviderFullAnnotationNames?.TableName)
                       ?? rootType.DisplayName();
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
                var rootAnnotations = new RelationalAnnotations(EntityType.RootType());
                return (string)rootAnnotations.GetAnnotation(
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
                var rootType = EntityType.RootType();
                var rootAnnotations = new RelationalAnnotations(rootType);
                var propertyName = (string)rootAnnotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.DiscriminatorProperty,
                    ProviderFullAnnotationNames?.DiscriminatorProperty);

                return propertyName == null ? null : rootType.FindProperty(propertyName);
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
        {
            if (value != null)
            {
                if (EntityType != EntityType.RootType())
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DiscriminatorPropertyMustBeOnRoot(EntityType));
                }

                if (value.DeclaringEntityType != EntityType)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DiscriminatorPropertyNotFound(value.Name, EntityType));
                }
            }

            foreach (var derivedType in EntityType.GetDerivedTypes())
            {
                new RelationalAnnotations(derivedType).SetAnnotation(
                    RelationalFullAnnotationNames.Instance.DiscriminatorValue,
                    ProviderFullAnnotationNames?.DiscriminatorValue,
                    null);
            }

            return Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.DiscriminatorProperty,
                ProviderFullAnnotationNames?.DiscriminatorProperty,
                value?.Name);
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
            if (DiscriminatorProperty == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.NoDiscriminatorForValue(EntityType.DisplayName(), EntityType.RootType().DisplayName()));
            }

            if ((value != null)
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
    }
}

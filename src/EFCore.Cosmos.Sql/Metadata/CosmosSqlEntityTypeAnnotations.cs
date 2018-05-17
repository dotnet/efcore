// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata
{
    public class CosmosSqlEntityTypeAnnotations : ICosmosSqlEntityTypeAnnotations
    {
        public CosmosSqlEntityTypeAnnotations(IEntityType entityType)
            : this(new CosmosSqlAnnotations(entityType))
        {
        }

        protected CosmosSqlEntityTypeAnnotations(CosmosSqlAnnotations annotations) => Annotations = annotations;

        protected virtual CosmosSqlAnnotations Annotations { get; }

        protected virtual IEntityType EntityType => (IEntityType)Annotations.Metadata;

        protected virtual CosmosSqlEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new CosmosSqlEntityTypeAnnotations(entityType);

        public virtual string CollectionName
        {
            get => EntityType.BaseType != null
                ? GetAnnotations(EntityType.RootType()).CollectionName
                : ((string)Annotations.Metadata[CosmosSqlAnnotationNames.CollectionName])
                    ?? GetDefaultCollectionName();

            [param: CanBeNull]
            set => SetCollectionName(value);
        }

        private static string GetDefaultCollectionName() => "Unicorn";

        protected virtual bool SetCollectionName([CanBeNull] string value)
        {
            return Annotations.SetAnnotation(
                CosmosSqlAnnotationNames.CollectionName,
                Check.NullButNotEmpty(value, nameof(value)));
        }

        public virtual IProperty DiscriminatorProperty
        {
            get
            {
                if (EntityType.BaseType != null)
                {
                    return GetAnnotations(EntityType.RootType()).DiscriminatorProperty;
                }

                var propertyName = (string)Annotations.Metadata[CosmosSqlAnnotationNames.DiscriminatorProperty];

                return propertyName == null ? null : EntityType.FindProperty(propertyName);
            }
            [param: CanBeNull]
            set => SetDiscriminatorProperty(value);
        }

        protected virtual bool SetDiscriminatorProperty([CanBeNull] IProperty value)
            => SetDiscriminatorProperty(value, DiscriminatorProperty?.ClrType);

        protected virtual bool SetDiscriminatorProperty([CanBeNull] IProperty value, [CanBeNull] Type oldDiscriminatorType)
        {
            if (value != null)
            {
                if (EntityType != EntityType.RootType())
                {
                    //throw new InvalidOperationException(
                    //    RelationalStrings.DiscriminatorPropertyMustBeOnRoot(EntityType.DisplayName()));
                }

                if (value.DeclaringEntityType != EntityType)
                {
                    //throw new InvalidOperationException(
                    //    RelationalStrings.DiscriminatorPropertyNotFound(value.Name, EntityType.DisplayName()));
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
                CosmosSqlAnnotationNames.DiscriminatorProperty,
                value?.Name);
        }

        protected virtual bool RemoveDiscriminatorValue()
            => Annotations.RemoveAnnotation(CosmosSqlAnnotationNames.DiscriminatorValue);

        public virtual object DiscriminatorValue
        {
            get => Annotations.Metadata[CosmosSqlAnnotationNames.DiscriminatorValue];
            [param: CanBeNull]
            set => SetDiscriminatorValue(value);
        }

        protected virtual bool SetDiscriminatorValue([CanBeNull] object value)
        {
            if (value != null
                && DiscriminatorProperty == null)
            {
                //throw new InvalidOperationException(
                //    RelationalStrings.NoDiscriminatorForValue(EntityType.DisplayName(), EntityType.RootType().DisplayName()));
            }

            if (value != null
                && !DiscriminatorProperty.ClrType.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))
            {
                //throw new InvalidOperationException(
                //    RelationalStrings.DiscriminatorValueIncompatible(
                //        value, DiscriminatorProperty.Name, DiscriminatorProperty.ClrType));
            }

            return Annotations.SetAnnotation(CosmosSqlAnnotationNames.DiscriminatorValue, value);
        }
    }
}

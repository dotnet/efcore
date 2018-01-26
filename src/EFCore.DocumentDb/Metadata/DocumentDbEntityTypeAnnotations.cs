// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DocumentDbEntityTypeAnnotations : IDocumentDbEntityTypeAnnotations
    {
        public DocumentDbEntityTypeAnnotations(IEntityType entityType)
            : this(new DocumentDbAnnotations(entityType))
        {
        }

        protected DocumentDbEntityTypeAnnotations(DocumentDbAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected virtual DocumentDbAnnotations Annotations { get; }
        protected virtual IEntityType EntityType => (IEntityType)Annotations.Metadata;

        protected virtual bool SetCollectionName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                DocumentDbAnnotationNames.CollectionName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string CollectionName
        {
            get => EntityType.BaseType != null
                ? GetAnnotations(EntityType.RootType()).CollectionName
                : ((string)Annotations.Metadata[DocumentDbAnnotationNames.CollectionName]
                   ?? GetDefaultCollectionName());

            [param: CanBeNull]
            set => SetCollectionName(value);
        }

        private string GetDefaultCollectionName()
            => EntityType.ShortName();

        protected virtual DocumentDbEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new DocumentDbEntityTypeAnnotations(entityType);
        public virtual IProperty DiscriminatorProperty
        {
            get
            {
                if (EntityType.BaseType != null)
                {
                    return GetAnnotations(EntityType.RootType()).DiscriminatorProperty;
                }

                var propertyName = (string)Annotations.Metadata[DocumentDbAnnotationNames.DiscriminatorProperty];

                return propertyName == null ? null : EntityType.FindProperty(propertyName);
            }
            [param: CanBeNull]
            set => SetDiscriminatorProperty(value);
        }

        protected virtual bool SetDiscriminatorProperty([CanBeNull] IProperty value)
            => SetDiscriminatorProperty(value, DiscriminatorProperty?.ClrType);

        protected virtual bool SetDiscriminatorProperty(
            [CanBeNull] IProperty value, [CanBeNull] Type oldDiscriminatorType)
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
                    GetAnnotations(derivedType).DiscriminatorValue = null;
                }
            }

            return Annotations.SetAnnotation(
                DocumentDbAnnotationNames.DiscriminatorProperty,
                value?.Name);
        }

        public virtual object DiscriminatorValue
        {
            get => Annotations.Metadata[DocumentDbAnnotationNames.DiscriminatorValue];
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

            return Annotations.SetAnnotation(DocumentDbAnnotationNames.DiscriminatorValue, value);
        }

        protected virtual IProperty GetNonRootDiscriminatorProperty()
        {
            var propertyName = (string)Annotations.Metadata[DocumentDbAnnotationNames.DiscriminatorProperty];

            return propertyName == null ? null : EntityType.FindProperty(propertyName);
        }
    }
}

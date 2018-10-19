// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata
{
    public class CosmosEntityTypeAnnotations : ICosmosEntityTypeAnnotations
    {
        public CosmosEntityTypeAnnotations(IEntityType entityType)
            : this(new CosmosAnnotations(entityType))
        {
        }

        protected CosmosEntityTypeAnnotations(CosmosAnnotations annotations) => Annotations = annotations;

        protected virtual CosmosAnnotations Annotations { get; }

        protected virtual IEntityType EntityType => (IEntityType)Annotations.Metadata;

        protected virtual CosmosModelAnnotations GetAnnotations(IModel model)
            => new CosmosModelAnnotations(model);

        protected virtual CosmosEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new CosmosEntityTypeAnnotations(entityType);

        public virtual string ContainerName
        {
            get => EntityType.BaseType != null
                ? GetAnnotations(EntityType.RootType()).ContainerName
                : ((string)Annotations.Metadata[CosmosAnnotationNames.ContainerName])
                    ?? GetDefaultContainerName();

            [param: CanBeNull]
            set => SetContainerName(value);
        }

        private string GetDefaultContainerName() => GetAnnotations(EntityType.Model).DefaultContainerName
            ?? EntityType.ShortName();

        protected virtual bool SetContainerName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                CosmosAnnotationNames.ContainerName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual IProperty DiscriminatorProperty
        {
            get
            {
                if (EntityType.BaseType != null)
                {
                    return GetAnnotations(EntityType.RootType()).DiscriminatorProperty;
                }

                var propertyName = (string)Annotations.Metadata[CosmosAnnotationNames.DiscriminatorProperty];

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
                CosmosAnnotationNames.DiscriminatorProperty,
                value?.Name);
        }

        protected virtual bool RemoveDiscriminatorValue()
            => Annotations.RemoveAnnotation(CosmosAnnotationNames.DiscriminatorValue);

        public virtual object DiscriminatorValue
        {
            get => Annotations.Metadata[CosmosAnnotationNames.DiscriminatorValue];
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

            return Annotations.SetAnnotation(CosmosAnnotationNames.DiscriminatorValue, value);
        }
    }
}

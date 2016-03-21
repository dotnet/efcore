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
        public RelationalEntityTypeAnnotations([NotNull] IEntityType entityType, [CanBeNull] string providerPrefix)
            : this(new RelationalAnnotations(entityType, providerPrefix))
        {
        }

        protected RelationalEntityTypeAnnotations([NotNull] RelationalAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IEntityType EntityType => (IEntityType)Annotations.Metadata;

        public virtual string TableName
        {
            get
            {
                var rootType = EntityType.RootType();
                var rootAnnotations = new RelationalAnnotations(rootType, Annotations.ProviderPrefix);

                return (string)rootAnnotations.GetAnnotation(RelationalAnnotationNames.TableName)
                       ?? rootType.DisplayName();
            }
            [param: CanBeNull]
            set { SetTableName(value); }
        }

        protected virtual bool SetTableName([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.TableName, Check.NullButNotEmpty(value, nameof(value)));

        public virtual string Database
        {
            get
            {
                var rootAnnotations = new RelationalAnnotations(EntityType.RootType(), Annotations.ProviderPrefix);
                return (string)rootAnnotations.GetAnnotation(RelationalAnnotationNames.DatabaseName) ?? ((Model)EntityType.Model).Relational().DefaultSchema;
            }
            [param: CanBeNull]
            set { SetDatabase(value); }
        }

        public virtual string Schema
        {
            get
            {
                var rootAnnotations = new RelationalAnnotations(EntityType.RootType(), Annotations.ProviderPrefix);
                return (string)rootAnnotations.GetAnnotation(RelationalAnnotationNames.Schema) ?? ((Model)EntityType.Model).Relational().DefaultSchema;
            }
            [param: CanBeNull]
            set { SetSchema(value); }
        }

        protected virtual bool SetDatabase([CanBeNull] string value)
          => Annotations.SetAnnotation(RelationalAnnotationNames.DatabaseName, Check.NullButNotEmpty(value, nameof(value)));

        protected virtual bool SetSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.Schema, Check.NullButNotEmpty(value, nameof(value)));

        public virtual IProperty DiscriminatorProperty
        {
            get
            {
                var rootType = EntityType.RootType();
                var rootAnnotations = new RelationalAnnotations(rootType, Annotations.ProviderPrefix);
                var propertyName = (string)rootAnnotations.GetAnnotation(RelationalAnnotationNames.DiscriminatorProperty);

                return propertyName == null ? null : rootType.FindProperty(propertyName);
            }
            [param: CanBeNull]
            set { SetDiscriminatorProperty(value); }
        }

        protected virtual IProperty GetNonRootDiscriminatorProperty()
        {
            var propertyName = (string)Annotations.GetAnnotation(RelationalAnnotationNames.DiscriminatorProperty);

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

            return Annotations.SetAnnotation(RelationalAnnotationNames.DiscriminatorProperty, value?.Name);
        }

        public virtual object DiscriminatorValue
        {
            get { return Annotations.GetAnnotation(RelationalAnnotationNames.DiscriminatorValue); }
            [param: CanBeNull]
            set { SetDiscriminatorValue(value); }
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

            return Annotations.SetAnnotation(RelationalAnnotationNames.DiscriminatorValue, value);
        }
    }
}

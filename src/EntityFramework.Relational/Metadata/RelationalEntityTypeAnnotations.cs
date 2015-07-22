// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalEntityTypeAnnotations : RelationalAnnotationsBase, IRelationalEntityTypeAnnotations
    {
        public RelationalEntityTypeAnnotations([NotNull] IEntityType entityType, [CanBeNull] string providerPrefix)
            : base(entityType, providerPrefix)
        {
        }

        protected virtual IEntityType EntityType => (IEntityType)Metadata;

        public virtual string TableName
        {
            get
            {
                var rootType = EntityType.RootType();

                return (string)GetAnnotation(rootType, RelationalAnnotationNames.TableName)
                       ?? rootType.DisplayName();
            }
            [param: CanBeNull] set { SetAnnotation(RelationalAnnotationNames.TableName, Check.NullButNotEmpty(value, nameof(value))); }
        }

        public virtual string Schema
        {
            get { return (string)GetAnnotation(EntityType.RootType(), RelationalAnnotationNames.Schema); }
            [param: CanBeNull] set { SetAnnotation(RelationalAnnotationNames.Schema, Check.NullButNotEmpty(value, nameof(value))); }
        }

        public virtual IProperty DiscriminatorProperty
        {
            get
            {
                var rootType = EntityType.RootType();

                var propertyName = (string)GetAnnotation(rootType, RelationalAnnotationNames.DiscriminatorProperty);

                return propertyName == null ? null : rootType.GetProperty(propertyName);
            }
            [param: CanBeNull]
            set
            {
                if (value != null)
                {
                    if (EntityType != EntityType.RootType())
                    {
                        throw new InvalidOperationException(
                            Strings.DiscriminatorPropertyMustBeOnRoot(EntityType));
                    }

                    if (value.DeclaringEntityType != EntityType)
                    {
                        throw new InvalidOperationException(
                            Strings.DiscriminatorPropertyNotFound(value, EntityType));
                    }
                }

                SetAnnotation(RelationalAnnotationNames.DiscriminatorProperty, value?.Name);
            }
        }

        public virtual object DiscriminatorValue
        {
            get
            {
                return GetAnnotation(RelationalAnnotationNames.DiscriminatorValue)
                       ?? EntityType.DisplayName();
            }
            [param: CanBeNull] set { SetAnnotation(RelationalAnnotationNames.DiscriminatorValue, value); }
        }
    }
}

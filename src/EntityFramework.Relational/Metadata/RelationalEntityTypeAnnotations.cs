// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalEntityTypeAnnotations : ReadOnlyRelationalEntityTypeAnnotations
    {
        public RelationalEntityTypeAnnotations([NotNull] EntityType entityType)
            : base(entityType)
        {
        }

        public new virtual string Table
        {
            get { return base.Table; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                EntityType[RelationalTableAnnotation] = value;
            }
        }

        public new virtual string Schema
        {
            get { return base.Schema; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                EntityType[RelationalSchemaAnnotation] = value;
            }
        }

        public new virtual IProperty DiscriminatorProperty
        {
            get { return base.DiscriminatorProperty; }
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

                    if (value.EntityType != EntityType)
                    {
                        throw new InvalidOperationException(
                            Strings.DiscriminatorPropertyNotFound(value, EntityType));
                    }

                    EntityType[DiscriminatorPropertyAnnotation] = value.Name;
                }
                else
                {
                    EntityType[DiscriminatorPropertyAnnotation] = null;
                }
            }
        }

        public new virtual string DiscriminatorValue
        {
            get { return base.DiscriminatorValue; }
            [param: CanBeNull] set { EntityType[DiscriminatorValueAnnotation] = value; }
        }

        protected new virtual EntityType EntityType => (EntityType)base.EntityType;
    }
}

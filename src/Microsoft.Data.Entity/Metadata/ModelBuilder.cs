// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelBuilder
    {
        private readonly Model _model;

        public ModelBuilder([NotNull] Model model)
        {
            Check.NotNull(model, "model");

            _model = model;
        }

        public virtual EntityBuilder<T> Entity<T>()
        {
            var type = typeof(T);
            var entityType = _model.Entity(type);

            if (entityType == null)
            {
                _model.AddEntity(entityType = new EntityType(type));
            }

            return new EntityBuilder<T>(entityType);
        }

        public virtual ModelBuilder Annotation([NotNull] string annotation, [NotNull] string value)
        {
            Check.NotEmpty(annotation, "annotation");
            Check.NotEmpty(value, "value");

            _model[annotation] = value;

            return this;
        }

        public class MetadataBuilder<TMetadata, TMetadataBuilder>
            where TMetadata : MetadataBase
            where TMetadataBuilder : MetadataBuilder<TMetadata, TMetadataBuilder>
        {
            private readonly TMetadata _metadata;

            internal MetadataBuilder(TMetadata metadata)
            {
                _metadata = metadata;
            }

            public TMetadataBuilder Annotation([NotNull] string annotation, [NotNull] string value)
            {
                Check.NotEmpty(annotation, "annotation");
                Check.NotEmpty(value, "value");

                _metadata[annotation] = value;

                return (TMetadataBuilder)this;
            }

            protected TMetadata Metadata
            {
                get { return _metadata; }
            }
        }

        public class EntityBuilder<TEntity> : MetadataBuilder<EntityType, EntityBuilder<TEntity>>
        {
            internal EntityBuilder(EntityType entityType)
                : base(entityType)
            {
            }

            public EntityBuilder<TEntity> Key<TKey>([NotNull] Expression<Func<TEntity, TKey>> keyExpression)
            {
                Check.NotNull(keyExpression, "keyExpression");

                var propertyInfos = keyExpression.GetPropertyAccessList();

                Metadata.Key
                    = propertyInfos
                        .Select(pi => Metadata.Property(pi.Name) ?? new Property(pi));

                return this;
            }

            public EntityBuilder<TEntity> Properties([NotNull] Action<PropertiesBuilder> propertiesBuilder)
            {
                Check.NotNull(propertiesBuilder, "propertiesBuilder");

                propertiesBuilder(new PropertiesBuilder(Metadata));

                return this;
            }

            public EntityBuilder<TEntity> StorageName([NotNull] string storageName)
            {
                Metadata.StorageName = storageName;

                return this;
            }

            public class PropertiesBuilder
            {
                private readonly EntityType _entityType;

                internal PropertiesBuilder(EntityType entityType)
                {
                    _entityType = entityType;
                }

                public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
                {
                    var propertyInfo = propertyExpression.GetPropertyAccess();

                    var property = _entityType.Property(propertyInfo.Name);

                    if (property == null)
                    {
                        _entityType.AddProperty(property = new Property(propertyInfo));
                    }

                    return new PropertyBuilder(property);
                }

                public class PropertyBuilder : MetadataBuilder<Property, PropertyBuilder>
                {
                    internal PropertyBuilder(Property property)
                        : base(property)
                    {
                    }

                    public PropertyBuilder StorageName([NotNull] string storageName)
                    {
                        Metadata.StorageName = storageName;

                        return this;
                    }
                }
            }
        }
    }
}

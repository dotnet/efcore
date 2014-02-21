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

        public class EntityBuilder<TEntity>
        {
            private readonly EntityType _entityType;

            internal EntityBuilder()
            {
            }

            internal EntityBuilder(EntityType entityType)
            {
                _entityType = entityType;
            }

            public EntityBuilder<TEntity> Key<TKey>([NotNull] Expression<Func<TEntity, TKey>> keyExpression)
            {
                Check.NotNull(keyExpression, "keyExpression");

                var propertyInfos = keyExpression.GetPropertyAccessList();

                _entityType.Key
                    = propertyInfos
                        .Select(pi => _entityType.Property(pi.Name) ?? new Property(pi));

                return this;
            }

            public EntityBuilder<TEntity> Annotation([NotNull] string annotation, [NotNull] string value)
            {
                Check.NotEmpty(annotation, "annotation");
                Check.NotEmpty(value, "value");

                _entityType[annotation] = value;

                return this;
            }

            public EntityBuilder<TEntity> Properties([NotNull] Action<PropertiesBuilder> propertiesBuilder)
            {
                Check.NotNull(propertiesBuilder, "propertiesBuilder");

                propertiesBuilder(new PropertiesBuilder(_entityType));

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

                public class PropertyBuilder
                {
                    private readonly Property _property;

                    internal PropertyBuilder()
                    {
                    }

                    internal PropertyBuilder(Property property)
                    {
                        _property = property;
                    }

                    public PropertyBuilder Annotation([NotNull] string annotation, [NotNull] string value)
                    {
                        Check.NotEmpty(annotation, "annotation");
                        Check.NotEmpty(value, "value");

                        _property[annotation] = value;

                        return this;
                    }
                }
            }
        }
    }
}

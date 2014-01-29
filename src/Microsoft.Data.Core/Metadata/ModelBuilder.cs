// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core.Metadata
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using JetBrains.Annotations;
    using Microsoft.Data.Core.Utilities;

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
            var entity = _model.Entity(type);

            if (entity == null)
            {
                _model.AddEntity(entity = new Entity(type));
            }

            return new EntityBuilder<T>(entity);
        }

        public virtual ModelBuilder Annotation([NotNull] string annotation, [NotNull] object value)
        {
            Check.NotEmpty(annotation, "annotation");
            Check.NotNull(value, "value");

            _model[annotation] = value;

            return this;
        }

        public class EntityBuilder<TEntity>
        {
            private readonly Entity _entity;

            internal EntityBuilder()
            {
            }

            internal EntityBuilder(Entity entity)
            {
                DebugCheck.NotNull(entity);

                _entity = entity;
            }

            public EntityBuilder<TEntity> Key<TKey>([NotNull] Expression<Func<TEntity, TKey>> keyExpression)
            {
                Check.NotNull(keyExpression, "keyExpression");

                var propertyInfos = keyExpression.GetPropertyAccessList();

                _entity.Key
                    = propertyInfos
                        .Select(pi => _entity.Property(pi) ?? new Property(pi));

                return this;
            }

            public EntityBuilder<TEntity> Annotation([NotNull] string annotation, [NotNull] object value)
            {
                Check.NotEmpty(annotation, "annotation");
                Check.NotNull(value, "value");

                _entity[annotation] = value;

                return this;
            }

            public EntityBuilder<TEntity> Properties([NotNull] Action<PropertiesBuilder> propertiesBuilder)
            {
                Check.NotNull(propertiesBuilder, "propertiesBuilder");

                propertiesBuilder(new PropertiesBuilder(_entity));

                return this;
            }

            public class PropertiesBuilder
            {
                private readonly Entity _entity;

                internal PropertiesBuilder(Entity entity)
                {
                    DebugCheck.NotNull(entity);

                    _entity = entity;
                }

                public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
                {
                    var propertyInfo = propertyExpression.GetPropertyAccess();

                    var property = _entity.Property(propertyInfo);

                    if (property == null)
                    {
                        _entity.AddProperty(property = new Property(propertyInfo));
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
                        DebugCheck.NotNull(property);

                        _property = property;
                    }

                    public PropertyBuilder Annotation([NotNull] string annotation, [NotNull] object value)
                    {
                        Check.NotEmpty(annotation, "annotation");
                        Check.NotNull(value, "value");

                        _property[annotation] = value;

                        return this;
                    }
                }
            }
        }
    }
}

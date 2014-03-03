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
            var entityType = _model.TryGetEntityType(type);

            if (entityType == null)
            {
                _model.AddEntityType(entityType = new EntityType(type));
            }

            return new EntityBuilder<T>(entityType, this);
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
            private readonly ModelBuilder _modelBuilder;

            internal MetadataBuilder(TMetadata metadata)
                : this(metadata, null)
            {
            }

            internal MetadataBuilder(TMetadata metadata, ModelBuilder modelBuilder)
            {
                _metadata = metadata;
                _modelBuilder = modelBuilder;
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

            protected ModelBuilder ModelBuilder
            {
                get { return _modelBuilder; }
            }
        }

        public class EntityBuilder<TEntity> : MetadataBuilder<EntityType, EntityBuilder<TEntity>>
        {
            internal EntityBuilder(EntityType entityType, ModelBuilder modelBuilder)
                : base(entityType, modelBuilder)
            {
            }

            public EntityBuilder<TEntity> StorageName([NotNull] string storageName)
            {
                Metadata.StorageName = storageName;

                return this;
            }

            public EntityBuilder<TEntity> Key([NotNull] Expression<Func<TEntity, object>> keyExpression)
            {
                Check.NotNull(keyExpression, "keyExpression");

                Metadata.Key
                    = keyExpression.GetPropertyAccessList()
                        .Select(pi => Metadata.Property(pi.Name) ?? new Property(pi));

                return this;
            }

            public EntityBuilder<TEntity> Properties([NotNull] Action<PropertiesBuilder> propertiesBuilder)
            {
                Check.NotNull(propertiesBuilder, "propertiesBuilder");

                propertiesBuilder(new PropertiesBuilder(Metadata));

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

            public EntityBuilder<TEntity> ForeignKeys([NotNull] Action<ForeignKeysBuilder> foreignKeysBuilder)
            {
                Check.NotNull(foreignKeysBuilder, "foreignKeysBuilder");

                foreignKeysBuilder(new ForeignKeysBuilder(Metadata, ModelBuilder));

                return this;
            }

            public class ForeignKeysBuilder
            {
                private readonly EntityType _entityType;
                private readonly ModelBuilder _modelBuilder;

                internal ForeignKeysBuilder(EntityType entityType, ModelBuilder modelBuilder)
                {
                    _entityType = entityType;
                    _modelBuilder = modelBuilder;
                }

                public ForeignKeyBuilder ForeignKey<TReferencedEntityType>(
                    [NotNull] Expression<Func<TEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    var principalType = _modelBuilder.Entity<TReferencedEntityType>().Metadata;

                    var dependentProperties = foreignKeyExpression.GetPropertyAccessList()
                        .Select(pi => _entityType.Property(pi.Name) ?? new Property(pi));

                    // TODO: This code currently assumes that the FK maps to a PK on the principal end
                    var foreignKey = new ForeignKey(
                        principalType, principalType.Key.Zip(dependentProperties, (p, d) => new PropertyPair(p, d)));

                    _entityType.AddForeignKey(foreignKey);

                    return new ForeignKeyBuilder(foreignKey);
                }

                public class ForeignKeyBuilder : MetadataBuilder<ForeignKey, ForeignKeyBuilder>
                {
                    internal ForeignKeyBuilder(ForeignKey foreignKey)
                        : base(foreignKey)
                    {
                    }

                    public ForeignKeyBuilder StorageName([NotNull] string storageName)
                    {
                        Metadata.StorageName = storageName;

                        return this;
                    }

                    public ForeignKeyBuilder IsUnique()
                    {
                        Metadata.IsUnique = true;

                        return this;
                    }
                }
            }
        }
    }
}

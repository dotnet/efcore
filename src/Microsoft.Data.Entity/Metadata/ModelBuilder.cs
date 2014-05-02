// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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

        public ModelBuilder()
        {
            _model = new Model();
        }

        public ModelBuilder([NotNull] Model model)
        {
            Check.NotNull(model, "model");

            _model = model;
        }

        // TODO: Consider whether this is needed/desirable; currently builder is not full-fidelity
        public virtual Model Model
        {
            get { return _model; }
        }

        public virtual EntityBuilder<T> Entity<T>()
        {
            var type = typeof(T);
            var entityType = Entity(type);

            return new EntityBuilder<T>(entityType, this);
        }

        internal EntityType Entity(Type type)
        {
            var entityType = _model.TryGetEntityType(type);

            if (entityType == null)
            {
                _model.AddEntityType(entityType = new EntityType(type));
                OnEntityTypeAdded(entityType);
            }

            return entityType;
        }

        protected virtual void OnEntityTypeAdded([NotNull] EntityType entityType)
        {
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

            // TODO: Implement mechanism to add annotations to key.
            public EntityBuilder<TEntity> Key([NotNull] Expression<Func<TEntity, object>> keyExpression)
            {
                Check.NotNull(keyExpression, "keyExpression");

                Metadata.SetKey(
                    keyExpression.GetPropertyAccessList()
                        .Select(pi => Metadata.TryGetProperty(pi.Name)
                                      ?? Metadata.AddProperty(pi))
                        .ToArray());

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

                    var property
                        = _entityType.TryGetProperty(propertyInfo.Name)
                          ?? _entityType.AddProperty(propertyInfo);

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

                    var dependentProperties
                        = foreignKeyExpression.GetPropertyAccessList()
                            .Select(pi => _entityType.TryGetProperty(pi.Name) ?? _entityType.AddProperty(pi))
                            .ToArray();

                    // TODO: This code currently assumes that the FK maps to a PK on the principal end
                    var foreignKey
                        = _entityType.AddForeignKey(principalType.GetKey(), dependentProperties);

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
                        ((ForeignKey)Metadata).StorageName = storageName;

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

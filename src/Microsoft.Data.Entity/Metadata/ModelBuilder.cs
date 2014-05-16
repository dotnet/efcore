// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        public virtual EntityBuilder Entity([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            var entityType = GetEntity(name);

            return new EntityBuilder(entityType, this);
        }

        public virtual EntityBuilder<T> Entity<T>()
        {
            var type = typeof(T);
            var entityType = GetEntity(type);

            return new EntityBuilder<T>(entityType, this);
        }

        internal EntityType GetEntity(string name)
        {
            var entityType = _model.TryGetEntityType(name);

            if (entityType == null)
            {
                _model.AddEntityType(entityType = new EntityType(name));
                OnEntityTypeAdded(entityType);
            }

            return entityType;
        }

        internal EntityType GetEntity(Type type)
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

        public class EntityBuilderBase<TMetadataBuilder> : MetadataBuilder<EntityType, TMetadataBuilder>
            where TMetadataBuilder : MetadataBuilder<EntityType, TMetadataBuilder>
        {
            internal EntityBuilderBase(EntityType entityType, ModelBuilder modelBuilder)
                : base(entityType, modelBuilder)
            {
            }

            public EntityBuilderBase<TMetadataBuilder> StorageName([NotNull] string storageName)
            {
                Metadata.StorageName = storageName;

                return this;
            }

            // TODO: Implement mechanism to add annotations to key.
            public EntityBuilderBase<TMetadataBuilder> Key([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, "propertyNames");

                Metadata.SetKey(propertyNames.Select(n => Metadata.GetProperty(n)).ToArray());

                return this;
            }

            public EntityBuilderBase<TMetadataBuilder> Properties([NotNull] Action<PropertiesBuilder> propertiesBuilder)
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

                protected EntityType EntityType
                {
                    get { return _entityType; }
                }

                public virtual PropertyBuilder Property<TProperty>(
                    [NotNull] string name, bool shadowProperty = false, bool concurrencyToken = false)
                {
                    Check.NotEmpty(name, "name");

                    var property
                        = _entityType.TryGetProperty(name)
                          ?? _entityType.AddProperty(name, typeof(TProperty), shadowProperty, concurrencyToken);

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

                    // TODO Consider if this should be relational only
                    public PropertyBuilder UseStoreSequence()
                    {
                        Metadata.ValueGenerationOnAdd = ValueGenerationOnAdd.Server;
                        Metadata.ValueGenerationOnSave = ValueGenerationOnSave.None;

                        return this;
                    }

                    // TODO Consider if this should be relational only
                    public PropertyBuilder UseStoreSequence([NotNull] string sequenceName, int blockSize)
                    {
                        Check.NotEmpty(sequenceName, "sequenceName");

                        // TODO: Make these constants in some class once decided if this should be relational-only
                        Metadata["StoreSequenceName"] = sequenceName;
                        Metadata["StoreSequenceBlockSize"] = blockSize.ToString();

                        return UseStoreSequence();
                    }
                }
            }

            public EntityBuilderBase<TMetadataBuilder> ForeignKeys([NotNull] Action<ForeignKeysBuilder> foreignKeysBuilder)
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

                protected EntityType EntityType
                {
                    get { return _entityType; }
                }

                protected ModelBuilder ModelBuilder
                {
                    get { return _modelBuilder; }
                }

                public ForeignKeyBuilder ForeignKey([NotNull] string referencedEntityTypeName, [NotNull] params string[] propertyNames)
                {
                    Check.NotNull(referencedEntityTypeName, "referencedEntityTypeName");
                    Check.NotNull(propertyNames, "propertyNames");

                    var principalType = _modelBuilder._model.GetEntityType(referencedEntityTypeName);
                    var dependentProperties = propertyNames.Select(n => _entityType.GetProperty(n)).ToArray();

                    // TODO: This code currently assumes that the FK maps to a PK on the principal end
                    var foreignKey = _entityType.AddForeignKey(principalType.GetKey(), dependentProperties);

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

        public class EntityBuilder : EntityBuilderBase<EntityBuilder>
        {
            internal EntityBuilder(EntityType entityType, ModelBuilder modelBuilder)
                : base(entityType, modelBuilder)
            {
            }
        }

        public class EntityBuilder<TEntity> : EntityBuilderBase<EntityBuilder<TEntity>>
        {
            internal EntityBuilder(EntityType entityType, ModelBuilder modelBuilder)
                : base(entityType, modelBuilder)
            {
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

            public new class PropertiesBuilder : EntityBuilderBase<EntityBuilder<TEntity>>.PropertiesBuilder
            {
                internal PropertiesBuilder(EntityType entityType)
                    : base(entityType)
                {
                }

                public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
                {
                    var propertyInfo = propertyExpression.GetPropertyAccess();

                    var property
                        = EntityType.TryGetProperty(propertyInfo.Name)
                          ?? EntityType.AddProperty(propertyInfo);

                    return new PropertyBuilder(property);
                }
            }

            public EntityBuilder<TEntity> ForeignKeys([NotNull] Action<ForeignKeysBuilder> foreignKeysBuilder)
            {
                Check.NotNull(foreignKeysBuilder, "foreignKeysBuilder");

                foreignKeysBuilder(new ForeignKeysBuilder(Metadata, ModelBuilder));

                return this;
            }

            public new class ForeignKeysBuilder : EntityBuilderBase<EntityBuilder<TEntity>>.ForeignKeysBuilder
            {
                internal ForeignKeysBuilder(EntityType entityType, ModelBuilder modelBuilder)
                    : base(entityType, modelBuilder)
                {
                }

                public ForeignKeyBuilder ForeignKey<TReferencedEntityType>(
                    [NotNull] Expression<Func<TEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    var principalType = ModelBuilder.Entity<TReferencedEntityType>().Metadata;

                    var dependentProperties
                        = foreignKeyExpression.GetPropertyAccessList()
                            .Select(pi => EntityType.TryGetProperty(pi.Name) ?? EntityType.AddProperty(pi))
                            .ToArray();

                    // TODO: This code currently assumes that the FK maps to a PK on the principal end
                    var foreignKey
                        = EntityType.AddForeignKey(principalType.GetKey(), dependentProperties);

                    return new ForeignKeyBuilder(foreignKey);
                }
            }
        }
    }
}

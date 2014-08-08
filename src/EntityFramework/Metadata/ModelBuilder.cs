// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.ModelConventions;
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

            return new EntityBuilder(GetOrAddEntity(name), this);
        }

        public virtual EntityBuilder<T> Entity<T>()
        {
            return new EntityBuilder<T>(GetOrAddEntity(typeof(T)), this);
        }

        public virtual ModelBuilder Entity([NotNull] string name, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity(name));

            return this;
        }

        public virtual ModelBuilder Entity<T>([NotNull] Action<EntityBuilder<T>> entityBuilder)
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity<T>());

            return this;
        }

        internal EntityType GetOrAddEntity(string name)
        {
            var entityType = _model.TryGetEntityType(name);

            if (entityType == null)
            {
                _model.AddEntityType(entityType = new EntityType(name));
                OnEntityTypeAdded(entityType);
            }

            return entityType;
        }

        internal EntityType GetOrAddEntity(Type type)
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

            protected internal TMetadata Metadata
            {
                get { return _metadata; }
            }

            protected internal ModelBuilder ModelBuilder
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

            public KeyBuilder Key([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, "propertyNames");

                Metadata.SetKey(propertyNames.Select(n => Metadata.GetProperty(n)).ToArray());

                return new KeyBuilder(Metadata.GetKey(), ModelBuilder);
            }

            public class KeyBuilder : MetadataBuilder<Key, KeyBuilder>
            {
                internal KeyBuilder(Key key, ModelBuilder modelBuilder)
                    : base(key, modelBuilder)
                {
                }
            }

            public virtual PropertyBuilder Property<TProperty>(
                [NotNull] string name, bool shadowProperty = false, bool concurrencyToken = false)
            {
                Check.NotEmpty(name, "name");

                var property
                    = Metadata.TryGetProperty(name)
                      ?? Metadata.AddProperty(name, typeof(TProperty), shadowProperty, concurrencyToken);

                return new PropertyBuilder(property, ModelBuilder);
            }

            public class PropertyBuilder : MetadataBuilder<Property, PropertyBuilder>
            {
                internal PropertyBuilder(Property property, ModelBuilder modelBuilder)
                    : base(property, modelBuilder)
                {
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

                    return new ForeignKeyBuilder(foreignKey, ModelBuilder);
                }

                public class ForeignKeyBuilder : MetadataBuilder<ForeignKey, ForeignKeyBuilder>
                {
                    internal ForeignKeyBuilder(ForeignKey foreignKey, ModelBuilder modelBuilder)
                        : base(foreignKey, modelBuilder)
                    {
                    }

                    public ForeignKeyBuilder IsUnique()
                    {
                        Metadata.IsUnique = true;

                        return this;
                    }
                }
            }

            public EntityBuilderBase<TMetadataBuilder> Indexes([NotNull] Action<IndexesBuilder> indexesBuilder)
            {
                Check.NotNull(indexesBuilder, "indexesBuilder");

                indexesBuilder(new IndexesBuilder(Metadata, ModelBuilder));

                return this;
            }

            public class IndexesBuilder : MetadataBuilder<EntityType, IndexesBuilder>
            {
                internal IndexesBuilder(EntityType entityType, ModelBuilder modelBuilder)
                    : base(entityType, modelBuilder)
                {
                }

                public IndexBuilder Index([NotNull] params string[] propertyNames)
                {
                    Check.NotNull(propertyNames, "propertyNames");

                    var properties = propertyNames.Select(n => Metadata.GetProperty(n)).ToArray();
                    var index = Metadata.AddIndex(properties);

                    return new IndexBuilder(index, ModelBuilder);
                }

                public class IndexBuilder : MetadataBuilder<Index, IndexBuilder>
                {
                    internal IndexBuilder(Index index, ModelBuilder modelBuilder)
                        : base(index, modelBuilder)
                    {
                    }

                    public IndexBuilder IsUnique()
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

            public KeyBuilder Key([NotNull] Expression<Func<TEntity, object>> keyExpression)
            {
                Check.NotNull(keyExpression, "keyExpression");

                Metadata.SetKey(
                    keyExpression.GetPropertyAccessList()
                        .Select(pi => Metadata.TryGetProperty(pi.Name)
                                      ?? Metadata.AddProperty(pi))
                        .ToArray());

                return new KeyBuilder(Metadata.GetKey(), ModelBuilder);
            }

            public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
            {
                var propertyInfo = propertyExpression.GetPropertyAccess();

                var property
                    = Metadata.TryGetProperty(propertyInfo.Name)
                      ?? Metadata.AddProperty(propertyInfo);

                return new PropertyBuilder(property, ModelBuilder);
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
                    [NotNull] Expression<Func<TEntity, object>> foreignKeyExpression, bool isUnique = false)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    var principalType = ModelBuilder.Entity<TReferencedEntityType>().Metadata;

                    var dependentProperties
                        = foreignKeyExpression.GetPropertyAccessList()
                            .Select(pi => EntityType.TryGetProperty(pi.Name) ?? EntityType.AddProperty(pi))
                            .ToArray();

                    // TODO: This code currently assumes that the FK maps to a PK on the principal end
                    var foreignKey = EntityType.AddForeignKey(principalType.GetKey(), dependentProperties);
                    foreignKey.IsUnique = isUnique;

                    return new ForeignKeyBuilder(foreignKey, ModelBuilder);
                }
            }

            public EntityBuilder<TEntity> Indexes([NotNull] Action<IndexesBuilder> indexesBuilder)
            {
                Check.NotNull(indexesBuilder, "indexesBuilder");

                indexesBuilder(new IndexesBuilder(Metadata, ModelBuilder));

                return this;
            }

            public new class IndexesBuilder : EntityBuilderBase<EntityBuilder<TEntity>>.IndexesBuilder
            {
                internal IndexesBuilder(EntityType entityType, ModelBuilder modelBuilder)
                    : base(entityType, modelBuilder)
                {
                }

                public IndexBuilder Index([NotNull] Expression<Func<TEntity, object>> indexExpression)
                {
                    Check.NotNull(indexExpression, "indexExpression");

                    var properties
                        = indexExpression.GetPropertyAccessList()
                            .Select(pi => Metadata.TryGetProperty(pi.Name) ?? Metadata.AddProperty(pi))
                            .ToArray();
                    var index = Metadata.AddIndex(properties);

                    return new IndexBuilder(index, ModelBuilder);
                }
            }

            public OneToManyBuilder<TRelatedEntity> OneToMany<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null,
                [CanBeNull] Expression<Func<TRelatedEntity, TEntity>> reference = null)
            {
                return new OneToManyBuilder<TRelatedEntity>(OneToManyInternal(collection, reference));
            }

            public ManyToOneBuilder<TRelatedEntity> ManyToOne<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, TRelatedEntity>> reference = null,
                [CanBeNull] Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> collection = null)
            {
                return new ManyToOneBuilder<TRelatedEntity>(OneToManyInternal(collection, reference));
            }

            private OneToManyBuilderInternal<TPrincipalEntity, TDependentEntity> OneToManyInternal<TPrincipalEntity, TDependentEntity>(
                Expression<Func<TPrincipalEntity, IEnumerable<TDependentEntity>>> collection,
                Expression<Func<TDependentEntity, TPrincipalEntity>> reference)
            {
                // TODO: Checking for bad/inconsistent FK/navigation/type configuration in this method and below

                var dependentType = ModelBuilder.Entity<TDependentEntity>().Metadata;
                var principalType = ModelBuilder.Entity<TPrincipalEntity>().Metadata;

                // Find either navigation that already exists
                var navNameToDependent = collection != null ? collection.GetPropertyAccess().Name : null;
                var navNameToPrincipal = reference != null ? reference.GetPropertyAccess().Name : null;

                var navToDependent = principalType.Navigations.FirstOrDefault(e => e.Name == navNameToDependent);
                var navToPrincipal = dependentType.Navigations.FirstOrDefault(e => e.Name == navNameToPrincipal);

                // Find the associated FK on an already existing navigation, or create one by convention
                // TODO: If FK isn't already specified, then creating the navigation should cause it to be found/created
                // by convention, but this part of conventions is not done yet, so we do it here instead--kind of h.acky

                var foreignKey = navToDependent != null
                    ? navToDependent.ForeignKey
                    : navToPrincipal != null
                        ? navToPrincipal.ForeignKey
                        : new ForeignKeyConvention().FindOrCreateForeignKey(principalType, dependentType, navNameToPrincipal);

                if (navNameToDependent != null
                    && navToDependent == null)
                {
                    navToDependent = principalType.AddNavigation(new Navigation(foreignKey, navNameToDependent, pointsToPrincipal: false));
                }

                if (navNameToPrincipal != null
                    && navToPrincipal == null)
                {
                    navToPrincipal = dependentType.AddNavigation(new Navigation(foreignKey, navNameToPrincipal, pointsToPrincipal: true));
                }

                return new OneToManyBuilderInternal<TPrincipalEntity, TDependentEntity>(
                    foreignKey, ModelBuilder, principalType, dependentType, navToPrincipal, navToDependent);
            }

            public class OneToManyBuilder<TRelatedEntity> : MetadataBuilder<ForeignKey, OneToManyBuilder<TRelatedEntity>>
            {
                private readonly OneToManyBuilderInternal<TEntity, TRelatedEntity> _internalBuilder;

                internal OneToManyBuilder(OneToManyBuilderInternal<TEntity, TRelatedEntity> internalBuilder)
                    : base(internalBuilder.Metadata, internalBuilder.ModelBuilder)
                {
                    _internalBuilder = internalBuilder;
                }

                public OneToManyBuilder<TRelatedEntity> ForeignKey(
                    [NotNull] Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    return new OneToManyBuilder<TRelatedEntity>(_internalBuilder.ForeignKey(foreignKeyExpression));
                }
            }

            public class ManyToOneBuilder<TRelatedEntity> : MetadataBuilder<ForeignKey, ManyToOneBuilder<TRelatedEntity>>
            {
                private readonly OneToManyBuilderInternal<TRelatedEntity, TEntity> _internalBuilder;

                internal ManyToOneBuilder(OneToManyBuilderInternal<TRelatedEntity, TEntity> internalBuilder)
                    : base(internalBuilder.Metadata, internalBuilder.ModelBuilder)
                {
                    _internalBuilder = internalBuilder;
                }

                public ManyToOneBuilder<TRelatedEntity> ForeignKey(
                    [NotNull] Expression<Func<TEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    return new ManyToOneBuilder<TRelatedEntity>(_internalBuilder.ForeignKey(foreignKeyExpression));
                }
            }

            internal class OneToManyBuilderInternal<TPrincipalEntity, TDependentEntity>
                : MetadataBuilder<ForeignKey, OneToManyBuilderInternal<TPrincipalEntity, TDependentEntity>>
            {
                private readonly EntityType _principalType;
                private readonly EntityType _dependentType;
                private readonly Navigation _navigationToPrincipal;
                private readonly Navigation _navigationToDependent;

                public OneToManyBuilderInternal(
                    ForeignKey metadata, ModelBuilder modelBuilder,
                    EntityType principalType, EntityType dependentType,
                    Navigation navigationToPrincipal, Navigation navigationToDependent)
                    : base(metadata, modelBuilder)
                {
                    _principalType = principalType;
                    _dependentType = dependentType;
                    _navigationToPrincipal = navigationToPrincipal;
                    _navigationToDependent = navigationToDependent;
                }

                public OneToManyBuilderInternal<TPrincipalEntity, TDependentEntity> ForeignKey(
                    [NotNull] Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    var dependentProperties
                        = foreignKeyExpression.GetPropertyAccessList()
                            .Select(pi => _dependentType.TryGetProperty(pi.Name) ?? _dependentType.AddProperty(pi))
                            .ToArray();

                    var foreignKey = Metadata;
                    if (!foreignKey.Properties.SequenceEqual(dependentProperties))
                    {
                        foreignKey = new ForeignKeyConvention().FindOrCreateForeignKey(
                            _principalType,
                            _dependentType,
                            _navigationToPrincipal != null ? _navigationToPrincipal.Name : null,
                            new[] { dependentProperties });

                        // TODO: Remove FK only if it was added by convention
                        _dependentType.RemoveForeignKey(Metadata);

                        // TODO: Remove property only if it was added by convention
                        foreach (var property in Metadata.Properties.Except(dependentProperties))
                        {
                            _dependentType.RemoveProperty(property);
                        }

                        if (_navigationToPrincipal != null)
                        {
                            _navigationToPrincipal.ForeignKey = foreignKey;
                        }

                        if (_navigationToDependent != null)
                        {
                            _navigationToDependent.ForeignKey = foreignKey;
                        }
                    }

                    if (foreignKey.IsUnique)
                    {
                        // TODO: Only override this if it wasn't set explicitly. If it was, throw, or trust.
                        foreignKey.IsUnique = false;
                    }

                    return new OneToManyBuilderInternal<TPrincipalEntity, TDependentEntity>(
                        foreignKey, ModelBuilder, _principalType, _dependentType, _navigationToPrincipal, _navigationToDependent);
                }
            }
        }
    }
}

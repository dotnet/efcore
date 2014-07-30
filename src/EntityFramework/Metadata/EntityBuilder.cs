using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class EntityBuilderBase<TMetadataBuilder> : MetadataBuilder<EntityType, TMetadataBuilder>
        where TMetadataBuilder : MetadataBuilder<EntityType, TMetadataBuilder>
    {
        internal EntityBuilderBase(EntityType entityType, ModelBuilder modelBuilder)
            : base(entityType, modelBuilder)
        {
        }

        public EntityBuilderBase<TMetadataBuilder> Key([NotNull] params string[] propertyNames)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return Key(k => k.Properties(propertyNames));
        }

        public EntityBuilderBase<TMetadataBuilder> Key([NotNull] Action<KeyBuilder> keyBuilder)
        {
            Check.NotNull(keyBuilder, "keyBuilder");

            keyBuilder(new KeyBuilder(Metadata));

            return this;
        }

        public class KeyBuilder
        {
            private readonly EntityType _entityType;

            internal KeyBuilder(EntityType entityType)
            {
                _entityType = entityType;
            }

            protected EntityType EntityType
            {
                get { return _entityType; }
            }

            public KeyMetadataBuilder Properties([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, "propertyNames");

                _entityType.SetKey(propertyNames.Select(n => _entityType.GetProperty(n)).ToArray());

                return new KeyMetadataBuilder(_entityType.GetKey());
            }
        }

        public class KeyMetadataBuilder : MetadataBuilder<Key, KeyMetadataBuilder>
        {
            internal KeyMetadataBuilder(Key key)
                : base(key)
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

            return new PropertyBuilder(property);
        }

        public class PropertyBuilder : MetadataBuilder<Property, PropertyBuilder>
        {
            internal PropertyBuilder(Property property)
                : base(property)
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

                var principalType = _modelBuilder.Model.GetEntityType(referencedEntityTypeName);
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

            indexesBuilder(new IndexesBuilder(Metadata));

            return this;
        }

        public class IndexesBuilder
        {
            private readonly EntityType _entityType;

            internal IndexesBuilder(EntityType entityType)
            {
                _entityType = entityType;
            }

            protected EntityType EntityType
            {
                get { return _entityType; }
            }

            public IndexBuilder Index([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, "propertyNames");

                var properties = propertyNames.Select(n => _entityType.GetProperty(n)).ToArray();
                var index = _entityType.AddIndex(properties);

                return new IndexBuilder(index);
            }

            public class IndexBuilder : MetadataBuilder<Index, IndexBuilder>
            {
                internal IndexBuilder(Index index)
                    : base(index)
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

        public EntityBuilder<TEntity> Key([NotNull] Expression<Func<TEntity, object>> keyExpression)
        {
            Check.NotNull(keyExpression, "keyExpression");

            return Key(k => k.Properties(keyExpression));
        }

        public EntityBuilder<TEntity> Key([NotNull] Action<KeyBuilder> keyBuilder)
        {
            Check.NotNull(keyBuilder, "keyBuilder");

            keyBuilder(new KeyBuilder(Metadata));

            return this;
        }

        public new class KeyBuilder : EntityBuilderBase<EntityBuilder<TEntity>>.KeyBuilder
        {
            internal KeyBuilder(EntityType entityType)
                : base(entityType)
            {
            }

            public virtual KeyMetadataBuilder Properties([NotNull] Expression<Func<TEntity, object>> keyExpression)
            {
                Check.NotNull(keyExpression, "keyExpression");

                EntityType.SetKey(
                    keyExpression.GetPropertyAccessList()
                        .Select(pi => EntityType.TryGetProperty(pi.Name)
                                      ?? EntityType.AddProperty(pi))
                        .ToArray());

                return new KeyMetadataBuilder(EntityType.GetKey());
            }
        }

        public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
        {
            var propertyInfo = propertyExpression.GetPropertyAccess();

            var property
                = Metadata.TryGetProperty(propertyInfo.Name)
                  ?? Metadata.AddProperty(propertyInfo);

            return new PropertyBuilder(property);
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

                return new ForeignKeyBuilder(foreignKey);
            }
        }

        public EntityBuilder<TEntity> Indexes([NotNull] Action<IndexesBuilder> indexesBuilder)
        {
            Check.NotNull(indexesBuilder, "indexesBuilder");

            indexesBuilder(new IndexesBuilder(Metadata));

            return this;
        }

        public new class IndexesBuilder : EntityBuilderBase<EntityBuilder<TEntity>>.IndexesBuilder
        {
            internal IndexesBuilder(EntityType entityType)
                : base(entityType)
            {
            }

            public IndexBuilder Index([NotNull] Expression<Func<TEntity, object>> indexExpression)
            {
                Check.NotNull(indexExpression, "indexExpression");

                var properties
                    = indexExpression.GetPropertyAccessList()
                        .Select(pi => EntityType.TryGetProperty(pi.Name) ?? EntityType.AddProperty(pi))
                        .ToArray();
                var index = EntityType.AddIndex(properties);

                return new IndexBuilder(index);
            }
        }
    }

}
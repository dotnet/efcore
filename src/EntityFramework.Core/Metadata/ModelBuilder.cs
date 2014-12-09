// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelBuilder : IModelChangeListener, IModelBuilder<ModelBuilder>
    {
        private readonly InternalModelBuilder _builder;

        // TODO: Get the default convention list from DI
        // Issue #213
        // TODO: Configure property facets, foreign keys & navigation properties
        // Issue #213

        public ModelBuilder()
            : this(new Model())
        {
        }

        public ModelBuilder([NotNull] Model model)
        {
            Check.NotNull(model, "model");

            _builder = new InternalModelBuilder(model, this);
            EntityTypeConventions = new List<IEntityTypeConvention>
                {
                    new PropertiesConvention(),
                    new KeyConvention(),
                    new RelationshipDiscoveryConvention()
                };
        }

        protected ModelBuilder([NotNull] Model model, [NotNull] IList<IEntityTypeConvention> entityTypeConventions)
        {
            Check.NotNull(model, "model");
            Check.NotNull(entityTypeConventions, "entityTypeConventions");

            _builder = new InternalModelBuilder(model, this);
            EntityTypeConventions = entityTypeConventions;
        }

        protected internal ModelBuilder([NotNull] InternalModelBuilder internalBuilder)
        {
            Check.NotNull(internalBuilder, "internalBuilder");

            _builder = internalBuilder;
        }

        public virtual Model Metadata
        {
            get { return Builder.Metadata; }
        }

        public virtual Model Model
        {
            get { return Metadata; }
        }

        public virtual IList<IEntityTypeConvention> EntityTypeConventions { get; }

        public virtual ModelBuilder Annotation(string annotation, string value)
        {
            Check.NotEmpty(annotation, "annotation");
            Check.NotEmpty(value, "value");

            _builder.Annotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        protected virtual InternalModelBuilder Builder
        {
            get { return _builder; }
        }

        protected virtual void OnEntityTypeAdded([NotNull] InternalEntityBuilder entityBuilder)
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            foreach (var entityTypeConvention in EntityTypeConventions)
            {
                entityTypeConvention.Apply(entityBuilder);
            }
        }

        void IModelChangeListener.OnEntityTypeAdded(InternalEntityBuilder entityBuilder)
        {
            OnEntityTypeAdded(entityBuilder);
        }

        public virtual EntityBuilder<TEntity> Entity<TEntity>() where TEntity : class
        {
            return new EntityBuilder<TEntity>(Builder.Entity(typeof(TEntity), ConfigurationSource.Explicit));
        }

        public virtual EntityBuilder Entity([NotNull] Type entityType)
        {
            Check.NotNull(entityType, "entityType");

            return new EntityBuilder(Builder.Entity(entityType, ConfigurationSource.Explicit));
        }

        public virtual EntityBuilder Entity([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            return new EntityBuilder(Builder.Entity(name, ConfigurationSource.Explicit));
        }

        public virtual ModelBuilder Entity<TEntity>([NotNull] Action<EntityBuilder<TEntity>> entityBuilder) where TEntity : class
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity<TEntity>());

            return this;
        }

        public virtual ModelBuilder Entity([NotNull] Type entityType, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity(entityType));

            return this;
        }

        public virtual ModelBuilder Entity([NotNull] string name, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity(name));

            return this;
        }

        public virtual void Ignore<TEntity>() where TEntity : class
        {
            Ignore(typeof(TEntity));
        }

        public virtual void Ignore([NotNull] Type entityType)
        {
            Check.NotNull(entityType, "entityType");

            Builder.Ignore(entityType, ConfigurationSource.Explicit);
        }

        public virtual void Ignore([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            Builder.Ignore(name, ConfigurationSource.Explicit);
        }

        public class EntityBuilder : IEntityBuilder<EntityBuilder>
        {
            public EntityBuilder([NotNull] InternalEntityBuilder builder)
            {
                Check.NotNull(builder, "builder");

                Builder = builder;
            }

            protected virtual InternalEntityBuilder Builder { get; }

            public virtual EntityType Metadata
            {
                get { return Builder.Metadata; }
            }

            Model IMetadataBuilder<EntityType, EntityBuilder>.Model
            {
                get { return Builder.ModelBuilder.Metadata; }
            }

            public virtual EntityBuilder Annotation(string annotation, string value)
            {
                Check.NotEmpty(annotation, "annotation");
                Check.NotEmpty(value, "value");

                Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                return this;
            }

            public virtual KeyBuilder Key([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, "propertyNames");

                return new KeyBuilder(Builder.Key(propertyNames, ConfigurationSource.Explicit));
            }

            public virtual PropertyBuilder Property<TProperty>([NotNull] string propertyName)
            {
                Check.NotEmpty(propertyName, "propertyName");

                return Property(typeof(TProperty), propertyName);
            }

            public virtual PropertyBuilder Property([NotNull] Type propertyType, [NotNull] string propertyName)
            {
                Check.NotNull(propertyType, "propertyType");
                Check.NotEmpty(propertyName, "propertyName");

                return new PropertyBuilder(Builder.Property(propertyType, propertyName, ConfigurationSource.Explicit));
            }

            public virtual void Ignore([NotNull] string propertyName)
            {
                Check.NotEmpty(propertyName, "propertyName");

                Builder.Ignore(propertyName, ConfigurationSource.Explicit);
            }

            public virtual ForeignKeyBuilder ForeignKey([NotNull] string referencedEntityTypeName, [NotNull] params string[] propertyNames)
            {
                Check.NotNull(referencedEntityTypeName, "referencedEntityTypeName");
                Check.NotNull(propertyNames, "propertyNames");

                return new ForeignKeyBuilder(Builder.ForeignKey(referencedEntityTypeName, propertyNames, ConfigurationSource.Explicit));
            }

            public virtual IndexBuilder Index([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, "propertyNames");

                return new IndexBuilder(Builder.Index(propertyNames, ConfigurationSource.Explicit));
            }

            public virtual OneToManyBuilder OneToMany(
                [NotNull] Type relatedEntityType,
                [CanBeNull] string collection = null,
                [CanBeNull] string reference = null)
            {
                Check.NotNull(relatedEntityType, "relatedEntityType");

                return new OneToManyBuilder(Builder.Relationship(
                    Metadata,
                    Builder.ModelBuilder.Entity(relatedEntityType, ConfigurationSource.Explicit).Metadata,
                    reference,
                    collection,
                    oneToOne: false,
                    configurationSource: ConfigurationSource.Explicit));
            }

            public virtual ManyToOneBuilder ManyToOne(
                [NotNull] Type relatedEntityType,
                [CanBeNull] string reference = null,
                [CanBeNull] string collection = null)
            {
                Check.NotNull(relatedEntityType, "relatedEntityType");

                return new ManyToOneBuilder(Builder.Relationship(
                    Builder.ModelBuilder.Entity(relatedEntityType, ConfigurationSource.Explicit).Metadata,
                    Metadata,
                    reference,
                    collection,
                    oneToOne: false,
                    configurationSource: ConfigurationSource.Explicit));
            }

            public virtual OneToOneBuilder OneToOne(
                [NotNull] Type relatedEntityType,
                [CanBeNull] string navigationToDependent = null,
                [CanBeNull] string navigationToPrincipal = null)
            {
                Check.NotNull(relatedEntityType, "relatedEntityType");

                return new OneToOneBuilder(Builder.Relationship(
                    Metadata,
                    Builder.ModelBuilder.Entity(relatedEntityType, ConfigurationSource.Explicit).Metadata,
                    navigationToPrincipal,
                    navigationToDependent,
                    oneToOne: true,
                    configurationSource: ConfigurationSource.Explicit));
            }

            public virtual OneToManyBuilder OneToMany(
                [NotNull] string relatedEntityTypeName,
                [CanBeNull] string collection = null,
                [CanBeNull] string reference = null)
            {
                Check.NotEmpty(relatedEntityTypeName, "relatedEntityTypeName");

                return new OneToManyBuilder(Builder.Relationship(
                    Metadata,
                    Builder.ModelBuilder.Metadata.GetEntityType(relatedEntityTypeName),
                    reference,
                    collection,
                    oneToOne: false,
                    configurationSource: ConfigurationSource.Explicit));
            }

            public virtual ManyToOneBuilder ManyToOne(
                [NotNull] string relatedEntityTypeName,
                [CanBeNull] string reference = null,
                [CanBeNull] string collection = null)
            {
                Check.NotEmpty(relatedEntityTypeName, "relatedEntityTypeName");

                return new ManyToOneBuilder(Builder.Relationship(
                    Builder.ModelBuilder.Metadata.GetEntityType(relatedEntityTypeName),
                    Metadata,
                    reference,
                    collection,
                    oneToOne: false,
                    configurationSource: ConfigurationSource.Explicit));
            }

            public virtual OneToOneBuilder OneToOne(
                [NotNull] string relatedEntityTypeName,
                [CanBeNull] string navigationToDependent = null,
                [CanBeNull] string navigationToPrincipal = null)
            {
                Check.NotEmpty(relatedEntityTypeName, "relatedEntityTypeName");

                return new OneToOneBuilder(Builder.Relationship(
                    Metadata,
                    Builder.ModelBuilder.Metadata.GetEntityType(relatedEntityTypeName),
                    navigationToPrincipal,
                    navigationToDependent,
                    oneToOne: true,
                    configurationSource: ConfigurationSource.Explicit));
            }

            public class KeyBuilder : IKeyBuilder<KeyBuilder>
            {
                public KeyBuilder([NotNull] InternalKeyBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    Builder = builder;
                }

                protected virtual InternalKeyBuilder Builder { get; }

                public virtual Key Metadata
                {
                    get { return Builder.Metadata; }
                }

                Model IMetadataBuilder<Key, KeyBuilder>.Model
                {
                    get { return Builder.ModelBuilder.Metadata; }
                }

                public virtual KeyBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, "annotation");
                    Check.NotEmpty(value, "value");

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }
            }

            public class PropertyBuilder : IPropertyBuilder<PropertyBuilder>
            {
                public PropertyBuilder([NotNull] InternalPropertyBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    Builder = builder;
                }

                protected virtual InternalPropertyBuilder Builder { get; }

                public virtual Property Metadata
                {
                    get { return Builder.Metadata; }
                }

                Model IMetadataBuilder<Property, PropertyBuilder>.Model
                {
                    get { return Builder.ModelBuilder.Metadata; }
                }

                public virtual PropertyBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, "annotation");
                    Check.NotEmpty(value, "value");

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual PropertyBuilder Required(bool isRequired = true)
                {
                    Builder.Required(isRequired, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual PropertyBuilder MaxLength(int maxLength)
                {
                    Builder.MaxLength(maxLength, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual PropertyBuilder ConcurrencyToken(bool isConcurrencyToken = true)
                {
                    Builder.ConcurrencyToken(isConcurrencyToken, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual PropertyBuilder Shadow(bool isShadowProperty = true)
                {
                    Builder.Shadow(isShadowProperty, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual PropertyBuilder GenerateValueOnAdd(bool generateValue = true)
                {
                    Builder.GenerateValueOnAdd(generateValue, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual PropertyBuilder StoreComputed(bool computed = true)
                {
                    Builder.StoreComputed(computed, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual PropertyBuilder UseStoreDefault(bool useDefault = true)
                {
                    Builder.UseStoreDefault(useDefault, ConfigurationSource.Explicit);

                    return this;
                }
            }

            public class ForeignKeyBuilder : IForeignKeyBuilder<ForeignKeyBuilder>
            {
                public ForeignKeyBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    Builder = builder;
                }

                protected virtual InternalRelationshipBuilder Builder { get; }

                public virtual ForeignKey Metadata
                {
                    get { return Builder.Metadata; }
                }

                Model IMetadataBuilder<ForeignKey, ForeignKeyBuilder>.Model
                {
                    get { return Builder.ModelBuilder.Metadata; }
                }

                public virtual ForeignKeyBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, "annotation");
                    Check.NotEmpty(value, "value");

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual ForeignKeyBuilder IsUnique(bool isUnique = true)
                {
                    return new ForeignKeyBuilder(Builder.Unique(isUnique, ConfigurationSource.Explicit));
                }
            }

            public class IndexBuilder : IIndexBuilder<IndexBuilder>
            {
                public IndexBuilder([NotNull] InternalIndexBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    Builder = builder;
                }

                protected virtual InternalIndexBuilder Builder { get; }

                public virtual Index Metadata
                {
                    get { return Builder.Metadata; }
                }

                Model IMetadataBuilder<Index, IndexBuilder>.Model
                {
                    get { return Builder.ModelBuilder.Metadata; }
                }

                public virtual IndexBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, "annotation");
                    Check.NotEmpty(value, "value");

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual IndexBuilder IsUnique(bool isUnique = true)
                {
                    Builder.IsUnique(isUnique, ConfigurationSource.Explicit);

                    return this;
                }
            }

            public class OneToManyBuilder : IOneToManyBuilder<OneToManyBuilder>
            {
                public OneToManyBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    Builder = builder;
                }

                public virtual ForeignKey Metadata
                {
                    get { return Builder.Metadata; }
                }

                Model IMetadataBuilder<ForeignKey, OneToManyBuilder>.Model
                {
                    get { return Builder.ModelBuilder.Metadata; }
                }

                protected virtual InternalRelationshipBuilder Builder { get; }

                public virtual OneToManyBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, "annotation");
                    Check.NotEmpty(value, "value");

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual OneToManyBuilder ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, "foreignKeyPropertyNames");

                    return new OneToManyBuilder(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToManyBuilder ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, "keyPropertyNames");

                    return new OneToManyBuilder(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToManyBuilder Required(bool required = true)
                {
                    Builder.Required(required, ConfigurationSource.Explicit);

                    return this;
                }
            }

            public class ManyToOneBuilder : IManyToOneBuilder<ManyToOneBuilder>
            {
                public ManyToOneBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    Builder = builder;
                }

                protected virtual InternalRelationshipBuilder Builder { get; }

                public virtual ForeignKey Metadata
                {
                    get { return Builder.Metadata; }
                }

                Model IMetadataBuilder<ForeignKey, ManyToOneBuilder>.Model
                {
                    get { return Builder.ModelBuilder.Metadata; }
                }

                public virtual ManyToOneBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, "annotation");
                    Check.NotEmpty(value, "value");

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual ManyToOneBuilder ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, "foreignKeyPropertyNames");

                    return new ManyToOneBuilder(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual ManyToOneBuilder ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, "keyPropertyNames");

                    return new ManyToOneBuilder(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual ManyToOneBuilder Required(bool required = true)
                {
                    Builder.Required(required, ConfigurationSource.Explicit);

                    return this;
                }
            }

            public class OneToOneBuilder : IOneToOneBuilder<OneToOneBuilder>
            {
                public OneToOneBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    Builder = builder;
                }

                protected virtual InternalRelationshipBuilder Builder { get; }

                public virtual ForeignKey Metadata
                {
                    get { return Builder.Metadata; }
                }

                Model IMetadataBuilder<ForeignKey, OneToOneBuilder>.Model
                {
                    get { return Builder.ModelBuilder.Metadata; }
                }

                public virtual OneToOneBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, "annotation");
                    Check.NotEmpty(value, "value");

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual OneToOneBuilder ForeignKey(
                    [NotNull] Type dependentEntityType,
                    [NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(dependentEntityType, "dependentEntityType");
                    Check.NotNull(foreignKeyPropertyNames, "foreignKeyPropertyNames");

                    return new OneToOneBuilder(Builder.ForeignKey(dependentEntityType, foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ReferencedKey(
                    [NotNull] Type principalEntityType,
                    [NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(principalEntityType, "principalEntityType");
                    Check.NotNull(keyPropertyNames, "keyPropertyNames");

                    return new OneToOneBuilder(Builder.ReferencedKey(principalEntityType, keyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ForeignKey(
                    [NotNull] string dependentEntityTypeName,
                    [NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(dependentEntityTypeName, "dependentEntityTypeName");
                    Check.NotNull(foreignKeyPropertyNames, "foreignKeyPropertyNames");

                    return new OneToOneBuilder(Builder.ForeignKey(dependentEntityTypeName, foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ReferencedKey(
                    [NotNull] string principalEntityTypeName,
                    [NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(principalEntityTypeName, "principalEntityTypeName");
                    Check.NotNull(keyPropertyNames, "keyPropertyNames");

                    return new OneToOneBuilder(Builder.ReferencedKey(principalEntityTypeName, keyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ForeignKey<TDependentEntity>(
                    [NotNull] Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    return new OneToOneBuilder(
                        Builder.ForeignKey(typeof(TDependentEntity), foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ReferencedKey<TPrincipalEntity>(
                    [NotNull] Expression<Func<TPrincipalEntity, object>> keyExpression)
                {
                    Check.NotNull(keyExpression, "keyExpression");

                    return new OneToOneBuilder(Builder.ReferencedKey(typeof(TPrincipalEntity), keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder Required(bool required = true)
                {
                    Builder.Required(required, ConfigurationSource.Explicit);

                    return this;
                }
            }
        }

        public class EntityBuilder<TEntity> : EntityBuilder, IEntityBuilder<TEntity, EntityBuilder<TEntity>> where TEntity : class
        {
            public EntityBuilder([NotNull] InternalEntityBuilder builder)
                : base(builder)
            {
            }

            public new virtual EntityBuilder<TEntity> Annotation(string annotation, string value)
            {
                base.Annotation(annotation, value);

                return this;
            }

            Model IMetadataBuilder<EntityType, EntityBuilder<TEntity>>.Model
            {
                get { return Builder.ModelBuilder.Metadata; }
            }

            public virtual KeyBuilder Key([NotNull] Expression<Func<TEntity, object>> keyExpression)
            {
                Check.NotNull(keyExpression, "keyExpression");

                return new KeyBuilder(Builder.Key(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
            }

            public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
            {
                Check.NotNull(propertyExpression, "propertyExpression");

                var propertyInfo = propertyExpression.GetPropertyAccess();
                return new PropertyBuilder(Builder.Property(propertyInfo, ConfigurationSource.Explicit));
            }

            public virtual void Ignore([NotNull] Expression<Func<TEntity, object>> propertyExpression)
            {
                Check.NotNull(propertyExpression, "propertyExpression");

                var propertyName = propertyExpression.GetPropertyAccess().Name;
                Builder.Ignore(propertyName, ConfigurationSource.Explicit);
            }

            public virtual ForeignKeyBuilder ForeignKey<TReferencedEntityType>([NotNull] Expression<Func<TEntity, object>> foreignKeyExpression)
            {
                Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                return new ForeignKeyBuilder(Builder.ForeignKey(typeof(TReferencedEntityType), foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
            }

            public virtual IndexBuilder Index([NotNull] Expression<Func<TEntity, object>> indexExpression)
            {
                Check.NotNull(indexExpression, "indexExpression");

                return new IndexBuilder(Builder.Index(indexExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
            }

            public virtual OneToManyBuilder<TRelatedEntity> OneToMany<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null,
                [CanBeNull] Expression<Func<TRelatedEntity, TEntity>> reference = null)
            {
                return new OneToManyBuilder<TRelatedEntity>(BuildRelationship(collection, reference));
            }

            public virtual ManyToOneBuilder<TRelatedEntity> ManyToOne<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, TRelatedEntity>> reference = null,
                [CanBeNull] Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> collection = null)
            {
                return new ManyToOneBuilder<TRelatedEntity>(BuildRelationship(collection, reference));
            }

            private InternalRelationshipBuilder BuildRelationship<TPrincipalEntity, TDependentEntity>(
                Expression<Func<TPrincipalEntity, IEnumerable<TDependentEntity>>> collection,
                Expression<Func<TDependentEntity, TPrincipalEntity>> reference)
            {
                // Find either navigation that already exists
                var navNameToDependent = collection != null ? collection.GetPropertyAccess().Name : null;
                var navNameToPrincipal = reference != null ? reference.GetPropertyAccess().Name : null;

                return Builder.Relationship(
                    typeof(TPrincipalEntity), typeof(TDependentEntity), navNameToPrincipal, navNameToDependent, /*oneToOne:*/ false, ConfigurationSource.Explicit);
            }

            public virtual OneToOneBuilder OneToOne<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, TRelatedEntity>> navigationToDependent = null,
                [CanBeNull] Expression<Func<TRelatedEntity, TEntity>> navigationToPrincipal = null)
            {
                // Find either navigation that already exists
                var navNameToDependent = navigationToDependent != null ? navigationToDependent.GetPropertyAccess().Name : null;
                var navNameToPrincipal = navigationToPrincipal != null ? navigationToPrincipal.GetPropertyAccess().Name : null;

                return new OneToOneBuilder(Builder.Relationship(
                    typeof(TEntity), typeof(TRelatedEntity), navNameToPrincipal, navNameToDependent, /*oneToOne:*/ true, ConfigurationSource.Explicit));
            }

            public class OneToManyBuilder<TRelatedEntity> : OneToManyBuilder
            {
                public OneToManyBuilder([NotNull] InternalRelationshipBuilder builder)
                    : base(builder)
                {
                }

                public virtual OneToManyBuilder<TRelatedEntity> ForeignKey(
                    [NotNull] Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public virtual OneToManyBuilder<TRelatedEntity> ReferencedKey(
                    [NotNull] Expression<Func<TEntity, object>> keyExpression)
                {
                    Check.NotNull(keyExpression, "keyExpression");

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ReferencedKey(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public new virtual OneToManyBuilder<TRelatedEntity> Annotation([NotNull] string annotation, [NotNull] string value)
                {
                    Check.NotEmpty(annotation, "annotation");
                    Check.NotEmpty(value, "value");

                    return (OneToManyBuilder<TRelatedEntity>)base.Annotation(annotation, value);
                }

                public new virtual OneToManyBuilder<TRelatedEntity> ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, "foreignKeyPropertyNames");

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public new virtual OneToManyBuilder<TRelatedEntity> ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, "keyPropertyNames");

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                public new virtual OneToManyBuilder<TRelatedEntity> Required(bool required = true)
                {
                    return (OneToManyBuilder<TRelatedEntity>)base.Required(required);
                }
            }

            public class ManyToOneBuilder<TRelatedEntity> : ManyToOneBuilder
            {
                public ManyToOneBuilder([NotNull] InternalRelationshipBuilder builder)
                    : base(builder)
                {
                }

                public virtual ManyToOneBuilder<TRelatedEntity> ForeignKey(
                    [NotNull] Expression<Func<TEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public virtual ManyToOneBuilder<TRelatedEntity> ReferencedKey(
                    [NotNull] Expression<Func<TRelatedEntity, object>> keyExpression)
                {
                    Check.NotNull(keyExpression, "keyExpression");

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ReferencedKey(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public new virtual ManyToOneBuilder<TRelatedEntity> Annotation([NotNull] string annotation, [NotNull] string value)
                {
                    Check.NotEmpty(annotation, "annotation");
                    Check.NotEmpty(value, "value");

                    return (ManyToOneBuilder<TRelatedEntity>)base.Annotation(annotation, value);
                }

                public new virtual ManyToOneBuilder<TRelatedEntity> ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, "foreignKeyPropertyNames");

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public new virtual ManyToOneBuilder<TRelatedEntity> ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, "keyPropertyNames");

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                public new virtual ManyToOneBuilder<TRelatedEntity> Required(bool required = true)
                {
                    return (ManyToOneBuilder<TRelatedEntity>)base.Required(required);
                }
            }
        }
    }
}

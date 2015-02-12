// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class ModelBuilder : IModelBuilder<ModelBuilder>
    {
        private readonly InternalModelBuilder _builder;

        // TODO: Configure property facets, foreign keys & navigation properties
        // Issue #213

        public ModelBuilder()
            : this(new Model())
        {
        }

        public ModelBuilder([NotNull] Model model)
            : this(model, new ConventionSet())
        {
            Check.NotNull(model, nameof(model));
        }

        public ModelBuilder([NotNull] Model model, [NotNull] ConventionSet conventions)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(conventions, nameof(conventions));

            _builder = new InternalModelBuilder(model, conventions);
        }

        protected internal ModelBuilder([NotNull] InternalModelBuilder internalBuilder)
        {
            Check.NotNull(internalBuilder, nameof(internalBuilder));

            _builder = internalBuilder;
        }

        public virtual Model Metadata => Builder.Metadata;

        public virtual Model Model => Metadata;

        public virtual ModelBuilder Annotation(string annotation, string value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotEmpty(value, nameof(value));

            _builder.Annotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        protected virtual InternalModelBuilder Builder => _builder;

        public virtual EntityBuilder<TEntity> Entity<TEntity>() where TEntity : class
        {
            return new EntityBuilder<TEntity>(Builder.Entity(typeof(TEntity), ConfigurationSource.Explicit));
        }

        public virtual EntityBuilder Entity([NotNull] Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return new EntityBuilder(Builder.Entity(entityType, ConfigurationSource.Explicit));
        }

        public virtual EntityBuilder Entity([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return new EntityBuilder(Builder.Entity(name, ConfigurationSource.Explicit));
        }

        public virtual ModelBuilder Entity<TEntity>([NotNull] Action<EntityBuilder<TEntity>> entityBuilder) where TEntity : class
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity<TEntity>());

            return this;
        }

        public virtual ModelBuilder Entity([NotNull] Type entityType, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity(entityType));

            return this;
        }

        public virtual ModelBuilder Entity([NotNull] string name, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity(name));

            return this;
        }

        public virtual void Ignore<TEntity>() where TEntity : class
        {
            Ignore(typeof(TEntity));
        }

        public virtual void Ignore([NotNull] Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            Builder.Ignore(entityType, ConfigurationSource.Explicit);
        }

        public virtual void Ignore([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Builder.Ignore(name, ConfigurationSource.Explicit);
        }

        public class EntityBuilder : IEntityBuilder<EntityBuilder>
        {
            public EntityBuilder([NotNull] InternalEntityBuilder builder)
            {
                Check.NotNull(builder, nameof(builder));

                Builder = builder;
            }

            protected virtual InternalEntityBuilder Builder { get; }

            public virtual EntityType Metadata => Builder.Metadata;

            Model IMetadataBuilder<EntityType, EntityBuilder>.Model => Builder.ModelBuilder.Metadata;

            public virtual EntityBuilder Annotation(string annotation, string value)
            {
                Check.NotEmpty(annotation, nameof(annotation));
                Check.NotEmpty(value, nameof(value));

                Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                return this;
            }

            public virtual KeyBuilder Key([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, nameof(propertyNames));

                return new KeyBuilder(Builder.PrimaryKey(propertyNames, ConfigurationSource.Explicit));
            }

            public virtual PropertyBuilder Property<TProperty>([NotNull] string propertyName)
            {
                Check.NotEmpty(propertyName, nameof(propertyName));

                return Property(typeof(TProperty), propertyName);
            }

            public virtual PropertyBuilder Property([NotNull] Type propertyType, [NotNull] string propertyName)
            {
                Check.NotNull(propertyType, nameof(propertyType));
                Check.NotEmpty(propertyName, nameof(propertyName));

                return new PropertyBuilder(Builder.Property(propertyType, propertyName, ConfigurationSource.Explicit));
            }

            public virtual void Ignore([NotNull] string propertyName)
            {
                Check.NotEmpty(propertyName, nameof(propertyName));

                Builder.Ignore(propertyName, ConfigurationSource.Explicit);
            }

            public virtual IndexBuilder Index([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, nameof(propertyNames));

                return new IndexBuilder(Builder.Index(propertyNames, ConfigurationSource.Explicit));
            }

            public virtual ReferenceNavigationBuilder HasOne(
                [NotNull] Type relatedType,
                [CanBeNull] string reference = null)
            {
                Check.NotNull(relatedType, nameof(relatedType));

                var relatedEntityType = Builder.ModelBuilder.Entity(relatedType, ConfigurationSource.Explicit).Metadata;

                return new ReferenceNavigationBuilder(
                    relatedEntityType,
                    reference,
                    Builder.Relationship(
                        relatedEntityType,
                        Metadata,
                        reference ?? "",
                        navigationToDependentName: null,
                        configurationSource: ConfigurationSource.Explicit,
                        strictPrincipal: false));
            }

            public virtual CollectionNavigationBuilder HasMany(
                [NotNull] Type relatedEntityType,
                [CanBeNull] string collection = null)
            {
                Check.NotNull(relatedEntityType, nameof(relatedEntityType));

                return new CollectionNavigationBuilder(
                    collection ?? "",
                    Builder.Relationship(
                        Metadata,
                        Builder.ModelBuilder.Entity(relatedEntityType, ConfigurationSource.Explicit).Metadata,
                        null,
                        collection ?? "",
                        ConfigurationSource.Explicit,
                        isUnique: false));
            }

            public virtual ReferenceNavigationBuilder HasOne(
                [NotNull] string relatedEntityTypeName,
                [CanBeNull] string reference = null)
            {
                Check.NotEmpty(relatedEntityTypeName, nameof(relatedEntityTypeName));

                var relatedEntityType = Builder.ModelBuilder.Metadata.GetEntityType(relatedEntityTypeName);

                return new ReferenceNavigationBuilder(
                    relatedEntityType,
                    reference,
                    Builder.Relationship(
                        relatedEntityType,
                        Metadata,
                        reference ?? "",
                        navigationToDependentName: null,
                        configurationSource: ConfigurationSource.Explicit,
                        strictPrincipal: false));
            }

            public virtual CollectionNavigationBuilder HasMany(
                [NotNull] string relatedEntityTypeName,
                [CanBeNull] string collection = null)
            {
                Check.NotEmpty(relatedEntityTypeName, nameof(relatedEntityTypeName));

                return new CollectionNavigationBuilder(
                    collection ?? "",
                    Builder.Relationship(
                        Metadata,
                        Builder.ModelBuilder.Metadata.GetEntityType(relatedEntityTypeName),
                        null,
                        collection ?? "",
                        ConfigurationSource.Explicit,
                        isUnique: false));
            }

            public class KeyBuilder : IKeyBuilder<KeyBuilder>
            {
                public KeyBuilder([NotNull] InternalKeyBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                protected virtual InternalKeyBuilder Builder { get; }

                public virtual Key Metadata => Builder.Metadata;

                Model IMetadataBuilder<Key, KeyBuilder>.Model => Builder.ModelBuilder.Metadata;

                public virtual KeyBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }
            }

            public class PropertyBuilder : IPropertyBuilder<PropertyBuilder>
            {
                public PropertyBuilder([NotNull] InternalPropertyBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                protected virtual InternalPropertyBuilder Builder { get; }

                public virtual Property Metadata => Builder.Metadata;

                Model IMetadataBuilder<Property, PropertyBuilder>.Model => Builder.ModelBuilder.Metadata;

                public virtual PropertyBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

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

            public class IndexBuilder : IIndexBuilder<IndexBuilder>
            {
                public IndexBuilder([NotNull] InternalIndexBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                protected virtual InternalIndexBuilder Builder { get; }

                public virtual Index Metadata => Builder.Metadata;

                Model IMetadataBuilder<Index, IndexBuilder>.Model => Builder.ModelBuilder.Metadata;

                public virtual IndexBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual IndexBuilder IsUnique(bool isUnique = true)
                {
                    Builder.IsUnique(isUnique, ConfigurationSource.Explicit);

                    return this;
                }
            }

            public class ReferenceNavigationBuilder
            {
                public ReferenceNavigationBuilder(
                    [NotNull] EntityType relatedEntityType,
                    [CanBeNull] string reference,
                    [NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(relatedEntityType, nameof(relatedEntityType));
                    Check.NotNull(builder, nameof(builder));

                    RelatedEntityType = relatedEntityType;
                    Reference = reference;
                    Builder = builder;
                }

                protected string Reference { get; set; }

                protected EntityType RelatedEntityType { get; set; }

                public virtual ForeignKey Metadata => Builder.Metadata;

                protected virtual InternalRelationshipBuilder Builder { get; }

                public virtual ManyToOneBuilder WithMany([CanBeNull] string collection = null) => new ManyToOneBuilder(WithManyBuilder(collection));

                protected InternalRelationshipBuilder WithManyBuilder(string collection)
                {
                    var needToInvert = Metadata.ReferencedEntityType != RelatedEntityType;
                    Debug.Assert((needToInvert && Metadata.EntityType == RelatedEntityType)
                                 || Metadata.ReferencedEntityType == RelatedEntityType);

                    var builder = Builder;
                    if (needToInvert)
                    {
                        builder = builder.Invert(ConfigurationSource.Explicit);
                    }

                    if (((IForeignKey)Metadata).IsUnique)
                    {
                        builder = builder.NavigationToDependent(null, ConfigurationSource.Explicit);
                    }

                    builder = builder.Unique(false, ConfigurationSource.Explicit);

                    return builder.NavigationToDependent(collection, ConfigurationSource.Explicit, strictPreferExisting: true);
                }

                public virtual OneToOneBuilder WithOne([CanBeNull] string inverseReference = null) => new OneToOneBuilder(WithOneBuilder(inverseReference));

                protected InternalRelationshipBuilder WithOneBuilder(string inverseReferenceName)
                {
                    var inverseToPrincipal = Metadata.EntityType == RelatedEntityType
                                             && (string.IsNullOrEmpty(Reference) || Metadata.GetNavigationToDependent()?.Name == Reference);

                    Debug.Assert(inverseToPrincipal
                                 || (Metadata.ReferencedEntityType == RelatedEntityType
                                     && (string.IsNullOrEmpty(Reference) || Metadata.GetNavigationToPrincipal()?.Name == Reference)));

                    var builder = Builder;
                    if (!((IForeignKey)Metadata).IsUnique)
                    {
                        Debug.Assert(!inverseToPrincipal);

                        builder = builder.NavigationToDependent(null, ConfigurationSource.Explicit);
                    }

                    builder = builder.Unique(true, ConfigurationSource.Explicit);

                    builder = inverseToPrincipal
                        ? builder.NavigationToPrincipal(inverseReferenceName, ConfigurationSource.Explicit, strictPreferExisting: false)
                        : builder.NavigationToDependent(inverseReferenceName, ConfigurationSource.Explicit, strictPreferExisting: false);

                    return builder;
                }
            }

            public class CollectionNavigationBuilder
            {
                public CollectionNavigationBuilder(
                    [CanBeNull] string collection,
                    [NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Collection = collection;
                    Builder = builder;
                }

                protected string Collection { get; set; }

                public virtual ForeignKey Metadata => Builder.Metadata;

                protected virtual InternalRelationshipBuilder Builder { get; }

                public virtual OneToManyBuilder WithOne([CanBeNull] string reference = null) => new OneToManyBuilder(WithOneBuilder(reference));

                protected InternalRelationshipBuilder WithOneBuilder(string reference) => Builder.NavigationToPrincipal(
                    reference,
                    ConfigurationSource.Explicit,
                    strictPreferExisting: true);
            }

            public class OneToManyBuilder : IOneToManyBuilder<OneToManyBuilder>
            {
                public OneToManyBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                public virtual ForeignKey Metadata => Builder.Metadata;

                Model IMetadataBuilder<ForeignKey, OneToManyBuilder>.Model => Builder.ModelBuilder.Metadata;

                protected virtual InternalRelationshipBuilder Builder { get; }

                public virtual OneToManyBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual OneToManyBuilder ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new OneToManyBuilder(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToManyBuilder ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new OneToManyBuilder(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToManyBuilder Required(bool required = true)
                {
                    return new OneToManyBuilder(Builder.Required(required, ConfigurationSource.Explicit));
                }
            }

            public class ManyToOneBuilder : IManyToOneBuilder<ManyToOneBuilder>
            {
                public ManyToOneBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                protected virtual InternalRelationshipBuilder Builder { get; }

                public virtual ForeignKey Metadata => Builder.Metadata;

                Model IMetadataBuilder<ForeignKey, ManyToOneBuilder>.Model => Builder.ModelBuilder.Metadata;

                public virtual ManyToOneBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual ManyToOneBuilder ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new ManyToOneBuilder(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual ManyToOneBuilder ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new ManyToOneBuilder(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual ManyToOneBuilder Required(bool required = true)
                    => new ManyToOneBuilder(Builder.Required(required, ConfigurationSource.Explicit));
            }

            public class OneToOneBuilder : IOneToOneBuilder<OneToOneBuilder>
            {
                public OneToOneBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                protected virtual InternalRelationshipBuilder Builder { get; }

                public virtual ForeignKey Metadata => Builder.Metadata;

                Model IMetadataBuilder<ForeignKey, OneToOneBuilder>.Model => Builder.ModelBuilder.Metadata;

                public virtual OneToOneBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual OneToOneBuilder ForeignKey(
                    [NotNull] Type dependentEntityType,
                    [NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(dependentEntityType, nameof(dependentEntityType));
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new OneToOneBuilder(Builder.ForeignKey(dependentEntityType, foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ReferencedKey(
                    [NotNull] Type principalEntityType,
                    [NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(principalEntityType, nameof(principalEntityType));
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new OneToOneBuilder(Builder.ReferencedKey(principalEntityType, keyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ForeignKey(
                    [NotNull] string dependentEntityTypeName,
                    [NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(dependentEntityTypeName, nameof(dependentEntityTypeName));
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new OneToOneBuilder(Builder.ForeignKey(dependentEntityTypeName, foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ReferencedKey(
                    [NotNull] string principalEntityTypeName,
                    [NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(principalEntityTypeName, nameof(principalEntityTypeName));
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new OneToOneBuilder(Builder.ReferencedKey(principalEntityTypeName, keyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ForeignKey<TDependentEntity>(
                    [NotNull] Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression));

                    return new OneToOneBuilder(
                        Builder.ForeignKey(typeof(TDependentEntity), foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ReferencedKey<TPrincipalEntity>(
                    [NotNull] Expression<Func<TPrincipalEntity, object>> keyExpression)
                {
                    Check.NotNull(keyExpression, nameof(keyExpression));

                    return new OneToOneBuilder(Builder.ReferencedKey(typeof(TPrincipalEntity), keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder Required(bool required = true)
                {
                    return new OneToOneBuilder(Builder.Required(required, ConfigurationSource.Explicit));
                }
            }
        }

        public class EntityBuilder<TEntity> : EntityBuilder, IEntityBuilder<TEntity, EntityBuilder<TEntity>>
            where TEntity : class
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

            Model IMetadataBuilder<EntityType, EntityBuilder<TEntity>>.Model => Builder.ModelBuilder.Metadata;

            public virtual KeyBuilder Key([NotNull] Expression<Func<TEntity, object>> keyExpression)
            {
                Check.NotNull(keyExpression, nameof(keyExpression));

                return new KeyBuilder(Builder.PrimaryKey(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
            }

            public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
            {
                Check.NotNull(propertyExpression, nameof(propertyExpression));

                var propertyInfo = propertyExpression.GetPropertyAccess();
                return new PropertyBuilder(Builder.Property(propertyInfo, ConfigurationSource.Explicit));
            }

            public virtual void Ignore([NotNull] Expression<Func<TEntity, object>> propertyExpression)
            {
                Check.NotNull(propertyExpression, nameof(propertyExpression));

                var propertyName = propertyExpression.GetPropertyAccess().Name;
                Builder.Ignore(propertyName, ConfigurationSource.Explicit);
            }

            public virtual IndexBuilder Index([NotNull] Expression<Func<TEntity, object>> indexExpression)
            {
                Check.NotNull(indexExpression, nameof(indexExpression));

                return new IndexBuilder(Builder.Index(indexExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
            }

            public virtual ReferenceNavigationBuilder<TRelatedEntity> HasOne<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, TRelatedEntity>> reference = null)
            {
                var relatedEntityType = Builder.ModelBuilder.Entity(typeof(TRelatedEntity), ConfigurationSource.Explicit).Metadata;
                var referenceName = reference?.GetPropertyAccess().Name ?? "";

                return new ReferenceNavigationBuilder<TRelatedEntity>(
                    relatedEntityType,
                    referenceName,
                    Builder.Relationship(
                        relatedEntityType,
                        Metadata,
                        referenceName,
                        navigationToDependentName: null,
                        configurationSource: ConfigurationSource.Explicit,
                        strictPrincipal: false));
            }

            public virtual CollectionNavigationBuilder<TRelatedEntity> HasMany<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null)
            {
                var collectionName = collection?.GetPropertyAccess().Name ?? "";

                return new CollectionNavigationBuilder<TRelatedEntity>(
                    collectionName,
                    Builder.Relationship(
                        typeof(TEntity),
                        typeof(TRelatedEntity),
                        null,
                        collectionName,
                        configurationSource: ConfigurationSource.Explicit,
                        isUnique: false));
            }

            public class ReferenceNavigationBuilder<TRelatedEntity> : ReferenceNavigationBuilder
            {
                public ReferenceNavigationBuilder(
                    [NotNull] EntityType relatedEntityType,
                    [CanBeNull] string reference,
                    [NotNull] InternalRelationshipBuilder builder)
                    : base(relatedEntityType, reference, builder)
                {
                }

                public virtual ManyToOneBuilder<TRelatedEntity> WithMany(
                    [CanBeNull] Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> collection = null)
                    => new ManyToOneBuilder<TRelatedEntity>(WithManyBuilder(collection?.GetPropertyAccess().Name));

                public virtual OneToOneBuilder WithOne([CanBeNull] Expression<Func<TRelatedEntity, TEntity>> inverseReference = null)
                    => new OneToOneBuilder(WithOneBuilder(inverseReference?.GetPropertyAccess().Name));
            }

            public class CollectionNavigationBuilder<TRelatedEntity> : CollectionNavigationBuilder
            {
                public CollectionNavigationBuilder(
                    [CanBeNull] string collection,
                    [NotNull] InternalRelationshipBuilder builder)
                    : base(collection, builder)
                {
                }

                public virtual OneToManyBuilder<TRelatedEntity> WithOne([CanBeNull] Expression<Func<TRelatedEntity, TEntity>> reference = null)
                    => new OneToManyBuilder<TRelatedEntity>(WithOneBuilder(reference?.GetPropertyAccess().Name));
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
                    Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression));

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public virtual OneToManyBuilder<TRelatedEntity> ReferencedKey(
                    [NotNull] Expression<Func<TEntity, object>> keyExpression)
                {
                    Check.NotNull(keyExpression, nameof(keyExpression));

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ReferencedKey(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public new virtual OneToManyBuilder<TRelatedEntity> Annotation([NotNull] string annotation, [NotNull] string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    return (OneToManyBuilder<TRelatedEntity>)base.Annotation(annotation, value);
                }

                public new virtual OneToManyBuilder<TRelatedEntity> ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public new virtual OneToManyBuilder<TRelatedEntity> ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                public new virtual OneToManyBuilder<TRelatedEntity> Required(bool required = true)
                    => new OneToManyBuilder<TRelatedEntity>(Builder.Required(required, ConfigurationSource.Explicit));
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
                    Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression));

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public virtual ManyToOneBuilder<TRelatedEntity> ReferencedKey(
                    [NotNull] Expression<Func<TRelatedEntity, object>> keyExpression)
                {
                    Check.NotNull(keyExpression, nameof(keyExpression));

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ReferencedKey(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public new virtual ManyToOneBuilder<TRelatedEntity> Annotation([NotNull] string annotation, [NotNull] string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    return (ManyToOneBuilder<TRelatedEntity>)base.Annotation(annotation, value);
                }

                public new virtual ManyToOneBuilder<TRelatedEntity> ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public new virtual ManyToOneBuilder<TRelatedEntity> ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                public new virtual ManyToOneBuilder<TRelatedEntity> Required(bool required = true)
                    => new ManyToOneBuilder<TRelatedEntity>(Builder.Required(required, ConfigurationSource.Explicit));
            }
        }
    }
}

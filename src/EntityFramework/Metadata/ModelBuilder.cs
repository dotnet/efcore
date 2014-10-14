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
        private readonly IList<IModelConvention> _conventions;

        public ModelBuilder()
            : this(new Model())
        {
        }

        public ModelBuilder([NotNull] Model model)
        {
            Check.NotNull(model, "model");

            _builder = new InternalModelBuilder(model, this);
            _conventions = new List<IModelConvention>
                {
                    new PropertiesConvention(),
                    new KeyConvention()
                };
        }

        protected ModelBuilder([NotNull] Model model, [NotNull] IList<IModelConvention> conventions)
        {
            Check.NotNull(model, "model");
            Check.NotNull(conventions, "conventions");

            _builder = new InternalModelBuilder(model, this);
            _conventions = conventions;
        }

        protected internal ModelBuilder([NotNull] InternalModelBuilder internalBuilder)
        {
            Check.NotNull(internalBuilder, "internalBuilder");

            _builder = internalBuilder;
        }

        // TODO: Consider whether these conversions are useful
        // Issue #750
        public static explicit operator BasicModelBuilder([NotNull] ModelBuilder builder)
        {
            Check.NotNull(builder, "builder");

            return new BasicModelBuilder(builder.Builder);
        }

        public virtual Model Metadata
        {
            get { return Builder.Metadata; }
        }

        // TODO: Consider removing and just using the Metadata property
        public virtual Model Model
        {
            get { return Metadata; }
        }

        public virtual IList<IModelConvention> Conventions
        {
            get { return _conventions; }
        }

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

        protected virtual void OnEntityTypeAdded([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            foreach (var convention in Conventions)
            {
                convention.Apply(entityType);
            }
        }

        void IModelChangeListener.OnEntityTypeAdded(EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            OnEntityTypeAdded(entityType);
        }

        public virtual EntityBuilder<T> Entity<T>()
        {
            return new EntityBuilder<T>(Builder.Entity(typeof(T), ConfigurationSource.Explicit));
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

        public virtual ModelBuilder Entity<T>([NotNull] Action<EntityBuilder<T>> entityBuilder)
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity<T>());

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

        public class EntityBuilder : IEntityBuilder<EntityBuilder>
        {
            private readonly InternalEntityBuilder _builder;

            public EntityBuilder([NotNull] InternalEntityBuilder builder)
            {
                Check.NotNull(builder, "builder");

                _builder = builder;
            }

            protected virtual InternalEntityBuilder Builder
            {
                get { return _builder; }
            }

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

            public virtual PropertyBuilder Property<TProperty>([NotNull] string name)
            {
                Check.NotEmpty(name, "name");

                return Property(typeof(TProperty), name);
            }

            public virtual PropertyBuilder Property([NotNull] Type propertyType, [NotNull] string name)
            {
                Check.NotNull(propertyType, "propertyType");
                Check.NotEmpty(name, "name");

                return new PropertyBuilder(Builder.Property(propertyType, name, ConfigurationSource.Explicit));
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

                return new OneToManyBuilder(Builder.BuildRelationship(
                    Metadata, Builder.ModelBuilder.Entity(relatedEntityType, ConfigurationSource.Explicit).Metadata, reference, collection, /*oneToOne:*/ false, ConfigurationSource.Explicit));
            }

            public virtual ManyToOneBuilder ManyToOne(
                [NotNull] Type relatedEntityType,
                [CanBeNull] string reference = null,
                [CanBeNull] string collection = null)
            {
                Check.NotNull(relatedEntityType, "relatedEntityType");

                return new ManyToOneBuilder(Builder.BuildRelationship(
                    Builder.ModelBuilder.Entity(relatedEntityType, ConfigurationSource.Explicit).Metadata, Metadata, reference, collection, /*oneToOne:*/ false, ConfigurationSource.Explicit));
            }

            public virtual OneToOneBuilder OneToOne(
                [NotNull] Type relatedEntityType,
                [CanBeNull] string reference = null,
                [CanBeNull] string inverse = null)
            {
                Check.NotNull(relatedEntityType, "relatedEntityType");

                return new OneToOneBuilder(Builder.BuildRelationship(
                    Metadata, Builder.ModelBuilder.Entity(relatedEntityType, ConfigurationSource.Explicit).Metadata, inverse, reference, /*oneToOne:*/ true, ConfigurationSource.Explicit));
            }

            public virtual OneToManyBuilder OneToMany(
                [NotNull] string relatedEntityTypeName,
                [CanBeNull] string collection = null,
                [CanBeNull] string reference = null)
            {
                Check.NotEmpty(relatedEntityTypeName, "relatedEntityTypeName");

                return new OneToManyBuilder(Builder.BuildRelationship(
                    Metadata, Builder.ModelBuilder.Metadata.GetEntityType(relatedEntityTypeName), reference, collection, /*oneToOne:*/ false, ConfigurationSource.Explicit));
            }

            public virtual ManyToOneBuilder ManyToOne(
                [NotNull] string relatedEntityTypeName,
                [CanBeNull] string reference = null,
                [CanBeNull] string collection = null)
            {
                Check.NotEmpty(relatedEntityTypeName, "relatedEntityTypeName");

                return new ManyToOneBuilder(Builder.BuildRelationship(
                    Builder.ModelBuilder.Metadata.GetEntityType(relatedEntityTypeName), Metadata, reference, collection, /*oneToOne:*/ false, ConfigurationSource.Explicit));
            }

            public virtual OneToOneBuilder OneToOne(
                [NotNull] string relatedEntityTypeName,
                [CanBeNull] string reference = null,
                [CanBeNull] string inverse = null)
            {
                Check.NotEmpty(relatedEntityTypeName, "relatedEntityTypeName");

                return new OneToOneBuilder(Builder.BuildRelationship(
                    Metadata, Builder.ModelBuilder.Metadata.GetEntityType(relatedEntityTypeName), inverse, reference, /*oneToOne:*/ true, ConfigurationSource.Explicit));
            }

            public class KeyBuilder : IKeyBuilder<KeyBuilder>
            {
                private readonly InternalKeyBuilder _builder;

                public KeyBuilder([NotNull] InternalKeyBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    _builder = builder;
                }

                protected virtual InternalKeyBuilder Builder
                {
                    get { return _builder; }
                }

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
                private readonly InternalPropertyBuilder _builder;

                public PropertyBuilder([NotNull] InternalPropertyBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    _builder = builder;
                }

                protected virtual InternalPropertyBuilder Builder
                {
                    get { return _builder; }
                }

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
                    Builder.Required(isRequired);

                    return this;
                }

                public virtual PropertyBuilder MaxLength(int maxLength)
                {
                    Builder.MaxLength(maxLength);

                    return this;
                }

                public virtual PropertyBuilder ConcurrencyToken(bool isConcurrencyToken = true)
                {
                    Builder.ConcurrencyToken(isConcurrencyToken);

                    return this;
                }

                public virtual PropertyBuilder Shadow(bool isShadowProperty = true)
                {
                    Builder.Shadow(isShadowProperty);

                    return this;
                }

                public virtual PropertyBuilder GenerateValuesOnAdd(bool generateValues = true)
                {
                    Builder.GenerateValuesOnAdd(generateValues);

                    return this;
                }

                public virtual PropertyBuilder StoreComputed(bool computed = true)
                {
                    Builder.StoreComputed(computed);

                    return this;
                }

                public virtual PropertyBuilder UseStoreDefault(bool useDefault = true)
                {
                    Builder.UseStoreDefault(useDefault);

                    return this;
                }
            }

            public class ForeignKeyBuilder : IForeignKeyBuilder<ForeignKeyBuilder>
            {
                private readonly InternalForeignKeyBuilder _builder;

                public ForeignKeyBuilder([NotNull] InternalForeignKeyBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    _builder = builder;
                }

                protected virtual InternalForeignKeyBuilder Builder
                {
                    get { return _builder; }
                }

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
                    Builder.IsUnique(isUnique);

                    return this;
                }
            }

            public class IndexBuilder : IIndexBuilder<IndexBuilder>
            {
                private readonly InternalIndexBuilder _builder;

                public IndexBuilder([NotNull] InternalIndexBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    _builder = builder;
                }

                protected virtual InternalIndexBuilder Builder
                {
                    get { return _builder; }
                }

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
                    Builder.IsUnique(isUnique);

                    return this;
                }
            }

            public class OneToManyBuilder : IOneToManyBuilder<OneToManyBuilder>
            {
                private readonly InternalRelationshipBuilder _builder;

                public OneToManyBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    _builder = builder;
                }

                public virtual ForeignKey Metadata
                {
                    get { return Builder.Metadata; }
                }

                Model IMetadataBuilder<ForeignKey, OneToManyBuilder>.Model
                {
                    get { return Builder.ModelBuilder.Metadata; }
                }

                protected virtual InternalRelationshipBuilder Builder
                {
                    get { return _builder; }
                }

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
                    Builder.Required(required);

                    return this;
                }
            }

            public class ManyToOneBuilder : IManyToOneBuilder<ManyToOneBuilder>
            {
                private readonly InternalRelationshipBuilder _builder;

                public ManyToOneBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    _builder = builder;
                }

                protected virtual InternalRelationshipBuilder Builder
                {
                    get { return _builder; }
                }

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
                    Builder.Required(required);

                    return this;
                }
            }

            public class OneToOneBuilder : IOneToOneBuilder<OneToOneBuilder>
            {
                private readonly InternalRelationshipBuilder _builder;

                public OneToOneBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, "builder");

                    _builder = builder;
                }

                protected virtual InternalRelationshipBuilder Builder
                {
                    get { return _builder; }
                }

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

                    return new OneToOneBuilder(Builder.OneToOneForeignKey(dependentEntityType, foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ReferencedKey(
                    [NotNull] Type principalEntityType,
                    [NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(principalEntityType, "principalEntityType");
                    Check.NotNull(keyPropertyNames, "keyPropertyNames");

                    return new OneToOneBuilder(Builder.OneToOneReferencedKey(principalEntityType, keyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ForeignKey(
                    [NotNull] string dependentEntityTypeName,
                    [NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(dependentEntityTypeName, "dependentEntityTypeName");
                    Check.NotNull(foreignKeyPropertyNames, "foreignKeyPropertyNames");

                    return new OneToOneBuilder(Builder.OneToOneForeignKey(dependentEntityTypeName, foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ReferencedKey(
                    [NotNull] string principalEntityTypeName,
                    [NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(principalEntityTypeName, "principalEntityTypeName");
                    Check.NotNull(keyPropertyNames, "keyPropertyNames");

                    return new OneToOneBuilder(Builder.OneToOneReferencedKey(principalEntityTypeName, keyPropertyNames, ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ForeignKey<TDependentEntity>(
                    [NotNull] Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    return new OneToOneBuilder(
                        Builder.OneToOneForeignKey(typeof(TDependentEntity), foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder ReferencedKey<TPrincipalEntity>(
                    [NotNull] Expression<Func<TPrincipalEntity, object>> keyExpression)
                {
                    Check.NotNull(keyExpression, "keyExpression");

                    return new OneToOneBuilder(Builder.OneToOneReferencedKey(typeof(TPrincipalEntity), keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                public virtual OneToOneBuilder Required(bool required = true)
                {
                    Builder.Required(required);

                    return this;
                }
            }
        }

        public class EntityBuilder<TEntity> : EntityBuilder, IEntityBuilder<TEntity, EntityBuilder<TEntity>>
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

                return new PropertyBuilder(Builder.Property(propertyExpression.GetPropertyAccess(), ConfigurationSource.Explicit));
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

                return Builder.BuildRelationship(
                    typeof(TPrincipalEntity), typeof(TDependentEntity), navNameToPrincipal, navNameToDependent, /*oneToOne:*/ false, ConfigurationSource.Explicit);
            }

            public virtual OneToOneBuilder OneToOne<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, TRelatedEntity>> reference = null,
                [CanBeNull] Expression<Func<TRelatedEntity, TEntity>> inverse = null)
            {
                // Find either navigation that already exists
                var navNameToDependent = reference != null ? reference.GetPropertyAccess().Name : null;
                var navNameToPrincipal = inverse != null ? inverse.GetPropertyAccess().Name : null;

                return new OneToOneBuilder(Builder.BuildRelationship(
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

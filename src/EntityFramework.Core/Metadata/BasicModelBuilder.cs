// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class BasicModelBuilder : IModelBuilder<BasicModelBuilder>
    {
        private readonly InternalModelBuilder _builder;

        public BasicModelBuilder()
            : this(new Model())
        {
        }

        public BasicModelBuilder([NotNull] Model model)
        {
            Check.NotNull(model, "model");

            _builder = new InternalModelBuilder(model, new ConventionsDispatcher());
        }

        protected internal BasicModelBuilder([NotNull] InternalModelBuilder internalBuilder)
        {
            Check.NotNull(internalBuilder, "internalBuilder");

            _builder = internalBuilder;
        }

        public virtual Model Model
        {
            get { return Metadata; }
        }

        public virtual Model Metadata
        {
            get { return Builder.Metadata; }
        }

        public virtual BasicModelBuilder Annotation(string annotation, string value)
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

        public virtual BasicModelBuilder Entity<TEntity>([NotNull] Action<EntityBuilder<TEntity>> entityBuilder) where TEntity : class
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity<TEntity>());

            return this;
        }

        public virtual BasicModelBuilder Entity([NotNull] Type entityType, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity(entityType));

            return this;
        }

        public virtual BasicModelBuilder Entity([NotNull] string name, [NotNull] Action<EntityBuilder> entityBuilder)
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

            public virtual EntityBuilder Annotation(string annotation, string value)
            {
                Check.NotEmpty(annotation, "annotation");
                Check.NotEmpty(value, "value");

                Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                return this;
            }

            public virtual EntityType Metadata
            {
                get { return Builder.Metadata; }
            }

            Model IMetadataBuilder<EntityType, EntityBuilder>.Model
            {
                get { return Builder.ModelBuilder.Metadata; }
            }

            public virtual KeyBuilder Key([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, "propertyNames");

                return new KeyBuilder(Builder.PrimaryKey(propertyNames, ConfigurationSource.Explicit));
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
                    Check.NotNull(annotation, "annotation");
                    Check.NotNull(value, "value");

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
                private readonly InternalRelationshipBuilder _builder;

                public ForeignKeyBuilder([NotNull] InternalRelationshipBuilder builder)
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
                    Builder.IsUnique(isUnique, ConfigurationSource.Explicit);

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

                return new KeyBuilder(Builder.PrimaryKey(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
            }

            public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
            {
                Check.NotNull(propertyExpression, "propertyExpression");

                var propertyInfo = propertyExpression.GetPropertyAccess();
                return new PropertyBuilder(Builder.Property(propertyInfo, ConfigurationSource.Explicit));
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
        }
    }
}

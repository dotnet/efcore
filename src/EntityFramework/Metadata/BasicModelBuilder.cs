// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
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

            _builder = new InternalModelBuilder(model, null);
        }

        protected internal BasicModelBuilder([NotNull] InternalModelBuilder internalBuilder)
        {
            Check.NotNull(internalBuilder, "internalBuilder");

            _builder = internalBuilder;
        }

        // TODO: Consider whether these conversions are useful
        // Issue #750
        public static explicit operator ModelBuilder([NotNull] BasicModelBuilder builder)
        {
            Check.NotNull(builder, "builder");

            return new ModelBuilder(builder.Builder);
        }

        // TODO: Consider removing this and just using Metadata
        // Issue #751
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

            _builder.Annotation(annotation, value);

            return this;
        }

        protected virtual InternalModelBuilder Builder
        {
            get { return _builder; }
        }

        public virtual EntityBuilder<T> Entity<T>()
        {
            return new EntityBuilder<T>(Builder.GetOrAddEntity(typeof(T)));
        }

        public virtual EntityBuilder Entity([NotNull] Type entityType)
        {
            Check.NotNull(entityType, "entityType");

            return new EntityBuilder(Builder.GetOrAddEntity(entityType));
        }

        public virtual EntityBuilder Entity([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            return new EntityBuilder(Builder.GetOrAddEntity(name));
        }

        public virtual BasicModelBuilder Entity<T>([NotNull] Action<EntityBuilder<T>> entityBuilder)
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity<T>());

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

                Builder.Annotation(annotation, value);

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

                return new KeyBuilder(Builder.Key(propertyNames));
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

                return new PropertyBuilder(Builder.Property(propertyType, name));
            }

            public virtual ForeignKeyBuilder ForeignKey([NotNull] string referencedEntityTypeName, [NotNull] params string[] propertyNames)
            {
                Check.NotNull(referencedEntityTypeName, "referencedEntityTypeName");
                Check.NotNull(propertyNames, "propertyNames");

                return new ForeignKeyBuilder(Builder.ForeignKey(referencedEntityTypeName, propertyNames));
            }

            public virtual IndexBuilder Index([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, "propertyNames");

                return new IndexBuilder(Builder.Index(propertyNames));
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

                    Builder.Annotation(annotation, value);

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

                    Builder.Annotation(annotation, value);

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

                    Builder.Annotation(annotation, value);

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

                    Builder.Annotation(annotation, value);

                    return this;
                }

                public virtual IndexBuilder IsUnique(bool isUnique = true)
                {
                    Builder.IsUnique(isUnique);

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

                return new KeyBuilder(Builder.Key(keyExpression.GetPropertyAccessList()));
            }

            public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
            {
                Check.NotNull(propertyExpression, "propertyExpression");

                return new PropertyBuilder(Builder.Property(propertyExpression.GetPropertyAccess()));
            }

            public virtual ForeignKeyBuilder ForeignKey<TReferencedEntityType>([NotNull] Expression<Func<TEntity, object>> foreignKeyExpression)
            {
                Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                return new ForeignKeyBuilder(Builder.ForeignKey(typeof(TReferencedEntityType), foreignKeyExpression.GetPropertyAccessList()));
            }

            public virtual IndexBuilder Index([NotNull] Expression<Func<TEntity, object>> indexExpression)
            {
                Check.NotNull(indexExpression, "indexExpression");

                return new IndexBuilder(Builder.Index(indexExpression.GetPropertyAccessList()));
            }
        }
    }
}

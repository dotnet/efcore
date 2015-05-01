// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Builders
{
    public class BasicModelBuilder
    {
        private readonly InternalModelBuilder _builder;

        public BasicModelBuilder()
            : this(new Model())
        {
        }

        public BasicModelBuilder([NotNull] Model model)
        {
            Check.NotNull(model, nameof(model));

            _builder = new InternalModelBuilder(model, new ConventionSet());
        }

        public virtual Model Model => Metadata;

        public virtual Model Metadata => Builder.Metadata;

        public virtual BasicModelBuilder Annotation([NotNull] string annotation, [NotNull] object value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotNull(value, nameof(value));

            _builder.Annotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        protected virtual InternalModelBuilder Builder => _builder;

        public virtual EntityTypeBuilder<TEntity> Entity<TEntity>() where TEntity : class 
            => new EntityTypeBuilder<TEntity>(Builder.Entity(typeof(TEntity), ConfigurationSource.Explicit));

        public virtual EntityTypeBuilder Entity([NotNull] Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return new EntityTypeBuilder(Builder.Entity(entityType, ConfigurationSource.Explicit));
        }

        public virtual EntityTypeBuilder Entity([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return new EntityTypeBuilder(Builder.Entity(name, ConfigurationSource.Explicit));
        }

        public virtual BasicModelBuilder Entity<TEntity>([NotNull] Action<EntityTypeBuilder<TEntity>> entityBuilder) where TEntity : class
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity<TEntity>());

            return this;
        }

        public virtual BasicModelBuilder Entity([NotNull] Type entityType, [NotNull] Action<EntityTypeBuilder> entityBuilder)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity(entityType));

            return this;
        }

        public virtual BasicModelBuilder Entity([NotNull] string name, [NotNull] Action<EntityTypeBuilder> entityBuilder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity(name));

            return this;
        }

        public class EntityTypeBuilder
        {
            public EntityTypeBuilder([NotNull] InternalEntityTypeBuilder builder)
            {
                Check.NotNull(builder, nameof(builder));

                Builder = builder;
            }

            protected virtual InternalEntityTypeBuilder Builder { get; }

            public virtual EntityTypeBuilder Annotation([NotNull] string annotation, [NotNull] object value)
            {
                Check.NotEmpty(annotation, nameof(annotation));
                Check.NotNull(value, nameof(value));

                Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                return this;
            }

            public virtual EntityType Metadata => Builder.Metadata;

            public virtual KeyBuilder Key([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, nameof(propertyNames));

                return new KeyBuilder(Builder.PrimaryKey(propertyNames, ConfigurationSource.Explicit));
            }

            public virtual PropertyBuilder Property<TProperty>([NotNull] string name)
            {
                Check.NotEmpty(name, nameof(name));

                return Property(typeof(TProperty), name);
            }

            public virtual PropertyBuilder Property([NotNull] Type propertyType, [NotNull] string name)
            {
                Check.NotNull(propertyType, nameof(propertyType));
                Check.NotEmpty(name, nameof(name));

                return new PropertyBuilder(Builder.Property(propertyType, name, ConfigurationSource.Explicit));
            }

            public virtual ForeignKeyBuilder ForeignKey([NotNull] string referencedEntityTypeName, [NotNull] params string[] propertyNames)
            {
                Check.NotNull(referencedEntityTypeName, nameof(referencedEntityTypeName));
                Check.NotNull(propertyNames, nameof(propertyNames));

                return new ForeignKeyBuilder(Builder.ForeignKey(referencedEntityTypeName, propertyNames, ConfigurationSource.Explicit));
            }

            public virtual IndexBuilder Index([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, nameof(propertyNames));

                return new IndexBuilder(Builder.Index(propertyNames, ConfigurationSource.Explicit));
            }

            public class KeyBuilder
            {
                public KeyBuilder([NotNull] InternalKeyBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                protected virtual InternalKeyBuilder Builder { get; }

                public virtual Key Metadata => Builder.Metadata;

                public virtual KeyBuilder Annotation([NotNull] string annotation, [NotNull] object value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotNull(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }
            }

            public class PropertyBuilder
            {
                public PropertyBuilder([NotNull] InternalPropertyBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                protected virtual InternalPropertyBuilder Builder { get; }

                public virtual Property Metadata => Builder.Metadata;

                public virtual PropertyBuilder Annotation([NotNull] string annotation, [NotNull] object value)
                {
                    Check.NotNull(annotation, nameof(annotation));
                    Check.NotNull(value, nameof(value));

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

                public virtual PropertyBuilder StoreGeneratedPattern(StoreGeneratedPattern storeGeneratedPattern)
                {
                    Builder.StoreGeneratedPattern(storeGeneratedPattern, ConfigurationSource.Explicit);

                    return this;
                }
            }

            public class ForeignKeyBuilder
            {
                public ForeignKeyBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                protected virtual InternalRelationshipBuilder Builder { get; }

                public virtual ForeignKey Metadata => Builder.Metadata;

                public virtual ForeignKeyBuilder Annotation([NotNull] string annotation, [NotNull] object value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotNull(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                public virtual ForeignKeyBuilder IsUnique(bool isUnique = true)
                {
                    return new ForeignKeyBuilder(Builder.Unique(isUnique, ConfigurationSource.Explicit));
                }
            }

            public class IndexBuilder
            {
                public IndexBuilder([NotNull] InternalIndexBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                protected virtual InternalIndexBuilder Builder { get; }

                public virtual Index Metadata => Builder.Metadata;

                public virtual IndexBuilder Annotation([NotNull] string annotation, [NotNull] object value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotNull(value, nameof(value));

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

        public class EntityTypeBuilder<TEntity> : EntityTypeBuilder
            where TEntity : class
        {
            public EntityTypeBuilder([NotNull] InternalEntityTypeBuilder builder)
                : base(builder)
            {
            }

            public new virtual EntityTypeBuilder<TEntity> Annotation([NotNull] string annotation, [NotNull] object value)
            {
                base.Annotation(annotation, value);

                return this;
            }

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

            public virtual ForeignKeyBuilder ForeignKey<TReferencedEntityType>([NotNull] Expression<Func<TEntity, object>> foreignKeyExpression)
            {
                Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression));

                return new ForeignKeyBuilder(Builder.ForeignKey(typeof(TReferencedEntityType), foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
            }

            public virtual IndexBuilder Index([NotNull] Expression<Func<TEntity, object>> indexExpression)
            {
                Check.NotNull(indexExpression, nameof(indexExpression));

                return new IndexBuilder(Builder.Index(indexExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
            }
        }
    }
}

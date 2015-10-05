// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a one-to-one relationship.
    ///     </para>
    ///     <para>
    ///         If multiple reference key properties are specified, the order of reference key properties should
    ///         match the order that the primary key or unique index properties were configured on the principal
    ///         entity type.
    ///     </para>
    /// </summary>
    public class ReferenceReferenceBuilder<TEntity, TRelatedEntity> : ReferenceReferenceBuilder
        where TEntity : class
        where TRelatedEntity : class
    {
        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="ReferenceReferenceBuilder" /> class.
        ///     </para>
        ///     <para>
        ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
        ///         and it is not designed to be directly constructed in your application code.
        ///     </para>
        /// </summary>
        /// <param name="builder"> The internal builder being used to configure this relationship. </param>
        public ReferenceReferenceBuilder([NotNull] InternalRelationshipBuilder builder)
            : base(builder)
        {
        }

        /// <summary>
        ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasAnnotation([NotNull] string annotation, [NotNull] object value)
            => (ReferenceReferenceBuilder<TEntity, TRelatedEntity>)base.HasAnnotation(annotation, value);

        /// <summary>
        ///     <para>
        ///         Configures the property(s) to use as the foreign key for this relationship.
        ///     </para>
        ///     <para>
        ///         If the specified property name(s) do not exist on the entity type then a new shadow state
        ///         property(s) will be added to serve as the foreign key. A shadow state property is one
        ///         that does not have a corresponding property in the entity class. The current value for the
        ///         property is stored in the <see cref="ChangeTracker" /> rather than being stored in instances
        ///         of the entity class.
        ///     </para>
        ///     <para>
        ///         If <see cref="HasPrincipalKey(System.Type,string[])" /> is not specified, then an attempt will be made to
        ///         match the data type and order of foreign key properties against the primary key of the principal
        ///         entity type. If they do not match, new shadow state properties that form a unique index will be
        ///         added to the principal entity type to serve as the reference key.
        ///     </para>
        /// </summary>
        /// <param name="dependentEntityType">
        ///     The entity type that is the dependent in this relationship (the type that has the foreign key
        ///     properties).
        /// </param>
        /// <param name="foreignKeyPropertyNames">
        ///     The name(s) of the foreign key property(s).
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey(
            [NotNull] Type dependentEntityType,
            [NotNull] params string[] foreignKeyPropertyNames)
        {
            Check.NotNull(dependentEntityType, nameof(dependentEntityType));
            Check.NotEmpty(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

            return new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(Builder.HasForeignKey(dependentEntityType, foreignKeyPropertyNames, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique constraint
        ///     will be introduced.
        /// </summary>
        /// <param name="principalEntityType">
        ///     The entity type that is the principal in this relationship (the type
        ///     that has the reference key properties).
        /// </param>
        /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
            [NotNull] Type principalEntityType,
            [NotNull] params string[] keyPropertyNames)
        {
            Check.NotNull(principalEntityType, nameof(principalEntityType));
            Check.NotEmpty(keyPropertyNames, nameof(keyPropertyNames));

            return new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(Builder.HasPrincipalKey(principalEntityType, keyPropertyNames, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     <para>
        ///         Configures the property(s) to use as the foreign key for this relationship.
        ///     </para>
        ///     <para>
        ///         If the specified property name(s) do not exist on the entity type then a new shadow state
        ///         property(s) will be added to serve as the foreign key. A shadow state property is one
        ///         that does not have a corresponding property in the entity class. The current value for the
        ///         property is stored in the <see cref="ChangeTracker" /> rather than being stored in instances
        ///         of the entity class.
        ///     </para>
        ///     <para>
        ///         If <see cref="HasPrincipalKey(Type,string[])" /> is not specified, then an attempt will be made to
        ///         match the data type and order of foreign key properties against the primary key of the principal
        ///         entity type. If they do not match, new shadow state properties that form a unique index will be
        ///         added to the principal entity type to serve as the reference key.
        ///     </para>
        /// </summary>
        /// <param name="dependentEntityTypeName">
        ///     The name of entity type that is the dependent in this relationship (the type that has the foreign key
        ///     properties).
        /// </param>
        /// <param name="foreignKeyPropertyNames">
        ///     The name(s) of the foreign key property(s).
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey(
            [NotNull] string dependentEntityTypeName,
            [NotNull] params string[] foreignKeyPropertyNames)
        {
            Check.NotEmpty(dependentEntityTypeName, nameof(dependentEntityTypeName));
            Check.NotEmpty(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

            return new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(Builder.HasForeignKey(dependentEntityTypeName, foreignKeyPropertyNames, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique constraint
        ///     will be introduced.
        /// </summary>
        /// <param name="principalEntityTypeName">
        ///     The name of entity type that is the principal in this relationship (the type
        ///     that has the reference key properties).
        /// </param>
        /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
            [NotNull] string principalEntityTypeName,
            [NotNull] params string[] keyPropertyNames)
        {
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
            Check.NotEmpty(keyPropertyNames, nameof(keyPropertyNames));

            return new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(Builder.HasPrincipalKey(principalEntityTypeName, keyPropertyNames, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     <para>
        ///         Configures the property(s) to use as the foreign key for this relationship.
        ///     </para>
        ///     <para>
        ///         If <see cref="HasPrincipalKey{TPrincipalEntity}" />
        ///         is not specified, then an attempt will be made to match the data type and order of foreign key
        ///         properties against the primary key of the principal entity type. If they do not match, new shadow
        ///         state properties that form a unique index will be
        ///         added to the principal entity type to serve as the reference key.
        ///         A shadow state property is one that does not have a corresponding property in the entity class. The
        ///         current value for the property is stored in the <see cref="ChangeTracker" /> rather than being
        ///         stored in instances of the entity class.
        ///     </para>
        /// </summary>
        /// <typeparam name="TDependentEntity">
        ///     The entity type that is the dependent in this relationship. That is, the type
        ///     that has the foreign key properties.
        /// </typeparam>
        /// <param name="foreignKeyExpression">
        ///     <para>
        ///         A lambda expression representing the foreign key property(s) (<c>t => t.Id1</c>).
        ///     </para>
        ///     <para>
        ///         If the foreign key is made up of multiple properties then specify an anonymous type including the
        ///         properties (<c>t => new { t.Id1, t.Id2 }</c>). The order specified should match the order of
        ///         corresponding keys in <see cref="HasPrincipalKey{TPrincipalEntity}" />.
        ///     </para>
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
            [NotNull] Expression<Func<TDependentEntity, object>> foreignKeyExpression)
        {
            Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression));

            return new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                Builder.HasForeignKey(typeof(TDependentEntity), foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique constraint
        ///     will be introduced.
        /// </summary>
        /// <typeparam name="TPrincipalEntity">
        ///     The entity type that is the principal in this relationship. That is, the type
        ///     that has the reference key properties.
        /// </typeparam>
        /// <param name="keyExpression">
        ///     <para>
        ///         A lambda expression representing the reference key property(s) (<c>t => t.Id</c>).
        ///     </para>
        ///     <para>
        ///         If the principal key is made up of multiple properties then specify an anonymous type including
        ///         the properties (<c>t => new { t.Id1, t.Id2 }</c>).
        ///     </para>
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            [NotNull] Expression<Func<TPrincipalEntity, object>> keyExpression)
        {
            Check.NotNull(keyExpression, nameof(keyExpression));

            return new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(Builder.HasPrincipalKey(typeof(TPrincipalEntity), keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
        ///     be assigned null).
        /// </summary>
        /// <param name="required"> A value indicating whether this is a required relationship. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> IsRequired(bool required = true)
            => new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(Builder.IsRequired(required, ConfigurationSource.Explicit));

        public new virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> WillCascadeOnDelete(bool cascade = true)
            => new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                Builder.DeleteBehavior(cascade ? DeleteBehavior.Cascade : DeleteBehavior.None, ConfigurationSource.Explicit));

        private InternalRelationshipBuilder Builder => this.GetService<InternalRelationshipBuilder>();
    }
}

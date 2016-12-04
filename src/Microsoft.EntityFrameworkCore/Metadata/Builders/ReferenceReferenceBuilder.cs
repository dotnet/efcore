// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
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
    public class ReferenceReferenceBuilder : IInfrastructure<IMutableModel>, IInfrastructure<InternalRelationshipBuilder>
    {
        private readonly IReadOnlyList<Property> _foreignKeyProperties;
        private readonly IReadOnlyList<Property> _principalKeyProperties;
        private readonly EntityType _declaringEntityType;
        private readonly EntityType _relatedEntityType;
        private readonly bool? _required;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceReferenceBuilder(
            [NotNull] InternalRelationshipBuilder builder,
            [NotNull] EntityType declaringEntityType,
            [NotNull] EntityType relatedEntityType)
            : this(builder, null)
        {
            Check.NotNull(builder, nameof(builder));
            _declaringEntityType = declaringEntityType;
            _relatedEntityType = relatedEntityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ReferenceReferenceBuilder(
            InternalRelationshipBuilder builder,
            ReferenceReferenceBuilder oldBuilder,
            bool inverted = false,
            bool foreignKeySet = false,
            bool principalKeySet = false,
            bool requiredSet = false)
        {
            Builder = builder;

            if (oldBuilder != null)
            {
                if (inverted)
                {
                    if ((oldBuilder._foreignKeyProperties != null)
                        || (oldBuilder._principalKeyProperties != null))
                    {
                        throw new InvalidOperationException(CoreStrings.RelationshipCannotBeInverted);
                    }
                }

                _declaringEntityType = oldBuilder._declaringEntityType;
                _relatedEntityType = oldBuilder._relatedEntityType;

                _foreignKeyProperties = foreignKeySet
                    ? builder.Metadata.Properties
                    : oldBuilder._foreignKeyProperties;
                _principalKeyProperties = principalKeySet
                    ? builder.Metadata.PrincipalKey.Properties
                    : oldBuilder._principalKeyProperties;
                _required = requiredSet
                    ? builder.Metadata.IsRequired
                    : oldBuilder._required;

                var foreignKey = builder.Metadata;
                ForeignKey.AreCompatible(
                    foreignKey.PrincipalEntityType,
                    foreignKey.DeclaringEntityType,
                    foreignKey.DependentToPrincipal?.PropertyInfo,
                    foreignKey.PrincipalToDependent?.PropertyInfo,
                    _foreignKeyProperties,
                    _principalKeyProperties,
                    foreignKey.IsUnique,
                    _required,
                    shouldThrow: true);
            }
        }

        /// <summary>
        ///     Gets the internal builder being used to configure this relationship.
        /// </summary>
        protected virtual InternalRelationshipBuilder Builder { get; }

        /// <summary>
        ///     Gets the internal builder being used to configure this relationship.
        /// </summary>
        InternalRelationshipBuilder IInfrastructure<InternalRelationshipBuilder>.Instance => Builder;

        /// <summary>
        ///     The foreign key that represents this relationship.
        /// </summary>
        public virtual IMutableForeignKey Metadata => Builder.Metadata;

        /// <summary>
        ///     The model that this relationship belongs to.
        /// </summary>
        IMutableModel IInfrastructure<IMutableModel>.Instance => Builder.ModelBuilder.Metadata;

        /// <summary>
        ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceReferenceBuilder HasAnnotation([NotNull] string annotation, [NotNull] object value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotNull(value, nameof(value));

            Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

            return this;
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
        public virtual ReferenceReferenceBuilder HasForeignKey(
            [NotNull] Type dependentEntityType,
            [NotNull] params string[] foreignKeyPropertyNames)
            => new ReferenceReferenceBuilder(
                HasForeignKeyBuilder(ResolveEntityType(Check.NotNull(dependentEntityType, nameof(dependentEntityType))),
                    dependentEntityType.ShortDisplayName(),
                    Check.NotEmpty(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames))),
                this,
                Builder.Metadata.DeclaringEntityType.ClrType != dependentEntityType,
                true);

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
        /// <typeparam name="TDependentEntity">
        ///     The entity type that is the dependent in this relationship (the type that has the foreign key
        ///     properties).
        /// </typeparam>
        /// <param name="foreignKeyPropertyNames">
        ///     The name(s) of the foreign key property(s).
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceReferenceBuilder HasForeignKey<TDependentEntity>(
            [NotNull] params string[] foreignKeyPropertyNames)
            where TDependentEntity : class
            => HasForeignKey(typeof(TDependentEntity), foreignKeyPropertyNames);

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
        /// <param name="dependentEntityTypeName">
        ///     The name of the entity type that is the dependent in this relationship (the type that has the foreign
        ///     key properties).
        /// </param>
        /// <param name="foreignKeyPropertyNames">
        ///     The name(s) of the foreign key property(s).
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceReferenceBuilder HasForeignKey(
            [NotNull] string dependentEntityTypeName,
            [NotNull] params string[] foreignKeyPropertyNames)
            => new ReferenceReferenceBuilder(
                HasForeignKeyBuilder(ResolveEntityType(Check.NotNull(dependentEntityTypeName, nameof(dependentEntityTypeName))),
                    dependentEntityTypeName,
                    Check.NotEmpty(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames))),
                this,
                Builder.Metadata.DeclaringEntityType.Name != ResolveEntityType(dependentEntityTypeName).Name,
                true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasForeignKeyBuilder(
            [CanBeNull] EntityType dependentEntityType,
            [NotNull] string dependentEntityTypeName,
            [NotNull] IReadOnlyList<string> foreignKeyPropertyNames)
            => HasForeignKeyBuilder(dependentEntityType, dependentEntityTypeName,
                (b, d) => b.HasForeignKey(foreignKeyPropertyNames, d, ConfigurationSource.Explicit));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasForeignKeyBuilder(
            [NotNull] EntityType dependentEntityType,
            [NotNull] string dependentEntityTypeName,
            [NotNull] IReadOnlyList<PropertyInfo> foreignKeyProperties)
            => HasForeignKeyBuilder(dependentEntityType, dependentEntityTypeName,
                (b, d) => b.HasForeignKey(foreignKeyProperties, d, ConfigurationSource.Explicit));

        private InternalRelationshipBuilder HasForeignKeyBuilder(
            EntityType dependentEntityType,
            string dependentEntityTypeName,
            Func<InternalRelationshipBuilder, EntityType, InternalRelationshipBuilder> hasForeignKey)
        {
            if (dependentEntityType == null)
            {
                throw new InvalidOperationException(CoreStrings.DependentEntityTypeNotInRelationship(
                    _declaringEntityType.DisplayName(),
                    _relatedEntityType.DisplayName(),
                    dependentEntityTypeName));
            }
            var principalEntityType = GetOtherEntityType(dependentEntityType);

            var builder = Builder.RelatedEntityTypes(principalEntityType, dependentEntityType, ConfigurationSource.Explicit);

            return hasForeignKey(builder, dependentEntityType);
        }

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
        ///     constraint will be introduced.
        /// </summary>
        /// <param name="principalEntityType">
        ///     The entity type that is the principal in this relationship (the type
        ///     that has the reference key properties).
        /// </param>
        /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceReferenceBuilder HasPrincipalKey(
            [NotNull] Type principalEntityType,
            [NotNull] params string[] keyPropertyNames)
            => new ReferenceReferenceBuilder(
                HasPrincipalKeyBuilder(
                    ResolveEntityType(Check.NotNull(principalEntityType, nameof(principalEntityType))),
                    principalEntityType.ShortDisplayName(),
                    Check.NotEmpty(keyPropertyNames, nameof(keyPropertyNames))),
                this,
                inverted: Builder.Metadata.PrincipalEntityType.ClrType != principalEntityType,
                principalKeySet: true);

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
        ///     constraint will be introduced.
        /// </summary>
        /// <typeparam name="TPrincipalEntity">
        ///     The entity type that is the principal in this relationship (the type
        ///     that has the reference key properties).
        /// </typeparam>
        /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceReferenceBuilder HasPrincipalKey<TPrincipalEntity>(
            [NotNull] params string[] keyPropertyNames)
            where TPrincipalEntity : class
            => HasPrincipalKey(typeof(TPrincipalEntity), keyPropertyNames);

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
        ///     constraint will be introduced.
        /// </summary>
        /// <param name="principalEntityTypeName">
        ///     The name of the entity type that is the principal in this relationship (the type
        ///     that has the reference key properties).
        /// </param>
        /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceReferenceBuilder HasPrincipalKey(
            [NotNull] string principalEntityTypeName,
            [NotNull] params string[] keyPropertyNames)
            => new ReferenceReferenceBuilder(
                HasPrincipalKeyBuilder(
                    ResolveEntityType(Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName))),
                    principalEntityTypeName,
                    Check.NotEmpty(keyPropertyNames, nameof(keyPropertyNames))),
                this,
                inverted: Builder.Metadata.PrincipalEntityType.Name != ResolveEntityType(principalEntityTypeName).Name,
                principalKeySet: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasPrincipalKeyBuilder(
            [CanBeNull] EntityType principalEntityType,
            [NotNull] string principalEntityTypeName,
            [NotNull] IReadOnlyList<string> foreignKeyPropertyNames)
            => HasPrincipalKeyBuilder(principalEntityType, principalEntityTypeName,
                b => b.HasPrincipalKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasPrincipalKeyBuilder(
            [NotNull] EntityType principalEntityType,
            [NotNull] string principalEntityTypeName,
            [NotNull] IReadOnlyList<PropertyInfo> foreignKeyProperties)
            => HasPrincipalKeyBuilder(principalEntityType, principalEntityTypeName,
                b => b.HasPrincipalKey(foreignKeyProperties, ConfigurationSource.Explicit));

        private InternalRelationshipBuilder HasPrincipalKeyBuilder(
            EntityType principalEntityType,
            string principalEntityTypeName,
            Func<InternalRelationshipBuilder, InternalRelationshipBuilder> hasPrincipalKey)
        {
            if (principalEntityType == null)
            {
                throw new InvalidOperationException(CoreStrings.PrincipalEntityTypeNotInRelationship(
                    _declaringEntityType.DisplayName(),
                    _relatedEntityType.DisplayName(),
                    principalEntityTypeName));
            }

            var builder = Builder.RelatedEntityTypes(principalEntityType, GetOtherEntityType(principalEntityType), ConfigurationSource.Explicit);

            return hasPrincipalKey(builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityType ResolveEntityType([NotNull] string entityTypeName)
        {
            if (_declaringEntityType.Name == entityTypeName)
            {
                return _declaringEntityType;
            }

            if (_relatedEntityType.Name == entityTypeName)
            {
                return _relatedEntityType;
            }

            if (_declaringEntityType.DisplayName() == entityTypeName)
            {
                return _declaringEntityType;
            }

            if (_relatedEntityType.DisplayName() == entityTypeName)
            {
                return _relatedEntityType;
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityType ResolveEntityType([NotNull] Type entityType)
        {
            if (_declaringEntityType.ClrType == entityType)
            {
                return _declaringEntityType;
            }

            if (_relatedEntityType.ClrType == entityType)
            {
                return _relatedEntityType;
            }

            return null;
        }

        private EntityType GetOtherEntityType(EntityType entityType)
            => _declaringEntityType == entityType ? _relatedEntityType : _declaringEntityType;

        /// <summary>
        ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
        ///     be assigned null).
        /// </summary>
        /// <param name="required"> A value indicating whether this is a required relationship. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceReferenceBuilder IsRequired(bool required = true)
            => new ReferenceReferenceBuilder(Builder.IsRequired(required, ConfigurationSource.Explicit), this, requiredSet: true);

        /// <summary>
        ///     Configures how a delete operation is applied to dependent entities in the relationship when the
        ///     principal is deleted or the relationship is severed.
        /// </summary>
        /// <param name="deleteBehavior"> The action to perform. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceReferenceBuilder OnDelete(DeleteBehavior deleteBehavior)
            => new ReferenceReferenceBuilder(Builder.DeleteBehavior(deleteBehavior, ConfigurationSource.Explicit), this);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
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
    public class ReferenceReferenceBuilder : IInfrastructure<IMutableModel>, IInfrastructure<InternalRelationshipBuilder>
    {
        private readonly IReadOnlyList<Property> _foreignKeyProperties;
        private readonly IReadOnlyList<Property> _principalKeyProperties;
        private readonly string _navigationToPrincipalName;
        private readonly string _navigationToDependentName;
        private readonly bool? _required;

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
            : this(builder, null)
        {
            Check.NotNull(builder, nameof(builder));
            _navigationToPrincipalName = builder.Metadata.DependentToPrincipal?.Name;
            _navigationToDependentName = builder.Metadata.PrincipalToDependent?.Name;
        }

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="ReferenceReferenceBuilder" /> class.
        ///     </para>
        /// </summary>
        /// <param name="builder"> The internal builder being used to configure this relationship. </param>
        /// <param name="oldBuilder"> A builder to copy configuration from. </param>
        /// <param name="inverted">
        ///     A value indicating whether to reverse the direction of the relationship.
        /// </param>
        /// <param name="foreignKeySet">
        ///     A value indicating whether the foreign key properties have been configured in this chain of configuration calls.
        /// </param>
        /// <param name="principalKeySet">
        ///     A value indicating whether the principal key properties have been configured in this chain of configuration calls.
        /// </param>
        /// <param name="requiredSet">
        ///     A value indicating whether required/optional has been configured in this chain of configuration calls.
        /// </param>
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

                    var navigationName = _navigationToDependentName;
                    _navigationToDependentName = _navigationToPrincipalName;
                    _navigationToPrincipalName = navigationName;
                }

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
                    _navigationToPrincipalName,
                    _navigationToDependentName,
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
        {
            Check.NotNull(dependentEntityType, nameof(dependentEntityType));
            Check.NotEmpty(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

            return new ReferenceReferenceBuilder(
                Builder.DependentEntityType(dependentEntityType, ConfigurationSource.Explicit)
                    .HasForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit),
                this,
                inverted: Builder.Metadata.DeclaringEntityType.ClrType != dependentEntityType,
                foreignKeySet: true);
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
        {
            Check.NotEmpty(dependentEntityTypeName, nameof(dependentEntityTypeName));
            Check.NotEmpty(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

            return new ReferenceReferenceBuilder(
                Builder.DependentEntityType(dependentEntityTypeName, ConfigurationSource.Explicit)
                    .HasForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit),
                this,
                inverted: Builder.Metadata.DeclaringEntityType.Name != dependentEntityTypeName,
                foreignKeySet: true);
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
        {
            Check.NotNull(principalEntityType, nameof(principalEntityType));
            Check.NotEmpty(keyPropertyNames, nameof(keyPropertyNames));

            return new ReferenceReferenceBuilder(
                Builder.PrincipalEntityType(principalEntityType, ConfigurationSource.Explicit)
                    .HasPrincipalKey(keyPropertyNames, ConfigurationSource.Explicit),
                this,
                inverted: Builder.Metadata.PrincipalEntityType.ClrType != principalEntityType,
                principalKeySet: true);
        }

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
        {
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
            Check.NotEmpty(keyPropertyNames, nameof(keyPropertyNames));

            return new ReferenceReferenceBuilder(
                Builder.PrincipalEntityType(principalEntityTypeName, ConfigurationSource.Explicit)
                    .HasPrincipalKey(keyPropertyNames, ConfigurationSource.Explicit),
                this,
                inverted: Builder.Metadata.PrincipalEntityType.Name != principalEntityTypeName,
                principalKeySet: true);
        }

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
            => new ReferenceReferenceBuilder(
                Builder.DeleteBehavior(deleteBehavior, ConfigurationSource.Explicit), this);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a one-to-many relationship.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class ReferenceCollectionBuilder : IInfrastructure<IMutableModel>, IInfrastructure<InternalRelationshipBuilder>
    {
        private readonly EntityType _principalEntityType;
        private readonly EntityType _dependentEntityType;
        private readonly IReadOnlyList<Property> _foreignKeyProperties;
        private readonly IReadOnlyList<Property> _principalKeyProperties;
        private readonly bool? _required;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceCollectionBuilder(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            [NotNull] InternalRelationshipBuilder builder)
            : this(builder, null)
        {
            Check.NotNull(builder, nameof(builder));

            _principalEntityType = principalEntityType;
            _dependentEntityType = dependentEntityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ReferenceCollectionBuilder(
            InternalRelationshipBuilder builder,
            ReferenceCollectionBuilder oldBuilder,
            bool foreignKeySet = false,
            bool principalKeySet = false,
            bool requiredSet = false)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
            if (oldBuilder != null)
            {
                _principalEntityType = oldBuilder._principalEntityType;
                _dependentEntityType = oldBuilder._dependentEntityType;
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
                    _principalEntityType,
                    _dependentEntityType,
                    foreignKey.DependentToPrincipal?.PropertyInfo,
                    foreignKey.PrincipalToDependent?.PropertyInfo,
                    _foreignKeyProperties,
                    _principalKeyProperties,
                    foreignKey.IsUnique,
                    _required,
                    true);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder Builder { get; }

        /// <summary>
        ///     The foreign key that represents this relationship.
        /// </summary>
        public virtual IMutableForeignKey Metadata => Builder.Metadata;

        /// <summary>
        ///     The model that this relationship belongs to.
        /// </summary>
        IMutableModel IInfrastructure<IMutableModel>.Instance => Builder.ModelBuilder.Metadata;

        /// <summary>
        ///     Gets the internal builder being used to configure this relationship.
        /// </summary>
        InternalRelationshipBuilder IInfrastructure<InternalRelationshipBuilder>.Instance => Builder;

        /// <summary>
        ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
        ///     <paramref name="annotation" />
        ///     already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceCollectionBuilder HasAnnotation([NotNull] string annotation, [NotNull] object value)
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
        ///         property(s) will be added to serve as the foreign key. A shadow state property is one that does not
        ///         have a corresponding property in the entity class. The current value for the  property is stored in
        ///         the <see cref="ChangeTracker" /> rather than being stored in instances
        ///         of the entity class.
        ///     </para>
        ///     <para>
        ///         If <see cref="HasPrincipalKey" /> is not specified, then an attempt will be made to match
        ///         the data type and order of foreign key properties against the primary key of the principal
        ///         entity type. If they do not match, new shadow state properties that form a unique index will be
        ///         added to the principal entity type to serve as the reference key.
        ///     </para>
        /// </summary>
        /// <param name="foreignKeyPropertyNames">
        ///     The name(s) of the foreign key property(s).
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceCollectionBuilder HasForeignKey([NotNull] params string[] foreignKeyPropertyNames)
            => new ReferenceCollectionBuilder(
                HasForeignKeyBuilder(Check.NotEmpty(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames))),
                this,
                foreignKeySet: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasForeignKeyBuilder([NotNull] IReadOnlyList<string> foreignKeyPropertyNames)
            => Builder.HasForeignKey(foreignKeyPropertyNames, _dependentEntityType, ConfigurationSource.Explicit);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasForeignKeyBuilder([NotNull] IReadOnlyList<PropertyInfo> foreignKeyProperties)
            => Builder.HasForeignKey(foreignKeyProperties, _dependentEntityType, ConfigurationSource.Explicit);

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
        ///     constraint will be introduced.
        /// </summary>
        /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceCollectionBuilder HasPrincipalKey([NotNull] params string[] keyPropertyNames)
            => new ReferenceCollectionBuilder(
                HasPrincipalKeyBuilder(Check.NotEmpty(keyPropertyNames, nameof(keyPropertyNames))),
                this,
                principalKeySet: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasPrincipalKeyBuilder([NotNull] IReadOnlyList<string> keyPropertyNames)
            => Builder.HasPrincipalKey(keyPropertyNames, ConfigurationSource.Explicit);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasPrincipalKeyBuilder([NotNull] IReadOnlyList<PropertyInfo> keyProperties)
            => Builder.HasPrincipalKey(keyProperties, ConfigurationSource.Explicit);

        /// <summary>
        ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
        ///     be assigned null).
        /// </summary>
        /// <param name="required"> A value indicating whether this is a required relationship. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceCollectionBuilder IsRequired(bool required = true)
            => new ReferenceCollectionBuilder(
                Builder.IsRequired(required, ConfigurationSource.Explicit),
                this,
                requiredSet: true);

        /// <summary>
        ///     Configures how a delete operation is applied to dependent entities in the relationship when the
        ///     principal is deleted or the relationship is severed.
        /// </summary>
        /// <param name="deleteBehavior"> The action to perform. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceCollectionBuilder OnDelete(DeleteBehavior deleteBehavior)
            => new ReferenceCollectionBuilder(
                Builder.DeleteBehavior(deleteBehavior, ConfigurationSource.Explicit),
                this);
    }
}

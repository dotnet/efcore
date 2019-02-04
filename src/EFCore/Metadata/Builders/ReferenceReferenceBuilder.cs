// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a one-to-one relationship.
    ///     </para>
    /// </summary>
    public class ReferenceReferenceBuilder : InvertibleRelationshipBuilderBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceReferenceBuilder(
            [NotNull] EntityType declaringEntityType,
            [NotNull] EntityType relatedEntityType,
            [NotNull] InternalRelationshipBuilder builder)
            : base(declaringEntityType, relatedEntityType, builder)
        {
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
            : base(builder, oldBuilder, inverted, foreignKeySet, principalKeySet, requiredSet)
        {
        }

        /// <summary>
        ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
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
                HasForeignKeyBuilder(
                    ResolveEntityType(Check.NotNull(dependentEntityTypeName, nameof(dependentEntityTypeName))),
                    dependentEntityTypeName,
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames))),
                this,
                Builder.Metadata.DeclaringEntityType.Name != ResolveEntityType(dependentEntityTypeName).Name,
                foreignKeySet: foreignKeyPropertyNames.Length > 0);

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
                HasForeignKeyBuilder(
                    ResolveEntityType(Check.NotNull(dependentEntityType, nameof(dependentEntityType))),
                    dependentEntityType.ShortDisplayName(),
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames))),
                this,
                Builder.Metadata.DeclaringEntityType.ClrType != dependentEntityType,
                foreignKeySet: foreignKeyPropertyNames.Length > 0);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasForeignKeyBuilder(
            [CanBeNull] EntityType dependentEntityType,
            [NotNull] string dependentEntityTypeName,
            [NotNull] IReadOnlyList<string> foreignKeyPropertyNames)
            => HasForeignKeyBuilder(
                dependentEntityType, dependentEntityTypeName,
                (b, d) => b.HasForeignKey(foreignKeyPropertyNames, d, ConfigurationSource.Explicit));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasForeignKeyBuilder(
            [NotNull] EntityType dependentEntityType,
            [NotNull] string dependentEntityTypeName,
            [NotNull] IReadOnlyList<PropertyInfo> foreignKeyProperties)
            => HasForeignKeyBuilder(
                dependentEntityType, dependentEntityTypeName,
                (b, d) => b.HasForeignKey(foreignKeyProperties, d, ConfigurationSource.Explicit));

        private InternalRelationshipBuilder HasForeignKeyBuilder(
            EntityType dependentEntityType,
            string dependentEntityTypeName,
            Func<InternalRelationshipBuilder, EntityType, InternalRelationshipBuilder> hasForeignKey)
        {
            if (dependentEntityType == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DependentEntityTypeNotInRelationship(
                        DeclaringEntityType.DisplayName(),
                        RelatedEntityType.DisplayName(),
                        dependentEntityTypeName));
            }

            using (var batch = dependentEntityType.Model.ConventionDispatcher.StartBatch())
            {
                var builder = Builder.RelatedEntityTypes(
                    GetOtherEntityType(dependentEntityType), dependentEntityType, ConfigurationSource.Explicit);
                builder = hasForeignKey(builder, dependentEntityType);

                return batch.Run(builder);
            }
        }

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
        ///     constraint will be introduced.
        /// </summary>
        /// <remarks>
        ///     If multiple principal key properties are specified, the order of principal key properties should
        ///     match the order that the primary key or unique constraint properties were configured on the principal
        ///     entity type.
        /// </remarks>
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
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames))),
                this,
                inverted: Builder.Metadata.PrincipalEntityType.Name != ResolveEntityType(principalEntityTypeName).Name,
                principalKeySet: keyPropertyNames.Length > 0);

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
        ///     constraint will be introduced.
        /// </summary>
        /// <remarks>
        ///     If multiple principal key properties are specified, the order of principal key properties should
        ///     match the order that the primary key or unique constraint properties were configured on the principal
        ///     entity type.
        /// </remarks>
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
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames))),
                this,
                inverted: Builder.Metadata.PrincipalEntityType.ClrType != principalEntityType,
                principalKeySet: keyPropertyNames.Length > 0);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasPrincipalKeyBuilder(
            [CanBeNull] EntityType principalEntityType,
            [NotNull] string principalEntityTypeName,
            [NotNull] IReadOnlyList<string> foreignKeyPropertyNames)
            => HasPrincipalKeyBuilder(
                principalEntityType, principalEntityTypeName,
                b => b.HasPrincipalKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder HasPrincipalKeyBuilder(
            [NotNull] EntityType principalEntityType,
            [NotNull] string principalEntityTypeName,
            [NotNull] IReadOnlyList<PropertyInfo> foreignKeyProperties)
            => HasPrincipalKeyBuilder(
                principalEntityType, principalEntityTypeName,
                b => b.HasPrincipalKey(foreignKeyProperties, ConfigurationSource.Explicit));

        private InternalRelationshipBuilder HasPrincipalKeyBuilder(
            EntityType principalEntityType,
            string principalEntityTypeName,
            Func<InternalRelationshipBuilder, InternalRelationshipBuilder> hasPrincipalKey)
        {
            if (principalEntityType == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PrincipalEntityTypeNotInRelationship(
                        DeclaringEntityType.DisplayName(),
                        RelatedEntityType.DisplayName(),
                        principalEntityTypeName));
            }

            using (var batch = principalEntityType.Model.ConventionDispatcher.StartBatch())
            {
                var builder = Builder.RelatedEntityTypes(
                    principalEntityType, GetOtherEntityType(principalEntityType), ConfigurationSource.Explicit);
                builder = hasPrincipalKey(builder);

                return batch.Run(builder);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityType ResolveEntityType([NotNull] string entityTypeName)
        {
            if (DeclaringEntityType.Name == entityTypeName)
            {
                return DeclaringEntityType;
            }

            if (RelatedEntityType.Name == entityTypeName)
            {
                return RelatedEntityType;
            }

            if (DeclaringEntityType.DisplayName() == entityTypeName)
            {
                return DeclaringEntityType;
            }

            return RelatedEntityType.DisplayName() == entityTypeName ? RelatedEntityType : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityType ResolveEntityType([NotNull] Type entityType)
        {
            if (DeclaringEntityType.ClrType == entityType)
            {
                return DeclaringEntityType;
            }

            return RelatedEntityType.ClrType == entityType ? RelatedEntityType : null;
        }

        private EntityType GetOtherEntityType(EntityType entityType)
            => DeclaringEntityType == entityType ? RelatedEntityType : DeclaringEntityType;

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

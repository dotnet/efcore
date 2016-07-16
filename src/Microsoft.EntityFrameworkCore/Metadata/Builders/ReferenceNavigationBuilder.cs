// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a relationship where configuration began on
    ///         an end of the relationship with a reference that points to an instance of another entity type.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class ReferenceNavigationBuilder : IInfrastructure<InternalRelationshipBuilder>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceNavigationBuilder(
            [NotNull] EntityType declaringEntityType,
            [NotNull] EntityType relatedEntityType,
            [CanBeNull] string navigationName,
            [NotNull] InternalRelationshipBuilder builder)
        {
            Check.NotNull(relatedEntityType, nameof(relatedEntityType));
            Check.NotNull(builder, nameof(builder));

            DeclaringEntityType = declaringEntityType;
            RelatedEntityType = relatedEntityType;
            ReferenceName = navigationName;
            Builder = builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceNavigationBuilder(
            [NotNull] EntityType declaringEntityType,
            [NotNull] EntityType relatedEntityType,
            [CanBeNull] PropertyInfo navigationProperty,
            [NotNull] InternalRelationshipBuilder builder)
        {
            Check.NotNull(relatedEntityType, nameof(relatedEntityType));
            Check.NotNull(builder, nameof(builder));

            DeclaringEntityType = declaringEntityType;
            RelatedEntityType = relatedEntityType;
            ReferenceProperty = navigationProperty;
            ReferenceName = navigationProperty?.Name;
            Builder = builder;
        }

        private InternalRelationshipBuilder Builder { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual string ReferenceName { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual PropertyInfo ReferenceProperty { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityType RelatedEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the internal builder being used to configure the relationship.
        /// </summary>
        InternalRelationshipBuilder IInfrastructure<InternalRelationshipBuilder>.Instance => Builder;

        /// <summary>
        ///     Configures this as a one-to-many relationship.
        /// </summary>
        /// <param name="collection">
        ///     The name of the collection navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceCollectionBuilder WithMany([CanBeNull] string collection = null)
            => new ReferenceCollectionBuilder(
                RelatedEntityType,
                DeclaringEntityType,
                WithManyBuilder(Check.NullButNotEmpty(collection, nameof(collection))));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder WithManyBuilder([CanBeNull] string navigationName)
            => WithManyBuilder(PropertyIdentity.Create(navigationName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder WithManyBuilder([CanBeNull] PropertyInfo navigationProperty)
            => WithManyBuilder(PropertyIdentity.Create(navigationProperty));

        private InternalRelationshipBuilder WithManyBuilder(PropertyIdentity collection)
        {
            var builder = Builder.RelatedEntityTypes(RelatedEntityType, DeclaringEntityType, ConfigurationSource.Explicit);
            var collectionName = collection.Name;
            if (builder.Metadata.IsUnique
                && builder.Metadata.PrincipalToDependent != null
                && builder.Metadata.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
                && collectionName != null)
            {
                ThrowForConflictingNavigation(builder.Metadata, collectionName, false);
            }

            builder = builder.IsUnique(false, ConfigurationSource.Explicit);
            var foreingKey = builder.Metadata;
            if (collectionName != null
                && foreingKey.PrincipalToDependent != null
                && foreingKey.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
                && foreingKey.PrincipalToDependent.Name != collectionName)
            {
                ThrowForConflictingNavigation(foreingKey, collectionName, false);
            }

            if (RelatedEntityType != foreingKey.PrincipalEntityType)
            {
                return collection.Property == null && ReferenceProperty == null
                    ? builder.Navigations(ReferenceName, collection.Name, RelatedEntityType, DeclaringEntityType, ConfigurationSource.Explicit)
                    : builder.Navigations(ReferenceProperty, collection.Property, RelatedEntityType, DeclaringEntityType, ConfigurationSource.Explicit);
            }

            return collection.Property != null
                ? builder.PrincipalToDependent(collection.Property, ConfigurationSource.Explicit)
                : builder.PrincipalToDependent(collection.Name, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     Configures this as a one-to-one relationship.
        /// </summary>
        /// <param name="reference">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceReferenceBuilder WithOne([CanBeNull] string reference = null)
            => new ReferenceReferenceBuilder(
                WithOneBuilder(Check.NullButNotEmpty(reference, nameof(reference))),
                DeclaringEntityType,
                RelatedEntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder WithOneBuilder([CanBeNull] string navigationName)
            => WithOneBuilder(PropertyIdentity.Create(navigationName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalRelationshipBuilder WithOneBuilder([CanBeNull] PropertyInfo navigationProperty)
            => WithOneBuilder(PropertyIdentity.Create(navigationProperty));

        private InternalRelationshipBuilder WithOneBuilder(PropertyIdentity reference)
        {
            var referenceName = reference.Name;
            if (!Builder.Metadata.IsUnique
                && Builder.Metadata.PrincipalToDependent != null
                && Builder.Metadata.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
                && referenceName != null)
            {
                ThrowForConflictingNavigation(Builder.Metadata, referenceName, false);
            }

            var builder = Builder.IsUnique(true, ConfigurationSource.Explicit);
            var foreingKey = builder.Metadata;
            if (foreingKey.IsSelfReferencing()
                && referenceName != null
                && ReferenceName == referenceName)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateNavigation(
                    referenceName, RelatedEntityType.DisplayName(), RelatedEntityType.DisplayName()));
            }

            var pointsToPrincipal = !foreingKey.IsSelfReferencing()
                                    && (!foreingKey.DeclaringEntityType.IsAssignableFrom(DeclaringEntityType)
                                        || !foreingKey.PrincipalEntityType.IsAssignableFrom(RelatedEntityType)
                                        || (foreingKey.DeclaringEntityType.IsAssignableFrom(RelatedEntityType)
                                            && foreingKey.PrincipalEntityType.IsAssignableFrom(DeclaringEntityType)
                                            && foreingKey.PrincipalToDependent != null
                                            && foreingKey.PrincipalToDependent.Name == ReferenceName));

            if (referenceName != null
                && ((pointsToPrincipal
                     && foreingKey.DependentToPrincipal != null
                     && foreingKey.GetDependentToPrincipalConfigurationSource() == ConfigurationSource.Explicit
                     && foreingKey.DependentToPrincipal.Name != referenceName)
                    || (!pointsToPrincipal
                        && foreingKey.PrincipalToDependent != null
                        && foreingKey.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
                        && foreingKey.PrincipalToDependent.Name != referenceName)))
            {
                ThrowForConflictingNavigation(foreingKey, referenceName, pointsToPrincipal);
            }

            if (referenceName != null)
            {
                if (pointsToPrincipal
                    && RelatedEntityType != foreingKey.DeclaringEntityType)
                {
                    return reference.Property == null && ReferenceProperty == null
                        ? builder.Navigations(reference.Name, ReferenceName, DeclaringEntityType, RelatedEntityType, ConfigurationSource.Explicit)
                        : builder.Navigations(reference.Property, ReferenceProperty, DeclaringEntityType, RelatedEntityType, ConfigurationSource.Explicit);
                }
                if (!pointsToPrincipal
                    && RelatedEntityType != foreingKey.PrincipalEntityType)
                {
                    return reference.Property == null && ReferenceProperty == null
                        ? builder.Navigations(ReferenceName, reference.Name, RelatedEntityType, DeclaringEntityType, ConfigurationSource.Explicit)
                        : builder.Navigations(ReferenceProperty, reference.Property, RelatedEntityType, DeclaringEntityType, ConfigurationSource.Explicit);
                }
            }

            var referenceProperty = reference.Property;
            if (referenceProperty != null)
            {
                return pointsToPrincipal
                    ? builder.DependentToPrincipal(referenceProperty, ConfigurationSource.Explicit)
                    : builder.PrincipalToDependent(referenceProperty, ConfigurationSource.Explicit);
            }

            return pointsToPrincipal
                ? builder.DependentToPrincipal(reference.Name, ConfigurationSource.Explicit)
                : builder.PrincipalToDependent(reference.Name, ConfigurationSource.Explicit);
        }

        private void ThrowForConflictingNavigation(ForeignKey foreingKey, string newInverseName, bool newToPrincipal)
        {
            throw new InvalidOperationException(CoreStrings.ConflictingRelationshipNavigation(
                foreingKey.PrincipalEntityType.DisplayName(),
                newToPrincipal ? foreingKey.PrincipalToDependent.Name : newInverseName,
                foreingKey.DeclaringEntityType.DisplayName(),
                newToPrincipal ? newInverseName : foreingKey.DependentToPrincipal.Name,
                foreingKey.PrincipalEntityType.DisplayName(),
                foreingKey.PrincipalToDependent.Name,
                foreingKey.DeclaringEntityType.DisplayName(),
                foreingKey.DependentToPrincipal.Name));
        }
    }
}

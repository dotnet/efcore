// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
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
            ReferenceName = navigationProperty?.GetSimpleMemberName();
            Builder = builder;
        }

        private InternalRelationshipBuilder Builder { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual string ReferenceName { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual PropertyInfo ReferenceProperty { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityType RelatedEntityType { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityType DeclaringEntityType { [DebuggerStepThrough] get; }

        /// <summary>
        ///     Gets the internal builder being used to configure the relationship.
        /// </summary>
        InternalRelationshipBuilder IInfrastructure<InternalRelationshipBuilder>.Instance => Builder;

        /// <summary>
        ///     <para>
        ///         Configures this as a one-to-many relationship.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
        ///     </para>
        /// </summary>
        /// <param name="collection">
        ///     The name of the collection navigation property on the other end of this relationship.
        ///     If null or not specified, there is no navigation property on the other end of the relationship.
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
            var foreignKey = builder.Metadata;
            if (collectionName != null
                && foreignKey.PrincipalToDependent != null
                && foreignKey.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
                && foreignKey.PrincipalToDependent.Name != collectionName)
            {
                ThrowForConflictingNavigation(foreignKey, collectionName, false);
            }

            return RelatedEntityType != foreignKey.PrincipalEntityType
                ? collection.Property == null && ReferenceProperty == null
                    ? builder.Navigations(ReferenceName, collection.Name, RelatedEntityType, DeclaringEntityType, ConfigurationSource.Explicit)
                    : builder.Navigations(ReferenceProperty, collection.Property, RelatedEntityType, DeclaringEntityType, ConfigurationSource.Explicit)
                : collection.Property != null
                    ? builder.PrincipalToDependent(collection.Property, ConfigurationSource.Explicit)
                    : builder.PrincipalToDependent(collection.Name, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     <para>
        ///         Configures this as a one-to-one relationship.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
        ///     </para>
        /// </summary>
        /// <param name="reference">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null or not specified, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceReferenceBuilder WithOne([CanBeNull] string reference = null)
            => new ReferenceReferenceBuilder(
                DeclaringEntityType,
                RelatedEntityType,
                WithOneBuilder(Check.NullButNotEmpty(reference, nameof(reference))));

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

            using (var batch = Builder.Metadata.DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
            {
                var builder = Builder.IsUnique(true, ConfigurationSource.Explicit);
                var foreignKey = builder.Metadata;
                if (foreignKey.IsSelfReferencing()
                    && referenceName != null
                    && ReferenceName == referenceName)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ConflictingPropertyOrNavigation(
                            referenceName, RelatedEntityType.DisplayName(), RelatedEntityType.DisplayName()));
                }

                var pointsToPrincipal = !foreignKey.IsSelfReferencing()
                                        && (!foreignKey.DeclaringEntityType.IsAssignableFrom(DeclaringEntityType)
                                            || !foreignKey.PrincipalEntityType.IsAssignableFrom(RelatedEntityType)
                                            || (foreignKey.DeclaringEntityType.IsAssignableFrom(RelatedEntityType)
                                                && foreignKey.PrincipalEntityType.IsAssignableFrom(DeclaringEntityType)
                                                && foreignKey.PrincipalToDependent != null
                                                && foreignKey.PrincipalToDependent.Name == ReferenceName));

                if (referenceName != null
                    && ((pointsToPrincipal
                         && foreignKey.DependentToPrincipal != null
                         && foreignKey.GetDependentToPrincipalConfigurationSource() == ConfigurationSource.Explicit
                         && foreignKey.DependentToPrincipal.Name != referenceName)
                        || (!pointsToPrincipal
                            && foreignKey.PrincipalToDependent != null
                            && foreignKey.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
                            && foreignKey.PrincipalToDependent.Name != referenceName)))
                {
                    ThrowForConflictingNavigation(foreignKey, referenceName, pointsToPrincipal);
                }

                var referenceProperty = reference.Property;
                if (referenceName != null
                    && pointsToPrincipal
                    && RelatedEntityType != foreignKey.DeclaringEntityType)
                {
                    builder = referenceProperty == null && ReferenceProperty == null
                        ? builder.Navigations(
                            referenceName, ReferenceName, DeclaringEntityType, RelatedEntityType, ConfigurationSource.Explicit)
                        : builder.Navigations(
                            referenceProperty, ReferenceProperty, DeclaringEntityType, RelatedEntityType, ConfigurationSource.Explicit);
                }
                else if (referenceName != null
                         && !pointsToPrincipal
                         && RelatedEntityType != foreignKey.PrincipalEntityType)
                {
                    builder = referenceProperty == null && ReferenceProperty == null
                        ? builder.Navigations(
                            ReferenceName, referenceName, RelatedEntityType, DeclaringEntityType, ConfigurationSource.Explicit)
                        : builder.Navigations(
                            ReferenceProperty, referenceProperty, RelatedEntityType, DeclaringEntityType, ConfigurationSource.Explicit);
                }
                else
                {
                    if (referenceProperty != null)
                    {
                        builder = pointsToPrincipal
                            ? builder.DependentToPrincipal(referenceProperty, ConfigurationSource.Explicit)
                            : builder.PrincipalToDependent(referenceProperty, ConfigurationSource.Explicit);
                    }
                    else
                    {
                        builder = pointsToPrincipal
                            ? builder.DependentToPrincipal(referenceName, ConfigurationSource.Explicit)
                            : builder.PrincipalToDependent(referenceName, ConfigurationSource.Explicit);
                    }
                }

                return batch.Run(builder);
            }
        }

        private static void ThrowForConflictingNavigation(ForeignKey foreignKey, string newInverseName, bool newToPrincipal)
        {
            throw new InvalidOperationException(
                CoreStrings.ConflictingRelationshipNavigation(
                    foreignKey.PrincipalEntityType.DisplayName(),
                    newToPrincipal ? foreignKey.PrincipalToDependent?.Name : newInverseName,
                    foreignKey.DeclaringEntityType.DisplayName(),
                    newToPrincipal ? newInverseName : foreignKey.DependentToPrincipal?.Name,
                    foreignKey.PrincipalEntityType.DisplayName(),
                    foreignKey.PrincipalToDependent?.Name,
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.DependentToPrincipal?.Name));
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}

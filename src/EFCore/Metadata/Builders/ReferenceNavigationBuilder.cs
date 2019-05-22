// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public ReferenceNavigationBuilder(
            [NotNull] IMutableEntityType declaringEntityType,
            [NotNull] IMutableEntityType relatedEntityType,
            [CanBeNull] string navigationName,
            [NotNull] IMutableForeignKey foreignKey)
        {
            Check.NotNull(relatedEntityType, nameof(relatedEntityType));
            Check.NotNull(foreignKey, nameof(foreignKey));

            DeclaringEntityType = declaringEntityType;
            RelatedEntityType = relatedEntityType;
            ReferenceName = navigationName;
            Builder = ((ForeignKey)foreignKey).Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public ReferenceNavigationBuilder(
            [NotNull] IMutableEntityType declaringEntityType,
            [NotNull] IMutableEntityType relatedEntityType,
            [CanBeNull] PropertyInfo navigationProperty,
            [NotNull] IMutableForeignKey foreignKey)
        {
            Check.NotNull(relatedEntityType, nameof(relatedEntityType));
            Check.NotNull(foreignKey, nameof(foreignKey));

            DeclaringEntityType = declaringEntityType;
            RelatedEntityType = relatedEntityType;
            ReferenceProperty = navigationProperty;
            ReferenceName = navigationProperty?.GetSimpleMemberName();
            Builder = ((ForeignKey)foreignKey).Builder;
        }

        private InternalRelationshipBuilder Builder { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual string ReferenceName { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual PropertyInfo ReferenceProperty { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual IMutableEntityType RelatedEntityType { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual IMutableEntityType DeclaringEntityType { [DebuggerStepThrough] get; }

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
                WithManyBuilder(Check.NullButNotEmpty(collection, nameof(collection))).Metadata);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalRelationshipBuilder WithManyBuilder([CanBeNull] string navigationName)
            => WithManyBuilder(PropertyIdentity.Create(navigationName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalRelationshipBuilder WithManyBuilder([CanBeNull] PropertyInfo navigationProperty)
            => WithManyBuilder(PropertyIdentity.Create(navigationProperty));

        private InternalRelationshipBuilder WithManyBuilder(PropertyIdentity collection)
        {
            var builder = Builder.HasEntityTypes(
                (EntityType)RelatedEntityType, (EntityType)DeclaringEntityType, ConfigurationSource.Explicit);
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
                ? collection.MemberInfo == null && ReferenceProperty == null
                    ? builder.HasNavigations(
                        ReferenceName, collection.Name,
                        (EntityType)RelatedEntityType, (EntityType)DeclaringEntityType, ConfigurationSource.Explicit)
                    : builder.HasNavigations(
                        ReferenceProperty, collection.MemberInfo,
                        (EntityType)RelatedEntityType, (EntityType)DeclaringEntityType, ConfigurationSource.Explicit)
                : collection.MemberInfo != null
                    ? builder.HasNavigation(
                        collection.MemberInfo,
                        pointsToPrincipal: false,
                        ConfigurationSource.Explicit)
                    : builder.HasNavigation(
                        collection.Name,
                        pointsToPrincipal: false,
                        ConfigurationSource.Explicit);
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
                WithOneBuilder(Check.NullButNotEmpty(reference, nameof(reference))).Metadata);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalRelationshipBuilder WithOneBuilder([CanBeNull] string navigationName)
            => WithOneBuilder(PropertyIdentity.Create(navigationName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
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

                var referenceProperty = reference.MemberInfo;
                if (referenceName != null
                    && pointsToPrincipal
                    && RelatedEntityType != foreignKey.DeclaringEntityType)
                {
                    builder = referenceProperty == null && ReferenceProperty == null
                        ? builder.HasNavigations(
                            referenceName, ReferenceName,
                            (EntityType)DeclaringEntityType, (EntityType)RelatedEntityType, ConfigurationSource.Explicit)
                        : builder.HasNavigations(
                            referenceProperty, ReferenceProperty,
                            (EntityType)DeclaringEntityType, (EntityType)RelatedEntityType, ConfigurationSource.Explicit);
                }
                else if (referenceName != null
                         && !pointsToPrincipal
                         && RelatedEntityType != foreignKey.PrincipalEntityType)
                {
                    builder = referenceProperty == null && ReferenceProperty == null
                        ? builder.HasNavigations(
                            ReferenceName, referenceName,
                            (EntityType)RelatedEntityType, (EntityType)DeclaringEntityType, ConfigurationSource.Explicit)
                        : builder.HasNavigations(
                            ReferenceProperty, referenceProperty,
                            (EntityType)RelatedEntityType, (EntityType)DeclaringEntityType, ConfigurationSource.Explicit);
                }
                else
                {
                    if (referenceProperty != null)
                    {
                        builder = builder.HasNavigation(
                            referenceProperty,
                            pointsToPrincipal,
                            ConfigurationSource.Explicit);
                    }
                    else
                    {
                        builder = builder.HasNavigation(
                            referenceName,
                            pointsToPrincipal,
                            ConfigurationSource.Explicit);
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
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}

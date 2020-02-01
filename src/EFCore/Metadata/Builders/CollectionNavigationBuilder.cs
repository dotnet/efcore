// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a relationship where configuration began on
    ///         an end of the relationship with a collection that contains instances of another entity type.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class CollectionNavigationBuilder : IInfrastructure<InternalRelationshipBuilder>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public CollectionNavigationBuilder(
            [NotNull] IMutableEntityType declaringEntityType,
            [NotNull] IMutableEntityType relatedEntityType,
            MemberIdentity navigation,
            [CanBeNull] IMutableForeignKey foreignKey,
            [CanBeNull] IMutableSkipNavigation skipNavigation)
        {
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));
            Check.NotNull(relatedEntityType, nameof(relatedEntityType));

            DeclaringEntityType = declaringEntityType;
            RelatedEntityType = relatedEntityType;
            CollectionMember = navigation.MemberInfo;
            CollectionName = navigation.Name;
            Builder = ((ForeignKey)foreignKey)?.Builder;
            SkipNavigation = skipNavigation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalRelationshipBuilder Builder { get; private set; }

        private IMutableSkipNavigation SkipNavigation { get; set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual string CollectionName { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual MemberInfo CollectionMember { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual IMutableEntityType RelatedEntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual IMutableEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     <para>
        ///         Gets the internal builder being used to configure the relationship.
        ///     </para>
        ///     <para>
        ///         This property is intended for use by extension methods that need to make use of services
        ///         not directly exposed in the public API surface.
        ///     </para>
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
        /// <param name="navigationName">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null or not specified, then there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceCollectionBuilder WithOne([CanBeNull] string navigationName = null)
            => new ReferenceCollectionBuilder(
                DeclaringEntityType,
                RelatedEntityType,
                WithOneBuilder(Check.NullButNotEmpty(navigationName, nameof(navigationName))).Metadata);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalRelationshipBuilder WithOneBuilder([CanBeNull] string navigationName)
            => WithOneBuilder(MemberIdentity.Create(navigationName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalRelationshipBuilder WithOneBuilder([CanBeNull] MemberInfo navigationMemberInfo)
            => WithOneBuilder(MemberIdentity.Create(navigationMemberInfo));

        private InternalRelationshipBuilder WithOneBuilder(MemberIdentity reference)
        {
            if (SkipNavigation != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingRelationshipNavigation(
                        SkipNavigation.DeclaringEntityType.DisplayName() + "." + SkipNavigation.Name,
                        RelatedEntityType.DisplayName() + (reference.Name == null
                                                        ? ""
                                                        : "." + reference.Name),
                        SkipNavigation.DeclaringEntityType.DisplayName() + "." + SkipNavigation.Name,
                        SkipNavigation.TargetEntityType.DisplayName() + (SkipNavigation.Inverse == null
                                                                        ? ""
                                                                        : "." + SkipNavigation.Inverse.Name)));
            }

            var foreignKey = Builder.Metadata;
            var referenceName = reference.Name;
            if (referenceName != null
                && foreignKey.DependentToPrincipal != null
                && foreignKey.GetDependentToPrincipalConfigurationSource() == ConfigurationSource.Explicit
                && foreignKey.DependentToPrincipal.Name != referenceName)
            {
                InternalRelationshipBuilder.ThrowForConflictingNavigation(foreignKey, referenceName, newToPrincipal: true);
            }

            return reference.MemberInfo == null || CollectionMember == null
                ? Builder.HasNavigations(
                    reference.Name, CollectionName,
                    (EntityType)DeclaringEntityType, (EntityType)RelatedEntityType,
                    ConfigurationSource.Explicit)
                : Builder.HasNavigations(
                    reference.MemberInfo, CollectionMember,
                    (EntityType)DeclaringEntityType, (EntityType)RelatedEntityType,
                    ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     <para>
        ///         Configures this as a many-to-many relationship.
        ///     </para>
        /// </summary>
        /// <param name="navigationName">
        ///     The name of the collection navigation property on the other end of this relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual CollectionCollectionBuilder WithMany([NotNull] string navigationName)
        {
            var leftName = Builder?.Metadata.PrincipalToDependent.Name;
            return new CollectionCollectionBuilder(
                           RelatedEntityType,
                           DeclaringEntityType,
                           WithLeftManyNavigation(navigationName),
                           WithRightManyNavigation(navigationName, leftName));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual IMutableSkipNavigation WithLeftManyNavigation([NotNull] MemberInfo inverseMemberInfo)
            => WithLeftManyNavigation(inverseMemberInfo.Name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual IMutableSkipNavigation WithLeftManyNavigation([NotNull] string inverseName)
        {
            Check.NotEmpty(inverseName, nameof(inverseName));

            if (SkipNavigation != null)
            {
                return SkipNavigation;
            }

            var foreignKey = Builder.Metadata;
            var navigationMember = foreignKey.PrincipalToDependent.CreateMemberIdentity();
            if (foreignKey.GetDependentToPrincipalConfigurationSource() == ConfigurationSource.Explicit)
            {
                InternalRelationshipBuilder.ThrowForConflictingNavigation(
                    foreignKey, DeclaringEntityType, RelatedEntityType, navigationMember.Name, inverseName);
            }

            using (foreignKey.DeclaringEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                foreignKey.DeclaringEntityType.RemoveForeignKey(foreignKey);
                Builder = null;
                return ((EntityType)DeclaringEntityType).Builder.HasSkipNavigation(
                    navigationMember,
                    (EntityType)RelatedEntityType,
                    ConfigurationSource.Explicit).Metadata;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual IMutableSkipNavigation WithRightManyNavigation([NotNull] string navigationName, [NotNull] string inverseName)
            => WithRightManyNavigation(MemberIdentity.Create(navigationName), inverseName);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual IMutableSkipNavigation WithRightManyNavigation([NotNull] MemberInfo navigationMemberInfo, [NotNull] string inverseName)
            => WithRightManyNavigation(MemberIdentity.Create(navigationMemberInfo), inverseName);

        private IMutableSkipNavigation WithRightManyNavigation(MemberIdentity navigationMember, [NotNull] string inverseName)
        {
            Check.DebugAssert(Builder == null, "Expected no associated foreign key at this point");

            var navigationName = navigationMember.Name;
            var conflictingNavigation = RelatedEntityType.FindNavigation(navigationName) as IConventionNavigation;
            var foreignKey = (ForeignKey)conflictingNavigation?.ForeignKey;
            if (conflictingNavigation?.GetConfigurationSource() == ConfigurationSource.Explicit)
            {
                InternalRelationshipBuilder.ThrowForConflictingNavigation(
                    foreignKey, DeclaringEntityType, RelatedEntityType, inverseName, navigationName);
            }

            using (((EntityType)RelatedEntityType).Model.ConventionDispatcher.DelayConventions())
            {
                if (conflictingNavigation != null)
                {
                    foreignKey.DeclaringEntityType.RemoveForeignKey(foreignKey);
                }
                else
                {
                    var skipNavigation = RelatedEntityType.FindSkipNavigation(navigationMember.Name);
                    if (skipNavigation != null)
                    {
                        return skipNavigation;
                    }
                }

                return ((EntityType)RelatedEntityType).Builder.HasSkipNavigation(
                    navigationMember,
                    (EntityType)DeclaringEntityType,
                    ConfigurationSource.Explicit).Metadata;
            }
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

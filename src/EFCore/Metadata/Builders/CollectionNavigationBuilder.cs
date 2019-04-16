// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CollectionNavigationBuilder(
            [NotNull] EntityType declaringEntityType,
            [NotNull] EntityType relatedEntityType,
            [CanBeNull] string navigationName,
            [NotNull] InternalRelationshipBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            DeclaringEntityType = declaringEntityType;
            RelatedEntityType = relatedEntityType;
            CollectionName = navigationName;
            Builder = builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CollectionNavigationBuilder(
            [NotNull] EntityType declaringEntityType,
            [NotNull] EntityType relatedEntityType,
            [CanBeNull] PropertyInfo navigationProperty,
            [NotNull] InternalRelationshipBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            DeclaringEntityType = declaringEntityType;
            RelatedEntityType = relatedEntityType;
            CollectionProperty = navigationProperty;
            CollectionName = navigationProperty?.GetSimpleMemberName();
            Builder = builder;
        }

        private InternalRelationshipBuilder Builder { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual string CollectionName { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual PropertyInfo CollectionProperty { get; }

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
                WithOneBuilder(Check.NullButNotEmpty(navigationName, nameof(navigationName))));

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
            var foreignKey = Builder.Metadata;
            var referenceName = reference.Name;
            if (referenceName != null
                && foreignKey.DependentToPrincipal != null
                && foreignKey.GetDependentToPrincipalConfigurationSource() == ConfigurationSource.Explicit
                && foreignKey.DependentToPrincipal.Name != referenceName)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingRelationshipNavigation(
                        foreignKey.PrincipalEntityType.DisplayName(),
                        foreignKey.PrincipalToDependent.Name,
                        foreignKey.DeclaringEntityType.DisplayName(),
                        referenceName,
                        foreignKey.PrincipalEntityType.DisplayName(),
                        foreignKey.PrincipalToDependent.Name,
                        foreignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.DependentToPrincipal.Name));
            }

            return referenceName != null
                   && RelatedEntityType != foreignKey.DeclaringEntityType
                ? reference.Property == null && CollectionProperty == null
                    ? Builder.Navigations(reference.Name, CollectionName, DeclaringEntityType, RelatedEntityType, ConfigurationSource.Explicit)
                    : Builder.Navigations(reference.Property, CollectionProperty, DeclaringEntityType, RelatedEntityType, ConfigurationSource.Explicit)
                : reference.Property == null
                    ? Builder.DependentToPrincipal(reference.Name, ConfigurationSource.Explicit)
                    : Builder.DependentToPrincipal(reference.Property, ConfigurationSource.Explicit);
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

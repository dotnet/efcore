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
        public CollectionNavigationBuilder([NotNull] InternalRelationshipBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
        }

        private InternalRelationshipBuilder Builder { get; }

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
        ///     Configures this as a one-to-many relationship.
        /// </summary>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceCollectionBuilder WithOne([CanBeNull] string navigationName = null)
            => new ReferenceCollectionBuilder(WithOneBuilder(Check.NullButNotEmpty(navigationName, nameof(navigationName))));

        /// <summary>
        ///     Returns the internal builder to be used when <see cref="WithOne" /> is called.
        /// </summary>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> The internal builder to further configure the relationship. </returns>
        protected virtual InternalRelationshipBuilder WithOneBuilder([CanBeNull] string navigationName)
            => WithOneBuilder(PropertyIdentity.Create(navigationName));

        /// <summary>
        ///     Returns the internal builder to be used when <see cref="WithOne" /> is called.
        /// </summary>
        /// <param name="navigationProperty">
        ///     The reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> The internal builder to further configure the relationship. </returns>
        protected virtual InternalRelationshipBuilder WithOneBuilder([CanBeNull] PropertyInfo navigationProperty)
            => WithOneBuilder(PropertyIdentity.Create(navigationProperty));

        private InternalRelationshipBuilder WithOneBuilder([CanBeNull] PropertyIdentity reference)
        {
            var foreingKey = Builder.Metadata;
            var referenceName = reference.Name;
            if (referenceName != null
                && foreingKey.DependentToPrincipal != null
                && foreingKey.GetDependentToPrincipalConfigurationSource() == ConfigurationSource.Explicit
                && foreingKey.DependentToPrincipal.Name != referenceName)
            {
                throw new InvalidOperationException(CoreStrings.ConflictingRelationshipNavigation(
                    foreingKey.PrincipalEntityType.DisplayName(),
                    foreingKey.PrincipalToDependent.Name,
                    foreingKey.DeclaringEntityType.DisplayName(),
                    referenceName,
                    foreingKey.PrincipalEntityType.DisplayName(),
                    foreingKey.PrincipalToDependent.Name,
                    foreingKey.DeclaringEntityType.DisplayName(),
                    foreingKey.DependentToPrincipal.Name));
            }

            return reference.Property == null
                ? Builder.DependentToPrincipal(reference.Name, ConfigurationSource.Explicit)
                : Builder.DependentToPrincipal(reference.Property, ConfigurationSource.Explicit);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Builders
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
    public class ReferenceNavigationBuilder : IAccessor<InternalRelationshipBuilder>
    {
        private readonly InternalRelationshipBuilder _builder;

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="ReferenceNavigationBuilder" /> class.
        ///     </para>
        ///     <para>
        ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
        ///         and it is not designed to be directly constructed in your application code.
        ///     </para>
        /// </summary>
        /// <param name="relatedEntityType"> The entity type that the reference points to. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on the end of the relationship that configuration began
        ///     on. If null, there is no navigation property on this end of the relationship.
        /// </param>
        /// <param name="builder"> The internal builder being used to configure the relationship. </param>
        public ReferenceNavigationBuilder(
            [NotNull] EntityType relatedEntityType,
            [CanBeNull] string navigationName,
            [NotNull] InternalRelationshipBuilder builder)
        {
            Check.NotNull(relatedEntityType, nameof(relatedEntityType));
            Check.NotNull(builder, nameof(builder));

            RelatedEntityType = relatedEntityType;
            ReferenceName = navigationName;
            _builder = builder;
        }

        /// <summary>
        ///     Gets the name of the reference navigation property on the end of the relationship that
        ///     configuration began on. If null, there is no navigation property on this end of the relationship.
        /// </summary>
        protected virtual string ReferenceName { get; }

        /// <summary>
        ///     Gets the entity type that the reference points to.
        /// </summary>
        protected virtual EntityType RelatedEntityType { get; }

        /// <summary>
        ///     Gets the internal builder being used to configure the relationship.
        /// </summary>
        InternalRelationshipBuilder IAccessor<InternalRelationshipBuilder>.Service => _builder;

        /// <summary>
        ///     Configures this as a one-to-many relationship.
        /// </summary>
        /// <param name="collection">
        ///     The name of the collection navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceCollectionBuilder InverseCollection([CanBeNull] string collection = null)
            => new ReferenceCollectionBuilder(InverseCollectionBuilder(collection));

        /// <summary>
        ///     Returns the internal builder to be used when <see cref="InverseCollection" /> is called.
        /// </summary>
        /// <param name="collection">
        ///     The name of the collection navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> The internal builder to further configure the relationship. </returns>
        protected virtual InternalRelationshipBuilder InverseCollectionBuilder([CanBeNull] string collection)
        {
            var needToInvert = _builder.Metadata.PrincipalEntityType != RelatedEntityType;
            Debug.Assert((needToInvert && _builder.Metadata.EntityType == RelatedEntityType)
                         || _builder.Metadata.PrincipalEntityType == RelatedEntityType);

            var builder = Builder;
            if (needToInvert)
            {
                builder = builder.Invert(ConfigurationSource.Explicit);
            }

            if (((IForeignKey)_builder.Metadata).IsUnique)
            {
                builder = builder.NavigationToDependent(null, ConfigurationSource.Explicit);
            }

            builder = builder.Unique(false, ConfigurationSource.Explicit);

            return builder.NavigationToDependent(collection, ConfigurationSource.Explicit, strictPreferExisting: true);
        }

        /// <summary>
        ///     Configures this as a one-to-one relationship.
        /// </summary>
        /// <param name="reference">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceReferenceBuilder InverseReference([CanBeNull] string reference = null)
            => new ReferenceReferenceBuilder(InverseReferenceBuilder(reference));

        /// <summary>
        ///     Returns the internal builder to be used when <see cref="InverseReference" /> is called.
        /// </summary>
        /// <param name="reference">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> The internal builder to further configure the relationship. </returns>
        protected virtual InternalRelationshipBuilder InverseReferenceBuilder([CanBeNull] string reference)
        {
            var builder = Builder;
            if (!((IForeignKey)_builder.Metadata).IsUnique)
            {
                Debug.Assert(_builder.Metadata.DependentToPrincipal?.Name == ReferenceName);

                builder = builder.NavigationToDependent(null, ConfigurationSource.Explicit);
            }

            builder = builder.Unique(true, ConfigurationSource.Explicit);
            var foreignKey = builder.Metadata;

            var inverseToPrincipal = !foreignKey.IsSelfReferencing()
                                     && foreignKey.EntityType == RelatedEntityType
                                     && foreignKey.PrincipalToDependent?.Name == ReferenceName;

            Debug.Assert(inverseToPrincipal
                         || (foreignKey.PrincipalEntityType == RelatedEntityType
                             && foreignKey.DependentToPrincipal?.Name == ReferenceName));
            builder = inverseToPrincipal
                ? builder.NavigationToPrincipal(reference, ConfigurationSource.Explicit, strictPreferExisting: false)
                : builder.NavigationToDependent(reference, ConfigurationSource.Explicit, strictPreferExisting: false);

            return builder;
        }

        private InternalRelationshipBuilder Builder => this.GetService();
    }
}

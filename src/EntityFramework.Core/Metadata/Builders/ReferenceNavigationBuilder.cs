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
            Builder = builder;
        }

        private InternalRelationshipBuilder Builder { get; }

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
        InternalRelationshipBuilder IAccessor<InternalRelationshipBuilder>.Service => Builder;

        /// <summary>
        ///     Configures this as a one-to-many relationship.
        /// </summary>
        /// <param name="collection">
        ///     The name of the collection navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceCollectionBuilder WithMany([CanBeNull] string collection = null)
            => new ReferenceCollectionBuilder(WithManyBuilder(collection));

        /// <summary>
        ///     Returns the internal builder to be used when <see cref="WithMany" /> is called.
        /// </summary>
        /// <param name="collection">
        ///     The name of the collection navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> The internal builder to further configure the relationship. </returns>
        protected virtual InternalRelationshipBuilder WithManyBuilder([CanBeNull] string collection)
        {
            var needToInvert = Builder.Metadata.PrincipalEntityType != RelatedEntityType;

            Debug.Assert(!needToInvert
                         || Builder.Metadata.DeclaringEntityType == RelatedEntityType);
                         
            var builder = Builder;
            if (needToInvert)
            {
                builder = builder.Invert(ConfigurationSource.Explicit);
            }

            if (((IForeignKey)Builder.Metadata).IsUnique)
            {
                builder = builder.PrincipalToDependent(null, ConfigurationSource.Explicit);
            }

            builder = builder.IsUnique(false, ConfigurationSource.Explicit);

            return builder.PrincipalToDependent(collection, ConfigurationSource.Explicit, strictPrincipal: true);
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
            => new ReferenceReferenceBuilder(WithOneBuilder(reference));

        /// <summary>
        ///     Returns the internal builder to be used when <see cref="WithOne" /> is called.
        /// </summary>
        /// <param name="reference">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> The internal builder to further configure the relationship. </returns>
        protected virtual InternalRelationshipBuilder WithOneBuilder([CanBeNull] string reference)
        {
            var builder = Builder.IsUnique(true, ConfigurationSource.Explicit);
            var foreignKey = builder.Metadata;

            var isSelfReferencing = foreignKey.IsSelfReferencing();
            var inverseToPrincipal = (isSelfReferencing
                                      || foreignKey.DeclaringEntityType == RelatedEntityType)
                                     && (!isSelfReferencing
                                         || ReferenceName != null)
                                     && foreignKey.PrincipalToDependent?.Name == ReferenceName;

            Debug.Assert(inverseToPrincipal
                         || (!isSelfReferencing
                             && foreignKey.PrincipalEntityType == RelatedEntityType)
                         || (isSelfReferencing
                             && ReferenceName == null)
                         || foreignKey.DependentToPrincipal?.Name == ReferenceName);

            return inverseToPrincipal
                ? builder.DependentToPrincipal(reference, ConfigurationSource.Explicit, strictPrincipal: isSelfReferencing)
                : builder.PrincipalToDependent(reference, ConfigurationSource.Explicit, strictPrincipal: isSelfReferencing);
        }
    }
}

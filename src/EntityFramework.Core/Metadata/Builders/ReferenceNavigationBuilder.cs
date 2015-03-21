// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
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
    public class ReferenceNavigationBuilder
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
        /// <param name="referenceName">
        ///     The name of the reference navigation property on the end of the relationship that configuration began
        ///     on. If null, there is no navigation property on this end of the relationship.
        /// </param>
        /// <param name="builder"> The internal builder being used to configure the relationship. </param>
        public ReferenceNavigationBuilder(
            [NotNull] EntityType relatedEntityType,
            [CanBeNull] string referenceName,
            [NotNull] InternalRelationshipBuilder builder)
        {
            Check.NotNull(relatedEntityType, nameof(relatedEntityType));
            Check.NotNull(builder, nameof(builder));

            RelatedEntityType = relatedEntityType;
            ReferenceName = referenceName;
            Builder = builder;
        }

        /// <summary>
        ///     Gets or sets the name of the reference navigation property on the end of the relationship that
        ///     configuration began on. If null, there is no navigation property on this end of the relationship.
        /// </summary>
        protected string ReferenceName { get; set; }

        /// <summary>
        ///     Gets or sets the entity type that the reference points to.
        /// </summary>
        protected EntityType RelatedEntityType { get; set; }

        /// <summary>
        ///     The foreign key that represents this relationship.
        /// </summary>
        public virtual ForeignKey Metadata => Builder.Metadata;

        /// <summary>
        ///     Gets the internal builder being used to configure the relationship.
        /// </summary>
        protected virtual InternalRelationshipBuilder Builder { get; }

        /// <summary>
        ///     Configures this as a one-to-many relationship.
        /// </summary>
        /// <param name="collection">
        ///     The name of the collection navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ManyToOneBuilder WithMany([CanBeNull] string collection = null) => new ManyToOneBuilder(WithManyBuilder(collection));

        /// <summary>
        ///     Returns the internal builder to be used when <see cref="WithMany" /> is called.
        /// </summary>
        /// <param name="collection">
        ///     The name of the collection navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> The internal builder to further configure the relationship. </returns>
        protected InternalRelationshipBuilder WithManyBuilder(string collection)
        {
            var needToInvert = Metadata.ReferencedEntityType != RelatedEntityType;
            Debug.Assert((needToInvert && Metadata.EntityType == RelatedEntityType)
                         || Metadata.ReferencedEntityType == RelatedEntityType);

            var builder = Builder;
            if (needToInvert)
            {
                builder = builder.Invert(ConfigurationSource.Explicit);
            }

            if (((IForeignKey)Metadata).IsUnique)
            {
                builder = builder.NavigationToDependent(null, ConfigurationSource.Explicit);
            }

            builder = builder.Unique(false, ConfigurationSource.Explicit);

            return builder.NavigationToDependent(collection, ConfigurationSource.Explicit, strictPreferExisting: true);
        }

        /// <summary>
        ///     Configures this as a one-to-one relationship.
        /// </summary>
        /// <param name="inverseReference">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual OneToOneBuilder WithOne([CanBeNull] string inverseReference = null) => new OneToOneBuilder(WithOneBuilder(inverseReference));

        /// <summary>
        ///     Returns the internal builder to be used when <see cref="WithOne" /> is called.
        /// </summary>
        /// <param name="inverseReferenceName">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> The internal builder to further configure the relationship. </returns>
        protected InternalRelationshipBuilder WithOneBuilder(string inverseReferenceName)
        {
            var builder = Builder;
            if (!((IForeignKey)Metadata).IsUnique)
            {
                Debug.Assert(Metadata.GetNavigationToPrincipal()?.Name == ReferenceName);

                builder = builder.NavigationToDependent(null, ConfigurationSource.Explicit);
            }

            builder = builder.Unique(true, ConfigurationSource.Explicit);
            var foreignKey = builder.Metadata;

            var inverseToPrincipal = !foreignKey.IsSelfReferencing()
                                     && foreignKey.EntityType == RelatedEntityType
                                     && foreignKey.GetNavigationToDependent()?.Name == ReferenceName;

            Debug.Assert(inverseToPrincipal
                         || (foreignKey.ReferencedEntityType == RelatedEntityType
                             && foreignKey.GetNavigationToPrincipal()?.Name == ReferenceName));
            builder = inverseToPrincipal
                ? builder.NavigationToPrincipal(inverseReferenceName, ConfigurationSource.Explicit, strictPreferExisting: false)
                : builder.NavigationToDependent(inverseReferenceName, ConfigurationSource.Explicit, strictPreferExisting: false);

            return builder;
        }
    }
}

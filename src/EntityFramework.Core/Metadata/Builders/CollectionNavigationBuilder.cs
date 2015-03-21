// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Builders
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
    public class CollectionNavigationBuilder
    {
        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="CollectionNavigationBuilder" /> class.
        ///     </para>
        ///     <para>
        ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
        ///         and it is not designed to be directly constructed in your application code.
        ///     </para>
        /// </summary>
        /// <param name="builder"> The internal builder being used to configure the relationship. </param>
        public CollectionNavigationBuilder([NotNull] InternalRelationshipBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
        }

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
        /// <param name="reference">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual OneToManyBuilder WithOne([CanBeNull] string reference = null) => new OneToManyBuilder(WithOneBuilder(reference));

        /// <summary>
        ///     Returns the internal builder to be used when <see cref="WithOne" /> is called.
        /// </summary>
        /// <param name="reference">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> The internal builder to further configure the relationship. </returns>
        protected InternalRelationshipBuilder WithOneBuilder(string reference) => Builder.NavigationToPrincipal(
            reference,
            ConfigurationSource.Explicit,
            strictPreferExisting: true);
    }
}

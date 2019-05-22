// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a primary or alternate key on an entity.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IKey" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IConventionKey : IConventionAnnotatable, IKey
    {
        /// <summary>
        ///     Gets the builder that can be used to configure this key.
        /// </summary>
        IConventionKeyBuilder Builder { get; }

        /// <summary>
        ///     Gets the properties that make up the key.
        /// </summary>
        new IReadOnlyList<IConventionProperty> Properties { get; }

        /// <summary>
        ///     Gets the entity type the key is defined on. This may be different from the type that <see cref="Properties" />
        ///     are defined on when the key is defined a derived type in an inheritance hierarchy (since the properties
        ///     may be defined on a base type).
        /// </summary>
        new IConventionEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Returns the configuration source for this key.
        /// </summary>
        /// <returns> The configuration source. </returns>
        ConfigurationSource GetConfigurationSource();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents an index on a set of properties.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IIndex" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IConventionIndex : IIndex, IConventionAnnotatable
    {
        /// <summary>
        ///     Gets the builder that can be used to configure this index.
        /// </summary>
        new IConventionIndexBuilder Builder { get; }

        /// <summary>
        ///     Gets the properties that this index is defined on.
        /// </summary>
        new IReadOnlyList<IConventionProperty> Properties { get; }

        /// <summary>
        ///     Gets the entity type the index is defined on. This may be different from the type that <see cref="Properties" />
        ///     are defined on when the index is defined a derived type in an inheritance hierarchy (since the properties
        ///     may be defined on a base type).
        /// </summary>
        new IConventionEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Returns the configuration source for this index.
        /// </summary>
        /// <returns> The configuration source. </returns>
        ConfigurationSource GetConfigurationSource();

        /// <summary>
        ///     Sets a value indicating whether the values assigned to the index properties are unique.
        /// </summary>
        /// <param name="unique"> A value indicating whether the values assigned to the index properties are unique. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured uniqueness. </returns>
        bool? SetIsUnique(bool? unique, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IIndex.IsUnique" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IIndex.IsUnique" />. </returns>
        ConfigurationSource? GetIsUniqueConfigurationSource();
    }
}

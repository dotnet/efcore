// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents an index on a set of properties.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IIndex" /> represents a ready-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableIndex : IIndex, IMutableAnnotatable, IMutableMetadataElement, IMutableMetadataProperties
    {
        /// <summary>
        ///     Gets or sets a value indicating whether the values assigned to the indexed properties are unique.
        /// </summary>
        new bool IsUnique { get; set; }
    }
}

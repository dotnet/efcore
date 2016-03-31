// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents an index on a set of properties.
    /// </summary>
    public interface IIndex : IAnnotatable, IMetadataElement, IMetadataProperties
    {
        /// <summary>
        ///     Gets a value indicating whether the values assigned to the indexed properties are unique.
        /// </summary>
        bool IsUnique { get; }
    }
}

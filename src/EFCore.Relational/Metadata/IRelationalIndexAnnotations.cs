// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IIndex)" />.
    /// </summary>
    public interface IRelationalIndexAnnotations
    {
        /// <summary>
        ///     The index name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The index filter expression.
        /// </summary>
        string Filter { get; }
    }
}

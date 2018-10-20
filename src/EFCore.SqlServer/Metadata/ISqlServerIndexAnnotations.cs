// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="SqlServerMetadataExtensions.SqlServer(IIndex)" />.
    /// </summary>
    public interface ISqlServerIndexAnnotations : IRelationalIndexAnnotations
    {
        /// <summary>
        ///     Indicates whether or not the index is clustered, or <c>null</c> if clustering has not
        ///     been specified.
        /// </summary>
        bool? IsClustered { get; }

        /// <summary>
        ///     Returns included property names, or <c>null</c> if they have not been specified.
        /// </summary>
        IReadOnlyList<string> IncludeProperties { get; }

        /// <summary>
        ///     Indicates whether or not the index is created with online option, or <c>null</c> if
        ///     online option has not been specified.
        /// </summary>
        bool? IsOnline { get; }
    }
}

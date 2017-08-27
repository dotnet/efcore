// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="SqlServerMetadataExtensions.SqlServer(IEntityType)" />.
    /// </summary>
    public interface ISqlServerEntityTypeAnnotations : IRelationalEntityTypeAnnotations
    {
        /// <summary>
        ///     Indicates whether or not the type is mapped to a memory-optimized table.
        /// </summary>
        bool IsMemoryOptimized { get; }
    }
}

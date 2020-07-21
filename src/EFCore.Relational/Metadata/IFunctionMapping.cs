// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents entity type mapping to a function.
    /// </summary>
    public interface IFunctionMapping : ITableMappingBase
    {
        /// <summary>
        ///     Gets the value indicating whether this is the function mapping
        ///     that should be used when the entity type is queried.
        /// </summary>
        bool IsDefaultFunctionMapping { get; }

        /// <summary>
        ///     Gets the target function.
        /// </summary>
        IStoreFunction StoreFunction { get; }

        /// <summary>
        ///     Gets the target function.
        /// </summary>
        IDbFunction DbFunction { get; }

        /// <summary>
        ///     Gets the properties mapped to columns on the target function.
        /// </summary>
        new IEnumerable<IFunctionColumnMapping> ColumnMappings { get; }
    }
}

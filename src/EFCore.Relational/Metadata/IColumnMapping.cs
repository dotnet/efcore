// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents property mapping to a column.
    /// </summary>
    public interface IColumnMapping : IColumnMappingBase
    {
        /// <summary>
        ///     Gets the target column.
        /// </summary>
        new IColumn Column { get; }

        /// <summary>
        ///     Gets the containing table mapping.
        /// </summary>
        new ITableMapping TableMapping { get; }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents property mapping to a SQL query column.
    /// </summary>
    public interface ISqlQueryColumnMapping : IColumnMappingBase
    {
        /// <summary>
        ///     Gets the target column.
        /// </summary>
        new ISqlQueryColumn Column { get; }

        /// <summary>
        ///     Gets the containing SQL query mapping.
        /// </summary>
        ISqlQueryMapping SqlQueryMapping { get; }
    }
}

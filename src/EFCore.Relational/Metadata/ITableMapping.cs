// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents entity type mapping to a table.
    /// </summary>
    public interface ITableMapping : ITableMappingBase
    {
        /// <summary>
        ///     The target table.
        /// </summary>
        new ITable Table { get; }

        /// <summary>
        ///     The properties mapped to columns on the target table.
        /// </summary>
        new IEnumerable<IColumnMapping> ColumnMappings { get; }
    }
}

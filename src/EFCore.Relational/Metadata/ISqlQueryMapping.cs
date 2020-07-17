// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents entity type mapping to a SQL query.
    /// </summary>
    public interface ISqlQueryMapping : ITableMappingBase
    {
        /// <summary>
        ///     Gets the value indicating whether this is the SQL query mapping
        ///     that should be used when the entity type is queried.
        /// </summary>
        bool IsDefaultSqlQueryMapping { get; set; }

        /// <summary>
        ///     Gets the target SQL query.
        /// </summary>
        ISqlQuery SqlQuery { get; }

        /// <summary>
        ///     Gets the properties mapped to columns on the target SQL query.
        /// </summary>
        new IEnumerable<ISqlQueryColumnMapping> ColumnMappings { get; }
    }
}

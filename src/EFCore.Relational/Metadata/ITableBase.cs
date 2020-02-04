// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a table-like object in the database.
    /// </summary>
    public interface ITableBase : IAnnotatable
    {
        /// <summary>
        ///     The name of the table in the database.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The schema of the table in the database.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     The entity type mappings.
        /// </summary>
        IEnumerable<ITableMappingBase> EntityTypeMappings { get; }

        /// <summary>
        ///     The columns defined for this table.
        /// </summary>
        IEnumerable<IColumnBase> Columns { get; }
    }
}

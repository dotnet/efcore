// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents entity type mapping to a table-like object.
    /// </summary>
    public interface ITableMappingBase : IAnnotatable
    {
        /// <summary>
        ///     Gets the mapped entity type.
        /// </summary>
        IEntityType EntityType { get; }

        /// <summary>
        ///     Gets the target table-like object.
        /// </summary>
        ITableBase Table { get; }

        /// <summary>
        ///     Gets the properties mapped to columns on the target table.
        /// </summary>
        IEnumerable<IColumnMappingBase> ColumnMappings { get; }

        /// <summary>
        ///     Gets the value indicating whether this is the mapping for the principal entity type
        ///     if the table is shared or the root entity type when using TPH.
        /// </summary>
        bool IsMainEntityTypeMapping { get; }

        /// <summary>
        ///     Gets the value indicating whether this is the mapping for the table
        ///     that should be used first when the entity type is split.
        /// </summary>
        bool IsMainTableMapping { get; }

        /// <summary>
        ///     Gets the value indicating whether the inherited properties use the same mapping.
        /// </summary>
        bool IncludesDerivedTypes { get; }
    }
}

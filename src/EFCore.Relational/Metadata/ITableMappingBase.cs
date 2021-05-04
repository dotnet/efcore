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
        ///     if the table-like object is shared.
        /// </summary>
        bool IsSharedTablePrincipal { get; }

        /// <summary>
        ///     Gets the value indicating whether this is the mapping for the principal table-like object
        ///     if the entity type is split.
        /// </summary>
        bool IsSplitEntityTypePrincipal { get; }

        /// <summary>
        ///     Gets the value indicating whether the mapped table-like object includes rows for the derived entity types.
        ///     Set to <see langword="false" /> for inherited mappings.
        /// </summary>
        bool IncludesDerivedTypes { get; }
    }
}

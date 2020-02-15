// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        ///     Returns a value indicating whether multiple entity types are sharing the rows in the table.
        /// </summary>
        bool IsSplit { get; }

        /// <summary>
        ///     The entity type mappings.
        /// </summary>
        IEnumerable<ITableMappingBase> EntityTypeMappings { get; }

        /// <summary>
        ///     The columns defined for this table.
        /// </summary>
        IEnumerable<IColumnBase> Columns { get; }

        /// <summary>
        ///     Returns the column with a given name. Returns <c>null</c> if no column with the given name is defined.
        /// </summary>
        IColumnBase FindColumn([NotNull] string name);

        /// <summary>
        ///     Returns the foreign keys for the given entity type that point to other entity types sharing this table.
        /// </summary>
        IEnumerable<IForeignKey> GetInternalForeignKeys([NotNull] IEntityType entityType);

        /// <summary>
        ///     Returns the foreign keys referencing the given entity type from other entity types sharing this table.
        /// </summary>
        IEnumerable<IForeignKey> GetReferencingInternalForeignKeys([NotNull] IEntityType entityType);
    }
}

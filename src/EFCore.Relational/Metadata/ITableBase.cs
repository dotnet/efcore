// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a table-like object in the database.
    /// </summary>
    public interface ITableBase : IAnnotatable
    {
        /// <summary>
        ///     Gets the name of the table in the database.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the schema of the table in the database.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     Gets the database model.
        /// </summary>
        IRelationalModel Model { get; }

        /// <summary>
        ///     Gets the value indicating whether multiple entity types are sharing the rows in the table.
        /// </summary>
        bool IsShared { get; }

        /// <summary>
        ///     Gets the entity type mappings.
        /// </summary>
        IEnumerable<ITableMappingBase> EntityTypeMappings { get; }

        /// <summary>
        ///     Gets the columns defined for this table.
        /// </summary>
        IEnumerable<IColumnBase> Columns { get; }

        /// <summary>
        ///     Gets the column with the given name. Returns <see langword="null" /> if no column with the given name is defined.
        /// </summary>
        IColumnBase FindColumn([NotNull] string name);

        /// <summary>
        ///     Gets the column mapped to the given property. Returns <see langword="null" /> if no column is mapped to the given property.
        /// </summary>
        IColumnBase FindColumn([NotNull] IProperty property);

        /// <summary>
        ///     Gets the foreign keys for the given entity type that point to other entity types sharing this table.
        /// </summary>
        IEnumerable<IForeignKey> GetRowInternalForeignKeys([NotNull] IEntityType entityType);

        /// <summary>
        ///     Gets the foreign keys referencing the given entity type from other entity types sharing this table.
        /// </summary>
        IEnumerable<IForeignKey> GetReferencingRowInternalForeignKeys([NotNull] IEntityType entityType);

        /// <summary>
        ///     Gets the value indicating whether an entity of the given type might not be present in a row.
        /// </summary>
        bool IsOptional([NotNull] IEntityType entityType);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Values that are used as the eventId when logging messages from the SQLite Design Entity Framework Core
    ///     components.
    /// </summary>
    public enum SqliteDesignEventId
    {
        /// <summary>
        ///     Column name empty on index.
        /// </summary>
        ColumnNameEmptyOnIndex = 1,

        /// <summary>
        ///     Found table.
        /// </summary>
        FoundTable,

        /// <summary>
        ///     Table not in selection set.
        /// </summary>
        TableNotInSelectionSet,

        /// <summary>
        ///     Found column.
        /// </summary>
        FoundColumn,

        /// <summary>
        ///     Found index.
        /// </summary>
        FoundIndex,

        /// <summary>
        ///     Found index column.
        /// </summary>
        FoundIndexColumn,

        /// <summary>
        ///     Found foreign key column.
        /// </summary>
        FoundForeignKeyColumn,

        /// <summary>
        ///     Principal table not found.
        /// </summary>
        PrincipalTableNotFound,

        /// <summary>
        ///     Principal column not found.
        /// </summary>
        PrincipalColumnNotFound,

        /// <summary>
        ///     Using schema selections warning.
        /// </summary>
        UsingSchemaSelectionsWarning
    }
}

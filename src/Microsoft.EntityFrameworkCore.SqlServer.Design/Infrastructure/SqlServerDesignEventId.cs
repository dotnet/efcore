// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Values that are used as the eventId when logging messages from the SQL Server Design Entity Framework Core
    ///     components.
    /// </summary>
    public enum SqlServerDesignEventId
    {
        /// <summary>
        ///     Found default schema.
        /// </summary>
        FoundDefaultSchema = 1,

        /// <summary>
        ///     Found type alias.
        /// </summary>
        FoundTypeAlias,

        /// <summary>
        ///     Column name empty on table.
        /// </summary>
        ColumnMustBeNamedWarning,

        /// <summary>
        ///     Index name empty.
        /// </summary>
        IndexMustBeNamedWarning,

        /// <summary>
        ///     Unable to find table for index.
        /// </summary>
        IndexTableMissingWarning,

        /// <summary>
        ///     Column name empty on index.
        /// </summary>
        IndexColumnMustBeNamedWarning,

        /// <summary>
        ///     Foreign key name empty.
        /// </summary>
        ForeignKeyMustBeNamedWarning,

        /// <summary>
        ///     Foreign key column not in selection set.
        /// </summary>
        ForeignKeyColumnSkipped,

        /// <summary>
        ///     Column name empty on foreign key.
        /// </summary>
        ColumnNameEmptyOnForeignKey,

        /// <summary>
        ///     Data type does not allow SQL Server identity strategy.
        /// </summary>
        DataTypeDoesNotAllowSqlServerIdentityStrategyWarning,

        /// <summary>
        ///     Cannot interpret default value.
        /// </summary>
        CannotInterpretDefaultValueWarning,

        /// <summary>
        ///     Cannot interpret computed value.
        /// </summary>
        CannotInterpretComputedValueWarning
    }
}

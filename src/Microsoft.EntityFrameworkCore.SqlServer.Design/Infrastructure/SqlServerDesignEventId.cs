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
        ///     Found sequence.
        /// </summary>
        FoundSequence,

        /// <summary>
        ///     Sequence name empty.
        /// </summary>
        SequenceNameEmpty,

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
        ///     Column not in selection set.
        /// </summary>
        ColumnNotInSelectionSet,

        /// <summary>
        ///     Column name empty on table.
        /// </summary>
        ColumnNameEmptyOnTable,

        /// <summary>
        ///     Unable to find table for column.
        /// </summary>
        UnableToFindTableForColumn,

        /// <summary>
        ///     Found index column.
        /// </summary>
        FoundIndexColumn,

        /// <summary>
        ///     Index column not in selection set.
        /// </summary>
        IndexColumnNotInSelectionSet,

        /// <summary>
        ///     Index name empty.
        /// </summary>
        IndexNameEmpty,

        /// <summary>
        ///     Unable to find table for index.
        /// </summary>
        UnableToFindTableForIndex,

        /// <summary>
        ///     Column name empty on index.
        /// </summary>
        ColumnNameEmptyOnIndex,

        /// <summary>
        ///     Unable to find column for index.
        /// </summary>
        UnableToFindColumnForIndex,

        /// <summary>
        ///     Found foreign key column.
        /// </summary>
        FoundForeignKeyColumn,

        /// <summary>
        ///     Foreign key name empty.
        /// </summary>
        ForeignKeyNameEmpty,

        /// <summary>
        ///     Foreign key column not in selection set.
        /// </summary>
        ForeignKeyColumnNotInSelectionSet,

        /// <summary>
        ///     Principal table not in selection set.
        /// </summary>
        PrincipalTableNotInSelectionSet,

        /// <summary>
        ///     Column name empty on foreign key.
        /// </summary>
        ColumnNameEmptyOnForeignKey,

        /// <summary>
        ///     Unable to find column for foreign key.
        /// </summary>
        UnableToFindColumnForForeignKey,

        /// <summary>
        ///     Data type does not allow SQL Server identity strategy.
        /// </summary>
        DataTypeDoesNotAllowSqlServerIdentityStrategy,

        /// <summary>
        ///     Cannot interpret default value.
        /// </summary>
        CannotInterpretDefaultValue,

        /// <summary>
        ///     Cannot interpret computed value.
        /// </summary>
        CannotInterpretComputedValue
    }
}

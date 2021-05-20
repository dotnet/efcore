// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Represents an interface of conceptual command to the database to insert/update/delete a row.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public interface IModificationCommand
    {
        /// <summary>
        ///     The name of the table containing the data to be modified.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        ///     The schema containing the table, or <see langword="null" /> to use the default schema.
        /// </summary>
        public string? Schema { get; }

        /// <summary>
        ///     The list of <see cref="IColumnModification" />s needed to perform the insert, update, or delete.
        /// </summary>
        public IReadOnlyList<IColumnModification> ColumnModifications { get; }

        /// <summary>
        ///     Indicates whether or not the database will return values for some mapped properties
        ///     that will then need to be propagated back to the tracked entities.
        /// </summary>
        public bool RequiresResultPropagation { get; }

        /// <summary>
        ///     The <see cref="IUpdateEntry" />s that represent the entities that are mapped to the row to update.
        /// </summary>
        ///
        /// TODO: Seems this property used in MSSQL provider only.
        ///
        public IReadOnlyList<IUpdateEntry> Entries { get; }

        /// <summary>
        ///     The <see cref="EntityFrameworkCore.EntityState" /> that indicates whether the row will be
        ///     inserted (<see cref="Microsoft.EntityFrameworkCore.EntityState.Added" />),
        ///     updated (<see cref="Microsoft.EntityFrameworkCore.EntityState.Modified" />),
        ///     or deleted ((<see cref="Microsoft.EntityFrameworkCore.EntityState.Deleted" />).
        /// </summary>
        public EntityState EntityState { get; }
    }
}

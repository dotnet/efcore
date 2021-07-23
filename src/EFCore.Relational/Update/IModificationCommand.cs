// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Represents a conceptual database command to insert/update/delete a row.
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
        ///     The list of <see cref="IColumnModification" /> needed to perform the insert, update, or delete.
        /// </summary>
        public IReadOnlyList<IColumnModification> ColumnModifications { get; }

        /// <summary>
        ///     Indicates whether the database will return values for some mapped properties
        ///     that will then need to be propagated back to the tracked entities.
        /// </summary>
        public bool RequiresResultPropagation { get; }

        /// <summary>
        ///     The <see cref="IUpdateEntry" /> that represent the entities that are mapped to the row to update.
        /// </summary>
        public IReadOnlyList<IUpdateEntry> Entries { get; }

        /// <summary>
        ///     The <see cref="EntityFrameworkCore.EntityState" /> that indicates whether the row will be
        ///     inserted (<see cref="Microsoft.EntityFrameworkCore.EntityState.Added" />),
        ///     updated (<see cref="Microsoft.EntityFrameworkCore.EntityState.Modified" />),
        ///     or deleted ((<see cref="Microsoft.EntityFrameworkCore.EntityState.Deleted" />).
        /// </summary>
        public EntityState EntityState { get; }

        /// <summary>
        ///     Reads values returned from the database in the given <see cref="ValueBuffer" /> and
        ///     propagates them back to into the appropriate <see cref="IColumnModification" />
        ///     from which the values can be propagated on to tracked entities.
        /// </summary>
        /// <param name="valueBuffer"> The buffer containing the values read from the database. </param>
        public void PropagateResults(ValueBuffer valueBuffer);
    }
}

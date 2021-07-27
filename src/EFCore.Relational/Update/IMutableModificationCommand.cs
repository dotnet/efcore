// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Represents a mutable conceptual database command to insert/update/delete a row.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public interface IMutableModificationCommand : IModificationCommand
    {
        /// <summary>
        ///     Adds an entry to the command.
        /// </summary>
        /// <param name="entry" > Entry object. </param>
        /// <param name="mainEntry"> Whether this is the main entry. Only one main entry can be added to a given command. </param>
        public void AddEntry(IUpdateEntry entry, bool mainEntry);

        /// <summary>
        ///     Creates a new <see cref="IColumnModification" /> and add it to this command.
        /// </summary>
        /// <param name="columnModificationParameters"> Creation parameters. </param>
        /// <returns> The new <see cref="IColumnModification" /> instance. </returns>
        IColumnModification AddColumnModification(ColumnModificationParameters columnModificationParameters);
    }
}

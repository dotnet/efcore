// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Represents a mutable conceptual database command to insert/update/delete a row.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface INonTrackedModificationCommand : IReadOnlyModificationCommand
{
    /// <summary>
    ///     The <see cref="EntityFrameworkCore.EntityState" /> that indicates whether the row will be
    ///     inserted (<see cref="Microsoft.EntityFrameworkCore.EntityState.Added" />),
    ///     updated (<see cref="Microsoft.EntityFrameworkCore.EntityState.Modified" />),
    ///     or deleted ((<see cref="Microsoft.EntityFrameworkCore.EntityState.Deleted" />).
    /// </summary>
    public new EntityState EntityState { get; set; }

    /// <summary>
    ///     Creates a new <see cref="IColumnModification" /> and add it to this command.
    /// </summary>
    /// <param name="columnModificationParameters">Creation parameters.</param>
    /// <returns>The new <see cref="IColumnModification" /> instance.</returns>
    IColumnModification AddColumnModification(in ColumnModificationParameters columnModificationParameters);
}

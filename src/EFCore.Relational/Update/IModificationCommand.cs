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
public interface IModificationCommand : IReadOnlyModificationCommand
{
    /// <summary>
    ///     Adds an entry to the command.
    /// </summary>
    /// <param name="entry">Entry object.</param>
    /// <param name="mainEntry">Whether this is the main entry. Only one main entry can be added to a given command.</param>
    public void AddEntry(IUpdateEntry entry, bool mainEntry);
}

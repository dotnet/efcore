// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for renaming an existing sequence.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("ALTER SEQUENCE {Name} RENAME TO {NewName}")]
public class RenameSequenceOperation : MigrationOperation
{
    /// <summary>
    ///     The old name of the sequence.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    ///     The schema that contains the sequence, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    public virtual string? Schema { get; set; }

    /// <summary>
    ///     The new sequence name or <see langword="null" /> if only the schema has changed.
    /// </summary>
    public virtual string? NewName { get; set; }

    /// <summary>
    ///     The new schema name or <see langword="null" /> if only the name has changed.
    /// </summary>
    public virtual string? NewSchema { get; set; }
}

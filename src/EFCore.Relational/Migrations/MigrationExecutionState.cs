// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     Contains the state of the current migration execution.
/// </summary>
public sealed class MigrationExecutionState
{
    /// <summary>
    ///     The index of the last command that was committed to the database.
    /// </summary>
    public int LastCommittedCommandIndex { get; set; }

    /// <summary>
    ///     The id the migration that is currently being applied.
    /// </summary>
    public string? CurrentMigrationId { get; set; }

    /// <summary>
    ///    Indicates whether any migration operation was performed.
    /// </summary>
    public bool AnyOperationPerformed { get; set; }

    /// <summary>
    ///     The database lock that is in use.
    /// </summary>
    public IMigrationsDatabaseLock? DatabaseLock { get; set; }

    /// <summary>
    ///     The transaction that is in use.
    /// </summary>
    public IDbContextTransaction? Transaction { get; set; }
}

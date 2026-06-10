// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> to alter an existing database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("ALTER DATABASE {Name}")]
public class AlterDatabaseOperation : DatabaseOperation, IAlterMigrationOperation
{
    /// <summary>
    ///     An operation representing the database as it was before being altered.
    /// </summary>
    public virtual DatabaseOperation OldDatabase { get; } = new CreateDatabaseOperation();

    /// <inheritdoc />
    IMutableAnnotatable IAlterMigrationOperation.OldAnnotations
        => OldDatabase;

    private sealed class CreateDatabaseOperation : DatabaseOperation;
}

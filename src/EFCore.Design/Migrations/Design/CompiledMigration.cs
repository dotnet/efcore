// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     Represents a migration that has been compiled from scaffolded source code.
/// </summary>
public class CompiledMigration
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CompiledMigration" /> class.
    /// </summary>
    /// <param name="assembly">The compiled assembly containing the migration.</param>
    /// <param name="migrationTypeInfo">The type information for the migration class.</param>
    /// <param name="snapshotTypeInfo">The type information for the model snapshot class, if any.</param>
    /// <param name="migrationId">The migration identifier.</param>
    /// <param name="sourceCode">The original scaffolded source code.</param>
    public CompiledMigration(
        Assembly assembly,
        TypeInfo migrationTypeInfo,
        TypeInfo? snapshotTypeInfo,
        string migrationId,
        ScaffoldedMigration sourceCode)
    {
        Assembly = assembly;
        MigrationTypeInfo = migrationTypeInfo;
        SnapshotTypeInfo = snapshotTypeInfo;
        MigrationId = migrationId;
        SourceCode = sourceCode;
    }

    /// <summary>
    ///     Gets the compiled assembly containing the migration.
    /// </summary>
    public virtual Assembly Assembly { get; }

    /// <summary>
    ///     Gets the type information for the migration class.
    /// </summary>
    public virtual TypeInfo MigrationTypeInfo { get; }

    /// <summary>
    ///     Gets the type information for the model snapshot class, if any.
    /// </summary>
    public virtual TypeInfo? SnapshotTypeInfo { get; }

    /// <summary>
    ///     Gets the migration identifier.
    /// </summary>
    public virtual string MigrationId { get; }

    /// <summary>
    ///     Gets the original scaffolded source code.
    /// </summary>
    public virtual ScaffoldedMigration SourceCode { get; }
}

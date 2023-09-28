// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     The options to use when generating SQL for migrations.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[Flags]
public enum MigrationsSqlGenerationOptions
{
    /// <summary>
    ///     Generate SQL to execute at runtime.
    /// </summary>
    Default = 0,

    /// <summary>
    ///     Generate SQL for a script. Automatically added by <see cref="IMigrator.GenerateScript" />.
    /// </summary>
    Script = 1,

    /// <summary>
    ///     Generate SQL for an idempotent script.
    /// </summary>
    Idempotent = 1 << 1,

    /// <summary>
    ///     Generate SQL for a script without transaction statements.
    /// </summary>
    NoTransactions = 1 << 2
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for ensuring that a schema exists. That is, the
///     schema will be created if and only if it does not already exist.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
[DebuggerDisplay("CREATE SCHEMA {Name}")]
public class EnsureSchemaOperation : MigrationOperation
{
    /// <summary>
    ///     The name of the schema.
    /// </summary>
    public virtual string Name { get; set; } = null!;
}

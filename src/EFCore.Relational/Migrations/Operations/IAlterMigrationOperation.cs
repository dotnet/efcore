// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     An interface for any <see cref="MigrationOperation" /> that alters some existing database object.
/// </summary>
/// <remarks>
///     <para>
///         All such operations contain an 'Old...' property that provides access to the configuration to the
///         database object as it was before being altered. This interface provides a common way to access
///         annotations on that 'old' database object.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public interface IAlterMigrationOperation
{
    /// <summary>
    ///     Annotations on the database object as they were before being altered.
    /// </summary>
    IMutableAnnotatable OldAnnotations { get; }
}

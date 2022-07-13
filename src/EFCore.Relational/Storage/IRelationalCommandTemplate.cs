// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         A command template to populate an <see cref="IRelationalCommand" /> or create a <see cref="DbCommand" />
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface IRelationalCommandTemplate
{
    /// <summary>
    ///     Gets the command text to be copied to the destination command.
    /// </summary>
    string CommandText { get; }

    /// <summary>
    ///     Gets the parameters to be copied to the destination command.
    /// </summary>
    IReadOnlyList<IRelationalParameter> Parameters { get; }

    /// <summary>
    ///     <para>
    ///         Called by the execute methods to create a <see cref="DbCommand" /> for the given <see cref="DbConnection" />
    ///         and configure timeouts and transactions.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <param name="commandId">The command correlation ID.</param>
    /// <param name="commandMethod">The method that will be called on the created command.</param>
    /// <returns>The created command.</returns>
    DbCommand CreateDbCommand(
        RelationalCommandParameterObject parameterObject,
        Guid commandId,
        DbCommandMethod commandMethod);
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         A command to be executed against a relational database.
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
public interface IRelationalCommand : IRelationalCommandTemplate
{
    /// <summary>
    ///     Executes the command with no results.
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <returns>The number of rows affected.</returns>
    int ExecuteNonQuery(RelationalCommandParameterObject parameterObject);

    /// <summary>
    ///     Asynchronously executes the command with no results.
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of rows affected.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<int> ExecuteNonQueryAsync(
        RelationalCommandParameterObject parameterObject,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the command with a single scalar result.
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <returns>The result of the command.</returns>
    object? ExecuteScalar(RelationalCommandParameterObject parameterObject);

    /// <summary>
    ///     Asynchronously executes the command with a single scalar result.
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the result of the command.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<object?> ExecuteScalarAsync(
        RelationalCommandParameterObject parameterObject,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the command with a <see cref="RelationalDataReader" /> result.
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <returns>The result of the command.</returns>
    RelationalDataReader ExecuteReader(RelationalCommandParameterObject parameterObject);

    /// <summary>
    ///     Asynchronously executes the command with a <see cref="RelationalDataReader" /> result.
    /// </summary>
    /// <param name="parameterObject">Parameters for this method.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the result of the command.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<RelationalDataReader> ExecuteReaderAsync(
        RelationalCommandParameterObject parameterObject,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Populates this command from the provided <paramref name="commandTemplate" />.
    /// </summary>
    /// <param name="commandTemplate">A template command from which the command text and parameters will be copied.</param>
    void PopulateFrom(IRelationalCommandTemplate commandTemplate);
}

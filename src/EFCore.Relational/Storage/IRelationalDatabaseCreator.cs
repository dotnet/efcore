// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Performs database/schema creation, and other related operations.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IRelationalDatabaseCreator : IDatabaseCreator
{
    /// <summary>
    ///     Determines whether the physical database exists. No attempt is made to determine if the database
    ///     contains the schema for the current model.
    /// </summary>
    /// <returns>
    ///     <see langword="true" /> if the database exists; otherwise <see langword="false" />.
    /// </returns>
    bool Exists();

    /// <summary>
    ///     Asynchronously determines whether the physical database exists. No attempt is made to determine if
    ///     the database contains the schema for the current model.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     <see langword="true" /> if the database exists; otherwise <see langword="false" />.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Determines whether the database contains any tables. No attempt is made to determine if
    ///     tables belong to the current model or not.
    /// </summary>
    /// <returns>A value indicating whether any tables are present in the database.</returns>
    bool HasTables();

    /// <summary>
    ///     Asynchronously determines whether the database contains any tables. No attempt is made to determine if
    ///     tables belong to the current model or not.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     a value indicating whether any tables are present in the database.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<bool> HasTablesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates the physical database. Does not attempt to populate it with any schema.
    /// </summary>
    void Create();

    /// <summary>
    ///     Asynchronously creates the physical database. Does not attempt to populate it with any schema.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task CreateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the physical database.
    /// </summary>
    void Delete();

    /// <summary>
    ///     Asynchronously deletes the physical database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task DeleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates all tables for the current model in the database. No attempt is made
    ///     to incrementally update the schema. It is assumed that none of the tables exist in the database.
    /// </summary>
    void CreateTables();

    /// <summary>
    ///     Asynchronously creates all tables for the current model in the database. No attempt is made
    ///     to incrementally update the schema. It is assumed that none of the tables exist in the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task CreateTablesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Generates a script to create all tables for the current model.
    /// </summary>
    /// <returns>
    ///     A SQL script.
    /// </returns>
    string GenerateCreateScript();
}

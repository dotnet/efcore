// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Performs database/schema creation, and other related operations.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IRelationalDatabaseCreator : IDatabaseCreator
    {
        /// <summary>
        ///     Determines whether the physical database exists. No attempt is made to determine if the database
        ///     contains the schema for the current model.
        /// </summary>
        /// <returns>
        ///     True if the database exists; otherwise false.
        /// </returns>
        bool Exists();

        /// <summary>
        ///     Asynchronously determines whether the physical database exists. No attempt is made to determine if
        ///     the database contains the schema for the current model.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains
        ///     true if the database exists; otherwise false.
        /// </returns>
        Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Determines whether the database contains any tables. No attempt is made to determine if
        ///     tables belong to the current model or not.
        /// </summary>
        /// <returns> A value indicating whether any tables are present in the database. </returns>
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
        Task<bool> HasTablesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Creates the physical database. Does not attempt to populate it with any schema.
        /// </summary>
        void Create();

        /// <summary>
        ///     Asynchronously creates the physical database. Does not attempt to populate it with any schema.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        Task CreateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Deletes the physical database.
        /// </summary>
        void Delete();

        /// <summary>
        ///     Asynchronously deletes the physical database.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
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
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        Task CreateTablesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Generates a script to create all tables for the current model.
        /// </summary>
        /// <returns>
        ///     A SQL script.
        /// </returns>
        string GenerateCreateScript();
    }
}

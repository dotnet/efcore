// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents a connection with a relational database.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IRelationalConnection : IRelationalTransactionManager, IDisposable, IAsyncDisposable
    {
        /// <summary>
        ///     Gets the connection string for the database.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        ///     Gets the underlying <see cref="System.Data.Common.DbConnection" /> used to connect to the database.
        /// </summary>
        DbConnection DbConnection { get; }

        /// <summary>
        ///     The <see cref="DbContext"/> currently in use, or null if not known.
        /// </summary>
        DbContext Context { get; }

        /// <summary>
        ///     Gets the connection identifier.
        /// </summary>
        Guid ConnectionId { get; }

        /// <summary>
        ///     Gets the timeout for executing a command against the database.
        /// </summary>
        int? CommandTimeout { get; set; }

        /// <summary>
        ///     Opens the connection to the database.
        /// </summary>
        /// <param name="errorsExpected"> Indicate if the connection errors are expected and should be logged as debug message. </param>
        /// <returns> True if the underlying connection was actually opened; false otherwise. </returns>
        bool Open(bool errorsExpected = false);

        /// <summary>
        ///     Asynchronously opens the connection to the database.
        /// </summary>
        /// <param name="errorsExpected"> Indicate if the connection errors are expected and should be logged as debug message. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, with a value of true if the connection
        ///     was actually opened.
        /// </returns>
        Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false);

        /// <summary>
        ///     Closes the connection to the database.
        /// </summary>
        /// <returns> True if the underlying connection was actually closed; false otherwise. </returns>
        bool Close();

        /// <summary>
        ///     Closes the connection to the database.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation, with a value of true if the connection
        ///     was actually closed.
        /// </returns>
        Task<bool> CloseAsync();

        /// <summary>
        ///     Gets a value indicating whether the multiple active result sets feature is enabled.
        /// </summary>
        bool IsMultipleActiveResultSetsEnabled { get; }

        /// <summary>
        ///     Gets the current transaction.
        /// </summary>
        new IDbContextTransaction CurrentTransaction { get; }

        /// <summary>
        ///     Gets a semaphore used to serialize access to this connection.
        /// </summary>
        /// <value>
        ///     The semaphore.
        /// </value>
        SemaphoreSlim Semaphore { get; }
    }
}

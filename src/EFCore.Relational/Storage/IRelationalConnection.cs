// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IRelationalConnection : IRelationalTransactionManager, IDisposable, IAsyncDisposable
    {
        /// <summary>
        ///     Gets or sets the connection string for the database.
        /// </summary>
        string? ConnectionString { get; set; }

        /// <summary>
        ///     <para>
        ///         Gets or sets the underlying <see cref="System.Data.Common.DbConnection" /> used to connect to the database.
        ///     </para>
        ///     <para>
        ///         The connection can only be changed when the existing connection, if any, is not open.
        ///     </para>
        ///     <para>
        ///         Note that the connection must be disposed by application code since it was not created by Entity Framework.
        ///     </para>
        /// </summary>
        [AllowNull]
        DbConnection DbConnection { get; set; }

        /// <summary>
        ///     The <see cref="DbContext" /> currently in use, or null if not known.
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
        /// <returns> <see langword="true" /> if the underlying connection was actually opened; <see langword="false" /> otherwise. </returns>
        bool Open(bool errorsExpected = false);

        /// <summary>
        ///     Asynchronously opens the connection to the database.
        /// </summary>
        /// <param name="errorsExpected"> Indicate if the connection errors are expected and should be logged as debug message. </param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, with a value of <see langword="true" /> if the connection
        ///     was actually opened.
        /// </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false);

        /// <summary>
        ///     Closes the connection to the database.
        /// </summary>
        /// <returns> <see langword="true" /> if the underlying connection was actually closed; <see langword="false" /> otherwise. </returns>
        bool Close();

        /// <summary>
        ///     Closes the connection to the database.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation, with a value of <see langword="true" /> if the connection
        ///     was actually closed.
        /// </returns>
        Task<bool> CloseAsync();

        /// <summary>
        ///     Rents a relational command that can be executed with this connection.
        /// </summary>
        /// <returns> A relational command that can be executed with this connection. </returns>
        IRelationalCommand RentCommand();

        /// <summary>
        ///     Returns a relational command to this connection, so that it can be reused in the future.
        /// </summary>
        void ReturnCommand(IRelationalCommand command);

        /// <summary>
        ///     Gets the current transaction.
        /// </summary>
        new IDbContextTransaction? CurrentTransaction { get; }

        /// <summary>
        ///     Gets a semaphore used to serialize access to this connection.
        /// </summary>
        /// <value>
        ///     The semaphore.
        /// </value>
        [Obsolete("EF Core no longer uses this semaphore. It will be removed in an upcoming release.")]
        SemaphoreSlim Semaphore { get; }
    }
}

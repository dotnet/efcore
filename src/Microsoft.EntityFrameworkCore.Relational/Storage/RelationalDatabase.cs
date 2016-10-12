// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The main interaction point between a context and the database provider.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalDatabase : Database
    {
        private readonly ICommandBatchPreparer _batchPreparer;
        private readonly IBatchExecutor _batchExecutor;
        private readonly IRelationalConnection _connection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalDatabase" /> class.
        /// </summary>
        /// <param name="queryCompilationContextFactory"> The <see cref="IQueryCompilationContextFactory" /> to be used. </param>
        /// <param name="batchPreparer"> The <see cref="ICommandBatchPreparer" /> to be used. </param>
        /// <param name="batchExecutor"> The <see cref="IBatchExecutor" /> to be used. </param>
        /// <param name="connection"> The <see cref="IRelationalConnection" /> to be used. </param>
        public RelationalDatabase(
            [NotNull] IQueryCompilationContextFactory queryCompilationContextFactory,
            [NotNull] ICommandBatchPreparer batchPreparer,
            [NotNull] IBatchExecutor batchExecutor,
            [NotNull] IRelationalConnection connection)
            : base(queryCompilationContextFactory)
        {
            Check.NotNull(batchPreparer, nameof(batchPreparer));
            Check.NotNull(batchExecutor, nameof(batchExecutor));
            Check.NotNull(connection, nameof(connection));

            _batchPreparer = batchPreparer;
            _batchExecutor = batchExecutor;
            _connection = connection;
        }

        /// <summary>
        ///     Persists changes from the supplied entries to the database.
        /// </summary>
        /// <param name="entries"> Entries representing the changes to be persisted. </param>
        /// <returns> The number of state entries persisted to the database. </returns>
        public override int SaveChanges(
            IReadOnlyList<IUpdateEntry> entries)
            => _batchExecutor.Execute(
                _batchPreparer.BatchCommands(
                    Check.NotNull(entries, nameof(entries))),
                _connection);

        /// <summary>
        ///     Asynchronously persists changes from the supplied entries to the database.
        /// </summary>
        /// <param name="entries"> Entries representing the changes to be persisted. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of entries persisted to the database.
        /// </returns>
        public override Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken))
            => _batchExecutor.ExecuteAsync(
                _batchPreparer.BatchCommands(
                    Check.NotNull(entries, nameof(entries))),
                _connection,
                cancellationToken);
    }
}

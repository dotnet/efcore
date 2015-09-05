// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Update.Internal
{
    public class BatchExecutor : IBatchExecutor
    {
        private readonly IRelationalTypeMapper _typeMapper;
        private readonly DbContext _context;
        private readonly LazyRef<ILogger> _logger;

        public BatchExecutor(
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] DbContext context,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _typeMapper = typeMapper;
            _context = context;
            _logger = new LazyRef<ILogger>(() => (loggerFactory.CreateLogger<BatchExecutor>()));
        }

        protected virtual ILogger Logger => _logger.Value;

        public virtual int Execute(
            IEnumerable<ModificationCommandBatch> commandBatches,
            IRelationalConnection connection)
        {
            var rowsAffected = 0;
            connection.Open();
            IRelationalTransaction startedTransaction = null;
            try
            {
                if (connection.Transaction == null)
                {
                    startedTransaction = connection.BeginTransaction();
                }

                foreach (var commandbatch in commandBatches)
                {
                    commandbatch.Execute(
                        connection.Transaction,
                        _typeMapper,
                        _context,
                        Logger);
                    rowsAffected += commandbatch.ModificationCommands.Count;
                }

                startedTransaction?.Commit();
            }
            finally
            {
                startedTransaction?.Dispose();
                connection.Close();
            }

            return rowsAffected;
        }

        public virtual async Task<int> ExecuteAsync(
            IEnumerable<ModificationCommandBatch> commandBatches,
            IRelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var rowsAffected = 0;
            await connection.OpenAsync(cancellationToken);
            IRelationalTransaction startedTransaction = null;
            try
            {
                if (connection.Transaction == null)
                {
                    startedTransaction = connection.BeginTransaction();
                }

                foreach (var commandbatch in commandBatches)
                {
                    await commandbatch.ExecuteAsync(
                        connection.Transaction,
                        _typeMapper,
                        _context,
                        Logger, cancellationToken);
                    rowsAffected += commandbatch.ModificationCommands.Count;
                }

                startedTransaction?.Commit();
            }
            finally
            {
                startedTransaction?.Dispose();
                connection.Close();
            }

            return rowsAffected;
        }
    }
}

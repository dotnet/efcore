// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Update
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
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(context, nameof(context));
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _typeMapper = typeMapper;
            _context = context;
            _logger = new LazyRef<ILogger>(() => (loggerFactory.CreateLogger<BatchExecutor>()));
        }

        protected virtual ILogger Logger => _logger.Value;

        public virtual int Execute(
            IEnumerable<ModificationCommandBatch> commandBatches,
            IRelationalConnection connection)
        {
            Check.NotNull(commandBatches, nameof(commandBatches));
            Check.NotNull(connection, nameof(connection));

            var rowsAffected = 0;
            connection.Open();
            RelationalTransaction startedTransaction = null;
            try
            {
                if (connection.Transaction == null)
                {
                    startedTransaction = connection.BeginTransaction();
                }

                foreach (var commandbatch in commandBatches)
                {
                    rowsAffected += commandbatch.Execute(
                        connection.Transaction,
                        _typeMapper,
                        _context,
                        Logger);
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
            Check.NotNull(commandBatches, nameof(commandBatches));
            Check.NotNull(connection, nameof(connection));

            var rowsAffected = 0;
            await connection.OpenAsync(cancellationToken).WithCurrentCulture();
            RelationalTransaction startedTransaction = null;
            try
            {
                if (connection.Transaction == null)
                {
                    startedTransaction = connection.BeginTransaction();
                }

                foreach (var commandbatch in commandBatches)
                {
                    rowsAffected += await commandbatch.ExecuteAsync(
                        connection.Transaction,
                        _typeMapper,
                        _context,
                        Logger, cancellationToken)
                        .WithCurrentCulture();
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

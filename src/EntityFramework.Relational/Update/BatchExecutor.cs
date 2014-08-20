// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class BatchExecutor
    {
        private readonly RelationalTypeMapper _typeMapper;
        private readonly DbContext _context;
        private readonly LazyRef<ILogger> _logger;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected BatchExecutor()
        {
        }

        public BatchExecutor(
            [NotNull] RelationalTypeMapper typeMapper,
            [NotNull] DbContextConfiguration contextConfiguration)
        {
            Check.NotNull(typeMapper, "typeMapper");
            Check.NotNull(contextConfiguration, "contextConfiguration");

            _typeMapper = typeMapper;
            _context = contextConfiguration.Context;
            _logger = new LazyRef<ILogger>(() => (_context.Configuration.LoggerFactory.Create(GetType().Name)));
        }

        protected virtual ILogger Logger { get { return _logger.Value; } }

        public virtual async Task<int> ExecuteAsync(
            [NotNull] IEnumerable<ModificationCommandBatch> commandBatches,
            [NotNull] RelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(commandBatches, "commandBatches");
            Check.NotNull(connection, "connection");

            var rowsAffected = 0;
            await connection.OpenAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
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
                        Logger,
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(continueOnCapturedContext: false);
                }

                if (startedTransaction != null)
                {
                    startedTransaction.Commit();
                }
            }
            catch
            {
                if (connection.Transaction != null)
                {
                    connection.Transaction.Rollback();
                }

                throw;
            }
            finally
            {
                if (startedTransaction != null)
                {
                    startedTransaction.Dispose();
                }
                connection.Close();
            }

            return rowsAffected;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalDatabase : Database
    {
        private readonly ICommandBatchPreparer _batchPreparer;
        private readonly IBatchExecutor _batchExecutor;
        private readonly IRelationalConnection _connection;
        private readonly IDbContextOptions _options;

        public RelationalDatabase(
            [NotNull] IQueryCompilationContextFactory queryCompilationContextFactory,
            [NotNull] ICommandBatchPreparer batchPreparer,
            [NotNull] IBatchExecutor batchExecutor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IDbContextOptions options)
            : base(queryCompilationContextFactory)
        {
            Check.NotNull(batchPreparer, nameof(batchPreparer));
            Check.NotNull(batchExecutor, nameof(batchExecutor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(options, nameof(options));

            _batchPreparer = batchPreparer;
            _batchExecutor = batchExecutor;
            _connection = connection;
            _options = options;
        }

        public override int SaveChanges(
            IReadOnlyList<InternalEntityEntry> entries)
            => _batchExecutor.Execute(
                _batchPreparer.BatchCommands(
                    Check.NotNull(entries, nameof(entries)), _options),
                _connection);

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<InternalEntityEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken))
            => _batchExecutor.ExecuteAsync(
                _batchPreparer.BatchCommands(
                    Check.NotNull(entries, nameof(entries)), _options),
                _connection,
                cancellationToken);
    }
}

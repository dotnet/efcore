// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Remotion.Linq;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryDatabase : Database, IInMemoryDatabase
    {
        private readonly IInMemoryStore _store;
        private readonly ILogger<InMemoryDatabase> _logger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryDatabase(
            [NotNull] IQueryCompilationContextFactory queryCompilationContextFactory,
            [NotNull] IInMemoryStoreSource storeSource,
            [NotNull] IDbContextOptions options,
            [NotNull] ILogger<InMemoryDatabase> logger)
            : base(queryCompilationContextFactory)
        {
            Check.NotNull(queryCompilationContextFactory, nameof(queryCompilationContextFactory));
            Check.NotNull(storeSource, nameof(storeSource));
            Check.NotNull(options, nameof(options));
            Check.NotNull(logger, nameof(logger));

            _store = storeSource.GetStore(options);
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IInMemoryStore Store => _store;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override int SaveChanges(IReadOnlyList<IUpdateEntry> entries)
            => _store.ExecuteTransaction(Check.NotNull(entries, nameof(entries)), _logger);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult(_store.ExecuteTransaction(Check.NotNull(entries, nameof(entries)), _logger));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool EnsureDatabaseCreated(IModel model)
            => _store.EnsureCreated(Check.NotNull(model, nameof(model)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var syncQueryExecutor = CompileQuery<TResult>(queryModel);

            return qc => syncQueryExecutor(qc).ToAsyncEnumerable();
        }
    }
}

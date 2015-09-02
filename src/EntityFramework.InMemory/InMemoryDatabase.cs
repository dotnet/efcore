// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDatabase : Database, IInMemoryDatabase
    {
        private readonly ThreadSafeLazyRef<IInMemoryStore> _database;

        public InMemoryDatabase(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IQueryCompilationContextFactory queryCompilationContextFactory,
            [NotNull] IInMemoryStore persistentStore,
            [NotNull] IDbContextOptions options)
            : base(queryCompilationContextFactory)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(queryCompilationContextFactory, nameof(queryCompilationContextFactory));
            Check.NotNull(persistentStore, nameof(persistentStore));
            Check.NotNull(options, nameof(options));

            var storeConfig = options.Extensions
                .OfType<InMemoryOptionsExtension>()
                .FirstOrDefault();

            _database = new ThreadSafeLazyRef<IInMemoryStore>(
                () => storeConfig?.Persist ?? true
                    ? persistentStore
                    : new InMemoryStore(loggerFactory));
        }

        public virtual IInMemoryStore Store => _database.Value;

        public override int SaveChanges(IReadOnlyList<InternalEntityEntry> entries)
            => _database.Value.ExecuteTransaction(Check.NotNull(entries, nameof(entries)));

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<InternalEntityEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult(_database.Value.ExecuteTransaction(Check.NotNull(entries, nameof(entries))));

        public virtual bool EnsureDatabaseCreated(IModel model)
            => _database.Value.EnsureCreated(Check.NotNull(model, nameof(model)));

        public override Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var syncQueryExecutor = CompileQuery<TResult>(queryModel);

            return qc => syncQueryExecutor(qc).ToAsyncEnumerable();
        }
    }
}

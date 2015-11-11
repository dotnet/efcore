// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class InMemoryDatabase : Database, IInMemoryDatabase
    {
        private readonly IInMemoryStore _database;

        public InMemoryDatabase(
            [NotNull] IQueryCompilationContextFactory queryCompilationContextFactory,
            [NotNull] IInMemoryStore persistentStore,
            [NotNull] IDbContextOptions options)
            : base(queryCompilationContextFactory)
        {
            Check.NotNull(queryCompilationContextFactory, nameof(queryCompilationContextFactory));
            Check.NotNull(persistentStore, nameof(persistentStore));
            Check.NotNull(options, nameof(options));

            _database = persistentStore;
        }

        public virtual IInMemoryStore Store => _database;

        public override int SaveChanges(IReadOnlyList<IUpdateEntry> entries)
            => _database.ExecuteTransaction(Check.NotNull(entries, nameof(entries)));

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult(_database.ExecuteTransaction(Check.NotNull(entries, nameof(entries))));

        public virtual bool EnsureDatabaseCreated(IModel model)
            => _database.EnsureCreated(Check.NotNull(model, nameof(model)));

        public override Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var syncQueryExecutor = CompileQuery<TResult>(queryModel);

            return qc => syncQueryExecutor(qc).ToAsyncEnumerable();
        }
    }
}

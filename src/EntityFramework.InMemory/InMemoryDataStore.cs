// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory.Query;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStore : DataStore, IInMemoryDataStore
    {
        private readonly bool _persist;
        private readonly ThreadSafeLazyRef<InMemoryDatabase> _database;

        public InMemoryDataStore(
            [NotNull] IModel model,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] EntityMaterializerSource entityMaterializerSource,
            [NotNull] InMemoryDatabase persistentDatabase,
            [NotNull] IDbContextOptions options,
            [NotNull] ILoggerFactory loggerFactory)
            : base(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource)),
                Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource)),
                Check.NotNull(loggerFactory, nameof(loggerFactory)))
        {
            Check.NotNull(persistentDatabase, nameof(persistentDatabase));

            var storeConfig = options.Extensions
                .OfType<InMemoryOptionsExtension>()
                .FirstOrDefault();

            _persist = storeConfig?.Persist ?? true;

            _database = new ThreadSafeLazyRef<InMemoryDatabase>(
                () => _persist
                    ? persistentDatabase
                    : new InMemoryDatabase(loggerFactory));
        }

        public virtual InMemoryDatabase Database => _database.Value;

        public override int SaveChanges(IReadOnlyList<InternalEntityEntry> entries)
        {
            Check.NotNull(entries, nameof(entries));

            return _database.Value.ExecuteTransaction(entries);
        }

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<InternalEntityEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entries, nameof(entries));

            return Task.FromResult(_database.Value.ExecuteTransaction(entries));
        }

        public override Func<QueryContext, IEnumerable<TResult>> CompileQuery<TResult>(QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            return new InMemoryQueryCompilationContext(
                Model,
                Logger,
                EntityMaterializerSource,
                EntityKeyFactorySource)
                .CreateQueryModelVisitor()
                .CreateQueryExecutor<TResult>(queryModel);
        }

        public override Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var syncQueryExecutor = CompileQuery<TResult>(queryModel);

            return qc => syncQueryExecutor(qc).ToAsyncEnumerable();
        }

        public virtual bool EnsureDatabaseCreated(IModel model)
        {
            Check.NotNull(model, nameof(model));

            return _database.Value.EnsureCreated(model);
        }
    }
}

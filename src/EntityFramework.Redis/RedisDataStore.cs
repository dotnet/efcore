// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Redis.Query;
using Microsoft.Data.Entity.Redis.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisDataStore : DataStore
    {
        private readonly LazyRef<RedisDatabase> _database;

        public RedisDataStore([NotNull] DbContextConfiguration configuration)
            : base(configuration)
        {
            _database = new LazyRef<RedisDatabase>(() => (RedisDatabase)configuration.Database);
        }
        public override int SaveChanges(
            IReadOnlyList<StateEntry> stateEntries)
        {
            Check.NotNull(stateEntries, "stateEntries");

            return _database.Value.SaveChanges(stateEntries);
        }

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");

            return _database.Value.SaveChangesAsync(stateEntries, cancellationToken);
        }

        public override IEnumerable<TResult> Query<TResult>(QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            var queryCompilationContext
                = CreateQueryCompilationContext(
                    new LinqOperatorProvider(),
                    new ResultOperatorHandler());

            var queryExecutor = queryCompilationContext.CreateQueryModelVisitor().CreateQueryExecutor<TResult>(queryModel);

            var queryContext
                = new RedisQueryContext(
                    Logger,
                    CreateQueryBuffer(),
                    StateManager,
                    _database.Value);

            return queryExecutor(queryContext);
        }

        public override IAsyncEnumerable<TResult> AsyncQuery<TResult>(
            QueryModel queryModel, CancellationToken cancellationToken)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected virtual RedisQueryCompilationContext CreateQueryCompilationContext(
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler)
        {
            Check.NotNull(linqOperatorProvider, "linqOperatorProvider");
            Check.NotNull(resultOperatorHandler, "resultOperatorHandler");

            return new RedisQueryCompilationContext(
                Model, linqOperatorProvider, resultOperatorHandler);
        }
    }
}

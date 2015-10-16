// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class Database : IDatabase
    {
        private readonly IQueryCompilationContextFactory _queryCompilationContextFactory;

        protected Database([NotNull] IQueryCompilationContextFactory queryCompilationContextFactory)
        {
            Check.NotNull(queryCompilationContextFactory, nameof(queryCompilationContextFactory));

            _queryCompilationContextFactory = queryCompilationContextFactory;
        }

        public abstract int SaveChanges(IReadOnlyList<IUpdateEntry> entries);

        public abstract Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken));

        public virtual Func<QueryContext, IEnumerable<TResult>> CompileQuery<TResult>(QueryModel queryModel)
            => _queryCompilationContextFactory
                .Create(async: false)
                .CreateQueryModelVisitor()
                .CreateQueryExecutor<TResult>(Check.NotNull(queryModel, nameof(queryModel)));

        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
            => _queryCompilationContextFactory
                .Create(async: true)
                .CreateQueryModelVisitor()
                .CreateAsyncQueryExecutor<TResult>(Check.NotNull(queryModel, nameof(queryModel)));
    }
}

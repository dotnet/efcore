// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryExecutor : IQueryExecutor
    {
        private readonly DbContext _context;
        private readonly LazyRef<ILogger> _logger;

        public EntityQueryExecutor([NotNull] DbContext context)
        {
            Check.NotNull(context, "context");

            _context = context;
            _logger = new LazyRef<ILogger>(() => (_context.Configuration.LoggerFactory.Create("EntityQueryExecutor")));
        }

        public virtual T ExecuteScalar<T>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            return ExecuteCollection<T>(queryModel).Single();
        }

        public virtual Task<T> ExecuteScalarAsync<T>([NotNull] QueryModel queryModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(queryModel, "queryModel");

            return ((IAsyncEnumerable<T>)ExecuteCollection<T>(queryModel)).SingleAsync(cancellationToken);
        }

        public virtual T ExecuteSingle<T>([NotNull] QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            Check.NotNull(queryModel, "queryModel");

            var enumerable = ExecuteCollection<T>(queryModel);

            return returnDefaultWhenEmpty
                ? enumerable.SingleOrDefault()
                : enumerable.Single();
        }

        public virtual Task<T> ExecuteSingleAsync<T>(
            [NotNull] QueryModel queryModel,
            bool returnDefaultWhenEmpty,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(queryModel, "queryModel");

            var asyncEnumerable = (IAsyncEnumerable<T>)ExecuteCollection<T>(queryModel);

            return returnDefaultWhenEmpty
                ? asyncEnumerable.SingleOrDefaultAsync(cancellationToken)
                : asyncEnumerable.SingleAsync(cancellationToken);
        }

        public IEnumerable<T> ExecuteCollection<T>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            if (_logger.Value.IsEnabled(TraceType.Information))
            {
                _logger.Value.WriteInformation(queryModel + Environment.NewLine);
            }

            return _context.Configuration.DataStore
                .Query<T>(
                    queryModel,
                    _context.Configuration.Model,
                    _context.Configuration.Services.StateManager);
        }
    }
}

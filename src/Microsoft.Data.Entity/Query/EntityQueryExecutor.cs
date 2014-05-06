// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Framework.Logging;
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

        public virtual Task<T> ExecuteScalarAsync<T>(
            [NotNull] QueryModel queryModel, CancellationToken cancellationToken)
        {
            Check.NotNull(queryModel, "queryModel");

            return AsyncExecuteCollection<T>(queryModel).Single(cancellationToken);
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
            CancellationToken cancellationToken)
        {
            Check.NotNull(queryModel, "queryModel");

            var asyncEnumerable = AsyncExecuteCollection<T>(queryModel);

            return returnDefaultWhenEmpty
                ? asyncEnumerable.SingleOrDefault(cancellationToken)
                : asyncEnumerable.Single(cancellationToken);
        }

        public virtual IEnumerable<T> ExecuteCollection<T>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            LogQueryModel(queryModel);

            return _context.Configuration.DataStore
                .Query<T>(queryModel, _context.Configuration.Services.StateManager);
        }

        public virtual IAsyncEnumerable<T> AsyncExecuteCollection<T>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            LogQueryModel(queryModel);

            return _context.Configuration.DataStore
                .AsyncQuery<T>(queryModel, _context.Configuration.Services.StateManager);
        }

        private void LogQueryModel(QueryModel queryModel)
        {
            if (_logger.Value.IsEnabled(TraceType.Information))
            {
                _logger.Value.WriteInformation(queryModel + Environment.NewLine);
            }
        }
    }
}

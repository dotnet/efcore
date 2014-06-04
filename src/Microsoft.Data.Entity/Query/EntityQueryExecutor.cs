// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Transformations;

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

            new SubQueryFlattener().VisitQueryModel(queryModel);

            LogQueryModel(queryModel);

            return _context.Configuration.DataStore
                .Query<T>(queryModel, _context.Configuration.Services.StateManager);
        }

        public virtual IAsyncEnumerable<T> AsyncExecuteCollection<T>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            new SubQueryFlattener().VisitQueryModel(queryModel);

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

        private class SubQueryFlattener : SubQueryFromClauseFlattener
        {
            protected override void FlattenSubQuery(
                SubQueryExpression subQueryExpression,
                FromClauseBase fromClause,
                QueryModel queryModel,
                int destinationIndex)
            {
                var subQueryModel = subQueryExpression.QueryModel;

                if (!(subQueryModel.ResultOperators.Count <= 0
                      && !subQueryModel.BodyClauses.Any(bc => bc is OrderByClause)))
                {
                    return;
                }

                var innerMainFromClause
                    = subQueryExpression.QueryModel.MainFromClause;

                CopyFromClauseData(innerMainFromClause, fromClause);

                var innerSelectorMapping = new QuerySourceMapping();
                innerSelectorMapping.AddMapping(fromClause, subQueryExpression.QueryModel.SelectClause.Selector);

                queryModel.TransformExpressions(
                    ex => ReferenceReplacingExpressionTreeVisitor
                        .ReplaceClauseReferences(ex, innerSelectorMapping, false));

                InsertBodyClauses(subQueryExpression.QueryModel.BodyClauses, queryModel, destinationIndex);

                var innerBodyClauseMapping = new QuerySourceMapping();
                innerBodyClauseMapping.AddMapping(innerMainFromClause, new QuerySourceReferenceExpression(fromClause));

                queryModel.TransformExpressions(
                    ex => ReferenceReplacingExpressionTreeVisitor
                        .ReplaceClauseReferences(ex, innerBodyClauseMapping, false));
            }
        }
    }
}

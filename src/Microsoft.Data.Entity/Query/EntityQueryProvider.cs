// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryProvider : QueryProviderBase, IAsyncQueryProvider
    {
        private static IQueryParser CreateQueryParser()
        {
            var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();

            var expressionTreeParser
                = new ExpressionTreeParser(
                    ExpressionTreeParser.CreateDefaultNodeTypeProvider(),
                    ExpressionTreeParser.CreateDefaultProcessor(transformerRegistry));

            return new QueryParser(expressionTreeParser);
        }

        public EntityQueryProvider([NotNull] EntityQueryExecutor entityQueryExecutor)
            : base(
                CreateQueryParser(),
                Check.NotNull(entityQueryExecutor, "entityQueryExecutor"))
        {
        }

        public virtual Task<T> ExecuteAsync<T>(
            Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, "expression");

            cancellationToken.ThrowIfCancellationRequested();

            var queryModel = GenerateQueryModel(expression);
            var streamedDataInfo = queryModel.GetOutputDataInfo();
            var entityQueryExecutor = (EntityQueryExecutor)Executor;

            if (streamedDataInfo is StreamedScalarValueInfo)
            {
                return entityQueryExecutor.ExecuteScalarAsync<T>(queryModel, cancellationToken);
            }

            return entityQueryExecutor
                .ExecuteSingleAsync<T>(
                    queryModel,
                    ((StreamedSingleValueInfo)streamedDataInfo).ReturnDefaultWhenEmpty,
                    cancellationToken);
        }

        public virtual Task<object> ExecuteAsync(
            Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, "expression");

            cancellationToken.ThrowIfCancellationRequested();

            return ExecuteAsync<object>(expression, cancellationToken);
        }

        public virtual IAsyncEnumerable<T> AsyncQuery<T>([NotNull] Expression expression)
        {
            Check.NotNull(expression, "expression");

            var queryModel = GenerateQueryModel(expression);

            return ((EntityQueryExecutor)Executor).AsyncExecuteCollection<T>(queryModel);
        }

        public override IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new EntityQueryable<T>(this, expression);
        }
    }
}

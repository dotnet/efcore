// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        private static readonly MethodInfo _executeScalarAsyncMethod
            = (typeof(EntityQueryExecutor).GetMethod("ExecuteScalarAsync"));

        private static readonly MethodInfo _executeSingleAsyncMethod
            = (typeof(EntityQueryExecutor).GetMethod("ExecuteSingleAsync"));

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

        public Task<T> ExecuteAsync<T>(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, "expression");

            cancellationToken.ThrowIfCancellationRequested();

            var queryModel = GenerateQueryModel(expression);
            var streamedDataInfo = queryModel.GetOutputDataInfo();

            MethodInfo closedExecuteMethod;

            if (streamedDataInfo is StreamedScalarValueInfo)
            {
                closedExecuteMethod = _executeScalarAsyncMethod.MakeGenericMethod(typeof(T));

                return (Task<T>)closedExecuteMethod.Invoke(Executor, new object[] { queryModel, cancellationToken });
            }

            var streamedSingleValueInfo = streamedDataInfo as StreamedSingleValueInfo;

            Contract.Assert(streamedSingleValueInfo != null);

            closedExecuteMethod = _executeSingleAsyncMethod.MakeGenericMethod(typeof(T));

            return (Task<T>)closedExecuteMethod.Invoke(
                Executor,
                new object[] { queryModel, streamedSingleValueInfo.ReturnDefaultWhenEmpty, cancellationToken });
        }

        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, "expression");

            cancellationToken.ThrowIfCancellationRequested();

            return ExecuteAsync<object>(expression, cancellationToken);
        }

        public override IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new EntityQueryable<T>(this, expression);
        }
    }
}

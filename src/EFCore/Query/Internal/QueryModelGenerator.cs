// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class QueryModelGenerator : IQueryModelGenerator
    {
        private readonly INodeTypeProvider _nodeTypeProvider;
        private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryModelGenerator(
            [NotNull] INodeTypeProviderFactory nodeTypeProviderFactory,
            [NotNull] IEvaluatableExpressionFilter evaluatableExpressionFilter)
        {
            Check.NotNull(nodeTypeProviderFactory, nameof(nodeTypeProviderFactory));
            Check.NotNull(evaluatableExpressionFilter, nameof(evaluatableExpressionFilter));

            _nodeTypeProvider = nodeTypeProviderFactory.Create();
            _evaluatableExpressionFilter = evaluatableExpressionFilter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression ExtractParameters(
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            Expression query,
            IParameterValues parameterValues,
            Type contextType,
            bool parameterize = true,
            bool generateContextAccessors = false)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(parameterValues, nameof(parameterValues));

            var visitor
                = new ParameterExtractingExpressionVisitor(
                    _evaluatableExpressionFilter,
                    parameterValues,
                    logger,
                    contextType,
                    parameterize,
                    generateContextAccessors);

            return visitor.ExtractParameters(query);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual QueryModel ParseQuery(Expression query)
            => CreateQueryParser(_nodeTypeProvider).GetParsedQuery(query);

        private QueryParser CreateQueryParser(INodeTypeProvider nodeTypeProvider)
            => new QueryParser(
                new ExpressionTreeParser(
                    nodeTypeProvider,
                    new CompoundExpressionTreeProcessor(
                        new IExpressionTreeProcessor[]
                        {
                            new PartialEvaluatingExpressionTreeProcessor(_evaluatableExpressionFilter),
                            new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault())
                        })));
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using ReLinq = Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class QueryModelGenerator : IQueryModelGenerator
    {
        private readonly INodeTypeProvider _nodeTypeProvider;
        private readonly ReLinq.IEvaluatableExpressionFilter _reLinqEvaluatableExpressionFilter;
        private readonly ICurrentDbContext _currentDbContext;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryModelGenerator(
            [NotNull] INodeTypeProviderFactory nodeTypeProviderFactory,
            [NotNull] ReLinq.IEvaluatableExpressionFilter reLinqEvaluatableExpressionFilter,
            [NotNull] IEvaluatableExpressionFilter evaluatableExpressionFilter,
            [NotNull] ICurrentDbContext currentDbContext)
        {
            Check.NotNull(nodeTypeProviderFactory, nameof(nodeTypeProviderFactory));
            Check.NotNull(reLinqEvaluatableExpressionFilter, nameof(reLinqEvaluatableExpressionFilter));
            Check.NotNull(evaluatableExpressionFilter, nameof(evaluatableExpressionFilter));
            Check.NotNull(currentDbContext, nameof(currentDbContext));

            _nodeTypeProvider = nodeTypeProviderFactory.Create();
            _reLinqEvaluatableExpressionFilter = reLinqEvaluatableExpressionFilter;
            _currentDbContext = currentDbContext;
            EvaluatableExpressionFilter = evaluatableExpressionFilter;
        }

        public virtual IEvaluatableExpressionFilter EvaluatableExpressionFilter { get; }

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
                            new PartialEvaluatingExpressionTreeProcessor(_reLinqEvaluatableExpressionFilter),
                            new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault())
                        })));
    }
}

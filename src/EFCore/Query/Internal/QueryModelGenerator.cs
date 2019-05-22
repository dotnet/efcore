// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Remotion.Linq;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using ReLinq = Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class QueryModelGenerator : IQueryModelGenerator
    {
        private readonly INodeTypeProvider _nodeTypeProvider;
        private readonly ReLinq.IEvaluatableExpressionFilter _reLinqEvaluatableExpressionFilter;
        private readonly ICurrentDbContext _currentDbContext;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEvaluatableExpressionFilter EvaluatableExpressionFilter { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual QueryModel ParseQuery(Expression query)
            => CreateQueryParser(_nodeTypeProvider).GetParsedQuery(query);

        private QueryParser CreateQueryParser(INodeTypeProvider nodeTypeProvider)
            => new QueryParser(
                new ExpressionTreeParser(
                    nodeTypeProvider,
                    new CompoundExpressionTreeProcessor(
                        new IExpressionTreeProcessor[] { new PartialEvaluatingExpressionTreeProcessor(_reLinqEvaluatableExpressionFilter), new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault()) })));
    }
}

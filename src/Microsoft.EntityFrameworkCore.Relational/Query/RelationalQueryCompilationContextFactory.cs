// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A relational factory for instances of <see cref="QueryCompilationContext" />.
    /// </summary>
    public class RelationalQueryCompilationContextFactory : QueryCompilationContextFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryCompilationContextFactory(
            [NotNull] IModel model,
            [NotNull] ISensitiveDataLogger<RelationalQueryCompilationContextFactory> logger,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] MethodInfoBasedNodeTypeRegistry methodInfoBasedNodeTypeRegistry,
            [NotNull] ICurrentDbContext currentContext)
            : base(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(logger, nameof(logger)),
                Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory)),
                Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory)),
                Check.NotNull(currentContext, nameof(currentContext)))
        {
            methodInfoBasedNodeTypeRegistry
                .Register(FromSqlExpressionNode.SupportedMethods, typeof(FromSqlExpressionNode));
        }

        /// <summary>
        ///     Creates a new QueryCompilationContext.
        /// </summary>
        /// <param name="async"> true if the query is asynchronous. </param>
        /// <returns>
        ///     A QueryCompilationContext.
        /// </returns>
        public override QueryCompilationContext Create(bool async)
            => async
                ? new RelationalQueryCompilationContext(
                    Model,
                    (ISensitiveDataLogger)Logger,
                    EntityQueryModelVisitorFactory,
                    RequiresMaterializationExpressionVisitorFactory,
                    new AsyncLinqOperatorProvider(),
                    new AsyncQueryMethodProvider(),
                    ContextType,
                    TrackQueryResults)
                : new RelationalQueryCompilationContext(
                    Model,
                    (ISensitiveDataLogger)Logger,
                    EntityQueryModelVisitorFactory,
                    RequiresMaterializationExpressionVisitorFactory,
                    new LinqOperatorProvider(),
                    new QueryMethodProvider(),
                    ContextType,
                    TrackQueryResults);
    }
}

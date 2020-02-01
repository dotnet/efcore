// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public partial class InMemoryShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly Type _contextType;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

        public InMemoryShapedQueryCompilingExpressionVisitor(
            [NotNull] ShapedQueryCompilingExpressionVisitorDependencies dependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
            _contextType = queryCompilationContext.ContextType;
            _logger = queryCompilationContext.Logger;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            switch (extensionExpression)
            {
                case InMemoryQueryExpression inMemoryQueryExpression:
                    inMemoryQueryExpression.ApplyProjection();
                    return Visit(inMemoryQueryExpression.ServerQueryExpression);

                case InMemoryTableExpression inMemoryTableExpression:
                    return Expression.Call(
                        _tableMethodInfo,
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(inMemoryTableExpression.EntityType));
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            Check.NotNull(shapedQueryExpression, nameof(shapedQueryExpression));

            var inMemoryQueryExpression = (InMemoryQueryExpression)shapedQueryExpression.QueryExpression;

            var shaper = new ShaperExpressionProcessingExpressionVisitor(
                    inMemoryQueryExpression, inMemoryQueryExpression.CurrentParameter)
                .Inject(shapedQueryExpression.ShaperExpression);

            shaper = InjectEntityMaterializers(shaper);

            var innerEnumerable = Visit(inMemoryQueryExpression);

            shaper = new InMemoryProjectionBindingRemovingExpressionVisitor().Visit(shaper);

            shaper = new CustomShaperCompilingExpressionVisitor(IsTracking).Visit(shaper);

            var shaperLambda = (LambdaExpression)shaper;

            return Expression.New(
                typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                QueryCompilationContext.QueryContextParameter,
                innerEnumerable,
                Expression.Constant(shaperLambda.Compile()),
                Expression.Constant(_contextType),
                Expression.Constant(_logger));
        }

        private static readonly MethodInfo _tableMethodInfo
            = typeof(InMemoryShapedQueryCompilingExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(Table));

        private static IEnumerable<ValueBuffer> Table(
            QueryContext queryContext,
            IEntityType entityType)
            => ((InMemoryQueryContext)queryContext).GetValueBuffers(entityType);
    }
}

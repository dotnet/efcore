// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public partial class InMemoryShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly Type _contextType;
        private readonly bool _concurrencyDetectionEnabled;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InMemoryShapedQueryCompilingExpressionVisitor(
            [NotNull] ShapedQueryCompilingExpressionVisitorDependencies dependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
            _contextType = queryCompilationContext.ContextType;
            _concurrencyDetectionEnabled = dependencies.CoreSingletonOptions.IsConcurrencyDetectionEnabled;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
        {
            Check.NotNull(shapedQueryExpression, nameof(shapedQueryExpression));

            var inMemoryQueryExpression = (InMemoryQueryExpression)shapedQueryExpression.QueryExpression;

            var shaper = new ShaperExpressionProcessingExpressionVisitor(
                    inMemoryQueryExpression, inMemoryQueryExpression.CurrentParameter)
                .Inject(shapedQueryExpression.ShaperExpression);

            shaper = InjectEntityMaterializers(shaper);

            var innerEnumerable = Visit(inMemoryQueryExpression);

            shaper = new InMemoryProjectionBindingRemovingExpressionVisitor().Visit(shaper);

            shaper = new CustomShaperCompilingExpressionVisitor(
                QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll).Visit(shaper);

            var shaperLambda = (LambdaExpression)shaper;

            return Expression.New(
                typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                QueryCompilationContext.QueryContextParameter,
                innerEnumerable,
                Expression.Constant(shaperLambda.Compile()),
                Expression.Constant(_contextType),
                Expression.Constant(
                    QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                Expression.Constant(_concurrencyDetectionEnabled));
        }

        private static readonly MethodInfo _tableMethodInfo
            = typeof(InMemoryShapedQueryCompilingExpressionVisitor).GetRequiredDeclaredMethod(nameof(Table));

        private static IEnumerable<ValueBuffer> Table(
            QueryContext queryContext,
            IEntityType entityType)
            => ((InMemoryQueryContext)queryContext).GetValueBuffers(entityType);
    }
}

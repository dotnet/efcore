// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class QueryFlattener
    {
        private readonly IQuerySource _querySource;
        private readonly MethodInfo _operatorToFlatten;
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        private readonly int _readerOffset;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryFlattener(
            [NotNull] IQuerySource querySource,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext,
            [NotNull] MethodInfo operatorToFlatten,
            int readerOffset)
        {
            Check.NotNull(relationalQueryCompilationContext, nameof(relationalQueryCompilationContext));
            Check.NotNull(operatorToFlatten, nameof(operatorToFlatten));

            _querySource = querySource;
            _relationalQueryCompilationContext = relationalQueryCompilationContext;
            _readerOffset = readerOffset;
            _operatorToFlatten = operatorToFlatten;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Flatten([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.Method.MethodIsClosedFormOf(_operatorToFlatten))
            {
                var outerShapedQuery = (MethodCallExpression)methodCallExpression.Arguments[0];

                var outerShaper = (Shaper)((ConstantExpression)outerShapedQuery.Arguments[2]).Value;

                var innerLambda
                    = methodCallExpression.Arguments[1] as LambdaExpression; // SelectMany case

                var innerShapedQuery
                    = innerLambda != null
                        ? (MethodCallExpression)innerLambda.Body
                        : (MethodCallExpression)methodCallExpression.Arguments[1];

                if (innerShapedQuery.Method.Name == "DefaultIfEmpty")
                {
                    innerShapedQuery = (MethodCallExpression)innerShapedQuery.Arguments.Single();
                }

                var innerShaper = (Shaper)((ConstantExpression)innerShapedQuery.Arguments[2]).Value;

                var innerEntityShaper = innerShaper as EntityShaper;

                if (innerEntityShaper != null)
                {
                    innerShaper = innerEntityShaper.WithOffset(_readerOffset);
                }

                var materializerLambda = (LambdaExpression)methodCallExpression.Arguments.Last();
                var materializerReturnType = materializerLambda.ReturnType;
                var materializer = materializerLambda.Compile();

                if (_operatorToFlatten.Name != "_GroupJoin")
                {
                    var compositeShaper
                        = (Shaper)_createCompositeShaperMethodInfo
                            .MakeGenericMethod(
                                outerShaper.Type,
                                innerShaper.Type,
                                materializerReturnType)
                            .Invoke(
                                null,
                                new object[]
                                {
                                    _querySource,
                                    outerShaper,
                                    innerShaper,
                                    materializer
                                });

                    compositeShaper.SaveAccessorExpression(
                        _relationalQueryCompilationContext.QuerySourceMapping);

                    return Expression.Call(
                        outerShapedQuery.Method
                            .GetGenericMethodDefinition()
                            .MakeGenericMethod(materializerReturnType),
                        outerShapedQuery.Arguments[0],
                        outerShapedQuery.Arguments[1],
                        Expression.Constant(compositeShaper));
                }

                var groupJoinMethod
                    = _relationalQueryCompilationContext.QueryMethodProvider
                        .GroupJoinMethod
                        .MakeGenericMethod(
                            outerShaper.Type,
                            innerShaper.Type,
                            ((LambdaExpression)methodCallExpression.Arguments[2]).ReturnType,
                            materializerReturnType);

                var newShapedQueryMethod
                    = Expression.Call(
                        _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod,
                        outerShapedQuery.Arguments[0],
                        outerShapedQuery.Arguments[1],
                        Expression.Default(typeof(int?)));

                var defaultGroupJoinInclude
                    = Expression.Default(
                        _relationalQueryCompilationContext.QueryMethodProvider.GroupJoinIncludeType);

                return
                    Expression.Call(
                        groupJoinMethod,
                        Expression.Convert(
                            EntityQueryModelVisitor.QueryContextParameter,
                            typeof(RelationalQueryContext)),
                        newShapedQueryMethod,
                        Expression.Constant(outerShaper),
                        Expression.Constant(innerShaper),
                        methodCallExpression.Arguments[3],
                        methodCallExpression.Arguments[4],
                        defaultGroupJoinInclude,
                        defaultGroupJoinInclude);
            }

            return methodCallExpression;
        }

        private static readonly MethodInfo _createCompositeShaperMethodInfo
            = typeof(QueryFlattener).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateCompositeShaper));

        [UsedImplicitly]
        private static CompositeShaper<TOuter, TInner, TResult> CreateCompositeShaper<TOuter, TInner, TResult>(
            IQuerySource querySource,
            IShaper<TOuter> outerShaper,
            IShaper<TInner> innerShaper,
            Func<TOuter, TInner, TResult> materializer)
            => new CompositeShaper<TOuter, TInner, TResult>(
                querySource, outerShaper, innerShaper, materializer);

        private class CompositeShaper<TOuter, TInner, TResult> : Shaper, IShaper<TResult>
        {
            private readonly IShaper<TOuter> _outerShaper;
            private readonly IShaper<TInner> _innerShaper;
            private readonly Func<TOuter, TInner, TResult> _materializer;

            public CompositeShaper(
                IQuerySource querySource,
                IShaper<TOuter> outerShaper,
                IShaper<TInner> innerShaper,
                Func<TOuter, TInner, TResult> materializer)
                : base(querySource)
            {
                _outerShaper = outerShaper;
                _innerShaper = innerShaper;
                _materializer = materializer;
            }

            public TResult Shape(QueryContext queryContext, ValueBuffer valueBuffer)
                => _materializer(
                    _outerShaper.Shape(queryContext, valueBuffer),
                    _innerShaper.Shape(queryContext, valueBuffer));

            public override Type Type => typeof(TResult);

            public override bool IsShaperForQuerySource(IQuerySource querySource)
                => base.IsShaperForQuerySource(querySource)
                   || _outerShaper.IsShaperForQuerySource(querySource)
                   || _innerShaper.IsShaperForQuerySource(querySource);

            public override void SaveAccessorExpression(QuerySourceMapping querySourceMapping)
            {
                _outerShaper.SaveAccessorExpression(querySourceMapping);
                _innerShaper.SaveAccessorExpression(querySourceMapping);
            }

            public override Expression GetAccessorExpression(IQuerySource querySource)
                => _outerShaper.GetAccessorExpression(querySource)
                   ?? _innerShaper.GetAccessorExpression(querySource);
        }
    }
}

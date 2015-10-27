// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class QueryFlattener
    {
        private readonly IQuerySource _querySource;
        private readonly MethodInfo _operatorToFlatten;
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        private readonly int _readerOffset;

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
                        = _createCompositeShaperMethodInfo
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
                        _relationalQueryCompilationContext.QueryMethodProvider
                            .QueryMethod,
                        outerShapedQuery.Arguments[0],
                        outerShapedQuery.Arguments[1],
                        Expression.Default(typeof(int?)));

                return
                    Expression.Call(
                        groupJoinMethod,
                        EntityQueryModelVisitor.QueryContextParameter,
                        newShapedQueryMethod,
                        Expression.Constant(outerShaper),
                        Expression.Constant(innerShaper),
                        methodCallExpression.Arguments[3],
                        methodCallExpression.Arguments[4]);
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
        }
    }
}

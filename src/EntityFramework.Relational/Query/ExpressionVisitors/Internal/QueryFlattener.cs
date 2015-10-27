// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class QueryFlattener
    {
        private readonly MethodInfo _operatorToFlatten;
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        private readonly int _readerOffset;

        public QueryFlattener(
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext,
            [NotNull] MethodInfo operatorToFlatten,
            int readerOffset)
        {
            Check.NotNull(relationalQueryCompilationContext, nameof(relationalQueryCompilationContext));
            Check.NotNull(operatorToFlatten, nameof(operatorToFlatten));

            _relationalQueryCompilationContext = relationalQueryCompilationContext;
            _readerOffset = readerOffset;
            _operatorToFlatten = operatorToFlatten;
        }

        public virtual Expression Flatten([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.Method.MethodIsClosedFormOf(_operatorToFlatten))
            {
                var outerShapedQuery = methodCallExpression.Arguments[0];

                var outerShaper
                    = ((LambdaExpression)
                        ((MethodCallExpression)outerShapedQuery)
                            .Arguments[2])
                        .Body;

                var innerLambda
                    = methodCallExpression.Arguments[1] as LambdaExpression; // SelectMany case

                var innerShapedQuery
                    = innerLambda != null
                        ? (MethodCallExpression)innerLambda.Body
                        : (MethodCallExpression)methodCallExpression.Arguments[1];

                var innerShaper
                    = (MethodCallExpression)
                        ((LambdaExpression)
                            innerShapedQuery.Arguments[2]).Body;

                if (innerShaper.Arguments.Count > 3)
                {
                    // CreateEntity shaper, adjust the valueBufferOffset and allowNullResult

                    var newArguments = innerShaper.Arguments.ToList();

                    var oldBufferOffset
                        = (int)((ConstantExpression)innerShaper.Arguments[2]).Value;

                    newArguments[2] = Expression.Constant(oldBufferOffset + _readerOffset);
                    newArguments[8] = Expression.Constant(true);

                    innerShaper = innerShaper.Update(innerShaper.Object, newArguments);
                }

                var resultSelector
                    = (MethodCallExpression)
                        ((LambdaExpression)methodCallExpression
                            .Arguments.Last())
                            .Body;

                if (_operatorToFlatten.Name != "_GroupJoin")
                {
                    var newResultSelector
                        = Expression.Lambda(
                            Expression.Call(resultSelector.Method, outerShaper, innerShaper),
                            (ParameterExpression)innerShaper.Arguments[1]);

                    return Expression.Call(
                        ((MethodCallExpression)outerShapedQuery).Method
                            .GetGenericMethodDefinition()
                            .MakeGenericMethod(newResultSelector.ReturnType),
                        ((MethodCallExpression)outerShapedQuery).Arguments[0],
                        ((MethodCallExpression)outerShapedQuery).Arguments[1],
                        newResultSelector);
                }

                var groupJoinMethod
                    = _relationalQueryCompilationContext.QueryMethodProvider
                        .GroupJoinMethod
                        .MakeGenericMethod(
                            outerShaper.Type,
                            innerShaper.Type,
                            ((LambdaExpression)methodCallExpression.Arguments[2]).ReturnType,
                            resultSelector.Type);

                var newShapedQueryMethod
                    = Expression.Call(
                        _relationalQueryCompilationContext.QueryMethodProvider
                            .QueryMethod,
                        ((MethodCallExpression)outerShapedQuery).Arguments[0],
                        ((MethodCallExpression)outerShapedQuery).Arguments[1],
                        Expression.Default(typeof(int?)));

                return
                    Expression.Call(
                        groupJoinMethod,
                        newShapedQueryMethod,
                        Expression
                            .Lambda(
                                outerShaper,
                                (ParameterExpression)innerShaper.Arguments[1]),
                        Expression
                            .Lambda(
                                innerShaper,
                                (ParameterExpression)innerShaper.Arguments[1]),
                        methodCallExpression.Arguments[3],
                        methodCallExpression.Arguments[4]);
            }

            return methodCallExpression;
        }
    }
}

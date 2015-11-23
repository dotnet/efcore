// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class ParameterExtractingExpressionVisitor : ExpressionVisitorBase
    {
        public static Expression ExtractParameters(
            [NotNull] Expression expression,
            [NotNull] QueryContext queryContext,
            [NotNull] IEvaluatableExpressionFilter evaluatableExpressionFilter,
            [NotNull] ISensitiveDataLogger logger)
        {
            var partialEvaluationInfo
                = EvaluatableTreeFindingExpressionVisitor
                    .Analyze(expression, evaluatableExpressionFilter);

            var visitor = new ParameterExtractingExpressionVisitor(partialEvaluationInfo, queryContext, logger);

            return visitor.Visit(expression);
        }

        private readonly PartialEvaluationInfo _partialEvaluationInfo;
        private readonly QueryContext _queryContext;
        private readonly ISensitiveDataLogger _logger;

        private ParameterExtractingExpressionVisitor(
            PartialEvaluationInfo partialEvaluationInfo,
            QueryContext queryContext,
            ISensitiveDataLogger logger)
        {
            _partialEvaluationInfo = partialEvaluationInfo;
            _queryContext = queryContext;
            _logger = logger;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsGenericMethod
                && ReferenceEquals(
                    node.Method.GetGenericMethodDefinition(),
                    EntityQueryModelVisitor.PropertyMethodInfo))
            {
                return node;
            }

            node = ProcessNotParameterizableArguments(node);

            return base.VisitMethodCall(node);
        }

        private static MethodCallExpression ProcessNotParameterizableArguments(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsStatic
                && (methodCallExpression.Method.DeclaringType != typeof(Queryable)))
            {
                var parameterInfos = methodCallExpression.Method.GetParameters();

                Expression[] newArgs = null;

                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    if ((parameterInfos[i].GetCustomAttribute<NotParameterizedAttribute>() != null)
                        && !(methodCallExpression.Arguments[i] is ConstantExpression))
                    {
                        if (newArgs == null)
                        {
                            newArgs = new Expression[parameterInfos.Length];

                            for (var j = 0; j < i; j++)
                            {
                                newArgs[j] = methodCallExpression.Arguments[j];
                            }
                        }

                        string _;
                        newArgs[i] = Expression.Constant(Evaluate(methodCallExpression.Arguments[i], out _));
                    }
                    else if (newArgs != null)
                    {
                        newArgs[i] = methodCallExpression.Arguments[i];
                    }
                }

                if (newArgs != null)
                {
                    methodCallExpression
                        = methodCallExpression.Update(methodCallExpression.Object, newArgs);
                }
            }
            return methodCallExpression;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            if ((node.NodeType == ExpressionType.Lambda)
                || !_partialEvaluationInfo.IsEvaluatableExpression(node))
            {
                return base.Visit(node);
            }

            var e = node;

            if (node.NodeType == ExpressionType.Convert)
            {
                if (node.RemoveConvert() is ConstantExpression)
                {
                    return node;
                }

                var unaryExpression = (UnaryExpression)node;

                if ((unaryExpression.Type.IsNullableType()
                     && !unaryExpression.Operand.Type.IsNullableType())
                    || (unaryExpression.Type == typeof(object)))
                {
                    e = unaryExpression.Operand;
                }
            }

            if (!typeof(IQueryable).GetTypeInfo().IsAssignableFrom(e.Type.GetTypeInfo()))
            {
                var constantExpression = e as ConstantExpression;

                if ((constantExpression == null)
                    || (constantExpression.Value is IEnumerable && (constantExpression.Type != typeof(string)) && (constantExpression.Type != typeof(byte[]))))
                {
                    try
                    {
                        string parameterName;

                        var parameterValue = Evaluate(e, out parameterName);

                        if (parameterName == null)
                        {
                            parameterName = "p";
                        }

                        var compilerPrefixIndex
                            = parameterName.LastIndexOf(">", StringComparison.Ordinal);

                        if (compilerPrefixIndex != -1)
                        {
                            parameterName = parameterName.Substring(compilerPrefixIndex + 1);
                        }

                        parameterName
                            = CompiledQueryCache.CompiledQueryParameterPrefix
                              + parameterName
                              + "_"
                              + _queryContext.ParameterValues.Count;

                        _queryContext.AddParameter(parameterName, parameterValue);

                        return e.Type == node.Type
                            ? Expression.Parameter(e.Type, parameterName)
                            : (Expression)Expression.Convert(
                                Expression.Parameter(e.Type, parameterName),
                                node.Type);
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(
                            _logger.LogSensitiveData
                                ? CoreStrings.ExpressionParameterizationExceptionSensitive(node)
                                : CoreStrings.ExpressionParameterizationException,
                            exception);
                    }
                }
            }

            return node;
        }

        public static object Evaluate(
            [CanBeNull] Expression expression,
            [CanBeNull] out string parameterName)
        {
            parameterName = null;

            if (expression == null)
            {
                return null;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        var memberExpression = (MemberExpression)expression;
                        var @object = Evaluate(memberExpression.Expression, out parameterName);

                        var fieldInfo = memberExpression.Member as FieldInfo;

                        if (fieldInfo != null)
                        {
                            parameterName = parameterName != null
                                ? parameterName + "_" + fieldInfo.Name
                                : fieldInfo.Name;

                            try
                            {
                                return fieldInfo.GetValue(@object);
                            }
                            catch
                            {
                                // Try again when we compile the delegate
                            }
                        }

                        var propertyInfo = memberExpression.Member as PropertyInfo;

                        if (propertyInfo != null)
                        {
                            parameterName = parameterName != null
                                ? parameterName + "_" + propertyInfo.Name
                                : propertyInfo.Name;

                            try
                            {
                                return propertyInfo.GetValue(@object);
                            }
                            catch
                            {
                                // Try again when we compile the delegate
                            }
                        }

                        break;
                    }
                case ExpressionType.Constant:
                    {
                        return ((ConstantExpression)expression).Value;
                    }
                case ExpressionType.Call:
                    {
                        parameterName = ((MethodCallExpression)expression).Method.Name;

                        break;
                    }
            }

            return Expression.Lambda<Func<object>>(
                Expression.Convert(expression, typeof(object)))
                .Compile()
                .Invoke();
        }
    }
}

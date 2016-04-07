// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class ParameterExtractingExpressionVisitor : ExpressionVisitor
    {
        private static readonly TypeInfo _queryableTypeInfo = typeof(IQueryable).GetTypeInfo();

        public static Expression ExtractParameters(
            [NotNull] Expression expression,
            [NotNull] QueryContext queryContext,
            [NotNull] IEvaluatableExpressionFilter evaluatableExpressionFilter,
            [NotNull] ISensitiveDataLogger logger)
        {
            var visitor = new ParameterExtractingExpressionVisitor(evaluatableExpressionFilter, queryContext, logger);

            return visitor.ExtractParameters(expression);
        }

        private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;
        private readonly QueryContext _queryContext;
        private readonly ISensitiveDataLogger _logger;

        private PartialEvaluationInfo _partialEvaluationInfo;

        private bool _inLambda;

        private ParameterExtractingExpressionVisitor(
            IEvaluatableExpressionFilter evaluatableExpressionFilter,
            QueryContext queryContext,
            ISensitiveDataLogger logger)
        {
            _evaluatableExpressionFilter = evaluatableExpressionFilter;
            _queryContext = queryContext;
            _logger = logger;
        }

        public Expression ExtractParameters([NotNull] Expression expression)
        {
            var oldPartialEvaluationInfo = _partialEvaluationInfo;

            _partialEvaluationInfo
                = EvaluatableTreeFindingExpressionVisitor
                    .Analyze(expression, _evaluatableExpressionFilter);

            try
            {
                return Visit(expression);
            }
            finally
            {
                _partialEvaluationInfo = oldPartialEvaluationInfo;
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var methodInfo = methodCallExpression.Method;
            var declaringType = methodInfo.DeclaringType;

            if (declaringType == typeof(DbContext))
            {
                return methodCallExpression;
            }

            if (!methodInfo.IsStatic
                || declaringType == typeof(Queryable)
                || declaringType == typeof(EntityFrameworkQueryableExtensions))
            {
                return base.VisitMethodCall(methodCallExpression);
            }

            ParameterInfo[] parameterInfos = null;
            Expression[] newArguments = null;

            var arguments = methodCallExpression.Arguments;

            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                var newArgument = Visit(argument);

                if (newArgument != argument)
                {
                    if (newArguments == null)
                    {
                        parameterInfos = methodInfo.GetParameters();
                        newArguments = new Expression[arguments.Count];

                        for (var j = 0; j < i; j++)
                        {
                            newArguments[j] = arguments[j];
                        }
                    }

                    if (parameterInfos[i].GetCustomAttribute<NotParameterizedAttribute>() != null)
                    {
                        var parameter = newArgument as ParameterExpression;

                        if (parameter != null)
                        {
                            newArgument = Expression.Constant(_queryContext.RemoveParameter(parameter.Name));
                        }
                    }

                    newArguments[i] = newArgument;
                }
                else if (newArguments != null)
                {
                    newArguments[i] = newArgument;
                }
            }

            if (newArguments != null)
            {
                methodCallExpression = methodCallExpression.Update(methodCallExpression.Object, newArguments);
            }

            return methodCallExpression;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (!_partialEvaluationInfo.IsEvaluatableExpression(memberExpression))
            {
                return base.VisitMember(memberExpression);
            }

            if (!_queryableTypeInfo.IsAssignableFrom(memberExpression.Type.GetTypeInfo()))
            {
                return TryExtractParameter(memberExpression);
            }

            string _;
            var queryable = (IQueryable)Evaluate(memberExpression, out _);

            return ExtractParameters(queryable.Expression);
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => !_inLambda
               && _partialEvaluationInfo.IsEvaluatableExpression(constantExpression)
               && !_queryableTypeInfo.IsAssignableFrom(constantExpression.Type.GetTypeInfo())
                ? TryExtractParameter(constantExpression)
                : constantExpression;

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var oldInLambda = _inLambda;

            _inLambda = true;

            try
            {
                return base.VisitLambda(node);
            }
            finally
            {
                _inLambda = oldInLambda;
            }
        }

        private Expression TryExtractParameter(Expression expression)
        {
            try
            {
                string parameterName;

                var parameterValue = Evaluate(expression, out parameterName);

                var parameterExpression = parameterValue as Expression;
                if (parameterExpression != null)
                {
                    return parameterExpression;
                }

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

                return Expression.Parameter(expression.Type, parameterName);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    _logger.LogSensitiveData
                        ? CoreStrings.ExpressionParameterizationExceptionSensitive(expression)
                        : CoreStrings.ExpressionParameterizationException,
                    exception);
            }
        }

        public static object Evaluate([CanBeNull] Expression expression, [CanBeNull] out string parameterName)
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

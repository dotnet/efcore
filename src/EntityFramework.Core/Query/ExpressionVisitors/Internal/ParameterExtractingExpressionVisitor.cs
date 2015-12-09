// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public class ParameterExtractingExpressionVisitor : ExpressionVisitor
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

        private bool _inLambda;

        private ParameterExtractingExpressionVisitor(
            PartialEvaluationInfo partialEvaluationInfo,
            QueryContext queryContext,
            ISensitiveDataLogger logger)
        {
            _partialEvaluationInfo = partialEvaluationInfo;
            _queryContext = queryContext;
            _logger = logger;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var methodInfo = methodCallExpression.Method;
            var declaringType = methodInfo.DeclaringType;

            if (declaringType == typeof(EF)
                || declaringType == typeof(DbContext))
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
            => !_partialEvaluationInfo.IsEvaluatableExpression(memberExpression)
                ? base.VisitMember(memberExpression)
                : !typeof(IQueryable).GetTypeInfo().IsAssignableFrom(memberExpression.Type.GetTypeInfo())
                    ? TryExtractParameter(memberExpression)
                    : memberExpression;

        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => !_inLambda
               && _partialEvaluationInfo.IsEvaluatableExpression(constantExpression)
               && !typeof(IQueryable).GetTypeInfo().IsAssignableFrom(constantExpression.Type.GetTypeInfo())
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

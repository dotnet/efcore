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
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ParameterExtractingExpressionVisitor : ExpressionVisitor
    {
        private static readonly TypeInfo _queryableTypeInfo = typeof(IQueryable).GetTypeInfo();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Expression ExtractParameters(
            [NotNull] Expression expression,
            [NotNull] QueryContext queryContext,
            [NotNull] IEvaluatableExpressionFilter evaluatableExpressionFilter,
            [NotNull] ISensitiveDataLogger logger,
            bool parameterize)
        {
            var visitor 
                = new ParameterExtractingExpressionVisitor(
                    evaluatableExpressionFilter, 
                    queryContext, 
                    logger,
                    parameterize);

            return visitor.ExtractParameters(expression);
        }

        private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;
        private readonly QueryContext _queryContext;
        private readonly ISensitiveDataLogger _logger;
        private readonly bool _parameterize;

        private PartialEvaluationInfo _partialEvaluationInfo;

        private bool _inLambda;

        private ParameterExtractingExpressionVisitor(
            IEvaluatableExpressionFilter evaluatableExpressionFilter,
            QueryContext queryContext,
            ISensitiveDataLogger logger,
            bool parameterize)
        {
            _evaluatableExpressionFilter = evaluatableExpressionFilter;
            _queryContext = queryContext;
            _logger = logger;
            _parameterize = parameterize;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var methodInfo = methodCallExpression.Method;
            var declaringType = methodInfo.DeclaringType;

            if (declaringType == typeof(DbContext))
            {
                return methodCallExpression;
            }

            if (declaringType == typeof(Queryable)
                || declaringType == typeof(EntityFrameworkQueryableExtensions)
                && (!methodInfo.IsGenericMethod
                    || methodInfo.GetGenericMethodDefinition() != EntityFrameworkQueryableExtensions.StringIncludeMethodInfo))
            {
                return base.VisitMethodCall(methodCallExpression);
            }

            if (_partialEvaluationInfo.IsEvaluatableExpression(methodCallExpression))
            {
                return TryExtractParameter(methodCallExpression);
            }

            if (!methodInfo.IsStatic)
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
                        var parameter = newArgument.RemoveConvert() as ParameterExpression;

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            var detachableContext = constantExpression.Value as IDetachableContext;

            if (detachableContext != null)
            {
                return Expression.Constant(detachableContext.DetachContext());
            }

            return !_inLambda
                   && _partialEvaluationInfo.IsEvaluatableExpression(constantExpression)
                   && !_queryableTypeInfo.IsAssignableFrom(constantExpression.Type.GetTypeInfo())
                ? TryExtractParameter(constantExpression)
                : constantExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var newExpression = base.VisitUnary(unaryExpression);

            if (newExpression != unaryExpression
                && newExpression.NodeType == ExpressionType.Convert)
            {
                var newUnaryExpression = (UnaryExpression)newExpression;

                if (newUnaryExpression.Operand.NodeType == ExpressionType.Parameter
                    && newUnaryExpression.Operand.Type == typeof(object))
                {
                    return Expression.Parameter(
                        newUnaryExpression.Type,
                        ((ParameterExpression)newUnaryExpression.Operand).Name);
                }
            }

            return newExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (!binaryExpression.IsLogicalOperation())
            {
                return base.VisitBinary(binaryExpression);
            }

            var newLeftExpression = TryOptimize(binaryExpression.Left) ?? Visit(binaryExpression.Left);

            var leftConstantExpression = newLeftExpression as ConstantExpression;
            if (leftConstantExpression != null)
            {
                var constantValue = (bool)leftConstantExpression.Value;
                if (constantValue && binaryExpression.NodeType == ExpressionType.OrElse
                    || !constantValue && binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    return newLeftExpression;
                }
            }

            var newRightExpression = TryOptimize(binaryExpression.Right) ?? Visit(binaryExpression.Right);

            var rightConstantExpression = newRightExpression as ConstantExpression;
            if (rightConstantExpression != null)
            {
                var constantValue = (bool)rightConstantExpression.Value;
                if (constantValue && binaryExpression.NodeType == ExpressionType.OrElse
                    || !constantValue && binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    return newRightExpression;
                }
            }

            return binaryExpression.Update(newLeftExpression, binaryExpression.Conversion, newRightExpression);
        }

        private Expression TryOptimize(Expression expression)
        {
            if (_partialEvaluationInfo.IsEvaluatableExpression(expression)
                && !_queryableTypeInfo.IsAssignableFrom(expression.Type.GetTypeInfo()))
            {
                string _;
                var value = Evaluate(expression, out _);
                if (value is bool)
                {
                    return Expression.Constant(value, typeof(bool));
                }
            }

            return null;
        }

        private Expression TryExtractParameter(Expression expression)
        {
            string parameterName;

            var parameterValue = Evaluate(expression, out parameterName);

            var parameterExpression = parameterValue as Expression;

            if (parameterExpression != null)
            {
                return parameterExpression;
            }

            if (!_parameterize)
            {
                return Expression.Constant(parameterValue);
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public object Evaluate([CanBeNull] Expression expression, [CanBeNull] out string parameterName)
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

            try
            {
                return Expression.Lambda<Func<object>>(
                        Expression.Convert(expression, typeof(object)))
                    .Compile()
                    .Invoke();
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
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
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
        private const string QueryFilterPrefix = "ef_filter";

        private static readonly TypeInfo _queryableTypeInfo = typeof(IQueryable).GetTypeInfo();

        private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;
        private readonly IParameterValues _parameterValues;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private readonly ContextParameterReplacingExpressionVisitor _contextParameterReplacingExpressionVisitor;

        private readonly bool _parameterize;
        private readonly bool _generateContextAccessors;

        private PartialEvaluationInfo _partialEvaluationInfo;

        private bool _inLambda;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ParameterExtractingExpressionVisitor(
            [NotNull] IEvaluatableExpressionFilter evaluatableExpressionFilter,
            [NotNull] IParameterValues parameterValues,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            bool parameterize,
            bool generateContextAccessors = false)
        {
            _evaluatableExpressionFilter = evaluatableExpressionFilter;
            _parameterValues = parameterValues;
            _logger = logger;
            _parameterize = parameterize;
            _generateContextAccessors = generateContextAccessors;

            if (_generateContextAccessors)
            {
                _contextParameterReplacingExpressionVisitor = new ContextParameterReplacingExpressionVisitor();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression ExtractParameters([NotNull] Expression expression)
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

            if (declaringType == typeof(Queryable)
                || declaringType == typeof(EntityFrameworkQueryableExtensions)
                && (!methodInfo.IsGenericMethod
                    || !methodInfo.GetGenericMethodDefinition()
                        .Equals(EntityFrameworkQueryableExtensions.StringIncludeMethodInfo)))
            {
                return base.VisitMethodCall(methodCallExpression);
            }

            var returnTypeInfo = methodInfo.ReturnType.GetTypeInfo();

            if (returnTypeInfo.IsGenericType
                && returnTypeInfo.GetGenericTypeDefinition() == typeof(DbSet<>))
            {
                if (_generateContextAccessors)
                {
                    return NullAsyncQueryProvider.Instance
                        .CreateEntityQueryableExpression(
                            methodCallExpression.Type.GetGenericArguments()[0]);
                }

                var queryable = (IQueryable)Evaluate(methodCallExpression, out _);

                return ExtractParameters(queryable.Expression);
            }

            if (_partialEvaluationInfo.IsEvaluatableExpression(methodCallExpression))
            {
                if (_queryableTypeInfo.IsAssignableFrom(methodCallExpression.Type))
                {
                    var queryable = (IQueryable)Evaluate(methodCallExpression, out _);

                    return ExtractParameters(queryable.Expression);
                }

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
                        if (newArgument.RemoveConvert() is ParameterExpression parameter)
                        {
                            var parameterValue = _parameterValues.RemoveParameter(parameter.Name);

                            if (parameter.Type == typeof(FormattableString))
                            {
                                if (Evaluate(methodCallExpression, out _) is IQueryable queryable)
                                {
                                    var oldInLambda = _inLambda;

                                    _inLambda = false;

                                    try
                                    {
                                        return ExtractParameters(queryable.Expression);
                                    }
                                    finally
                                    {
                                        _inLambda = oldInLambda;
                                    }
                                }
                            }
                            else
                            {
                                var constantParameterValue = Expression.Constant(parameterValue);

                                if (newArgument is UnaryExpression unaryExpression
                                    && unaryExpression.NodeType == ExpressionType.Convert)
                                {
                                    newArgument = unaryExpression.Update(constantParameterValue);
                                }
                                else
                                {
                                    newArgument = constantParameterValue;
                                }
                            }
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

            if (_generateContextAccessors
                && memberExpression.Type.IsGenericType
                && memberExpression.Type.GetGenericTypeDefinition() == typeof(DbSet<>))
            {
                return NullAsyncQueryProvider.Instance
                    .CreateEntityQueryableExpression(
                        memberExpression.Type.GetGenericArguments()[0]);
            }

            var queryable = (IQueryable)Evaluate(memberExpression, out _);

            return ExtractParameters(queryable.Expression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Value is IDetachableContext detachableContext)
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
        protected override Expression VisitNew(NewExpression node)
        {
            var arguments = Visit(node.Arguments);

            var newNode = node.Update(arguments);

            return newNode;
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
            if (binaryExpression.NodeType == ExpressionType.ArrayIndex
                && _partialEvaluationInfo.IsEvaluatableExpression(binaryExpression))
            {
                return TryExtractParameter(binaryExpression);
            }

            if (!binaryExpression.IsLogicalOperation())
            {
                return base.VisitBinary(binaryExpression);
            }

            var newLeftExpression = TryOptimize(binaryExpression.Left) ?? Visit(binaryExpression.Left);

            if (newLeftExpression is ConstantExpression leftConstantExpression)
            {
                var constantValue = (bool)leftConstantExpression.Value;
                if (constantValue && binaryExpression.NodeType == ExpressionType.OrElse
                    || !constantValue && binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    return newLeftExpression;
                }
            }

            var newRightExpression = TryOptimize(binaryExpression.Right) ?? Visit(binaryExpression.Right);

            if (newRightExpression is ConstantExpression rightConstantExpression)
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
            var parameterValue = Evaluate(expression, out var parameterName);

            if (parameterName == null
                || !parameterName.StartsWith(QueryFilterPrefix, StringComparison.Ordinal))
            {
                if (parameterValue is Expression valueExpression)
                {
                    return ExtractParameters(valueExpression);
                }

                if (!_parameterize)
                {
                    return Expression.Constant(parameterValue, expression.Type);
                }
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
                  + _parameterValues.ParameterValues.Count;

            _parameterValues.AddParameter(parameterName, parameterValue);

            return Expression.Parameter(expression.Type, parameterName);
        }

        private sealed class ContextParameterReplacingExpressionVisitor : ExpressionVisitor
        {
            public ParameterExpression ContextParameterExpression;

            protected override Expression VisitConstant(ConstantExpression constantExpression)
            {
                if (typeof(DbContext).GetTypeInfo()
                    .IsAssignableFrom(constantExpression.Type.GetTypeInfo()))
                {
                    return ContextParameterExpression
                           ?? (ContextParameterExpression
                               = Expression.Parameter(constantExpression.Type, "context"));
                }

                return constantExpression;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object Evaluate([CanBeNull] Expression expression, out string parameterName)
        {
            parameterName = null;

            if (expression == null)
            {
                return null;
            }

            if (_generateContextAccessors)
            {
                var newExpression = _contextParameterReplacingExpressionVisitor.Visit(expression);

                if (newExpression != expression)
                {
                    parameterName = QueryFilterPrefix + "__"
                                    + (expression is MemberExpression memberExpression
                                        ? memberExpression.Member.Name
                                        : QueryFilterPrefix);

                    return Expression.Lambda(
                        newExpression,
                        _contextParameterReplacingExpressionVisitor.ContextParameterExpression);
                }
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                {
                    var memberExpression = (MemberExpression)expression;
                    var @object = Evaluate(memberExpression.Expression, out parameterName);

                    if (memberExpression.Member is FieldInfo fieldInfo)
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

                    if (memberExpression.Member is PropertyInfo propertyInfo)
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
                    _logger.ShouldLogSensitiveData()
                        ? CoreStrings.ExpressionParameterizationExceptionSensitive(expression)
                        : CoreStrings.ExpressionParameterizationException,
                    exception);
            }
        }
    }
}

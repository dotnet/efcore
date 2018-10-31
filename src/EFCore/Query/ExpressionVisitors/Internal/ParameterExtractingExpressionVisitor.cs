// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class ParameterExtractingExpressionVisitor : ExpressionVisitor
    {
        private const string QueryFilterPrefix = "ef_filter";

        private readonly IParameterValues _parameterValues;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private readonly bool _parameterize;
        private readonly bool _generateContextAccessors;
        private readonly EvaluatableExpressionFindingExpressionVisitor _evaluatableExpressionFindingExpressionVisitor;
        private readonly ContextParameterReplacingExpressionVisitor _contextParameterReplacingExpressionVisitor;

        private readonly Dictionary<Expression, Expression> _evaluatedValues
            = new Dictionary<Expression, Expression>(ExpressionEqualityComparer.Instance);

        private IDictionary<Expression, bool> _evaluatableExpressions;
        private IQueryProvider _currentQueryProvider;

        public ParameterExtractingExpressionVisitor(
            IEvaluatableExpressionFilter evaluatableExpressionFilter,
            IParameterValues parameterValues,
            Type contextType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            bool parameterize,
            bool generateContextAccessors)
        {
            _evaluatableExpressionFindingExpressionVisitor
                = new EvaluatableExpressionFindingExpressionVisitor(evaluatableExpressionFilter);
            _parameterValues = parameterValues;
            _logger = logger;
            _parameterize = parameterize;
            _generateContextAccessors = generateContextAccessors;
            if (_generateContextAccessors)
            {
                _contextParameterReplacingExpressionVisitor
                    = new ContextParameterReplacingExpressionVisitor(contextType);
            }
        }

        public virtual Expression ExtractParameters(Expression expression)
        {
            var oldEvaluatableExpressions = _evaluatableExpressions;
            _evaluatableExpressions = _evaluatableExpressionFindingExpressionVisitor.Find(expression);

            try
            {
                return Visit(expression);
            }
            finally
            {
                _evaluatableExpressions = oldEvaluatableExpressions;
                _evaluatedValues.Clear();
            }
        }

        public override Expression Visit(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (_evaluatableExpressions.TryGetValue(expression, out var generateParameter)
                && !PreserveConvertNode(expression))
            {
                return Evaluate(expression, _parameterize && generateParameter);
            }

            return base.Visit(expression);
        }

        private static bool PreserveConvertNode(Expression expression)
        {
            if (expression is UnaryExpression unaryExpression
                && (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked))
            {
                if (unaryExpression.Type == typeof(object)
                    || unaryExpression.Type == typeof(Enum))
                {
                    return true;
                }

                if (unaryExpression.Operand.Type.UnwrapNullableType().IsEnum)
                {
                    return true;
                }
            }

            return false;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (!binaryExpression.IsLogicalOperation())
            {
                return base.VisitBinary(binaryExpression);
            }

            var newLeftExpression = TryGetConstantValue(binaryExpression.Left) ?? Visit(binaryExpression.Left);
            if (ShortCircuitBinaryExpression(newLeftExpression, binaryExpression.NodeType))
            {
                return newLeftExpression;
            }

            var newRightExpression = TryGetConstantValue(binaryExpression.Right) ?? Visit(binaryExpression.Right);
            if (ShortCircuitBinaryExpression(newRightExpression, binaryExpression.NodeType))
            {
                return newRightExpression;
            }

            return binaryExpression.Update(newLeftExpression, binaryExpression.Conversion, newRightExpression);
        }

        private Expression TryGetConstantValue(Expression expression)
        {
            if (_evaluatableExpressions.ContainsKey(expression))
            {
                var value = GetValue(expression, out var _);

                if (value is bool)
                {
                    return Expression.Constant(value, typeof(bool));
                }
            }

            return null;
        }

        private static bool ShortCircuitBinaryExpression(Expression expression, ExpressionType nodeType)
            => expression is ConstantExpression constantExpression
                && constantExpression.Value is bool constantValue
                && ((constantValue && nodeType == ExpressionType.OrElse)
                    || (!constantValue && nodeType == ExpressionType.AndAlso));

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Value is IDetachableContext detachableContext)
            {
                var queryProvider = ((IQueryable)constantExpression.Value).Provider;
                if (_currentQueryProvider == null)
                {
                    _currentQueryProvider = queryProvider;
                }
                else if (!ReferenceEquals(queryProvider, _currentQueryProvider)
                    && queryProvider.GetType() == _currentQueryProvider.GetType())
                {
                    throw new InvalidOperationException(CoreStrings.ErrorInvalidQueryable);
                }

                return Expression.Constant(detachableContext.DetachContext());
            }

            return base.VisitConstant(constantExpression);
        }

        private static Expression GenerateConstantExpression(object value, Type returnType)
        {
            var constantExpression = Expression.Constant(value, value?.GetType() ?? returnType);

            return constantExpression.Type != returnType
                ? Expression.Convert(constantExpression, returnType)
                : (Expression)constantExpression;
        }

        private Expression Evaluate(Expression expression, bool generateParameter)
        {
            if (_evaluatedValues.TryGetValue(expression, out var cachedValue))
            {
                return cachedValue;
            }

            var parameterValue = GetValue(expression, out var parameterName);

            if (parameterValue is IQueryable innerQueryable)
            {
                return ExtractParameters(innerQueryable.Expression);
            }

            if (parameterName?.StartsWith(QueryFilterPrefix, StringComparison.Ordinal) != true)
            {
                if (parameterValue is Expression innerExpression)
                {
                    return ExtractParameters(innerExpression);
                }

                if (!generateParameter)
                {
                    var constantValue = GenerateConstantExpression(parameterValue, expression.Type);

                    _evaluatedValues.Add(expression, constantValue);

                    return constantValue;
                }
            }

            if (parameterName == null)
            {
                parameterName = "p";
            }

            if (string.Equals(QueryFilterPrefix, parameterName, StringComparison.Ordinal))
            {
                parameterName = QueryFilterPrefix + "__p";
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

            var parameter = Expression.Parameter(expression.Type, parameterName);

            _evaluatedValues.Add(expression, parameter);

            return parameter;
        }

        private class ContextParameterReplacingExpressionVisitor : ExpressionVisitor
        {
            private readonly Type _contextType;

            public ContextParameterReplacingExpressionVisitor(Type contextType)
            {
                ContextParameterExpression = Expression.Parameter(contextType, "context");
                _contextType = contextType;
            }

            public ParameterExpression ContextParameterExpression { get; }

            public override Expression Visit(Expression expression)
            {
                return expression?.Type.GetTypeInfo().IsAssignableFrom(_contextType) == true
                    ? ContextParameterExpression
                    : base.Visit(expression);
            }
        }

        private object GetValue(Expression expression, out string parameterName)
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
                    if (newExpression.Type is IQueryable)
                    {
                        return newExpression;
                    }

                    parameterName = QueryFilterPrefix
                                    + (expression.RemoveConvert() is MemberExpression memberExpression
                                        ? ("__" + memberExpression.Member.Name)
                                        : "");

                    return Expression.Lambda(
                        newExpression,
                        _contextParameterReplacingExpressionVisitor.ContextParameterExpression);
                }
            }

            switch (expression)
            {
                case MemberExpression memberExpression:
                    var instanceValue = GetValue(memberExpression.Expression, out parameterName);
                    try
                    {
                        switch (memberExpression.Member)
                        {
                            case FieldInfo fieldInfo:
                                parameterName = (parameterName != null ? parameterName + "_" : "") + fieldInfo.Name;
                                return fieldInfo.GetValue(instanceValue);

                            case PropertyInfo propertyInfo:
                                parameterName = (parameterName != null ? parameterName + "_" : "") + propertyInfo.Name;
                                return propertyInfo.GetValue(instanceValue);
                        }
                    }
                    catch
                    {
                        // Try again when we compile the delegate
                    }
                    break;

                case ConstantExpression constantExpression:
                    return constantExpression.Value;

                case MethodCallExpression methodCallExpression:
                    parameterName = methodCallExpression.Method.Name;
                    break;

                case UnaryExpression unaryExpression
                when (unaryExpression.NodeType == ExpressionType.Convert
                      || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                    && (unaryExpression.Type.UnwrapNullableType() == unaryExpression.Operand.Type):
                    return GetValue(unaryExpression.Operand, out parameterName);
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

        private class EvaluatableExpressionFindingExpressionVisitor : ExpressionVisitor
        {
            private readonly IEvaluatableExpressionFilter _evaluatableExpressionFilter;
            private readonly ISet<ParameterExpression> _allowedParameters = new HashSet<ParameterExpression>();

            private bool _evaluatable;
            private bool _containsClosure;
            private bool _inLambda;
            private IDictionary<Expression, bool> _evaluatableExpressions;

            public EvaluatableExpressionFindingExpressionVisitor(IEvaluatableExpressionFilter evaluatableExpressionFilter)
            {
                _evaluatableExpressionFilter = evaluatableExpressionFilter;
            }

            public IDictionary<Expression, bool> Find(Expression expression)
            {
                _evaluatable = true;
                _containsClosure = false;
                _inLambda = false;
                _evaluatableExpressions = new Dictionary<Expression, bool>();
                _allowedParameters.Clear();

                Visit(expression);

                return _evaluatableExpressions;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression == null)
                {
                    return base.Visit(expression);
                }

                var parentEvaluatable = _evaluatable;
                var parentContainsClosure = _containsClosure;

                _evaluatable = IsEvalutableNodeType(expression)
                    // Extension point to disable funcletization
                    && _evaluatableExpressionFilter.IsEvaluatableExpression(expression);
                _containsClosure = false;

                base.Visit(expression);

                if (_evaluatable)
                {
                    _evaluatableExpressions[expression] = _containsClosure;
                }

                _evaluatable = parentEvaluatable && _evaluatable;
                _containsClosure = parentContainsClosure || _containsClosure;

                return expression;
            }

            protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
            {
                var oldInLambda = _inLambda;
                _inLambda = true;

                // Note: Don't skip visiting parameter here.
                // SelectMany does not use parameter in lambda but we should still block it from evaluating
                base.VisitLambda(lambdaExpression);

                _inLambda = oldInLambda;
                return lambdaExpression;
            }

            protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
            {
                Visit(memberInitExpression.Bindings, VisitMemberBinding);

                // Cannot make parameter for NewExpression if Bindings cannot be evaluated
                if (_evaluatable)
                {
                    Visit(memberInitExpression.NewExpression);
                }

                return memberInitExpression;
            }

            protected override Expression VisitListInit(ListInitExpression listInitExpression)
            {
                Visit(listInitExpression.Initializers, VisitElementInit);

                // Cannot make parameter for NewExpression if Initializers cannot be evaluated
                if (_evaluatable)
                {
                    Visit(listInitExpression.NewExpression);
                }

                return listInitExpression;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                Visit(methodCallExpression.Object);
                var parameterInfos = methodCallExpression.Method.GetParameters();
                for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
                {
                    if (i == 1
                        && _evaluatableExpressions.ContainsKey(methodCallExpression.Arguments[0])
                        && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                        && methodCallExpression.Method.Name == nameof(Enumerable.Select)
                        && methodCallExpression.Arguments[1] is LambdaExpression lambdaExpression)
                    {
                        // Allow evaluation Enumerable.Select operation
                        foreach (var parameter in lambdaExpression.Parameters)
                        {
                            _allowedParameters.Add(parameter);
                        }
                    }

                    Visit(methodCallExpression.Arguments[i]);

                    if (_evaluatableExpressions.ContainsKey(methodCallExpression.Arguments[i]))
                    {
                        if (parameterInfos[i].GetCustomAttribute<NotParameterizedAttribute>() != null
                            || methodCallExpression.Method.IsEFIndexer())
                        {
                            _evaluatableExpressions[methodCallExpression.Arguments[i]] = false;
                        }
                        else if (!_inLambda)
                        {
                            // Force parameterization when not in lambada
                            _evaluatableExpressions[methodCallExpression.Arguments[i]] = true;
                        }
                    }
                }

                return methodCallExpression;
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                if (memberExpression.Expression == null)
                {
                    // Static members which can change value
                    _containsClosure
                        = !(memberExpression.Member is FieldInfo fieldInfo && fieldInfo.IsInitOnly);
                }

                return base.VisitMember(memberExpression);
            }

            protected override Expression VisitParameter(ParameterExpression parameterExpression)
            {
                _evaluatable = _allowedParameters.Contains(parameterExpression);

                return base.VisitParameter(parameterExpression);
            }

            protected override Expression VisitConstant(ConstantExpression constantExpression)
            {
                _evaluatable = !(constantExpression.Value is IDetachableContext)
                                    && !(constantExpression.Value is IQueryable);
#pragma warning disable RCS1096 // Use bitwise operation instead of calling 'HasFlag'.
                _containsClosure = constantExpression.Type.Attributes.HasFlag(TypeAttributes.NestedPrivate) // Closure
                    || constantExpression.Type == typeof(ValueBuffer); // Find method
#pragma warning restore RCS1096 // Use bitwise operation instead of calling 'HasFlag'.

                return base.VisitConstant(constantExpression);
            }

            private static bool IsEvalutableNodeType(Expression expression)
            {
                if (expression.NodeType == ExpressionType.Extension)
                {
                    if (!expression.CanReduce)
                    {
                        return false;
                    }

                    return IsEvalutableNodeType(expression.ReduceAndCheck());
                }

                return true;
            }
        }
    }
}

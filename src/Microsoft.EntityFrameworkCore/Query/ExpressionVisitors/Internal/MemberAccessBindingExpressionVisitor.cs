// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MemberAccessBindingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly QuerySourceMapping _querySourceMapping;
        private readonly EntityQueryModelVisitor _queryModelVisitor;

        private int _depth = 0;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MemberAccessBindingExpressionVisitor(
            [NotNull] QuerySourceMapping querySourceMapping,
            [NotNull] EntityQueryModelVisitor queryModelVisitor)
        {
            _querySourceMapping = querySourceMapping;
            _queryModelVisitor = queryModelVisitor;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression Visit(Expression node)
        {
            _depth++;

            node = base.Visit(node);

            _depth--;

            if (_depth == 0)
            {
                node = new UnboundValueBufferReadVisitor(_queryModelVisitor).Visit(node);
            }

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitNew(NewExpression expression)
        {
            var newArguments = new Expression[expression.Arguments.Count];

            for (var i = 0; i < expression.Arguments.Count; i++)
            {
                var oldArgument = expression.Arguments[i];
                var newArgument = Visit(oldArgument);

                var valueBufferRead = newArgument as ValueBufferReadExpression;

                if (valueBufferRead != null)
                {
                    newArgument
                        = _queryModelVisitor
                            .BindValueBufferReadExpression(valueBufferRead, i);
                }

                newArguments[i] = newArgument;
            }

            return expression.Update(newArguments);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var leftConstantExpression = node.Left.RemoveConvert() as ConstantExpression;
            var isLeftNullConstant = leftConstantExpression != null && leftConstantExpression.Value == null;

            var rightConstantExpression = node.Right.RemoveConvert() as ConstantExpression;
            var isRightNullConstant = rightConstantExpression != null && rightConstantExpression.Value == null;

            if (isLeftNullConstant || isRightNullConstant)
            {
                var nonNullExpression = isLeftNullConstant ? node.Right : node.Left;

                var methodCallExpression = nonNullExpression as MethodCallExpression;
                if (methodCallExpression != null)
                {
                    if (EntityQueryModelVisitor.IsPropertyMethod(methodCallExpression.Method))
                    {
                        var firstArgument = methodCallExpression.Arguments[0];
                        var visitedArgument = Visit(firstArgument);
                        if (visitedArgument.Type == typeof(ValueBuffer))
                        {
                            var propertyAccessExpression = Visit(nonNullExpression);

                            return Expression.MakeBinary(
                                node.NodeType,
                                Expression.Condition(
                                    IsValueBufferEmpty(visitedArgument),
                                    Expression.Constant(null, propertyAccessExpression.Type),
                                    propertyAccessExpression),
                                Expression.Constant(null));
                        }
                    }
                }

                var referencedQuerySource = (nonNullExpression as QuerySourceReferenceExpression)?.ReferencedQuerySource;
                if (referencedQuerySource != null)
                {
                    var entityType = _queryModelVisitor.QueryCompilationContext.Model.FindEntityType(referencedQuerySource.ItemType);
                    if (entityType != null)
                    {
                        var visitedExpression = Visit(nonNullExpression);
                        if (visitedExpression.Type == typeof(ValueBuffer))
                        {
                            if (node.NodeType == ExpressionType.Equal)
                            {
                                return IsValueBufferEmpty(visitedExpression);
                            }
                            else if (node.NodeType == ExpressionType.NotEqual)
                            {
                                return Expression.Not(IsValueBufferEmpty(visitedExpression));
                            }
                        }
                    }
                }
            }

            var newLeft = Visit(node.Left);

            if (newLeft.Type == typeof(ValueBuffer))
            {
                newLeft = _queryModelVisitor.BindReadValueMethod(node.Left.Type, newLeft, 0);
            }

            var newRight = Visit(node.Right);

            if (newRight.Type == typeof(ValueBuffer))
            {
                newRight = _queryModelVisitor.BindReadValueMethod(node.Right.Type, newRight, 0);
            }

            var newConversion = VisitAndConvert(node.Conversion, "VisitBinary");

            var updated = node.Update(newLeft, newConversion, newRight);

            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:

                    var rightValueBufferRead = newRight as ValueBufferReadExpression;
                    if (rightValueBufferRead != null)
                    {
                        updated = Expression.AndAlso(
                            Expression.Not(IsValueBufferEmpty(rightValueBufferRead.ValueBuffer)),
                            updated);
                    }

                    var leftValueBufferRead = newLeft as ValueBufferReadExpression;
                    if (leftValueBufferRead != null)
                    {
                        updated = Expression.AndAlso(
                            Expression.Not(IsValueBufferEmpty(leftValueBufferRead.ValueBuffer)),
                            updated);
                    }

                    break;
            }

            return updated;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            var newOperand = Visit(node.Operand);

            var valueBufferReadExpression = newOperand as ValueBufferReadExpression;
            if (valueBufferReadExpression != null
                && node.Type.IsNullableType()
                && !valueBufferReadExpression.Type.IsNullableType())
            {
                return valueBufferReadExpression.Update(node);
            }

            return node.Update(newOperand);
        }

        private Expression IsValueBufferEmpty(Expression valueBufferExpression) 
            => Expression.MakeMemberAccess(
                valueBufferExpression,
                typeof(ValueBuffer).GetRuntimeProperty(nameof(ValueBuffer.IsEmpty)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            var newExpression
                = _querySourceMapping.ContainsMapping(expression.ReferencedQuerySource)
                    ? _querySourceMapping.GetExpression(expression.ReferencedQuerySource)
                    : expression;
            
            if (newExpression.Type.IsConstructedGenericType)
            {
                var genericTypeDefinition = newExpression.Type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(IOrderedAsyncEnumerable<>))
                {
                    newExpression
                        = Expression.Call(
                            _queryModelVisitor.LinqOperatorProvider.ToOrdered
                                .MakeGenericMethod(newExpression.Type.GenericTypeArguments[0]),
                            newExpression);
                }
                else if (genericTypeDefinition == typeof(IAsyncEnumerable<>))
                {
                    newExpression
                        = Expression.Call(
                            _queryModelVisitor.LinqOperatorProvider.ToEnumerable
                                .MakeGenericMethod(newExpression.Type.GenericTypeArguments[0]),
                            newExpression);
                }
            }

            return newExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            expression.QueryModel.TransformExpressions(Visit);

            return expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression node)
        {
            var nullConditionalExpression = node as NullConditionalExpression;

            if (nullConditionalExpression != null)
            {
                var newCaller = Visit(nullConditionalExpression.Caller);

                if (newCaller != nullConditionalExpression.Caller && newCaller.Type == typeof(ValueBuffer))
                {
                    var convertedAccessOperation 
                        = Expression.Convert(
                            nullConditionalExpression.AccessOperation, 
                            nullConditionalExpression.Type);

                    var newAccessOperation = Visit(convertedAccessOperation);

                    if (newAccessOperation != convertedAccessOperation)
                    {
                        var test = IsValueBufferEmpty(newCaller);
                        var ifTrue = Expression.Default(convertedAccessOperation.Type);
                        var ifFalse = newAccessOperation;

                        return Expression.Condition(test, ifTrue, ifFalse);
                    }
                }
            }

            return base.VisitExtension(node);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = node.Expression.RemoveConvert();

            if (expression != node.Expression
                && !(expression is QuerySourceReferenceExpression))
            {
                expression = node.Expression;
            }

            var newExpression = Visit(expression);

            if (newExpression != expression)
            {
                if (newExpression.Type == typeof(ValueBuffer))
                {
                    var boundExpression = _queryModelVisitor.BindMemberExpression(node, (property, querySource) =>
                    {
                        return new ValueBufferReadExpression(
                            node,
                            newExpression,
                            property,
                            querySource);
                    });

                    return boundExpression ?? (Expression)node;
                }

                var member = node.Member;
                var typeInfo = newExpression.Type.GetTypeInfo();

                if (newExpression.Type.IsGrouping())
                {
                    member = typeInfo.GetDeclaredProperty("Key");
                }

                return Expression.MakeMemberAccess(newExpression, member);
            }

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            MethodCallExpression newExpression = null;

            if (EntityQueryModelVisitor.IsPropertyMethod(methodCallExpression.Method))
            {
                var newArguments
                    = VisitAndConvert(
                        new List<Expression>
                        {
                            methodCallExpression.Arguments[0].RemoveConvert(),
                            methodCallExpression.Arguments[1]
                        }.AsReadOnly(),
                        "VisitMethodCall");

                if (newArguments[0].Type == typeof(ValueBuffer))
                {
                    // Compensate for ValueBuffer being a struct, and hence not compatible with Object method
                    newExpression
                        = Expression.Call(
                            methodCallExpression.Method,
                            Expression.Convert(newArguments[0], typeof(object)),
                            newArguments[1]);

                    return _queryModelVisitor.BindMethodCallExpression(methodCallExpression, (property, querySource) =>
                    {
                        return new ValueBufferReadExpression(
                            methodCallExpression,
                            newArguments[0],
                            property,
                            querySource);
                    }) 
                    ?? (Expression)newExpression;
                }
            }
            else if (methodCallExpression.Method.MethodIsClosedFormOf(_getValueMethodInfo))
            {
                var newArguments
                    = VisitAndConvert(
                        new List<Expression>
                        {
                            methodCallExpression.Arguments[0],
                            methodCallExpression.Arguments[1],
                            methodCallExpression.Arguments[2]
                        }.AsReadOnly(),
                        "VisitMethodCall");

                if (newArguments[1].Type == typeof(ValueBuffer))
                {
                    var querySource
                        = (methodCallExpression.Arguments[1] as QuerySourceReferenceExpression)
                            ?.ReferencedQuerySource;

                    return new ValueBufferReadExpression(
                        methodCallExpression,
                        newArguments[1],
                        (newArguments[2] as ConstantExpression).Value as IProperty,
                        querySource);
                }
            }

            if (newExpression == null)
            {
                newExpression = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);
            }

            var firstArgument = newExpression.Arguments.FirstOrDefault();

            var boundExpression = _queryModelVisitor.BindMethodCallExpression<Expression>(methodCallExpression, (property, querySource) =>
            {
                var propertyType = methodCallExpression.Method.GetGenericArguments()[0];

                var maybeConstantExpression = firstArgument as ConstantExpression;

                if (maybeConstantExpression != null)
                {
                    return Expression.Constant(
                        property.GetGetter().GetClrValue(maybeConstantExpression.Value),
                        propertyType);
                }

                var maybeMethodCallExpression = firstArgument as MethodCallExpression;

                                   if (maybeMethodCallExpression != null
                                       && maybeMethodCallExpression.Method.IsGenericMethod
                                       && maybeMethodCallExpression.Method.GetGenericMethodDefinition()
                                           .Equals(DefaultQueryExpressionVisitor.GetParameterValueMethodInfo)
                                       || (newExpression.Arguments[0].NodeType == ExpressionType.Parameter
                                           && !property.IsShadowProperty))
                                   {
                                       // The target is a parameter, try and get the value from it directly.
                                       return Expression.Call(
                                           _getValueFromEntityMethodInfo
                                               .MakeGenericMethod(propertyType),
                                           Expression.Constant(property.GetGetter()),
                                           newExpression.Arguments[0]);
                                   }

                return Expression.Call(
                    _getValueMethodInfo.MakeGenericMethod(propertyType),
                    EntityQueryModelVisitor.QueryContextParameter,
                    firstArgument,
                    Expression.Constant(property));
            });

            return boundExpression ?? newExpression;
        }

        private static readonly MethodInfo _getValueMethodInfo
            = typeof(MemberAccessBindingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetValue));

        [UsedImplicitly]
        private static T GetValue<T>(QueryContext queryContext, object entity, IProperty property)
        {
            if (entity == null)
            {
                return default(T);
            }

            return (T)queryContext.QueryBuffer.GetPropertyValue(entity, property);
        }

        private static readonly MethodInfo _getValueFromEntityMethodInfo
            = typeof(MemberAccessBindingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetValueFromEntity));

        [UsedImplicitly]
        private static T GetValueFromEntity<T>(IClrPropertyGetter clrPropertyGetter, object entity)
        {
            if (entity == null)
            {
                return default(T);
            }

            return (T)clrPropertyGetter.GetClrValue(entity);
        }

        private class UnboundValueBufferReadVisitor : ExpressionVisitor
        {
            private readonly EntityQueryModelVisitor _queryModelVisitor;

            public UnboundValueBufferReadVisitor(EntityQueryModelVisitor queryModelVisitor)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            protected override Expression VisitExtension(Expression node)
            {
                var valueBufferRead = node as ValueBufferReadExpression;

                if (valueBufferRead != null)
                {
                    return _queryModelVisitor.BindValueBufferReadExpression(valueBufferRead, 0);
                }

                return base.VisitExtension(node);
            }
        }
    }
}

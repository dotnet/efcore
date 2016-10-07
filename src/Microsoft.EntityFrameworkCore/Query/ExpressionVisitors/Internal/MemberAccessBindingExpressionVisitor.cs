// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
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
        private readonly bool _inProjection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MemberAccessBindingExpressionVisitor(
            [NotNull] QuerySourceMapping querySourceMapping,
            [NotNull] EntityQueryModelVisitor queryModelVisitor,
            bool inProjection)
        {
            _querySourceMapping = querySourceMapping;
            _queryModelVisitor = queryModelVisitor;
            _inProjection = inProjection;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitNew(NewExpression expression)
        {
            var newArguments = Visit(expression.Arguments).ToList();

            for (var i = 0; i < newArguments.Count; i++)
            {
                if (newArguments[i].Type == typeof(ValueBuffer))
                {
                    newArguments[i]
                        = _queryModelVisitor
                            .BindReadValueMethod(expression.Arguments[i].Type, newArguments[i], 0);
                }
            }

            return expression.Update(newArguments);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var newLeft = Visit(node.Left);
            var newRight = Visit(node.Right);

            if (newLeft.Type == typeof(ValueBuffer))
            {
                if (node.NodeType == ExpressionType.Equal
                    || node.NodeType == ExpressionType.NotEqual)
                {
                    var rightConstantExpression = newRight as ConstantExpression;
                    if (rightConstantExpression != null
                        && rightConstantExpression.Value == null)
                    {
                        return ValueBufferNullCheck(newLeft, node.NodeType == ExpressionType.Equal);
                    }
                }

                newLeft = _queryModelVisitor.BindReadValueMethod(node.Left.Type, newLeft, 0);
            }

            if (newRight.Type == typeof(ValueBuffer))
            {
                if (node.NodeType == ExpressionType.Equal
                    || node.NodeType == ExpressionType.NotEqual)
                {
                    var leftConstantExpression = newLeft as ConstantExpression;
                    if (leftConstantExpression != null
                        && leftConstantExpression.Value == null)
                    {
                        return ValueBufferNullCheck(newRight, node.NodeType == ExpressionType.Equal);
                    }
                }

                newRight = _queryModelVisitor.BindReadValueMethod(node.Right.Type, newRight, 0);
            }

            var newConversion = VisitAndConvert(node.Conversion, "VisitBinary");

            return node.Update(newLeft, newConversion, newRight);
        }

        private static Expression ValueBufferNullCheck(Expression valueBufferExpression, bool equality)
        {
            var equalsMethod = typeof(ValueBuffer).GetRuntimeMethod(nameof(ValueBuffer.Equals), new[] { typeof(object) });
            var equalsExpression = Expression.Call(
                valueBufferExpression,
                equalsMethod,
                Expression.Constant(null, typeof(object)));

            return equality ? (Expression)equalsExpression : Expression.Not(equalsExpression);
        }

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

            if (_inProjection
                && newExpression.Type.IsConstructedGenericType)
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

                if (newCaller != nullConditionalExpression.Caller
                    && newCaller.Type == typeof(ValueBuffer))
                {
                    var newAccessOperation = Visit(nullConditionalExpression.AccessOperation);

                    if (newAccessOperation != nullConditionalExpression.AccessOperation)
                    {
                        return newAccessOperation;
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
                    return _queryModelVisitor
                               .BindMemberToValueBuffer(node, newExpression)
                           ?? node;
                }

                var member = node.Member;
                var typeInfo = newExpression.Type.GetTypeInfo();

                if (typeInfo.IsGenericType
                    && (typeInfo.GetGenericTypeDefinition() == typeof(IGrouping<,>)
                        || typeInfo.GetGenericTypeDefinition() == typeof(IAsyncGrouping<,>)))
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
            Expression firstArgument = null;

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
                    firstArgument = newArguments[0];

                    // Compensate for ValueBuffer being a struct, and hence not compatible with Object method
                    newExpression
                        = Expression.Call(
                            methodCallExpression.Method,
                            Expression.Convert(newArguments[0], typeof(object)),
                            newArguments[1]);
                }
            }

            if (newExpression == null)
            {
                newExpression
                    = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);
            }

            firstArgument = firstArgument ?? newExpression.Arguments.FirstOrDefault();

            if (newExpression != methodCallExpression
                && firstArgument?.Type == typeof(ValueBuffer))
            {
                return
                    _queryModelVisitor
                        .BindMethodCallToValueBuffer(methodCallExpression, firstArgument)
                    ?? newExpression;
            }

            return _queryModelVisitor
                       .BindMethodCallExpression<Expression>(
                           methodCallExpression,
                           (property, _) =>
                               {
                                   var propertyType = newExpression.Method.GetGenericArguments()[0];

                                   var maybeConstantExpression = newExpression.Arguments[0] as ConstantExpression;

                                   if (maybeConstantExpression != null)
                                   {
                                       return Expression.Constant(
                                           property.GetGetter().GetClrValue(maybeConstantExpression.Value),
                                           propertyType);
                                   }

                                   var maybeMethodCallExpression = newExpression.Arguments[0] as MethodCallExpression;

                                   if ((maybeMethodCallExpression != null
                                        && maybeMethodCallExpression.Method.IsGenericMethod
                                        && maybeMethodCallExpression.Method.GetGenericMethodDefinition()
                                        == DefaultQueryExpressionVisitor.GetParameterValueMethodInfo)
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
                                       newExpression.Arguments[0],
                                       Expression.Constant(property));
                               })
                   ?? newExpression;
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
    }
}

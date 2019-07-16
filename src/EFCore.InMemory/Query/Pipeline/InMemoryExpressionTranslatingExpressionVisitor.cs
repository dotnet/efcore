// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryExpressionTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;

        public InMemoryExpressionTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        {
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
        }

        public Expression Translate(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);

            if (innerExpression is EntityProjectionExpression
                || (innerExpression is UnaryExpression innerUnaryExpression
                    && innerUnaryExpression.NodeType == ExpressionType.Convert
                    && innerUnaryExpression.Operand is EntityProjectionExpression))
            {
                return BindProperty(innerExpression, memberExpression.Member.GetSimpleMemberName(), memberExpression.Type);
            }

            return memberExpression.Update(innerExpression);
        }

        private Expression BindProperty(Expression source, string propertyName, Type type)
        {
            Type convertedType = null;
            if (source is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                source = unaryExpression.Operand;
                if (unaryExpression.Type != typeof(object))
                {
                    convertedType = unaryExpression.Type;
                }
            }

            if (source is EntityProjectionExpression entityProjection)
            {
                var entityType = entityProjection.EntityType;
                if (convertedType != null)
                {
                    entityType = entityType.RootType().GetDerivedTypesInclusive()
                        .FirstOrDefault(et => et.ClrType == convertedType);

                    if (entityType == null)
                    {
                        return null;
                    }
                }

                var result = BindProperty(entityProjection, entityType.FindProperty(propertyName));
                return result.Type == type
                    ? result
                    : Expression.Convert(result, type);
            }

            throw new InvalidOperationException();
        }

        private Expression BindProperty(EntityProjectionExpression entityProjectionExpression, IProperty property)
        {
            return entityProjectionExpression.BindProperty(property);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            // EF.Property case
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                return BindProperty(Visit(source), propertyName, methodCallExpression.Type);
            }

            // Subquery case
            var subqueryTranslation = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
            if (subqueryTranslation != null)
            {
                var subquery = (InMemoryQueryExpression)subqueryTranslation.QueryExpression;
                if (subqueryTranslation.ResultType == ResultType.Enumerable)
                {
                    return null;
                }

                subquery.ApplyServerProjection();
                if (subquery.Projection.Count != 1)
                {
                    return null;
                }

                Expression result;

                // Unwrap ResultEnumerable
                var selectMethod = (MethodCallExpression)subquery.ServerQueryExpression;
                var resultEnumerable = (NewExpression)selectMethod.Arguments[0];
                var resultFunc = ((LambdaExpression)resultEnumerable.Arguments[0]).Body;
                // New ValueBuffer construct
                if (resultFunc is NewExpression newValueBufferExpression)
                {
                    var innerExpression = ((NewArrayExpression)newValueBufferExpression.Arguments[0]).Expressions[0];
                    if (innerExpression is UnaryExpression unaryExpression
                        && innerExpression.NodeType == ExpressionType.Convert
                        && innerExpression.Type == typeof(object))
                    {
                        result = unaryExpression.Operand;
                    }
                    else
                    {
                        result = innerExpression;
                    }

                    return result.Type == methodCallExpression.Type
                        ? result
                        : Expression.Convert(result, methodCallExpression.Type);
                }
                else
                {
                    var selector = (LambdaExpression)selectMethod.Arguments[1];
                    var readValueExpression = ((NewArrayExpression)((NewExpression)selector.Body).Arguments[0]).Expressions[0];
                    if (readValueExpression is UnaryExpression unaryExpression2
                        && unaryExpression2.NodeType == ExpressionType.Convert
                        && unaryExpression2.Type == typeof(object))
                    {
                        readValueExpression = unaryExpression2.Operand;
                    }

                    var valueBufferVariable = Expression.Variable(typeof(ValueBuffer));
                    var replacedReadExpression = ReplacingExpressionVisitor.Replace(
                        selector.Parameters[0],
                        valueBufferVariable,
                        readValueExpression);

                    replacedReadExpression = replacedReadExpression.Type == methodCallExpression.Type
                        ? replacedReadExpression
                        : Expression.Convert(replacedReadExpression, methodCallExpression.Type);

                    return Expression.Block(
                        variables: new[] { valueBufferVariable },
                        Expression.Assign(valueBufferVariable, resultFunc),
                        Expression.Condition(
                            Expression.MakeMemberAccess(valueBufferVariable, _valueBufferIsEmpty),
                            Expression.Default(methodCallExpression.Type),
                            replacedReadExpression));
                }
            }

            // MethodCall translators
            var @object = Visit(methodCallExpression.Object);
            if (TranslationFailed(methodCallExpression.Object, @object))
            {
                return null;
            }

            var arguments = new Expression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = Visit(methodCallExpression.Arguments[i]);
                if (TranslationFailed(methodCallExpression.Arguments[i], argument))
                {
                    return null;
                }
                arguments[i] = argument;
            }

            return methodCallExpression.Update(@object, arguments);
        }

        private static readonly MemberInfo _valueBufferIsEmpty = typeof(ValueBuffer).GetMember(nameof(ValueBuffer.IsEmpty))[0];

        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            if (typeBinaryExpression.NodeType == ExpressionType.TypeIs
                && Visit(typeBinaryExpression.Expression) is EntityProjectionExpression entityProjectionExpression)
            {
                var entityType = entityProjectionExpression.EntityType;
                if (entityType.GetAllBaseTypesInclusive().Any(et => et.ClrType == typeBinaryExpression.TypeOperand))
                {
                    return Expression.Constant(true);
                }

                //var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == typeBinaryExpression.TypeOperand);
                //if (derivedType != null)
                //{
                //    var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();
                //    var discriminatorColumn = BindProperty(entityProjectionExpression, entityType.GetDiscriminatorProperty());

                //    return concreteEntityTypes.Count == 1
                //        ? _sqlExpressionFactory.Equal(discriminatorColumn,
                //            _sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                //        : (Expression)_sqlExpressionFactory.In(discriminatorColumn,
                //            _sqlExpressionFactory.Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()),
                //            negated: false);
                //}

                //return _sqlExpressionFactory.Constant(false);
            }

            return null;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case EntityProjectionExpression _:
                    return extensionExpression;

                case EntityShaperExpression entityShaperExpression:
                    return Visit(entityShaperExpression.ValueBufferExpression);

                case ProjectionBindingExpression projectionBindingExpression:
                    return ((InMemoryQueryExpression)projectionBindingExpression.QueryExpression)
                        .GetMappedProjection(projectionBindingExpression.ProjectionMember);

                case NullConditionalExpression nullConditionalExpression:
                    {
                        var translation = Visit(nullConditionalExpression.AccessOperation);

                        return translation.Type == nullConditionalExpression.Type
                            ? translation
                            : Expression.Convert(translation, nullConditionalExpression.Type);
                    }

                case CorrelationPredicateExpression correlationPredicateExpression:
                    return Visit(correlationPredicateExpression.EqualExpression);

                default:
                    throw new InvalidOperationException();
            }
        }

        protected override Expression VisitListInit(ListInitExpression node) => null;

        protected override Expression VisitInvocation(InvocationExpression node) => null;

        protected override Expression VisitLambda<T>(Expression<T> node) => null;

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (parameterExpression.Name.StartsWith(CompiledQueryCache.CompiledQueryParameterPrefix))
            {
                return Expression.Call(
                    _getParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                    QueryCompilationContext.QueryContextParameter,
                    Expression.Constant(parameterExpression.Name));
            }

            throw new InvalidOperationException();
        }

        private static readonly MethodInfo _getParameterValueMethodInfo
            = typeof(InMemoryExpressionTranslatingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue));

#pragma warning disable IDE0052 // Remove unread private members
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
#pragma warning restore IDE0052 // Remove unread private members
            => (T)queryContext.ParameterValues[parameterName];

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var result = base.VisitUnary(unaryExpression);
            if (result is UnaryExpression outerUnary
                && outerUnary.NodeType == ExpressionType.Convert
                && outerUnary.Operand is UnaryExpression innerUnary
                && innerUnary.NodeType == ExpressionType.Convert)
            {
                var innerMostType = innerUnary.Operand.Type;
                var intermediateType = innerUnary.Type;
                var outerMostType = outerUnary.Type;

                if (outerMostType == innerMostType
                    && intermediateType == innerMostType.UnwrapNullableType())
                {
                    result = innerUnary.Operand;
                }
                else if (outerMostType == typeof(object)
                    && intermediateType == innerMostType.UnwrapNullableType())
                {
                    result = Expression.Convert(innerUnary.Operand, typeof(object));
                }
            }

            return result;
        }

        [DebuggerStepThrough]
        private bool TranslationFailed(Expression original, Expression translation)
            => original != null && translation is EntityProjectionExpression;
    }

}

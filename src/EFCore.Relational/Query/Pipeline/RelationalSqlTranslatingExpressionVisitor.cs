// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalSqlTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly IModel _model;
        private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IMemberTranslatorProvider _memberTranslatorProvider;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;
        private readonly SqlTypeMappingVerifyingExpressionVisitor _sqlVerifyingExpressionVisitor;

        public RelationalSqlTranslatingExpressionVisitor(
            IModel model,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _model = model;
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _sqlExpressionFactory = sqlExpressionFactory;
            _memberTranslatorProvider = memberTranslatorProvider;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
            _sqlVerifyingExpressionVisitor = new SqlTypeMappingVerifyingExpressionVisitor();
        }

        public virtual SqlExpression Translate(Expression expression)
        {
            var result = Visit(expression);

            if (result is SqlExpression translation)
            {
                if (translation is SqlUnaryExpression sqlUnaryExpression
                    && sqlUnaryExpression.OperatorType == ExpressionType.Convert
                    && sqlUnaryExpression.Type == typeof(object))
                {
                    translation = sqlUnaryExpression.Operand;
                }

                translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(translation);

                if (translation is SqlConstantExpression
                    && translation.TypeMapping == null)
                {
                    // Non-mappable constant
                    return null;
                }

                _sqlVerifyingExpressionVisitor.Visit(translation);

                return translation;
            }

            return null;
        }

        public virtual SqlExpression TranslateAverage(Expression expression)
        {
            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = Translate(expression);
            }

            var inputType = sqlExpression.Type.UnwrapNullableType();
            if (inputType == typeof(int)
                || inputType == typeof(long))
            {
                sqlExpression = _sqlExpressionFactory.ApplyDefaultTypeMapping(
                    _sqlExpressionFactory.Convert(sqlExpression, typeof(double)));
            }

            return inputType == typeof(float)
                ? _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.Function(
                            "AVG", new[] { sqlExpression }, typeof(double), null),
                        sqlExpression.Type,
                        sqlExpression.TypeMapping)
                : (SqlExpression)_sqlExpressionFactory.Function(
                    "AVG", new[] { sqlExpression }, sqlExpression.Type, sqlExpression.TypeMapping);
        }

        public virtual SqlExpression TranslateCount(Expression expression = null)
        {
            // TODO: Translate Count with predicate for GroupBy
            return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function("COUNT", new[] { _sqlExpressionFactory.Fragment("*") }, typeof(int)));
        }

        public virtual SqlExpression TranslateLongCount(Expression expression = null)
        {
            // TODO: Translate Count with predicate for GroupBy
            return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function("COUNT", new[] { _sqlExpressionFactory.Fragment("*") }, typeof(long)));
        }

        public virtual SqlExpression TranslateMax(Expression expression)
        {
            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = Translate(expression);
            }

            return _sqlExpressionFactory.Function("MAX", new[] { sqlExpression }, sqlExpression.Type, sqlExpression.TypeMapping);
        }

        public virtual SqlExpression TranslateMin(Expression expression)
        {
            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = Translate(expression);
            }

            return _sqlExpressionFactory.Function("MIN", new[] { sqlExpression }, sqlExpression.Type, sqlExpression.TypeMapping);
        }

        public virtual SqlExpression TranslateSum(Expression expression)
        {
            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = Translate(expression);
            }

            var inputType = sqlExpression.Type.UnwrapNullableType();

            return inputType == typeof(float)
                ? _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.Function("SUM", new[] { sqlExpression }, typeof(double)),
                        inputType,
                        sqlExpression.TypeMapping)
                : (SqlExpression)_sqlExpressionFactory.Function(
                    "SUM", new[] { sqlExpression }, inputType, sqlExpression.TypeMapping);
        }

        private class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression node)
            {
                if (node is SqlExpression sqlExpression
                    && !(node is SqlFragmentExpression))
                {
                    if (sqlExpression.TypeMapping == null)
                    {
                        throw new InvalidOperationException("Null TypeMapping in Sql Tree");
                    }
                }

                return base.VisitExtension(node);
            }
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);

            if ((innerExpression is EntityProjectionExpression
                || (innerExpression is UnaryExpression innerUnaryExpression
                    && innerUnaryExpression.NodeType == ExpressionType.Convert
                    && innerUnaryExpression.Operand is EntityProjectionExpression))
                && TryBindMember(innerExpression, MemberIdentity.Create(memberExpression.Member), out var result))
            {
                return result;
            }

            return TranslationFailed(memberExpression.Expression, innerExpression)
                ? null
                : _memberTranslatorProvider.Translate((SqlExpression)innerExpression, memberExpression.Member, memberExpression.Type);
        }

        private bool TryBindMember(Expression source, MemberIdentity member, out Expression expression)
        {
            expression = null;
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

            if (source is EntityProjectionExpression entityProjectionExpression)
            {
                var entityType = entityProjectionExpression.EntityType;
                if (convertedType != null)
                {
                    entityType = entityType.RootType().GetDerivedTypesInclusive()
                        .FirstOrDefault(et => et.ClrType == convertedType);

                    if (entityType == null)
                    {
                        return false;
                    }
                }

                var property = member.MemberInfo != null
                    ? entityType.FindProperty(member.MemberInfo)
                    : entityType.FindProperty(member.Name);
                if (property != null)
                {
                    expression = entityProjectionExpression.BindProperty(property);
                    return true;
                }
            }

            return false;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            if (typeBinaryExpression.NodeType == ExpressionType.TypeIs
                && Visit(typeBinaryExpression.Expression) is EntityProjectionExpression entityProjectionExpression)
            {
                var entityType = entityProjectionExpression.EntityType;
                if (entityType.GetAllBaseTypesInclusive().Any(et => et.ClrType == typeBinaryExpression.TypeOperand))
                {
                    return _sqlExpressionFactory.Constant(true);
                }

                var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == typeBinaryExpression.TypeOperand);
                if (derivedType != null)
                {
                    var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();
                    var discriminatorColumn = entityProjectionExpression.BindProperty(entityType.GetDiscriminatorProperty());

                    return concreteEntityTypes.Count == 1
                        ? _sqlExpressionFactory.Equal(discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                        : (Expression)_sqlExpressionFactory.In(discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()),
                            negated: false);
                }

                return _sqlExpressionFactory.Constant(false);
            }

            return null;
        }

        private Expression GetSelector(MethodCallExpression methodCallExpression, GroupByShaperExpression groupByShaperExpression)
        {
            if (methodCallExpression.Arguments.Count == 1)
            {
                return groupByShaperExpression.ElementSelector;
            }

            if (methodCallExpression.Arguments.Count == 2)
            {
                var selectorLambda = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                return ReplacingExpressionVisitor.Replace(
                    selectorLambda.Parameters[0],
                    groupByShaperExpression.ElementSelector,
                    selectorLambda.Body);
            }

            throw new InvalidOperationException();
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            // EF.Property case
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                if (TryBindMember(Visit(source), MemberIdentity.Create(propertyName), out var result))
                {
                    return result;
                }

                throw new InvalidOperationException("EF.Property called with wrong property name.");
            }

            // GroupBy Aggregate case
            if (methodCallExpression.Object == null
                && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                && methodCallExpression.Arguments.Count > 0
                && methodCallExpression.Arguments[0] is GroupByShaperExpression groupByShaperExpression)
            {
                return methodCallExpression.Method.Name switch
                {
                    nameof(Enumerable.Average) => TranslateAverage(GetSelector(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Count) => TranslateCount(),
                    nameof(Enumerable.LongCount) => TranslateLongCount(),
                    nameof(Enumerable.Max) => TranslateMax(GetSelector(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Min) => TranslateMin(GetSelector(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Sum) => TranslateSum(GetSelector(methodCallExpression, groupByShaperExpression)),
                    _ => throw new InvalidOperationException("Unknown aggregate operator encountered.")
                };
            }

            // Subquery case
            var subqueryTranslation = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
            if (subqueryTranslation != null)
            {
                if (subqueryTranslation.ResultType == ResultType.Enumerable)
                {
                    return null;
                }

                var subquery = (SelectExpression)subqueryTranslation.QueryExpression;
                subquery.ApplyProjection();

                if (subquery.Projection.Count != 1)
                {
                    return null;
                }

                if ((methodCallExpression.Method.Name == nameof(Queryable.Any)
                    || methodCallExpression.Method.Name == nameof(Queryable.All)
                    || methodCallExpression.Method.Name == nameof(Queryable.Contains))
                    && subquery.Tables.Count == 0)
                {
                    return subquery.Projection[0].Expression;
                }

                return new SubSelectExpression(subquery);
            }

            // MethodCall translators
            var @object = Visit(methodCallExpression.Object);
            if (TranslationFailed(methodCallExpression.Object, @object))
            {
                return null;
            }

            var arguments = new SqlExpression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = Visit(methodCallExpression.Arguments[i]);
                if (TranslationFailed(methodCallExpression.Arguments[i], argument))
                {
                    return null;
                }
                arguments[i] = (SqlExpression)argument;
            }

            return _methodCallTranslatorProvider.Translate(_model, (SqlExpression)@object, methodCallExpression.Method, arguments);
        }

        private static Expression TryRemoveImplicitConvert(Expression expression)
        {
            if (expression is UnaryExpression unaryExpression)
            {
                if (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                {
                    var innerType = unaryExpression.Operand.Type.UnwrapNullableType();
                    if (innerType.IsEnum)
                    {
                        innerType = Enum.GetUnderlyingType(innerType);
                    }
                    var convertedType = unaryExpression.Type.UnwrapNullableType();

                    if (innerType == convertedType
                        || (convertedType == typeof(int)
                            && (innerType == typeof(byte)
                                || innerType == typeof(sbyte)
                                || innerType == typeof(char)
                                || innerType == typeof(short)
                                || innerType == typeof(ushort))))
                    {
                        return TryRemoveImplicitConvert(unaryExpression.Operand);
                    }
                }
            }

            return expression;
        }

        private Expression ConvertAnonymousObjectEqualityComparison(BinaryExpression binaryExpression)
        {
            static Expression removeObjectConvert(Expression expression)
            {
                if (expression is UnaryExpression unaryExpression
                    && expression.Type == typeof(object)
                    && expression.NodeType == ExpressionType.Convert)
                {
                    return unaryExpression.Operand;
                }

                return expression;
            }

            var leftExpressions = ((NewArrayExpression)((NewExpression)binaryExpression.Left).Arguments[0]).Expressions;
            var rightExpressions = ((NewArrayExpression)((NewExpression)binaryExpression.Right).Arguments[0]).Expressions;

            return leftExpressions.Zip(
                rightExpressions,
                (l, r) =>
                {
                    l = removeObjectConvert(l);
                    r = removeObjectConvert(r);
                    if (l.Type.IsNullableType())
                    {
                        r = r.Type.IsNullableType() ? r : Expression.Convert(r, l.Type);
                    }
                    else if (r.Type.IsNullableType())
                    {
                        l = l.Type.IsNullableType() ? l : Expression.Convert(l, r.Type);
                    }

                    return Expression.Equal(l, r);
                })
                .Aggregate((a, b) => Expression.AndAlso(a, b));
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.Left.Type == typeof(AnonymousObject)
                && binaryExpression.NodeType == ExpressionType.Equal)
            {
                return Visit(ConvertAnonymousObjectEqualityComparison(binaryExpression));
            }

            var left = TryRemoveImplicitConvert(binaryExpression.Left);
            var right = TryRemoveImplicitConvert(binaryExpression.Right);

            left = Visit(left);
            right = Visit(right);

            if (TranslationFailed(binaryExpression.Left, left)
                || TranslationFailed(binaryExpression.Right, right))
            {
                return null;
            }

            return _sqlExpressionFactory.MakeBinary(
                binaryExpression.NodeType,
                (SqlExpression)left,
                (SqlExpression)right,
                null);
        }

        protected override Expression VisitNew(NewExpression node) => null;

        protected override Expression VisitMemberInit(MemberInitExpression node) => null;

        protected override Expression VisitNewArray(NewArrayExpression node) => null;

        protected override Expression VisitListInit(ListInitExpression node) => null;

        protected override Expression VisitInvocation(InvocationExpression node) => null;

        protected override Expression VisitLambda<T>(Expression<T> node) => null;

        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => new SqlConstantExpression(constantExpression, null);

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
            => new SqlParameterExpression(parameterExpression, null);

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case EntityProjectionExpression _:
                case SqlExpression _:
                    return extensionExpression;

                case NullConditionalExpression nullConditionalExpression:
                    return Visit(nullConditionalExpression.AccessOperation);

                case EntityShaperExpression entityShaperExpression:
                    return Visit(entityShaperExpression.ValueBufferExpression);

                case CorrelationPredicateExpression correlationPredicateExpression:
                    return Visit(correlationPredicateExpression.EqualExpression);

                case ProjectionBindingExpression projectionBindingExpression:
                    var selectExpression = (SelectExpression)projectionBindingExpression.QueryExpression;
                    return selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember);

                default:
                    return null;
            }
        }

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var test = Visit(conditionalExpression.Test);
            var ifTrue = Visit(conditionalExpression.IfTrue);
            var ifFalse = Visit(conditionalExpression.IfFalse);

            if (TranslationFailed(conditionalExpression.Test, test)
                || TranslationFailed(conditionalExpression.IfTrue, ifTrue)
                || TranslationFailed(conditionalExpression.IfFalse, ifFalse))
            {
                return null;
            }

            return _sqlExpressionFactory.Case(
                new[]
                {
                    new CaseWhenClause((SqlExpression)test,(SqlExpression) ifTrue)
                },
                (SqlExpression)ifFalse);
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var operand = Visit(unaryExpression.Operand);

            if (operand is EntityProjectionExpression)
            {
                return unaryExpression.Update(operand);
            }

            if (TranslationFailed(unaryExpression.Operand, operand))
            {
                return null;
            }

            var sqlOperand = (SqlExpression)operand;
            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Not:
                    return _sqlExpressionFactory.Not(sqlOperand);

                case ExpressionType.Negate:
                    return _sqlExpressionFactory.Negate(sqlOperand);

                case ExpressionType.Convert:
                    // Object convert needs to be converted to explicit cast when mismatching types
                    if (operand.Type.IsInterface
                            && unaryExpression.Type.GetInterfaces().Any(e => e == operand.Type)
                        || unaryExpression.Type.UnwrapNullableType() == operand.Type
                        || unaryExpression.Type.UnwrapNullableType() == typeof(Enum))
                    {
                        return sqlOperand;
                    }

                    // Introduce explicit cast only if the target type is mapped else we need to client eval
                    if (unaryExpression.Type == typeof(object)
                        || _sqlExpressionFactory.FindMapping(unaryExpression.Type) != null)
                    {
                        sqlOperand = _sqlExpressionFactory.ApplyDefaultTypeMapping(sqlOperand);

                        return _sqlExpressionFactory.Convert(sqlOperand, unaryExpression.Type);
                    }

                    break;
            }

            return null;
        }

        [DebuggerStepThrough]
        private bool TranslationFailed(Expression original, Expression translation)
            => original != null && !(translation is SqlExpression);
    }
}

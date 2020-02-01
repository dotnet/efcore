// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalSqlTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly IModel _model;
        private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly SqlTypeMappingVerifyingExpressionVisitor _sqlTypeMappingVerifyingExpressionVisitor;

        protected virtual ISqlExpressionFactory SqlExpressionFactory { get; }

        public RelationalSqlTranslatingExpressionVisitor(
            [NotNull] RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] IModel model,
            [NotNull] QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(model, nameof(model));
            Check.NotNull(queryableMethodTranslatingExpressionVisitor, nameof(queryableMethodTranslatingExpressionVisitor));

            Dependencies = dependencies;
            SqlExpressionFactory = dependencies.SqlExpressionFactory;

            _model = model;
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _sqlTypeMappingVerifyingExpressionVisitor = new SqlTypeMappingVerifyingExpressionVisitor();
        }

        protected virtual RelationalSqlTranslatingExpressionVisitorDependencies Dependencies { get; }

        public virtual SqlExpression Translate([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var result = Visit(expression);

            if (result is SqlExpression translation)
            {
                if (translation is SqlUnaryExpression sqlUnaryExpression
                    && sqlUnaryExpression.OperatorType == ExpressionType.Convert
                    && sqlUnaryExpression.Type == typeof(object))
                {
                    translation = sqlUnaryExpression.Operand;
                }

                translation = SqlExpressionFactory.ApplyDefaultTypeMapping(translation);

                if (translation.TypeMapping == null)
                {
                    // The return type is not-mappable hence return null
                    return null;
                }

                _sqlTypeMappingVerifyingExpressionVisitor.Visit(translation);

                return translation;
            }

            return null;
        }

        public virtual SqlExpression TranslateAverage([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = Translate(expression);
            }

            if (sqlExpression == null)
            {
                throw new InvalidOperationException(CoreStrings.TranslationFailed(expression.Print()));
            }

            var inputType = sqlExpression.Type.UnwrapNullableType();
            if (inputType == typeof(int)
                || inputType == typeof(long))
            {
                sqlExpression = SqlExpressionFactory.ApplyDefaultTypeMapping(
                    SqlExpressionFactory.Convert(sqlExpression, typeof(double)));
            }

            return inputType == typeof(float)
                ? SqlExpressionFactory.Convert(
                    SqlExpressionFactory.Function(
                        "AVG",
                        new[] { sqlExpression },
                        nullResultAllowed: true,
                        argumentsPropagateNullability: new[] { false },
                        typeof(double)),
                    sqlExpression.Type,
                    sqlExpression.TypeMapping)
                : (SqlExpression)SqlExpressionFactory.Function(
                    "AVG",
                    new[] { sqlExpression },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { false },
                    sqlExpression.Type,
                    sqlExpression.TypeMapping);
        }

        public virtual SqlExpression TranslateCount([CanBeNull] Expression expression = null)
        {
            if (expression != null)
            {
                // TODO: Translate Count with predicate for GroupBy
                return null;
            }

            return SqlExpressionFactory.ApplyDefaultTypeMapping(
                SqlExpressionFactory.Function(
                    "COUNT",
                    new[] { SqlExpressionFactory.Fragment("*") },
                    nullResultAllowed: false,
                    argumentsPropagateNullability: new[] { false },
                    typeof(int)));
        }

        public virtual SqlExpression TranslateLongCount([CanBeNull] Expression expression = null)
        {
            if (expression != null)
            {
                // TODO: Translate Count with predicate for GroupBy
                return null;
            }

            return SqlExpressionFactory.ApplyDefaultTypeMapping(
                SqlExpressionFactory.Function(
                    "COUNT",
                    new[] { SqlExpressionFactory.Fragment("*") },
                    nullResultAllowed: false,
                    argumentsPropagateNullability: new[] { false },
                    typeof(long)));
        }

        public virtual SqlExpression TranslateMax([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = Translate(expression);
            }

            return sqlExpression != null
                ? SqlExpressionFactory.Function(
                    "MAX",
                    new[] { sqlExpression },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { false },
                    sqlExpression.Type,
                    sqlExpression.TypeMapping)
                : null;
        }

        public virtual SqlExpression TranslateMin([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = Translate(expression);
            }

            return sqlExpression != null
                ? SqlExpressionFactory.Function(
                    "MIN",
                    new[] { sqlExpression },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { false },
                    sqlExpression.Type,
                    sqlExpression.TypeMapping)
                : null;
        }

        public virtual SqlExpression TranslateSum([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = Translate(expression);
            }

            if (sqlExpression == null)
            {
                throw new InvalidOperationException(CoreStrings.TranslationFailed(expression.Print()));
            }

            var inputType = sqlExpression.Type.UnwrapNullableType();

            return inputType == typeof(float)
                ? SqlExpressionFactory.Convert(
                    SqlExpressionFactory.Function(
                        "SUM",
                        new[] { sqlExpression },
                        nullResultAllowed: true,
                        argumentsPropagateNullability: new[] { false },
                        typeof(double)),
                    inputType,
                    sqlExpression.TypeMapping)
                : (SqlExpression)SqlExpressionFactory.Function(
                    "SUM",
                    new[] { sqlExpression },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { false },
                    inputType,
                    sqlExpression.TypeMapping);
        }

        private sealed class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression node)
            {
                Check.NotNull(node, nameof(node));

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
            Check.NotNull(memberExpression, nameof(memberExpression));

            return TryBindMember(memberExpression.Expression, MemberIdentity.Create(memberExpression.Member), out var result)
                ? result
                : TranslationFailed(memberExpression.Expression, base.Visit(memberExpression.Expression), out var sqlInnerExpression)
                    ? null
                    : Dependencies.MemberTranslatorProvider.Translate(sqlInnerExpression, memberExpression.Member, memberExpression.Type);
        }

        private bool TryBindMember(Expression source, MemberIdentity member, out Expression expression)
        {
            source = source.UnwrapTypeConversion(out var convertedType);
            expression = null;
            if (source is EntityShaperExpression entityShaperExpression)
            {
                var entityType = entityShaperExpression.EntityType;
                if (convertedType != null)
                {
                    entityType = entityType.GetRootType().GetDerivedTypesInclusive()
                        .FirstOrDefault(et => et.ClrType == convertedType);
                    if (entityType == null)
                    {
                        return false;
                    }
                }

                var property = member.MemberInfo != null
                    ? entityType.FindProperty(member.MemberInfo)
                    : entityType.FindProperty(member.Name);
                if (property != null
                    && Visit(entityShaperExpression.ValueBufferExpression) is EntityProjectionExpression entityProjectionExpression
                    && (entityProjectionExpression.EntityType.IsAssignableFrom(property.DeclaringEntityType)
                        || property.DeclaringEntityType.IsAssignableFrom(entityProjectionExpression.EntityType)))
                {
                    expression = entityProjectionExpression.BindProperty(property);
                    return true;
                }
            }

            return false;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            Check.NotNull(typeBinaryExpression, nameof(typeBinaryExpression));

            if (typeBinaryExpression.NodeType == ExpressionType.TypeIs
                && Visit(typeBinaryExpression.Expression) is EntityProjectionExpression entityProjectionExpression)
            {
                var entityType = entityProjectionExpression.EntityType;
                if (entityType.GetAllBaseTypesInclusive().Any(et => et.ClrType == typeBinaryExpression.TypeOperand))
                {
                    return SqlExpressionFactory.Constant(true);
                }

                var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == typeBinaryExpression.TypeOperand);
                if (derivedType != null)
                {
                    var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();
                    var discriminatorColumn = entityProjectionExpression.BindProperty(entityType.GetDiscriminatorProperty());

                    return concreteEntityTypes.Count == 1
                        ? SqlExpressionFactory.Equal(
                            discriminatorColumn,
                            SqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                        : (Expression)SqlExpressionFactory.In(
                            discriminatorColumn,
                            SqlExpressionFactory.Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()),
                            negated: false);
                }

                return SqlExpressionFactory.Constant(false);
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

            throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
        }

        private Expression GetPredicate(MethodCallExpression methodCallExpression, GroupByShaperExpression groupByShaperExpression)
        {
            if (methodCallExpression.Arguments.Count == 1)
            {
                return null;
            }

            if (methodCallExpression.Arguments.Count == 2)
            {
                var selectorLambda = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                return ReplacingExpressionVisitor.Replace(
                    selectorLambda.Parameters[0],
                    groupByShaperExpression.ElementSelector,
                    selectorLambda.Body);
            }

            throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            // EF.Property case
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                if (TryBindMember(source, MemberIdentity.Create(propertyName), out var result))
                {
                    return result;
                }

                throw new InvalidOperationException("EF.Property called with wrong property name.");
            }

            // EF Indexer property
            if (methodCallExpression.TryGetIndexerArguments(_model, out source, out propertyName))
            {
                return TryBindMember(source, MemberIdentity.Create(propertyName), out var result) ? result : null;
            }

            // GroupBy Aggregate case
            if (methodCallExpression.Object == null
                && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                && methodCallExpression.Arguments.Count > 0
                && methodCallExpression.Arguments[0] is GroupByShaperExpression groupByShaperExpression)
            {
                var translatedAggregate = methodCallExpression.Method.Name switch
                {
                    nameof(Enumerable.Average) => TranslateAverage(GetSelector(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Count) => TranslateCount(GetPredicate(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.LongCount) => TranslateLongCount(GetPredicate(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Max) => TranslateMax(GetSelector(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Min) => TranslateMin(GetSelector(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Sum) => TranslateSum(GetSelector(methodCallExpression, groupByShaperExpression)),
                    _ => null
                };

                if (translatedAggregate == null)
                {
                    throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
                }

                return translatedAggregate;
            }

            // Subquery case
            var subqueryTranslation = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
            if (subqueryTranslation != null)
            {
                static bool IsAggregateResultWithCustomShaper(MethodInfo method)
                {
                    if (method.IsGenericMethod)
                    {
                        method = method.GetGenericMethodDefinition();
                    }

                    return QueryableMethods.IsAverageWithoutSelector(method)
                        || QueryableMethods.IsAverageWithSelector(method)
                        || method == QueryableMethods.MaxWithoutSelector
                        || method == QueryableMethods.MaxWithSelector
                        || method == QueryableMethods.MinWithoutSelector
                        || method == QueryableMethods.MinWithSelector
                        || QueryableMethods.IsSumWithoutSelector(method)
                        || QueryableMethods.IsSumWithSelector(method);
                }

                if (subqueryTranslation.ResultCardinality == ResultCardinality.Enumerable)
                {
                    return null;
                }

                var subquery = (SelectExpression)subqueryTranslation.QueryExpression;
                subquery.ApplyProjection();

                if (!(subqueryTranslation.ShaperExpression is ProjectionBindingExpression
                    || IsAggregateResultWithCustomShaper(methodCallExpression.Method)))
                {
                    return null;
                }

#pragma warning disable IDE0046 // Convert to conditional expression
                if (subquery.Tables.Count == 0
#pragma warning restore IDE0046 // Convert to conditional expression
                    && methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() is MethodInfo genericMethod
                    && (genericMethod == QueryableMethods.AnyWithoutPredicate
                        || genericMethod == QueryableMethods.AnyWithPredicate
                        || genericMethod == QueryableMethods.All
                        || genericMethod == QueryableMethods.Contains))
                {
                    return subquery.Projection[0].Expression;
                }

                return new ScalarSubqueryExpression(subquery);
            }

            // MethodCall translators
            if (TranslationFailed(methodCallExpression.Object, Visit(methodCallExpression.Object), out var sqlObject))
            {
                return null;
            }

            var arguments = new SqlExpression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = methodCallExpression.Arguments[i];
                if (TranslationFailed(argument, Visit(argument), out var sqlArgument))
                {
                    return null;
                }

                arguments[i] = sqlArgument;
            }

            return Dependencies.MethodCallTranslatorProvider.Translate(_model, sqlObject, methodCallExpression.Method, arguments);
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
            var leftExpressions = ((NewArrayExpression)((NewExpression)binaryExpression.Left).Arguments[0]).Expressions;
            var rightExpressions = ((NewArrayExpression)((NewExpression)binaryExpression.Right).Arguments[0]).Expressions;

            return leftExpressions.Zip(
                    rightExpressions,
                    (l, r) =>
                    {
                        l = RemoveObjectConvert(l);
                        r = RemoveObjectConvert(r);
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

            static Expression RemoveObjectConvert(Expression expression)
                => expression is UnaryExpression unaryExpression
                    && expression.Type == typeof(object)
                    && expression.NodeType == ExpressionType.Convert
                    ? unaryExpression.Operand
                    : expression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (binaryExpression.Left.Type == typeof(AnonymousObject)
                && binaryExpression.NodeType == ExpressionType.Equal)
            {
                return Visit(ConvertAnonymousObjectEqualityComparison(binaryExpression));
            }

            var left = TryRemoveImplicitConvert(binaryExpression.Left);
            var right = TryRemoveImplicitConvert(binaryExpression.Right);

            return TranslationFailed(binaryExpression.Left, Visit(left), out var sqlLeft)
                || TranslationFailed(binaryExpression.Right, Visit(right), out var sqlRight)
                ? null
                : binaryExpression.NodeType == ExpressionType.Coalesce
                    ? SqlExpressionFactory.Coalesce(sqlLeft, sqlRight)
                    : (Expression)SqlExpressionFactory.MakeBinary(
                        binaryExpression.NodeType,
                        sqlLeft,
                        sqlRight,
                        null);
        }

        private SqlConstantExpression GetConstantOrNull(Expression expression)
        {
            if (CanEvaluate(expression))
            {
                var value = Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile().Invoke();
                return new SqlConstantExpression(Expression.Constant(value, expression.Type), null);
            }

            return null;
        }

        private static bool CanEvaluate(Expression expression)
        {
#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (expression)
#pragma warning restore IDE0066 // Convert switch statement to expression
            {
                case ConstantExpression constantExpression:
                    return true;

                case NewExpression newExpression:
                    return newExpression.Arguments.All(e => CanEvaluate(e));

                case MemberInitExpression memberInitExpression:
                    return CanEvaluate(memberInitExpression.NewExpression)
                        && memberInitExpression.Bindings.All(
                            mb => mb is MemberAssignment memberAssignment && CanEvaluate(memberAssignment.Expression));

                default:
                    return false;
            }
        }

        protected override Expression VisitNew(NewExpression node)
        {
            Check.NotNull(node, nameof(node));

            return GetConstantOrNull(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            Check.NotNull(node, nameof(node));

            return GetConstantOrNull(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            Check.NotNull(constantExpression, nameof(constantExpression));

            return new SqlConstantExpression(constantExpression, null);
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            return new SqlParameterExpression(parameterExpression, null);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            switch (extensionExpression)
            {
                case EntityProjectionExpression _:
                case SqlExpression _:
                    return extensionExpression;

                case EntityShaperExpression entityShaperExpression:
                    return Visit(entityShaperExpression.ValueBufferExpression);

                case ProjectionBindingExpression projectionBindingExpression:
                    return projectionBindingExpression.ProjectionMember != null
                        ? ((SelectExpression)projectionBindingExpression.QueryExpression)
                        .GetMappedProjection(projectionBindingExpression.ProjectionMember)
                        : null;

                default:
                    return null;
            }
        }

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            Check.NotNull(conditionalExpression, nameof(conditionalExpression));

            var test = Visit(conditionalExpression.Test);
            var ifTrue = Visit(conditionalExpression.IfTrue);
            var ifFalse = Visit(conditionalExpression.IfFalse);

            return TranslationFailed(conditionalExpression.Test, test, out var sqlTest)
                || TranslationFailed(conditionalExpression.IfTrue, ifTrue, out var sqlIfTrue)
                || TranslationFailed(conditionalExpression.IfFalse, ifFalse, out var sqlIfFalse)
                ? null
                : SqlExpressionFactory.Case(new[] { new CaseWhenClause(sqlTest, sqlIfTrue) }, sqlIfFalse);
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, nameof(unaryExpression));

            var operand = Visit(unaryExpression.Operand);

            if (TranslationFailed(unaryExpression.Operand, operand, out var sqlOperand))
            {
                return null;
            }

            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Not:
                    return SqlExpressionFactory.Not(sqlOperand);

                case ExpressionType.Negate:
                    return SqlExpressionFactory.Negate(sqlOperand);

                case ExpressionType.Convert:
                case ExpressionType.TypeAs:
                    // Object convert needs to be converted to explicit cast when mismatching types
                    if (operand.Type.IsInterface
                        && unaryExpression.Type.GetInterfaces().Any(e => e == operand.Type)
                        || unaryExpression.Type.UnwrapNullableType() == operand.Type.UnwrapNullableType()
                        || unaryExpression.Type.UnwrapNullableType() == typeof(Enum))
                    {
                        return sqlOperand;
                    }

                    // Introduce explicit cast only if the target type is mapped else we need to client eval
                    if (unaryExpression.Type == typeof(object)
                        || SqlExpressionFactory.FindMapping(unaryExpression.Type) != null)
                    {
                        sqlOperand = SqlExpressionFactory.ApplyDefaultTypeMapping(sqlOperand);

                        return SqlExpressionFactory.Convert(sqlOperand, unaryExpression.Type);
                    }

                    break;
            }

            return null;
        }

        [DebuggerStepThrough]
        private bool TranslationFailed(Expression original, Expression translation, out SqlExpression castTranslation)
        {
            if (original != null
                && !(translation is SqlExpression))
            {
                castTranslation = null;
                return true;
            }

            castTranslation = translation as SqlExpression;
            return false;
        }
    }
}

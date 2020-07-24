// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A class that translates expressions to corresponding SQL representation.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalSqlTranslatingExpressionVisitor : ExpressionVisitor
    {
        private const string RuntimeParameterPrefix = QueryCompilationContext.QueryParameterPrefix + "entity_equality_";

        private static readonly MethodInfo _parameterValueExtractor =
            typeof(RelationalSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor));
        private static readonly MethodInfo _parameterListValueExtractor =
            typeof(RelationalSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterListValueExtractor));

        private static readonly MethodInfo _stringEqualsWithStringComparison
            = typeof(string).GetRuntimeMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) });
        private static readonly MethodInfo _stringEqualsWithStringComparisonStatic
            = typeof(string).GetRuntimeMethod(nameof(string.Equals), new[] { typeof(string), typeof(string), typeof(StringComparison) });
        private static readonly MethodInfo _objectEqualsMethodInfo
                 = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) });

        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly SqlTypeMappingVerifyingExpressionVisitor _sqlTypeMappingVerifyingExpressionVisitor;

        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalSqlTranslatingExpressionVisitor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="queryCompilationContext"> The query compilation context object to use. </param>
        /// <param name="queryableMethodTranslatingExpressionVisitor"> A parent queryable method translating expression visitor to translate subquery. </param>
        public RelationalSqlTranslatingExpressionVisitor(
            [NotNull] RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));
            Check.NotNull(queryableMethodTranslatingExpressionVisitor, nameof(queryableMethodTranslatingExpressionVisitor));

            Dependencies = dependencies;
            _sqlExpressionFactory = dependencies.SqlExpressionFactory;
            _queryCompilationContext = queryCompilationContext;
            _model = queryCompilationContext.Model;
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _sqlTypeMappingVerifyingExpressionVisitor = new SqlTypeMappingVerifyingExpressionVisitor();
        }

        /// <summary>
        ///     Detailed information about errors encountered during translation.
        /// </summary>
        public virtual string TranslationErrorDetails { get; private set; }

        /// <summary>
        ///     Adds detailed information about error encountered during translation.
        /// </summary>
        /// <param name="details"> Detailed information about error encountered during translation. </param>
        protected virtual void AddTranslationErrorDetails([NotNull] string details)
        {
            Check.NotNull(details, nameof(details));

            if (TranslationErrorDetails == null)
            {
                TranslationErrorDetails = details;
            }
            else
            {
                TranslationErrorDetails += Environment.NewLine + details;
            }
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual RelationalSqlTranslatingExpressionVisitorDependencies Dependencies { get; }

        /// <summary>
        ///     Translates an expression to an equivalent SQL representation.
        /// </summary>
        /// <param name="expression"> An expression to translate. </param>
        /// <returns> A SQL translation of the given expression. </returns>
        public virtual SqlExpression Translate([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            TranslationErrorDetails = null;

            return TranslateInternal(expression);
        }

        private SqlExpression TranslateInternal(Expression expression)
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

        /// <summary>
        ///     Translates Average over an expression to an equivalent SQL representation.
        /// </summary>
        /// <param name="expression"> An expression to translate Average over. </param>
        /// <returns> A SQL translation of Average over the given expression. </returns>
        public virtual SqlExpression TranslateAverage([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = TranslateInternal(expression);
            }

            if (sqlExpression == null)
            {
                throw new InvalidOperationException(
                    TranslationErrorDetails == null
                        ? CoreStrings.TranslationFailed(expression.Print())
                        : CoreStrings.TranslationFailedWithDetails(expression.Print(), TranslationErrorDetails));
            }

            var inputType = sqlExpression.Type;
            if (inputType == typeof(int)
                || inputType == typeof(long))
            {
                sqlExpression = _sqlExpressionFactory.ApplyDefaultTypeMapping(
                    _sqlExpressionFactory.Convert(sqlExpression, typeof(double)));
            }

            return inputType == typeof(float)
                ? _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function(
                        "AVG",
                        new[] { sqlExpression },
                        nullable: true,
                        argumentsPropagateNullability: new[] { false },
                        typeof(double)),
                    sqlExpression.Type,
                    sqlExpression.TypeMapping)
                : (SqlExpression)_sqlExpressionFactory.Function(
                    "AVG",
                    new[] { sqlExpression },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false },
                    sqlExpression.Type,
                    sqlExpression.TypeMapping);
        }

        /// <summary>
        ///     Translates Count over an expression to an equivalent SQL representation.
        /// </summary>
        /// <param name="expression"> An expression to translate Count over. </param>
        /// <returns> A SQL translation of Count over the given expression. </returns>
        public virtual SqlExpression TranslateCount([CanBeNull] Expression expression = null)
        {
            if (expression != null)
            {
                // TODO: Translate Count with predicate for GroupBy
                return null;
            }

            return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function(
                    "COUNT",
                    new[] { _sqlExpressionFactory.Fragment("*") },
                    nullable: false,
                    argumentsPropagateNullability: new[] { false },
                    typeof(int)));
        }

        /// <summary>
        ///     Translates LongCount over an expression to an equivalent SQL representation.
        /// </summary>
        /// <param name="expression"> An expression to translate LongCount over. </param>
        /// <returns> A SQL translation of LongCount over the given expression. </returns>
        public virtual SqlExpression TranslateLongCount([CanBeNull] Expression expression = null)
        {
            if (expression != null)
            {
                // TODO: Translate Count with predicate for GroupBy
                return null;
            }

            return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function(
                    "COUNT",
                    new[] { _sqlExpressionFactory.Fragment("*") },
                    nullable: false,
                    argumentsPropagateNullability: new[] { false },
                    typeof(long)));
        }

        /// <summary>
        ///     Translates Max over an expression to an equivalent SQL representation.
        /// </summary>
        /// <param name="expression"> An expression to translate Max over. </param>
        /// <returns> A SQL translation of Max over the given expression. </returns>
        public virtual SqlExpression TranslateMax([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = TranslateInternal(expression);
            }

            return sqlExpression != null
                ? _sqlExpressionFactory.Function(
                    "MAX",
                    new[] { sqlExpression },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false },
                    sqlExpression.Type,
                    sqlExpression.TypeMapping)
                : null;
        }

        /// <summary>
        ///     Translates Min over an expression to an equivalent SQL representation.
        /// </summary>
        /// <param name="expression"> An expression to translate Min over. </param>
        /// <returns> A SQL translation of Min over the given expression. </returns>
        public virtual SqlExpression TranslateMin([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = TranslateInternal(expression);
            }

            return sqlExpression != null
                ? _sqlExpressionFactory.Function(
                    "MIN",
                    new[] { sqlExpression },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false },
                    sqlExpression.Type,
                    sqlExpression.TypeMapping)
                : null;
        }

        /// <summary>
        ///     Translates Sum over an expression to an equivalent SQL representation.
        /// </summary>
        /// <param name="expression"> An expression to translate Sum over. </param>
        /// <returns> A SQL translation of Sum over the given expression. </returns>
        public virtual SqlExpression TranslateSum([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (!(expression is SqlExpression sqlExpression))
            {
                sqlExpression = TranslateInternal(expression);
            }

            if (sqlExpression == null)
            {
                throw new InvalidOperationException(
                    TranslationErrorDetails == null
                        ? CoreStrings.TranslationFailed(expression.Print())
                        : CoreStrings.TranslationFailedWithDetails(expression.Print(), TranslationErrorDetails));
            }

            var inputType = sqlExpression.Type;

            return inputType == typeof(float)
                ? _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function(
                        "SUM",
                        new[] { sqlExpression },
                        nullable: true,
                        argumentsPropagateNullability: new[] { false },
                        typeof(double)),
                    inputType,
                    sqlExpression.TypeMapping)
                : (SqlExpression)_sqlExpressionFactory.Function(
                    "SUM",
                    new[] { sqlExpression },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false },
                    inputType,
                    sqlExpression.TypeMapping);
        }

        /// <inheritdoc />
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (binaryExpression.Left.Type == typeof(object[])
                && binaryExpression.Left is NewArrayExpression
                && binaryExpression.NodeType == ExpressionType.Equal)
            {
                return Visit(ConvertObjectArrayEqualityComparison(binaryExpression.Left, binaryExpression.Right));
            }

            var left = TryRemoveImplicitConvert(binaryExpression.Left);
            var right = TryRemoveImplicitConvert(binaryExpression.Right);

            // Remove convert-to-object nodes if both sides have them, or if the other side is null constant
            var isLeftConvertToObject = TryUnwrapConvertToObject(left, out var leftOperand);
            var isRightConvertToObject = TryUnwrapConvertToObject(right, out var rightOperand);
            if (isLeftConvertToObject && isRightConvertToObject)
            {
                left = leftOperand;
                right = rightOperand;
            }
            else if (isLeftConvertToObject && right.IsNullConstantExpression())
            {
                left = leftOperand;
            }
            else if (isRightConvertToObject && left.IsNullConstantExpression())
            {
                right = rightOperand;
            }

            var visitedLeft = Visit(left);
            var visitedRight = Visit(right);

            if ((binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
                // Visited expression could be null, We need to pass MemberInitExpression
                && TryRewriteEntityEquality(binaryExpression.NodeType, visitedLeft ?? left, visitedRight ?? right, out var result))
            {
                return result;
            }

            var uncheckedNodeTypeVariant = binaryExpression.NodeType switch
            {
                ExpressionType.AddChecked => ExpressionType.Add,
                ExpressionType.SubtractChecked => ExpressionType.Subtract,
                ExpressionType.MultiplyChecked => ExpressionType.Multiply,
                _ => binaryExpression.NodeType
            };

            return TranslationFailed(binaryExpression.Left, visitedLeft, out var sqlLeft)
                || TranslationFailed(binaryExpression.Right, visitedRight, out var sqlRight)
                ? null
                : uncheckedNodeTypeVariant == ExpressionType.Coalesce
                    ? _sqlExpressionFactory.Coalesce(sqlLeft, sqlRight)
                    : (Expression)_sqlExpressionFactory.MakeBinary(
                        uncheckedNodeTypeVariant,
                        sqlLeft,
                        sqlRight,
                        null);

            static bool TryUnwrapConvertToObject(Expression expression, out Expression operand)
            {
                if (expression is UnaryExpression convertExpression
                    && (convertExpression.NodeType == ExpressionType.Convert
                        || convertExpression.NodeType == ExpressionType.ConvertChecked)
                    && expression.Type == typeof(object))
                {
                    operand = convertExpression.Operand;
                    return true;
                }

                operand = null;
                return false;
            }
        }

        /// <inheritdoc />
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
                : _sqlExpressionFactory.Case(new[] { new CaseWhenClause(sqlTest, sqlIfTrue) }, sqlIfFalse);
        }

        /// <inheritdoc />
        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => new SqlConstantExpression(Check.NotNull(constantExpression, nameof(constantExpression)), null);

        /// <inheritdoc />
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            switch (extensionExpression)
            {
                case EntityProjectionExpression _:
                case EntityReferenceExpression _:
                case SqlExpression _:
                    return extensionExpression;

                case EntityShaperExpression entityShaperExpression:
                    return new EntityReferenceExpression(entityShaperExpression);

                case ProjectionBindingExpression projectionBindingExpression:
                    return projectionBindingExpression.ProjectionMember != null
                        ? ((SelectExpression)projectionBindingExpression.QueryExpression)
                            .GetMappedProjection(projectionBindingExpression.ProjectionMember)
                        : null;

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        protected override Expression VisitInvocation(InvocationExpression invocationExpression) => null;
        /// <inheritdoc />
        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression) => null;
        /// <inheritdoc />
        protected override Expression VisitListInit(ListInitExpression listInitExpression) => null;

        /// <inheritdoc />
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            var innerExpression = Visit(memberExpression.Expression);

            return TryBindMember(innerExpression, MemberIdentity.Create(memberExpression.Member))
                ?? (TranslationFailed(memberExpression.Expression, Visit(memberExpression.Expression), out var sqlInnerExpression)
                    ? null
                    : Dependencies.MemberTranslatorProvider.Translate(sqlInnerExpression, memberExpression.Member, memberExpression.Type));
        }

        /// <inheritdoc />
        protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
            => GetConstantOrNull(Check.NotNull(memberInitExpression, nameof(memberInitExpression)));

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            // EF.Property case
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                return TryBindMember(Visit(source), MemberIdentity.Create(propertyName))
                    ?? throw new InvalidOperationException(CoreStrings.QueryUnableToTranslateEFProperty(methodCallExpression.Print()));
            }

            // EF Indexer property
            if (methodCallExpression.TryGetIndexerArguments(_model, out source, out propertyName))
            {
                return TryBindMember(Visit(source), MemberIdentity.Create(propertyName));
            }

            // GroupBy Aggregate case
            if (methodCallExpression.Object == null
                && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                && methodCallExpression.Arguments.Count > 0
                && methodCallExpression.Arguments[0] is GroupByShaperExpression groupByShaperExpression)
            {
                var translatedAggregate = methodCallExpression.Method.Name switch
                {
                    nameof(Enumerable.Average) => TranslateAverage(GetSelectorOnGrouping(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Count) => TranslateCount(GetPredicateOnGrouping(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.LongCount) => TranslateLongCount(GetPredicateOnGrouping(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Max) => TranslateMax(GetSelectorOnGrouping(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Min) => TranslateMin(GetSelectorOnGrouping(methodCallExpression, groupByShaperExpression)),
                    nameof(Enumerable.Sum) => TranslateSum(GetSelectorOnGrouping(methodCallExpression, groupByShaperExpression)),
                    _ => null
                };

                if (translatedAggregate == null)
                {
                    throw new InvalidOperationException(
                        TranslationErrorDetails == null
                            ? CoreStrings.TranslationFailed(methodCallExpression.Print())
                            : CoreStrings.TranslationFailedWithDetails(methodCallExpression.Print(), TranslationErrorDetails));
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

                if (subqueryTranslation.ShaperExpression is EntityShaperExpression entityShaperExpression)
                {
                    return new EntityReferenceExpression(subqueryTranslation);
                }

                if (!(subqueryTranslation.ShaperExpression is ProjectionBindingExpression
                    || (subqueryTranslation.ShaperExpression is UnaryExpression unaryExpression
                        && unaryExpression.NodeType == ExpressionType.Convert
                        && unaryExpression.Type.MakeNullable() == unaryExpression.Operand.Type
                        && unaryExpression.Operand is ProjectionBindingExpression)
                    || IsAggregateResultWithCustomShaper(methodCallExpression.Method)))
                {
                    return null;
                }

                var subquery = (SelectExpression)subqueryTranslation.QueryExpression;
                subquery.ApplyProjection();

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

                SqlExpression scalarSubqueryExpression = new ScalarSubqueryExpression(subquery);

                if (subqueryTranslation.ResultCardinality == ResultCardinality.SingleOrDefault
                    && !subqueryTranslation.ShaperExpression.Type.IsNullableType())
                {
                    scalarSubqueryExpression = _sqlExpressionFactory.Coalesce(
                        scalarSubqueryExpression,
                        (SqlExpression)Visit(subqueryTranslation.ShaperExpression.Type.GetDefaultValueConstant()));
                }

                return scalarSubqueryExpression;
            }

            SqlExpression sqlObject = null;
            SqlExpression[] arguments;
            var method = methodCallExpression.Method;

            if (method.Name == nameof(object.Equals)
                && methodCallExpression.Object != null
                && methodCallExpression.Arguments.Count == 1)
            {
                var left = Visit(methodCallExpression.Object);
                var right = Visit(RemoveObjectConvert(methodCallExpression.Arguments[0]));

                if (TryRewriteEntityEquality(ExpressionType.Equal,
                        left ?? methodCallExpression.Object,
                        right ?? methodCallExpression.Arguments[0],
                        out var result))
                {
                    return result;
                }

                if (left is SqlExpression leftSql
                    && right is SqlExpression rightSql)
                {
                    sqlObject = leftSql;
                    arguments = new SqlExpression[1] { rightSql };
                }
                else
                {
                    return null;
                }
            }
            else if (method.Name == nameof(object.Equals)
                && methodCallExpression.Object == null
                && methodCallExpression.Arguments.Count == 2)
            {
                if (methodCallExpression.Arguments[0].Type == typeof(object[])
                    && methodCallExpression.Arguments[0] is NewArrayExpression)
                {
                    return Visit(ConvertObjectArrayEqualityComparison(
                        methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]));
                }

                var left = Visit(RemoveObjectConvert(methodCallExpression.Arguments[0]));
                var right = Visit(RemoveObjectConvert(methodCallExpression.Arguments[1]));

                if (TryRewriteEntityEquality(ExpressionType.Equal,
                    left ?? methodCallExpression.Arguments[0],
                    right ?? methodCallExpression.Arguments[1],
                    out var result))
                {
                    return result;
                }

                if (left is SqlExpression leftSql
                    && right is SqlExpression rightSql)
                {
                    arguments = new SqlExpression[2] { leftSql, rightSql };
                }
                else
                {
                    return null;
                }
            }
            else if (method.IsGenericMethod
                && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains))
            {
                var enumerable = Visit(methodCallExpression.Arguments[0]);
                var item = Visit(methodCallExpression.Arguments[1]);

                if (TryRewriteContainsEntity(enumerable, item ?? methodCallExpression.Arguments[1], out var result))
                {
                    return result;
                }

                if (enumerable is SqlExpression sqlEnumerable
                    && item is SqlExpression sqlItem)
                {
                    arguments = new SqlExpression[2] { sqlEnumerable, sqlItem };
                }
                else
                {
                    return null;
                }
            }
            else if (methodCallExpression.Arguments.Count == 1
                && method.IsContainsMethod())
            {
                var enumerable = Visit(methodCallExpression.Object);
                var item = Visit(methodCallExpression.Arguments[0]);

                if (TryRewriteContainsEntity(enumerable, item ?? methodCallExpression.Arguments[0], out var result))
                {
                    return result;
                }

                if (enumerable is SqlExpression sqlEnumerable
                    && item is SqlExpression sqlItem)
                {
                    sqlObject = sqlEnumerable;
                    arguments = new SqlExpression[1] { sqlItem };
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (TranslationFailed(methodCallExpression.Object, Visit(methodCallExpression.Object), out sqlObject))
                {
                    return null;
                }

                arguments = new SqlExpression[methodCallExpression.Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                {
                    var argument = methodCallExpression.Arguments[i];
                    if (TranslationFailed(argument, Visit(argument), out var sqlArgument))
                    {
                        return null;
                    }

                    arguments[i] = sqlArgument;
                }
            }

            var translation = Dependencies.MethodCallTranslatorProvider.Translate(_model, sqlObject, methodCallExpression.Method, arguments);

            if (translation == null)
            {
                if (methodCallExpression.Method == _stringEqualsWithStringComparison
                    || methodCallExpression.Method == _stringEqualsWithStringComparisonStatic)
                {
                    AddTranslationErrorDetails(CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison);
                }
                else
                {
                    AddTranslationErrorDetails(CoreStrings.QueryUnableToTranslateMethod(
                        methodCallExpression.Method.Name,
                        methodCallExpression.Method.DeclaringType?.DisplayName()));
                }
            }

            return translation;
        }

        /// <inheritdoc />
        protected override Expression VisitNew(NewExpression newExpression)
            => GetConstantOrNull(Check.NotNull(newExpression, nameof(newExpression)));

        /// <inheritdoc />
        protected override Expression VisitNewArray(NewArrayExpression newArrayExpression) => null;

        /// <inheritdoc />
        protected override Expression VisitParameter(ParameterExpression parameterExpression)
            => parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal) == true
                ? new SqlParameterExpression(Check.NotNull(parameterExpression, nameof(parameterExpression)), null)
                : null;

        /// <inheritdoc />
        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            Check.NotNull(typeBinaryExpression, nameof(typeBinaryExpression));

            var innerExpression = Visit(typeBinaryExpression.Expression);

            if (typeBinaryExpression.NodeType == ExpressionType.TypeIs
                && innerExpression is EntityReferenceExpression entityReferenceExpression)
            {
                var entityType = entityReferenceExpression.EntityType;
                if (entityType.GetAllBaseTypesInclusive().Any(et => et.ClrType == typeBinaryExpression.TypeOperand))
                {
                    return _sqlExpressionFactory.Constant(true);
                }

                var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == typeBinaryExpression.TypeOperand);
                if (derivedType != null)
                {
                    var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();
                    var discriminatorProperty = entityType.GetDiscriminatorProperty();
                    if (discriminatorProperty == null)
                    {
                        // TPT
                        if (entityReferenceExpression.SubqueryEntity != null)
                        {
                            var entityShaper = (EntityShaperExpression)entityReferenceExpression.SubqueryEntity.ShaperExpression;
                            var entityProjection = (EntityProjectionExpression)Visit(entityShaper.ValueBufferExpression);
                            var subSelectExpression = (SelectExpression)entityReferenceExpression.SubqueryEntity.QueryExpression;

                            var predicate = entityProjection.EntityTypeIdentifyingExpressionMap
                                .Where(kvp => concreteEntityTypes.Contains(kvp.Key))
                                .Select(kvp => kvp.Value)
                                .Aggregate((l, r) => _sqlExpressionFactory.OrElse(l, r));

                            subSelectExpression.ApplyPredicate(predicate);
                            subSelectExpression.ReplaceProjectionMapping(new Dictionary<ProjectionMember, Expression>());
                            if (subSelectExpression.Limit == null
                                && subSelectExpression.Offset == null)
                            {
                                subSelectExpression.ClearOrdering();
                            }

                            return _sqlExpressionFactory.Exists(subSelectExpression, false);
                        }

                        if (entityReferenceExpression.ParameterEntity != null)
                        {
                            var entityProjection = (EntityProjectionExpression)Visit(
                                entityReferenceExpression.ParameterEntity.ValueBufferExpression);

                            return entityProjection.EntityTypeIdentifyingExpressionMap
                                .Where(kvp => concreteEntityTypes.Contains(kvp.Key))
                                .Select(kvp => kvp.Value)
                                .Aggregate((l, r) => _sqlExpressionFactory.OrElse(l, r));
                        }
                    }
                    else
                    {
                        var discriminatorColumn = BindProperty(entityReferenceExpression, discriminatorProperty);

                        if (discriminatorColumn != null)
                        {
                            return concreteEntityTypes.Count == 1
                                ? _sqlExpressionFactory.Equal(
                                    discriminatorColumn,
                                    _sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                                : (Expression)_sqlExpressionFactory.In(
                                    discriminatorColumn,
                                    _sqlExpressionFactory.Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()),
                                    negated: false);
                        }
                    }
                }
            }

            return null;
        }

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, nameof(unaryExpression));

            var operand = Visit(unaryExpression.Operand);

            if (operand is EntityReferenceExpression entityReferenceExpression
                && (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked
                    || unaryExpression.NodeType == ExpressionType.TypeAs))
            {
                return entityReferenceExpression.Convert(unaryExpression.Type);
            }

            if (TranslationFailed(unaryExpression.Operand, operand, out var sqlOperand))
            {
                return null;
            }

            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Not:
                    return _sqlExpressionFactory.Not(sqlOperand);

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return _sqlExpressionFactory.Negate(sqlOperand);

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
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
                        || Dependencies.TypeMappingSource.FindMapping(unaryExpression.Type) != null)
                    {
                        sqlOperand = _sqlExpressionFactory.ApplyDefaultTypeMapping(sqlOperand);

                        return _sqlExpressionFactory.Convert(sqlOperand, unaryExpression.Type);
                    }

                    break;

                case ExpressionType.Quote:
                    return operand;
            }

            return null;
        }

        private Expression TryBindMember(Expression source, MemberIdentity member)
        {
            if (!(source is EntityReferenceExpression entityReferenceExpression))
            {
                return null;
            }

            var entityType = entityReferenceExpression.EntityType;
            var property = member.MemberInfo != null
                ? entityType.FindProperty(member.MemberInfo)
                : entityType.FindProperty(member.Name);

            if (property != null)
            {
                return BindProperty(entityReferenceExpression, property);
            }

            AddTranslationErrorDetails(
                CoreStrings.QueryUnableToTranslateMember(
                    member.Name,
                    entityReferenceExpression.EntityType.DisplayName()));

            return null;
        }

        private SqlExpression BindProperty(EntityReferenceExpression entityReferenceExpression, IProperty property)
        {
            if (entityReferenceExpression.ParameterEntity != null)
            {
                return ((EntityProjectionExpression)Visit(entityReferenceExpression.ParameterEntity.ValueBufferExpression)).BindProperty(property);
            }

            if (entityReferenceExpression.SubqueryEntity != null)
            {
                var entityShaper = (EntityShaperExpression)entityReferenceExpression.SubqueryEntity.ShaperExpression;
                var innerProjection = ((EntityProjectionExpression)Visit(entityShaper.ValueBufferExpression)).BindProperty(property);
                var subSelectExpression = (SelectExpression)entityReferenceExpression.SubqueryEntity.QueryExpression;
                subSelectExpression.AddToProjection(innerProjection);

                return new ScalarSubqueryExpression(subSelectExpression);
            }

            return null;
        }

        private static Expression GetSelectorOnGrouping(
            MethodCallExpression methodCallExpression, GroupByShaperExpression groupByShaperExpression)
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

        private static Expression GetPredicateOnGrouping(
            MethodCallExpression methodCallExpression, GroupByShaperExpression groupByShaperExpression)
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

        private static Expression TryRemoveImplicitConvert(Expression expression)
        {
            if (expression is UnaryExpression unaryExpression
                && (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked))
            {
                var innerType = unaryExpression.Operand.Type.UnwrapNullableType();
                if (innerType.IsEnum)
                {
                    innerType = Enum.GetUnderlyingType(innerType);
                }

                var convertedType = expression.Type.UnwrapNullableType();

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

            return expression;
        }

        private static Expression RemoveObjectConvert(Expression expression)
            => expression is UnaryExpression unaryExpression
                && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                && unaryExpression.Type == typeof(object)
                    ? unaryExpression.Operand
                    : expression;

        private static Expression ConvertObjectArrayEqualityComparison(Expression left, Expression right)
        {
            var leftExpressions = ((NewArrayExpression)left).Expressions;
            var rightExpressions = ((NewArrayExpression)right).Expressions;

            return leftExpressions.Zip(
                    rightExpressions,
                    (l, r) => (Expression)Expression.Call(_objectEqualsMethodInfo, l, r))
                .Aggregate((a, b) => Expression.AndAlso(a, b));
        }

        private static SqlConstantExpression GetConstantOrNull(Expression expression)
            => CanEvaluate(expression)
                ? new SqlConstantExpression(
                    Expression.Constant(
                        Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile().Invoke(),
                        expression.Type),
                    null)
                : null;

        private bool TryRewriteContainsEntity(Expression source, Expression item, out Expression result)
        {
            result = null;

            if (!(item is EntityReferenceExpression itemEntityReference))
            {
                return false;
            }

            var entityType = itemEntityReference.EntityType;
            var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;
            if (primaryKeyProperties == null)
            {
                throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(entityType.DisplayName()));
            }

            if (primaryKeyProperties.Count > 1)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityEqualityContainsWithCompositeKeyNotSupported(entityType.DisplayName()));
            }

            var property = primaryKeyProperties[0];
            Expression rewrittenSource;
            switch (source)
            {
                case SqlConstantExpression sqlConstantExpression:
                    var values = (IEnumerable)sqlConstantExpression.Value;
                    var propertyValueList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(property.ClrType.MakeNullable()));
                    var propertyGetter = property.GetGetter();
                    foreach (var value in values)
                    {
                        propertyValueList.Add(propertyGetter.GetClrValue(value));
                    }

                    rewrittenSource = Expression.Constant(propertyValueList);
                    break;

                case SqlParameterExpression sqlParameterExpression
                when sqlParameterExpression.Name.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal):
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            _parameterListValueExtractor.MakeGenericMethod(entityType.ClrType, property.ClrType.MakeNullable()),
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(sqlParameterExpression.Name, typeof(string)),
                            Expression.Constant(property, typeof(IProperty))),
                        QueryCompilationContext.QueryContextParameter
                    );

                    var newParameterName =
                        $"{RuntimeParameterPrefix}" +
                        $"{sqlParameterExpression.Name.Substring(QueryCompilationContext.QueryParameterPrefix.Length)}_{property.Name}";

                    rewrittenSource = _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                    break;

                default:
                    return false;
            }

            result = Visit(Expression.Call(
                EnumerableMethods.Contains.MakeGenericMethod(property.ClrType.MakeNullable()),
                rewrittenSource,
                CreatePropertyAccessExpression(item, property)));

            return true;
        }

        private bool TryRewriteEntityEquality(ExpressionType nodeType, Expression left, Expression right, out Expression result)
        {
            var leftEntityReference = left as EntityReferenceExpression;
            var rightEntityReference = right as EntityReferenceExpression;

            if (leftEntityReference == null
                && rightEntityReference == null)
            {
                result = null;
                return false;
            }

            if (IsNullSqlConstantExpression(left)
                || IsNullSqlConstantExpression(right))
            {
                var nonNullEntityReference = IsNullSqlConstantExpression(left) ? rightEntityReference : leftEntityReference;
                var entityType1 = nonNullEntityReference.EntityType;

                var linkingFks = entityType1.GetViewOrTableMappings().FirstOrDefault()?.Table.GetRowInternalForeignKeys(entityType1);
                if (linkingFks != null
                    && linkingFks.Any())
                {
                    // Optional dependent sharing table
                    var requiredNonPkProperties = entityType1.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
                    if (requiredNonPkProperties.Count > 0)
                    {
                        result = Visit(requiredNonPkProperties.Select(p =>
                            {
                                var comparison = Expression.Call(_objectEqualsMethodInfo,
                                    Expression.Convert(CreatePropertyAccessExpression(nonNullEntityReference, p), typeof(object)),
                                    Expression.Convert(Expression.Constant(null, p.ClrType.MakeNullable()), typeof(object)));

                                return nodeType == ExpressionType.Equal
                                    ? (Expression)comparison
                                    : Expression.Not(comparison);
                            }).Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r)));

                        return true;
                    }
                    else
                    {
                        var allNonPkProperties = entityType1.GetProperties().Where(p => !p.IsPrimaryKey()).ToList();
                        if (allNonPkProperties.Count > 0)
                        {
                            result = Visit(allNonPkProperties.Select(p =>
                                {
                                    var comparison = Expression.Call(_objectEqualsMethodInfo,
                                        Expression.Convert(CreatePropertyAccessExpression(nonNullEntityReference, p), typeof(object)),
                                        Expression.Convert(Expression.Constant(null, p.ClrType.MakeNullable()), typeof(object)));

                                    return nodeType == ExpressionType.Equal
                                        ? (Expression)comparison
                                        : Expression.Not(comparison);
                                }).Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.AndAlso(l, r) : Expression.OrElse(l, r)));

                            return true;
                        }

                        result = null;
                        return false;
                    }
                }

                var primaryKeyProperties1 = entityType1.FindPrimaryKey()?.Properties;
                if (primaryKeyProperties1 == null)
                {
                    throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(entityType1.DisplayName()));
                }

                result = Visit(primaryKeyProperties1.Select(p =>
                    {
                    var comparison = Expression.Call(_objectEqualsMethodInfo,
                        Expression.Convert(CreatePropertyAccessExpression(nonNullEntityReference, p), typeof(object)),
                        Expression.Convert(Expression.Constant(null, p.ClrType.MakeNullable()), typeof(object)));

                        return nodeType == ExpressionType.Equal
                            ? (Expression)comparison
                            : Expression.Not(comparison);
                    }).Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r)));

                return true;
            }

            var leftEntityType = leftEntityReference?.EntityType;
            var rightEntityType = rightEntityReference?.EntityType;
            var entityType = leftEntityType ?? rightEntityType;

            Debug.Assert(entityType != null, "At least one side should be entityReference so entityType should be non-null.");

            if (leftEntityType != null
                && rightEntityType != null
                && leftEntityType.GetRootType() != rightEntityType.GetRootType())
            {
                result = _sqlExpressionFactory.Constant(false);
                return true;
            }

            var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;
            if (primaryKeyProperties == null)
            {
                throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(entityType.DisplayName()));
            }

            if (primaryKeyProperties.Count > 1
                && (leftEntityReference?.SubqueryEntity != null
                    || rightEntityReference?.SubqueryEntity != null))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityEqualitySubqueryWithCompositeKeyNotSupported(entityType.DisplayName()));
            }

            result = Visit(primaryKeyProperties.Select(p =>
                {
                    var comparison = Expression.Call(_objectEqualsMethodInfo,
                        Expression.Convert(CreatePropertyAccessExpression(left, p), typeof(object)),
                        Expression.Convert(CreatePropertyAccessExpression(right, p), typeof(object)));

                    return nodeType == ExpressionType.Equal
                        ? (Expression)comparison
                        : Expression.Not(comparison);
                }).Aggregate((l, r) => Expression.AndAlso(l, r)));

            return true;
        }

        private Expression CreatePropertyAccessExpression(Expression target, IProperty property)
        {
            switch (target)
            {
                case SqlConstantExpression sqlConstantExpression:
                    return Expression.Constant(
                        property.GetGetter().GetClrValue(sqlConstantExpression.Value), property.ClrType.MakeNullable());

                case SqlParameterExpression sqlParameterExpression
                when sqlParameterExpression.Name.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal):
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            _parameterValueExtractor.MakeGenericMethod(property.ClrType.MakeNullable()),
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(sqlParameterExpression.Name, typeof(string)),
                            Expression.Constant(property, typeof(IProperty))),
                        QueryCompilationContext.QueryContextParameter);

                    var newParameterName =
                        $"{RuntimeParameterPrefix}" +
                        $"{sqlParameterExpression.Name.Substring(QueryCompilationContext.QueryParameterPrefix.Length)}_{property.Name}";

                    return _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);

                case MemberInitExpression memberInitExpression
                when memberInitExpression.Bindings.SingleOrDefault(
                    mb => mb.Member.Name == property.Name) is MemberAssignment memberAssignment:
                    return memberAssignment.Expression;

                default:
                    return target.CreateEFPropertyExpression(property);
            }
        }

        private static T ParameterValueExtractor<T>(QueryContext context, string baseParameterName, IProperty property)
        {
            var baseParameter = context.ParameterValues[baseParameterName];
            return baseParameter == null ? (T)(object)null : (T)property.GetGetter().GetClrValue(baseParameter);
        }

        private static List<TProperty> ParameterListValueExtractor<TEntity, TProperty>(
            QueryContext context, string baseParameterName, IProperty property)
        {
            if (!(context.ParameterValues[baseParameterName] is IEnumerable<TEntity> baseListParameter))
            {
                return null;
            }

            var getter = property.GetGetter();
            return baseListParameter.Select(e => e != null ? (TProperty)getter.GetClrValue(e) : (TProperty)(object)null).ToList();
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

        private static bool IsNullSqlConstantExpression(Expression expression)
            => expression is SqlConstantExpression sqlConstant && sqlConstant.Value == null;

        [DebuggerStepThrough]
        private static bool TranslationFailed(Expression original, Expression translation, out SqlExpression castTranslation)
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

        private sealed class EntityReferenceExpression : Expression
        {
            public EntityReferenceExpression(EntityShaperExpression parameter)
            {
                ParameterEntity = parameter;
                EntityType = parameter.EntityType;
            }

            public EntityReferenceExpression(ShapedQueryExpression subquery)
            {
                SubqueryEntity = subquery;
                EntityType = ((EntityShaperExpression)subquery.ShaperExpression).EntityType;
            }

            private EntityReferenceExpression(EntityReferenceExpression entityReferenceExpression, IEntityType entityType)
            {
                ParameterEntity = entityReferenceExpression.ParameterEntity;
                SubqueryEntity = entityReferenceExpression.SubqueryEntity;
                EntityType = entityType;
            }

            public EntityShaperExpression ParameterEntity { get; }
            public ShapedQueryExpression SubqueryEntity { get; }
            public IEntityType EntityType { get; }

            public override Type Type => EntityType.ClrType;
            public override ExpressionType NodeType => ExpressionType.Extension;

            public Expression Convert(Type type)
            {
                if (type == typeof(object) // Ignore object conversion
                    || type.IsAssignableFrom(Type)) // Ignore casting to base type/interface
                {
                    return this;
                }

                var derivedEntityType = EntityType.GetDerivedTypes().FirstOrDefault(et => et.ClrType == type);

                return derivedEntityType == null ? null : new EntityReferenceExpression(this, derivedEntityType);
            }
        }

        private sealed class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                if (extensionExpression is SqlExpression sqlExpression
                    && !(extensionExpression is SqlFragmentExpression))
                {
                    if (sqlExpression.TypeMapping == null)
                    {
                        throw new InvalidOperationException(CoreStrings.NullTypeMappingInSqlTree);
                    }
                }

                return base.VisitExtension(extensionExpression);
            }
        }
    }
}

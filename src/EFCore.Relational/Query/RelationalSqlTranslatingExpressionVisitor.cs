// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

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

    private static readonly List<MethodInfo> SingleResultMethodInfos = new()
    {
        QueryableMethods.FirstWithPredicate,
        QueryableMethods.FirstWithoutPredicate,
        QueryableMethods.FirstOrDefaultWithPredicate,
        QueryableMethods.FirstOrDefaultWithoutPredicate,
        QueryableMethods.SingleWithPredicate,
        QueryableMethods.SingleWithoutPredicate,
        QueryableMethods.SingleOrDefaultWithPredicate,
        QueryableMethods.SingleOrDefaultWithoutPredicate,
        QueryableMethods.LastWithPredicate,
        QueryableMethods.LastWithoutPredicate,
        QueryableMethods.LastOrDefaultWithPredicate,
        QueryableMethods.LastOrDefaultWithoutPredicate
        //QueryableMethodProvider.ElementAtMethodInfo,
        //QueryableMethodProvider.ElementAtOrDefaultMethodInfo
    };

    private static readonly List<MethodInfo> PredicateAggregateMethodInfos = new()
    {
        QueryableMethods.CountWithPredicate,
        QueryableMethods.CountWithoutPredicate,
        QueryableMethods.LongCountWithPredicate,
        QueryableMethods.LongCountWithoutPredicate
    };

    private static readonly MethodInfo ParameterValueExtractorMethod =
        typeof(RelationalSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor))!;

    private static readonly MethodInfo ParameterListValueExtractorMethod =
        typeof(RelationalSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterListValueExtractor))!;

    private static readonly MethodInfo StringEqualsWithStringComparison
        = typeof(string).GetRuntimeMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) })!;

    private static readonly MethodInfo StringEqualsWithStringComparisonStatic
        = typeof(string).GetRuntimeMethod(nameof(string.Equals), new[] { typeof(string), typeof(string), typeof(StringComparison) })!;

    private static readonly MethodInfo GetTypeMethodInfo = typeof(object).GetTypeInfo().GetDeclaredMethod(nameof(GetType))!;

    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly IModel _model;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
    private readonly SqlTypeMappingVerifyingExpressionVisitor _sqlTypeMappingVerifyingExpressionVisitor;

    private bool _throwForNotTranslatedEfProperty;

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalSqlTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    /// <param name="queryableMethodTranslatingExpressionVisitor">A parent queryable method translating expression visitor to translate subquery.</param>
    public RelationalSqlTranslatingExpressionVisitor(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
    {
        Dependencies = dependencies;
        _sqlExpressionFactory = dependencies.SqlExpressionFactory;
        _queryCompilationContext = queryCompilationContext;
        _model = queryCompilationContext.Model;
        _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
        _sqlTypeMappingVerifyingExpressionVisitor = new SqlTypeMappingVerifyingExpressionVisitor();
        _throwForNotTranslatedEfProperty = true;
    }

    /// <summary>
    ///     Detailed information about errors encountered during translation.
    /// </summary>
    public virtual string? TranslationErrorDetails { get; private set; }

    /// <summary>
    ///     Adds detailed information about error encountered during translation.
    /// </summary>
    /// <param name="details">Detailed information about error encountered during translation.</param>
    protected virtual void AddTranslationErrorDetails(string details)
    {
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
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalSqlTranslatingExpressionVisitorDependencies Dependencies { get; }

    /// <summary>
    ///     Translates an expression to an equivalent SQL representation.
    /// </summary>
    /// <param name="expression">An expression to translate.</param>
    /// <returns>A SQL translation of the given expression.</returns>
    public virtual SqlExpression? Translate(Expression expression)
    {
        TranslationErrorDetails = null;

        return TranslateInternal(expression);
    }

    private SqlExpression? TranslateInternal(Expression expression)
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
    /// <param name="sqlExpression">An expression to translate Average over.</param>
    /// <returns>A SQL translation of Average over the given expression.</returns>
    [Obsolete("Use IAggregateMethodCallTranslatorProvider to add translation for aggregate methods")]
    public virtual SqlExpression? TranslateAverage(SqlExpression sqlExpression)
    {
        var inputType = sqlExpression.Type;
        if (inputType == typeof(int)
            || inputType == typeof(long))
        {
            sqlExpression = sqlExpression is DistinctExpression distinctExpression
                ? new DistinctExpression(
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(
                        _sqlExpressionFactory.Convert(distinctExpression.Operand, typeof(double))))
                : _sqlExpressionFactory.ApplyDefaultTypeMapping(
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
            : _sqlExpressionFactory.Function(
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
    /// <param name="sqlExpression">An expression to translate Count over.</param>
    /// <returns>A SQL translation of Count over the given expression.</returns>
    [Obsolete("Use IAggregateMethodCallTranslatorProvider to add translation for aggregate methods")]
    public virtual SqlExpression? TranslateCount(SqlExpression sqlExpression)
        => _sqlExpressionFactory.ApplyDefaultTypeMapping(
            _sqlExpressionFactory.Function(
                "COUNT",
                new[] { sqlExpression },
                nullable: false,
                argumentsPropagateNullability: new[] { false },
                typeof(int)));

    /// <summary>
    ///     Translates LongCount over an expression to an equivalent SQL representation.
    /// </summary>
    /// <param name="sqlExpression">An expression to translate LongCount over.</param>
    /// <returns>A SQL translation of LongCount over the given expression.</returns>
    [Obsolete("Use IAggregateMethodCallTranslatorProvider to add translation for aggregate methods")]
    public virtual SqlExpression? TranslateLongCount(SqlExpression sqlExpression)
        => _sqlExpressionFactory.ApplyDefaultTypeMapping(
            _sqlExpressionFactory.Function(
                "COUNT",
                new[] { sqlExpression },
                nullable: false,
                argumentsPropagateNullability: new[] { false },
                typeof(long)));

    /// <summary>
    ///     Translates Max over an expression to an equivalent SQL representation.
    /// </summary>
    /// <param name="sqlExpression">An expression to translate Max over.</param>
    /// <returns>A SQL translation of Max over the given expression.</returns>
    [Obsolete("Use IAggregateMethodCallTranslatorProvider to add translation for aggregate methods")]
    public virtual SqlExpression? TranslateMax(SqlExpression sqlExpression)
        => sqlExpression != null
            ? _sqlExpressionFactory.Function(
                "MAX",
                new[] { sqlExpression },
                nullable: true,
                argumentsPropagateNullability: new[] { false },
                sqlExpression.Type,
                sqlExpression.TypeMapping)
            : null;

    /// <summary>
    ///     Translates Min over an expression to an equivalent SQL representation.
    /// </summary>
    /// <param name="sqlExpression">An expression to translate Min over.</param>
    /// <returns>A SQL translation of Min over the given expression.</returns>
    [Obsolete("Use IAggregateMethodCallTranslatorProvider to add translation for aggregate methods")]
    public virtual SqlExpression? TranslateMin(SqlExpression sqlExpression)
        => sqlExpression != null
            ? _sqlExpressionFactory.Function(
                "MIN",
                new[] { sqlExpression },
                nullable: true,
                argumentsPropagateNullability: new[] { false },
                sqlExpression.Type,
                sqlExpression.TypeMapping)
            : null;

    /// <summary>
    ///     Translates Sum over an expression to an equivalent SQL representation.
    /// </summary>
    /// <param name="sqlExpression">An expression to translate Sum over.</param>
    /// <returns>A SQL translation of Sum over the given expression.</returns>
    [Obsolete("Use IAggregateMethodCallTranslatorProvider to add translation for aggregate methods")]
    public virtual SqlExpression? TranslateSum(SqlExpression sqlExpression)
    {
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
            : _sqlExpressionFactory.Function(
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
        if (binaryExpression.Left.Type == typeof(object[])
            && binaryExpression.Left is NewArrayExpression
            && binaryExpression.NodeType == ExpressionType.Equal)
        {
            return Visit(ConvertObjectArrayEqualityComparison(binaryExpression.Left, binaryExpression.Right));
        }

        if (binaryExpression.NodeType == ExpressionType.Equal
            || binaryExpression.NodeType == ExpressionType.NotEqual
            && binaryExpression.Left.Type == typeof(Type))
        {
            if (IsGetTypeMethodCall(binaryExpression.Left, out var entityReference1)
                && IsTypeConstant(binaryExpression.Right, out var type1))
            {
                return ProcessGetType(entityReference1!, type1!, binaryExpression.NodeType == ExpressionType.Equal);
            }

            if (IsGetTypeMethodCall(binaryExpression.Right, out var entityReference2)
                && IsTypeConstant(binaryExpression.Left, out var type2))
            {
                return ProcessGetType(entityReference2!, type2!, binaryExpression.NodeType == ExpressionType.Equal);
            }
        }

        var left = TryRemoveImplicitConvert(binaryExpression.Left);
        var right = TryRemoveImplicitConvert(binaryExpression.Right);

        // Remove convert-to-object nodes if both sides have them, or if the other side is null constant
        var isLeftConvertToObject = TryUnwrapConvertToObject(left, out var leftOperand);
        var isRightConvertToObject = TryUnwrapConvertToObject(right, out var rightOperand);
        if (isLeftConvertToObject && isRightConvertToObject)
        {
            left = leftOperand!;
            right = rightOperand!;
        }
        else if (isLeftConvertToObject && right.IsNullConstantExpression())
        {
            left = leftOperand!;
        }
        else if (isRightConvertToObject && left.IsNullConstantExpression())
        {
            right = rightOperand!;
        }

        if ((binaryExpression.NodeType == ExpressionType.Equal || binaryExpression.NodeType == ExpressionType.NotEqual)
            && (left.IsNullConstantExpression() || right.IsNullConstantExpression()))
        {
            var nonNullExpression = left.IsNullConstantExpression() ? right : left;
            if (nonNullExpression is MethodCallExpression nonNullMethodCallExpression
                && nonNullMethodCallExpression.Method.DeclaringType == typeof(Queryable)
                && nonNullMethodCallExpression.Method.IsGenericMethod
                && SingleResultMethodInfos.Contains(nonNullMethodCallExpression.Method.GetGenericMethodDefinition()))
            {
                var source = nonNullMethodCallExpression.Arguments[0];
                if (nonNullMethodCallExpression.Arguments.Count == 2)
                {
                    source = Expression.Call(
                        QueryableMethods.Where.MakeGenericMethod(source.Type.GetSequenceType()),
                        source,
                        nonNullMethodCallExpression.Arguments[1]);
                }

                var translatedSubquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(source);
                if (translatedSubquery != null)
                {
                    var projection = translatedSubquery.ShaperExpression;
                    if (projection is NewExpression
                        || RemoveConvert(projection) is EntityShaperExpression { IsNullable: false }
                        || RemoveConvert(projection) is CollectionResultExpression)
                    {
                        var anySubquery = Expression.Call(
                            QueryableMethods.AnyWithoutPredicate.MakeGenericMethod(translatedSubquery.Type.GetSequenceType()),
                            translatedSubquery);

                        return Visit(
                            binaryExpression.NodeType == ExpressionType.Equal
                                ? Expression.Not(anySubquery)
                                : anySubquery);
                    }

                    static Expression RemoveConvert(Expression e)
                        => e is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary
                            ? RemoveConvert(unary.Operand)
                            : e;
                }
            }
        }

        var visitedLeft = Visit(left);
        var visitedRight = Visit(right);

        if ((binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            // Visited expression could be null, We need to pass MemberInitExpression
            && TryRewriteEntityEquality(
                binaryExpression.NodeType,
                visitedLeft == QueryCompilationContext.NotTranslatedExpression ? left : visitedLeft,
                visitedRight == QueryCompilationContext.NotTranslatedExpression ? right : visitedRight,
                equalsMethod: false, out var result))
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
                ? QueryCompilationContext.NotTranslatedExpression
                : uncheckedNodeTypeVariant == ExpressionType.Coalesce
                    ? _sqlExpressionFactory.Coalesce(sqlLeft!, sqlRight!)
                    : _sqlExpressionFactory.MakeBinary(
                        uncheckedNodeTypeVariant,
                        sqlLeft!,
                        sqlRight!,
                        null)
                    ?? QueryCompilationContext.NotTranslatedExpression;

        Expression ProcessGetType(EntityReferenceExpression entityReferenceExpression, Type comparisonType, bool match)
        {
            var entityType = entityReferenceExpression.EntityType;

            if (entityType.BaseType == null
                && !entityType.GetDirectlyDerivedTypes().Any())
            {
                // No hierarchy
                return _sqlExpressionFactory.Constant((entityType.ClrType == comparisonType) == match);
            }

            if (entityType.GetAllBaseTypes().Any(e => e.ClrType == comparisonType))
            {
                // EntitySet will never contain a type of base type
                return _sqlExpressionFactory.Constant(!match);
            }

            var derivedType = entityType.GetDerivedTypesInclusive().SingleOrDefault(et => et.ClrType == comparisonType);
            // If no derived type matches then fail the translation
            if (derivedType == null)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            // If the derived type is abstract type then predicate will always be false
            if (derivedType.IsAbstract())
            {
                return _sqlExpressionFactory.Constant(!match);
            }

            // Or add predicate for matching that particular type discriminator value
            var discriminatorProperty = entityType.FindDiscriminatorProperty();
            if (discriminatorProperty == null)
            {
                // TPT or TPC
                var discriminatorValue = derivedType.ShortName();
                if (entityReferenceExpression.SubqueryEntity != null)
                {
                    var entityShaper = (EntityShaperExpression)entityReferenceExpression.SubqueryEntity.ShaperExpression;
                    var entityProjection = (EntityProjectionExpression)Visit(entityShaper.ValueBufferExpression);
                    var subSelectExpression = (SelectExpression)entityReferenceExpression.SubqueryEntity.QueryExpression;

                    var predicate = GeneratePredicateTpt(entityProjection);

                    subSelectExpression.ApplyPredicate(predicate);
                    subSelectExpression.ReplaceProjection(new List<Expression>());
                    subSelectExpression.ApplyProjection();
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

                    return GeneratePredicateTpt(entityProjection);
                }

                SqlExpression GeneratePredicateTpt(EntityProjectionExpression entityProjectionExpression)
                {
                    if (entityProjectionExpression.DiscriminatorExpression is CaseExpression caseExpression)
                    {
                        // TPT case
                        // Most root type doesn't have matching case
                        // All derived types needs to be excluded
                        var derivedTypeValues = derivedType.GetDerivedTypes().Where(e => !e.IsAbstract()).Select(e => e.ShortName())
                            .ToList();
                        var predicates = new List<SqlExpression>();
                        foreach (var caseWhenClause in caseExpression.WhenClauses)
                        {
                            var value = (string)((SqlConstantExpression)caseWhenClause.Result).Value!;
                            if (value == discriminatorValue)
                            {
                                predicates.Add(caseWhenClause.Test);
                            }
                            else if (derivedTypeValues.Contains(value))
                            {
                                predicates.Add(_sqlExpressionFactory.Not(caseWhenClause.Test));
                            }
                        }

                        var result = predicates.Aggregate((a, b) => _sqlExpressionFactory.AndAlso(a, b));

                        return match ? result : _sqlExpressionFactory.Not(result);
                    }

                    return match
                        ? _sqlExpressionFactory.Equal(
                            entityProjectionExpression.DiscriminatorExpression!,
                            _sqlExpressionFactory.Constant(discriminatorValue))
                        : _sqlExpressionFactory.NotEqual(
                            entityProjectionExpression.DiscriminatorExpression!,
                            _sqlExpressionFactory.Constant(discriminatorValue));
                }
            }
            else
            {
                var discriminatorColumn = BindProperty(entityReferenceExpression, discriminatorProperty);
                if (discriminatorColumn != null)
                {
                    return match
                        ? _sqlExpressionFactory.Equal(
                            discriminatorColumn,
                            _sqlExpressionFactory.Constant(derivedType.GetDiscriminatorValue()))
                        : _sqlExpressionFactory.NotEqual(
                            discriminatorColumn,
                            _sqlExpressionFactory.Constant(derivedType.GetDiscriminatorValue()));
                }
            }

            return QueryCompilationContext.NotTranslatedExpression;
        }

        bool IsGetTypeMethodCall(Expression expression, out EntityReferenceExpression? entityReferenceExpression)
        {
            entityReferenceExpression = null;
            if (expression is not MethodCallExpression methodCallExpression
                || methodCallExpression.Method != GetTypeMethodInfo)
            {
                return false;
            }

            entityReferenceExpression = Visit(methodCallExpression.Object) as EntityReferenceExpression;
            return entityReferenceExpression != null;
        }

        static bool IsTypeConstant(Expression expression, out Type? type)
        {
            type = null;
            if (expression is not UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression
                || unaryExpression.Operand is not ConstantExpression constantExpression)
            {
                return false;
            }

            type = constantExpression.Value as Type;
            return type != null;
        }

        static bool TryUnwrapConvertToObject(Expression expression, out Expression? operand)
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
        var test = Visit(conditionalExpression.Test);
        var ifTrue = Visit(conditionalExpression.IfTrue);
        var ifFalse = Visit(conditionalExpression.IfFalse);

        return TranslationFailed(conditionalExpression.Test, test, out var sqlTest)
            || TranslationFailed(conditionalExpression.IfTrue, ifTrue, out var sqlIfTrue)
            || TranslationFailed(conditionalExpression.IfFalse, ifFalse, out var sqlIfFalse)
                ? QueryCompilationContext.NotTranslatedExpression
                : _sqlExpressionFactory.Case(new[] { new CaseWhenClause(sqlTest!, sqlIfTrue!) }, sqlIfFalse);
    }

    /// <inheritdoc />
    protected override Expression VisitConstant(ConstantExpression constantExpression)
        => new SqlConstantExpression(constantExpression, null);

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case EntityProjectionExpression:
            case EntityReferenceExpression:
            case SqlExpression:
            case EnumerableExpression:
            case JsonQueryExpression:
                return extensionExpression;

            case EntityShaperExpression entityShaperExpression:
                return new EntityReferenceExpression(entityShaperExpression);

            case ProjectionBindingExpression projectionBindingExpression:
                return Visit(
                    ((SelectExpression)projectionBindingExpression.QueryExpression)
                    .GetProjection(projectionBindingExpression));

            case ShapedQueryExpression shapedQueryExpression:
                if (shapedQueryExpression.ResultCardinality == ResultCardinality.Enumerable)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                var shaperExpression = shapedQueryExpression.ShaperExpression;
                ProjectionBindingExpression? mappedProjectionBindingExpression = null;

                var innerExpression = shaperExpression;
                Type? convertedType = null;
                if (shaperExpression is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert)
                {
                    convertedType = unaryExpression.Type;
                    innerExpression = unaryExpression.Operand;
                }

                if (innerExpression is EntityShaperExpression ese
                    && (convertedType == null
                        || convertedType.IsAssignableFrom(ese.Type)))
                {
                    return new EntityReferenceExpression(shapedQueryExpression.UpdateShaperExpression(innerExpression));
                }

                if (innerExpression is ProjectionBindingExpression pbe
                    && (convertedType == null
                        || convertedType.MakeNullable() == innerExpression.Type))
                {
                    mappedProjectionBindingExpression = pbe;
                }

                if (mappedProjectionBindingExpression == null
                    && shaperExpression is BlockExpression blockExpression
                    && blockExpression.Expressions.Count == 2
                    && blockExpression.Expressions[0] is BinaryExpression binaryExpression
                    && binaryExpression.NodeType == ExpressionType.Assign
                    && binaryExpression.Right is ProjectionBindingExpression pbe2)
                {
                    mappedProjectionBindingExpression = pbe2;
                }

                if (mappedProjectionBindingExpression == null)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                var subquery = (SelectExpression)shapedQueryExpression.QueryExpression;
                var projection = subquery.GetProjection(mappedProjectionBindingExpression);
                if (projection is not SqlExpression sqlExpression)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                if (subquery.Tables.Count == 0)
                {
                    return sqlExpression;
                }

                subquery.ReplaceProjection(new List<Expression> { sqlExpression });
                subquery.ApplyProjection();

                SqlExpression scalarSubqueryExpression = new ScalarSubqueryExpression(subquery);

                if (shapedQueryExpression.ResultCardinality == ResultCardinality.SingleOrDefault
                    && !shaperExpression.Type.IsNullableType())
                {
                    scalarSubqueryExpression = _sqlExpressionFactory.Coalesce(
                        scalarSubqueryExpression,
                        (SqlExpression)Visit(shaperExpression.Type.GetDefaultValueConstant()));
                }

                return scalarSubqueryExpression;

            default:
                return QueryCompilationContext.NotTranslatedExpression;
        }
    }

    /// <inheritdoc />
    protected override Expression VisitInvocation(InvocationExpression invocationExpression)
        => QueryCompilationContext.NotTranslatedExpression;

    /// <inheritdoc />
    protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        => throw new InvalidOperationException(CoreStrings.TranslationFailed(lambdaExpression.Print()));

    /// <inheritdoc />
    protected override Expression VisitListInit(ListInitExpression listInitExpression)
        => QueryCompilationContext.NotTranslatedExpression;

    /// <inheritdoc />
    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        var innerExpression = Visit(memberExpression.Expression);

        return TryBindMember(innerExpression, MemberIdentity.Create(memberExpression.Member))
            ?? (TranslationFailed(memberExpression.Expression, innerExpression, out var sqlInnerExpression)
                ? QueryCompilationContext.NotTranslatedExpression
                : Dependencies.MemberTranslatorProvider.Translate(
                    sqlInnerExpression, memberExpression.Member, memberExpression.Type, _queryCompilationContext.Logger))
            ?? QueryCompilationContext.NotTranslatedExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        => GetConstantOrNotTranslated(memberInitExpression);

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        // EF.Property case
        if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
        {
            if (TryBindMember(Visit(source), MemberIdentity.Create(propertyName)) is SqlExpression result)
            {
                return result;
            }

            var message = CoreStrings.QueryUnableToTranslateEFProperty(methodCallExpression.Print());
            if (_throwForNotTranslatedEfProperty)
            {
                throw new InvalidOperationException(message);
            }

            AddTranslationErrorDetails(message);

            return QueryCompilationContext.NotTranslatedExpression;
        }

        // EF Indexer property
        if (methodCallExpression.TryGetIndexerArguments(_model, out source, out propertyName)
            && TryBindMember(Visit(source), MemberIdentity.Create(propertyName)) is SqlExpression indexerResult)
        {
            return indexerResult;
        }

        var method = methodCallExpression.Method;
        var arguments = methodCallExpression.Arguments;

        EnumerableExpression? enumerableExpression = null;
        SqlExpression? sqlObject = null;
        List<SqlExpression> scalarArguments;

        if (method.Name == nameof(object.Equals)
            && methodCallExpression.Object != null
            && arguments.Count == 1)
        {
            var left = Visit(methodCallExpression.Object);
            var right = Visit(RemoveObjectConvert(arguments[0]));

            if (TryRewriteEntityEquality(
                    ExpressionType.Equal,
                    left == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Object : left,
                    right == QueryCompilationContext.NotTranslatedExpression ? arguments[0] : right,
                    equalsMethod: true,
                    out var result))
            {
                return result;
            }

            if (left is SqlExpression leftSql
                && right is SqlExpression rightSql)
            {
                sqlObject = leftSql;
                scalarArguments = new List<SqlExpression> { rightSql };
            }
            else
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }
        else if (method.Name == nameof(object.Equals)
                 && methodCallExpression.Object == null
                 && arguments.Count == 2)
        {
            if (arguments[0].Type == typeof(object[])
                && arguments[0] is NewArrayExpression)
            {
                return Visit(
                    ConvertObjectArrayEqualityComparison(
                        arguments[0], arguments[1]));
            }

            var left = Visit(RemoveObjectConvert(arguments[0]));
            var right = Visit(RemoveObjectConvert(arguments[1]));

            if (TryRewriteEntityEquality(
                    ExpressionType.Equal,
                    left == QueryCompilationContext.NotTranslatedExpression ? arguments[0] : left,
                    right == QueryCompilationContext.NotTranslatedExpression ? arguments[1] : right,
                    equalsMethod: true,
                    out var result))
            {
                return result;
            }

            if (left is SqlExpression leftSql
                && right is SqlExpression rightSql)
            {
                scalarArguments = new List<SqlExpression> { leftSql, rightSql };
            }
            else
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }
        else if (method.IsGenericMethod
                 && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains))
        {
            var enumerable = Visit(arguments[0]);
            var item = Visit(arguments[1]);

            if (TryRewriteContainsEntity(
                    enumerable,
                    item == QueryCompilationContext.NotTranslatedExpression ? arguments[1] : item, out var result))
            {
                return result;
            }

            if (enumerable is SqlExpression sqlEnumerable
                && item is SqlExpression sqlItem)
            {
                scalarArguments = new List<SqlExpression> { sqlEnumerable, sqlItem };
            }
            else
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }
        else if (arguments.Count == 1
                 && method.IsContainsMethod())
        {
            var enumerable = Visit(methodCallExpression.Object);
            var item = Visit(arguments[0]);

            if (TryRewriteContainsEntity(
                    enumerable!,
                    item == QueryCompilationContext.NotTranslatedExpression ? arguments[0] : item, out var result))
            {
                return result;
            }

            if (enumerable is SqlExpression sqlEnumerable
                && item is SqlExpression sqlItem)
            {
                sqlObject = sqlEnumerable;
                scalarArguments = new List<SqlExpression> { sqlItem };
            }
            else
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }
        else
        {
            if (method.IsStatic
                && arguments.Count > 0
                && method.DeclaringType == typeof(Queryable))
            {
                // For queryable methods, either we translate the whole aggregate or we go to subquery mode
                // We don't try to translate component-wise it. Providers should implement in subquery translation.
                if (TryTranslateAggregateMethodCall(methodCallExpression, out var translatedAggregate))
                {
                    return translatedAggregate;
                }

                goto SubqueryTranslation;
            }

            scalarArguments = new List<SqlExpression>();
            if (!TryTranslateAsEnumerableExpression(methodCallExpression.Object, out enumerableExpression)
                && TranslationFailed(methodCallExpression.Object, Visit(methodCallExpression.Object), out sqlObject))
            {
                goto SubqueryTranslation;
            }

            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                if (TryTranslateAsEnumerableExpression(argument, out var eea))
                {
                    if (enumerableExpression != null)
                    {
                        goto SubqueryTranslation;
                    }

                    enumerableExpression = eea;
                    continue;
                }

                var visitedArgument = Visit(argument);
                if (TranslationFailed(argument, visitedArgument, out var sqlArgument))
                {
                    goto SubqueryTranslation;
                }

                scalarArguments.Add(sqlArgument!);
            }
        }

        var translation = enumerableExpression != null
            ? TranslateAggregateMethod(enumerableExpression, method, scalarArguments)
            : Dependencies.MethodCallTranslatorProvider.Translate(
                _model, sqlObject, method, scalarArguments, _queryCompilationContext.Logger);

        if (translation != null)
        {
            return translation;
        }

        if (method == StringEqualsWithStringComparison
            || method == StringEqualsWithStringComparisonStatic)
        {
            AddTranslationErrorDetails(CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison);
        }
        else
        {
            AddTranslationErrorDetails(
                CoreStrings.QueryUnableToTranslateMethod(
                    method.DeclaringType?.DisplayName(),
                    method.Name));
        }

        // Subquery case
        SubqueryTranslation:
        var subqueryTranslation = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);

        return subqueryTranslation == null
            ? QueryCompilationContext.NotTranslatedExpression
            : Visit(subqueryTranslation);
    }

    /// <inheritdoc />
    protected override Expression VisitNew(NewExpression newExpression)
        => GetConstantOrNotTranslated(newExpression);

    /// <inheritdoc />
    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
        => QueryCompilationContext.NotTranslatedExpression;

    /// <inheritdoc />
    protected override Expression VisitParameter(ParameterExpression parameterExpression)
        => parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal) == true
            ? new SqlParameterExpression(parameterExpression, null)
            : throw new InvalidOperationException(CoreStrings.TranslationFailed(parameterExpression.Print()));

    /// <inheritdoc />
    protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
    {
        var innerExpression = Visit(typeBinaryExpression.Expression);

        if (typeBinaryExpression.NodeType != ExpressionType.TypeIs
            || innerExpression is not EntityReferenceExpression entityReferenceExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }
        var entityType = entityReferenceExpression.EntityType;
        if (entityType.GetAllBaseTypesInclusive().Any(et => et.ClrType == typeBinaryExpression.TypeOperand))
        {
            return _sqlExpressionFactory.Constant(true);
        }

        var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == typeBinaryExpression.TypeOperand);
        if (derivedType == null)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }
        var discriminatorProperty = entityType.FindDiscriminatorProperty();
        if (discriminatorProperty == null)
        {
            if (entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy
                && entityType.GetDerivedTypesInclusive().Count(e => !e.IsAbstract()) == 1)
            {
                return _sqlExpressionFactory.Constant(true);
            }

            // TPT or TPC
            var discriminatorValues = derivedType.GetConcreteDerivedTypesInclusive()
                .Select(e => (string)e.GetDiscriminatorValue()!).ToList();
            if (entityReferenceExpression.SubqueryEntity != null)
            {
                var entityShaper = (EntityShaperExpression)entityReferenceExpression.SubqueryEntity.ShaperExpression;
                var entityProjection = (EntityProjectionExpression)Visit(entityShaper.ValueBufferExpression);
                var subSelectExpression = (SelectExpression)entityReferenceExpression.SubqueryEntity.QueryExpression;

                var predicate = GeneratePredicateTpt(entityProjection);

                subSelectExpression.ApplyPredicate(predicate);
                subSelectExpression.ReplaceProjection(new List<Expression>());
                subSelectExpression.ApplyProjection();
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

                return GeneratePredicateTpt(entityProjection);
            }

            SqlExpression GeneratePredicateTpt(EntityProjectionExpression entityProjectionExpression)
            {
                if (entityProjectionExpression.DiscriminatorExpression is CaseExpression caseExpression)
                {
                    var matchingCaseWhenClauses = caseExpression.WhenClauses
                        .Where(wc => discriminatorValues.Contains((string)((SqlConstantExpression)wc.Result).Value!))
                        .ToList();

                    return matchingCaseWhenClauses.Count == 1
                        ? matchingCaseWhenClauses[0].Test
                        : matchingCaseWhenClauses.Select(e => e.Test)
                            .Aggregate((l, r) => _sqlExpressionFactory.OrElse(l, r));
                }

                return discriminatorValues.Count == 1
                    ? _sqlExpressionFactory.Equal(
                        entityProjectionExpression.DiscriminatorExpression!,
                        _sqlExpressionFactory.Constant(discriminatorValues[0]))
                    : _sqlExpressionFactory.In(
                        entityProjectionExpression.DiscriminatorExpression!,
                        _sqlExpressionFactory.Constant(discriminatorValues),
                        negated: false);
            }
        }
        else
        {
            if (!derivedType.GetRootType().GetIsDiscriminatorMappingComplete()
                || !derivedType.GetAllBaseTypesInclusiveAscending()
                    .All(e => (e == derivedType || e.IsAbstract()) && !HasSiblings(e)))
            {
                var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();
                var discriminatorColumn = BindProperty(entityReferenceExpression, discriminatorProperty);
                if (discriminatorColumn != null)
                {
                    return concreteEntityTypes.Count == 1
                        ? _sqlExpressionFactory.Equal(
                            discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                        : _sqlExpressionFactory.In(
                            discriminatorColumn,
                            _sqlExpressionFactory.Constant(
                                concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()),
                            negated: false);
                }
            }
            else
            {
                return _sqlExpressionFactory.Constant(true);
            }
        }

        return QueryCompilationContext.NotTranslatedExpression;

        static bool HasSiblings(IEntityType entityType)
            => entityType.BaseType?.GetDirectlyDerivedTypes().Any(i => i != entityType) == true;
    }

    /// <inheritdoc />
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
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
            return QueryCompilationContext.NotTranslatedExpression;
        }

        switch (unaryExpression.NodeType)
        {
            case ExpressionType.Not:
                return _sqlExpressionFactory.Not(sqlOperand!);

            case ExpressionType.Negate:
            case ExpressionType.NegateChecked:
                return _sqlExpressionFactory.Negate(sqlOperand!);

            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
            case ExpressionType.TypeAs:
                // Object convert needs to be converted to explicit cast when mismatching types
                if (operand.Type.IsInterface
                    && unaryExpression.Type.GetInterfaces().Any(e => e == operand.Type)
                    || unaryExpression.Type.UnwrapNullableType() == operand.Type.UnwrapNullableType()
                    || unaryExpression.Type.UnwrapNullableType() == typeof(Enum))
                {
                    return sqlOperand!;
                }

                // Introduce explicit cast only if the target type is mapped else we need to client eval
                if (unaryExpression.Type == typeof(object)
                    || Dependencies.TypeMappingSource.FindMapping(unaryExpression.Type, Dependencies.Model) != null)
                {
                    sqlOperand = _sqlExpressionFactory.ApplyDefaultTypeMapping(sqlOperand);

                    return _sqlExpressionFactory.Convert(sqlOperand!, unaryExpression.Type);
                }

                break;

            case ExpressionType.Quote:
                return operand;
        }

        return QueryCompilationContext.NotTranslatedExpression;
    }

    private SqlExpression? TryBindMember(Expression? source, MemberIdentity member)
    {
        if (source is not EntityReferenceExpression entityReferenceExpression)
        {
            return null;
        }

        var entityType = entityReferenceExpression.EntityType;
        var property = member.MemberInfo != null
            ? entityType.FindProperty(member.MemberInfo)
            : entityType.FindProperty(member.Name!);

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

    private SqlExpression? BindProperty(EntityReferenceExpression entityReferenceExpression, IProperty property)
    {
        if (entityReferenceExpression.ParameterEntity != null)
        {
            var valueBufferExpression = Visit(entityReferenceExpression.ParameterEntity.ValueBufferExpression);
            if (valueBufferExpression is JsonQueryExpression jsonQueryExpression)
            {
                return jsonQueryExpression.BindProperty(property);
            }

            var entityProjectionExpression = (EntityProjectionExpression)valueBufferExpression;
            var propertyAccess = entityProjectionExpression.BindProperty(property);

            var entityType = entityReferenceExpression.EntityType;
            if (entityType.FindDiscriminatorProperty() != null
                || entityType.FindPrimaryKey() == null
                || entityType.GetRootType() != entityType
                || entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
            {
                return propertyAccess;
            }

            var table = entityType.GetViewOrTableMappings().SingleOrDefault(e => e.IsSplitEntityTypePrincipal ?? true)?.Table
                ?? entityType.GetDefaultMappings().Single().Table;
            if (!table.IsOptional(entityType))
            {
                return propertyAccess;
            }

            // this is optional dependent sharing table
            var nonPrincipalSharedNonPkProperties = entityType.GetNonPrincipalSharedNonPkProperties(table);
            if (nonPrincipalSharedNonPkProperties.Contains(property))
            {
                // The column is not being shared with principal side so we can always use directly
                return propertyAccess;
            }

            SqlExpression? condition = null;
            // Property is being shared with principal side, so we need to make it conditional access
            var allRequiredNonPkProperties =
                entityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
            if (allRequiredNonPkProperties.Count > 0)
            {
                condition = allRequiredNonPkProperties.Select(p => entityProjectionExpression.BindProperty(p))
                    .Select(c => (SqlExpression)_sqlExpressionFactory.NotEqual(c, _sqlExpressionFactory.Constant(null)))
                    .Aggregate((a, b) => _sqlExpressionFactory.AndAlso(a, b));
            }

            if (nonPrincipalSharedNonPkProperties.Count != 0
                && nonPrincipalSharedNonPkProperties.All(p => p.IsNullable))
            {
                // If all non principal shared properties are nullable then we need additional condition
                var atLeastOneNonNullValueInNullableColumnsCondition = nonPrincipalSharedNonPkProperties
                    .Select(p => entityProjectionExpression.BindProperty(p))
                    .Select(c => (SqlExpression)_sqlExpressionFactory.NotEqual(c, _sqlExpressionFactory.Constant(null)))
                    .Aggregate((a, b) => _sqlExpressionFactory.OrElse(a, b));

                condition = condition == null
                    ? atLeastOneNonNullValueInNullableColumnsCondition
                    : _sqlExpressionFactory.AndAlso(condition, atLeastOneNonNullValueInNullableColumnsCondition);
            }

            if (condition == null)
            {
                // if we cannot compute condition then we just return property access (and hope for the best)
                return propertyAccess;
            }

            return _sqlExpressionFactory.Case(
                new List<CaseWhenClause> { new(condition, propertyAccess) },
                elseResult: null);

            // We don't do above processing for subquery entity since it comes from after subquery which has been
            // single result so either it is regular entity or a collection which always have their own table.
        }

        if (entityReferenceExpression.SubqueryEntity != null)
        {
            var entityShaper = (EntityShaperExpression)entityReferenceExpression.SubqueryEntity.ShaperExpression;
            var subSelectExpression = (SelectExpression)entityReferenceExpression.SubqueryEntity.QueryExpression;

            var projectionBindingExpression = (ProjectionBindingExpression)entityShaper.ValueBufferExpression;
            var entityProjectionExpression = (EntityProjectionExpression)subSelectExpression.GetProjection(projectionBindingExpression);
            var innerProjection = entityProjectionExpression.BindProperty(property);
            subSelectExpression.ReplaceProjection(new List<Expression> { innerProjection });
            subSelectExpression.ApplyProjection();

            return new ScalarSubqueryExpression(subSelectExpression);
        }

        return null;
    }

    private bool TryTranslateAggregateMethodCall(
        MethodCallExpression methodCallExpression,
        [NotNullWhen(true)] out SqlExpression? translation)
    {
        if (methodCallExpression.Method.IsStatic
            && methodCallExpression.Arguments.Count > 0
            && methodCallExpression.Method.DeclaringType == typeof(Queryable))
        {
            var genericMethod = methodCallExpression.Method.IsGenericMethod
                ? methodCallExpression.Method.GetGenericMethodDefinition()
                : methodCallExpression.Method;
            var arguments = methodCallExpression.Arguments;
            var abortTranslation = false;
            if (TryTranslateAsEnumerableExpression(arguments[0], out var enumerableExpression))
            {
                switch (genericMethod.Name)
                {
                    case nameof(Queryable.Average)
                        when QueryableMethods.IsAverageWithoutSelector(genericMethod):
                    case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithoutSelector:
                    case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithoutSelector:
                    case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithoutSelector(genericMethod):
                    case nameof(Queryable.Count)
                        when genericMethod == QueryableMethods.CountWithoutPredicate:
                    case nameof(Queryable.LongCount)
                        when genericMethod == QueryableMethods.LongCountWithoutPredicate:
                        break;

                    case nameof(Queryable.Average)
                        when QueryableMethods.IsAverageWithSelector(genericMethod):
                    case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithSelector:
                    case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithSelector:
                    case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithSelector(genericMethod):
                        enumerableExpression = ProcessSelector(enumerableExpression, arguments[1].UnwrapLambdaFromQuote());
                        break;

                    case nameof(Queryable.Count)
                        when genericMethod == QueryableMethods.CountWithPredicate:
                    case nameof(Queryable.LongCount)
                        when genericMethod == QueryableMethods.LongCountWithPredicate:
                        var eep = ProcessPredicate(enumerableExpression, arguments[1].UnwrapLambdaFromQuote());
                        if (eep != null)
                        {
                            enumerableExpression = eep;
                        }
                        else
                        {
                            abortTranslation = true;
                        }

                        break;

                    default:
                        abortTranslation = true;
                        break;
                }

                if (!abortTranslation)
                {
                    translation = TranslateAggregateMethod(
                        enumerableExpression, methodCallExpression.Method, new List<SqlExpression>());

                    return translation != null;
                }
            }
        }

        translation = null;
        return false;
    }

    private bool TryTranslateAsEnumerableExpression(
        Expression? expression,
        [NotNullWhen(true)] out EnumerableExpression? enumerableExpression)
    {
        if (expression is RelationalGroupByShaperExpression relationalGroupByShaperExpression)
        {
            enumerableExpression = new EnumerableExpression(relationalGroupByShaperExpression.ElementSelector);
            return true;
        }

        if (expression is EnumerableExpression ee)
        {
            enumerableExpression = ee;
            return true;
        }

        if (expression is MethodCallExpression methodCallExpression
            && methodCallExpression.Method.IsStatic
            && methodCallExpression.Arguments.Count > 0
            && methodCallExpression.Method.DeclaringType == typeof(Queryable))
        {
            var genericMethod = methodCallExpression.Method.IsGenericMethod
                ? methodCallExpression.Method.GetGenericMethodDefinition()
                : methodCallExpression.Method;
            var arguments = methodCallExpression.Arguments;

            if (TryTranslateAsEnumerableExpression(arguments[0], out var enumerableSource))
            {
                switch (genericMethod.Name)
                {
                    case nameof(Queryable.AsQueryable)
                        when genericMethod == QueryableMethods.AsQueryable:
                        enumerableExpression = enumerableSource;
                        return true;

                    case nameof(Queryable.Distinct)
                        when genericMethod == QueryableMethods.Distinct:
                        if (enumerableSource.Selector is EntityShaperExpression entityShaperExpression
                            && entityShaperExpression.EntityType.FindPrimaryKey() != null)
                        {
                            enumerableExpression = enumerableSource;
                            return true;
                        }

                        if (!enumerableSource.IsDistinct)
                        {
                            enumerableExpression = enumerableSource.ApplyDistinct();
                            return true;
                        }

                        break;

                    case nameof(Queryable.OrderBy)
                        when genericMethod == QueryableMethods.OrderBy:
                        enumerableExpression = ProcessOrderByThenBy(
                            enumerableSource, arguments[1].UnwrapLambdaFromQuote(), thenBy: false, ascending: true);
                        return enumerableExpression != null;

                    case nameof(Queryable.OrderByDescending)
                        when genericMethod == QueryableMethods.OrderByDescending:
                        enumerableExpression = ProcessOrderByThenBy(
                            enumerableSource, arguments[1].UnwrapLambdaFromQuote(), thenBy: false, ascending: false);
                        return enumerableExpression != null;

                    case nameof(Queryable.ThenBy)
                        when genericMethod == QueryableMethods.ThenBy:
                        enumerableExpression = ProcessOrderByThenBy(
                            enumerableSource, arguments[1].UnwrapLambdaFromQuote(), thenBy: true, ascending: true);
                        return enumerableExpression != null;

                    case nameof(Queryable.ThenByDescending)
                        when genericMethod == QueryableMethods.ThenByDescending:
                        enumerableExpression = ProcessOrderByThenBy(
                            enumerableSource, arguments[1].UnwrapLambdaFromQuote(), thenBy: true, ascending: false);
                        return enumerableExpression != null;

                    case nameof(Queryable.Select)
                        when genericMethod == QueryableMethods.Select:
                        enumerableExpression = ProcessSelector(enumerableSource, arguments[1].UnwrapLambdaFromQuote());
                        return true;

                    case nameof(Queryable.Where)
                        when genericMethod == QueryableMethods.Where:
                        enumerableExpression = ProcessPredicate(enumerableSource, arguments[1].UnwrapLambdaFromQuote());
                        return enumerableExpression != null;
                }
            }
        }

        enumerableExpression = null;
        return false;
    }

    private SqlExpression? TranslateAggregateMethod(
        EnumerableExpression enumerableExpression,
        MethodInfo method,
        List<SqlExpression> scalarArguments)
    {
        _throwForNotTranslatedEfProperty = false;
        var selector = TranslateInternal(enumerableExpression.Selector);
        _throwForNotTranslatedEfProperty = true;
        if (selector != null)
        {
            enumerableExpression = enumerableExpression.ApplySelector(selector);
        }

        return Dependencies.AggregateMethodCallTranslatorProvider.Translate(
            _model, method, enumerableExpression, scalarArguments, _queryCompilationContext.Logger);
    }

    private static Expression RemapLambda(EnumerableExpression enumerableExpression, LambdaExpression lambdaExpression)
        => ReplacingExpressionVisitor.Replace(lambdaExpression.Parameters[0], enumerableExpression.Selector, lambdaExpression.Body);

    private static EnumerableExpression ProcessSelector(EnumerableExpression enumerableExpression, LambdaExpression lambdaExpression)
        => enumerableExpression.ApplySelector(RemapLambda(enumerableExpression, lambdaExpression));

    private EnumerableExpression? ProcessOrderByThenBy(
        EnumerableExpression enumerableExpression,
        LambdaExpression lambdaExpression,
        bool thenBy,
        bool ascending)
    {
        var lambdaBody = RemapLambda(enumerableExpression, lambdaExpression);
        var keySelector = TranslateInternal(lambdaBody);
        if (keySelector == null)
        {
            return null;
        }

        var orderingExpression = new OrderingExpression(keySelector, ascending);
        return thenBy
            ? enumerableExpression.AppendOrdering(orderingExpression)
            : enumerableExpression.ApplyOrdering(orderingExpression);
    }

    private EnumerableExpression? ProcessPredicate(EnumerableExpression enumerableExpression, LambdaExpression lambdaExpression)
    {
        var lambdaBody = RemapLambda(enumerableExpression, lambdaExpression);
        var predicate = TranslateInternal(lambdaBody);
        return predicate == null ? null : enumerableExpression.ApplyPredicate(predicate);
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
                (l, r) => Infrastructure.ExpressionExtensions.CreateEqualsExpression(l, r))
            .Aggregate((a, b) => Expression.AndAlso(a, b));
    }

    private static Expression GetConstantOrNotTranslated(Expression expression)
        => CanEvaluate(expression)
            ? new SqlConstantExpression(
                Expression.Constant(
                    Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile().Invoke(),
                    expression.Type),
                null)
            : QueryCompilationContext.NotTranslatedExpression;

    private bool TryRewriteContainsEntity(Expression source, Expression item, [NotNullWhen(true)] out Expression? result)
    {
        result = null;

        if (item is not EntityReferenceExpression itemEntityReference)
        {
            return false;
        }

        var entityType = itemEntityReference.EntityType;
        var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;
        if (primaryKeyProperties == null)
        {
            throw new InvalidOperationException(
                CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                    nameof(Queryable.Contains), entityType.DisplayName()));
        }

        if (primaryKeyProperties.Count > 1)
        {
            throw new InvalidOperationException(
                CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported(
                    nameof(Queryable.Contains), entityType.DisplayName()));
        }

        var property = primaryKeyProperties[0];
        Expression rewrittenSource;
        switch (source)
        {
            case SqlConstantExpression sqlConstantExpression:
                var values = (IEnumerable)sqlConstantExpression.Value!;
                var propertyValueList =
                    (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(property.ClrType.MakeNullable()))!;
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
                        ParameterListValueExtractorMethod.MakeGenericMethod(entityType.ClrType, property.ClrType.MakeNullable()),
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(sqlParameterExpression.Name, typeof(string)),
                        Expression.Constant(property, typeof(IProperty))),
                    QueryCompilationContext.QueryContextParameter);

                var newParameterName =
                    $"{RuntimeParameterPrefix}"
                    + $"{sqlParameterExpression.Name[QueryCompilationContext.QueryParameterPrefix.Length..]}_{property.Name}";

                rewrittenSource = _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                break;

            default:
                return false;
        }

        result = Visit(
            Expression.Call(
                EnumerableMethods.Contains.MakeGenericMethod(property.ClrType.MakeNullable()),
                rewrittenSource,
                CreatePropertyAccessExpression(item, property)));

        return true;
    }

    private bool TryRewriteEntityEquality(
        ExpressionType nodeType,
        Expression left,
        Expression right,
        bool equalsMethod,
        [NotNullWhen(true)] out Expression? result)
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
            var nonNullEntityReference = (IsNullSqlConstantExpression(left) ? rightEntityReference : leftEntityReference)!;
            var nullComparedEntityType = nonNullEntityReference.EntityType;
            var nullComparedEntityTypePrimaryKeyProperties = nullComparedEntityType.FindPrimaryKey()?.Properties;
            if (nullComparedEntityTypePrimaryKeyProperties == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                        nodeType == ExpressionType.Equal
                            ? equalsMethod ? nameof(object.Equals) : "=="
                            : equalsMethod
                                ? "!" + nameof(object.Equals)
                                : "!=",
                        nullComparedEntityType.DisplayName()));
            }

            if (nullComparedEntityType.GetRootType() == nullComparedEntityType
                && nullComparedEntityType.GetMappingStrategy() != RelationalAnnotationNames.TpcMappingStrategy)
            {
                var table = nullComparedEntityType.GetViewOrTableMappings().SingleOrDefault()?.Table
                    ?? nullComparedEntityType.GetDefaultMappings().Single().Table;
                if (table.IsOptional(nullComparedEntityType))
                {
                    Expression? condition = null;
                    // Optional dependent sharing table
                    var requiredNonPkProperties = nullComparedEntityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
                    if (requiredNonPkProperties.Count > 0)
                    {
                        condition = requiredNonPkProperties.Select(
                                p => Infrastructure.ExpressionExtensions.CreateEqualsExpression(
                                    CreatePropertyAccessExpression(nonNullEntityReference, p),
                                    Expression.Constant(null, p.ClrType.MakeNullable()),
                                    nodeType != ExpressionType.Equal))
                            .Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r));
                    }

                    var allNonPrincipalSharedNonPkProperties = nullComparedEntityType.GetNonPrincipalSharedNonPkProperties(table);
                    // We don't need condition for nullable property if there exist at least one required property which is non shared.
                    if (allNonPrincipalSharedNonPkProperties.Count != 0
                        && allNonPrincipalSharedNonPkProperties.All(p => p.IsNullable))
                    {
                        var atLeastOneNonNullValueInNullablePropertyCondition = allNonPrincipalSharedNonPkProperties
                            .Select(
                                p => Infrastructure.ExpressionExtensions.CreateEqualsExpression(
                                    CreatePropertyAccessExpression(nonNullEntityReference, p),
                                    Expression.Constant(null, p.ClrType.MakeNullable()),
                                    nodeType != ExpressionType.Equal))
                            .Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r));

                        condition = condition == null
                            ? atLeastOneNonNullValueInNullablePropertyCondition
                            : nodeType == ExpressionType.Equal
                                ? Expression.OrElse(condition, atLeastOneNonNullValueInNullablePropertyCondition)
                                : Expression.AndAlso(condition, atLeastOneNonNullValueInNullablePropertyCondition);
                    }

                    if (condition != null)
                    {
                        result = Visit(condition);
                        return true;
                    }

                    result = null;
                    return false;
                }
            }

            result = Visit(
                nullComparedEntityTypePrimaryKeyProperties.Select(
                        p => Infrastructure.ExpressionExtensions.CreateEqualsExpression(
                            CreatePropertyAccessExpression(nonNullEntityReference, p),
                            Expression.Constant(null, p.ClrType.MakeNullable()),
                            nodeType != ExpressionType.Equal))
                    .Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r)));

            return true;
        }

        var leftEntityType = leftEntityReference?.EntityType;
        var rightEntityType = rightEntityReference?.EntityType;
        var entityType = leftEntityType ?? rightEntityType;

        Check.DebugAssert(entityType != null, "At least one side should be entityReference so entityType should be non-null.");

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
            throw new InvalidOperationException(
                CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                    nodeType == ExpressionType.Equal
                        ? equalsMethod ? nameof(object.Equals) : "=="
                        : equalsMethod
                            ? "!" + nameof(object.Equals)
                            : "!=",
                    entityType.DisplayName()));
        }

        if (primaryKeyProperties.Count > 1
            && (leftEntityReference?.SubqueryEntity != null
                || rightEntityReference?.SubqueryEntity != null))
        {
            throw new InvalidOperationException(
                CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported(
                    nodeType == ExpressionType.Equal
                        ? equalsMethod ? nameof(object.Equals) : "=="
                        : equalsMethod
                            ? "!" + nameof(object.Equals)
                            : "!=",
                    entityType.DisplayName()));
        }

        result = Visit(
            primaryKeyProperties.Select(
                    p => Infrastructure.ExpressionExtensions.CreateEqualsExpression(
                        CreatePropertyAccessExpression(left, p),
                        CreatePropertyAccessExpression(right, p),
                        nodeType != ExpressionType.Equal))
                .Aggregate(
                    (l, r) => nodeType == ExpressionType.Equal
                        ? Expression.AndAlso(l, r)
                        : Expression.OrElse(l, r)));

        return true;
    }

    private Expression CreatePropertyAccessExpression(Expression target, IProperty property)
    {
        switch (target)
        {
            case SqlConstantExpression sqlConstantExpression:
                return Expression.Constant(
                    sqlConstantExpression.Value is null
                        ? null
                        : property.GetGetter().GetClrValue(sqlConstantExpression.Value),
                    property.ClrType.MakeNullable());

            case SqlParameterExpression sqlParameterExpression
                when sqlParameterExpression.Name.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal):
                var lambda = Expression.Lambda(
                    Expression.Call(
                        ParameterValueExtractorMethod.MakeGenericMethod(property.ClrType.MakeNullable()),
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(sqlParameterExpression.Name, typeof(string)),
                        Expression.Constant(property, typeof(IProperty))),
                    QueryCompilationContext.QueryContextParameter);

                var newParameterName =
                    $"{RuntimeParameterPrefix}"
                    + $"{sqlParameterExpression.Name[QueryCompilationContext.QueryParameterPrefix.Length..]}_{property.Name}";

                return _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);

            case MemberInitExpression memberInitExpression
                when memberInitExpression.Bindings.SingleOrDefault(
                    mb => mb.Member.Name == property.Name) is MemberAssignment memberAssignment:
                return memberAssignment.Expression;

            default:
                return target.CreateEFPropertyExpression(property);
        }
    }

    private static T? ParameterValueExtractor<T>(QueryContext context, string baseParameterName, IProperty property)
    {
        var baseParameter = context.ParameterValues[baseParameterName];
        return baseParameter == null ? (T?)(object?)null : (T?)property.GetGetter().GetClrValue(baseParameter);
    }

    private static List<TProperty?>? ParameterListValueExtractor<TEntity, TProperty>(
        QueryContext context,
        string baseParameterName,
        IProperty property)
    {
        if (context.ParameterValues[baseParameterName] is not IEnumerable<TEntity> baseListParameter)
        {
            return null;
        }

        var getter = property.GetGetter();
        return baseListParameter.Select(e => e != null ? (TProperty?)getter.GetClrValue(e) : (TProperty?)(object?)null).ToList();
    }

    private static bool CanEvaluate(Expression expression)
    {
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (expression)
#pragma warning restore IDE0066 // Convert switch statement to expression
        {
            case ConstantExpression:
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
    private static bool TranslationFailed(Expression? original, Expression? translation, out SqlExpression? castTranslation)
    {
        if (original != null
            && translation is not SqlExpression)
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

        public EntityShaperExpression? ParameterEntity { get; }
        public ShapedQueryExpression? SubqueryEntity { get; }
        public IEntityType EntityType { get; }

        public override Type Type
            => EntityType.ClrType;

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        public Expression Convert(Type type)
        {
            if (type == typeof(object) // Ignore object conversion
                || type.IsAssignableFrom(Type)) // Ignore casting to base type/interface
            {
                return this;
            }

            var derivedEntityType = EntityType.GetDerivedTypes().FirstOrDefault(et => et.ClrType == type);

            return derivedEntityType == null
                ? QueryCompilationContext.NotTranslatedExpression
                : new EntityReferenceExpression(this, derivedEntityType);
        }
    }

    private sealed class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is SqlExpression { TypeMapping: null } sqlExpression
                && extensionExpression is not SqlFragmentExpression
                ? throw new InvalidOperationException(RelationalStrings.NullTypeMappingInSqlTree(sqlExpression.Print()))
                : base.VisitExtension(extensionExpression);
    }
}

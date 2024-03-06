// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
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

    private static readonly List<MethodInfo> SingleResultMethodInfos =
    [
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
        QueryableMethods.LastOrDefaultWithoutPredicate,
        QueryableMethods.ElementAt,
        QueryableMethods.ElementAtOrDefault
    ];

    private static readonly MethodInfo ParameterValueExtractorMethod =
        typeof(RelationalSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor))!;

    private static readonly MethodInfo ParameterListValueExtractorMethod =
        typeof(RelationalSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterListValueExtractor))!;

    private static readonly MethodInfo StringEqualsWithStringComparison
        = typeof(string).GetRuntimeMethod(nameof(string.Equals), [typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo StringEqualsWithStringComparisonStatic
        = typeof(string).GetRuntimeMethod(nameof(string.Equals), [typeof(string), typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo LeastMethodInfo
        = typeof(RelationalDbFunctionsExtensions).GetMethod(nameof(RelationalDbFunctionsExtensions.Least))!;

    private static readonly MethodInfo GreatestMethodInfo
        = typeof(RelationalDbFunctionsExtensions).GetMethod(nameof(RelationalDbFunctionsExtensions.Greatest))!;

    private static readonly MethodInfo GetTypeMethodInfo = typeof(object).GetTypeInfo().GetDeclaredMethod(nameof(GetType))!;

    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly IModel _model;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;

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
    /// <param name="applyDefaultTypeMapping">
    ///     Whether to apply the default type mapping on the top-most element if it has none. Defaults to <see langword="true" />.
    /// </param>
    /// <returns>A SQL translation of the given expression.</returns>
    public virtual SqlExpression? Translate(Expression expression, bool applyDefaultTypeMapping = true)
    {
        TranslationErrorDetails = null;

        return TranslateInternal(expression, applyDefaultTypeMapping) as SqlExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual Expression? TranslateProjection(Expression expression, bool applyDefaultTypeMapping = true)
    {
        TranslationErrorDetails = null;

        return TranslateInternal(expression, applyDefaultTypeMapping) switch
        {
            // This is the case of a structural type getting projected out via Select (possibly also an owned entity one day, if we stop
            // expanding them in pre-visitation)
            StructuralTypeReferenceExpression { Parameter: StructuralTypeShaperExpression shaper }
                => shaper,

            StructuralTypeReferenceExpression { Subquery: not null }
                => null, // TODO: think about this - probably unsupported (if so, message)

            SqlExpression s => s,

            _ => null
        };
    }

    private Expression? TranslateInternal(Expression expression, bool applyDefaultTypeMapping = true)
    {
        var result = Visit(expression);

        if (result is SqlExpression translation)
        {
            if (translation is SqlUnaryExpression { OperatorType: ExpressionType.Convert } sqlUnaryExpression
                && sqlUnaryExpression.Type == typeof(object))
            {
                translation = sqlUnaryExpression.Operand;
            }

            if (applyDefaultTypeMapping)
            {
                translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(translation);

                if (translation.TypeMapping == null)
                {
                    // The return type is not-mappable hence return null
                    return null;
                }
            }

            return translation;
        }

        return result;
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
            && binaryExpression is { Left: NewArrayExpression, NodeType: ExpressionType.Equal })
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

        if (binaryExpression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual
            && (left.IsNullConstantExpression() || right.IsNullConstantExpression()))
        {
            var nonNullExpression = left.IsNullConstantExpression() ? right : left;
            if (nonNullExpression is MethodCallExpression nonNullMethodCallExpression
                && nonNullMethodCallExpression.Method.DeclaringType == typeof(Queryable)
                && nonNullMethodCallExpression.Method.IsGenericMethod
                && SingleResultMethodInfos.Contains(nonNullMethodCallExpression.Method.GetGenericMethodDefinition()))
            {
                var source = nonNullMethodCallExpression.Arguments[0];
                var genericMethod = nonNullMethodCallExpression.Method.GetGenericMethodDefinition();
                if (genericMethod == QueryableMethods.FirstWithPredicate
                    || genericMethod == QueryableMethods.FirstOrDefaultWithPredicate
                    || genericMethod == QueryableMethods.SingleWithPredicate
                    || genericMethod == QueryableMethods.SingleOrDefaultWithPredicate
                    || genericMethod == QueryableMethods.LastWithPredicate
                    || genericMethod == QueryableMethods.LastOrDefaultWithPredicate)
                {
                    source = Expression.Call(
                        QueryableMethods.Where.MakeGenericMethod(source.Type.GetSequenceType()),
                        source,
                        nonNullMethodCallExpression.Arguments[1]);
                }
                else if ((genericMethod == QueryableMethods.ElementAt || genericMethod == QueryableMethods.ElementAtOrDefault)
                         && nonNullMethodCallExpression.Arguments[1] is not ConstantExpression { Value: 0 })
                {
                    source = Expression.Call(
                        QueryableMethods.Skip.MakeGenericMethod(source.Type.GetSequenceType()),
                        source,
                        nonNullMethodCallExpression.Arguments[1]);
                }

                var translatedSubquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(source);
                if (translatedSubquery != null)
                {
                    var projection = translatedSubquery.ShaperExpression;
                    if (projection is NewExpression
                        || RemoveConvert(projection) is StructuralTypeShaperExpression { IsNullable: false }
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

        if (binaryExpression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual
            // Visited expression could be null, We need to pass MemberInitExpression
            && TryRewriteStructuralTypeEquality(
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

        Expression ProcessGetType(StructuralTypeReferenceExpression typeReference, Type comparisonType, bool match)
        {
            if (typeReference.StructuralType is not IEntityType entityType
                || (entityType.BaseType == null
                    && !entityType.GetDirectlyDerivedTypes().Any()))
            {
                // No hierarchy
                return _sqlExpressionFactory.Constant((typeReference.StructuralType.ClrType == comparisonType) == match);
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
                if (typeReference.Subquery != null)
                {
                    var shaper = (StructuralTypeShaperExpression)typeReference.Subquery.ShaperExpression;
                    var projection = (StructuralTypeProjectionExpression)Visit(shaper.ValueBufferExpression);
                    var subSelectExpression = (SelectExpression)typeReference.Subquery.QueryExpression;

                    var predicate = GeneratePredicateTpt(projection);

                    subSelectExpression.ApplyPredicate(predicate);
                    subSelectExpression.ReplaceProjection(new List<Expression>());
                    subSelectExpression.ApplyProjection();
                    if (subSelectExpression.Limit == null
                        && subSelectExpression.Offset == null)
                    {
                        subSelectExpression.ClearOrdering();
                    }

                    return _sqlExpressionFactory.Exists(subSelectExpression);
                }

                if (typeReference.Parameter != null)
                {
                    var projection = (StructuralTypeProjectionExpression)Visit(typeReference.Parameter.ValueBufferExpression);
                    return GeneratePredicateTpt(projection);
                }

                SqlExpression GeneratePredicateTpt(StructuralTypeProjectionExpression projection)
                {
                    if (projection.DiscriminatorExpression is CaseExpression caseExpression)
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
                            projection.DiscriminatorExpression!,
                            _sqlExpressionFactory.Constant(discriminatorValue))
                        : _sqlExpressionFactory.NotEqual(
                            projection.DiscriminatorExpression!,
                            _sqlExpressionFactory.Constant(discriminatorValue));
                }
            }
            else
            {
                var discriminatorColumn = BindProperty(typeReference, discriminatorProperty);
                return match
                    ? _sqlExpressionFactory.Equal(
                        discriminatorColumn,
                        _sqlExpressionFactory.Constant(derivedType.GetDiscriminatorValue()!))
                    : _sqlExpressionFactory.NotEqual(
                        discriminatorColumn,
                        _sqlExpressionFactory.Constant(derivedType.GetDiscriminatorValue()!));
            }

            return QueryCompilationContext.NotTranslatedExpression;
        }

        bool IsGetTypeMethodCall(Expression expression, out StructuralTypeReferenceExpression? typeReference)
        {
            typeReference = null;
            if (expression is not MethodCallExpression methodCallExpression
                || methodCallExpression.Method != GetTypeMethodInfo)
            {
                return false;
            }

            typeReference = Visit(methodCallExpression.Object) as StructuralTypeReferenceExpression;
            return typeReference != null;
        }

        static bool IsTypeConstant(Expression expression, out Type? type)
        {
            type = null;
            if (expression is not UnaryExpression
                {
                    NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked, Operand: ConstantExpression constantExpression
                })
            {
                return false;
            }

            type = constantExpression.Value as Type;
            return type != null;
        }

        static bool TryUnwrapConvertToObject(Expression expression, out Expression? operand)
        {
            if (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } convertExpression
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
        => new SqlConstantExpression(constantExpression.Value, constantExpression.Type, typeMapping: null);

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case StructuralTypeProjectionExpression:
            case StructuralTypeReferenceExpression:
            case SqlExpression:
            case EnumerableExpression:
            case JsonQueryExpression:
                return extensionExpression;

            case StructuralTypeShaperExpression shaper:
                return new StructuralTypeReferenceExpression(shaper);

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
                if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
                {
                    convertedType = unaryExpression.Type;
                    innerExpression = unaryExpression.Operand;
                }

                if (innerExpression is StructuralTypeShaperExpression ese
                    && (convertedType == null
                        || convertedType.IsAssignableFrom(ese.Type)))
                {
                    return new StructuralTypeReferenceExpression(shapedQueryExpression.UpdateShaperExpression(innerExpression));
                }

                if (innerExpression is ProjectionBindingExpression pbe
                    && (convertedType == null
                        || convertedType.MakeNullable() == innerExpression.Type))
                {
                    mappedProjectionBindingExpression = pbe;
                }

                if (mappedProjectionBindingExpression == null
                    && shaperExpression is BlockExpression
                    {
                        Expressions: [BinaryExpression { NodeType: ExpressionType.Assign, Right: ProjectionBindingExpression pbe2 }, _]
                    })
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

            // We have e.g. an array parameter inside a Where clause; this is represented as a QueryableParameterQueryRootExpression so
            // that we can translate queryable operators over it (query root in subquery context), but in normal SQL translation context
            // we just unwrap the query root expression to get the parameter out.
            case ParameterQueryRootExpression queryableParameterQueryRootExpression:
                return Visit(queryableParameterQueryRootExpression.ParameterExpression);

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

        return TryBindMember(innerExpression, MemberIdentity.Create(memberExpression.Member), out var expression)
            ? expression
            : (TranslationFailed(memberExpression.Expression, innerExpression, out var sqlInnerExpression)
                ? QueryCompilationContext.NotTranslatedExpression
                : Dependencies.MemberTranslatorProvider.Translate(
                    sqlInnerExpression, memberExpression.Member, memberExpression.Type, _queryCompilationContext.Logger))
            ?? QueryCompilationContext.NotTranslatedExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        => TryEvaluateToConstant(memberInitExpression, out var sqlConstantExpression)
            ? sqlConstantExpression
            : QueryCompilationContext.NotTranslatedExpression;

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        // EF.Property case
        if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
        {
            if (TryBindMember(Visit(source), MemberIdentity.Create(propertyName), out var result))
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
            && TryBindMember(Visit(source), MemberIdentity.Create(propertyName), out var indexerResult))
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

            if (TryRewriteStructuralTypeEquality(
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
                scalarArguments = [rightSql];
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

            if (TryRewriteStructuralTypeEquality(
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
                scalarArguments = [leftSql, rightSql];
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
                scalarArguments = [sqlEnumerable, sqlItem];
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
                scalarArguments = [sqlItem];
            }
            else
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }
        // Translate EF.Functions.Greatest/Least.
        // These are here rather than in a MethodTranslator since the parameter is an array, and that's not supported in regular
        // translation.
        else if (method.DeclaringType == typeof(RelationalDbFunctionsExtensions)
                 && method.IsGenericMethod
                 && method.GetGenericMethodDefinition() is var genericMethodDefinition
                 && (genericMethodDefinition == LeastMethodInfo || genericMethodDefinition == GreatestMethodInfo)
                 && methodCallExpression.Arguments[1] is NewArrayExpression newArray)
        {
            var values = newArray.Expressions;
            var translatedValues = new SqlExpression[values.Count];

            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                var visitedValue = Visit(value);

                if (TranslationFailed(value, visitedValue, out var translatedValue))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                translatedValues[i] = translatedValue!;
            }

            var elementClrType = newArray.Type.GetElementType()!;

            if (genericMethodDefinition == LeastMethodInfo
                && _sqlExpressionFactory.TryCreateLeast(translatedValues, elementClrType, out var leastExpression))
            {
                return leastExpression;
            }

            if (genericMethodDefinition == GreatestMethodInfo
                && _sqlExpressionFactory.TryCreateGreatest(translatedValues, elementClrType, out var greatestExpression))
            {
                return greatestExpression;
            }

            throw new UnreachableException();
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

            scalarArguments = [];
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
        => TryEvaluateToConstant(newExpression, out var sqlConstantExpression)
            ? sqlConstantExpression
            : QueryCompilationContext.NotTranslatedExpression;

    /// <inheritdoc />
    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
    {
        if (TryEvaluateToConstant(newArrayExpression, out var sqlConstantExpression))
        {
            return sqlConstantExpression;
        }

        AddTranslationErrorDetails(RelationalStrings.CannotTranslateNonConstantNewArrayExpression(newArrayExpression.Print()));
        return QueryCompilationContext.NotTranslatedExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitParameter(ParameterExpression parameterExpression)
        => parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal) == true
            ? new SqlParameterExpression(parameterExpression.Name, parameterExpression.Type, null)
            : throw new InvalidOperationException(CoreStrings.TranslationFailed(parameterExpression.Print()));

    /// <inheritdoc />
    protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
    {
        var innerExpression = Visit(typeBinaryExpression.Expression);

        if (typeBinaryExpression.NodeType != ExpressionType.TypeIs
            || innerExpression is not StructuralTypeReferenceExpression typeReference)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        if (typeReference.StructuralType is not IEntityType entityType)
        {
            return Expression.Constant(typeReference.StructuralType.ClrType == typeBinaryExpression.TypeOperand);
        }

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
            if (typeReference.Subquery != null)
            {
                var shaper = (StructuralTypeShaperExpression)typeReference.Subquery.ShaperExpression;
                var projection = (StructuralTypeProjectionExpression)Visit(shaper.ValueBufferExpression);
                var subSelectExpression = (SelectExpression)typeReference.Subquery.QueryExpression;

                var predicate = GeneratePredicateTpt(projection);

                subSelectExpression.ApplyPredicate(predicate);
                subSelectExpression.ReplaceProjection(new List<Expression>());
                subSelectExpression.ApplyProjection();
                if (subSelectExpression.Limit == null
                    && subSelectExpression.Offset == null)
                {
                    subSelectExpression.ClearOrdering();
                }

                return _sqlExpressionFactory.Exists(subSelectExpression);
            }

            if (typeReference.Parameter != null)
            {
                var typeProjection = (StructuralTypeProjectionExpression)Visit(typeReference.Parameter.ValueBufferExpression);

                return GeneratePredicateTpt(typeProjection);
            }

            SqlExpression GeneratePredicateTpt(StructuralTypeProjectionExpression entityProjectionExpression)
            {
                if (entityProjectionExpression.DiscriminatorExpression is CaseExpression caseExpression)
                {
                    var matchingCaseWhenClauses = caseExpression.WhenClauses
                        .Where(wc => discriminatorValues.Contains((string)((SqlConstantExpression)wc.Result).Value!))
                        .ToList();

                    return matchingCaseWhenClauses.Count == 1
                        ? matchingCaseWhenClauses[0].Test
                        : matchingCaseWhenClauses.Select(e => e.Test)
                            .Aggregate(_sqlExpressionFactory.OrElse);
                }

                return discriminatorValues.Count == 1
                    ? _sqlExpressionFactory.Equal(
                        entityProjectionExpression.DiscriminatorExpression!,
                        _sqlExpressionFactory.Constant(discriminatorValues[0]))
                    : _sqlExpressionFactory.In(
                        entityProjectionExpression.DiscriminatorExpression!,
                        discriminatorValues.Select(d => _sqlExpressionFactory.Constant(d)).ToArray());
            }
        }
        else
        {
            if (!derivedType.GetRootType().GetIsDiscriminatorMappingComplete()
                || !derivedType.GetAllBaseTypesInclusiveAscending()
                    .All(e => (e == derivedType || e.IsAbstract()) && !HasSiblings(e)))
            {
                var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();
                var discriminatorColumn = BindProperty(typeReference, discriminatorProperty);
                return concreteEntityTypes.Count == 1
                    ? _sqlExpressionFactory.Equal(
                        discriminatorColumn,
                        _sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue(), discriminatorColumn.Type))
                    : _sqlExpressionFactory.In(
                        discriminatorColumn,
                        concreteEntityTypes
                            .Select(et => _sqlExpressionFactory.Constant(et.GetDiscriminatorValue(), discriminatorColumn.Type))
                            .ToArray());
            }

            return _sqlExpressionFactory.Constant(true);
        }

        return QueryCompilationContext.NotTranslatedExpression;

        static bool HasSiblings(IEntityType entityType)
            => entityType.BaseType?.GetDirectlyDerivedTypes().Any(i => i != entityType) == true;
    }

    /// <inheritdoc />
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        var operand = Visit(unaryExpression.Operand);

        if (operand is StructuralTypeReferenceExpression typeReference
            && unaryExpression.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs)
        {
            return typeReference.Convert(unaryExpression.Type);
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

    private bool TryBindMember(
        Expression? source,
        MemberIdentity member,
        [NotNullWhen(true)] out Expression? expression)
        => TryBindMember(source, member, out expression, out _);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual bool TryBindMember(
        Expression? source,
        MemberIdentity member,
        [NotNullWhen(true)] out Expression? expression,
        [NotNullWhen(true)] out IPropertyBase? property)
    {
        if (source is not StructuralTypeReferenceExpression typeReference)
        {
            expression = null;
            property = null;
            return false;
        }

        var structuralType = typeReference.StructuralType;

        var regularProperty = member.MemberInfo != null
            ? structuralType.FindProperty(member.MemberInfo)
            : structuralType.FindProperty(member.Name!);

        if (regularProperty != null)
        {
            expression = BindProperty(typeReference, regularProperty);
            property = regularProperty;
            return true;
        }

        var complexProperty = member.MemberInfo != null
            ? structuralType.FindComplexProperty(member.MemberInfo)
            : structuralType.FindComplexProperty(member.Name!);

        if (complexProperty is not null)
        {
            expression = BindComplexProperty(typeReference, complexProperty);
            property = complexProperty;
            return true;
        }

        AddTranslationErrorDetails(
            CoreStrings.QueryUnableToTranslateMember(
                member.Name,
                typeReference.StructuralType.DisplayName()));

        expression = null;
        property = null;
        return false;
    }

    private SqlExpression BindProperty(StructuralTypeReferenceExpression typeReference, IProperty property)
    {
        switch (typeReference)
        {
            case { Parameter: StructuralTypeShaperExpression shaper }:
            {
                var valueBufferExpression = Visit(shaper.ValueBufferExpression);
                if (valueBufferExpression is JsonQueryExpression jsonQueryExpression)
                {
                    return jsonQueryExpression.BindProperty(property);
                }

                var projection = (StructuralTypeProjectionExpression)valueBufferExpression;
                var propertyAccess = projection.BindProperty(property);

                if (typeReference.StructuralType is not IEntityType entityType
                    || entityType.FindDiscriminatorProperty() != null
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
                    condition = allRequiredNonPkProperties.Select(p => projection.BindProperty(p))
                        .Select(c => (SqlExpression)_sqlExpressionFactory.NotEqual(c, _sqlExpressionFactory.Constant(null, c.Type)))
                        .Aggregate((a, b) => _sqlExpressionFactory.AndAlso(a, b));
                }

                if (nonPrincipalSharedNonPkProperties.Count != 0
                    && nonPrincipalSharedNonPkProperties.All(p => p.IsNullable))
                {
                    // If all non principal shared properties are nullable then we need additional condition
                    var atLeastOneNonNullValueInNullableColumnsCondition = nonPrincipalSharedNonPkProperties
                        .Select(p => projection.BindProperty(p))
                        .Select(c => (SqlExpression)_sqlExpressionFactory.NotEqual(c, _sqlExpressionFactory.Constant(null, c.Type)))
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

            case { Subquery: ShapedQueryExpression subquery }:
            {
                var entityShaper = (StructuralTypeShaperExpression)subquery.ShaperExpression;
                var subSelectExpression = (SelectExpression)subquery.QueryExpression;

                var projectionBindingExpression = (ProjectionBindingExpression)entityShaper.ValueBufferExpression;
                var projection = (StructuralTypeProjectionExpression)subSelectExpression.GetProjection(projectionBindingExpression);
                var innerProjection = projection.BindProperty(property);
                subSelectExpression.ReplaceProjection(new List<Expression> { innerProjection });
                subSelectExpression.ApplyProjection();

                return new ScalarSubqueryExpression(subSelectExpression);
            }

            default:
                throw new UnreachableException();
        }
    }

    private StructuralTypeReferenceExpression BindComplexProperty(
        StructuralTypeReferenceExpression typeReference,
        IComplexProperty complexProperty)
    {
        switch (typeReference)
        {
            case { Parameter: StructuralTypeShaperExpression shaper }:
                var projection = (StructuralTypeProjectionExpression)Visit(shaper.ValueBufferExpression);

                // TODO: Move all this logic into StructuralTypeProjectionExpression
                Check.DebugAssert(projection.IsNullable == shaper.IsNullable, "Nullability mismatch");
                return new StructuralTypeReferenceExpression(projection.BindComplexProperty(complexProperty));

            case { Subquery: ShapedQueryExpression }:
                throw new InvalidOperationException(); // TODO: Figure this out; do we support it?

            default:
                throw new UnreachableException();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual bool TryTranslateAggregateMethodCall(
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
                        enumerableExpression, methodCallExpression.Method, []);

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

        if (expression is MethodCallExpression { Method.IsStatic: true, Arguments.Count: > 0 } methodCallExpression
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
                        if (enumerableSource.Selector is StructuralTypeShaperExpression { StructuralType: IEntityType entityType }
                            && entityType.FindPrimaryKey() != null)
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
        if (selector is SqlExpression)
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
        if (TranslateInternal(lambdaBody) is not SqlExpression keySelector)
        {
            return null;
        }

        var orderingExpression = new OrderingExpression(keySelector, ascending);
        return thenBy
            ? enumerableExpression.AppendOrdering(orderingExpression)
            : enumerableExpression.ApplyOrdering(orderingExpression);
    }

    private EnumerableExpression? ProcessPredicate(EnumerableExpression enumerableExpression, LambdaExpression lambdaExpression)
        => TranslateInternal(RemapLambda(enumerableExpression, lambdaExpression)) is SqlExpression predicate
            ? enumerableExpression.ApplyPredicate(predicate)
            : null;

    private static Expression TryRemoveImplicitConvert(Expression expression)
    {
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression)
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
        => expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression
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
            .Aggregate(Expression.AndAlso);
    }

    private static bool TryEvaluateToConstant(Expression expression, [NotNullWhen(true)] out SqlConstantExpression? sqlConstantExpression)
    {
        if (CanEvaluate(expression))
        {
            sqlConstantExpression = new SqlConstantExpression(
                Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object)))
                    .Compile(preferInterpretation: true)
                    .Invoke(),
                expression.Type,
                typeMapping: null);
            return true;
        }

        sqlConstantExpression = null;
        return false;
    }

    private bool TryRewriteContainsEntity(Expression source, Expression item, [NotNullWhen(true)] out Expression? result)
    {
        result = null;

        if (item is not StructuralTypeReferenceExpression { StructuralType: IEntityType entityType })
        {
            return false;
        }

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

    private bool TryRewriteStructuralTypeEquality(
        ExpressionType nodeType,
        Expression left,
        Expression right,
        bool equalsMethod,
        [NotNullWhen(true)] out Expression? result)
    {
        var leftReference = left as StructuralTypeReferenceExpression;
        var rightReference = right as StructuralTypeReferenceExpression;

        switch ((leftEntityReference: leftReference, rightEntityReference: rightReference))
        {
            case ({ StructuralType: IEntityType }, { StructuralType: IEntityType } or null):
            case ({ StructuralType: IEntityType } or null, { StructuralType: IEntityType }):
                return TryRewriteEntityEquality(out result);

            case ({ StructuralType: IComplexType }, { StructuralType: IComplexType } or null):
            case ({ StructuralType: IComplexType } or null, { StructuralType: IComplexType }):
                return TryRewriteComplexTypeEquality(out result);

            default:
                result = null;
                return false;
        }

        bool TryRewriteEntityEquality([NotNullWhen(true)] out Expression? result)
        {
            if (IsNullSqlConstantExpression(left)
                || IsNullSqlConstantExpression(right))
            {
                var nonNullEntityReference = (IsNullSqlConstantExpression(left) ? rightReference : leftReference)!;
                var nullComparedEntityType = (IEntityType)nonNullEntityReference.StructuralType;

                if (nonNullEntityReference is { Parameter.ValueBufferExpression: JsonQueryExpression jsonQueryExpression })
                {
                    var jsonScalarExpression = new JsonScalarExpression(
                        jsonQueryExpression.JsonColumn,
                        jsonQueryExpression.Path,
                        jsonQueryExpression.JsonColumn.Type,
                        jsonQueryExpression.JsonColumn.TypeMapping!,
                        jsonQueryExpression.IsNullable);

                    result = nodeType == ExpressionType.Equal
                        ? _sqlExpressionFactory.IsNull(jsonScalarExpression)
                        : _sqlExpressionFactory.IsNotNull(jsonScalarExpression);

                    return true;
                }

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
                        var requiredNonPkProperties = nullComparedEntityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey())
                            .ToList();
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
                            // if we don't have any required properties to properly check the nullability,
                            // we rely on optional ones (somewhat unreliably)
                            // - if entity is to be null, all the properties must be null
                            // - if the entity is to be not null, at least one property must be not null
                            var optionalPropertiesCondition = allNonPrincipalSharedNonPkProperties
                                .Select(
                                    p => Infrastructure.ExpressionExtensions.CreateEqualsExpression(
                                        CreatePropertyAccessExpression(nonNullEntityReference, p),
                                        Expression.Constant(null, p.ClrType.MakeNullable()),
                                        nodeType != ExpressionType.Equal))
                                .Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.AndAlso(l, r) : Expression.OrElse(l, r));

                            condition = condition == null
                                ? optionalPropertiesCondition
                                : nodeType == ExpressionType.Equal
                                    ? Expression.OrElse(condition, optionalPropertiesCondition)
                                    : Expression.AndAlso(condition, optionalPropertiesCondition);
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

            var leftEntityType = leftReference?.StructuralType as IEntityType;
            var rightEntityType = rightReference?.StructuralType as IEntityType;
            var entityType = leftEntityType ?? rightEntityType;

            Check.DebugAssert(entityType != null, "We checked that at least one side is an entity type before calling this function");

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
                && (leftReference?.Subquery != null
                    || rightReference?.Subquery != null))
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

        bool TryRewriteComplexTypeEquality([NotNullWhen(true)] out Expression? result)
        {
            if (IsNullSqlConstantExpression(left)
                || IsNullSqlConstantExpression(right))
            {
                // TODO: when we support optional complex types - or projecting required complex types via optional navigations - we'll
                // be able to translate this.
                throw new InvalidOperationException(RelationalStrings.CannotCompareComplexTypeToNull);
            }

            var leftComplexType = leftReference?.StructuralType as IComplexType;
            var rightComplexType = rightReference?.StructuralType as IComplexType;
            var complexType = leftComplexType ?? rightComplexType;

            Check.DebugAssert(complexType != null, "We checked that at least one side is a complex type before calling this function");

            // If a complex type is the result of a subquery, then comparing its columns would mean duplicating the subquery, which would
            // be potentially very inefficient.
            // TODO: Enable this by extracting the subquery out to a common table expressions (WITH)
            if (leftReference is { Subquery: not null } || rightReference is { Subquery: not null })
            {
                throw new InvalidOperationException(RelationalStrings.SubqueryOverComplexTypesNotSupported(complexType.DisplayName()));
            }

            // Generate an expression that compares each property on the left to the same property on the right; this needs to recursively
            // include all properties in nested complex types.
            Expression? comparisons = null;
            GenerateComparisons(complexType, left, right);

            // Indicates failure to bind to a complex property while generating the comparisons
            if (comparisons is null)
            {
                result = null;
                return false;
            }

            result = Visit(comparisons);
            return true;

            void GenerateComparisons(IComplexType type, Expression left, Expression right)
            {
                foreach (var property in type.GetProperties())
                {
                    var comparison = Infrastructure.ExpressionExtensions.CreateEqualsExpression(
                        CreatePropertyAccessExpression(left, property),
                        CreatePropertyAccessExpression(right, property),
                        nodeType != ExpressionType.Equal);

                    comparisons = comparisons is null
                        ? comparison
                        : nodeType == ExpressionType.Equal
                            ? Expression.AndAlso(comparisons, comparison)
                            : Expression.OrElse(comparisons, comparison);
                }

                foreach (var complexProperty in type.GetComplexProperties())
                {
                    Check.DebugAssert(
                        left is not StructuralTypeReferenceExpression { Subquery: not null }
                        && right is not StructuralTypeReferenceExpression { Subquery: not null },
                        "Subquery complex type references are not supported");

                    // TODO: Implement/test non-entity binding (i.e. with a constant instance)
                    var nestedLeft = left is StructuralTypeReferenceExpression leftReference
                        ? BindComplexProperty(leftReference, complexProperty)
                        : CreateComplexPropertyAccessExpression(left, complexProperty);
                    var nestedRight = right is StructuralTypeReferenceExpression rightReference
                        ? BindComplexProperty(rightReference, complexProperty)
                        : CreateComplexPropertyAccessExpression(right, complexProperty);

                    if (nestedLeft is null || nestedRight is null)
                    {
                        comparisons = null;
                        return;
                    }

                    GenerateComparisons(complexProperty.ComplexType, nestedLeft, nestedRight);
                }
            }
        }
    }

    private Expression CreatePropertyAccessExpression(Expression target, IProperty property)
    {
        switch (target)
        {
            // TODO: Cleanup, why do we need both SqlConstantExpression and ConstantExpression
            case SqlConstantExpression sqlConstantExpression:
                return Expression.Constant(
                    sqlConstantExpression.Value is null
                        ? null
                        : property.GetGetter().GetClrValue(sqlConstantExpression.Value),
                    property.ClrType.MakeNullable());

            case ConstantExpression sqlConstantExpression:
                return Expression.Constant(
                    sqlConstantExpression.Value is null
                        ? null
                        : property.GetGetter().GetClrValue(sqlConstantExpression.Value),
                    property.ClrType.MakeNullable());

            case SqlParameterExpression sqlParameterExpression
                when sqlParameterExpression.Name.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal):
            {
                var lambda = Expression.Lambda(
                    Expression.Call(
                        ParameterValueExtractorMethod.MakeGenericMethod(property.ClrType.MakeNullable()),
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(sqlParameterExpression.Name, typeof(string)),
                        Expression.Constant(null, typeof(List<IComplexProperty>)),
                        Expression.Constant(property, typeof(IProperty))),
                    QueryCompilationContext.QueryContextParameter);

                var newParameterName =
                    $"{RuntimeParameterPrefix}"
                    + $"{sqlParameterExpression.Name[QueryCompilationContext.QueryParameterPrefix.Length..]}_{property.Name}";

                return _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
            }

            case ParameterBasedComplexPropertyChainExpression chainExpression:
            {
                var lambda = Expression.Lambda(
                    Expression.Call(
                        ParameterValueExtractorMethod.MakeGenericMethod(property.ClrType.MakeNullable()),
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(chainExpression.ParameterExpression.Name, typeof(string)),
                        Expression.Constant(chainExpression.ComplexPropertyChain, typeof(List<IComplexProperty>)),
                        Expression.Constant(property, typeof(IProperty))),
                    QueryCompilationContext.QueryContextParameter);

                var newParameterName =
                    $"{RuntimeParameterPrefix}"
                    + $"{chainExpression.ParameterExpression.Name[QueryCompilationContext.QueryParameterPrefix.Length..]}_{property.Name}";

                return _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
            }

            case MemberInitExpression memberInitExpression
                when memberInitExpression.Bindings.SingleOrDefault(
                    mb => mb.Member.Name == property.Name) is MemberAssignment memberAssignment:
                return memberAssignment.Expression;

            default:
                return target.CreateEFPropertyExpression(property);
        }
    }

    private Expression CreateComplexPropertyAccessExpression(Expression target, IComplexProperty complexProperty)
        => target switch
        {
            SqlConstantExpression constant => Expression.Constant(
                constant.Value is null ? null : complexProperty.GetGetter().GetClrValue(constant.Value),
                complexProperty.ClrType.MakeNullable()),

            SqlParameterExpression sqlParameterExpression
                when sqlParameterExpression.Name.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal)
                => new ParameterBasedComplexPropertyChainExpression(sqlParameterExpression, complexProperty),

            MemberInitExpression memberInitExpression
                when memberInitExpression.Bindings.SingleOrDefault(mb => mb.Member.Name == complexProperty.Name) is MemberAssignment
                    memberAssignment
                => memberAssignment.Expression,

            // For non-constant/parameter complex property accesses, BindComplexProperty is called instead of this method
            // TODO: possibly refactor, folding this method into BindComplexProperty to have it handle the constant/parameter cases as well
            // (but consider the non-complex property case as well)
            _ => throw new UnreachableException()
        };

    private static T? ParameterValueExtractor<T>(
        QueryContext context,
        string baseParameterName,
        List<IComplexProperty>? complexPropertyChain,
        IProperty property)
    {
        var baseValue = context.ParameterValues[baseParameterName];

        if (complexPropertyChain is not null)
        {
            foreach (var complexProperty in complexPropertyChain)
            {
                if (baseValue is null)
                {
                    break;
                }

                baseValue = complexProperty.GetGetter().GetClrValue(baseValue);
            }
        }

        return baseValue == null ? (T?)(object?)null : (T?)property.GetGetter().GetClrValue(baseValue);
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

    private sealed class ParameterBasedComplexPropertyChainExpression : Expression
    {
        public ParameterBasedComplexPropertyChainExpression(
            SqlParameterExpression parameterExpression,
            IComplexProperty firstComplexProperty)
        {
            ParameterExpression = parameterExpression;
            ComplexPropertyChain = [firstComplexProperty];
        }

        public SqlParameterExpression ParameterExpression { get; }
        public List<IComplexProperty> ComplexPropertyChain { get; }
    }

    private static bool CanEvaluate(Expression expression)
        => expression switch
        {
            ConstantExpression => true,
            NewExpression e => e.Arguments.All(CanEvaluate),
            NewArrayExpression e => e.Expressions.All(CanEvaluate),
            MemberInitExpression e => CanEvaluate(e.NewExpression)
                && e.Bindings.All(mb => mb is MemberAssignment memberAssignment && CanEvaluate(memberAssignment.Expression)),
            _ => false
        };

    private static bool IsNullSqlConstantExpression(Expression expression)
        => expression is SqlConstantExpression { Value: null };

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

    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    private sealed class StructuralTypeReferenceExpression : Expression
    {
        public StructuralTypeReferenceExpression(StructuralTypeShaperExpression parameter)
        {
            Parameter = parameter;
            StructuralType = parameter.StructuralType;
        }

        public StructuralTypeReferenceExpression(ShapedQueryExpression subquery)
        {
            Subquery = subquery;
            StructuralType = ((StructuralTypeShaperExpression)subquery.ShaperExpression).StructuralType;
        }

        private StructuralTypeReferenceExpression(StructuralTypeReferenceExpression typeReference, ITypeBase structuralType)
        {
            Parameter = typeReference.Parameter;
            Subquery = typeReference.Subquery;
            StructuralType = structuralType;
        }

        public new StructuralTypeShaperExpression? Parameter { get; }
        public ShapedQueryExpression? Subquery { get; }
        public ITypeBase StructuralType { get; }

        public override Type Type
            => StructuralType.ClrType;

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        public Expression Convert(Type type)
        {
            if (type == typeof(object) // Ignore object conversion
                || type.IsAssignableFrom(Type)) // Ignore casting to base type/interface
            {
                return this;
            }

            return StructuralType is IEntityType entityType
                && entityType.GetDerivedTypes().FirstOrDefault(et => et.ClrType == type) is IEntityType derivedEntityType
                    ? new StructuralTypeReferenceExpression(this, derivedEntityType)
                    : QueryCompilationContext.NotTranslatedExpression;
        }

        private string DebuggerDisplay()
            => this switch
            {
                { Parameter: not null } => Parameter.DebuggerDisplay(),
                { Subquery: not null } => ExpressionPrinter.Print(Subquery!),
                _ => throw new UnreachableException()
            };
    }
}

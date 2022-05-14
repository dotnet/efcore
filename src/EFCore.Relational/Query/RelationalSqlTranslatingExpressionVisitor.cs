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

    private static readonly MethodInfo ObjectEqualsMethodInfo
        = typeof(object).GetRuntimeMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) })!;

    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly IModel _model;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
    private readonly SqlTypeMappingVerifyingExpressionVisitor _sqlTypeMappingVerifyingExpressionVisitor;

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
                return extensionExpression;

            case EntityShaperExpression entityShaperExpression:
                return new EntityReferenceExpression(entityShaperExpression);

            case ProjectionBindingExpression projectionBindingExpression:
                return ((SelectExpression)projectionBindingExpression.QueryExpression)
                    .GetProjection(projectionBindingExpression);

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
            ?? (TranslationFailed(memberExpression.Expression, Visit(memberExpression.Expression), out var sqlInnerExpression)
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
            return TryBindMember(Visit(source), MemberIdentity.Create(propertyName))
                ?? throw new InvalidOperationException(CoreStrings.QueryUnableToTranslateEFProperty(methodCallExpression.Print()));
        }

        // EF Indexer property
        if (methodCallExpression.TryGetIndexerArguments(_model, out source, out propertyName))
        {
            var result = TryBindMember(Visit(source), MemberIdentity.Create(propertyName));
            if (result != null)
            {
                return result;
            }
        }

        // Subquery case
        // TODO: Refactor in future to avoid repeated visitation.
        // Specifically ordering of visiting aggregate chain, subquery, method arguments.
        if (methodCallExpression.Method.IsStatic
            && methodCallExpression.Arguments.Count > 0
            && methodCallExpression.Method.DeclaringType == typeof(Queryable))
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable
                && methodCallExpression.Arguments[0] is RelationalGroupByShaperExpression groupByShaperExpression)
            {
                return new EnumerableExpression(groupByShaperExpression.ElementSelector);
            }

            var enumerableSource = Visit(methodCallExpression.Arguments[0]);
            if (enumerableSource is EnumerableExpression enumerableExpression)
            {
                Expression? result = null;
                switch (methodCallExpression.Method.Name)
                {
                    case nameof(Queryable.Average):
                        if (methodCallExpression.Arguments.Count == 2)
                        {
                            enumerableExpression = ProcessSelector(
                                enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                        }

                        result = TranslateAggregate(methodCallExpression.Method, enumerableExpression);
                        break;

                    case nameof(Queryable.Count):
                        if (methodCallExpression.Arguments.Count == 2)
                        {
                            var newEnumerableExpression = ProcessPredicate(
                                enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                            if (newEnumerableExpression == null)
                            {
                                break;
                            }

                            enumerableExpression = newEnumerableExpression;
                        }

                        result = TranslateAggregate(methodCallExpression.Method, enumerableExpression);
                        break;


                    case nameof(Queryable.Distinct):
                        result = enumerableExpression.Selector is EntityShaperExpression entityShaperExpression
                            && entityShaperExpression.EntityType.FindPrimaryKey() != null
                            ? enumerableExpression
                            : !enumerableExpression.IsDistinct
                                ? enumerableExpression.ApplyDistinct()
                                : (Expression?)null;
                        break;

                    case nameof(Queryable.LongCount):
                        if (methodCallExpression.Arguments.Count == 2)
                        {
                            var newEnumerableExpression = ProcessPredicate(
                                enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                            if (newEnumerableExpression == null)
                            {
                                break;
                            }

                            enumerableExpression = newEnumerableExpression;
                        }

                        result = TranslateAggregate(methodCallExpression.Method, enumerableExpression);
                        break;

                    case nameof(Queryable.Max):
                        if (methodCallExpression.Arguments.Count == 2)
                        {
                            enumerableExpression = ProcessSelector(
                                enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                        }

                        result = TranslateAggregate(methodCallExpression.Method, enumerableExpression);
                        break;

                    case nameof(Queryable.Min):
                        if (methodCallExpression.Arguments.Count == 2)
                        {
                            enumerableExpression = ProcessSelector(
                                enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                        }

                        result = TranslateAggregate(methodCallExpression.Method, enumerableExpression);
                        break;

                    case nameof(Queryable.OrderBy):
                        result = ProcessOrderByThenBy(
                            enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(), thenBy: false, ascending: true);
                        break;

                    case nameof(Queryable.OrderByDescending):
                        result = ProcessOrderByThenBy(
                            enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(), thenBy: false, ascending: false);
                        break;

                    case nameof(Queryable.ThenBy):
                        result = ProcessOrderByThenBy(
                            enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(), thenBy: true, ascending: true);
                        break;

                    case nameof(Queryable.ThenByDescending):
                        result = ProcessOrderByThenBy(
                            enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(), thenBy: true, ascending: false);
                        break;

                    case nameof(Queryable.Select):
                        result = ProcessSelector(enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                        break;

                    case nameof(Queryable.Sum):
                        if (methodCallExpression.Arguments.Count == 2)
                        {
                            enumerableExpression = ProcessSelector(
                                enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                        }

                        result = TranslateAggregate(methodCallExpression.Method, enumerableExpression);
                        break;

                    case nameof(Queryable.Where):
                        result = ProcessPredicate(enumerableExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                        break;
                }

                if (result != null)
                {
                    return result;
                }
            }
        }

        var subqueryTranslation = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
        if (subqueryTranslation != null)
        {
            if (subqueryTranslation.ResultCardinality == ResultCardinality.Enumerable)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var shaperExpression = subqueryTranslation.ShaperExpression;
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
                return new EntityReferenceExpression(subqueryTranslation.UpdateShaperExpression(innerExpression));
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

            var subquery = (SelectExpression)subqueryTranslation.QueryExpression;
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

            if (subqueryTranslation.ResultCardinality == ResultCardinality.SingleOrDefault
                && !shaperExpression.Type.IsNullableType())
            {
                scalarSubqueryExpression = _sqlExpressionFactory.Coalesce(
                    scalarSubqueryExpression,
                    (SqlExpression)Visit(shaperExpression.Type.GetDefaultValueConstant()));
            }

            return scalarSubqueryExpression;
        }

        SqlExpression? sqlObject = null;
        SqlExpression[] arguments;
        var method = methodCallExpression.Method;

        if (method.Name == nameof(object.Equals)
            && methodCallExpression.Object != null
            && methodCallExpression.Arguments.Count == 1)
        {
            var left = Visit(methodCallExpression.Object);
            var right = Visit(RemoveObjectConvert(methodCallExpression.Arguments[0]));

            if (TryRewriteEntityEquality(
                    ExpressionType.Equal,
                    left == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Object : left,
                    right == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[0] : right,
                    equalsMethod: true,
                    out var result))
            {
                return result;
            }

            if (left is SqlExpression leftSql
                && right is SqlExpression rightSql)
            {
                sqlObject = leftSql;
                arguments = new[] { rightSql };
            }
            else
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }
        else if (method.Name == nameof(object.Equals)
                 && methodCallExpression.Object == null
                 && methodCallExpression.Arguments.Count == 2)
        {
            if (methodCallExpression.Arguments[0].Type == typeof(object[])
                && methodCallExpression.Arguments[0] is NewArrayExpression)
            {
                return Visit(
                    ConvertObjectArrayEqualityComparison(
                        methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]));
            }

            var left = Visit(RemoveObjectConvert(methodCallExpression.Arguments[0]));
            var right = Visit(RemoveObjectConvert(methodCallExpression.Arguments[1]));

            if (TryRewriteEntityEquality(
                    ExpressionType.Equal,
                    left == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[0] : left,
                    right == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[1] : right,
                    equalsMethod: true,
                    out var result))
            {
                return result;
            }

            if (left is SqlExpression leftSql
                && right is SqlExpression rightSql)
            {
                arguments = new[] { leftSql, rightSql };
            }
            else
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }
        else if (method.IsGenericMethod
                 && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains))
        {
            var enumerable = Visit(methodCallExpression.Arguments[0]);
            var item = Visit(methodCallExpression.Arguments[1]);

            if (TryRewriteContainsEntity(
                    enumerable,
                    item == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[1] : item, out var result))
            {
                return result;
            }

            if (enumerable is SqlExpression sqlEnumerable
                && item is SqlExpression sqlItem)
            {
                arguments = new[] { sqlEnumerable, sqlItem };
            }
            else
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }
        else if (methodCallExpression.Arguments.Count == 1
                 && method.IsContainsMethod())
        {
            var enumerable = Visit(methodCallExpression.Object);
            var item = Visit(methodCallExpression.Arguments[0]);

            if (TryRewriteContainsEntity(
                    enumerable!,
                    item == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[0] : item, out var result))
            {
                return result;
            }

            if (enumerable is SqlExpression sqlEnumerable
                && item is SqlExpression sqlItem)
            {
                sqlObject = sqlEnumerable;
                arguments = new[] { sqlItem };
            }
            else
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }
        else
        {
            if (TranslationFailed(methodCallExpression.Object, Visit(methodCallExpression.Object), out sqlObject))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            arguments = new SqlExpression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = methodCallExpression.Arguments[i];
                if (TranslationFailed(argument, Visit(argument), out var sqlArgument))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                arguments[i] = sqlArgument!;
            }
        }

        var translation = Dependencies.MethodCallTranslatorProvider.Translate(
            _model, sqlObject, methodCallExpression.Method, arguments, _queryCompilationContext.Logger);

        if (translation == null)
        {
            if (methodCallExpression.Method == StringEqualsWithStringComparison
                || methodCallExpression.Method == StringEqualsWithStringComparisonStatic)
            {
                AddTranslationErrorDetails(CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison);
            }
            else
            {
                AddTranslationErrorDetails(
                    CoreStrings.QueryUnableToTranslateMethod(
                        methodCallExpression.Method.DeclaringType?.DisplayName(),
                        methodCallExpression.Method.Name));
            }
        }

        return translation ?? QueryCompilationContext.NotTranslatedExpression;
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
                var discriminatorProperty = entityType.FindDiscriminatorProperty();
                if (discriminatorProperty == null)
                {
                    if (entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy
                        && entityType.GetDerivedTypesInclusive().Count(e => !e.IsAbstract()) == 1)
                    {
                        return _sqlExpressionFactory.Constant(true);
                    }

                    // TPT or TPC
                    var discriminatorValues = derivedType.GetTptDiscriminatorValues();
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

    private Expression? TryBindMember(Expression? source, MemberIdentity member)
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

            var table = entityType.GetViewOrTableMappings().SingleOrDefault()?.Table
                ?? entityType.GetDefaultMappings().Single().Table;
            if (!table.IsOptional(entityType))
            {
                return propertyAccess;
            }

            // this is optional dependent sharing table
            var nonPrincipalSharedNonPkProperties = entityType.GetNonPrincipalSharedNonPkProperties(table).ToList();
            if (nonPrincipalSharedNonPkProperties.Contains(property))
            {
                // The column is not being shared with principal side so we can always use directly
                return propertyAccess;
            }

            var condition = nonPrincipalSharedNonPkProperties
                .Where(e => !e.IsNullable)
                .Select(p => entityProjectionExpression.BindProperty(p))
                .Select(c => (SqlExpression)_sqlExpressionFactory.NotEqual(c, _sqlExpressionFactory.Constant(null)))
                .Aggregate((a, b) => _sqlExpressionFactory.AndAlso(a, b));

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

    private static Expression RemapLambda(EnumerableExpression enumerableExpression, LambdaExpression lambdaExpression)
        => ReplacingExpressionVisitor.Replace(lambdaExpression.Parameters[0], enumerableExpression.Selector, lambdaExpression.Body);

    private static EnumerableExpression ProcessSelector(EnumerableExpression enumerableExpression, LambdaExpression lambdaExpression)
        => enumerableExpression.ApplySelector(RemapLambda(enumerableExpression, lambdaExpression));

    private EnumerableExpression? ProcessOrderByThenBy(
        EnumerableExpression enumerableExpression, LambdaExpression lambdaExpression, bool thenBy, bool ascending)
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
        if (predicate == null)
        {
            return null;
        }

        return enumerableExpression.ApplyPredicate(predicate);
    }

    private SqlExpression? TranslateAggregate(MethodInfo methodInfo, EnumerableExpression enumerableExpression)
    {
        var selector = TranslateInternal(enumerableExpression.Selector);
        if (selector == null)
        {
            if (methodInfo.IsGenericMethod
                && PredicateAggregateMethodInfos.Contains(methodInfo.GetGenericMethodDefinition()))
            {
                selector = _sqlExpressionFactory.Fragment("*");
            }
            else
            {
                return null;
            }
        }
        enumerableExpression.ApplySelector(selector);

        if (enumerableExpression.Predicate != null)
        {
            if (selector is SqlFragmentExpression)
            {
                selector = _sqlExpressionFactory.Constant(1);
            }

            selector = _sqlExpressionFactory.Case(
                new List<CaseWhenClause> { new(enumerableExpression.Predicate, selector) },
                elseResult: null);
        }

        if (enumerableExpression.IsDistinct)
        {
            selector = new DistinctExpression(selector);
        }

        // TODO: Issue#22957
        return methodInfo.Name switch
        {
            nameof(Queryable.Average) => TranslateAverage(selector),
            nameof(Queryable.Count) => TranslateCount(selector),
            nameof(Queryable.LongCount) => TranslateLongCount(selector),
            nameof(Queryable.Max) => TranslateMax(selector),
            nameof(Queryable.Min) => TranslateMin(selector),
            nameof(Queryable.Sum) => TranslateSum(selector),
            _ => null,
        };
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
                (l, r) => (Expression)Expression.Call(ObjectEqualsMethodInfo, l, r))
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
                    QueryCompilationContext.QueryContextParameter
                );

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
                    var condition = nullComparedEntityType.GetNonPrincipalSharedNonPkProperties(table)
                        .Where(e => !e.IsNullable)
                        .Select(
                            p =>
                            {
                                var comparison = Expression.Call(
                                    ObjectEqualsMethodInfo,
                                    Expression.Convert(CreatePropertyAccessExpression(nonNullEntityReference, p), typeof(object)),
                                    Expression.Convert(Expression.Constant(null, p.ClrType.MakeNullable()), typeof(object)));

                                return nodeType == ExpressionType.Equal
                                    ? (Expression)comparison
                                    : Expression.Not(comparison);
                            })
                        .Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r));

                    result = Visit(condition);
                    return true;
                }
            }

            result = Visit(
                nullComparedEntityTypePrimaryKeyProperties.Select(
                    p =>
                    {
                        var comparison = Expression.Call(
                            ObjectEqualsMethodInfo,
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
                p =>
                {
                    var comparison = Expression.Call(
                        ObjectEqualsMethodInfo,
                        Expression.Convert(CreatePropertyAccessExpression(left, p), typeof(object)),
                        Expression.Convert(CreatePropertyAccessExpression(right, p), typeof(object)));

                    return nodeType == ExpressionType.Equal
                        ? (Expression)comparison
                        : Expression.Not(comparison);
                }).Aggregate(
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
        {
            if (extensionExpression is SqlExpression sqlExpression
                && extensionExpression is not SqlFragmentExpression)
            {
                if (sqlExpression.TypeMapping == null)
                {
                    throw new InvalidOperationException(RelationalStrings.NullTypeMappingInSqlTree(sqlExpression.Print()));
                }
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}

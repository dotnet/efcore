// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosSqlTranslatingExpressionVisitor(
        QueryCompilationContext queryCompilationContext,
        ISqlExpressionFactory sqlExpressionFactory,
        ITypeMappingSource typeMappingSource,
        IMemberTranslatorProvider memberTranslatorProvider,
        IMethodCallTranslatorProvider methodCallTranslatorProvider,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
    : ExpressionVisitor
{
    private const string RuntimeParameterPrefix = QueryCompilationContext.QueryParameterPrefix + "entity_equality_";

    private static readonly MethodInfo ParameterValueExtractorMethod =
        typeof(CosmosSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor))!;

    private static readonly MethodInfo ParameterListValueExtractorMethod =
        typeof(CosmosSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterListValueExtractor))!;

    private static readonly MethodInfo ConcatMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.Concat), [typeof(object), typeof(object)])!;

    private static readonly MethodInfo StringEqualsWithStringComparison
        = typeof(string).GetRuntimeMethod(nameof(string.Equals), [typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo StringEqualsWithStringComparisonStatic
        = typeof(string).GetRuntimeMethod(nameof(string.Equals), [typeof(string), typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo GetTypeMethodInfo = typeof(object).GetTypeInfo().GetDeclaredMethod(nameof(GetType))!;

    private readonly IModel _model = queryCompilationContext.Model;
    private readonly SqlTypeMappingVerifyingExpressionVisitor _sqlVerifyingExpressionVisitor = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? TranslationErrorDetails { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
            translation = sqlExpressionFactory.ApplyDefaultTypeMapping(translation);

            if (translation.TypeMapping == null)
            {
                // The return type is not-mappable hence return null
                return null;
            }

            _sqlVerifyingExpressionVisitor.Visit(translation);

            return translation;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        switch (binaryExpression.NodeType)
        {
            case ExpressionType.Coalesce:
                var ifTrue = binaryExpression.Left;
                var ifFalse = binaryExpression.Right;
                if (ifTrue.Type != ifFalse.Type)
                {
                    ifFalse = Expression.Convert(ifFalse, ifTrue.Type);
                }

                return Visit(
                    Expression.Condition(
                        Expression.NotEqual(ifTrue, Expression.Constant(null, ifTrue.Type)),
                        ifTrue,
                        ifFalse));

            case ExpressionType.Equal:
            case ExpressionType.NotEqual when binaryExpression.Left.Type == typeof(Type):
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

                break;
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

        var visitedLeft = Visit(left);
        var visitedRight = Visit(right);

        if (binaryExpression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual
            // Visited expression could be null, We need to pass MemberInitExpression
            && TryRewriteEntityEquality(
                binaryExpression.NodeType,
                visitedLeft == QueryCompilationContext.NotTranslatedExpression ? left : visitedLeft,
                visitedRight == QueryCompilationContext.NotTranslatedExpression ? right : visitedRight,
                equalsMethod: false,
                out var result))
        {
            return result;
        }

        if (binaryExpression.Method == ConcatMethodInfo)
        {
            return QueryCompilationContext.NotTranslatedExpression;
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
                : sqlExpressionFactory.MakeBinary(
                    uncheckedNodeTypeVariant,
                    sqlLeft!,
                    sqlRight!,
                    typeMapping: null)
                ?? QueryCompilationContext.NotTranslatedExpression;

        Expression ProcessGetType(EntityReferenceExpression entityReferenceExpression, Type comparisonType, bool match)
        {
            var entityType = entityReferenceExpression.EntityType;

            if (entityType.BaseType == null
                && !entityType.GetDirectlyDerivedTypes().Any())
            {
                // No hierarchy
                return sqlExpressionFactory.Constant((entityType.ClrType == comparisonType) == match);
            }

            if (entityType.GetAllBaseTypes().Any(e => e.ClrType == comparisonType))
            {
                // EntitySet will never contain a type of base type
                return sqlExpressionFactory.Constant(!match);
            }

            var derivedType = entityType.GetDerivedTypesInclusive().SingleOrDefault(et => et.ClrType == comparisonType);
            // If no derived type matches then fail the translation
            if (derivedType != null)
            {
                // If the derived type is abstract type then predicate will always be false
                if (derivedType.IsAbstract())
                {
                    return sqlExpressionFactory.Constant(!match);
                }

                // Or add predicate for matching that particular type discriminator value
                // All hierarchies have discriminator property
                if (TryBindMember(
                        entityReferenceExpression,
                        MemberIdentity.Create(entityType.GetDiscriminatorPropertyName()),
                        out var discriminatorMember,
                        out _)
                    && discriminatorMember is SqlExpression discriminatorColumn)
                {
                    return match
                        ? sqlExpressionFactory.Equal(
                            discriminatorColumn,
                            sqlExpressionFactory.Constant(derivedType.GetDiscriminatorValue()))
                        : sqlExpressionFactory.NotEqual(
                            discriminatorColumn,
                            sqlExpressionFactory.Constant(derivedType.GetDiscriminatorValue()));
                }
            }

            return QueryCompilationContext.NotTranslatedExpression;
        }

        bool IsGetTypeMethodCall(Expression expression, [NotNullWhen(true)] out EntityReferenceExpression? entityReferenceExpression)
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

        static bool IsTypeConstant(Expression expression, [NotNullWhen(true)] out Type? type)
        {
            if (expression is UnaryExpression
                {
                    NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked,
                    Operand: ConstantExpression { Value: Type t }
                })
            {
                type = t;
                return true;
            }

            type = null;
            return false;
        }

        static bool TryUnwrapConvertToObject(Expression expression, [NotNullWhen(true)] out Expression? operand)
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
    {
        var test = Visit(conditionalExpression.Test);
        var ifTrue = Visit(conditionalExpression.IfTrue);
        var ifFalse = Visit(conditionalExpression.IfFalse);

        return TranslationFailed(conditionalExpression.Test, test, out var sqlTest)
            || TranslationFailed(conditionalExpression.IfTrue, ifTrue, out var sqlIfTrue)
            || TranslationFailed(conditionalExpression.IfFalse, ifFalse, out var sqlIfFalse)
                ? QueryCompilationContext.NotTranslatedExpression
                : sqlExpressionFactory.Condition(sqlTest!, sqlIfTrue!, sqlIfFalse!);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitConstant(ConstantExpression constantExpression)
        => new SqlConstantExpression(constantExpression, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case EntityProjectionExpression:
            case EntityReferenceExpression:
            case SqlExpression:
                return extensionExpression;

            case StructuralTypeShaperExpression entityShaperExpression:
                var result = Visit(entityShaperExpression.ValueBufferExpression);

                if (result is UnaryExpression
                    {
                        NodeType: ExpressionType.Convert,
                        Operand.NodeType: ExpressionType.Convert
                    } outerUnary
                    && outerUnary.Type == typeof(ValueBuffer)
                    && outerUnary.Operand.Type == typeof(object))
                {
                    result = ((UnaryExpression)outerUnary.Operand).Operand;
                }

                return result is EntityProjectionExpression entityProjectionExpression
                    ? new EntityReferenceExpression(entityProjectionExpression)
                    : QueryCompilationContext.NotTranslatedExpression;

            case ProjectionBindingExpression projectionBindingExpression:
                return projectionBindingExpression.ProjectionMember != null
                    ? ((SelectExpression)projectionBindingExpression.QueryExpression)
                    .GetMappedProjection(projectionBindingExpression.ProjectionMember)
                    : QueryCompilationContext.NotTranslatedExpression;

            // This case is for a subquery embedded in a lambda, returning a scalar, e.g. Where(b => b.Posts.Count() > 0).
            // For most cases, generate a scalar subquery (WHERE (SELECT COUNT(*) FROM Posts) > 0).
            case ShapedQueryExpression { ResultCardinality: not ResultCardinality.Enumerable } shapedQuery:
            {
                var shaperExpression = shapedQuery.ShaperExpression;
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
                    // TODO: Subquery projecting out an entity/structural type
                    return QueryCompilationContext.NotTranslatedExpression;
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

                var subquery = (SelectExpression)shapedQuery.QueryExpression;

                var projection = mappedProjectionBindingExpression.ProjectionMember is ProjectionMember projectionMember
                    ? subquery.GetMappedProjection(projectionMember)
                    : throw new NotImplementedException("Subquery with index projection binding");
                if (projection is not SqlExpression sqlExpression)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                if (subquery.Sources.Count == 0)
                {
                    return sqlExpression;
                }

                // TODO
                // subquery.ReplaceProjection(new List<Expression> { sqlExpression });
                subquery.ApplyProjection();

                SqlExpression scalarSubqueryExpression = new ScalarSubqueryExpression(subquery);

                if (shapedQuery.ResultCardinality is ResultCardinality.SingleOrDefault
                    && !shaperExpression.Type.IsNullableType())
                {
                    throw new NotImplementedException("Subquery with SingleOrDefault");
                    // scalarSubqueryExpression = sqlExpressionFactory.Coalesce(
                    //     scalarSubqueryExpression,
                    //     (SqlExpression)Visit(shaperExpression.Type.GetDefaultValueConstant()));
                }

                return scalarSubqueryExpression;
            }

            // This case is for a subquery embedded in a lambda, returning an array, e.g. Where(b => b.Ints == new[] { 1, 2, 3 }).
            // If the subquery represents a bare array (without any operators composed on top), simply extract and return that.
            // Otherwise, wrap the subquery with an ARRAY() operator, converting the subquery to an array first.
            case ShapedQueryExpression { ResultCardinality: ResultCardinality.Enumerable } shapedQuery
                when CosmosQueryUtils.TryConvertToArray(shapedQuery, typeMappingSource, out var array):
                return array;

            default:
                return QueryCompilationContext.NotTranslatedExpression;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitInvocation(InvocationExpression invocationExpression)
        => QueryCompilationContext.NotTranslatedExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        => throw new InvalidOperationException(CoreStrings.TranslationFailed(lambdaExpression.Print()));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitListInit(ListInitExpression listInitExpression)
        => QueryCompilationContext.NotTranslatedExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        var innerExpression = Visit(memberExpression.Expression);

        return TryBindMember(innerExpression, MemberIdentity.Create(memberExpression.Member), out var expression, out _)
            ? expression
            : (TranslationFailed(memberExpression.Expression, innerExpression, out var sqlInnerExpression)
                ? QueryCompilationContext.NotTranslatedExpression
                : memberTranslatorProvider.Translate(
                    sqlInnerExpression, memberExpression.Member, memberExpression.Type, queryCompilationContext.Logger))
            ?? QueryCompilationContext.NotTranslatedExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        => TryEvaluateToConstant(memberInitExpression, out var sqlConstantExpression)
            ? sqlConstantExpression
            : QueryCompilationContext.NotTranslatedExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName)
            || methodCallExpression.TryGetIndexerArguments(_model, out source, out propertyName))
        {
            return TryBindMember(Visit(source), MemberIdentity.Create(propertyName), out var result, out _)
                ? result
                : QueryCompilationContext.NotTranslatedExpression;
        }

        SqlExpression? sqlObject = null;
        SqlExpression[] arguments;
        var method = methodCallExpression.Method;

        switch (methodCallExpression)
        {
            case
            {
                Method.Name: nameof(object.Equals),
                Object: not null,
                Arguments.Count: 1
            }:
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
                    arguments = [rightSql];
                }
                else
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                break;
            }

            case
            {
                Method.Name: nameof(object.Equals),
                Object: null,
                Arguments.Count: 2
            }:
            {
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
                    arguments = [leftSql, rightSql];
                }
                else
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                break;
            }

            case { Method: { Name: nameof(Enumerable.Contains), IsGenericMethod: true } }
                when method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains):
                return TranslateContains(methodCallExpression.Arguments[1], methodCallExpression.Arguments[0]);

            case { Arguments: [var argument] } when method.IsContainsMethod():
                return TranslateContains(argument, methodCallExpression.Object!);

            // For queryable methods, either we translate the whole aggregate or we go to subquery mode
            case { Method.IsStatic: true, Arguments.Count: > 0 } when method.DeclaringType == typeof(Queryable):
                return TranslateAsSubquery(methodCallExpression);

            default:
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
                        return TranslateAsSubquery(methodCallExpression);
                    }

                    arguments[i] = sqlArgument!;
                }

                break;
            }
        }

        Expression? translation = methodCallTranslatorProvider.Translate(
            _model, sqlObject, methodCallExpression.Method, arguments, queryCompilationContext.Logger);
        if (translation is not null)
        {
            return translation;
        }

        translation = TranslateAsSubquery(methodCallExpression);
        if (translation != QueryCompilationContext.NotTranslatedExpression)
        {
            return translation;
        }

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

        return QueryCompilationContext.NotTranslatedExpression;

        Expression TranslateAsSubquery(Expression expression)
        {
            var subqueryTranslation = queryableMethodTranslatingExpressionVisitor.TranslateSubquery(expression);

            return subqueryTranslation == null
                ? QueryCompilationContext.NotTranslatedExpression
                : Visit(subqueryTranslation);
        }

        Expression TranslateContains(Expression untranslatedItem, Expression untranslatedCollection)
        {
            var collection = Visit(untranslatedCollection);
            var itemUnchecked = Visit(untranslatedItem);

            if (TryRewriteContainsEntity(
                    collection,
                    itemUnchecked == QueryCompilationContext.NotTranslatedExpression ? untranslatedItem : itemUnchecked,
                    out var result))
            {
                return result;
            }

            if (itemUnchecked is not SqlExpression translatedItem)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            switch (collection)
            {
                // If the collection was an inline NewArrayExpression with constants only, we get a single constant for that array.
                case SqlConstantExpression { Value: IEnumerable values, TypeMapping: var typeMapping }:
                {
                    var translatedValues = values is IList iList
                        ? new List<SqlExpression>(iList.Count)
                        : [];
                    foreach (var value in values)
                    {
                        translatedValues.Add(sqlExpressionFactory.Constant(value, typeMapping));
                    }

                    return sqlExpressionFactory.In(translatedItem, translatedValues);
                }

                // If the collection was an inline NewArrayExpression with at least one non-constant, the NewArrayExpression makes it
                // as-is to translation, where it (currently) cannot be translated. Identify this case and translate the elements.
                case not SqlExpression when untranslatedCollection is NewArrayExpression { Expressions: var values }:
                {
                    var translatedValues = new SqlExpression[values.Count];
                    for (var i = 0; i < values.Count; i++)
                    {
                        if (Visit(values[i]) is not SqlExpression value)
                        {
                            return QueryCompilationContext.NotTranslatedExpression;
                        }

                        translatedValues[i] = value;
                    }

                    return sqlExpressionFactory.In(translatedItem, translatedValues);
                }

                // If the collection was a captured variable (parameter), construct an InExpression over that;
                // InExpressionValuesExpandingExpressionVisitor will expand the values as constants later.
                case SqlParameterExpression sqlParameterExpression:
                    return sqlExpressionFactory.In(translatedItem, sqlParameterExpression);

                default:
                    return QueryCompilationContext.NotTranslatedExpression;
            }
        }

        static Expression RemoveObjectConvert(Expression expression)
            => expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression
                && unaryExpression.Type == typeof(object)
                    ? unaryExpression.Operand
                    : expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNew(NewExpression newExpression)
        => TryEvaluateToConstant(newExpression, out var sqlConstantExpression)
            ? sqlConstantExpression
            : QueryCompilationContext.NotTranslatedExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
    {
        var expressions = newArrayExpression.Expressions;
        var translatedItems = new SqlExpression[expressions.Count];

        for (var i = 0; i < expressions.Count; i++)
        {
            if (Translate(expressions[i]) is not SqlExpression translatedItem)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            translatedItems[i] = translatedItem;
        }

        var arrayTypeMapping = typeMappingSource.FindMapping(newArrayExpression.Type);
        var elementClrType = newArrayExpression.Type.GetElementType()!;
        var inlineArray = new ArrayConstantExpression(elementClrType, translatedItems, arrayTypeMapping);

        return inlineArray;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitParameter(ParameterExpression parameterExpression)
        => parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal) == true
            ? new SqlParameterExpression(parameterExpression, null)
            : QueryCompilationContext.NotTranslatedExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        var operand = Visit(unaryExpression.Operand);

        if (operand is EntityReferenceExpression entityReferenceExpression
            && unaryExpression.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs)
        {
            return entityReferenceExpression.Convert(unaryExpression.Type);
        }

        if (TranslationFailed(unaryExpression.Operand, operand, out var sqlOperand))
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        return unaryExpression.NodeType switch
        {
            ExpressionType.Not
                => sqlExpressionFactory.Not(sqlOperand!),

            ExpressionType.Negate or ExpressionType.NegateChecked
                => sqlExpressionFactory.Negate(sqlOperand!),

            ExpressionType.Convert or ExpressionType.ConvertChecked
                when operand.Type.IsInterface
                && unaryExpression.Type.GetInterfaces().Any(e => e == operand.Type)
                || unaryExpression.Type.UnwrapNullableType() == operand.Type
                || unaryExpression.Type.UnwrapNullableType() == typeof(Enum)
                // Object convert needs to be converted to explicit cast when mismatching types
                // But we let is pass here since we don't have explicit cast mechanism here and in some cases object convert is due to value types
                || unaryExpression.Type == typeof(object)
                => sqlOperand!,

            _ => QueryCompilationContext.NotTranslatedExpression
        };
    }

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
                return sqlExpressionFactory.Constant(true);
            }

            var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == typeBinaryExpression.TypeOperand);
            if (derivedType != null
                && TryBindMember(
                    entityReferenceExpression,
                    MemberIdentity.Create(entityType.GetDiscriminatorPropertyName()),
                    out var discriminatorMember,
                    out _)
                    && discriminatorMember is SqlExpression discriminatorColumn)
            {
                var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();

                return concreteEntityTypes.Count == 1
                    ? sqlExpressionFactory.Equal(
                        discriminatorColumn,
                        sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                    : sqlExpressionFactory.In(
                        discriminatorColumn,
                        concreteEntityTypes.Select(et => sqlExpressionFactory.Constant(et.GetDiscriminatorValue())).ToArray());
            }
        }

        return QueryCompilationContext.NotTranslatedExpression;
    }

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
        if (source is not EntityReferenceExpression entityReferenceExpression)
        {
            expression = null;
            property = null;
            return false;
        }

        var result = member switch
        {
            { MemberInfo: MemberInfo memberInfo }
                => entityReferenceExpression.ParameterEntity.BindMember(
                    memberInfo, entityReferenceExpression.Type, clientEval: false, out property),

            { Name: string name }
                => entityReferenceExpression.ParameterEntity.BindMember(
                    name, entityReferenceExpression.Type, clientEval: false, out property),

            _ => throw new UnreachableException()
        };

        switch (result)
        {
            case EntityProjectionExpression entityProjectionExpression:
                expression = new EntityReferenceExpression(entityProjectionExpression);
                Check.DebugAssert(property is not null, "Property cannot be null if binding result was non-null");
                return true;
            case ObjectArrayProjectionExpression objectArrayProjectionExpression:
                expression = new EntityReferenceExpression(objectArrayProjectionExpression.InnerProjection);
                Check.DebugAssert(property is not null, "Property cannot be null if binding result was non-null");
                return true;
            case null:
                AddTranslationErrorDetails(
                    CoreStrings.QueryUnableToTranslateMember(
                        member.Name,
                        entityReferenceExpression.EntityType.DisplayName()));
                expression = null;
                return false;
            default:
                expression = result;
                Check.DebugAssert(property is not null, "Property cannot be null if binding result was non-null");
                return true;
        }
    }

    private static Expression TryRemoveImplicitConvert(Expression expression)
    {
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression)
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
                        || innerType == typeof(ushort)))
                || (convertedType == typeof(double)
                    && (innerType == typeof(float))))
            {
                return TryRemoveImplicitConvert(unaryExpression.Operand);
            }
        }

        return expression;
    }

    private bool TryRewriteContainsEntity(Expression source, Expression item, [NotNullWhen(true)] out Expression? result)
    {
        result = null;

        if (item is not EntityReferenceExpression itemEntityReference)
        {
            return false;
        }

        var entityType = itemEntityReference.EntityType;
        var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;

        switch (primaryKeyProperties)
        {
            case null:
                throw new InvalidOperationException(
                    CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                        nameof(Queryable.Contains), entityType.DisplayName()));

            case { Count: > 1 }:
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

                rewrittenSource = queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
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
            var entityType1 = nonNullEntityReference.EntityType;
            var primaryKeyProperties1 = entityType1.FindPrimaryKey()?.Properties;
            if (primaryKeyProperties1 == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                        nodeType == ExpressionType.Equal
                            ? equalsMethod ? nameof(object.Equals) : "=="
                            : equalsMethod
                                ? "!" + nameof(object.Equals)
                                : "!=",
                        entityType1.DisplayName()));
            }

            result = Visit(
                primaryKeyProperties1.Select(
                        p =>
                            Expression.MakeBinary(
                                nodeType, CreatePropertyAccessExpression(nonNullEntityReference, p),
                                Expression.Constant(null, p.ClrType.MakeNullable())))
                    .Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r)));

            return true;
        }

        var leftEntityType = leftEntityReference?.EntityType;
        var rightEntityType = rightEntityReference?.EntityType;
        var entityType = leftEntityType ?? rightEntityType;

        Check.DebugAssert(entityType != null, "At least either side should be entityReference so entityType should be non-null.");

        if (leftEntityType != null
            && rightEntityType != null
            && leftEntityType.GetRootType() != rightEntityType.GetRootType())
        {
            result = sqlExpressionFactory.Constant(false);
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

        result = Visit(
            primaryKeyProperties.Select(
                    p =>
                        Expression.MakeBinary(
                            nodeType,
                            CreatePropertyAccessExpression(left, p),
                            CreatePropertyAccessExpression(right, p)))
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
                    property.GetGetter().GetClrValue(sqlConstantExpression.Value!), property.ClrType.MakeNullable());

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

                return queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);

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

    private static bool IsNullSqlConstantExpression(Expression expression)
        => expression is SqlConstantExpression { Value: null };

    private static bool TryEvaluateToConstant(Expression expression, [NotNullWhen(true)] out SqlConstantExpression? sqlConstantExpression)
    {
        if (CanEvaluate(expression))
        {
            sqlConstantExpression = new SqlConstantExpression(
                Expression.Constant(
                    Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object)))
                        .Compile(preferInterpretation: true)
                        .Invoke(),
                    expression.Type),
                null);
            return true;
        }

        sqlConstantExpression = null;
        return false;
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
        public EntityReferenceExpression(EntityProjectionExpression parameter)
        {
            ParameterEntity = parameter;
            EntityType = parameter.EntityType;
            Type = EntityType.ClrType;
        }

        private EntityReferenceExpression(EntityProjectionExpression parameter, Type type)
        {
            ParameterEntity = parameter;
            EntityType = parameter.EntityType;
            Type = type;
        }

        public EntityProjectionExpression ParameterEntity { get; }
        public IEntityType EntityType { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        public Expression Convert(Type type)
            => type == typeof(object) // Ignore object conversion
                || type.IsAssignableFrom(Type) // Ignore conversion to base/interface
                    ? this
                    : new EntityReferenceExpression(ParameterEntity, type);
    }

    private sealed class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is SqlExpression { TypeMapping: null } sqlExpression
                ? throw new InvalidOperationException(CosmosStrings.NullTypeMappingInSqlTree(sqlExpression.Print()))
                : base.VisitExtension(extensionExpression);
    }
}

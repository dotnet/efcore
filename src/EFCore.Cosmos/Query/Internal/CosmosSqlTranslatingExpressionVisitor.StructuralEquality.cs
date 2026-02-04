// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal.Expressions;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosSqlTranslatingExpressionVisitor
{
    private const string RuntimeParameterPrefix = "entity_equality_";

    private static readonly MethodInfo ParameterPropertyValueExtractorMethod =
        typeof(CosmosSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterPropertyValueExtractor))!;

    private static readonly MethodInfo ParameterValueExtractorMethod =
        typeof(CosmosSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor))!;

    private static readonly MethodInfo ParameterListValueExtractorMethod =
        typeof(CosmosSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterListValueExtractor))!;

    private bool TryRewriteContainsEntity(Expression source, Expression item, [NotNullWhen(true)] out Expression? result)
    {
        result = null;

        if (item is not StructuralTypeReferenceExpression itemEntityReference ||
            itemEntityReference.StructuralType is not IEntityType entityType) // #36468
        {
            return false;
        }

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

            case SqlParameterExpression sqlParameterExpression:
                var lambda = Expression.Lambda(
                    Expression.Call(
                        ParameterListValueExtractorMethod.MakeGenericMethod(entityType.ClrType, property.ClrType.MakeNullable()),
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(sqlParameterExpression.Name, typeof(string)),
                        Expression.Constant(property, typeof(IProperty))),
                    QueryCompilationContext.QueryContextParameter
                );

                var newParameterName = $"{RuntimeParameterPrefix}{sqlParameterExpression.Name}_{property.Name}";

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

    private bool TryRewriteStructuralTypeEquality(
        ExpressionType nodeType,
        Expression left,
        Expression right,
        bool equalsMethod,
        [NotNullWhen(true)] out SqlExpression? result)
    {
        var leftReference = left as StructuralTypeReferenceExpression;
        var rightReference = right as StructuralTypeReferenceExpression;
        var reference = leftReference ?? rightReference;
        if (reference == null)
        {
            result = null;
            return false;
        }

        var leftShaper = leftReference?.Parameter
            ?? (StructuralTypeShaperExpression?)(leftReference?.Subquery)?.ShaperExpression;
        var rightShaper = rightReference?.Parameter
            ?? (StructuralTypeShaperExpression?)(rightReference?.Subquery)?.ShaperExpression;
        var shaper = leftShaper ?? rightShaper ?? throw new UnreachableException();

        if (left is SqlConstantExpression { Value: null }
            || right is SqlConstantExpression { Value: null })
        {
            if (!shaper.IsNullable)
            {
                result = sqlExpressionFactory.Constant(nodeType != ExpressionType.Equal);
                return true;
            }

            var access = new SqlObjectAccessExpression(Visit(shaper.ValueBufferExpression)); // @TODO
            result = sqlExpressionFactory.MakeBinary(
                nodeType,
                access,
                sqlExpressionFactory.Constant(null, typeof(object))!,
                typeMappingSource.FindMapping(typeof(bool)))!;
            return true;
        }

        var leftStructuralType = leftReference?.StructuralType;
        var rightStructuralType = rightReference?.StructuralType;
        var structuralType = reference.StructuralType;

        Check.DebugAssert(structuralType != null, "We checked that at least one side is an entity type");

        switch (structuralType)
        {
            case IEntityType entityType:
                return TryRewriteEntityEquality(entityType, out result);
            case IComplexType complexType:
                return TryRewriteComplexTypeEquality(
                    complexType, out result);
        }

        result = null;
        return false;

        bool TryRewriteEntityEquality(IEntityType entityType, [NotNullWhen(true)] out SqlExpression? result)
        {
            if (leftStructuralType != null
            && rightStructuralType != null
            && leftStructuralType.GetRootType() != rightStructuralType.GetRootType())
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
                        structuralType.DisplayName()));
            }

            result = Visit(
                primaryKeyProperties.Select(p => Expression.MakeBinary(nodeType,
                        CreatePropertyAccessExpression(left, p),
                        CreatePropertyAccessExpression(right, p)))
                    .Aggregate((l, r) => nodeType == ExpressionType.Equal
                        ? Expression.AndAlso(l, r)
                        : Expression.OrElse(l, r))) as SqlExpression;

            return result is not null;
        }

        bool TryRewriteComplexTypeEquality(IComplexType complexType, [NotNullWhen(true)] out SqlExpression? result)
        {
            if (leftStructuralType is not null
                && rightStructuralType is not null
                && leftStructuralType.ClrType != rightStructuralType.ClrType)
            {
                // Currently only support comparing complex types of the same CLR type.
                // We could allow any case where the complex types have the same properties (some may be shadow).
                result = null;
                return false;
            }

            // @TODO: Alternative would be a bitwise comparison... But structure and order of properties would matter then.

            // Generate an expression that compares each property on the left to the same property on the right; this needs to recursively
            // include all properties in nested complex types.
            var boolTypeMapping = typeMappingSource.FindMapping(typeof(bool))!;
            SqlExpression? comparisons = null;

            if (!TryGeneratePropertyComparisons(ref comparisons))
            {
                result = null;
                return false;
            }

            result = comparisons;
            return true;

            bool TryGeneratePropertyComparisons([NotNullWhen(true)] ref SqlExpression? comparisons)
            {
                // We need to know here the difference between
                // x.Collection == x.Collection (should return null, as we need All support)
                // x.Collection[1] == x.Collection[1] (should run below)
                // @TODO: Is there a better way to do this? It feels like this might not be the right place.
                // In relational, this wouldn't have come from a StructuralTypeReferenceExpression, but a CollectionResultExpression? At-least if it's json..
                // See BindMember on StructuralTypeProjectionExpression
                if ((leftReference?.Parameter ?? leftReference?.Subquery?.ShaperExpression as StructuralTypeShaperExpression)
                    ?.ValueBufferExpression is ObjectArrayAccessExpression
                    ||
                    (rightReference?.Parameter ?? rightReference?.Subquery?.ShaperExpression as StructuralTypeShaperExpression)
                    ?.ValueBufferExpression is ObjectArrayAccessExpression)
                {
                    return false;
                }

                foreach (var property in complexType.GetProperties().Cast<IPropertyBase>().Concat(complexType.GetComplexProperties()))
                {
                    var leftAccess = CreatePropertyAccessExpression(left, property);
                    var rightAccess = CreatePropertyAccessExpression(right, property);

                    var comparison = Visit(Expression.MakeBinary(nodeType, leftAccess, rightAccess)) as SqlExpression;
                    if (comparison == null || comparison == QueryCompilationContext.NotTranslatedExpression)
                    {
                        return false;
                    }

                    if (comparison is SqlConstantExpression { Value: false } && nodeType == ExpressionType.Equal)
                    {
                        comparisons = comparison;
                        return true;
                    }

                    comparisons = comparisons is null
                    ? comparison
                    : nodeType == ExpressionType.Equal
                        ? sqlExpressionFactory.AndAlso(comparisons, comparison)
                        : sqlExpressionFactory.OrElse(comparisons, comparison);
                }

                var compare = reference == rightReference ? left : right;
                if (comparisons != null && (leftShaper?.IsNullable == true || rightShaper?.IsNullable == true))
                {
                    var nullCompare = compare;

                    if (nullCompare is SqlParameterExpression sqlParameterExpression)
                    {
                        // @TODO: Can we optimize this instead in CosmosQuerySqlGenerator?
                        // My idea would be to create a SqlParameterComparisonExpression that will hold the comparisons below
                        // But also hold a sql == null or != null expression
                        // The CosmosQuerySqlGenerator will check the value of the parameter and chooses which tree to visit at sql generation time
                        // (or just call a method on SqlParameterComparisonExpression to get which tree to visit, so logic can live there...)
                        var lambda = Expression.Lambda(
                            Expression.Condition(
                                Expression.Equal(
                                    Expression.Call(ParameterValueExtractorMethod.MakeGenericMethod(sqlParameterExpression.Type.MakeNullable()), QueryCompilationContext.QueryContextParameter, Expression.Constant(sqlParameterExpression.Name, typeof(string))),
                                    Expression.Constant(null)),
                                Expression.Constant(null),
                                Expression.Constant(new object())),
                            QueryCompilationContext.QueryContextParameter);

                        var newParameterName = $"{RuntimeParameterPrefix}{sqlParameterExpression.Name}";
                        var queryParam = queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                        nullCompare = new SqlParameterExpression(queryParam.Name, queryParam.Type, CosmosTypeMapping.Default);
                    }

                    comparisons = (SqlExpression)Visit(
                        Expression.OrElse(
                            Expression.AndAlso(
                                Expression.MakeBinary(nodeType, reference, Expression.Constant(null)),
                                Expression.MakeBinary(nodeType, nullCompare, Expression.Constant(null))),
                            Expression.OrElse(
                                Expression.NotEqual(nullCompare, Expression.Constant(null)),
                                comparisons)));
                }

                return comparisons is not null;
            }
        }
    }

    private Expression CreatePropertyAccessExpression(Expression target, IPropertyBase property)
    {
        switch (target)
        {
            case SqlConstantExpression sqlConstantExpression:
                return Expression.Constant(
                    property.GetGetter().GetClrValue(sqlConstantExpression.Value!), property.ClrType.MakeNullable());

            case SqlParameterExpression sqlParameterExpression:
                var lambda = Expression.Lambda(
                    Expression.Call(
                        ParameterPropertyValueExtractorMethod.MakeGenericMethod(property.ClrType.MakeNullable()),
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(sqlParameterExpression.Name, typeof(string)),
                        Expression.Constant(property, typeof(IPropertyBase))),
                    QueryCompilationContext.QueryContextParameter);

                var newParameterName = $"{RuntimeParameterPrefix}{sqlParameterExpression.Name}_{property.Name}";

                return queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);

            case MemberInitExpression memberInitExpression
                when memberInitExpression.Bindings.SingleOrDefault(mb => mb.Member.Name == property.Name) is MemberAssignment
                    memberAssignment:
                return memberAssignment.Expression;

            default:
                return target.CreateEFPropertyExpression(property);
        }
    }

    private static T? ParameterPropertyValueExtractor<T>(QueryContext context, string baseParameterName, IPropertyBase property)
    {
        var baseParameter = context.Parameters[baseParameterName];
        return baseParameter == null ? (T?)(object?)null : (T?)property.GetGetter().GetClrValue(baseParameter);
    }

    private static T? ParameterValueExtractor<T>(QueryContext context, string baseParameterName)
    {
        var baseParameter = context.Parameters[baseParameterName];
        return (T?)baseParameter;
    }

    private static List<TProperty?>? ParameterListValueExtractor<TEntity, TProperty>(
        QueryContext context,
        string baseParameterName,
        IProperty property)
    {
        if (context.Parameters[baseParameterName] is not IEnumerable<TEntity> baseListParameter)
        {
            return null;
        }

        var getter = property.GetGetter();
        return baseListParameter.Select(e => e != null ? (TProperty?)getter.GetClrValue(e) : (TProperty?)(object?)null).ToList();
    }
}

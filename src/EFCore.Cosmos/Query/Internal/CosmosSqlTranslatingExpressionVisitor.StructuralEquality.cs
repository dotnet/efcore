// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        switch (left, right)
        {
            case (StructuralTypeReferenceExpression, SqlConstantExpression { Value: null }):
            case (SqlConstantExpression { Value: null }, StructuralTypeReferenceExpression):
                return RewriteNullEquality(out result);

            case (StructuralTypeReferenceExpression { StructuralType: IEntityType }, _):
            case (_, StructuralTypeReferenceExpression { StructuralType: IEntityType }):
                return TryRewriteEntityEquality(out result);

            case (StructuralTypeReferenceExpression { StructuralType: IComplexType }, _):
            case (_, StructuralTypeReferenceExpression { StructuralType: IComplexType }):
                return TryRewriteComplexTypeEquality(collection: false, out result);

            case (CollectionResultExpression, _):
            case (_, CollectionResultExpression):
                return TryRewriteComplexTypeEquality(collection: true, out result);

            default:
                result = null;
                return false;
        }

        bool RewriteNullEquality(out SqlExpression? result)
        {
            var reference = left as StructuralTypeReferenceExpression ?? (StructuralTypeReferenceExpression)right;
            var boolTypeMapping = typeMappingSource.FindMapping(typeof(bool))!;

            var shaper = reference.Parameter ??
                (StructuralTypeShaperExpression)reference.Subquery!.ShaperExpression;
            if (!shaper.IsNullable)
            {
                result = sqlExpressionFactory.Constant(nodeType != ExpressionType.Equal, boolTypeMapping);
                return true;
            }

            var access = Visit(shaper.ValueBufferExpression);
            result = new SqlBinaryExpression(
                nodeType,
                access,
                sqlExpressionFactory.Constant(null, typeof(object), CosmosTypeMapping.Default)!,
                typeof(bool),
                boolTypeMapping)!;
            return true;
        }

        bool TryRewriteEntityEquality(out SqlExpression? result)
        {
            var leftReference = left as StructuralTypeReferenceExpression;
            var rightReference = right as StructuralTypeReferenceExpression;

            var leftEntityType = leftReference?.StructuralType as IEntityType;
            var rightEntityType = rightReference?.StructuralType as IEntityType;
            var entityType = leftEntityType ?? rightEntityType;

            Check.DebugAssert(entityType != null, "We checked that at least one side is an entity type before calling this function");

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
                primaryKeyProperties.Select(p => Expression.MakeBinary(nodeType,
                        CreatePropertyAccessExpression(left, p),
                        CreatePropertyAccessExpression(right, p)))
                    .Aggregate((l, r) => nodeType == ExpressionType.Equal
                        ? Expression.AndAlso(l, r)
                        : Expression.OrElse(l, r))) as SqlExpression;

            return result is not null;

        }

        bool TryRewriteComplexTypeEquality(bool collection, out SqlExpression? result)
        {
            var (leftAccess, leftComplexType) = ParseComplexAccess(left);
            var (rightAccess, rightComplexType) = ParseComplexAccess(right);

            if (leftAccess is null || leftAccess == QueryCompilationContext.NotTranslatedExpression ||
                rightAccess is null || rightAccess == QueryCompilationContext.NotTranslatedExpression)
            {
                result = null;
                return false;
            }

            if (leftComplexType is not null
                && rightComplexType is not null
                && leftComplexType.ClrType != rightComplexType.ClrType)
            {
                // Currently only support comparing complex types of the same CLR type.
                // We could allow any case where the complex types have the same properties (some may be shadow).
                result = null;
                return false;
            }

            var boolTypeMapping = typeMappingSource.FindMapping(typeof(bool))!;
            result = new SqlBinaryExpression(
                    nodeType,
                    leftAccess,
                    rightAccess,
                    typeof(bool),
                    boolTypeMapping)!;
            return true;

            (Expression?, IComplexType?) ParseComplexAccess(Expression expression)
                => expression switch
                {
                    StructuralTypeReferenceExpression { StructuralType: IComplexType type } reference
                        => (Visit((reference.Parameter ?? (StructuralTypeShaperExpression)reference.Subquery!.ShaperExpression).ValueBufferExpression), type),
                    CollectionResultExpression { ComplexProperty: IComplexProperty { ComplexType: var type } } collectionResult
                        => (Visit((collectionResult.Parameter ?? (StructuralTypeShaperExpression)collectionResult.Subquery!.ShaperExpression).ValueBufferExpression), type),

                    SqlParameterExpression sqlParameterExpression
                        => (CreateJsonQueryParameter(sqlParameterExpression), null),
                    SqlConstantExpression constant
                        => (sqlExpressionFactory.Constant(
                            CosmosSerializationUtilities.SerializeObjectToComplexProperty(type, constant.Value, collectionResult != null),
                            CosmosTypeMapping.Default), null),

                    _ => (null, null)
                };

            Expression CreateJsonQueryParameter(SqlParameterExpression sqlParameterExpression)
            {
                var lambda = Expression.Lambda(
                                Expression.Call(
                                    CosmosSerializationUtilities.SerializeObjectToComplexPropertyMethod,
                                    Expression.Constant(type, typeof(IComplexType)),
                                    Expression.Call(ParameterValueExtractorMethod.MakeGenericMethod(sqlParameterExpression.Type.MakeNullable()), QueryCompilationContext.QueryContextParameter, Expression.Constant(sqlParameterExpression.Name, typeof(string))),
                                    Expression.Constant(collectionResult != null)),
                                QueryCompilationContext.QueryContextParameter);

                var param = queryCompilationContext.RegisterRuntimeParameter($"{RuntimeParameterPrefix}{sqlParameterExpression.Name}", lambda);
                return new SqlParameterExpression(param.Name, param.Type, CosmosTypeMapping.Default);
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

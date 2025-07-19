// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

// Contains the parts of RelationalSqlTranslatingExpressionVisitor which handle structural equality,
// i.e. when an entity or complex type is compared to another structural type (as opposed to scalar equality):
// context.Customers.Where(c => c.ShippingAddress == c.BillingAddress)
public partial class RelationalSqlTranslatingExpressionVisitor
{
    private static readonly MethodInfo ParameterValueExtractorMethod =
        typeof(RelationalSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor))!;

    private static readonly MethodInfo ParameterListValueExtractorMethod =
        typeof(RelationalSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterListValueExtractor))!;

    private static readonly MethodInfo SerializeComplexTypeToJsonMethod =
        typeof(RelationalSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(SerializeComplexTypeToJson))!;

    private bool TryRewriteContainsEntity(Expression source, Expression item, [NotNullWhen(true)] out SqlExpression? result)
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

            case SqlParameterExpression sqlParameterExpression:
                var lambda = Expression.Lambda(
                    Expression.Call(
                        ParameterListValueExtractorMethod.MakeGenericMethod(entityType.ClrType, property.ClrType.MakeNullable()),
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(sqlParameterExpression.Name, typeof(string)),
                        Expression.Constant(property, typeof(IProperty))),
                    QueryCompilationContext.QueryContextParameter);

                var newParameterName =
                    $"{RuntimeParameterPrefix}{sqlParameterExpression.Name}_{property.Name}";

                rewrittenSource = _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                break;

            default:
                return false;
        }

        result = (SqlExpression)Visit(
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
        switch ((left, right))
        {
            case (StructuralTypeReferenceExpression { StructuralType: IEntityType }, _):
            case (_, StructuralTypeReferenceExpression { StructuralType: IEntityType }):
                return TryRewriteEntityEquality(out result);

            case (StructuralTypeReferenceExpression { StructuralType: IComplexType }, _):
            case (_, StructuralTypeReferenceExpression { StructuralType: IComplexType }):
                return TryRewriteComplexTypeEquality(collection: false, out result);

            case (CollectionResultExpression { Relationship: IComplexProperty }, _):
            case (_, CollectionResultExpression { Relationship: IComplexProperty }):
                return TryRewriteComplexTypeEquality(collection: true, out result);

            default:
                result = null;
                return false;
        }

        bool TryRewriteEntityEquality([NotNullWhen(true)] out SqlExpression? result)
        {
            var leftReference = left as StructuralTypeReferenceExpression;
            var rightReference = right as StructuralTypeReferenceExpression;

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
                                ? equalsMethod ? nameof(Equals) : "=="
                                : equalsMethod
                                    ? "!" + nameof(Equals)
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
                            result = (SqlExpression)Visit(condition);
                            return true;
                        }

                        result = null;
                        return false;
                    }
                }

                result = (SqlExpression)Visit(
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

            result = (SqlExpression)Visit(
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

        bool TryRewriteComplexTypeEquality(bool collection, [NotNullWhen(true)] out SqlExpression? result)
        {
            var leftComplexType = left switch
            {
                StructuralTypeReferenceExpression { StructuralType: IComplexType t } => t,
                CollectionResultExpression { Relationship: IComplexProperty { ComplexType: var t } } => t,
                _ => null
            };

            var rightComplexType = right switch
            {
                StructuralTypeReferenceExpression { StructuralType: IComplexType t } => t,
                CollectionResultExpression { Relationship: IComplexProperty { ComplexType: var t } } => t,
                _ => null
            };

            if (leftComplexType is not null
                && rightComplexType is not null
                && leftComplexType.ClrType != rightComplexType.ClrType)
            {
                // Currently only support comparing complex types of the same CLR type.
                // We could allow any case where the complex types have the same properties (some may be shadow).
                result = null;
                return false;
            }

            var complexType = leftComplexType ?? rightComplexType;

            Check.DebugAssert(complexType != null, "We checked that at least one side is a complex type before calling this function");

            // Comparison to null needs to be handled in a special way for table splitting, but for JSON mapping is handled via
            // the regular JSON flow below.
            if ((IsNullSqlConstantExpression(left) || IsNullSqlConstantExpression(right)) && !complexType.IsMappedToJson())
            {
                // TODO: when we support optional complex types with table splitting - or projecting required complex types via optional
                // navigations - we'll be able to translate this, #31376
                throw new InvalidOperationException(RelationalStrings.CannotCompareComplexTypeToNull);
            }

            // If a complex type is the result of a subquery, then comparing its columns would mean duplicating the subquery, which would
            // be potentially very inefficient.
            // TODO: Enable this by extracting the subquery out to a common table expressions (WITH), #31237
            if (left is StructuralTypeReferenceExpression { Subquery: not null }
                || right is StructuralTypeReferenceExpression { Subquery: not null })
            {
                throw new InvalidOperationException(RelationalStrings.SubqueryOverComplexTypesNotSupported(complexType.DisplayName()));
            }

            // Generate an expression that compares each property on the left to the same property on the right; this needs to recursively
            // include all properties in nested complex types.
            var boolTypeMapping = Dependencies.TypeMappingSource.FindMapping(typeof(bool))!;
            SqlExpression? comparisons = null;

            if (!TryGenerateComparisons(complexType, left, right, ref comparisons))
            {
                result = null;
                return false;
            }

            result = comparisons;
            return true;

            // For table splitting, we simply go over all properties and generate an equality for each one; we recurse
            // into complex properties to generate a flattened list of comparisons.
            // The moment we reach a a complex property that's mapped to JSON, we stop and generate a single comparison
            // for the whole complex type.
            bool TryGenerateComparisons(IComplexType type, Expression left, Expression right, [NotNullWhen(true)] ref SqlExpression? comparisons)
            {
                if (type.IsMappedToJson())
                {
                    var leftScalar = Process(left);
                    var rightScalar = Process(right);

                    var comparison = _sqlExpressionFactory.MakeBinary(nodeType, leftScalar, rightScalar, boolTypeMapping)!;

                    comparisons = comparisons is null
                        ? comparison
                        : nodeType == ExpressionType.Equal
                            ? _sqlExpressionFactory.AndAlso(comparisons, comparison)
                            : _sqlExpressionFactory.OrElse(comparisons, comparison);

                    return true;

                    SqlExpression Process(Expression expression)
                        => expression switch
                        {
                            // When a non-collection JSON column - or a nested complex property within a JSON column - is compared,
                            // we get a StructuralTypeReferenceExpression over a JsonQueryExpression. Convert this to a
                            // JsonScalarExpression, which is our current representation for a complex JSON in the SQL tree
                            // (as opposed to in the shaper) - see #36392.
                            StructuralTypeReferenceExpression
                            { Parameter: StructuralTypeShaperExpression { ValueBufferExpression: JsonQueryExpression jsonQuery } }
                                => new JsonScalarExpression(
                                    jsonQuery.JsonColumn,
                                    jsonQuery.Path,
                                    jsonQuery.Type,
                                    jsonQuery.JsonColumn.TypeMapping,
                                    jsonQuery.IsNullable),

                            // As above, but for a complex JSON collectio
                            CollectionResultExpression { QueryExpression: JsonQueryExpression jsonQuery }
                                => new JsonScalarExpression(
                                    jsonQuery.JsonColumn,
                                    jsonQuery.Path,
                                    jsonQuery.Type,
                                    jsonQuery.JsonColumn.TypeMapping,
                                    jsonQuery.IsNullable),

                            // When an object is instantiated inline (e.g. Where(c => c.ShippingAddress == new Address { ... })), we get a SqlConstantExpression
                            // with the .NET instance. Serialize it to JSON and replace the constant (note that the type mapping will be inferred from the
                            // JSON column on other side above - important for e.g. nvarchar vs. json columns)
                            SqlConstantExpression constant
                                => new SqlConstantExpression(
                                    SerializeComplexTypeToJson(complexType, constant.Value, collection),
                                    typeof(string),
                                    typeMapping: null),

                            SqlParameterExpression parameter
                                => (SqlParameterExpression)Visit(_queryCompilationContext.RegisterRuntimeParameter(
                                    $"{RuntimeParameterPrefix}{parameter.Name}",
                                    Expression.Lambda(
                                        Expression.Call(
                                            SerializeComplexTypeToJsonMethod,
                                            Expression.Constant(complexType),
                                            Expression.MakeIndex(
                                                Expression.Property(QueryCompilationContext.QueryContextParameter, nameof(QueryContext.Parameters)),
                                                indexer: typeof(Dictionary<string, object>).GetProperty("Item", [typeof(string)]),
                                                [Expression.Constant(parameter.Name, typeof(string))]),
                                            Expression.Constant(collection)),
                                        QueryCompilationContext.QueryContextParameter))),

                            _ => throw new UnreachableException()
                        };
                }

                // We handled complex JSON above, from here we handle table splitting
                foreach (var property in type.GetProperties())
                {
                    if (TryTranslatePropertyAccess(left, property, out var leftTranslation)
                        && TryTranslatePropertyAccess(right, property, out var rightTranslation))
                    {
                        var comparison = _sqlExpressionFactory.MakeBinary(nodeType, leftTranslation, rightTranslation, boolTypeMapping)!;

                        comparisons = comparisons is null
                            ? comparison
                            : nodeType == ExpressionType.Equal
                                ? _sqlExpressionFactory.AndAlso(comparisons, comparison)
                                : _sqlExpressionFactory.OrElse(comparisons, comparison);
                    }
                    else
                    {
                        return false;
                    }
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

                    if (nestedLeft is null
                        || nestedRight is null
                        || !TryGenerateComparisons(complexProperty.ComplexType, nestedLeft, nestedRight, ref comparisons))
                    {
                        return false;
                    }
                }

                return comparisons is not null;
            }
        }
    }

    private bool TryTranslatePropertyAccess(Expression target, IPropertyBase property, [NotNullWhen(true)] out SqlExpression? translation)
    {
        var expression = CreatePropertyAccessExpression(target, property);
        translation = Translate(expression);
        return translation is not null;
    }

    Expression CreatePropertyAccessExpression(Expression target, IPropertyBase property)
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

            case SqlParameterExpression sqlParameterExpression:
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
                    $"{RuntimeParameterPrefix}{sqlParameterExpression.Name}_{property.Name}";

                return _queryCompilationContext.RegisterRuntimeParameter($"{RuntimeParameterPrefix}{sqlParameterExpression.Name}_{property.Name}", lambda);
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

                var parameterNameBuilder = new StringBuilder(RuntimeParameterPrefix)
                    .Append(chainExpression.ParameterExpression.Name)
                    .Append('_');

                foreach (var complexProperty in chainExpression.ComplexPropertyChain)
                {
                    parameterNameBuilder.Append(complexProperty.Name).Append('_');
                }

                parameterNameBuilder.Append(property.Name);

                return _queryCompilationContext.RegisterRuntimeParameter(parameterNameBuilder.ToString(), lambda);
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static T? ParameterValueExtractor<T>(
        QueryContext context,
        string baseParameterName,
        List<IComplexProperty>? complexPropertyChain,
        IProperty property)
    {
        var baseValue = context.Parameters[baseParameterName];

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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static List<TProperty?>? ParameterListValueExtractor<TEntity, TProperty>(
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

    private sealed class ParameterBasedComplexPropertyChainExpression(
        SqlParameterExpression parameterExpression,
        IComplexProperty firstComplexProperty)
        : Expression
    {
        public SqlParameterExpression ParameterExpression { get; } = parameterExpression;
        public List<IComplexProperty> ComplexPropertyChain { get; } = [firstComplexProperty];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static string? SerializeComplexTypeToJson(IComplexType complexType, object? value, bool collection)
    {
        // Note that we treat toplevel null differently: we return a relational NULL for that case. For nested nulls,
        // we return JSON null string (so you get { "foo": null })
        if (value is null)
        {
            return null;
        }

        var stream = new MemoryStream();
        var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });

        WriteJson(writer, complexType, value, collection);

        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());

        void WriteJson(Utf8JsonWriter writer, IComplexType complexType, object? value, bool collection)
        {
            if (collection)
            {
                if (value is null)
                {
                    writer.WriteNullValue();

                    return;
                }

                writer.WriteStartArray();

                foreach (var element in (IEnumerable)value)
                {
                    WriteJsonObject(writer, complexType, element);
                }

                writer.WriteEndArray();
                return;
            }

            WriteJsonObject(writer, complexType, value);
        }

        void WriteJsonObject(Utf8JsonWriter writer, IComplexType complexType, object? objectValue)
        {
            if (objectValue is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            foreach (var property in complexType.GetProperties())
            {
                var jsonPropertyName = property.GetJsonPropertyName();
                Check.DebugAssert(jsonPropertyName is not null);
                writer.WritePropertyName(jsonPropertyName);

                var propertyValue = property.GetGetter().GetClrValue(objectValue);
                if (propertyValue is null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    var jsonValueReaderWriter = property.GetJsonValueReaderWriter() ?? property.GetTypeMapping().JsonValueReaderWriter;
                    Check.DebugAssert(jsonValueReaderWriter is not null, "Missing JsonValueReaderWriter on JSON property");
                    jsonValueReaderWriter.ToJson(writer, propertyValue);
                }
            }

            foreach (var complexProperty in complexType.GetComplexProperties())
            {
                var jsonPropertyName = complexProperty.GetJsonPropertyName();
                Check.DebugAssert(jsonPropertyName is not null);
                writer.WritePropertyName(jsonPropertyName);

                var propertyValue = complexProperty.GetGetter().GetClrValue(objectValue);

                WriteJson(writer, complexProperty.ComplexType, propertyValue, complexProperty.IsCollection);
            }

            writer.WriteEndObject();
        }
    }
}

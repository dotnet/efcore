// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     Identifies Cosmos queries that can be transformed to optimized ReadItem form and performs the transformation.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class CosmosReadItemAndPartitionKeysExtractor : ExpressionVisitor
{
    private ISqlExpressionFactory _sqlExpressionFactory = null!;
    private IEntityType _entityType = null!;
    private string _rootAlias = null!;
    private bool _isPredicateCompatibleWithReadItem;
    private string? _discriminatorJsonPropertyName;
    private Dictionary<IProperty, Expression?> _jsonIdPropertyValues = null!;
    private Dictionary<IProperty, Expression?> _partitionKeyPropertyValues = null!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression ExtractPartitionKeysAndId(
        CosmosQueryCompilationContext queryCompilationContext,
        ISqlExpressionFactory sqlExpressionFactory,
        Expression expression)
    {
        _entityType = queryCompilationContext.RootEntityType
            ?? throw new UnreachableException("No root entity type was set during query processing.");
        _sqlExpressionFactory = sqlExpressionFactory;

        if (expression is not ShapedQueryExpression
            {
                QueryExpression: SelectExpression
                {
                    Sources: [{ Expression: ObjectReferenceExpression } rootSource, ..],
                    Predicate: SqlExpression predicate
                } select
            } shapedQuery)
        {
            return expression;
        }

        _rootAlias = rootSource.Alias;

        // We're going to be looking for equality comparisons on the JSON id definition properties and the partition key properties of the
        // entity type; build a dictionary where the properties are the keys, and where the values are expressions that will get populated
        // from the tree (either constants or parameters).
        // We also want to ignore the discriminator property if it's compared to our entity type's discriminator value (see below).
        _isPredicateCompatibleWithReadItem = true;
        var jsonIdProperties = _entityType.GetJsonIdDefinition()?.Properties ?? [];
        if (jsonIdProperties.Count == 0)
        {
            // No JSON ID definition - no ReadItem
            _isPredicateCompatibleWithReadItem = false;
        }

        _jsonIdPropertyValues = jsonIdProperties.ToDictionary(p => p, _ => (Expression?)null);

        var partitionKeyProperties = _entityType.GetPartitionKeyProperties();
        _partitionKeyPropertyValues = partitionKeyProperties.ToDictionary(p => p, _ => (Expression?)null);

        var discriminatorProperty = _entityType.FindDiscriminatorProperty();
        _discriminatorJsonPropertyName = discriminatorProperty?.GetJsonPropertyName();

        // Visit the predicate.
        // This will populate _jsonIdPropertyValues and _partitionKeyPropertyValues with comparisons found in the predicate, and return
        // a rewritten predicate where the partition key comparisons have been removed.
        var predicateWithoutPartitionKeyComparisons = (SqlExpression)Visit(predicate);

        // If the discriminator is part of the JSON id definition, a comparison may be missing from the predicate, since we don't add one
        // if it's not needed (e.g. only one entity type mapped to the container). For that case, add the entity type's discriminator value.
        if (discriminatorProperty is not null
            && _jsonIdPropertyValues.TryGetValue(discriminatorProperty, out var discriminatorValue)
            && discriminatorValue is null)
        {
            _jsonIdPropertyValues[discriminatorProperty] = _sqlExpressionFactory.Constant(
                _entityType.GetDiscriminatorValue(), discriminatorProperty.ClrType);
        }

        var allIdPropertiesSpecified =
            _jsonIdPropertyValues.Values.All(p => p is not null) && _jsonIdPropertyValues.Count > 0;
        var allPartitionKeyPropertiesSpecified = _partitionKeyPropertyValues.Values.All(p => p is not null);

        // First, take care of the partition key properties; if the visitation above returned a different predicate, that means that some
        // partition key comparisons were extracted (and therefore found). Lift these up to the query compilation context and rewrite
        // the SelectExpression with the new, reduced predicate.
        // Note that if the user called WithPartitionKey(), we'll have already populated the partition key property values from there, and
        // we skip lifting the predicate comparisons.
        if (allPartitionKeyPropertiesSpecified
            && queryCompilationContext.PartitionKeyPropertyValues.Count == 0)
        {
            foreach (var partitionKeyProperty in partitionKeyProperties)
            {
                queryCompilationContext.PartitionKeyPropertyValues.Add(_partitionKeyPropertyValues[partitionKeyProperty]!);
            }

            select = select.Update(
                select.Sources.ToList(),
                predicateWithoutPartitionKeyComparisons is SqlConstantExpression { Value: true }
                    ? null
                    : predicateWithoutPartitionKeyComparisons,
                select.Projection.ToList(),
                select.Orderings.ToList(),
                select.Offset,
                select.Limit);

            shapedQuery = shapedQuery.UpdateQueryExpression(select);
        }

        // Now, attempt to also transform the query to ReadItem form if possible.
        if (_isPredicateCompatibleWithReadItem
            && allIdPropertiesSpecified
            // Note that queryCompilationContext.PartitionKeyPropertyValues may have been populated with WithPartitionKey(), which has
            // a params object[] argument that gets parameterized as a single array. So the number of property values may not match the
            // number of partition key properties.
            && (partitionKeyProperties.Count == 0 || queryCompilationContext.PartitionKeyPropertyValues.Count > 0)
            // If the entity type being queried has derived types and the discriminator is part of the JSON id, we can't reliably use
            // ReadItem, since we don't know in advance which derived type the document represents.
            && (!jsonIdProperties.Contains(discriminatorProperty) || !_entityType.GetDerivedTypes().Any())
            && select is
            {
                Offset: null or SqlConstantExpression { Value: 0 },
                Limit: null or SqlConstantExpression { Value: > 0 }
            }
            // We only transform to ReadItem if the entire document (i.e. root entity type) is being projected out.
            // Using ReadItem even when a projection is present is tracked by #34163.
            && Unwrap(shapedQuery.ShaperExpression) is StructuralTypeShaperExpression { StructuralType: var projectedStructuralType }
            && projectedStructuralType == _entityType)
        {
            return shapedQuery.UpdateQueryExpression(select.WithReadItemInfo(new ReadItemInfo(_jsonIdPropertyValues!)));
        }

        return shapedQuery;

        Expression Unwrap(Expression shaper)
        {
            if (shaper is UnaryExpression { NodeType: ExpressionType.Convert } convert
                && convert.Type == typeof(object))
            {
                shaper = convert.Operand;
            }

            while (shaper is IncludeExpression { EntityExpression: var nested })
            {
                shaper = nested;
            }

            return shaper;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression node)
    {
        switch (node)
        {
            case SqlBinaryExpression { OperatorType: ExpressionType.Equal, Left: var left, Right: var right } binary:
            {
                // TODO: Handle property accesses into complex types/owned entity types, #25548
                var (scalarAccess, propertyValue) =
                    left is ScalarAccessExpression leftScalarAccess
                    && right is SqlParameterExpression or SqlConstantExpression
                        ? (leftScalarAccess, right)
                        : right is ScalarAccessExpression rightScalarAccess
                        && left is SqlParameterExpression or SqlConstantExpression
                            ? (rightScalarAccess, left)
                            : (null, null);

                if (scalarAccess?.Object is ObjectReferenceExpression { Name: var referencedSourceAlias }
                    && referencedSourceAlias == _rootAlias)
                {
                    return ProcessPropertyComparison(scalarAccess.PropertyName, propertyValue!, binary);
                }

                _isPredicateCompatibleWithReadItem = false;
                return binary;
            }

            // Bool property access (e.g. Where(b => b.BoolPartitionKey))
            case ScalarAccessExpression { PropertyName: var propertyName } scalarAccess:
                return ProcessPropertyComparison(propertyName, _sqlExpressionFactory.Constant(true), scalarAccess);

            // Negated bool property access (e.g. Where(b => !b.BoolPartitionKey))
            case SqlUnaryExpression
            {
                OperatorType: ExpressionType.Not,
                Operand: ScalarAccessExpression { PropertyName: var propertyName }
            } unary:
                return ProcessPropertyComparison(propertyName, _sqlExpressionFactory.Constant(false), unary);

            case SqlBinaryExpression { OperatorType: ExpressionType.AndAlso } binary:
                return _sqlExpressionFactory.MakeBinary(
                    ExpressionType.AndAlso,
                    (SqlExpression)Visit(binary.Left),
                    (SqlExpression)Visit(binary.Right),
                    binary.TypeMapping,
                    binary)!;

            default:
                // Anything else in the predicate, e.g. an OR, immediately disqualifies it from being a ReadItem query, and means we
                // can't extract partition key properties.
                _isPredicateCompatibleWithReadItem = false;
                return node;
        }

        SqlExpression ProcessPropertyComparison(string propertyName, SqlExpression propertyValue, SqlExpression originalExpression)
        {
            // We assume that the comparison is incompatible with ReadItem until proven otherwise, i.e. the comparison is for a JSON ID
            // property, a partition key property, or certain cases involving the discriminator property.
            var isCompatibleComparisonForReadItem = false;

            foreach (var property in _jsonIdPropertyValues.Keys)
            {
                if (propertyName == property.GetJsonPropertyName())
                {
                    if (_jsonIdPropertyValues.TryGetValue(property, out var previousValue)
                        && (previousValue is null || previousValue.Equals(propertyValue)))
                    {
                        _jsonIdPropertyValues[property] = propertyValue;
                        isCompatibleComparisonForReadItem = true;
                    }
                    break;
                }
            }

            foreach (var property in _partitionKeyPropertyValues.Keys)
            {
                // We found a comparison for a partition key property.
                // Extract its value expression and elide the comparison from the predicate - it'll be lifted out to the Cosmos SDK
                // call. Note that this is always considered a compatible comparison for ReadItem.
                if (propertyName == property.GetJsonPropertyName()
                    && _partitionKeyPropertyValues.TryGetValue(property, out var previousValue)
                    && (previousValue is null || previousValue.Equals(propertyValue)))
                {
                    _partitionKeyPropertyValues[property] = propertyValue;
                    return _sqlExpressionFactory.Constant(true);
                }
            }

            // The query contains a comparison on the discriminator property.
            // If the discriminator is part of the JSON ID property, it'll be handled below like any other JSON ID property.
            // However, if it isn't, we may need to ignore the comparison, and allow transforming to ReadItem. For example, when
            // multiple entity types are mapped to the same container, EF adds a discriminator comparison; but we want to use ReadItem
            // for these (common) cases - so we ignore the comparison for the purpose of ReadItem transformation, and validate the
            // discriminator coming back from Cosmos in the shaper, to ensure throwing for an incorrect type.
            if (isCompatibleComparisonForReadItem
                && propertyName == _discriminatorJsonPropertyName
                && propertyValue is SqlConstantExpression { Value: object specifiedDiscriminatorValue }
                && _entityType.FindDiscriminatorProperty() is IProperty discriminatorProperty
                && _entityType.GetDiscriminatorValue() is object entityDiscriminatorValue
                && discriminatorProperty.GetProviderValueComparer().Equals(specifiedDiscriminatorValue, entityDiscriminatorValue))
            {
                isCompatibleComparisonForReadItem = true;
            }

            if (!isCompatibleComparisonForReadItem)
            {
                _isPredicateCompatibleWithReadItem = false;
            }

            return originalExpression;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     Identifies Cosmos queries that can be transformed to optimized ReadItem form and performs the transformation; also extracts out
///     partition key comparisons from the predicate.
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
    private bool _discriminatorHandled;
    private string? _discriminatorJsonPropertyName;
    private Dictionary<IProperty, Expression?> _jsonIdPropertyValues = null!;
    private Dictionary<IProperty, (Expression? ValueExpression, Expression? OriginalExpression)> _partitionKeyPropertyValues = null!;

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
        var jsonIdDefinition = _entityType.GetJsonIdDefinition();

        var jsonIdProperties = jsonIdDefinition?.Properties ?? [];
        if (jsonIdProperties.Count == 0)
        {
            // No JSON ID definition - no ReadItem
            _isPredicateCompatibleWithReadItem = false;
        }

        _discriminatorHandled = jsonIdDefinition?.IncludesDiscriminator != true
            || jsonIdDefinition.DiscriminatorIsRootType;

        _jsonIdPropertyValues = jsonIdProperties.ToDictionary(p => p, _ => (Expression?)null);

        var partitionKeyProperties = _entityType.GetPartitionKeyProperties();
        _partitionKeyPropertyValues = partitionKeyProperties.ToDictionary(
            p => p, _ => (ValueExpression: (Expression?)null, (Expression?)null));

        _discriminatorJsonPropertyName = _entityType.FindDiscriminatorProperty()?.GetJsonPropertyName();

        // Visit the predicate.
        // This will populate _jsonIdPropertyValues and _partitionKeyPropertyValues with comparisons found in the predicate.
        // It does not modify the predicate (this may happen below if we lift our partition key comparisons).
        var samePredicate = (SqlExpression)Visit(predicate);
        Check.DebugAssert(ReferenceEquals(samePredicate, predicate), "Visitation shouldn't have changed the predicate.");

        var allIdPropertiesSpecified =
            _jsonIdPropertyValues.Values.All(p => p is not null) && _jsonIdPropertyValues.Count > 0;

        // First, go over the partition key properties and lift them from the predicate to the query compilation context, as possible.
        // We do this only as long as all partition key values are provided; the moment there's a gap we stop (so if PK1 and PK3 are
        // provided but not PK2, only PK1 will be lifted out).
        // Note that if the user called WithPartitionKey(), we'll have already populated the partition key property values from there; for
        // this case, we skip lifting the predicate comparisons and leave the predicate exactly as it is (it may conflict with the values
        // given in WithPartitionKey and return zero results - that's the expected behavior).
        var liftPartitionKeys = queryCompilationContext.PartitionKeyPropertyValues.Count == 0;
        foreach (var property in partitionKeyProperties)
        {
            if (liftPartitionKeys && _partitionKeyPropertyValues[property].ValueExpression is Expression valueExpression)
            {
                queryCompilationContext.PartitionKeyPropertyValues.Add(valueExpression);
            }
            else
            {
                // We either have a gap in the partition key comparisons in the predicate (so we can't lift later ones), or the user
                // specified a partition key value via WithPartitionKey. In either case, we need to not lift out comparisons and null out
                // _partitionKeyPropertyValues, to prevent us removing the comparisons from the predicate below.
                liftPartitionKeys = false;
                _partitionKeyPropertyValues[property] = (null, null);
            }
        }

        // Now, attempt to also transform the query to ReadItem form; this is only possible if all JSON ID properties were compared in the
        // predicate, and *all* partition key values are specified(in the predicate or via WithPartitionKey)
        if (_isPredicateCompatibleWithReadItem
            && allIdPropertiesSpecified
            && queryCompilationContext.PartitionKeyPropertyValues.Count == partitionKeyProperties.Count
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

        // We couldn't transform to ReadItem - some JSON ID or partition key property comparison was missing in the predicate.
        // However, comparisons might still be there for some (or all) of the partition key properties. These have already been lifted
        // up to the query compilation context (above), but we still need to remove them from the predicate.
        if (partitionKeyProperties.Count > 0 && _partitionKeyPropertyValues[partitionKeyProperties[0]].ValueExpression is not null)
        {
            var predicateWithoutPartitionKeyComparisons = (SqlExpression)new PredicateComparisonRemover(
                    _sqlExpressionFactory,
                    _partitionKeyPropertyValues.Values.Select(p => p.OriginalExpression).OfType<Expression>().ToList())
                .Visit(predicate);
            Check.DebugAssert(!ReferenceEquals(predicateWithoutPartitionKeyComparisons, predicate), "Predicate should have changed.");

            select = select.Update(
                select.Sources.ToList(),
                predicateWithoutPartitionKeyComparisons,
                select.Projection.ToList(),
                select.Orderings.ToList(),
                select.Offset,
                select.Limit);

            shapedQuery = shapedQuery.UpdateQueryExpression(select);
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
            case InExpression
            {
                Item: ScalarAccessExpression scalarAccessExpression,
                Values: SqlExpression[] sqlExpressions
            }:
            {
                // This is the case where there is more than one possible discriminator value to look for, so we can only
                // ignore it if the discriminator is either not included in the JSON id (which means it must be unique without it)
                // or the root discriminator type must be included in the JSON id, since this value is always known.
                if (_discriminatorHandled
                    && scalarAccessExpression.PropertyName == _discriminatorJsonPropertyName)
                {
                    var comparer = _entityType.FindDiscriminatorProperty()!.GetProviderValueComparer();
                    var discriminatorValues = _entityType.GetDerivedTypesInclusive().Select(e => e.GetDiscriminatorValue()).ToList();
                    if (discriminatorValues.Count == sqlExpressions.Length)
                    {
                        foreach (var sqlExpression in sqlExpressions)
                        {
                            if (sqlExpression is not SqlConstantExpression { Value: { } value }
                                || !discriminatorValues.Contains(value, comparer!))
                            {
                                _isPredicateCompatibleWithReadItem = false;
                                break;
                            }
                        }

                        return node;
                    }
                }

                _isPredicateCompatibleWithReadItem = false;
                return node;
            }

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
                    ProcessPropertyComparison(scalarAccess.PropertyName, propertyValue!, binary);
                    return node;
                }

                _isPredicateCompatibleWithReadItem = false;
                return binary;
            }

            // Bool property access (e.g. Where(b => b.BoolPartitionKey))
            case ScalarAccessExpression { PropertyName: var propertyName } scalarAccess:
                ProcessPropertyComparison(propertyName, _sqlExpressionFactory.Constant(true), scalarAccess);
                return node;

            // Negated bool property access (e.g. Where(b => !b.BoolPartitionKey))
            case SqlUnaryExpression
            {
                OperatorType: ExpressionType.Not,
                Operand: ScalarAccessExpression { PropertyName: var propertyName }
            } unary:
                ProcessPropertyComparison(propertyName, _sqlExpressionFactory.Constant(false), unary);
                return node;

            case SqlBinaryExpression { OperatorType: ExpressionType.AndAlso } binary:
                return binary.Update((SqlExpression)Visit(binary.Left), (SqlExpression)Visit(binary.Right));

            default:
                // Anything else in the predicate, e.g. an OR, immediately disqualifies it from being a ReadItem query, and means we
                // can't extract partition key properties.
                _isPredicateCompatibleWithReadItem = false;
                return node;
        }

        void ProcessPropertyComparison(string propertyName, SqlExpression propertyValue, SqlExpression originalExpression)
        {
            // We assume that the comparison is incompatible with ReadItem until proven otherwise, i.e. the comparison is for a JSON ID
            // property, a partition key property, or certain cases involving the discriminator property.
            var isCompatibleComparisonForReadItem = false;

            if (propertyName == _discriminatorJsonPropertyName
                && propertyValue is SqlConstantExpression { Value: { } specifiedDiscriminatorValue }
                && _entityType.FindDiscriminatorProperty() is { } discriminatorProperty
                && _entityType.GetDiscriminatorValue() is { } entityDiscriminatorValue
                && discriminatorProperty.GetProviderValueComparer().Equals(specifiedDiscriminatorValue, entityDiscriminatorValue))
            {
                // This is the case where there is a single leaf node with a discriminator value. We always know this value,
                // so the query never needs to drop out of ReadItem because of it.
                isCompatibleComparisonForReadItem = true;
            }
            else
            {
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
            }

            foreach (var property in _partitionKeyPropertyValues.Keys)
            {
                // We found a comparison for a partition key property.
                // Extract its value expression and elide the comparison from the predicate - it'll be lifted out to the Cosmos SDK
                // call. Note that this is always considered a compatible comparison for ReadItem.
                if (propertyName == property.GetJsonPropertyName()
                    && _partitionKeyPropertyValues.TryGetValue(property, out var previousValues)
                    && (previousValues.ValueExpression is null || previousValues.Equals(propertyValue)))
                {
                    _partitionKeyPropertyValues[property] = (ValueExpression: propertyValue, OriginalExpression: originalExpression);
                    return;
                }
            }

            if (!isCompatibleComparisonForReadItem)
            {
                _isPredicateCompatibleWithReadItem = false;
            }
        }
    }

    private sealed class PredicateComparisonRemover(ISqlExpressionFactory sqlExpressionFactory, List<Expression> comparisonsToRemove)
        : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
            => node switch
            {
                _ when comparisonsToRemove.Contains(node)
                    => sqlExpressionFactory.Constant(true),

                // This elides `AND true` from the predicate.
                // TODO: We shouldn't need to do this explicitly, see #34556.
                SqlBinaryExpression { OperatorType: ExpressionType.AndAlso } binary
                    => sqlExpressionFactory.MakeBinary(
                        ExpressionType.AndAlso,
                        (SqlExpression)Visit(binary.Left),
                        (SqlExpression)Visit(binary.Right),
                        binary.TypeMapping,
                        binary)!,

                _ => base.VisitExtension(node)
            };
    }
}

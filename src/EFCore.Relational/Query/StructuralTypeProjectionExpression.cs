// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents an entity in the projection of <see cref="SelectExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class StructuralTypeProjectionExpression : Expression
{
    private readonly IReadOnlyDictionary<IProperty, ColumnExpression> _propertyExpressionMap;
    private readonly Dictionary<INavigation, StructuralTypeShaperExpression> _ownedNavigationMap;
    private Dictionary<IComplexProperty, StructuralTypeShaperExpression>? _complexPropertyCache;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public StructuralTypeProjectionExpression(
        ITypeBase type,
        IReadOnlyDictionary<IProperty, ColumnExpression> propertyExpressionMap,
        IReadOnlyDictionary<ITableBase, TableReferenceExpression> tableMap,
        bool nullable = false,
        SqlExpression? discriminatorExpression = null)
        : this(
            type,
            propertyExpressionMap,
            new Dictionary<INavigation, StructuralTypeShaperExpression>(),
            tableMap,
            nullable,
            discriminatorExpression)
    {
    }

    private StructuralTypeProjectionExpression(
        ITypeBase type,
        IReadOnlyDictionary<IProperty, ColumnExpression> propertyExpressionMap,
        Dictionary<INavigation, StructuralTypeShaperExpression> ownedNavigationMap,
        IReadOnlyDictionary<ITableBase, TableReferenceExpression> tableMap,
        bool nullable,
        SqlExpression? discriminatorExpression = null)
    {
        StructuralType = type;
        _propertyExpressionMap = propertyExpressionMap;
        _ownedNavigationMap = ownedNavigationMap;
        TableMap = tableMap;
        IsNullable = nullable;
        DiscriminatorExpression = discriminatorExpression;
    }

    /// <summary>
    ///     The base type being projected out (entity or complex type)
    /// </summary>
    public virtual ITypeBase StructuralType { get; }

    /// <summary>
    /// TODO
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    [EntityFrameworkInternal]
    public virtual IReadOnlyDictionary<ITableBase, TableReferenceExpression> TableMap { get; }

    /// <summary>
    /// TODO
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    [EntityFrameworkInternal]
    public virtual bool IsNullable { get; }

    /// <summary>
    ///     A <see cref="SqlExpression" /> to generate discriminator for entity type.
    /// </summary>
    public virtual SqlExpression? DiscriminatorExpression { get; }

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public override Type Type
        => StructuralType.ClrType;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var changed = false;
        var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var (property, columnExpression) in _propertyExpressionMap)
        {
            var newExpression = (ColumnExpression)visitor.Visit(columnExpression);
            changed |= newExpression != columnExpression;

            propertyExpressionMap[property] = newExpression;
        }

        // We only need to visit the table map since TableReferenceUpdatingExpressionVisitor may need to modify it; it mutates
        // TableReferenceExpression (a new TableReferenceExpression is never returned), so we never need a new table map.
        foreach (var (_, tableExpression) in TableMap)
        {
            var newTableExpression = (TableReferenceExpression)visitor.Visit(tableExpression);
            Check.DebugAssert(newTableExpression == tableExpression, $"New {nameof(TableReferenceExpression)} returned during visitation!");
        }

        var discriminatorExpression = (SqlExpression?)visitor.Visit(DiscriminatorExpression);
        changed |= discriminatorExpression != DiscriminatorExpression;

        var ownedNavigationMap = new Dictionary<INavigation, StructuralTypeShaperExpression>();
        foreach (var (navigation, entityShaperExpression) in _ownedNavigationMap)
        {
            var newExpression = (StructuralTypeShaperExpression)visitor.Visit(entityShaperExpression);
            changed |= newExpression != entityShaperExpression;
            ownedNavigationMap[navigation] = newExpression;
        }

        return changed
            ? new StructuralTypeProjectionExpression(
                StructuralType, propertyExpressionMap, ownedNavigationMap, TableMap, IsNullable, discriminatorExpression)
            : this;
    }

    /// <summary>
    ///     Makes entity instance in projection nullable.
    /// </summary>
    /// <returns>A new entity projection expression which can project nullable entity.</returns>
    public virtual StructuralTypeProjectionExpression MakeNullable()
    {
        var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var (property, columnExpression) in _propertyExpressionMap)
        {
            propertyExpressionMap[property] = columnExpression.MakeNullable();
        }

        var discriminatorExpression = DiscriminatorExpression;
        if (discriminatorExpression is ColumnExpression ce)
        {
            // if discriminator is column then we need to make it nullable
            discriminatorExpression = ce.MakeNullable();
        }

        var ownedNavigationMap = new Dictionary<INavigation, StructuralTypeShaperExpression>();
        foreach (var (navigation, shaper) in _ownedNavigationMap)
        {
            if (shaper.StructuralType is IEntityType entityType && entityType.IsMappedToJson())
            {
                // even if shaper is nullable, we need to make sure key property map contains nullable keys,
                // if json entity itself is optional, the shaper would be null, but the PK of the owner entity would be non-nullable
                // initially
                var jsonQueryExpression = (JsonQueryExpression)shaper.ValueBufferExpression;
                var newJsonQueryExpression = jsonQueryExpression.MakeNullable();
                var newShaper = shaper.Update(newJsonQueryExpression).MakeNullable();
                ownedNavigationMap[navigation] = newShaper;
            }
        }

        return new StructuralTypeProjectionExpression(
            StructuralType,
            propertyExpressionMap,
            ownedNavigationMap,
            TableMap,
            nullable: true,
            discriminatorExpression);
    }

    /// <summary>
    ///     Updates the entity type being projected out to one of the derived type.
    /// </summary>
    /// <param name="derivedType">A derived entity type which should be projected.</param>
    /// <returns>A new entity projection expression which has the derived type being projected.</returns>
    public virtual StructuralTypeProjectionExpression UpdateEntityType(IEntityType derivedType)
    {
        if (StructuralType is not IEntityType entityType)
        {
            throw new UnreachableException($"{nameof(UpdateEntityType)} called on non-entity type '{StructuralType.DisplayName()}'");
        }

        if (!derivedType.GetAllBaseTypes().Contains(entityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.InvalidDerivedTypeInEntityProjection(
                    derivedType.DisplayName(), entityType.DisplayName()));
        }

        var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var (property, columnExpression) in _propertyExpressionMap)
        {
            if (derivedType.IsAssignableFrom(property.DeclaringType)
                || property.DeclaringType.IsAssignableFrom(derivedType))
            {
                propertyExpressionMap[property] = columnExpression;
            }
        }

        var ownedNavigationMap = new Dictionary<INavigation, StructuralTypeShaperExpression>();
        foreach (var (navigation, entityShaperExpression) in _ownedNavigationMap)
        {
            if (derivedType.IsAssignableFrom(navigation.DeclaringEntityType)
                || navigation.DeclaringEntityType.IsAssignableFrom(derivedType))
            {
                ownedNavigationMap[navigation] = entityShaperExpression;
            }
        }

        // Remove tables from the table map which aren't mapped to the new derived type.
        Dictionary<ITableBase, TableReferenceExpression>? newTableMap = null;
        switch (entityType.GetMappingStrategy())
        {
            case RelationalAnnotationNames.TphMappingStrategy:
                // In TPH, changing the entity type has no effect on the tables being mapped; just reuse the existing TableMap.
                break;

            case RelationalAnnotationNames.TpcMappingStrategy:
            case RelationalAnnotationNames.TptMappingStrategy:
                newTableMap = new();
                foreach (var (table, tableReferenceExpression) in TableMap)
                {
                    if (table.EntityTypeMappings.Any(m => m.TypeBase == derivedType))
                    {
                        newTableMap.Add(table, tableReferenceExpression);
                    }
                }
                break;

            case null:
                throw new UnreachableException($"Cannot be in {nameof(UpdateEntityType)} for entity type '{entityType.DisplayName()}' which has no mapping strategy");
            default:
                throw new UnreachableException("Unknown mapping strategy: " + entityType.GetMappingStrategy());
        }

        var discriminatorExpression = DiscriminatorExpression;
        if (discriminatorExpression is CaseExpression caseExpression)
        {
            var entityTypesToSelect =
                derivedType.GetConcreteDerivedTypesInclusive().Select(e => (string)e.GetDiscriminatorValue()!).ToList();
            var whenClauses = caseExpression.WhenClauses
                .Where(wc => entityTypesToSelect.Contains((string)((SqlConstantExpression)wc.Result).Value!))
                .ToList();

            discriminatorExpression = caseExpression.Update(operand: null, whenClauses, elseResult: null);
        }

        return new StructuralTypeProjectionExpression(
            derivedType, propertyExpressionMap, ownedNavigationMap, newTableMap ?? TableMap, IsNullable, discriminatorExpression);
    }

    /// <summary>
    ///     Binds a property with this structural type projection to get the SQL representation.
    /// </summary>
    /// <param name="property">A property to bind.</param>
    /// <returns>A column which is a SQL representation of the property.</returns>
    public virtual ColumnExpression BindProperty(IProperty property)
    {
        if (!StructuralType.IsAssignableFrom(property.DeclaringType)
            && !property.DeclaringType.IsAssignableFrom(StructuralType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("property", property.Name, StructuralType.DisplayName()));
        }

        return _propertyExpressionMap[property];
    }

    /// <summary>
    ///     Binds a complex property with this structural type projection to get a shaper expression for the target complex type.
    /// </summary>
    /// <param name="complexProperty">A complex property to bind.</param>
    /// <returns>A shaper expression for the target complex type.</returns>
    public virtual StructuralTypeShaperExpression BindComplexProperty(IComplexProperty complexProperty)
    {
        if (_complexPropertyCache is null || !_complexPropertyCache.TryGetValue(complexProperty, out var resultShaper))
        {
            _complexPropertyCache ??= new();
            resultShaper = _complexPropertyCache[complexProperty] =
                SelectExpression.GenerateComplexPropertyShaperExpression(this, complexProperty);
        }

        return resultShaper;
    }

    /// <summary>
    ///     Adds a navigation binding for this entity projection when the target entity type of the navigation is owned or weak.
    /// </summary>
    /// <param name="navigation">A navigation to add binding for.</param>
    /// <param name="shaper">An entity shaper expression for the target type.</param>
    public virtual void AddNavigationBinding(INavigation navigation, StructuralTypeShaperExpression shaper)
    {
        if (StructuralType is not IEntityType entityType)
        {
            throw new UnreachableException("Navigations are only supported on entity types");
        }

        if (!entityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(entityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, entityType.DisplayName()));
        }

        _ownedNavigationMap[navigation] = shaper;
    }

    /// <summary>
    ///     Binds a navigation with this entity projection to get entity shaper for the target entity type of the navigation which was
    ///     previously added using <see cref="AddNavigationBinding(INavigation, StructuralTypeShaperExpression)" /> method.
    /// </summary>
    /// <param name="navigation">A navigation to bind.</param>
    /// <returns>An entity shaper expression for the target entity type of the navigation.</returns>
    public virtual StructuralTypeShaperExpression? BindNavigation(INavigation navigation)
    {
        if (StructuralType is not IEntityType entityType)
        {
            throw new UnreachableException("Navigations are only supported on entity types");
        }

        if (!entityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(entityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, entityType.DisplayName()));
        }

        return _ownedNavigationMap.TryGetValue(navigation, out var expression)
            ? expression
            : null;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"EntityProjectionExpression: {StructuralType.ShortName()}";
}

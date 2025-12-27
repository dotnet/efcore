// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    private readonly IReadOnlyDictionary<IComplexProperty, Expression> _complexPropertyMap;

    private static readonly bool UseOldBehavior37205 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue37205", out var enabled) && enabled;

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
        IReadOnlyDictionary<IComplexProperty, Expression> complexPropertyMap,
        bool nullable = false,
        SqlExpression? discriminatorExpression = null)
        : this(
            type,
            propertyExpressionMap,
            ownedNavigationMap: [],
            complexPropertyMap,
            nullable,
            discriminatorExpression)
    {
    }

    private StructuralTypeProjectionExpression(
        ITypeBase type,
        IReadOnlyDictionary<IProperty, ColumnExpression> propertyExpressionMap,
        Dictionary<INavigation, StructuralTypeShaperExpression> ownedNavigationMap,
        IReadOnlyDictionary<IComplexProperty, Expression> complexPropertyMap,
        bool nullable,
        SqlExpression? discriminatorExpression = null)
    {
        StructuralType = type;
        _propertyExpressionMap = propertyExpressionMap;
        _ownedNavigationMap = ownedNavigationMap;
        _complexPropertyMap = complexPropertyMap;
        IsNullable = nullable;
        DiscriminatorExpression = discriminatorExpression;
    }

    /// <summary>
    ///     The base type being projected out (entity or complex type)
    /// </summary>
    public virtual ITypeBase StructuralType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual bool IsNullable { get; }

    /// <summary>
    ///     A <see cref="SqlExpression" /> to generate discriminator for entity type.
    /// </summary>
    public virtual SqlExpression? DiscriminatorExpression { get; }

    /// <summary>
    ///     The <see cref="ExpressionType" /> of the <see cref="Expression" />.
    /// </summary>
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <summary>
    ///     The <see cref="Type" /> of the value represented by this <see cref="Expression" />.
    /// </summary>
    public override Type Type
        => StructuralType.ClrType;

    /// <summary>
    ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(Expression)" /> method passing the reduced expression.
    ///     Throws an exception if the node isn't reducible.
    /// </summary>
    /// <param name="visitor">An instance of <see cref="ExpressionVisitor" />.</param>
    /// <returns>The expression being visited, or an expression which should replace it in the tree.</returns>
    /// <remarks>
    ///     Override this method to provide logic to walk the node's children.
    ///     A typical implementation will call visitor.Visit on each of its
    ///     children, and if any of them change, should return a new copy of
    ///     itself with the modified children.
    /// </remarks>
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

        var complexPropertyMap = new Dictionary<IComplexProperty, Expression>();
        foreach (var (complexProperty, complexShaper) in _complexPropertyMap)
        {
            var newComplexShaper = visitor.Visit(complexShaper);
            changed |= complexShaper != newComplexShaper;
            complexPropertyMap[complexProperty] = newComplexShaper;
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
                StructuralType, propertyExpressionMap, ownedNavigationMap, complexPropertyMap, IsNullable,
                discriminatorExpression)
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

        var complexPropertyMap = new Dictionary<IComplexProperty, Expression>();
        foreach (var (complexProperty, complexShaper) in _complexPropertyMap)
        {
            complexPropertyMap[complexProperty] = complexShaper switch
            {
                StructuralTypeShaperExpression s => s.MakeNullable(),
                CollectionResultExpression c => c,

                _ => throw new UnreachableException()
            };
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
            complexPropertyMap,
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

        Check.DebugAssert(entityType.GetMappingStrategy() is not null);

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

        var complexPropertyMap = new Dictionary<IComplexProperty, Expression>();
        foreach (var (complexProperty, complexShaper) in _complexPropertyMap)
        {
            if (derivedType.IsAssignableFrom(complexProperty.DeclaringType)
                || complexProperty.DeclaringType.IsAssignableFrom(derivedType))
            {
                complexPropertyMap[complexProperty] = complexShaper;
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
            derivedType, propertyExpressionMap, ownedNavigationMap, complexPropertyMap, IsNullable,
            discriminatorExpression);
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
    public virtual Expression BindComplexProperty(IComplexProperty complexProperty)
    {
        if (!StructuralType.IsAssignableFrom(complexProperty.DeclaringType)
            && !complexProperty.DeclaringType.IsAssignableFrom(StructuralType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("complexProperty", complexProperty.Name, StructuralType.DisplayName()));
        }

        return _complexPropertyMap[complexProperty];
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

        return _ownedNavigationMap.GetValueOrDefault(navigation);
    }

    /// <inheritdoc />
    public override string ToString()
        => $"StructuralTypeProjectionExpression: {StructuralType.ShortName()}";
}

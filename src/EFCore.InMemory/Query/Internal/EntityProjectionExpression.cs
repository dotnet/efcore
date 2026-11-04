// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityProjectionExpression : Expression, IPrintableExpression
{
    private readonly IReadOnlyDictionary<IProperty, MethodCallExpression> _readExpressionMap;
    private readonly Dictionary<INavigation, StructuralTypeShaperExpression> _navigationExpressionsCache = [];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityProjectionExpression(
        IEntityType entityType,
        IReadOnlyDictionary<IProperty, MethodCallExpression> readExpressionMap)
    {
        EntityType = entityType;
        _readExpressionMap = readExpressionMap;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     These are the properties of the entity type hierarchy, including the properties of its complex types, or only the
    ///     properties of a complex type when the projection is for a complex type (see <see cref="ProjectComplexType" />).
    /// </remarks>
    public virtual IEnumerable<IProperty> Properties
        => _readExpressionMap.Keys;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Type Type
        => EntityType.ClrType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityProjectionExpression UpdateEntityType(IEntityType derivedType)
    {
        if (!derivedType.GetAllBaseTypes().Contains(EntityType))
        {
            throw new InvalidOperationException(
                InMemoryStrings.InvalidDerivedTypeInEntityProjection(
                    derivedType.DisplayName(), EntityType.DisplayName()));
        }

        var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
        foreach (var (property, methodCallExpression) in _readExpressionMap)
        {
            var declaringEntityType = property.DeclaringType.ContainingEntityType;
            if (derivedType.IsAssignableFrom(declaringEntityType)
                || declaringEntityType.IsAssignableFrom(derivedType))
            {
                readExpressionMap[property] = methodCallExpression;
            }
        }

        return new EntityProjectionExpression(derivedType, readExpressionMap);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     Complex types are stored flattened with their containing entity type, so a projected complex type only
    ///     needs the properties of the complex type from the projection of the entity type.
    /// </remarks>
    public virtual EntityProjectionExpression ProjectComplexType(IComplexType complexType)
        => new(EntityType, complexType.GetFlattenedProperties().ToDictionary(p => p, BindProperty));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual MethodCallExpression BindProperty(IProperty property)
    {
        // Properties of complex types are bound through the entity type containing them.
        var entityType = property.DeclaringType.ContainingEntityType;
        if (!EntityType.IsAssignableFrom(entityType)
            && !entityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                InMemoryStrings.UnableToBindMemberToEntityProjection("property", property.Name, EntityType.DisplayName()));
        }

        return _readExpressionMap[property];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddNavigationBinding(INavigation navigation, StructuralTypeShaperExpression shaper)
    {
        if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                InMemoryStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, EntityType.DisplayName()));
        }

        _navigationExpressionsCache[navigation] = shaper;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StructuralTypeShaperExpression? BindNavigation(INavigation navigation)
        => !EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType)
                ? throw new InvalidOperationException(
                    InMemoryStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, EntityType.DisplayName()))
                : _navigationExpressionsCache.GetValueOrDefault(navigation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityProjectionExpression Clone()
    {
        var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>(_readExpressionMap);
        var entityProjectionExpression = new EntityProjectionExpression(EntityType, readExpressionMap);
        foreach (var (navigation, entityShaperExpression) in _navigationExpressionsCache)
        {
            entityProjectionExpression._navigationExpressionsCache[navigation] = new StructuralTypeShaperExpression(
                entityShaperExpression.StructuralType,
                ((EntityProjectionExpression)entityShaperExpression.ValueBufferExpression).Clone(),
                entityShaperExpression.IsNullable);
        }

        return entityProjectionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine(nameof(EntityProjectionExpression) + ":");
        using (expressionPrinter.Indent())
        {
            foreach (var (property, methodCallExpression) in _readExpressionMap)
            {
                expressionPrinter.Append(property + " -> ");
                expressionPrinter.Visit(methodCallExpression);
                expressionPrinter.AppendLine();
            }
        }
    }
}

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
    private readonly Dictionary<INavigation, StructuralTypeShaperExpression> _navigationExpressionsCache = new();

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
        this.EntityType = entityType;
        this._readExpressionMap = readExpressionMap;
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
    public override Type Type
        => this.EntityType.ClrType;

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
        if (!derivedType.GetAllBaseTypes().Contains(this.EntityType))
        {
            throw new InvalidOperationException(
                InMemoryStrings.InvalidDerivedTypeInEntityProjection(
                    derivedType.DisplayName(),
                    this.EntityType.DisplayName()));
        }

        var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
        foreach (var (property, methodCallExpression) in this._readExpressionMap)
        {
            if (derivedType.IsAssignableFrom(property.DeclaringType)
                || property.DeclaringType.IsAssignableFrom(derivedType))
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
    public virtual MethodCallExpression BindProperty(IProperty property)
    {
        if (property.DeclaringType is not IEntityType entityType)
        {
            if (this.EntityType != property.DeclaringType)
            {
                throw new InvalidOperationException(
                    InMemoryStrings.UnableToBindMemberToEntityProjection("property", property.Name, this.EntityType.DisplayName()));
            }
        }
        else if (!this.EntityType.IsAssignableFrom(entityType)
                 && !entityType.IsAssignableFrom(this.EntityType))
        {
            throw new InvalidOperationException(
                InMemoryStrings.UnableToBindMemberToEntityProjection("property", property.Name, this.EntityType.DisplayName()));
        }

        return this._readExpressionMap[property];
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddNavigationBinding(INavigation navigation, StructuralTypeShaperExpression shaper)
    {
        if (!this.EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(this.EntityType))
        {
            throw new InvalidOperationException(
                InMemoryStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, this.EntityType.DisplayName()));
        }

        this._navigationExpressionsCache[navigation] = shaper;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StructuralTypeShaperExpression? BindNavigation(INavigation navigation)
    {
        if (!this.EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(this.EntityType))
        {
            throw new InvalidOperationException(
                InMemoryStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, this.EntityType.DisplayName()));
        }

        return this._navigationExpressionsCache.GetValueOrDefault(navigation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityProjectionExpression Clone()
    {
        var readExpressionMap = new Dictionary<IProperty, MethodCallExpression>(this._readExpressionMap);
        var entityProjectionExpression = new EntityProjectionExpression(this.EntityType, readExpressionMap);
        foreach (var (navigation, entityShaperExpression) in this._navigationExpressionsCache)
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
            foreach (var (property, methodCallExpression) in this._readExpressionMap)
            {
                expressionPrinter.Append(property + " -> ");
                expressionPrinter.Visit(methodCallExpression);
                expressionPrinter.AppendLine();
            }
        }
    }
}

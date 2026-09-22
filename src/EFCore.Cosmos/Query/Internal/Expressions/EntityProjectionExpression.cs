// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityProjectionExpression : Expression, IPrintableExpression, IAccessExpression
{
    private readonly Dictionary<IProperty, IAccessExpression> _propertyExpressionsMap = new();
    private readonly Dictionary<INavigation, StructuralTypeShaperExpression> _navigationExpressionsMap = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityProjectionExpression(Expression @object, IEntityType entityType)
    {
        Object = @object;
        EntityType = entityType;
        PropertyName = (@object as IAccessExpression)?.PropertyName;
    }

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
    public override Type Type
        => EntityType.ClrType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Object { get; }

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
    public virtual string? PropertyName { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update(visitor.Visit(Object));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Update(Expression @object)
        => ReferenceEquals(@object, Object)
            ? this
            : new EntityProjectionExpression(@object, EntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression BindProperty(IProperty property, bool clientEval)
    {
        if (!EntityType.IsAssignableFrom(property.DeclaringType)
            && !property.DeclaringType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                CosmosStrings.UnableToBindMemberToEntityProjection("property", property.Name, EntityType.DisplayName()));
        }

        if (!_propertyExpressionsMap.TryGetValue(property, out var expression))
        {
            expression = new ScalarAccessExpression(
                Object, property.GetJsonPropertyName(), property.ClrType, property.GetTypeMapping());
            _propertyExpressionsMap[property] = expression;
        }

        if (!clientEval
            // TODO: Remove once __jObject is translated to the access root in a better fashion and
            // would not otherwise be found to be non-translatable. See issues #17670 and #14121.
            // TODO: We shouldn't be returning null from here
            && property.Name != CosmosPartitionKeyInPrimaryKeyConvention.JObjectPropertyName
            && expression.PropertyName?.Length is null or 0)
        {
            // Non-persisted property can't be translated
            return null!;
        }

        return (Expression)expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression BindNavigation(INavigation navigation, bool clientEval)
    {
        if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
            && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                CosmosStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, EntityType.DisplayName()));
        }

        if (!_navigationExpressionsMap.TryGetValue(navigation, out var expression))
        {
            // TODO: Unify ObjectAccessExpression and ObjectArrayAccessExpression
            expression = navigation.IsCollection
                ? new StructuralTypeShaperExpression(
                    navigation.TargetEntityType,
                    new ObjectArrayAccessExpression(Object, navigation),
                    nullable: true)
                : new StructuralTypeShaperExpression(
                    navigation.TargetEntityType,
                    new EntityProjectionExpression(new ObjectAccessExpression(Object, navigation), navigation.TargetEntityType),
                    nullable: !navigation.ForeignKey.IsRequiredDependent);

            _navigationExpressionsMap[navigation] = expression;
        }

        // if (!clientEval
        //     && expression.PropertyName?.Length is null or 0)
        // {
        //     // Non-persisted navigation can't be translated
        //     // TODO: We shouldn't be returning null from here
        //     return null!;
        // }

        return expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression? BindMember(
        string name,
        Type entityType,
        bool clientEval,
        out IPropertyBase? propertyBase)
        => BindMember(MemberIdentity.Create(name), entityType, clientEval, out propertyBase);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression? BindMember(
        MemberInfo memberInfo,
        Type entityType,
        bool clientEval,
        out IPropertyBase? propertyBase)
        => BindMember(MemberIdentity.Create(memberInfo), entityType, clientEval, out propertyBase);

    private Expression? BindMember(MemberIdentity member, Type? entityClrType, bool clientEval, out IPropertyBase? propertyBase)
    {
        var entityType = EntityType;
        if (entityClrType != null
            && !entityClrType.IsAssignableFrom(entityType.ClrType))
        {
            entityType = entityType.GetDerivedTypes().First(e => entityClrType.IsAssignableFrom(e.ClrType));
        }

        var property = member.MemberInfo == null
            ? entityType.FindProperty(member.Name!)
            : entityType.FindProperty(member.MemberInfo);
        if (property != null)
        {
            propertyBase = property;
            return BindProperty(property, clientEval);
        }

        var navigation = member.MemberInfo == null
            ? entityType.FindNavigation(member.Name!)
            : entityType.FindNavigation(member.MemberInfo);
        if (navigation != null)
        {
            propertyBase = navigation;
            return BindNavigation(navigation, clientEval);
        }

        // Entity member not found
        propertyBase = null;
        return null;
    }

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
                CosmosStrings.InvalidDerivedTypeInEntityProjection(
                    derivedType.DisplayName(), EntityType.DisplayName()));
        }

        return new EntityProjectionExpression(Object, derivedType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Visit(Object);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is EntityProjectionExpression entityProjectionExpression
                && Equals(entityProjectionExpression));

    private bool Equals(EntityProjectionExpression entityProjectionExpression)
        => Equals(EntityType, entityProjectionExpression.EntityType)
            && Object.Equals(entityProjectionExpression.Object);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetHashCode()
        => HashCode.Combine(EntityType, Object);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => $"EntityProjectionExpression: {EntityType.ShortName()}";
}

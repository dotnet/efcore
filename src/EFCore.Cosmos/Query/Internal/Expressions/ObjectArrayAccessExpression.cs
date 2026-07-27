// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     Represents a property access on a CosmosJSON object, which returns an array of JSON objects (structural types) (e.g.
///     <c>c.Addresses</c>).
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class ObjectArrayAccessExpression : Expression, IPrintableExpression, IAccessExpression
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ObjectArrayAccessExpression(
        Expression @object,
        IPropertyBase structuralProperty,
        StructuralTypeProjectionExpression? innerProjection = null)
    {
        ITypeBase targetType;
        string propertyName;

        switch (structuralProperty)
        {
            case INavigation navigation:
                targetType = navigation.TargetEntityType;
                propertyName = navigation.TargetEntityType.GetContainingPropertyName()
                    ?? throw new InvalidOperationException(
                        CosmosStrings.NavigationPropertyIsNotAnEmbeddedEntity(
                            navigation.DeclaringEntityType.DisplayName(), navigation.Name));
                break;
            case IComplexProperty complexProperty:
                targetType = complexProperty.ComplexType;
                propertyName = complexProperty.GetJsonPropertyName();
                break;
            default:
                throw new UnreachableException($"Unexpected structural property type: {structuralProperty.GetType().FullName}");
        }

        PropertyName = propertyName;
        Type = typeof(IEnumerable<>).MakeGenericType(targetType.ClrType);
        StructuralProperty = structuralProperty;
        Object = @object;
        InnerProjection = innerProjection
            ?? new StructuralTypeProjectionExpression(new ObjectReferenceExpression(targetType, ""), targetType);
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
    public override Type Type { get; }

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
    public virtual string PropertyName { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IPropertyBase StructuralProperty { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StructuralTypeProjectionExpression InnerProjection { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var accessExpression = visitor.Visit(Object);
        var innerProjection = visitor.Visit(InnerProjection);

        return Update(accessExpression, (StructuralTypeProjectionExpression)innerProjection);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ObjectArrayAccessExpression Update(
        Expression accessExpression,
        StructuralTypeProjectionExpression innerProjection)
        => accessExpression != Object || innerProjection != InnerProjection
            ? new ObjectArrayAccessExpression(accessExpression, StructuralProperty, innerProjection)
            : this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(ToString());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => $"{Object}[\"{PropertyName}\"]";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ObjectArrayAccessExpression arrayProjectionExpression
                && Equals(arrayProjectionExpression));

    private bool Equals(ObjectArrayAccessExpression objectArrayAccessExpression)
        => Object.Equals(objectArrayAccessExpression.Object)
            && InnerProjection.Equals(objectArrayAccessExpression.InnerProjection);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetHashCode()
        => HashCode.Combine(Object, InnerProjection);
}

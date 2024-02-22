// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ShadowValuesFactoryFactory : SnapshotFactoryFactory<IDictionary<string, object?>>
{
    private ShadowValuesFactoryFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ShadowValuesFactoryFactory Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override int GetPropertyIndex(IPropertyBase propertyBase)
        => propertyBase.GetShadowIndex();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override int GetPropertyCount(IRuntimeEntityType entityType)
        => entityType.ShadowPropertyCount;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ValueComparer? GetValueComparer(IProperty property)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MethodInfo? GetValueComparerMethod()
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool UseEntityVariable
        => false;

    private static readonly PropertyInfo DictionaryIndexer
        = typeof(IDictionary<string, object?>).GetRuntimeProperties().Single(p => p.GetIndexParameters().Length > 0);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression CreateReadShadowValueExpression(
        Expression? parameter,
        IPropertyBase property)
    {
        if (parameter == null)
        {
            return Expression.Default(property.ClrType);
        }

        if (parameter is NewArrayExpression newArrayExpression)
        {
            var valueExpression = newArrayExpression.Expressions[property.GetShadowIndex()];
            valueExpression = ((UnaryExpression)valueExpression).Operand; // Unwrap cast
            return valueExpression.Type == property.ClrType
                ? valueExpression
                : Expression.Convert(
                    valueExpression,
                    property.ClrType);
        }

        return Expression.Condition(Expression.Call(parameter, PropertyAccessorsFactory.ContainsKeyMethod, Expression.Constant(property.Name)),
            Expression.Convert(Expression.MakeIndex(
                parameter,
                DictionaryIndexer,
                new[] { Expression.Constant(property.Name) }),
                property.ClrType),
            Expression.Constant(property.Sentinel, property.ClrType));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression CreateReadValueExpression(
        Expression? parameter,
        IPropertyBase property)
        => CreateReadShadowValueExpression(parameter, property);
}

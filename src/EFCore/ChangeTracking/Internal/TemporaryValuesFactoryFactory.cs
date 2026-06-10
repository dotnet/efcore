// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TemporaryValuesFactoryFactory : SidecarValuesFactoryFactory
{
    private TemporaryValuesFactoryFactory()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new readonly TemporaryValuesFactoryFactory Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression CreateSnapshotExpression(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type? entityType,
        Expression? parameter,
        Type[] types,
        IList<IPropertyBase?> propertyBases)
    {
        var constructorExpression = Expression.Convert(
            Expression.New(
                Snapshot.CreateSnapshotType(types).GetDeclaredConstructor(types)!,
                types.Select(Expression.Default).ToArray()),
            typeof(ISnapshot));

        return constructorExpression;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ManyToManyLoaderFactory
{
    private static readonly MethodInfo _genericCreate
        = typeof(ManyToManyLoaderFactory).GetTypeInfo().GetRequiredDeclaredMethod(nameof(CreateManyToMany));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ICollectionLoader Create(ISkipNavigation skipNavigation)
        => (ICollectionLoader)_genericCreate.MakeGenericMethod(
                skipNavigation.TargetEntityType.ClrType,
                skipNavigation.DeclaringEntityType.ClrType)
            .Invoke(null, new object[] { skipNavigation })!;

    [UsedImplicitly]
    private static ICollectionLoader CreateManyToMany<TEntity, TTargetEntity>(ISkipNavigation skipNavigation)
        where TEntity : class
        where TTargetEntity : class
        => new ManyToManyLoader<TEntity, TTargetEntity>(skipNavigation);
}

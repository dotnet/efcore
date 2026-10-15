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
public class ManyToManyLoaderFactory : IManyToManyLoaderFactory
{
    private static readonly MethodInfo GenericCreate
        = typeof(ManyToManyLoaderFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateManyToMany))!;

    /// <inheritdoc />
    public virtual ICollectionLoader Create(ISkipNavigation skipNavigation)
        => (ICollectionLoader)GenericCreate.MakeGenericMethod(
                skipNavigation.TargetEntityType.ClrType,
                skipNavigation.DeclaringEntityType.ClrType)
            .Invoke(this, [skipNavigation])!;

    /// <inheritdoc />
    public virtual ICollectionLoader Create<TEntity, TSourceEntity>(ISkipNavigation skipNavigation)
        where TEntity : class
        where TSourceEntity : class
        => new ManyToManyLoader<TEntity, TSourceEntity>(skipNavigation);

    // Invoked via reflection by the non-generic Create above (GenericCreate). It deliberately
    // forwards to the virtual Create<,> so that a provider overriding Create<,> is still honored
    // on the reflection path. Do not inline this into MakeGenericMethod against Create<,> directly:
    // Create is overloaded, so GetDeclaredMethod(nameof(Create)) would be ambiguous.
    [UsedImplicitly]
    private ICollectionLoader CreateManyToMany<TEntity, TSourceEntity>(ISkipNavigation skipNavigation)
        where TEntity : class
        where TSourceEntity : class
        => Create<TEntity, TSourceEntity>(skipNavigation);
}

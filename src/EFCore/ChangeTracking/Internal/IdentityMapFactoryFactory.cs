// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class IdentityMapFactoryFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<bool, IIdentityMap> Create(IKey key)
        => (Func<bool, IIdentityMap>)GenericCreateFactory
            .MakeGenericMethod(key.GetKeyType())
            .Invoke(null, [key])!;

    private static readonly MethodInfo GenericCreateFactory
        = typeof(IdentityMapFactoryFactory).GetMethod(nameof(CreateFactory), BindingFlags.Static | BindingFlags.NonPublic)!;

    [UsedImplicitly]
    private static Func<bool, IIdentityMap> CreateFactory<TKey>(IKey key)
        where TKey : notnull
    {
        var factory = key.GetPrincipalKeyValueFactory<TKey>();

        return typeof(TKey).IsNullableType()
            ? sensitiveLoggingEnabled =>
                new NullableKeyIdentityMap<TKey>(key, factory, sensitiveLoggingEnabled)
            : sensitiveLoggingEnabled => new IdentityMap<TKey>(key, factory, sensitiveLoggingEnabled);
    }
}

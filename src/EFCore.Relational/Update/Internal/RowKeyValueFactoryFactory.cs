// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RowKeyValueFactoryFactory : IRowKeyValueFactoryFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRowKeyValueFactory Create(IUniqueConstraint key)
        => key.Columns.Count == 1
            ? (IRowKeyValueFactory)_createMethod
                .MakeGenericMethod(key.Columns.First().ProviderClrType)
                .Invoke(null, [key])!
            : new CompositeRowKeyValueFactory(key);

    private static readonly MethodInfo _createMethod = typeof(RowKeyValueFactoryFactory).GetTypeInfo()
        .GetDeclaredMethod(nameof(CreateSimpleFactory))!;

    [UsedImplicitly]
    private static IRowKeyValueFactory<TKey> CreateSimpleFactory<TKey>(IUniqueConstraint key)
        => new SimpleRowKeyValueFactory<TKey>(key);
}

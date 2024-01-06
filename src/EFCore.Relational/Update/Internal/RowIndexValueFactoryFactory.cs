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
public class RowIndexValueFactoryFactory : IRowIndexValueFactoryFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRowIndexValueFactory Create(ITableIndex index)
        => index.Columns.Count == 1
            ? (IRowIndexValueFactory)_createMethod
                .MakeGenericMethod(index.Columns.First().ProviderClrType)
                .Invoke(null, [index])!
            : new CompositeRowIndexValueFactory(index);

    private static readonly MethodInfo _createMethod = typeof(RowIndexValueFactoryFactory).GetTypeInfo()
        .GetDeclaredMethod(nameof(CreateSimple))!;

    [UsedImplicitly]
    private static IRowIndexValueFactory<TKey> CreateSimple<TKey>(ITableIndex index)
        => new SimpleRowIndexValueFactory<TKey>(index);
}

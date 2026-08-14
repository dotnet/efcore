// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryTableFactory : IInMemoryTableFactory
{
    private readonly bool _sensitiveLoggingEnabled;
    private readonly bool _nullabilityCheckEnabled;

    private readonly ConcurrentDictionary<Type, Func<IEntityType, IInMemoryTable?, bool, bool, IInMemoryTable>> _factories = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryTableFactory(
        ILoggingOptions loggingOptions,
        IInMemorySingletonOptions options)
    {
        _sensitiveLoggingEnabled = loggingOptions.IsSensitiveDataLoggingEnabled;
        _nullabilityCheckEnabled = options.IsNullabilityCheckEnabled;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IInMemoryTable Create(IEntityType entityType, IInMemoryTable? baseTable)
        => _factories.GetOrAdd(entityType.FindPrimaryKey()!.GetKeyType(), CreateTable)
            (entityType, baseTable, _sensitiveLoggingEnabled, _nullabilityCheckEnabled);

    private Func<IEntityType, IInMemoryTable?, bool, bool, IInMemoryTable> CreateTable(Type keyType)
        => (Func<IEntityType, IInMemoryTable?, bool, bool, IInMemoryTable>)typeof(InMemoryTableFactory).GetTypeInfo()
            .GetDeclaredMethod(nameof(CreateFactory))!
            .MakeGenericMethod(keyType)
            .Invoke(null, [])!;

    [UsedImplicitly]
    private static Func<IEntityType, IInMemoryTable?, bool, bool, IInMemoryTable> CreateFactory<TKey>()
        where TKey : notnull
        => (entityType, baseTable, sensitiveLoggingEnabled, nullabilityCheckEnabled)
            => new InMemoryTable<TKey>(entityType, baseTable, sensitiveLoggingEnabled, nullabilityCheckEnabled);
}

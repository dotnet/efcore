// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal
{
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

        private readonly ConcurrentDictionary<(IEntityType EntityType, IInMemoryTable? BaseTable), Func<IInMemoryTable>> _factories = new();

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
            => _factories.GetOrAdd((entityType, baseTable), e => CreateTable(e.EntityType, e.BaseTable))();

        private Func<IInMemoryTable> CreateTable(IEntityType entityType, IInMemoryTable? baseTable)
            => (Func<IInMemoryTable>)typeof(InMemoryTableFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))!
                .MakeGenericMethod(entityType.FindPrimaryKey()!.GetKeyType())
                .Invoke(null, new object?[] { entityType, baseTable, _sensitiveLoggingEnabled, _nullabilityCheckEnabled })!;

        [UsedImplicitly]
        private static Func<IInMemoryTable> CreateFactory<TKey>(
            IEntityType entityType,
            IInMemoryTable baseTable,
            bool sensitiveLoggingEnabled,
            bool nullabilityCheckEnabled)
            where TKey : notnull
            => () => new InMemoryTable<TKey>(entityType, baseTable, sensitiveLoggingEnabled, nullabilityCheckEnabled);
    }
}

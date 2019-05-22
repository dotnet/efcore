// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InMemoryTableFactory
        // WARNING: The in-memory provider is using EF internal code here. This should not be copied by other providers. See #15096
        : ChangeTracking.Internal.IdentityMapFactoryFactoryBase, IInMemoryTableFactory
    {
        private readonly bool _sensitiveLoggingEnabled;

        private readonly ConcurrentDictionary<IKey, Func<IInMemoryTable>> _factories
            = new ConcurrentDictionary<IKey, Func<IInMemoryTable>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InMemoryTableFactory([NotNull] ILoggingOptions loggingOptions)
        {
            Check.NotNull(loggingOptions, nameof(loggingOptions));

            _sensitiveLoggingEnabled = loggingOptions.IsSensitiveDataLoggingEnabled;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IInMemoryTable Create(IEntityType entityType)
            => _factories.GetOrAdd(entityType.FindPrimaryKey(), Create)();

        private Func<IInMemoryTable> Create([NotNull] IKey key)
            => (Func<IInMemoryTable>)typeof(InMemoryTableFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))
                .MakeGenericMethod(GetKeyType(key))
                .Invoke(null, new object[] { key, _sensitiveLoggingEnabled });

        [UsedImplicitly]
        private static Func<IInMemoryTable> CreateFactory<TKey>(IKey key, bool sensitiveLoggingEnabled)
            => () => new InMemoryTable<TKey>(
                // WARNING: The in-memory provider is using EF internal code here. This should not be copied by other providers. See #15096
                EntityFrameworkCore.Metadata.Internal.KeyExtensions.GetPrincipalKeyValueFactory<TKey>(key),
                sensitiveLoggingEnabled);
    }
}

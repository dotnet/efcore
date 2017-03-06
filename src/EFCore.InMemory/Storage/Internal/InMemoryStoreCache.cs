// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryStoreCache : IInMemoryStoreCache
    {
        private readonly IInMemoryTableFactory _tableFactory;

        private readonly Lazy<ConcurrentDictionary<string, IInMemoryStore>> _namedStores
            = new Lazy<ConcurrentDictionary<string, IInMemoryStore>>(
                () => new ConcurrentDictionary<string, IInMemoryStore>(), LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryStoreCache([NotNull] IInMemoryTableFactory tableFactory)
        {
            _tableFactory = tableFactory;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IInMemoryStore GetStore(string name)
            => _namedStores.Value.GetOrAdd(name, n => new InMemoryStore(_tableFactory));
    }
}

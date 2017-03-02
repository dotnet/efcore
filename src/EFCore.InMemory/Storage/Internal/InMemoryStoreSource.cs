// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryStoreSource : IInMemoryStoreSource
    {
        private readonly IInMemoryStoreCache _storeCache;
        private readonly IInMemoryTableFactory _tableFactory;
        private IInMemoryStore _transientStore;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryStoreSource(
            [NotNull] IInMemoryStoreCache storeCache,
            [NotNull] IInMemoryTableFactory tableFactory)
        {
            _storeCache = storeCache;
            _tableFactory = tableFactory;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IInMemoryStore GetPersistentStore(string name) => _storeCache.GetStore(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IInMemoryStore GetTransientStore()
            => _transientStore ?? (_transientStore = new InMemoryStore(_tableFactory));
    }
}

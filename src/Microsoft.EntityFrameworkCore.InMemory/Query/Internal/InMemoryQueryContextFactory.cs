// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryQueryContextFactory : QueryContextFactory
    {
        private readonly IInMemoryStore _store;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryQueryContextFactory(
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IConcurrencyDetector concurrencyDetector,
            [NotNull] IInMemoryStoreSource storeSource,
            [NotNull] IDbContextOptions contextOptions)
            : base(currentContext, concurrencyDetector)
        {
            _store = storeSource.GetStore(contextOptions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override QueryContext Create()
            => new InMemoryQueryContext(CreateQueryBuffer, _store, StateManager, ConcurrencyDetector);
    }
}

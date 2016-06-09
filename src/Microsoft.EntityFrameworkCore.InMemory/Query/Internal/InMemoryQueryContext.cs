// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryQueryContext : QueryContext
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryQueryContext(
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] IInMemoryStore store,
            [NotNull] LazyRef<IStateManager> stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector)
            : base(
                Check.NotNull(queryBufferFactory, nameof(queryBufferFactory)),
                Check.NotNull(stateManager, nameof(stateManager)),
                Check.NotNull(concurrencyDetector, nameof(concurrencyDetector)))
        {
            Store = store;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IInMemoryStore Store { get; }
    }
}

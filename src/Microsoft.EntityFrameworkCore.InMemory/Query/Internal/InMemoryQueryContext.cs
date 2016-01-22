// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class InMemoryQueryContext : QueryContext
    {
        public InMemoryQueryContext(
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] IInMemoryStore store,
            [NotNull] IStateManager stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector)
            : base(
                Check.NotNull(queryBufferFactory, nameof(queryBufferFactory)),
                Check.NotNull(stateManager, nameof(stateManager)),
                Check.NotNull(concurrencyDetector, nameof(concurrencyDetector)))
        {
            Store = store;
        }

        public virtual IInMemoryStore Store { get; }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryContextFactory : IQueryContextFactory
    {
        protected QueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector,
            [NotNull] IChangeDetector changeDetector)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(concurrencyDetector, nameof(concurrencyDetector));
            Check.NotNull(changeDetector, nameof(changeDetector));

            StateManager = stateManager;
            ConcurrencyDetector = concurrencyDetector;
            ChangeDetector = changeDetector;
        }

        protected virtual IQueryBuffer CreateQueryBuffer()
            => new QueryBuffer(StateManager, ChangeDetector);

        protected virtual IChangeDetector ChangeDetector { get; }

        protected virtual IStateManager StateManager { get; }

        protected virtual IConcurrencyDetector ConcurrencyDetector { get; }

        public abstract QueryContext Create();
    }
}

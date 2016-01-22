// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QueryContextFactory : IQueryContextFactory
    {
        protected QueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(concurrencyDetector, nameof(concurrencyDetector));

            StateManager = stateManager;
            ConcurrencyDetector = concurrencyDetector;
        }

        protected virtual IQueryBuffer CreateQueryBuffer()
            => new QueryBuffer(StateManager);

        protected virtual IStateManager StateManager { get; }

        protected virtual IConcurrencyDetector ConcurrencyDetector { get; }

        public abstract QueryContext Create();
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory.Query
{
    public class InMemoryQueryContext : QueryContext
    {
        public InMemoryQueryContext(
            [NotNull] ILogger logger,
            [NotNull] IQueryBuffer queryBuffer,
            [NotNull] IStateManager stateManager,
            [NotNull] IInMemoryDatabase database)
            : base(
                Check.NotNull(logger, nameof(logger)),
                Check.NotNull(queryBuffer, nameof(queryBuffer)),
                Check.NotNull(stateManager, nameof(stateManager)))
        {
            Database = database;
        }

        public virtual IInMemoryDatabase Database { get; }
    }
}

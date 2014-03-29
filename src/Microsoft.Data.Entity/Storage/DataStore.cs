// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStore
    {
        public virtual Task<int> SaveChangesAsync(
            [NotNull] IEnumerable<StateEntry> stateEntries,
            [NotNull] IModel model,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public virtual IAsyncEnumerable<TResult> Query<TResult>(
            [NotNull] QueryModel queryModel,
            [NotNull] IModel model, 
            [NotNull] StateManager stateManager)
        {
            throw new NotImplementedException();
        }
    }
}

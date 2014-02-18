// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStore
    {
        public virtual Task<int> SaveChangesAsync([NotNull] IEnumerable<EntityEntry> entityEntries)
        {
            return Task.FromResult(0);
        }
    }
}

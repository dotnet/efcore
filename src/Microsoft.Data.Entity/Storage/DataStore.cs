// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStore
    {
        public virtual Task<int> SaveChangesAsync(
            [NotNull] IEnumerable<StateEntry> stateEntries, [NotNull] IModel model)
        {
            throw new NotImplementedException();
        }

        public virtual IAsyncEnumerable<object[]> Read([NotNull] Type type, [NotNull] IModel model)
        {
            return Read(model.GetEntityType(type));
        }

        public virtual IAsyncEnumerable<object[]> Read([NotNull] IEntityType type)
        {
            throw new NotImplementedException();
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreCreator
    {
        public abstract Task CreateAsync(
            [NotNull] IModel model, CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken));

        public abstract void Create([NotNull] IModel model);

        public abstract bool Exists();

        public abstract void Delete();
    }
}

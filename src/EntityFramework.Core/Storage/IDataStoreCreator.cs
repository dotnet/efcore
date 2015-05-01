// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Storage
{
    public interface IDataStoreCreator
    {
        bool EnsureDeleted([NotNull] IModel model);
        Task<bool> EnsureDeletedAsync([NotNull] IModel model, CancellationToken cancellationToken = default(CancellationToken));
        bool EnsureCreated([NotNull] IModel model);
        Task<bool> EnsureCreatedAsync([NotNull] IModel model, CancellationToken cancellationToken = default(CancellationToken));
    }
}

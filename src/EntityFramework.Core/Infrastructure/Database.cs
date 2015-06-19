// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class Database : IAccessor<IServiceProvider>
    {
        private readonly DbContext _context;
        private readonly IDataStoreCreator _dataStoreCreator;

        public Database(
            [NotNull] DbContext context,
            [NotNull] IDataStoreCreator dataStoreCreator)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(dataStoreCreator, nameof(dataStoreCreator));

            _context = context;
            _dataStoreCreator = dataStoreCreator;
        }

        // TODO: Make sure API docs say that return value indicates whether or not the database or tables were created
        public virtual bool EnsureCreated() => _dataStoreCreator.EnsureCreated();

        // TODO: Make sure API docs say that return value indicates whether or not the database or tables were created
        public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
            => _dataStoreCreator.EnsureCreatedAsync(cancellationToken);

        // TODO: Make sure API docs say that return value indicates whether or not the database was deleted
        public virtual bool EnsureDeleted() => _dataStoreCreator.EnsureDeleted();

        // TODO: Make sure API docs say that return value indicates whether or not the database was deleted
        public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
            => _dataStoreCreator.EnsureDeletedAsync(cancellationToken);

        IServiceProvider IAccessor<IServiceProvider>.Service => ((IAccessor<IServiceProvider>)_context).Service;
    }
}

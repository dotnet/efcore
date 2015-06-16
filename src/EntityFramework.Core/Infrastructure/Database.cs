// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Infrastructure
{
    public abstract class Database : IAccessor<IDataStoreCreator>, IAccessor<ILogger>, IAccessor<IModel>, IAccessor<IServiceProvider>
    {
        private readonly DbContext _context;
        private readonly IDataStoreCreator _dataStoreCreator;
        private readonly LazyRef<ILogger> _logger;

        protected Database(
            [NotNull] DbContext context,
            [NotNull] IDataStoreCreator dataStoreCreator,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(dataStoreCreator, nameof(dataStoreCreator));
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _context = context;
            _dataStoreCreator = dataStoreCreator;
            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<Database>);
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

        IDataStoreCreator IAccessor<IDataStoreCreator>.Service => _dataStoreCreator;

        ILogger IAccessor<ILogger>.Service => _logger.Value;

        IModel IAccessor<IModel>.Service => _context.Model;

        IServiceProvider IAccessor<IServiceProvider>.Service => ((IAccessor<IServiceProvider>)_context).Service;
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class Database : IDbContextConfigurationAdapter
    {
        private readonly DbContextConfiguration _configuration;
        private readonly LazyRef<ILogger> _logger;

        public Database([NotNull] DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            _configuration = configuration;
            _logger = new LazyRef<ILogger>(() => Configuration.LoggerFactory.Create<Database>());
        }

        protected virtual DbContextConfiguration Configuration
        {
            get { return _configuration; }
        }

        DbContextConfiguration IDbContextConfigurationAdapter.Configuration
        {
            get { return Configuration; }
        }

        protected virtual ILogger Logger
        {
            get { return _logger.Value; }
        }

        public virtual DataStoreConnection Connection
        {
            get { return _configuration.Connection; }
        }

        // TODO: Make sure API docs say that return value indicates whether or not the database or tables were created
        public virtual bool EnsureCreated()
        {
            return _configuration.DataStoreCreator.EnsureCreated(_configuration.Model);
        }

        // TODO: Make sure API docs say that return value indicates whether or not the database or tables were created
        public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _configuration.DataStoreCreator.EnsureCreatedAsync(_configuration.Model, cancellationToken);
        }

        // TODO: Make sure API docs say that return value indicates whether or not the database was deleted
        public virtual bool EnsureDeleted()
        {
            return _configuration.DataStoreCreator.EnsureDeleted(_configuration.Model);
        }

        // TODO: Make sure API docs say that return value indicates whether or not the database was deleted
        public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _configuration.DataStoreCreator.EnsureDeletedAsync(_configuration.Model, cancellationToken);
        }
    }
}

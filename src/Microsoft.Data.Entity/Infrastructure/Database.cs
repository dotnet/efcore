// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class Database
    {
        private readonly DbContextConfiguration _configuration;

        public Database([NotNull] DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            _configuration = configuration;
        }

        protected virtual DbContextConfiguration Configuration
        {
            get { return _configuration; }
        }

        public virtual DataStoreConnection Connection
        {
            get { return _configuration.Connection; }
        }

        public virtual bool EnsureCreated()
        {
            return _configuration.DataStoreCreator.EnsureCreated(_configuration.Model);
        }

        public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _configuration.DataStoreCreator.EnsureCreatedAsync(_configuration.Model, cancellationToken);
        }

        public virtual bool EnsureDeleted()
        {
            return _configuration.DataStoreCreator.EnsureDeleted();
        }

        public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _configuration.DataStoreCreator.EnsureDeletedAsync(cancellationToken);
        }
    }
}

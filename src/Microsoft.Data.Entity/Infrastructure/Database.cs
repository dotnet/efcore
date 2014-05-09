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

        public virtual DataStoreConnection Connection
        {
            get { return _configuration.Connection; }
        }

        public virtual void Create()
        {
            _configuration.DataStoreCreator.Create(_configuration.Model);
        }

        public virtual void Delete()
        {
            _configuration.DataStoreCreator.Delete();
        }

        public virtual bool Exists()
        {
            return _configuration.DataStoreCreator.Exists();
        }

        public virtual Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _configuration.DataStoreCreator.CreateAsync(_configuration.Model, cancellationToken);
        }

        public virtual Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _configuration.DataStoreCreator.DeleteAsync(cancellationToken);
        }

        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _configuration.DataStoreCreator.ExistsAsync(cancellationToken);
        }
    }
}

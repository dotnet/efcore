// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalDatabase : Database
    {
        public RelationalDatabase([NotNull] DbContextConfiguration configuration)
            : base(configuration)
        {
        }

        public new virtual RelationalConnection Connection
        {
            get { return (RelationalConnection)Configuration.Connection; }
        }

        public virtual void Create()
        {
            RelationalDataStoreCreator.Create();
        }

        public virtual Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.CreateAsync(cancellationToken);
        }

        public virtual void CreateTables()
        {
            RelationalDataStoreCreator.CreateTables(Configuration.Model);
        }

        public virtual Task CreateTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.CreateTablesAsync(Configuration.Model, cancellationToken);
        }

        public virtual void Delete()
        {
            RelationalDataStoreCreator.Delete();
        }

        public virtual Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.DeleteAsync(cancellationToken);
        }

        public virtual bool Exists()
        {
            return RelationalDataStoreCreator.Exists();
        }

        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.ExistsAsync(cancellationToken);
        }

        public virtual bool HasTables()
        {
            return RelationalDataStoreCreator.HasTables();
        }

        public virtual Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RelationalDataStoreCreator.HasTablesAsync(cancellationToken);
        }

        private RelationalDataStoreCreator RelationalDataStoreCreator
        {
            get { return ((RelationalDataStoreCreator)Configuration.DataStoreCreator); }
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreCreator
    {
        public abstract bool Exists();

        public abstract Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken));

        public abstract void Create();

        public abstract Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken));

        public abstract void Delete();

        public abstract Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken));

        public abstract void CreateTables([NotNull] IModel model);

        public abstract Task CreateTablesAsync(
            [NotNull] IModel model, CancellationToken cancellationToken = default(CancellationToken));

        public abstract bool HasTables();

        public abstract Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken));

        public virtual bool EnsureDeleted()
        {
            if (Exists())
            {
                Delete();
                return true;
            }
            return false;
        }

        public async virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await ExistsAsync(cancellationToken))
            {
                await DeleteAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public virtual bool EnsureCreated([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            if (!Exists())
            {
                Create();
                CreateTables(model);
                return true;
            }

            if (!HasTables())
            {
                CreateTables(model);
                return true;
            }

            return false;
        }

        public async virtual Task<bool> EnsureCreatedAsync(
            [NotNull] IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!await ExistsAsync(cancellationToken))
            {
                await CreateAsync(cancellationToken);
                await CreateTablesAsync(model, cancellationToken);
                return true;
            }

            if (!await HasTablesAsync(cancellationToken))
            {
                await CreateTablesAsync(model, cancellationToken);
                return true;
            }

            return false;
        }
    }
}

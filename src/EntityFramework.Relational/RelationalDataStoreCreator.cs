// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalDataStoreCreator : IRelationalDataStoreCreator
    {
        public abstract bool Exists();

        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Exists());
        }

        public abstract void Create();

        public virtual Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Create();

            return Task.FromResult(0);
        }

        public abstract void Delete();

        public virtual Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Delete();

            return Task.FromResult(0);
        }

        public abstract void CreateTables(IModel model);

        public virtual Task CreateTablesAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            CreateTables(model);

            return Task.FromResult(0);
        }

        public abstract bool HasTables();

        public virtual Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(HasTables());
        }

        public virtual bool EnsureDeleted(IModel model)
        {
            Check.NotNull(model, nameof(model));

            if (Exists())
            {
                Delete();
                return true;
            }
            return false;
        }

        public virtual async Task<bool> EnsureDeletedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(model, nameof(model));

            if (await ExistsAsync(cancellationToken).WithCurrentCulture())
            {
                await DeleteAsync(cancellationToken).WithCurrentCulture();

                return true;
            }
            return false;
        }

        public virtual bool EnsureCreated(IModel model)
        {
            Check.NotNull(model, nameof(model));

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

        public virtual async Task<bool> EnsureCreatedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(model, nameof(model));

            if (!await ExistsAsync(cancellationToken).WithCurrentCulture())
            {
                await CreateAsync(cancellationToken).WithCurrentCulture();
                await CreateTablesAsync(model, cancellationToken).WithCurrentCulture();

                return true;
            }

            if (!await HasTablesAsync(cancellationToken).WithCurrentCulture())
            {
                await CreateTablesAsync(model, cancellationToken).WithCurrentCulture();

                return true;
            }

            return false;
        }
    }
}

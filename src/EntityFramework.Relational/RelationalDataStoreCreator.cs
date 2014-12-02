// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalDataStoreCreator : DataStoreCreator
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

        public override bool EnsureDeleted(IModel model)
        {
            if (Exists())
            {
                Delete();
                return true;
            }
            return false;
        }

        public override async Task<bool> EnsureDeletedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await ExistsAsync(cancellationToken).WithCurrentCulture())
            {
                await DeleteAsync(cancellationToken).WithCurrentCulture();

                return true;
            }
            return false;
        }

        public override bool EnsureCreated(IModel model)
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

        public override async Task<bool> EnsureCreatedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(model, "model");

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

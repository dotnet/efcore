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
    public class DatabaseFacade : IAccessor<IServiceProvider>
    {
        private readonly DbContext _context;

        public DatabaseFacade([NotNull] DbContext context)
        {
            Check.NotNull(context, nameof(context));

            _context = context;
        }

        // TODO: Make sure API docs say that return value indicates whether or not the database or tables were created
        public virtual bool EnsureCreated() => this.GetService<IDatabaseCreator>().EnsureCreated();

        // TODO: Make sure API docs say that return value indicates whether or not the database or tables were created
        public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
            => this.GetService<IDatabaseCreator>().EnsureCreatedAsync(cancellationToken);

        // TODO: Make sure API docs say that return value indicates whether or not the database was deleted
        public virtual bool EnsureDeleted() => this.GetService<IDatabaseCreator>().EnsureDeleted();

        // TODO: Make sure API docs say that return value indicates whether or not the database was deleted
        public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
            => this.GetService<IDatabaseCreator>().EnsureDeletedAsync(cancellationToken);

        IServiceProvider IAccessor<IServiceProvider>.Service => ((IAccessor<IServiceProvider>)_context).Service;
    }
}

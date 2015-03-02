// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreSource<TStoreServices, TOptionsExtension> : IDataStoreSource
        where TStoreServices : class, IDataStoreServices
        where TOptionsExtension : DbContextOptionsExtension
    {
        private readonly DbContextServices _services;
        private readonly IDbContextOptions _options;

        protected DataStoreSource([NotNull] DbContextServices services, [NotNull] IDbContextOptions options)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(options, nameof(options));

            _services = services;
            _options = options;
        }

        public virtual IDataStoreServices StoreServices => _services.ServiceProvider.GetRequiredService<TStoreServices>();

        public virtual bool IsConfigured => _options.Extensions.OfType<TOptionsExtension>().Any();

        public abstract string Name { get; }

        public virtual bool IsAvailable => IsConfigured;

        public virtual DbContextOptions ContextOptions => (DbContextOptions)_services.ContextOptions;

        public virtual void AutoConfigure()
        {
        }
    }
}

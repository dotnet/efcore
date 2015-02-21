// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreSource<TStoreServices, TOptionsExtension> : DataStoreSource
        where TStoreServices : DataStoreServices
        where TOptionsExtension : DbContextOptionsExtension
    {
        private readonly DbContextServices _services;
        private readonly DbContextService<IDbContextOptions> _options;

        protected DataStoreSource([NotNull] DbContextServices services, [NotNull] DbContextService<IDbContextOptions> options)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(options, nameof(options));

            _services = services;
            _options = options;
        }

        public override DataStoreServices StoreServices
        {
            get
            {
                // Using service locator here so that all services for every provider are not always
                // eagerly loaded during the provider selection process.
                return _services.ScopedServiceProvider.GetRequiredServiceChecked<TStoreServices>();
            }
        }

        public override bool IsConfigured
        {
            get { return _options.Service.Extensions.OfType<TOptionsExtension>().Any(); }
        }

        public override bool IsAvailable
        {
            get { return IsConfigured; }
        }

        public override DbContextOptions ContextOptions
        {
            get { return (DbContextOptions)_services.ContextOptions; }
        }
    }
}

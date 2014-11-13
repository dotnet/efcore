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
        private readonly DbContextConfiguration _configuration;
        private readonly LazyRef<IDbContextOptions> _options;

        protected DataStoreSource([NotNull] DbContextConfiguration configuration, [NotNull] LazyRef<IDbContextOptions> options)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(options, "options");

            _configuration = configuration;
            _options = options;
        }

        public override DataStoreServices StoreServices
        {
            get
            {
                // Using service locator here so that all services for every provider are not always
                // eagerly loaded during the provider selection process.
                return _configuration.ScopedServiceProvider.GetRequiredServiceChecked<TStoreServices>();
            }
        }

        public override bool IsConfigured
        {
            get { return _options.Value.Extensions.OfType<TOptionsExtension>().Any(); }
        }

        public override bool IsAvailable
        {
            get { return IsConfigured; }
        }

        public override DbContextOptions ContextOptions
        {
            get { return (DbContextOptions)_configuration.ContextOptions; }
        }
    }
}

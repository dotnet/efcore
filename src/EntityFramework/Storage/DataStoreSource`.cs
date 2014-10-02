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

        protected DataStoreSource([NotNull] DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            _configuration = configuration;
        }

        public override DataStoreServices StoreServices
        {
            get { return _configuration.Services.ServiceProvider.GetService<TStoreServices>(); }
        }

        public override bool IsConfigured
        {
            get { return _configuration.ContextOptions.Extensions.OfType<TOptionsExtension>().Any(); }
        }

        public override bool IsAvailable
        {
            // TODO: Consider finding connection string in config file by convention
            // Issue #763
            get { return IsConfigured; }
        }
    }
}

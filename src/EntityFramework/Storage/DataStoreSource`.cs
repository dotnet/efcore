// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
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
            get
            {
                try
                {
                    return _configuration.Services.ServiceProvider.GetService<TStoreServices>();
                }
                catch (TargetInvocationException ex)
                {
                    // See DependencyInjection Issue #127
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    return null;
                }
            }
        }

        public override bool IsConfigured
        {
            get { return _configuration.ContextOptions.Extensions.OfType<TOptionsExtension>().Any(); }
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

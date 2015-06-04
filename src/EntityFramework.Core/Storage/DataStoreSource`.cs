// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreSource<TStoreServices, TOptionsExtension> : IDataStoreSource
        where TStoreServices : class, IDataStoreServices
        where TOptionsExtension : class, IEntityOptionsExtension
    {
        public virtual IDataStoreServices GetStoreServices(IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            return serviceProvider.GetRequiredService<TStoreServices>();
        }

        public virtual bool IsConfigured(IEntityOptions options)
        {
            Check.NotNull(options, nameof(options));

            return options.Extensions.OfType<TOptionsExtension>().Any();
        }

        public abstract string Name { get; }

        public abstract void AutoConfigure(EntityOptionsBuilder optionsBuilder);
    }
}

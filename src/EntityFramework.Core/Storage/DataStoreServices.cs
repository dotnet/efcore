// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreServices : IDataStoreServices
    {
        protected DataStoreServices([NotNull] IServiceProvider services)
        {
            Check.NotNull(services, nameof(services));

            Services = services;
        }

        protected virtual IServiceProvider Services { get; }

        protected virtual TService GetService<TService>() => Services.GetRequiredService<TService>();

        public virtual IModelBuilderFactory ModelBuilderFactory => GetService<ModelBuilderFactory>();
        public virtual IValueGeneratorSelector ValueGeneratorSelector => GetService<ValueGeneratorSelector>();

        public abstract IDatabaseFactory DatabaseFactory { get; }
        public abstract IDataStore Store { get; }
        public abstract IDataStoreCreator Creator { get; }
        public abstract IDataStoreConnection Connection { get; }
        public abstract IModelSource ModelSource { get; }
        public abstract IQueryContextFactory QueryContextFactory { get; }
        public abstract IValueGeneratorCache ValueGeneratorCache { get; }
    }
}

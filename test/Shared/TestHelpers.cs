// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.Tests
{
    public static class TestHelpers
    {
        public static DbContextOptions CreateOptions(IModel model)
        {
            return new DbContextOptions().UseModel(model).UseProviderOptions();
        }

        public static DbContextOptions CreateOptions()
        {
            return new DbContextOptions();
        }

        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services
                .AddEntityFramework()
                .AddProviderServices();
            return services.BuildServiceProvider();
        }

        public static IServiceProvider CreateContextServices(IServiceProvider serviceProvider, IModel model)
        {
            return ((IDbContextServices)new DbContext(serviceProvider, CreateOptions(model))).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices(IServiceProvider serviceProvider)
        {
            return ((IDbContextServices)new DbContext(serviceProvider, CreateOptions())).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices(IModel model)
        {
            return ((IDbContextServices)new DbContext(CreateServiceProvider(), CreateOptions(model))).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices()
        {
            return ((IDbContextServices)new DbContext(CreateServiceProvider(), CreateOptions())).ScopedServiceProvider;
        }

        public static Model BuildModelFor<TEntity>()
        {
            var builder = new ModelBuilder();
            builder.Entity<TEntity>();
            return builder.Model;
        }

        public static StateEntry CreateStateEntry<TEntity>(
            IModel model, EntityState entityState = EntityState.Unknown, TEntity entity = null)
            where TEntity : class, new()
        {
            var entry = CreateContextServices(model)
                .GetRequiredService<StateManager>()
                .GetOrCreateEntry(entity ?? new TEntity());

            entry.EntityState = entityState;

            return entry;
        }
    }
}

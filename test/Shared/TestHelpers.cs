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
            return new DbContextOptions().UseProviderOptions();
        }

        public static IServiceProvider CreateServiceProvider(IServiceCollection customServices = null)
        {
            var services = new ServiceCollection();
            services
                .AddEntityFramework()
                .AddProviderServices();

            if (customServices != null)
            {
                foreach (var service in customServices)
                {
                    services.Add(service);
                }
            }

            return services.BuildServiceProvider();
        }

        public static DbContext CreateContext(IServiceProvider serviceProvider, IModel model)
        {
            return new DbContext(serviceProvider, CreateOptions(model));
        }

        public static DbContext CreateContext(IServiceProvider serviceProvider, DbContextOptions options)
        {
            return new DbContext(serviceProvider, options);
        }

        public static DbContext CreateContext(IServiceProvider serviceProvider)
        {
            return new DbContext(serviceProvider, CreateOptions());
        }

        public static DbContext CreateContext(IModel model)
        {
            return new DbContext(CreateServiceProvider(), CreateOptions(model));
        }

        public static DbContext CreateContext(DbContextOptions options)
        {
            return new DbContext(CreateServiceProvider(), options);
        }

        public static DbContext CreateContext()
        {
            return new DbContext(CreateServiceProvider(), CreateOptions());
        }

        public static DbContext CreateContext(IServiceCollection customServices, IModel model)
        {
            return new DbContext(CreateServiceProvider(customServices), CreateOptions(model));
        }

        public static DbContext CreateContext(IServiceCollection customServices, DbContextOptions options)
        {
            return new DbContext(CreateServiceProvider(customServices), options);
        }

        public static DbContext CreateContext(IServiceCollection customServices)
        {
            return new DbContext(CreateServiceProvider(customServices), CreateOptions());
        }

        public static IServiceProvider CreateContextServices(IServiceProvider serviceProvider, IModel model)
        {
            return ((IDbContextServices)CreateContext(serviceProvider, model)).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices(IServiceProvider serviceProvider, DbContextOptions options)
        {
            return ((IDbContextServices)CreateContext(serviceProvider, options)).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices(IServiceProvider serviceProvider)
        {
            return ((IDbContextServices)CreateContext(serviceProvider)).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices(IModel model)
        {
            return ((IDbContextServices)CreateContext(model)).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices(DbContextOptions options)
        {
            return ((IDbContextServices)CreateContext(options)).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices()
        {
            return ((IDbContextServices)CreateContext()).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices(IServiceCollection customServices, IModel model)
        {
            return ((IDbContextServices)CreateContext(customServices, model)).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices(IServiceCollection customServices, DbContextOptions options)
        {
            return ((IDbContextServices)CreateContext(customServices, options)).ScopedServiceProvider;
        }

        public static IServiceProvider CreateContextServices(IServiceCollection customServices)
        {
            return ((IDbContextServices)CreateContext(customServices)).ScopedServiceProvider;
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

            entry.SetEntityState(entityState);

            return entry;
        }
    }
}

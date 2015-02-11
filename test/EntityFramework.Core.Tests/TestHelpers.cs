// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.DependencyInjection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.Tests
{
    public class TestHelpers
    {
        protected TestHelpers()
        {
        }

        public static TestHelpers Instance { get; } = new TestHelpers();

        public DbContextOptions CreateOptions(IModel model)
        {
            return UseProviderOptions(new DbContextOptions().UseModel(model));
        }

        public DbContextOptions CreateOptions()
        {
            return UseProviderOptions(new DbContextOptions());
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection customServices = null)
        {
            return CreateServiceProvider(customServices, AddProviderServices);
        }

        public IServiceProvider CreateServiceProvider(
            IServiceCollection customServices,
            Func<EntityFrameworkServicesBuilder, EntityFrameworkServicesBuilder> addProviderServices)
        {
            var services = new ServiceCollection();
            addProviderServices(services.AddEntityFramework());

            if (customServices != null)
            {
                foreach (var service in customServices)
                {
                    services.Add(service);
                }
            }

            return services.BuildServiceProvider();
        }

        protected virtual EntityFrameworkServicesBuilder AddProviderServices(EntityFrameworkServicesBuilder builder)
        {
            return builder.AddInMemoryStore();
        }

        protected virtual DbContextOptions UseProviderOptions(DbContextOptions options)
        {
            return options;
        }

        public DbContext CreateContext(IServiceProvider serviceProvider, IModel model)
        {
            return new DbContext(serviceProvider, CreateOptions(model));
        }

        public DbContext CreateContext(IServiceProvider serviceProvider, DbContextOptions options)
        {
            return new DbContext(serviceProvider, options);
        }

        public DbContext CreateContext(IServiceProvider serviceProvider)
        {
            return new DbContext(serviceProvider, CreateOptions());
        }

        public DbContext CreateContext(IModel model)
        {
            return new DbContext(CreateServiceProvider(), CreateOptions(model));
        }

        public DbContext CreateContext(DbContextOptions options)
        {
            return new DbContext(CreateServiceProvider(), options);
        }

        public DbContext CreateContext()
        {
            return new DbContext(CreateServiceProvider(), CreateOptions());
        }

        public DbContext CreateContext(IServiceCollection customServices, IModel model)
        {
            return new DbContext(CreateServiceProvider(customServices), CreateOptions(model));
        }

        public DbContext CreateContext(IServiceCollection customServices, DbContextOptions options)
        {
            return new DbContext(CreateServiceProvider(customServices), options);
        }

        public DbContext CreateContext(IServiceCollection customServices)
        {
            return new DbContext(CreateServiceProvider(customServices), CreateOptions());
        }

        public IServiceProvider CreateContextServices(IServiceProvider serviceProvider, IModel model)
        {
            return ((IAccessor<IServiceProvider>)CreateContext(serviceProvider, model)).Service;
        }

        public IServiceProvider CreateContextServices(IServiceProvider serviceProvider, DbContextOptions options)
        {
            return ((IAccessor<IServiceProvider>)CreateContext(serviceProvider, options)).Service;
        }

        public IServiceProvider CreateContextServices(IServiceProvider serviceProvider)
        {
            return ((IAccessor<IServiceProvider>)CreateContext(serviceProvider)).Service;
        }

        public IServiceProvider CreateContextServices(IModel model)
        {
            return ((IAccessor<IServiceProvider>)CreateContext(model)).Service;
        }

        public IServiceProvider CreateContextServices(DbContextOptions options)
        {
            return ((IAccessor<IServiceProvider>)CreateContext(options)).Service;
        }

        public IServiceProvider CreateContextServices()
        {
            return ((IAccessor<IServiceProvider>)CreateContext()).Service;
        }

        public IServiceProvider CreateContextServices(IServiceCollection customServices, IModel model)
        {
            return ((IAccessor<IServiceProvider>)CreateContext(customServices, model)).Service;
        }

        public IServiceProvider CreateContextServices(IServiceCollection customServices, DbContextOptions options)
        {
            return ((IAccessor<IServiceProvider>)CreateContext(customServices, options)).Service;
        }

        public IServiceProvider CreateContextServices(IServiceCollection customServices)
        {
            return ((IAccessor<IServiceProvider>)CreateContext(customServices)).Service;
        }

        public Model BuildModelFor<TEntity>() where TEntity : class
        {
            var builder = CreateConventionBuilder();
            builder.Entity<TEntity>();
            return builder.Model;
        }

        public ModelBuilder CreateConventionBuilder(Model model = null)
        {
            return new ModelBuilderFactory().CreateConventionBuilder(model ?? new Model());
        }

        public InternalEntityEntry CreateInternalEntry<TEntity>(
            IModel model, EntityState entityState = EntityState.Detached, TEntity entity = null)
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

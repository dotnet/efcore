// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Framework.DependencyInjection;
using Xunit;

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
            var optionsBuilder = new DbContextOptionsBuilder();
            UseProviderOptions(optionsBuilder.UseModel(model));

            return optionsBuilder.Options;
        }

        public DbContextOptions CreateOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            UseProviderOptions(optionsBuilder);

            return optionsBuilder.Options;
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

        protected virtual void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
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
                .GetRequiredService<IStateManager>()
                .GetOrCreateEntry(entity ?? new TEntity());

            entry.SetEntityState(entityState);

            return entry;
        }

        public static int AssertResults<T>(
            IList<T> expected,
            IList<T> actual,
            bool assertOrder,
            Action<IList<T>, IList<T>> asserter = null)
        {
            Assert.Equal(expected.Count, actual.Count);

            if (asserter != null)
            {
                asserter(expected, actual);
            }
            else
            {
                if (assertOrder)
                {
                    Assert.Equal(expected, actual);
                }
                else
                {
                    foreach (var expectedItem in expected)
                    {
                        Assert.True(
                            actual.Contains(expectedItem),
                            $"\r\nExpected item: [{expectedItem}] not found in results: [{string.Join(", ", actual.Take(10))}]...");
                    }
                }
            }
            return actual.Count;
        }
    }
}

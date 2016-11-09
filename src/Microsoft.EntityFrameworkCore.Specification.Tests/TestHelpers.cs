// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public class TestHelpers
    {
        protected TestHelpers()
        {
        }

        public static TestHelpers Instance { get; } = new TestHelpers();

        public DbContextOptions CreateOptions(IModel model, IServiceProvider serviceProvider = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider);

            UseProviderOptions(optionsBuilder.UseModel(model));

            return optionsBuilder.Options;
        }

        public DbContextOptions CreateOptions(IServiceProvider serviceProvider = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider);

            UseProviderOptions(optionsBuilder);

            return optionsBuilder.Options;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection customServices = null)
            => CreateServiceProvider(customServices, AddProviderServices);

        private IServiceProvider CreateServiceProvider(
            IServiceCollection customServices,
            Func<IServiceCollection, IServiceCollection> addProviderServices)
        {
            var services = new ServiceCollection();
            addProviderServices(services);

            if (customServices != null)
            {
                foreach (var service in customServices)
                {
                    services.Add(service);
                }
            }

            return services.BuildServiceProvider();
        }

        public virtual IServiceCollection AddProviderServices(IServiceCollection services) => services.AddEntityFrameworkInMemoryDatabase();

        protected virtual void UseProviderOptions(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase();

        public DbContext CreateContext(IServiceProvider serviceProvider, IModel model)
            => new DbContext(CreateOptions(model, serviceProvider));

        public DbContext CreateContext(IServiceProvider serviceProvider, DbContextOptions options)
            => new DbContext(new DbContextOptionsBuilder(options).UseInternalServiceProvider(serviceProvider).Options);

        public DbContext CreateContext(IServiceProvider serviceProvider)
            => new DbContext(CreateOptions(serviceProvider));

        public DbContext CreateContext(IModel model)
            => new DbContext(CreateOptions(model, CreateServiceProvider()));

        public DbContext CreateContext(DbContextOptions options)
            => new DbContext(new DbContextOptionsBuilder(options).UseInternalServiceProvider(CreateServiceProvider()).Options);

        public DbContext CreateContext()
            => new DbContext(CreateOptions(CreateServiceProvider()));

        public DbContext CreateContext(IServiceCollection customServices, IModel model)
            => new DbContext(CreateOptions(model, CreateServiceProvider(customServices)));

        public DbContext CreateContext(IServiceCollection customServices, DbContextOptions options)
            => new DbContext(new DbContextOptionsBuilder(options).UseInternalServiceProvider(CreateServiceProvider(customServices)).Options);

        public DbContext CreateContext(IServiceCollection customServices)
            => new DbContext(CreateOptions(CreateServiceProvider(customServices)));

        public IServiceProvider CreateContextServices(IServiceProvider serviceProvider, IModel model)
            => ((IInfrastructure<IServiceProvider>)CreateContext(serviceProvider, model)).Instance;

        public IServiceProvider CreateContextServices(IServiceProvider serviceProvider, DbContextOptions options)
            => ((IInfrastructure<IServiceProvider>)CreateContext(serviceProvider, options)).Instance;

        public IServiceProvider CreateContextServices(IServiceProvider serviceProvider) => ((IInfrastructure<IServiceProvider>)CreateContext(serviceProvider)).Instance;

        public IServiceProvider CreateContextServices(IModel model)
            => ((IInfrastructure<IServiceProvider>)CreateContext(model)).Instance;

        public IServiceProvider CreateContextServices(DbContextOptions options)
            => ((IInfrastructure<IServiceProvider>)CreateContext(options)).Instance;

        public IServiceProvider CreateContextServices()
            => ((IInfrastructure<IServiceProvider>)CreateContext()).Instance;

        public IServiceProvider CreateContextServices(IServiceCollection customServices, IModel model)
            => ((IInfrastructure<IServiceProvider>)CreateContext(customServices, model)).Instance;

        public IServiceProvider CreateContextServices(IServiceCollection customServices, DbContextOptions options)
            => ((IInfrastructure<IServiceProvider>)CreateContext(customServices, options)).Instance;

        public IServiceProvider CreateContextServices(IServiceCollection customServices)
            => ((IInfrastructure<IServiceProvider>)CreateContext(customServices)).Instance;

        public IMutableModel BuildModelFor<TEntity>() where TEntity : class
        {
            var builder = CreateConventionBuilder();
            builder.Entity<TEntity>();
            return builder.Model;
        }

        public ModelBuilder CreateConventionBuilder()
        {
            var contextServices = CreateContextServices();

            var conventionSetBuilder = contextServices.GetRequiredService<IDatabaseProviderServices>().ConventionSetBuilder;
            var conventionSet = contextServices.GetRequiredService<ICoreConventionSetBuilder>().CreateConventionSet();
            conventionSet = conventionSetBuilder == null
                ? conventionSet
                : conventionSetBuilder.AddConventions(conventionSet);
            return new ModelBuilder(conventionSet);
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

        public static int AssertResults<T>(
            IList<T> expected,
            IList<T> actual,
            Func<T, object> elementSorter,
            Action<T, T> elementAsserter,
            bool verifyOrdered)
        {
            Assert.Equal(expected.Count, actual.Count);

            elementAsserter = elementAsserter ?? Assert.Equal;
            if (!verifyOrdered)
            {
                expected = expected.OrderBy(elementSorter).ToList();
                actual = actual.OrderBy(elementSorter).ToList();
            }

            for (var i = 0; i < expected.Count; i++)
            {
                elementAsserter(expected[i], actual[i]);
            }

            return actual.Count;
        }

        public static int AssertResults<T>(
            IList<T> expected,
            IList<T> actual,
            Func<T, T> elementSorter,
            Action<T, T> elementAsserter,
            bool verifyOrdered)
            where T : struct
        {
            Assert.Equal(expected.Count, actual.Count);

            elementAsserter = elementAsserter ?? Assert.Equal;
            if (!verifyOrdered)
            { 
                expected = expected.OrderBy(elementSorter).ToList();
                actual = actual.OrderBy(elementSorter).ToList();
            }

            for (var i = 0; i < expected.Count; i++)
            {
                elementAsserter(expected[i], actual[i]);
            }

            return actual.Count;
        }

        public static int AssertResultsNullable<T>(
            IList<T?> expected,
            IList<T?> actual,
            Func<T?, T?> elementSorter,
            Action<T?, T?> elementAsserter,
            bool verifyOrdered)
            where T : struct
        {
            Assert.Equal(expected.Count, actual.Count);

            elementAsserter = elementAsserter ?? Assert.Equal;
            if (!verifyOrdered)
            {
                expected = expected.OrderBy(elementSorter).ToList();
                actual = actual.OrderBy(elementSorter).ToList();
            }

            for (var i = 0; i < expected.Count; i++)
            {
                elementAsserter(expected[i], actual[i]);
            }

            return actual.Count;
        }
    }
}

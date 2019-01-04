// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public abstract class TestHelpers
    {
        /// <summary>
        ///     Tests that calling the 'With' method for each constructor-injected service creates a clone
        ///     of TDependencies with only that service replaced.
        /// </summary>
        public void TestDependenciesClone<TDependencies>(params string[] ignoreProperties)
        {
            var customServices = new ServiceCollection()
                .AddScoped<IDbContextOptions>(CreateOptions)
                .AddScoped<ICurrentDbContext, FakeCurrentDbContext>()
                .AddScoped<IModel, Model>();

            var services1 = CreateServiceProvider(customServices).CreateScope().ServiceProvider;
            var services2 = CreateServiceProvider(customServices).CreateScope().ServiceProvider;

            var dependencies = services1.GetService<TDependencies>();

            var serviceProperties = typeof(TDependencies).GetTypeInfo()
                .DeclaredProperties
                .Where(p => !ignoreProperties.Contains(p.Name))
                .ToList();

            var obsoleteTypes = serviceProperties
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(ObsoleteAttribute)))
                .Select(p => p.PropertyType)
                .ToList();

            serviceProperties = serviceProperties.Where(p => !obsoleteTypes.Contains(p.PropertyType)).ToList();

            var constructor = typeof(TDependencies).GetTypeInfo().DeclaredConstructors.OrderByDescending(c => c.GetParameters().Length).First();
            var constructorParameters = constructor.GetParameters().Where(p => !obsoleteTypes.Contains(p.ParameterType)).ToList();

            Assert.Equal(constructorParameters.Count, serviceProperties.Count);

            foreach (var serviceType in constructorParameters.Where(p => !ignoreProperties.Contains(p.Name)).Select(p => p.ParameterType))
            {
                var withMethod = typeof(TDependencies).GetTypeInfo().DeclaredMethods
                    .Single(
                        m => m.CustomAttributes.All(a => a.AttributeType != typeof(ObsoleteAttribute))
                             && m.Name == "With"
                             && m.GetParameters()[0].ParameterType == serviceType);

                var clone = withMethod.Invoke(dependencies, new[] { services2.GetService(serviceType) });

                foreach (var property in serviceProperties)
                {
                    if (property.PropertyType == serviceType)
                    {
                        Assert.NotSame(property.GetValue(clone), property.GetValue(dependencies));
                    }
                    else
                    {
                        Assert.Same(property.GetValue(clone), property.GetValue(dependencies));
                    }
                }
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class FakeCurrentDbContext : ICurrentDbContext
        {
            // ReSharper disable once UnassignedGetOnlyAutoProperty
            public DbContext Context { get; }
        }

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

        private static IServiceProvider CreateServiceProvider(
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

        public abstract IServiceCollection AddProviderServices(IServiceCollection services);

        public DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
            UseProviderOptions(optionsBuilder);
            return optionsBuilder;
        }

        protected abstract void UseProviderOptions(DbContextOptionsBuilder optionsBuilder);

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

        public IMutableModel BuildModelFor<TEntity>()
            where TEntity : class
        {
            var builder = CreateConventionBuilder();
            builder.Entity<TEntity>();
            return builder.Model;
        }

        public ModelBuilder CreateConventionBuilder(bool skipValidation = false)
        {
            var contextServices = CreateContextServices();

            var conventionSet = contextServices.GetRequiredService<ICoreConventionSetBuilder>().CreateConventionSet();
            conventionSet = new CompositeConventionSetBuilder(
                contextServices.GetRequiredService<IEnumerable<IConventionSetBuilder>>().ToList())
                .AddConventions(conventionSet);

            if (!skipValidation)
            {
                conventionSet.ModelBuiltConventions.Add(new ValidatingConvention(contextServices.GetService<IModelValidator>()));
            }

            return new ModelBuilder(conventionSet);
        }

        public ModelBuilder CreateConventionBuilder(
            DiagnosticsLogger<DbLoggerCategory.Model> modelLogger,
            DiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger)
        {
            var contextServices = CreateContextServices();

            var conventionSet = new CoreConventionSetBuilder(
                            contextServices.GetRequiredService<CoreConventionSetBuilderDependencies>().With(modelLogger))
                            .CreateConventionSet();
            conventionSet = new CompositeConventionSetBuilder(
                    contextServices.GetRequiredService<IEnumerable<IConventionSetBuilder>>().ToList())
                .AddConventions(conventionSet);

            conventionSet.ModelBuiltConventions.Add(new ValidatingConvention(CreateModelValidator(modelLogger, validationLogger)));

            return new ModelBuilder(conventionSet);
        }

        public virtual IModelValidator CreateModelValidator(
            DiagnosticsLogger<DbLoggerCategory.Model> modelLogger,
            DiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger)
            => new ModelValidator(new ModelValidatorDependencies(validationLogger, modelLogger));

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

            if (elementSorter == null
                && !verifyOrdered)
            {
                if (ShouldPerformUnsortedVerification(expected))
                {
                    if (elementAsserter != null)
                    {
                        throw new InvalidOperationException("Elemenent asserter will not be used because results are not properly ordered - either remove asserter from the AssertQuery, add element sorter or set assertOrder to 'true'.");
                    }

                    return AssertResults(expected, actual, assertOrder: false);
                }
            }

            elementSorter = elementSorter ?? (e => e);
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
        {
            Assert.Equal(expected.Count, actual.Count);

            if (elementSorter == null
                && !verifyOrdered)
            {
                if (ShouldPerformUnsortedVerification(expected))
                {
                    if (elementAsserter != null)
                    {
                        throw new InvalidOperationException("Elemenent asserter will not be used because results are not properly ordered - either remove asserter from the AssertQuery, add element sorter or set assertOrder to 'true'.");
                    }

                    return AssertResults(expected, actual, assertOrder: false);
                }
            }

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

            if (elementSorter == null
                && !verifyOrdered)
            {
                if (ShouldPerformUnsortedVerification(expected))
                {
                    if (elementAsserter != null)
                    {
                        throw new InvalidOperationException("Elemenent asserter will not be used because results are not properly ordered - either remove asserter from the AssertQuery, add element sorter or set assertOrder to 'true'.");
                    }

                    return AssertResults(expected, actual, assertOrder: false);
                }
            }

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

        private static bool ShouldPerformUnsortedVerification<T>(IList<T> expected)
        {
            if (expected.Count > 1)
            {
                var nonNullElement = expected.FirstOrDefault(e => e != null);

                return nonNullElement != null && nonNullElement.GetType().GetInterface(nameof(IComparable)) == null;
            }

            return false;
        }

        public static void ExecuteWithStrategyInTransaction<TContext>(
            Func<TContext> createContext,
            Action<DatabaseFacade, IDbContextTransaction> useTransaction,
            Action<TContext> testOperation,
            Action<TContext> nestedTestOperation1 = null,
            Action<TContext> nestedTestOperation2 = null,
            Action<TContext> nestedTestOperation3 = null)
            where TContext : DbContext
        {
            using (var c = createContext())
            {
                c.Database.CreateExecutionStrategy().Execute(
                    c, context =>
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            using (var innerContext = createContext())
                            {
                                useTransaction(innerContext.Database, transaction);
                                testOperation(innerContext);
                            }

                            if (nestedTestOperation1 == null)
                            {
                                return;
                            }

                            using (var innerContext1 = createContext())
                            {
                                useTransaction(innerContext1.Database, transaction);
                                nestedTestOperation1(innerContext1);
                            }

                            if (nestedTestOperation2 == null)
                            {
                                return;
                            }

                            using (var innerContext2 = createContext())
                            {
                                useTransaction(innerContext2.Database, transaction);
                                nestedTestOperation2(innerContext2);
                            }

                            if (nestedTestOperation3 == null)
                            {
                                return;
                            }

                            using (var innerContext3 = createContext())
                            {
                                useTransaction(innerContext3.Database, transaction);
                                nestedTestOperation3(innerContext3);
                            }
                        }
                    });
            }
        }

        public static async Task ExecuteWithStrategyInTransactionAsync<TContext>(
            Func<TContext> createContext,
            Action<DatabaseFacade, IDbContextTransaction> useTransaction,
            Func<TContext, Task> testOperation,
            Func<TContext, Task> nestedTestOperation1 = null,
            Func<TContext, Task> nestedTestOperation2 = null,
            Func<TContext, Task> nestedTestOperation3 = null)
            where TContext : DbContext
        {
            using (var c = createContext())
            {
                await c.Database.CreateExecutionStrategy().ExecuteAsync(
                    c, async context =>
                    {
                        using (var transaction = await context.Database.BeginTransactionAsync())
                        {
                            using (var innerContext = createContext())
                            {
                                useTransaction(innerContext.Database, transaction);
                                await testOperation(innerContext);
                            }

                            if (nestedTestOperation1 == null)
                            {
                                return;
                            }

                            using (var innerContext1 = createContext())
                            {
                                useTransaction(innerContext1.Database, transaction);
                                await nestedTestOperation1(innerContext1);
                            }

                            if (nestedTestOperation2 == null)
                            {
                                return;
                            }

                            using (var innerContext2 = createContext())
                            {
                                useTransaction(innerContext2.Database, transaction);
                                await nestedTestOperation2(innerContext2);
                            }

                            if (nestedTestOperation3 == null)
                            {
                                return;
                            }

                            using (var innerContext3 = createContext())
                            {
                                useTransaction(innerContext3.Database, transaction);
                                await nestedTestOperation3(innerContext3);
                            }
                        }
                    });
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ContextConfigurationTest
    {
        [Fact]
        public void Requesting_a_singleton_always_returns_same_instance()
        {
            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var contextServices1 = InMemoryTestHelpers.Instance.CreateContextServices(provider);
            var contextServices2 = InMemoryTestHelpers.Instance.CreateContextServices(provider);

            Assert.Same(contextServices1.GetRequiredService<IDbSetSource>(), contextServices2.GetRequiredService<IDbSetSource>());
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_same_instance_in_scope()
        {
            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(provider);

            Assert.Same(contextServices.GetRequiredService<IStateManager>(), contextServices.GetRequiredService<IStateManager>());
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_a_different_instance_in_a_different_scope()
        {
            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var contextServices1 = InMemoryTestHelpers.Instance.CreateContextServices(provider);
            var contextServices2 = InMemoryTestHelpers.Instance.CreateContextServices(provider);

            Assert.NotSame(contextServices1.GetRequiredService<IStateManager>(), contextServices2.GetRequiredService<IStateManager>());
        }

        [Fact]
        public void Scoped_provider_services_can_be_obtained_from_configuration()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

            IDatabase database;
            IDatabaseCreator creator;

            using (var context = new GiddyupContext(serviceProvider))
            {
                database = context.GetService<IDatabase>();
                creator = context.GetService<IDatabaseCreator>();

                Assert.Same(database, context.GetService<IDatabase>());
                Assert.Same(creator, context.GetService<IDatabaseCreator>());
            }

            using (var context = new GiddyupContext(serviceProvider))
            {
                Assert.NotSame(database, context.GetService<IDatabase>());
                Assert.NotSame(creator, context.GetService<IDatabaseCreator>());
            }
        }

        [Fact]
        public void Scoped_provider_services_can_be_obtained_from_configuration_with_implicit_service_provider()
        {
            IDatabase database;
            IDatabaseCreator creator;

            using (var context = new GiddyupContext())
            {
                database = context.GetService<IDatabase>();
                creator = context.GetService<IDatabaseCreator>();

                Assert.Same(database, context.GetService<IDatabase>());
                Assert.Same(creator, context.GetService<IDatabaseCreator>());
            }

            using (var context = new GiddyupContext())
            {
                Assert.NotSame(database, context.GetService<IDatabase>());
                Assert.NotSame(creator, context.GetService<IDatabaseCreator>());
            }
        }

        private class GiddyupContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public GiddyupContext()
            {
            }

            public GiddyupContext([NotNull] IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .UseInternalServiceProvider(_serviceProvider);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ContextConfigurationTest
    {
        [Fact]
        public void Requesting_a_singleton_always_returns_same_instance()
        {
            var provider = TestHelpers.Instance.CreateServiceProvider();
            var contextServices1 = TestHelpers.Instance.CreateContextServices(provider);
            var contextServices2 = TestHelpers.Instance.CreateContextServices(provider);

            Assert.Same(contextServices1.GetRequiredService<IMemberMapper>(), contextServices2.GetRequiredService<IMemberMapper>());
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_same_instance_in_scope()
        {
            var provider = TestHelpers.Instance.CreateServiceProvider();
            var contextServices = TestHelpers.Instance.CreateContextServices(provider);

            Assert.Same(contextServices.GetRequiredService<IStateManager>(), contextServices.GetRequiredService<IStateManager>());
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_a_different_instance_in_a_different_scope()
        {
            var provider = TestHelpers.Instance.CreateServiceProvider();
            var contextServices1 = TestHelpers.Instance.CreateContextServices(provider);
            var contextServices2 = TestHelpers.Instance.CreateContextServices(provider);

            Assert.NotSame(contextServices1.GetRequiredService<IStateManager>(), contextServices2.GetRequiredService<IStateManager>());
        }

        [Fact]
        public void Scoped_provider_services_can_be_obtained_from_configuration()
        {
            var serviceProvider = TestHelpers.Instance.CreateServiceProvider();

            IDatabase database;
            IDatabaseCreator creator;
            IDatabaseConnection connection;

            using (var context = new DbContext(serviceProvider))
            {
                database = context.GetService<IDatabase>();
                creator = context.GetService<IDatabaseCreator>();
                connection = context.GetService<IDatabaseConnection>();

                Assert.Same(database, context.GetService<IDatabase>());
                Assert.Same(creator, context.GetService<IDatabaseCreator>());
                Assert.Same(connection, context.GetService<IDatabaseConnection>());
            }

            using (var context = new DbContext(serviceProvider))
            {
                Assert.NotSame(database, context.GetService<IDatabase>());
                Assert.NotSame(creator, context.GetService<IDatabaseCreator>());
                Assert.NotSame(connection, context.GetService<IDatabaseConnection>());
            }
        }

        [Fact]
        public void Scoped_provider_services_can_be_obtained_from_configuration_with_implicit_service_provider()
        {
            IDatabase database;
            IDatabaseCreator creator;
            IDatabaseConnection connection;

            using (var context = new GiddyupContext())
            {
                database = context.GetService<IDatabase>();
                creator = context.GetService<IDatabaseCreator>();
                connection = context.GetService<IDatabaseConnection>();

                Assert.Same(database, context.GetService<IDatabase>());
                Assert.Same(creator, context.GetService<IDatabaseCreator>());
                Assert.Same(connection, context.GetService<IDatabaseConnection>());
            }

            using (var context = new GiddyupContext())
            {
                Assert.NotSame(database, context.GetService<IDatabase>());
                Assert.NotSame(creator, context.GetService<IDatabaseCreator>());
                Assert.NotSame(connection, context.GetService<IDatabaseConnection>());
            }
        }

        private class GiddyupContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase();
            }
        }
    }
}

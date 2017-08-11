// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class NamedConnectionStringResolverTest
    {
        [Fact]
        public void Throws_if_no_app_service_provider()
        {
            var resolver = new NamedConnectionStringResolver(new FakeOptions(null, false));

            Assert.Equal(
                RelationalStrings.NamedConnectionStringNotFound("foo"),
                Assert.Throws<InvalidOperationException>(
                    () => resolver.ResolveConnectionString("name=foo")).Message);
        }

        [Fact]
        public void Throws_if_no_IConfiguration()
        {
            var resolver = new NamedConnectionStringResolver(new FakeOptions(null));

            Assert.Equal(
                RelationalStrings.NamedConnectionStringNotFound("foo"),
                Assert.Throws<InvalidOperationException>(
                    () => resolver.ResolveConnectionString("name=foo")).Message);
        }

        [Fact]
        public void Throws_if_IConfiguration_does_not_contain_key()
        {
            var resolver = new NamedConnectionStringResolver(new FakeOptions(new ConfigurationBuilder().Build()));

            Assert.Equal(
                RelationalStrings.NamedConnectionStringNotFound("foo"),
                Assert.Throws<InvalidOperationException>(
                    () => resolver.ResolveConnectionString("name=foo")).Message);
        }

        [Fact]
        public void Returns_resolved_string_if_IConfiguration_contains_key()
        {
            var resolver = new NamedConnectionStringResolver(
                new FakeOptions(
                    new ConfigurationBuilder()
                        .AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                { "MyConnectuonString", "Conn1" },
                                { "ConnectionStrings:DefaultConnection", "Conn2" },
                                { "ConnectionStrings:MyConnectuonString", "Conn3" }
                            })
                        .Build()));

            Assert.Equal("Conn1", resolver.ResolveConnectionString("name=MyConnectuonString"));
            Assert.Equal("Conn2", resolver.ResolveConnectionString("name=ConnectionStrings:DefaultConnection"));
            Assert.Equal("Conn2", resolver.ResolveConnectionString("name=DefaultConnection"));
            Assert.Equal("Conn3", resolver.ResolveConnectionString("name=ConnectionStrings:MyConnectuonString"));

            Assert.Equal("Conn1", resolver.ResolveConnectionString("  NamE = MyConnectuonString   "));
        }

        [Fact]
        public void Returns_given_string_named_connection_string_doesnt_match_pattern()
        {
            var resolver = new NamedConnectionStringResolver(
                new FakeOptions(
                    new ConfigurationBuilder()
                        .AddInMemoryCollection(
                            new Dictionary<string, string>
                            {
                                { "Nope", "NoThanks" }
                            })
                        .Build()));

            Assert.Equal("name=Fox;DataSource=Jimony", resolver.ResolveConnectionString("name=Fox;DataSource=Jimony"));
            Assert.Equal("DataSource=Jimony", resolver.ResolveConnectionString("DataSource=Jimony"));
            Assert.Equal("Jimony", resolver.ResolveConnectionString("Jimony"));
        }

        private class FakeOptions : IDbContextOptions
        {
            private readonly IServiceProvider _serviceProvider;

            public FakeOptions(IConfiguration configuration, bool useServiceProvider = true)
            {
                if (useServiceProvider)
                {
                    var collection = new ServiceCollection();

                    if (configuration != null)
                    {
                        collection.AddSingleton(configuration);
                    }

                    _serviceProvider = collection.BuildServiceProvider();
                }
            }

            public IEnumerable<IDbContextOptionsExtension> Extensions => null;

            public TExtension FindExtension<TExtension>()
                where TExtension : class, IDbContextOptionsExtension
            {
                var coreOptionsExtension = new CoreOptionsExtension();

                if (_serviceProvider != null)
                {
                    coreOptionsExtension = coreOptionsExtension.WithApplicationServiceProvider(_serviceProvider);
                }

                return (TExtension)(object)coreOptionsExtension;
            }
        }
    }
}

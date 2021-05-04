// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class DesignTimeTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : DesignTimeTestBase<TFixture>.DesignTimeFixtureBase
    {
        protected TFixture Fixture { get; }

        protected DesignTimeTestBase(TFixture fixture)
            => Fixture = fixture;

        protected abstract Assembly ProviderAssembly { get; }

        [ConditionalFact]
        public void Can_get_reverse_engineering_services()
        {
            using var context = Fixture.CreateContext();
            var serviceCollection = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices();
            ((IDesignTimeServices)Activator.CreateInstance(
                ProviderAssembly.GetType(
                    ProviderAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>().TypeName,
                    throwOnError: true))!)
                .ConfigureDesignTimeServices(serviceCollection);
            using var services = serviceCollection.BuildServiceProvider();

            var reverseEngineerScaffolder = services.GetService<IReverseEngineerScaffolder>();

            Assert.NotNull(reverseEngineerScaffolder);
        }

        [ConditionalFact]
        public void Can_get_migrations_services()
        {
            using var context = Fixture.CreateContext();
            var serviceCollection = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                .AddDbContextDesignTimeServices(context);
            ((IDesignTimeServices)Activator.CreateInstance(
                ProviderAssembly.GetType(
                    ProviderAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>().TypeName,
                    throwOnError: true))!)
                .ConfigureDesignTimeServices(serviceCollection);
            using var services = serviceCollection.BuildServiceProvider();

            var migrationsScaffolder = services.GetService<IMigrationsScaffolder>();

            Assert.NotNull(migrationsScaffolder);
        }

        public abstract class DesignTimeFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName
                => "DesignTimeTest";
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class DbContextOptionsTest
    {
        [Fact]
        public void Warnings_can_be_configured()
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .ConfigureWarnings(c => c.Default(WarningBehavior.Throw));

            var warningConfiguration = optionsBuilder.Options.FindExtension<CoreOptionsExtension>().WarningsConfiguration;

            Assert.Equal(WarningBehavior.Throw, warningConfiguration.DefaultBehavior);
        }

        [Fact]
        public void Model_can_be_set_explicitly_in_options()
        {
            var model = new Model();

            var optionsBuilder = new DbContextOptionsBuilder().UseModel(model);

            Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
        }

        [Fact]
        public void Sensitive_data_logging_can_be_set_explicitly_in_options()
        {
            var model = new Model();

            var optionsBuilder = new DbContextOptionsBuilder().UseModel(model).EnableSensitiveDataLogging();

            Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
            Assert.True(optionsBuilder.Options.FindExtension<CoreOptionsExtension>().IsSensitiveDataLoggingEnabled);
        }

        [Fact]
        public void Extensions_can_be_added_to_options()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.Null(optionsBuilder.Options.FindExtension<FakeDbContextOptionsExtension1>());
            Assert.Empty(optionsBuilder.Options.Extensions);

            var extension1 = new FakeDbContextOptionsExtension1();
            var extension2 = new FakeDbContextOptionsExtension2();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension1);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension2);

            Assert.Equal(2, optionsBuilder.Options.Extensions.Count());
            Assert.Contains(extension1, optionsBuilder.Options.Extensions);
            Assert.Contains(extension2, optionsBuilder.Options.Extensions);

            Assert.Same(extension1, optionsBuilder.Options.FindExtension<FakeDbContextOptionsExtension1>());
            Assert.Same(extension2, optionsBuilder.Options.FindExtension<FakeDbContextOptionsExtension2>());
        }

        [Fact]
        public void Can_update_an_existing_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            var extension1 = new FakeDbContextOptionsExtension1
            {
                Something = "One "
            };
            var extension2 = new FakeDbContextOptionsExtension1
            {
                Something = "Two "
            };

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension1);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension2);

            Assert.Equal(1, optionsBuilder.Options.Extensions.Count());
            Assert.DoesNotContain(extension1, optionsBuilder.Options.Extensions);
            Assert.Contains(extension2, optionsBuilder.Options.Extensions);

            Assert.Same(extension2, optionsBuilder.Options.FindExtension<FakeDbContextOptionsExtension1>());
        }

        [Fact]
        public void IsConfigured_returns_true_if_any_provider_extensions_have_been_added()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(optionsBuilder.IsConfigured);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new FakeDbContextOptionsExtension2());

            Assert.True(optionsBuilder.IsConfigured);
        }

        [Fact]
        public void IsConfigured_returns_false_if_only_non_provider_extensions_have_been_added()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(optionsBuilder.IsConfigured);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new FakeDbContextOptionsExtension1());

            Assert.False(optionsBuilder.IsConfigured);
        }

        private class FakeDbContextOptionsExtension1 : IDbContextOptionsExtension
        {
            public string Something { get; set; }

            public virtual bool ApplyServices(IServiceCollection services) => false;

            public virtual long GetServiceProviderHashCode() => 0;

            public virtual void Validate(IDbContextOptions options)
            {
            }

            public virtual string LogFragment => "";
        }

        private class FakeDbContextOptionsExtension2 : IDbContextOptionsExtension
        {
            public virtual bool ApplyServices(IServiceCollection services) => true;

            public virtual long GetServiceProviderHashCode() => 0;

            public virtual void Validate(IDbContextOptions options)
            {
            }

            public virtual string LogFragment => "";
        }

        [Fact]
        public void UseModel_on_generic_builder_returns_generic_builder()
        {
            var model = new Model();

            var optionsBuilder = GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().UseModel(model));

            Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
        }

        [Fact]
        public void UseLoggerFactory_on_generic_builder_returns_generic_builder()
        {
            var loggerFactory = new LoggerFactory();

            var optionsBuilder = GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().UseLoggerFactory(loggerFactory));

            Assert.Same(loggerFactory, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().LoggerFactory);
        }

        [Fact]
        public void UseMemoryCache_on_generic_builder_returns_generic_builder()
        {
            var memoryCache = new FakeMemoryCache();

            var optionsBuilder = GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().UseMemoryCache(memoryCache));

            Assert.Same(memoryCache, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().MemoryCache);
        }

        private class FakeMemoryCache : IMemoryCache
        {
            public void Dispose() => throw new NotImplementedException();
            public bool TryGetValue(object key, out object value) => throw new NotImplementedException();
            public ICacheEntry CreateEntry(object key) => throw new NotImplementedException();
            public void Remove(object key) => throw new NotImplementedException();
        }

        [Fact]
        public void UseInternalServiceProvider_on_generic_builder_returns_generic_builder()
        {
            var serviceProvider = new FakeServiceProvider();

            var optionsBuilder = GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().UseInternalServiceProvider(serviceProvider));

            Assert.Same(serviceProvider, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().InternalServiceProvider);
        }

        private class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => throw new NotImplementedException();
        }

        [Fact]
        public void EnableSensitiveDataLogging_on_generic_builder_returns_generic_builder()
        {
            GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().EnableSensitiveDataLogging());
        }

        [Fact]
        public void EnableDetailedErrors_on_generic_builder_returns_generic_builder()
        {
            GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().EnableDetailedErrors());
        }

        [Fact]
        public void UseQueryTrackingBehavior_on_generic_builder_returns_generic_builder()
        {
            var optionsBuilder = GenericCheck(
                new DbContextOptionsBuilder<UnkoolContext>().UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            Assert.Equal(QueryTrackingBehavior.NoTracking, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().QueryTrackingBehavior);
        }

        [Fact]
        public void ConfigureWarnings_on_generic_builder_returns_generic_builder()
        {
            var optionsBuilder = GenericCheck(
                new DbContextOptionsBuilder<UnkoolContext>().ConfigureWarnings(c => c.Default(WarningBehavior.Throw)));

            var warningConfiguration = optionsBuilder.Options.FindExtension<CoreOptionsExtension>().WarningsConfiguration;

            Assert.Equal(WarningBehavior.Throw, warningConfiguration.DefaultBehavior);
        }

        private DbContextOptionsBuilder<UnkoolContext> GenericCheck(DbContextOptionsBuilder<UnkoolContext> optionsBuilder) => optionsBuilder;

        [Fact]
        public void Generic_builder_returns_generic_options()
        {
            var builder = new DbContextOptionsBuilder<UnkoolContext>();
            Assert.Same(((DbContextOptionsBuilder)builder).Options, GenericCheck(builder.Options));
        }

        private DbContextOptions<UnkoolContext> GenericCheck(DbContextOptions<UnkoolContext> options) => options;

        private class UnkoolContext : DbContext
        {
        }
    }
}

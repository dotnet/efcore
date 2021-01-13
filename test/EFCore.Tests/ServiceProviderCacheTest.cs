// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ServiceProviderCacheTest
    {
        [ConditionalFact]
        public void Returns_same_provider_for_same_type_of_configured_extensions()
        {
            var loggerFactory = new ListLoggerFactory();

            var config1 = CreateOptions<FakeDbContextOptionsExtension1>(loggerFactory);
            var config2 = CreateOptions<FakeDbContextOptionsExtension1>(loggerFactory);

            var cache = new ServiceProviderCache();

            Assert.Same(cache.GetOrAdd(config1, true), cache.GetOrAdd(config2, true));

            Assert.Single(loggerFactory.Log);

            Assert.Equal(
                CoreResources.LogServiceProviderCreated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(),
                loggerFactory.Log[0].Message);
        }

        [ConditionalFact]
        public void Returns_different_provider_for_different_type_of_configured_extensions()
        {
            var loggerFactory = new ListLoggerFactory();

            var config1 = CreateOptions<FakeDbContextOptionsExtension1>(loggerFactory);
            var config2 = CreateOptions<FakeDbContextOptionsExtension2>(loggerFactory);

            var cache = new ServiceProviderCache();

            var first = cache.GetOrAdd(config1, true);
            var second = cache.GetOrAdd(config2, true);

            Assert.NotSame(first, second);

            Assert.Equal(2, loggerFactory.Log.Count);

            Assert.Equal(
                CoreResources.LogServiceProviderCreated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(),
                loggerFactory.Log[0].Message);

            Assert.Equal(
                CoreResources.LogServiceProviderDebugInfo(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    string.Join(
                        ", ",
                        CoreStrings.ServiceProviderConfigRemoved("Fake1"),
                        CoreStrings.ServiceProviderConfigAdded("Fake2"))),
                loggerFactory.Log[1].Message);
        }

        [ConditionalFact]
        public void Returns_same_provider_for_same_type_of_configured_extensions_and_replaced_service_types()
        {
            var loggerFactory = new ListLoggerFactory();

            var config1 = CreateOptions<CoreOptionsExtension>(loggerFactory);
            config1 = config1.WithExtension(
                config1.FindExtension<CoreOptionsExtension>()
                    .WithReplacedService(typeof(object), typeof(Random)));

            var config2 = CreateOptions<CoreOptionsExtension>(loggerFactory);
            config2 = config2.WithExtension(
                config2.FindExtension<CoreOptionsExtension>()
                    .WithReplacedService(typeof(object), typeof(Random)));

            var cache = new ServiceProviderCache();

            Assert.Same(cache.GetOrAdd(config1, true), cache.GetOrAdd(config2, true));

            Assert.Single(loggerFactory.Log);

            Assert.Equal(
                CoreResources.LogServiceProviderCreated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(),
                loggerFactory.Log[0].Message);
        }

        [ConditionalFact]
        public void Returns_different_provider_for_different_replaced_service_types()
        {
            var loggerFactory = new ListLoggerFactory();

            var config1 = CreateOptions<CoreOptionsExtension>(loggerFactory);
            config1 = config1.WithExtension(
                config1.FindExtension<CoreOptionsExtension>()
                    .WithReplacedService(typeof(object), typeof(Random)));

            var config2 = CreateOptions<CoreOptionsExtension>(loggerFactory);
            config2 = config2.WithExtension(
                config2.FindExtension<CoreOptionsExtension>()
                    .WithReplacedService(typeof(object), typeof(string)));

            var cache = new ServiceProviderCache();

            var first = cache.GetOrAdd(config1, true);
            var second = cache.GetOrAdd(config2, true);

            Assert.NotSame(first, second);

            Assert.Equal(2, loggerFactory.Log.Count);

            Assert.Equal(
                CoreResources.LogServiceProviderCreated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(),
                loggerFactory.Log[0].Message);

            Assert.Equal(
                CoreResources.LogServiceProviderDebugInfo(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    CoreStrings.ServiceProviderConfigChanged("Core:ReplaceService:" + typeof(object).DisplayName())),
                loggerFactory.Log[1].Message);
        }

        [ConditionalFact]
        public void Different_ILoggerFactory_instances_does_not_trigger_new_internal_provider()
        {
            var config1 = CreateOptions<CoreOptionsExtension>(new ListLoggerFactory());

            var loggerFactory = new ListLoggerFactory();

            var config2 = CreateOptions<CoreOptionsExtension>(loggerFactory);

            var cache = new ServiceProviderCache();

            var first = cache.GetOrAdd(config1, true);
            var second = cache.GetOrAdd(config2, true);

            Assert.Same(first, second);
        }

        [ConditionalFact]
        public void Reports_debug_info_for_most_similar_existing_service_provider()
        {
            // Do this a bunch of times since in the past this exposed issues with cache collisions
            for (var i = 0; i < 1000; i++)
            {
                var loggerFactory = new ListLoggerFactory();

                var config1 = new DbContextOptionsBuilder(CreateOptions<CoreOptionsExtension>(loggerFactory))
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(w => w.Throw(CoreEventId.CascadeDelete))
                    .Options;

                var config2 = new DbContextOptionsBuilder(CreateOptions<CoreOptionsExtension>(loggerFactory))
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(w => w.Throw(CoreEventId.CascadeDeleteOrphan))
                    .Options;

                var config3 = new DbContextOptionsBuilder(CreateOptions<CoreOptionsExtension>(loggerFactory))
                    .EnableDetailedErrors()
                    .ConfigureWarnings(w => w.Throw(CoreEventId.CascadeDelete))
                    .Options;

                var config4 = new DbContextOptionsBuilder(CreateOptions<CoreOptionsExtension>(loggerFactory))
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(w => w.Throw(CoreEventId.ContextDisposed))
                    .Options;

                var cache = new ServiceProviderCache();

                var first = cache.GetOrAdd(config1, true);
                var second = cache.GetOrAdd(config2, true);
                var third = cache.GetOrAdd(config3, true);
                var forth = cache.GetOrAdd(config4, true);

                Assert.NotSame(first, second);
                Assert.NotSame(first, third);
                Assert.NotSame(first, forth);
                Assert.NotSame(second, third);
                Assert.NotSame(second, forth);
                Assert.NotSame(third, forth);

                Assert.Equal(4, loggerFactory.Log.Count);

                Assert.Equal(
                    CoreResources.LogServiceProviderCreated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(),
                    loggerFactory.Log[0].Message);

                Assert.Equal(
                    CoreResources.LogServiceProviderDebugInfo(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        CoreStrings.ServiceProviderConfigChanged("Core:ConfigureWarnings")),
                    loggerFactory.Log[1].Message);

                Assert.Equal(
                    CoreResources.LogServiceProviderDebugInfo(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        CoreStrings.ServiceProviderConfigChanged("Core:EnableSensitiveDataLogging")),
                    loggerFactory.Log[2].Message);

                Assert.Equal(
                    CoreResources.LogServiceProviderDebugInfo(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        string.Join(
                            ", ",
                            CoreStrings.ServiceProviderConfigChanged("Core:EnableDetailedErrors"),
                            CoreStrings.ServiceProviderConfigChanged("Core:ConfigureWarnings"))),
                    loggerFactory.Log[3].Message);
            }
        }

        private static DbContextOptions CreateOptions<TExtension>(ILoggerFactory loggerFactory)
            where TExtension : class, IDbContextOptionsExtension, new()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new TExtension());
            optionsBuilder.UseLoggerFactory(loggerFactory);
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            return optionsBuilder.Options;
        }

        private class FakeDbContextOptionsExtension1 : IDbContextOptionsExtension
        {
            private DbContextOptionsExtensionInfo _info;

            public string Something { get; set; }

            public DbContextOptionsExtensionInfo Info
                => _info ??= new ExtensionInfo(this);

            public virtual void ApplyServices(IServiceCollection services)
            {
            }

            public virtual void Validate(IDbContextOptions options)
            {
            }

            private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
            {
                public ExtensionInfo(IDbContextOptionsExtension extension)
                    : base(extension)
                {
                }

                public override bool IsDatabaseProvider
                    => false;

                public override long GetServiceProviderHashCode()
                    => 0;

                public override string LogFragment
                    => "";

                public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
                {
                    debugInfo["Fake1"] = "1";
                }
            }
        }

        private class FakeDbContextOptionsExtension2 : IDbContextOptionsExtension
        {
            private DbContextOptionsExtensionInfo _info;

            public DbContextOptionsExtensionInfo Info
                => _info ??= new ExtensionInfo(this);

            public virtual void ApplyServices(IServiceCollection services)
            {
            }

            public virtual void Validate(IDbContextOptions options)
            {
            }

            private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
            {
                public ExtensionInfo(IDbContextOptionsExtension extension)
                    : base(extension)
                {
                }

                public override bool IsDatabaseProvider
                    => false;

                public override long GetServiceProviderHashCode()
                    => 0;

                public override string LogFragment
                    => "";

                public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
                {
                    debugInfo["Fake2"] = "1";
                }
            }
        }
    }
}

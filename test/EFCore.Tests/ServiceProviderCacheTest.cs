// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore;

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
    public void Returns_different_provider_for_extensions_configured_in_different_order()
    {
        var loggerFactory = new ListLoggerFactory();

        var config1Log = new List<string>();
        var config1Builder = new DbContextOptionsBuilder();
        ((IDbContextOptionsBuilderInfrastructure)config1Builder)
            .AddOrUpdateExtension(new FakeDbContextOptionsExtension1(config1Log));
        ((IDbContextOptionsBuilderInfrastructure)config1Builder)
            .AddOrUpdateExtension(new FakeDbContextOptionsExtension2(config1Log));
        config1Builder.UseLoggerFactory(loggerFactory);
        config1Builder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        var config2Log = new List<string>();
        var config2Builder = new DbContextOptionsBuilder();
        ((IDbContextOptionsBuilderInfrastructure)config2Builder)
            .AddOrUpdateExtension(new FakeDbContextOptionsExtension2(config2Log));
        ((IDbContextOptionsBuilderInfrastructure)config2Builder)
            .AddOrUpdateExtension(new FakeDbContextOptionsExtension1(config2Log));
        config2Builder.UseLoggerFactory(loggerFactory);
        config2Builder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        var cache = new ServiceProviderCache();

        Assert.NotSame(cache.GetOrAdd(config1Builder.Options, true), cache.GetOrAdd(config2Builder.Options, true));

        Assert.Equal(2, loggerFactory.Log.Count);

        Assert.Equal(new[] { nameof(FakeDbContextOptionsExtension1), nameof(FakeDbContextOptionsExtension2) }, config1Log);
        Assert.Equal(new[] { nameof(FakeDbContextOptionsExtension2), nameof(FakeDbContextOptionsExtension1) }, config2Log);
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

    private class FakeDbContextOptionsExtension1(List<string> log) : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo _info;
        private readonly List<string> _log = log;

        public string Something { get; set; }

        public DbContextOptionsExtensionInfo Info
            => _info ??= new ExtensionInfo(this);

        public FakeDbContextOptionsExtension1()
            : this([])
        {
        }

        public virtual void ApplyServices(IServiceCollection services)
            => _log.Add(GetType().ShortDisplayName());

        public virtual void Validate(IDbContextOptions options)
        {
        }

        private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
        {
            public override bool IsDatabaseProvider
                => false;

            public override int GetServiceProviderHashCode()
                => 0;

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
                => true;

            public override string LogFragment
                => "";

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
                => debugInfo["Fake1"] = "1";
        }
    }

    private class FakeDbContextOptionsExtension2(List<string> log) : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo _info;
        private readonly List<string> _log = log;

        public DbContextOptionsExtensionInfo Info
            => _info ??= new ExtensionInfo(this);

        public FakeDbContextOptionsExtension2()
            : this([])
        {
        }

        public virtual void ApplyServices(IServiceCollection services)
            => _log.Add(GetType().ShortDisplayName());

        public virtual void Validate(IDbContextOptions options)
        {
        }

        private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
        {
            public override bool IsDatabaseProvider
                => false;

            public override int GetServiceProviderHashCode()
                => 0;

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
                => true;

            public override string LogFragment
                => "";

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
                => debugInfo["Fake2"] = "1";
        }
    }
}

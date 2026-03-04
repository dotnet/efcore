// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class DbContextOptionsTest
{
    [ConditionalFact]
    public void Warnings_can_be_configured()
    {
        var optionsBuilder = new DbContextOptionsBuilder()
            .ConfigureWarnings(c => c.Default(WarningBehavior.Throw));

        var warningConfiguration = optionsBuilder.Options.FindExtension<CoreOptionsExtension>().WarningsConfiguration;

        Assert.Equal(WarningBehavior.Throw, warningConfiguration.DefaultBehavior);
    }

    [ConditionalFact]
    public void Model_can_be_set_explicitly_in_options()
    {
        var model = new Model();

        var optionsBuilder = new DbContextOptionsBuilder().UseModel(model.FinalizeModel());

        Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
    }

    [ConditionalFact]
    public void Sensitive_data_logging_can_be_set_explicitly_in_options()
    {
        var model = new Model();

        var optionsBuilder = new DbContextOptionsBuilder().UseModel(model.FinalizeModel()).EnableSensitiveDataLogging();

        Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
        Assert.True(optionsBuilder.Options.FindExtension<CoreOptionsExtension>().IsSensitiveDataLoggingEnabled);
    }

    [ConditionalFact]
    public void Can_find_extension_with_GetExtension()
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        Assert.Equal(
            CoreStrings.OptionsExtensionNotFound(nameof(FakeDbContextOptionsExtension1)),
            Assert.Throws<InvalidOperationException>(
                () => optionsBuilder.Options.GetExtension<FakeDbContextOptionsExtension1>()).Message);

        var extension = new FakeDbContextOptionsExtension1();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        Assert.Same(extension, optionsBuilder.Options.GetExtension<FakeDbContextOptionsExtension1>());
    }

    [ConditionalFact]
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

    [ConditionalFact]
    public void Can_update_an_existing_extension()
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        var extension1 = new FakeDbContextOptionsExtension1 { Something = "One " };
        var extension2 = new FakeDbContextOptionsExtension1 { Something = "Two " };

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension1);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension2);

        Assert.Single(optionsBuilder.Options.Extensions);
        Assert.DoesNotContain(extension1, optionsBuilder.Options.Extensions);
        Assert.Contains(extension2, optionsBuilder.Options.Extensions);

        Assert.Same(extension2, optionsBuilder.Options.FindExtension<FakeDbContextOptionsExtension1>());
    }

    [ConditionalFact]
    public void IsConfigured_returns_true_if_any_provider_extensions_have_been_added()
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        Assert.False(optionsBuilder.IsConfigured);

        var extension = new FakeDbContextOptionsExtension2();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        Assert.True(optionsBuilder.IsConfigured);
        Assert.False(extension.AppliedServices);
    }

    [ConditionalFact]
    public void IsConfigured_returns_false_if_only_non_provider_extensions_have_been_added()
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        Assert.False(optionsBuilder.IsConfigured);

        var extension = new FakeDbContextOptionsExtension1();
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        Assert.False(optionsBuilder.IsConfigured);
        Assert.False(extension.AppliedServices);
    }

    private class FakeDbContextOptionsExtension1 : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo _info;

        public string Something { get; set; }

        public DbContextOptionsExtensionInfo Info
            => _info ??= new ExtensionInfo(this);

        public bool AppliedServices { get; private set; }

        public virtual void ApplyServices(IServiceCollection services)
            => AppliedServices = true;

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
            {
            }
        }
    }

    private class FakeDbContextOptionsExtension2 : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo _info;

        public DbContextOptionsExtensionInfo Info
            => _info ??= new ExtensionInfo(this);

        public bool AppliedServices { get; private set; }

        public virtual void ApplyServices(IServiceCollection services)
            => AppliedServices = true;

        public virtual void Validate(IDbContextOptions options)
        {
        }

        private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
        {
            public override bool IsDatabaseProvider
                => true;

            public override int GetServiceProviderHashCode()
                => 0;

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
                => true;

            public override string LogFragment
                => "";

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
            }
        }
    }

    [ConditionalFact]
    public void UseModel_on_generic_builder_returns_generic_builder()
    {
        var model = new Model();

        var optionsBuilder = GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().UseModel(model));

        Assert.Same(model, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().Model);
    }

    [ConditionalFact]
    public void UseLoggerFactory_on_generic_builder_returns_generic_builder()
    {
        var loggerFactory = new LoggerFactory();

        var optionsBuilder = GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().UseLoggerFactory(loggerFactory));

        Assert.Same(loggerFactory, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().LoggerFactory);
    }

    [ConditionalFact]
    public void UseMemoryCache_on_generic_builder_returns_generic_builder()
    {
        var memoryCache = new FakeMemoryCache();

        var optionsBuilder = GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().UseMemoryCache(memoryCache));

        Assert.Same(memoryCache, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().MemoryCache);
    }

    private class FakeMemoryCache : IMemoryCache
    {
        public void Dispose()
            => throw new NotImplementedException();

        public bool TryGetValue(object key, out object value)
            => throw new NotImplementedException();

        public ICacheEntry CreateEntry(object key)
            => throw new NotImplementedException();

        public void Remove(object key)
            => throw new NotImplementedException();
    }

    [ConditionalFact]
    public void UseInternalServiceProvider_on_generic_builder_returns_generic_builder()
    {
        var serviceProvider = new FakeServiceProvider();

        var optionsBuilder = GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().UseInternalServiceProvider(serviceProvider));

        Assert.Same(serviceProvider, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().InternalServiceProvider);
    }

    private class FakeServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
            => throw new NotImplementedException();
    }

    [ConditionalFact]
    public void EnableSensitiveDataLogging_on_generic_builder_returns_generic_builder()
        => GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().EnableSensitiveDataLogging());

    [ConditionalFact]
    public void EnableDetailedErrors_on_generic_builder_returns_generic_builder()
        => GenericCheck(new DbContextOptionsBuilder<UnkoolContext>().EnableDetailedErrors());

    [ConditionalFact]
    public void UseQueryTrackingBehavior_on_generic_builder_returns_generic_builder()
    {
        var optionsBuilder = GenericCheck(
            new DbContextOptionsBuilder<UnkoolContext>().UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        Assert.Equal(
            QueryTrackingBehavior.NoTracking, optionsBuilder.Options.FindExtension<CoreOptionsExtension>().QueryTrackingBehavior);
    }

    [ConditionalFact]
    public void ConfigureWarnings_on_generic_builder_returns_generic_builder()
    {
        var optionsBuilder = GenericCheck(
            new DbContextOptionsBuilder<UnkoolContext>().ConfigureWarnings(c => c.Default(WarningBehavior.Throw)));

        var warningConfiguration = optionsBuilder.Options.FindExtension<CoreOptionsExtension>().WarningsConfiguration;

        Assert.Equal(WarningBehavior.Throw, warningConfiguration.DefaultBehavior);
    }

    private DbContextOptionsBuilder<UnkoolContext> GenericCheck(DbContextOptionsBuilder<UnkoolContext> optionsBuilder)
        => optionsBuilder;

    [ConditionalFact]
    public void Generic_builder_returns_generic_options()
    {
        var builder = new DbContextOptionsBuilder<UnkoolContext>();
        Assert.Same(((DbContextOptionsBuilder)builder).Options, GenericCheck(builder.Options));
    }

    private DbContextOptions<UnkoolContext> GenericCheck(DbContextOptions<UnkoolContext> options)
        => options;

    private class UnkoolContext : DbContext;
}

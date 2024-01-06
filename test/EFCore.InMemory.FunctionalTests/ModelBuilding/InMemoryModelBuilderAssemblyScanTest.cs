// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class InMemoryModelBuilderAssemblyScanTest : ModelBuilderTest
{
    private readonly Assembly _mockEntityTypeAssembly;

    public InMemoryModelBuilderAssemblyScanTest()
    {
        _mockEntityTypeAssembly = MockAssembly.Create(
            typeof(ScannerCustomerEntityConfiguration), typeof(ScannerCustomerEntityConfiguration2),
            typeof(AbstractCustomerEntityConfiguration), typeof(AbstractCustomerEntityConfigurationImpl));
    }

    [ConditionalFact]
    public void Should_scan_assemblies_for_entity_type_configurations()
    {
        var loggerFactory = new ListLoggerFactory();
        var logger = CreateModelLogger(loggerFactory);
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder(logger);
        builder.ApplyConfigurationsFromAssembly(_mockEntityTypeAssembly);

        var entityType = builder.Model.FindEntityType(typeof(ScannerCustomer));
        // ScannerCustomerEntityConfiguration called
        Assert.Equal(200, entityType.FindProperty(nameof(ScannerCustomer.FirstName)).GetMaxLength());
        // ScannerCustomerEntityConfiguration2 called
        Assert.Equal(1000, entityType.FindProperty(nameof(ScannerCustomer.LastName)).GetMaxLength());
        // AbstractCustomerEntityConfiguration not called
        Assert.Null(entityType.FindProperty(nameof(ScannerCustomer.MiddleName)).GetMaxLength());
        // AbstractCustomerEntityConfigurationImpl called
        Assert.Single(entityType.GetIndexes());

        Assert.Empty(loggerFactory.Log);
    }

    [ConditionalFact]
    public void Scan_reports_load_errors()
    {
        var types = new[]
        {
            typeof(ScannerCustomerEntityConfiguration),
            typeof(ScannerCustomerEntityConfiguration2),
            typeof(AbstractCustomerEntityConfiguration),
            typeof(AbstractCustomerEntityConfigurationImpl)
        };

        var assembly = MockAssembly.Create(
            types, null, new ReflectionTypeLoadException([types[1], types[2]], [new(), new()]));

        var loggerFactory = new ListLoggerFactory();
        var logger = CreateModelLogger(loggerFactory);
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder(logger);
        builder.ApplyConfigurationsFromAssembly(assembly);

        builder.Model.FindEntityType(typeof(ScannerCustomer));

        Assert.Equal(1, loggerFactory.Log.Count);

        var expectedMessage = CoreResources.LogTypeLoadingErrorWarning(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("A", "B");
        var actualMessage = loggerFactory.Log[0].Message;

        Assert.StartsWith(expectedMessage.Substring(0, 10), actualMessage);
        Assert.Contains(nameof(ReflectionTypeLoadException), actualMessage);
    }

    private static DiagnosticsLogger<DbLoggerCategory.Model> CreateModelLogger(ListLoggerFactory loggerFactory)
    {
        var options = new LoggingOptions();
        options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: false).Options);
        return new DiagnosticsLogger<DbLoggerCategory.Model>(
            loggerFactory,
            options,
            new DiagnosticListener("Fake"),
            new TestLoggingDefinitions(),
            new NullDbContextLogger());
    }

    [ConditionalFact]
    public void Should_support_filtering_for_entity_type_configurations()
    {
        var loggerFactory = new ListLoggerFactory();
        var logger = CreateModelLogger(loggerFactory);
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder(logger);
        builder.ApplyConfigurationsFromAssembly(
            _mockEntityTypeAssembly, type => type.Name == nameof(ScannerCustomerEntityConfiguration));

        var entityType = builder.Model.FindEntityType(typeof(ScannerCustomer));
        // ScannerCustomerEntityConfiguration called
        Assert.Equal(200, entityType.FindProperty(nameof(ScannerCustomer.FirstName)).GetMaxLength());
        // ScannerCustomerEntityConfiguration2 not called
        Assert.Null(entityType.FindProperty(nameof(ScannerCustomer.LastName)).GetMaxLength());
        // AbstractCustomerEntityConfiguration not called
        Assert.Null(entityType.FindProperty(nameof(ScannerCustomer.MiddleName)).GetMaxLength());
        // AbstractCustomerEntityConfigurationImpl not called
        Assert.Empty(entityType.GetIndexes());

        Assert.Empty(loggerFactory.Log);
    }

    [ConditionalFact]
    public void Should_skip_abstract_classes_for_entity_type_configurations()
    {
        var loggerFactory = new ListLoggerFactory();
        var logger = CreateModelLogger(loggerFactory);
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder(logger);
        builder.ApplyConfigurationsFromAssembly(
            _mockEntityTypeAssembly, type => type.Name == nameof(AbstractCustomerEntityConfiguration));

        var entityType = builder.Model.FindEntityType(typeof(ScannerCustomer));
        // No configuration should occur
        Assert.Null(entityType);

        var expectedMessage = CoreResources.LogNoEntityTypeConfigurationsWarning(
            new TestLogger<TestLoggingDefinitions>()).GenerateMessage(_mockEntityTypeAssembly.FullName);

        Assert.Equal(expectedMessage, loggerFactory.Log[0].Message);
    }

    [ConditionalFact]
    public void Should_log_when_no_entity_type_configurations_found()
    {
        var loggerFactory = new ListLoggerFactory();
        var logger = CreateModelLogger(loggerFactory);
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder(logger);
        builder.ApplyConfigurationsFromAssembly(typeof(Random).Assembly);

        Assert.Equal(1, loggerFactory.Log.Count);

        var expectedMessage = CoreResources.LogNoEntityTypeConfigurationsWarning(
            new TestLogger<TestLoggingDefinitions>()).GenerateMessage(typeof(Random).Assembly.FullName);

        Assert.Equal(expectedMessage, loggerFactory.Log[0].Message);
    }

    [ConditionalFact]
    public void Should_log_when_entity_type_configuration_has_no_parameterless_constructor()
    {
        var types = new[]
        {
            typeof(ScannerCustomerEntityConfiguration),
            typeof(ScannerCustomerEntityConfigurationNoConstructor),
            typeof(AbstractCustomerEntityConfiguration)
        };

        var assembly = MockAssembly.Create(types);

        var loggerFactory = new ListLoggerFactory();
        var logger = CreateModelLogger(loggerFactory);
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder(logger);
        builder.ApplyConfigurationsFromAssembly(assembly);

        Assert.Equal(1, loggerFactory.Log.Count);

        var expectedMessage = CoreResources.LogSkippedEntityTypeConfigurationWarning(
            new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
            "Microsoft.EntityFrameworkCore.ModelBuilding.InMemoryModelBuilderAssemblyScanTest+ScannerCustomerEntityConfigurationNoConstructor");

        Assert.Equal(expectedMessage, loggerFactory.Log[0].Message);
    }

    protected virtual ModelBuilder CreateModelBuilder()
        => InMemoryTestHelpers.Instance.CreateConventionBuilder();

    protected class ScannerCustomer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Address { get; set; }
        public int IndexedField { get; set; }
    }

    protected class ScannerCustomer2
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Address { get; set; }
        public int IndexedField { get; set; }
    }

#pragma warning disable CS9113 // Parameter '_' is unread
    private class ScannerCustomerEntityConfigurationNoConstructor(int _) : IEntityTypeConfiguration<ScannerCustomer>
#pragma warning restore CS9113
    {
        public void Configure(EntityTypeBuilder<ScannerCustomer> builder)
            => builder.Property(c => c.FirstName).HasMaxLength(200);
    }

    private class ScannerCustomerEntityConfiguration : IEntityTypeConfiguration<ScannerCustomer>
    {
        public void Configure(EntityTypeBuilder<ScannerCustomer> builder)
            => builder.Property(c => c.FirstName).HasMaxLength(200);
    }

    private class ScannerCustomerEntityConfiguration2 : IEntityTypeConfiguration<ScannerCustomer>
    {
        private ScannerCustomerEntityConfiguration2()
        {
        }

        public void Configure(EntityTypeBuilder<ScannerCustomer> builder)
            => builder.Property(c => c.LastName).HasMaxLength(1000);
    }

    private abstract class AbstractCustomerEntityConfiguration : IEntityTypeConfiguration<ScannerCustomer>
    {
        public virtual void Configure(EntityTypeBuilder<ScannerCustomer> builder)
            => builder.Property(c => c.MiddleName).HasMaxLength(500);
    }

    private class AbstractCustomerEntityConfigurationImpl : AbstractCustomerEntityConfiguration
    {
        public override void Configure(EntityTypeBuilder<ScannerCustomer> builder)
            => builder.HasIndex(c => c.IndexedField);
    }
}

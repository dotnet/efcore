// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class ModelSourceTest
{
    private readonly IServiceProvider _serviceProvider = new ServiceCollection()
        .AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true);

    [ConditionalFact]
    public void OnModelCreating_is_only_called_once()
    {
        const int threadCount = 5;

        var models = new IModel[threadCount];

        Parallel.For(
            0, threadCount,
            i =>
            {
                using var context = new SlowContext(_serviceProvider);
                models[i] = context.Model;
            });

        Assert.NotNull(models[0]);

        foreach (var model in models)
        {
            Assert.Same(models[0], model);
        }

        Assert.Equal(1, SlowContext.CallCount);
    }

    private class SlowContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public static int CallCount { get; private set; }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CallCount++;
            Thread.Sleep(200);
        }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(SlowContext));
    }

    [ConditionalFact]
    public void Adds_all_entities_based_on_all_distinct_entity_types_found()
    {
        var context = InMemoryTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddSingleton<IDbSetFinder, FakeSetFinder>());

        Assert.Equal(
            new[] { typeof(SetA).DisplayName(), typeof(SetB).DisplayName() },
            context.Model.GetEntityTypes().Select(e => e.Name).ToArray());
    }

    private class FakeModelValidator : IModelValidator
    {
        public void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
        }
    }

    private class FakeSetFinder : IDbSetFinder
    {
        public IReadOnlyList<DbSetProperty> FindSets(Type contextType)
            => new[]
            {
                new DbSetProperty("One", typeof(SetA), setter: null),
                new DbSetProperty("Two", typeof(SetB), setter: null),
                new DbSetProperty("Three", typeof(SetA), setter: null)
            };
    }

    private class JustAClass
    {
        public DbSet<Random> One { get; set; }
        protected DbSet<object> Two { get; set; }
        private DbSet<string> Three { get; set; }
        private DbSet<string> Four { get; set; }
    }

    private class SetA
    {
        public int Id { get; set; }
    }

    private class SetB
    {
        public int Id { get; set; }
    }

    [ConditionalFact]
    public void Caches_model_by_context_type()
    {
        var options = InMemoryTestHelpers.Instance.CreateOptions();
        var serviceProvider = new DbContext(options).GetInfrastructure();

        var modelSource = serviceProvider.GetRequiredService<IModelSource>();
        var testModelDependencies = serviceProvider.GetRequiredService<ModelCreationDependencies>();

        var model1 = modelSource.GetModel(new Context1(options), testModelDependencies, designTime: false);
        var model2 = modelSource.GetModel(new Context2(options), testModelDependencies, designTime: false);

        var designModel1 = modelSource.GetModel(new Context1(options), testModelDependencies, designTime: true);
        var designModel2 = modelSource.GetModel(new Context2(options), testModelDependencies, designTime: true);

        Assert.NotSame(model1, model2);
        Assert.Same(model1, modelSource.GetModel(new Context1(options), testModelDependencies, designTime: false));
        Assert.Same(model2, modelSource.GetModel(new Context2(options), testModelDependencies, designTime: false));

        Assert.NotSame(designModel1, designModel2);
        Assert.Same(designModel1, modelSource.GetModel(new Context1(options), testModelDependencies, designTime: true));
        Assert.Same(designModel2, modelSource.GetModel(new Context2(options), testModelDependencies, designTime: true));

        Assert.NotSame(model1, designModel1);
        Assert.NotSame(model2, designModel2);
    }

    [ConditionalFact]
    public void Model_from_options_is_preserved()
    {
        var options = InMemoryTestHelpers.Instance.CreateOptions();
        var context = new Context1(options);
        var serviceProvider = context.GetInfrastructure();

        var modelSource = serviceProvider.GetRequiredService<IModelSource>();
        var testModelDependencies = serviceProvider.GetRequiredService<ModelCreationDependencies>();

        var model = modelSource.GetModel(context, testModelDependencies, designTime: false);
        var designTimeModel = modelSource.GetModel(new Context1(options), testModelDependencies, designTime: true);

        Assert.NotSame(model, designTimeModel);

        var modelContext = new ModelContext(model, _serviceProvider);

        Assert.NotSame(modelContext.Model, modelContext.GetService<IDesignTimeModel>().Model);
        Assert.Same(model, modelContext.Model);
        Assert.NotSame(model, modelContext.GetService<IDesignTimeModel>().Model);

        var designTimeContext = new ModelContext(designTimeModel, _serviceProvider);

        Assert.NotSame(modelContext.Model, designTimeContext.GetService<IDesignTimeModel>().Model);
        Assert.NotSame(model, designTimeContext.Model);
        Assert.Same(designTimeModel, designTimeContext.GetService<IDesignTimeModel>().Model);
    }

    [ConditionalFact]
    public void Throws_for_model_from_options_with_different_version()
    {
        var model = (Model)InMemoryTestHelpers.Instance.CreateConventionBuilder().Model;
        model.SetProductVersion("1.0.0");

        var context = new ModelContext(model, _serviceProvider);
        var warning = CoreStrings.WarningAsErrorTemplate(
            CoreEventId.OldModelVersionWarning,
            CoreResources.LogOldModelVersion(
                new TestLogger<TestLoggingDefinitions>()).GenerateMessage("1.0.0", ProductInfo.GetVersion()),
            "CoreEventId.OldModelVersionWarning");

        Assert.Equal(
            warning,
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [ConditionalFact]
    public void Does_not_throw_for_model_from_options_with_different_patch_version()
    {
        var productVersion = ProductInfo.GetVersion();
        var productMinorVersion = productVersion[..productVersion.LastIndexOf('.')];

        var model = (Model)InMemoryTestHelpers.Instance.CreateConventionBuilder().Model;
        model.SetProductVersion(productMinorVersion + ".new");

        var context = new ModelContext(model, _serviceProvider);

        Assert.NotNull(context.Model);
    }

    private class ModelContext(IModel model, IServiceProvider serviceProvider) : DbContext
    {
        private readonly IModel _model = model;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder = optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(ModelContext))
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw));

            if (_model != null)
            {
                optionsBuilder.UseModel(_model);
            }
        }
    }

    [ConditionalFact]
    public void Stores_model_version_information_as_annotation_on_model()
    {
        var options = InMemoryTestHelpers.Instance.CreateOptions();
        var context = new Context1(options);
        var serviceProvider = context.GetInfrastructure();
        var modelSource = serviceProvider.GetRequiredService<IModelSource>();
        var testModelDependencies = serviceProvider.GetRequiredService<ModelCreationDependencies>();

        var model = modelSource.GetModel(context, testModelDependencies, designTime: false);
        var packageVersion = typeof(Context1).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(m => m.Key == "PackageVersion").Value;

        var prereleaseIndex = packageVersion.IndexOf("-", StringComparison.Ordinal);
        if (prereleaseIndex != -1)
        {
            packageVersion = packageVersion.Substring(0, prereleaseIndex);
        }

        Assert.StartsWith(packageVersion, model.GetProductVersion(), StringComparison.OrdinalIgnoreCase);
    }

    private class Context1(DbContextOptions options) : DbContext(options);

    private class Context2(DbContextOptions options) : DbContext(options);
}

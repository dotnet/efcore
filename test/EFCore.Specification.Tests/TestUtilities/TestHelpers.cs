// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public abstract class TestHelpers
{
    public DbContextOptions CreateOptions(IModel model, IServiceProvider? serviceProvider = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder()
            .UseInternalServiceProvider(serviceProvider);

        UseProviderOptions(optionsBuilder.UseModel(model));

        return optionsBuilder.Options;
    }

    public DbContextOptions CreateOptions(IServiceProvider? serviceProvider = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder()
            .UseInternalServiceProvider(serviceProvider);

        UseProviderOptions(optionsBuilder);

        return optionsBuilder.Options;
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection? customServices = null)
        => CreateServiceProvider(customServices, AddProviderServices);

    protected static IServiceProvider CreateServiceProvider(
        IServiceCollection? customServices,
        Func<IServiceCollection, IServiceCollection> addProviderServices)
    {
        var services = new ServiceCollection();
        addProviderServices(services);

        if (customServices != null)
        {
            foreach (var service in customServices)
            {
                services.Add(service);
            }
        }

        return services.BuildServiceProvider(); // No scope validation; test doubles violate scopes, but only resolved once.
    }

    public IServiceProvider CreateDesignServiceProvider(
        IServiceCollection? customServices = null,
        Action<EntityFrameworkDesignServicesBuilder>? replaceServices = null,
        Action<IServiceCollection>? addDesignTimeServices = null,
        IOperationReporter? reporter = null)
        => CreateDesignServiceProvider(
            CreateContext().GetService<IDatabaseProvider>().Name,
            customServices,
            replaceServices,
            addDesignTimeServices,
            reporter);

    public IServiceProvider CreateDesignServiceProvider(
        string provider,
        IServiceCollection? customServices = null,
        Action<EntityFrameworkDesignServicesBuilder>? replaceServices = null,
        Action<IServiceCollection>? addDesignTimeServices = null,
        IOperationReporter? reporter = null)
        => CreateServiceProvider(
            customServices, services =>
            {
                if (replaceServices != null)
                {
                    var builder = CreateEntityFrameworkDesignServicesBuilder(services);
                    replaceServices(builder);
                }

                if (addDesignTimeServices != null)
                {
                    addDesignTimeServices(services);
                }

                ConfigureProviderServices(provider, services);
                services.AddEntityFrameworkDesignTimeServices(reporter);

                return services;
            });

    protected virtual EntityFrameworkDesignServicesBuilder CreateEntityFrameworkDesignServicesBuilder(IServiceCollection services)
        => new(services);

    private void ConfigureProviderServices(string provider, IServiceCollection services)
    {
        var providerAssembly = Assembly.Load(new AssemblyName(provider));

        var providerServicesAttribute = providerAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>();
        if (providerServicesAttribute == null)
        {
            throw new InvalidOperationException(DesignStrings.CannotFindDesignTimeProviderAssemblyAttribute(provider));
        }

        var designTimeServicesType = providerAssembly.GetType(
            providerServicesAttribute.TypeName,
            throwOnError: true,
            ignoreCase: false)!;

        ConfigureDesignTimeServices(designTimeServicesType, services);
    }

    private static void ConfigureDesignTimeServices(
        Type designTimeServicesType,
        IServiceCollection services)
    {
        var designTimeServices = (IDesignTimeServices)Activator.CreateInstance(designTimeServicesType)!;
        designTimeServices.ConfigureDesignTimeServices(services);
    }

    public abstract IServiceCollection AddProviderServices(IServiceCollection services);

    public DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder optionsBuilder)
    {
        UseProviderOptions(optionsBuilder);
        return optionsBuilder;
    }

    public abstract DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder);

    public DbContext CreateContext(IServiceProvider serviceProvider, IModel model)
        => new(CreateOptions(model, serviceProvider));

    public DbContext CreateContext(IServiceProvider serviceProvider, DbContextOptions options)
        => new(new DbContextOptionsBuilder(options).UseInternalServiceProvider(serviceProvider).Options);

    public DbContext CreateContext(IServiceProvider serviceProvider)
        => new(CreateOptions(serviceProvider));

    public DbContext CreateContext(IModel model)
        => new(CreateOptions(model, CreateServiceProvider()));

    public DbContext CreateContext(DbContextOptions options)
        => new(new DbContextOptionsBuilder(options).UseInternalServiceProvider(CreateServiceProvider()).Options);

    public DbContext CreateContext()
        => new(CreateOptions(CreateServiceProvider()));

    public DbContext CreateContext(IServiceCollection customServices, IModel model)
        => new(CreateOptions(model, CreateServiceProvider(customServices)));

    public DbContext CreateContext(IServiceCollection customServices, DbContextOptions options)
        => new(
            new DbContextOptionsBuilder(options).UseInternalServiceProvider(CreateServiceProvider(customServices)).Options);

    public DbContext CreateContext(IServiceCollection customServices)
        => new(CreateOptions(CreateServiceProvider(customServices)));

    public IServiceProvider CreateContextServices(IServiceProvider serviceProvider, IModel model)
        => ((IInfrastructure<IServiceProvider>)CreateContext(serviceProvider, model)).Instance;

    public IServiceProvider CreateContextServices(IServiceProvider serviceProvider, DbContextOptions options)
        => ((IInfrastructure<IServiceProvider>)CreateContext(serviceProvider, options)).Instance;

    public IServiceProvider CreateContextServices(IServiceProvider serviceProvider)
        => ((IInfrastructure<IServiceProvider>)CreateContext(serviceProvider)).Instance;

    public IServiceProvider CreateContextServices(IModel model)
        => ((IInfrastructure<IServiceProvider>)CreateContext(model)).Instance;

    public IServiceProvider CreateContextServices(DbContextOptions options)
        => ((IInfrastructure<IServiceProvider>)CreateContext(options)).Instance;

    public IServiceProvider CreateContextServices()
        => ((IInfrastructure<IServiceProvider>)CreateContext()).Instance;

    public IServiceProvider CreateContextServices(IServiceCollection customServices, IModel model)
        => ((IInfrastructure<IServiceProvider>)CreateContext(customServices, model)).Instance;

    public IServiceProvider CreateContextServices(IServiceCollection customServices, DbContextOptions options)
        => ((IInfrastructure<IServiceProvider>)CreateContext(customServices, options)).Instance;

    public IServiceProvider CreateContextServices(IServiceCollection customServices)
        => ((IInfrastructure<IServiceProvider>)CreateContext(customServices)).Instance;

    public TestModelBuilder CreateConventionBuilder(
        IDiagnosticsLogger<DbLoggerCategory.Model>? modelLogger = null,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation>? validationLogger = null,
        Action<TestModelConfigurationBuilder>? configureConventions = null,
        Func<DbContextOptionsBuilder, DbContextOptionsBuilder>? configureContext = null,
        Func<IServiceCollection, IServiceCollection>? addServices = null)
    {
        var customServices = new ServiceCollection();
        addServices?.Invoke(customServices);

        if (modelLogger != null)
        {
            customServices.AddScoped(_ => modelLogger);
        }

        if (validationLogger != null)
        {
            customServices.AddScoped(_ => validationLogger);
        }

        var optionsBuilder = UseProviderOptions(new DbContextOptionsBuilder());
        var services = configureContext == null
            ? CreateContextServices(customServices, optionsBuilder.Options)
            : CreateContextServices(customServices, configureContext(optionsBuilder).Options);

        return CreateConventionBuilder(services, configureConventions, validationLogger);
    }

    public TestModelBuilder CreateConventionBuilder(
        IServiceProvider contextServices,
        Action<TestModelConfigurationBuilder>? configureConventions = null,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation>? validationLogger = null)
    {
        var modelCreationDependencies = contextServices.GetRequiredService<ModelCreationDependencies>();

        var modelConfigurationBuilder = new TestModelConfigurationBuilder(
            modelCreationDependencies.ConventionSetBuilder.CreateConventionSet(),
            contextServices);

        configureConventions?.Invoke(modelConfigurationBuilder);

        return modelConfigurationBuilder.CreateModelBuilder(
            modelCreationDependencies.ModelDependencies,
            modelCreationDependencies.ModelRuntimeInitializer,
            validationLogger ?? contextServices.GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Model.Validation>>());
    }

    public virtual LoggingDefinitions LoggingDefinitions { get; } = new TestLoggingDefinitions();

    public InternalEntityEntry CreateInternalEntry<TEntity>(
        IModel model,
        EntityState entityState = EntityState.Detached,
        TEntity? entity = null)
        where TEntity : class, new()
    {
        var entry = CreateContextServices(model)
            .GetRequiredService<IStateManager>()
            .GetOrCreateEntry(entity ?? new TEntity());

        entry.SetEntityState(entityState);

        return entry;
    }

    public virtual ModelAsserter ModelAsserter
        => ModelAsserter.Instance;

    private static int AssertResults<T>(IList<T> expected, IList<T> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        foreach (var expectedItem in expected)
        {
            Assert.True(
                actual.Contains(expectedItem),
                $"\r\nExpected item: [{expectedItem}] not found in results: [{string.Join(", ", actual.Take(10))}]...");
        }

        return actual.Count;
    }

    public static int AssertResults<T>(
        IList<T> expected,
        IList<T> actual,
        Func<T, object?>? elementSorter,
        Action<T, T>? elementAsserter,
        bool verifyOrdered)
    {
        Assert.Equal(expected.Count, actual.Count);

        if (elementSorter == null
            && !verifyOrdered
            && expected.Count > 1 // If there is only 1 element then sorting is not necessary
            && expected.FirstOrDefault(e => e != null) is T nonNullElement
            && nonNullElement.GetType().GetInterface(nameof(IComparable)) == null)
        {
            if (elementAsserter != null)
            {
                throw new InvalidOperationException(
                    "Element asserter will not be used because results are not properly ordered - either remove asserter from the AssertQuery, add element sorter or set assertOrder to 'true'.");
            }

            return AssertResults(expected, actual);
        }

        elementSorter ??= (e => e);
        elementAsserter ??= Assert.Equal;
        if (!verifyOrdered)
        {
            expected = expected.OrderBy(elementSorter).ToList();
            actual = actual.OrderBy(elementSorter).ToList();
        }

        for (var i = 0; i < expected.Count; i++)
        {
            elementAsserter(expected[i], actual[i]);
        }

        return actual.Count;
    }

    public static void AssertAllMethodsOverridden(Type testClass)
    {
        var methods = testClass
            .GetRuntimeMethods()
            .Where(
                m => m.DeclaringType != testClass
                    && (Attribute.IsDefined(m, typeof(ConditionalFactAttribute))
                        || Attribute.IsDefined(m, typeof(ConditionalTheoryAttribute))))
            .ToList();

        var methodCalls = new StringBuilder();

        foreach (var method in methods)
        {
            if (method.ReturnType == typeof(Task))
            {
                methodCalls.Append(
                    @$"public override async Task {method.Name}(bool async)
{{
    await base.{method.Name}(async);

    AssertSql();
}}

");
            }
            else
            {
                methodCalls.Append(
                    @$"public override void {method.Name}()
{{
    base.{method.Name}();

    AssertSql();
}}

");
            }
        }

        Assert.False(
            methods.Count > 0,
            "\r\n-- Missing test overrides --\r\n\r\n" + methodCalls);
    }

    public static async Task ExecuteWithStrategyInTransactionAsync<TContext>(
        Func<TContext> createContext,
        Action<DatabaseFacade, IDbContextTransaction> useTransaction,
        Func<TContext, Task> testOperation,
        Func<TContext, Task>? nestedTestOperation1 = null,
        Func<TContext, Task>? nestedTestOperation2 = null,
        Func<TContext, Task>? nestedTestOperation3 = null)
        where TContext : DbContext
    {
        using var c = createContext();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                using (var innerContext = createContext())
                {
                    useTransaction(innerContext.Database, transaction);
                    await testOperation(innerContext);
                }

                if (nestedTestOperation1 == null)
                {
                    return;
                }

                using (var innerContext1 = createContext())
                {
                    useTransaction(innerContext1.Database, transaction);
                    await nestedTestOperation1(innerContext1);
                }

                if (nestedTestOperation2 == null)
                {
                    return;
                }

                using (var innerContext2 = createContext())
                {
                    useTransaction(innerContext2.Database, transaction);
                    await nestedTestOperation2(innerContext2);
                }

                if (nestedTestOperation3 == null)
                {
                    return;
                }

                using var innerContext3 = createContext();
                useTransaction(innerContext3.Database, transaction);
                await nestedTestOperation3(innerContext3);
            });
    }

    public class TestModelBuilder(
        ConventionSet conventions,
        ModelDependencies modelDependencies,
        ModelConfiguration? modelConfiguration,
        IModelRuntimeInitializer modelRuntimeInitializer,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger)
        : ModelBuilder(conventions, modelDependencies, modelConfiguration)
    {
        private readonly IModelRuntimeInitializer _modelRuntimeInitializer = modelRuntimeInitializer;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model.Validation> _validationLogger = validationLogger;

        public override IModel FinalizeModel()
            => FinalizeModel(designTime: false);

        public IModel FinalizeModel(bool designTime = false, bool skipValidation = false)
            => _modelRuntimeInitializer.Initialize((IModel)Model, designTime, skipValidation ? null : _validationLogger);
    }

    public class TestModelConfigurationBuilder(ConventionSet conventionSet, IServiceProvider serviceProvider)
        : ModelConfigurationBuilder(conventionSet, serviceProvider)
    {
        public ConventionSet ConventionSet { get; } = conventionSet;

        public TestModelBuilder CreateModelBuilder(
            ModelDependencies modelDependencies,
            IModelRuntimeInitializer modelRuntimeInitializer,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger)
            => new(
                ConventionSet,
                modelDependencies,
                ModelConfiguration.IsEmpty() ? null : ModelConfiguration.Validate(),
                modelRuntimeInitializer,
                validationLogger);

        public void RemoveAllConventions()
        {
            ConventionSet.EntityTypeAddedConventions.Clear();
            ConventionSet.EntityTypeAnnotationChangedConventions.Clear();
            ConventionSet.EntityTypeBaseTypeChangedConventions.Clear();
            ConventionSet.TypeIgnoredConventions.Clear();
            ConventionSet.EntityTypeMemberIgnoredConventions.Clear();
            ConventionSet.EntityTypePrimaryKeyChangedConventions.Clear();
            ConventionSet.EntityTypeRemovedConventions.Clear();
            ConventionSet.ForeignKeyAddedConventions.Clear();
            ConventionSet.ForeignKeyAnnotationChangedConventions.Clear();
            ConventionSet.ForeignKeyDependentRequirednessChangedConventions.Clear();
            ConventionSet.ForeignKeyOwnershipChangedConventions.Clear();
            ConventionSet.ForeignKeyPrincipalEndChangedConventions.Clear();
            ConventionSet.ForeignKeyPropertiesChangedConventions.Clear();
            ConventionSet.ForeignKeyRemovedConventions.Clear();
            ConventionSet.ForeignKeyRequirednessChangedConventions.Clear();
            ConventionSet.ForeignKeyUniquenessChangedConventions.Clear();
            ConventionSet.IndexAddedConventions.Clear();
            ConventionSet.IndexAnnotationChangedConventions.Clear();
            ConventionSet.IndexRemovedConventions.Clear();
            ConventionSet.IndexUniquenessChangedConventions.Clear();
            ConventionSet.IndexSortOrderChangedConventions.Clear();
            ConventionSet.KeyAddedConventions.Clear();
            ConventionSet.KeyAnnotationChangedConventions.Clear();
            ConventionSet.KeyRemovedConventions.Clear();
            ConventionSet.ModelAnnotationChangedConventions.Clear();
            ConventionSet.ModelFinalizedConventions.Clear();
            ConventionSet.ModelFinalizingConventions.Clear();
            ConventionSet.ModelInitializedConventions.Clear();
            ConventionSet.NavigationAddedConventions.Clear();
            ConventionSet.NavigationAnnotationChangedConventions.Clear();
            ConventionSet.NavigationRemovedConventions.Clear();
            ConventionSet.PropertyAddedConventions.Clear();
            ConventionSet.PropertyAnnotationChangedConventions.Clear();
            ConventionSet.PropertyFieldChangedConventions.Clear();
            ConventionSet.PropertyNullabilityChangedConventions.Clear();
            ConventionSet.PropertyRemovedConventions.Clear();
            ConventionSet.SkipNavigationAddedConventions.Clear();
            ConventionSet.SkipNavigationAnnotationChangedConventions.Clear();
            ConventionSet.SkipNavigationForeignKeyChangedConventions.Clear();
            ConventionSet.SkipNavigationInverseChangedConventions.Clear();
            ConventionSet.SkipNavigationRemovedConventions.Clear();
        }
    }
}

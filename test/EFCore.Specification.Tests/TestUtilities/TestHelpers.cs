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
    public DbContextOptions CreateOptions(IModel model, IServiceProvider serviceProvider = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder()
            .UseInternalServiceProvider(serviceProvider);

        UseProviderOptions(optionsBuilder.UseModel(model));

        return optionsBuilder.Options;
    }

    public DbContextOptions CreateOptions(IServiceProvider serviceProvider = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder()
            .UseInternalServiceProvider(serviceProvider);

        UseProviderOptions(optionsBuilder);

        return optionsBuilder.Options;
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection customServices = null)
        => CreateServiceProvider(customServices, AddProviderServices);

    protected static IServiceProvider CreateServiceProvider(
        IServiceCollection customServices,
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
        IDiagnosticsLogger<DbLoggerCategory.Model> modelLogger = null,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger = null,
        Action<TestModelConfigurationBuilder> configureModel = null,
        Func<DbContextOptionsBuilder, DbContextOptionsBuilder> configureContext = null,
        IServiceCollection customServices = null)
    {
        customServices ??= new ServiceCollection();
        if (modelLogger != null)
        {
            customServices.AddScoped(_ => modelLogger);
        }

        if (validationLogger != null)
        {
            customServices.AddScoped(_ => validationLogger);
        }

        var services = configureContext == null
            ? CreateContextServices(customServices)
            : CreateContextServices(
                customServices,
                configureContext(UseProviderOptions(new DbContextOptionsBuilder())).Options);

        return CreateConventionBuilder(services, configureModel, validationLogger);
    }

    public TestModelBuilder CreateConventionBuilder(
        IServiceProvider contextServices,
        Action<TestModelConfigurationBuilder> configure = null,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger = null)
    {
        var modelCreationDependencies = contextServices.GetRequiredService<ModelCreationDependencies>();

        var modelConfigurationBuilder = new TestModelConfigurationBuilder(
            modelCreationDependencies.ConventionSetBuilder.CreateConventionSet());

        configure?.Invoke(modelConfigurationBuilder);

        return modelConfigurationBuilder.CreateModelBuilder(
            modelCreationDependencies.ModelDependencies,
            modelCreationDependencies.ModelRuntimeInitializer,
            validationLogger ?? contextServices.GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Model.Validation>>());
    }

    public virtual LoggingDefinitions LoggingDefinitions { get; } = new TestLoggingDefinitions();

    public InternalEntityEntry CreateInternalEntry<TEntity>(
        IModel model,
        EntityState entityState = EntityState.Detached,
        TEntity entity = null)
        where TEntity : class, new()
    {
        var entry = CreateContextServices(model)
            .GetRequiredService<IStateManager>()
            .GetOrCreateEntry(entity ?? new TEntity());

        entry.SetEntityState(entityState);

        return entry;
    }

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
        Func<T, object> elementSorter,
        Action<T, T> elementAsserter,
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
                    && m.GetCustomAttributes()
                        .Any(
                            a => a is ConditionalFactAttribute
                                || a is ConditionalTheoryAttribute))
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
            "\r\n-- Missing test overrides --\r\n" + methodCalls);
    }

    public static void ExecuteWithStrategyInTransaction<TContext>(
        Func<TContext> createContext,
        Action<DatabaseFacade, IDbContextTransaction> useTransaction,
        Action<TContext> testOperation,
        Action<TContext> nestedTestOperation1 = null,
        Action<TContext> nestedTestOperation2 = null,
        Action<TContext> nestedTestOperation3 = null)
        where TContext : DbContext
    {
        using var c = createContext();
        c.Database.CreateExecutionStrategy().Execute(
            c, context =>
            {
                using var transaction = context.Database.BeginTransaction();
                using (var innerContext = createContext())
                {
                    useTransaction(innerContext.Database, transaction);
                    testOperation(innerContext);
                }

                if (nestedTestOperation1 == null)
                {
                    return;
                }

                using (var innerContext1 = createContext())
                {
                    useTransaction(innerContext1.Database, transaction);
                    nestedTestOperation1(innerContext1);
                }

                if (nestedTestOperation2 == null)
                {
                    return;
                }

                using (var innerContext2 = createContext())
                {
                    useTransaction(innerContext2.Database, transaction);
                    nestedTestOperation2(innerContext2);
                }

                if (nestedTestOperation3 == null)
                {
                    return;
                }

                using var innerContext3 = createContext();
                useTransaction(innerContext3.Database, transaction);
                nestedTestOperation3(innerContext3);
            });
    }

    public static async Task ExecuteWithStrategyInTransactionAsync<TContext>(
        Func<TContext> createContext,
        Action<DatabaseFacade, IDbContextTransaction> useTransaction,
        Func<TContext, Task> testOperation,
        Func<TContext, Task> nestedTestOperation1 = null,
        Func<TContext, Task> nestedTestOperation2 = null,
        Func<TContext, Task> nestedTestOperation3 = null)
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

    public class TestModelBuilder : ModelBuilder
    {
        private readonly IModelRuntimeInitializer _modelRuntimeInitializer;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model.Validation> _validationLogger;

        public TestModelBuilder(
            ConventionSet conventions,
            ModelDependencies modelDependencies,
            ModelConfiguration modelConfiguration,
            IModelRuntimeInitializer modelRuntimeInitializer,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger)
            : base(conventions, modelDependencies, modelConfiguration)
        {
            _modelRuntimeInitializer = modelRuntimeInitializer;
            _validationLogger = validationLogger;
        }

        public override IModel FinalizeModel()
            => FinalizeModel(designTime: false);

        public IModel FinalizeModel(bool designTime = false, bool skipValidation = false)
            => _modelRuntimeInitializer.Initialize((IModel)Model, designTime, skipValidation ? null : _validationLogger);
    }

    public class TestModelConfigurationBuilder : ModelConfigurationBuilder
    {
        public TestModelConfigurationBuilder(ConventionSet conventions)
            : base(conventions)
        {
            Conventions = conventions;
        }

        public ConventionSet Conventions { get; }

        public TestModelBuilder CreateModelBuilder(
            ModelDependencies modelDependencies,
            IModelRuntimeInitializer modelRuntimeInitializer,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger)
            => new(
                Conventions,
                modelDependencies,
                ModelConfiguration.IsEmpty() ? null : ModelConfiguration.Validate(),
                modelRuntimeInitializer,
                validationLogger);

        public void RemoveAllConventions()
        {
            Conventions.EntityTypeAddedConventions.Clear();
            Conventions.EntityTypeAnnotationChangedConventions.Clear();
            Conventions.EntityTypeBaseTypeChangedConventions.Clear();
            Conventions.EntityTypeIgnoredConventions.Clear();
            Conventions.EntityTypeMemberIgnoredConventions.Clear();
            Conventions.EntityTypePrimaryKeyChangedConventions.Clear();
            Conventions.EntityTypeRemovedConventions.Clear();
            Conventions.ForeignKeyAddedConventions.Clear();
            Conventions.ForeignKeyAnnotationChangedConventions.Clear();
            Conventions.ForeignKeyDependentRequirednessChangedConventions.Clear();
            Conventions.ForeignKeyOwnershipChangedConventions.Clear();
            Conventions.ForeignKeyPrincipalEndChangedConventions.Clear();
            Conventions.ForeignKeyPropertiesChangedConventions.Clear();
            Conventions.ForeignKeyRemovedConventions.Clear();
            Conventions.ForeignKeyRequirednessChangedConventions.Clear();
            Conventions.ForeignKeyUniquenessChangedConventions.Clear();
            Conventions.IndexAddedConventions.Clear();
            Conventions.IndexAnnotationChangedConventions.Clear();
            Conventions.IndexRemovedConventions.Clear();
            Conventions.IndexUniquenessChangedConventions.Clear();
            Conventions.IndexSortOrderChangedConventions.Clear();
            Conventions.KeyAddedConventions.Clear();
            Conventions.KeyAnnotationChangedConventions.Clear();
            Conventions.KeyRemovedConventions.Clear();
            Conventions.ModelAnnotationChangedConventions.Clear();
            Conventions.ModelFinalizedConventions.Clear();
            Conventions.ModelFinalizingConventions.Clear();
            Conventions.ModelInitializedConventions.Clear();
            Conventions.NavigationAddedConventions.Clear();
            Conventions.NavigationAnnotationChangedConventions.Clear();
            Conventions.NavigationRemovedConventions.Clear();
            Conventions.PropertyAddedConventions.Clear();
            Conventions.PropertyAnnotationChangedConventions.Clear();
            Conventions.PropertyFieldChangedConventions.Clear();
            Conventions.PropertyNullabilityChangedConventions.Clear();
            Conventions.PropertyRemovedConventions.Clear();
            Conventions.SkipNavigationAddedConventions.Clear();
            Conventions.SkipNavigationAnnotationChangedConventions.Clear();
            Conventions.SkipNavigationForeignKeyChangedConventions.Clear();
            Conventions.SkipNavigationInverseChangedConventions.Clear();
            Conventions.SkipNavigationRemovedConventions.Clear();
        }
    }
}

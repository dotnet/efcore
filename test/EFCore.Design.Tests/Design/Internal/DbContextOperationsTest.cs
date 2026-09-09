// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

public class DbContextOperationsTest
{
    [ConditionalFact]
    public void CreateContext_gets_service()
        => CreateOperations(typeof(TestProgram), includeContext: false).CreateContext(typeof(TestContext).FullName.ToLower());

    [ConditionalFact]
    public void CreateContext_gets_service_without_name()
        => CreateOperations(typeof(TestProgram), includeContext: false).CreateContext(null);

    [ConditionalFact]
    public void CreateContext_gets_service_without_AddDbContext()
        => CreateOperations(typeof(TestProgramWithoutAddDbContext)).CreateContext(typeof(TestContext).FullName);

    [ConditionalFact]
    public void CreateContext_gets_service_when_context_factory_used()
        => CreateOperations(typeof(TestProgramWithContextFactory), includeContext: false).CreateContext(typeof(TestContextFromFactory).FullName);

    [ConditionalFact]
    public void CreateContext_gets_service_when_context_factory_used_without_name()
        => CreateOperations(typeof(TestProgramWithContextFactory), includeContext: false).CreateContext(null);

    [ConditionalFact]
    public void CreateContext_throws_if_context_type_not_found()
        => Assert.Equal(
            DesignStrings.NoContextWithName(typeof(TestContextFromFactory).FullName),
            Assert.Throws<OperationException>(
                () => CreateOperations(typeof(TestProgramRelationalBad)).CreateContext(typeof(TestContextFromFactory).FullName)).Message);

    [ConditionalFact]
    public void CreateContext_throws_if_ambiguous_context_type_by_case()
    {
        var assembly = MockAssembly.Create(typeof(TestContext), typeof(Testcontext));
        var reporter = new TestOperationReporter();
        var operations = new TestDbContextOperations(
            reporter,
            assembly,
            assembly,
            project: "",
            projectDir: "",
            rootNamespace: null,
            language: "C#",
            nullable: false,
            /* args: */ [],
            new TestAppServiceProviderFactory(assembly, reporter));

        Assert.Equal(
            DesignStrings.MultipleContextsWithName(typeof(TestContext).FullName.ToLower()),
            Assert.Throws<OperationException>(() => operations.CreateContext(typeof(TestContext).FullName.ToLower())).Message);

        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Critical);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Error);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public void CreateContext_throws_if_ambiguous_context_type_by_namespace()
    {
        var assembly = MockAssembly.Create(typeof(TestContext), typeof(DatabaseOperationsTest.TestContext));
        var reporter = new TestOperationReporter();
        var operations = new TestDbContextOperations(
            reporter,
            assembly,
            assembly,
            project: "",
            projectDir: "",
            rootNamespace: null,
            language: "C#",
            nullable: false,
            /* args: */ [],
            new TestAppServiceProviderFactory(assembly, reporter));

        Assert.Equal(
            DesignStrings.MultipleContextsWithQualifiedName(nameof(TestContext)),
            Assert.Throws<OperationException>(() => operations.CreateContext(nameof(TestContext))).Message);

        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Critical);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Error);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public void CreateContext_throws_if_ambiguous_context_type()
    {
        var assembly = MockAssembly.Create(typeof(TestContext), typeof(Testcontext));
        var reporter = new TestOperationReporter();
        var operations = new TestDbContextOperations(
            reporter,
            assembly,
            assembly,
            project: "",
            projectDir: "",
            rootNamespace: null,
            language: "C#",
            nullable: false,
            /* args: */ [],
            new TestAppServiceProviderFactory(assembly, reporter));

        Assert.Equal(
            DesignStrings.MultipleContexts,
            Assert.Throws<OperationException>(() => operations.CreateContext(null)).Message);

        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Critical);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Error);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public void CreateContext_throws_if_no_context_type()
    {
        var assembly = MockAssembly.Create();
        var reporter = new TestOperationReporter();
        var operations = new TestDbContextOperations(
            reporter,
            assembly,
            assembly,
            project: "",
            projectDir: "",
            rootNamespace: null,
            language: "C#",
            nullable: false,
            /* args: */ [],
            new TestAppServiceProviderFactory(assembly, reporter));

        Assert.Equal(
            DesignStrings.NoContext(nameof(MockAssembly)),
            Assert.Throws<OperationException>(() => operations.CreateContext(null)).Message);

        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Critical);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Error);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public void Can_pass_null_args()
    {
        // Even though newer versions of the tools will pass an empty array
        // older versions of the tools can pass null args.
        var assembly = MockAssembly.Create(typeof(TestContext));
        var reporter = new TestOperationReporter();
        var operations = new TestDbContextOperations(
            reporter,
            assembly,
            assembly,
            project: "",
            projectDir: "",
            rootNamespace: null,
            language: "C#",
            nullable: false,
            args: null,
            new TestAppServiceProviderFactory(assembly, reporter));

        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Critical);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Error);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public void CreateContext_uses_exact_factory_method()
    {
        var assembly = MockAssembly.Create(typeof(BaseContext), typeof(DerivedContext), typeof(HierarchyContextFactory));
        var reporter = new TestOperationReporter();
        var operations = new TestDbContextOperations(
            reporter,
            assembly,
            assembly,
            project: "",
            projectDir: "",
            rootNamespace: null,
            language: "C#",
            nullable: false,
            args: [],
            new TestAppServiceProviderFactory(assembly, reporter, throwOnCreate: true));

        var baseContext = Assert.IsType<BaseContext>(operations.CreateContext(nameof(BaseContext)));
        Assert.Equal(nameof(BaseContext), baseContext.FactoryUsed);

        var derivedContext = Assert.IsType<DerivedContext>(operations.CreateContext(nameof(DerivedContext)));
        Assert.Equal(nameof(DerivedContext), derivedContext.FactoryUsed);

        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Critical);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Error);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public void CreateAllContexts_creates_all_contexts()
    {
        var assembly = MockAssembly.Create(typeof(BaseContext), typeof(DerivedContext), typeof(HierarchyContextFactory));
        var reporter = new TestOperationReporter();
        var operations = new TestDbContextOperations(
            reporter,
            assembly,
            assembly,
            project: "",
            projectDir: "",
            rootNamespace: null,
            language: "C#",
            nullable: false,
            args: [],
            new TestAppServiceProviderFactory(assembly, reporter, throwOnCreate: true));

        var contexts = operations.CreateAllContexts().ToList();
        Assert.Collection(
            contexts,
            c => Assert.Equal(nameof(BaseContext), Assert.IsType<BaseContext>(c).FactoryUsed),
            c => Assert.Equal(nameof(DerivedContext), Assert.IsType<DerivedContext>(c).FactoryUsed));

        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Critical);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Error);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public void Optimize_throws_when_no_contexts()
    {
        var assembly = MockAssembly.Create();
        var reporter = new TestOperationReporter();
        var operations = new TestDbContextOperations(
            reporter,
            assembly,
            assembly,
            project: "",
            projectDir: "",
            rootNamespace: null,
            language: "C#",
            nullable: false,
            args: [],
            new TestAppServiceProviderFactory(assembly, reporter, throwOnCreate: true));

        Assert.Equal(
            DesignStrings.NoContextsToOptimize,
            Assert.Throws<OperationException>(() =>
                operations.Optimize(
                    null, null, contextTypeName: "*", null, scaffoldModel: true, precompileQueries: false, nativeAot: false)).Message);

        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Critical);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Error);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public void Optimize_shows_warning_when_nothing_was_generated()
    {
        var assembly = MockAssembly.Create(typeof(DerivedContext));
        var reporter = new TestOperationReporter();
        var operations = new TestDbContextOperations(
            reporter,
            assembly,
            assembly,
            project: "",
            projectDir: "",
            rootNamespace: null,
            language: "C#",
            nullable: false,
            args: [],
            new TestAppServiceProviderFactory(assembly, reporter, throwOnCreate: true));

        operations.Optimize(null, null, contextTypeName: "*", null, scaffoldModel: true, precompileQueries: false, nativeAot: false);

        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Critical);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Error);

        Assert.Equal(
            DesignStrings.OptimizeNoFilesGenerated,
            Assert.Single(reporter.Messages.Where(m => m.Level == LogLevel.Warning)).Message);
    }

    [ConditionalFact]
    public void GetContextInfo_returns_correct_info()
    {
        var info = CreateOperations(typeof(TestProgramRelational)).GetContextInfo(nameof(TestContext));

        Assert.Equal("Test", info.DatabaseName);
        Assert.Equal(@"(localdb)\mssqllocaldb", info.DataSource);
        Assert.Equal("EngineType=SqlServer", info.Options);
        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", info.ProviderName);
    }

    [ConditionalFact]
    public void GetContextInfo_does_not_throw_if_DbConnection_cannot_be_created()
    {
        Exception expected = null;
        try
        {
            new SqlConnection("Cake=None");
        }
        catch (Exception e)
        {
            expected = e;
        }

        var info = CreateOperations(typeof(TestProgramRelationalBad)).GetContextInfo(nameof(TestContext));

        Assert.Equal(DesignStrings.BadConnection(expected.Message), info.DatabaseName);
        Assert.Equal(DesignStrings.BadConnection(expected.Message), info.DataSource);
        Assert.Equal("EngineType=SqlServer", info.Options);
        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", info.ProviderName);
    }

    [ConditionalFact]
    public void GetContextInfo_does_not_throw_if_provider_not_relational()
    {
        var info = CreateOperations(typeof(TestProgram)).GetContextInfo(nameof(TestContext));

        Assert.Equal(DesignStrings.NoRelationalConnection, info.DatabaseName);
        Assert.Equal(DesignStrings.NoRelationalConnection, info.DataSource);
        Assert.Equal("StoreName=In-memory test database", info.Options);
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", info.ProviderName);
    }

    [ConditionalFact]
    public void Useful_exception_if_finding_context_types_throws()
        => Assert.Equal(
            DesignStrings.CannotFindDbContextTypes("Bang!"),
            Assert.Throws<OperationException>(
                () => CreateOperations(typeof(ThrowingTestProgram)).CreateContext(typeof(TestContext).FullName)).Message);

    private static class ThrowingTestProgram
    {
        private static TestWebHost BuildWebHost(string[] args)
            => CreateWebHost(_ => throw new Exception("Bang!"));
    }

    private static class TestProgram
    {
        private static TestWebHost BuildWebHost(string[] args)
            => CreateWebHost(b => b.UseInMemoryDatabase("In-memory test database"));
    }

    private static class TestProgramWithoutAddDbContext
    {
        private static TestWebHost BuildWebHost(string[] args)
            => new(
                new ServiceCollection()
                    .AddSingleton(
                        new TestContext(
                            new DbContextOptionsBuilder<TestContext>()
                                .UseInMemoryDatabase("In-memory test database")
                                .EnableServiceProviderCaching(false)
                                .Options))
                    .BuildServiceProvider(validateScopes: true));
    }

    private static class TestProgramWithContextFactory
    {
        private static TestWebHost BuildWebHost(string[] args)
            => new(
                new ServiceCollection()
                    .AddDbContextFactory<TestContextFromFactory>(b => b.UseInMemoryDatabase("In-memory test database"))
                    .BuildServiceProvider(validateScopes: true));
    }

    private static class TestProgramRelational
    {
        private static TestWebHost BuildWebHost(string[] args)
            => CreateWebHost(b => b.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test;ConnectRetryCount=0"));
    }

    private static class TestProgramRelationalBad
    {
        private static TestWebHost BuildWebHost(string[] args)
            => CreateWebHost(b => b.UseSqlServer(@"Cake=None"));
    }

    private static TestDbContextOperations CreateOperations(Type testProgramType, bool includeContext = true)
    {
        List<Type> types = [testProgramType];
        if (includeContext)
        {
            types.Add(typeof(TestContext));
        }
        var assembly = MockAssembly.Create([.. types]);
        var reporter = new TestOperationReporter();
        var operations = new TestDbContextOperations(
            reporter,
            assembly,
            assembly,
            project: "",
            projectDir: "",
            rootNamespace: null,
            language: "C#",
            nullable: false,
            /* args: */ [],
            new TestAppServiceProviderFactory(assembly, reporter));

        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Critical);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Error);
        Assert.DoesNotContain(reporter.Messages, m => m.Level == LogLevel.Warning);

        return operations;
    }

    private static TestWebHost CreateWebHost(Func<DbContextOptionsBuilder, DbContextOptionsBuilder> configureProvider)
        => new(
            new ServiceCollection()
                .AddDbContext<TestContext>(
                    b => configureProvider(b.EnableServiceProviderCaching(false)))
                .BuildServiceProvider(validateScopes: true));

    private class TestContext : DbContext
    {
        public TestContext()
            => throw new Exception("This isn't the constructor you're looking for.");

        public TestContext(DbContextOptions<TestContext> options)
            : base(options)
        {
            Assert.Equal("Development", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            Assert.Equal("Development", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
        }
    }

    private class TestContextFromFactory : DbContext
    {
        private TestContextFromFactory()
            => throw new Exception("This isn't the constructor you're looking for.");

        public TestContextFromFactory(DbContextOptions<TestContextFromFactory> options)
            : base(options)
        {
            Assert.Equal("Development", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            Assert.Equal("Development", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
        }
    }

    private class Testcontext : DbContext
    {
        public Testcontext()
            => throw new Exception("This isn't the constructor you're looking for.");

        public Testcontext(DbContextOptions<TestContext> options)
            : base(options)
        {
        }
    }

    private class BaseContext(string factoryUsed) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseInMemoryDatabase(GetType().Name);

        public string FactoryUsed { get; } = factoryUsed;
    }

    private class DerivedContext(string factoryUsed) : BaseContext(factoryUsed);

    private class HierarchyContextFactory : IDesignTimeDbContextFactory<BaseContext>, IDesignTimeDbContextFactory<DerivedContext>
    {
        BaseContext IDesignTimeDbContextFactory<BaseContext>.CreateDbContext(string[] args)
            => new(nameof(BaseContext));

        DerivedContext IDesignTimeDbContextFactory<DerivedContext>.CreateDbContext(string[] args)
            => new(nameof(DerivedContext));
    }
}

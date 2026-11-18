// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

public class DatabaseOperationsTest
{
    [Fact]
    public void Can_pass_null_args()
        // Even though newer versions of the tools will pass an empty array
        // older versions of the tools can pass null args.
        => CreateOperations(null);

    [Fact]
    public void ScaffoldContext_throws_exceptions_for_invalid_context_name()
    {
        ValidateContextNameInReverseEngineerGenerator("Invalid!CSharp*Class&Name");
        ValidateContextNameInReverseEngineerGenerator("1CSharpClassNameCannotStartWithNumber");
        ValidateContextNameInReverseEngineerGenerator("volatile");
    }

    [Fact]
    public void ScaffoldContext_throws_when_provider_does_not_support_reverse_engineering()
    {
        var operations = CreateOperations([]);

        var ex = Assert.Throws<OperationException>(() => operations.ScaffoldContext(
            "Microsoft.EntityFrameworkCore.InMemory",
            "connectionstring",
            "",
            "",
            dbContextClassName: nameof(TestContext),
            null!,
            null!,
            "FakeNamespace",
            contextNamespace: null,
            useDataAnnotations: false,
            overwriteFiles: true,
            useDatabaseNames: false,
            suppressOnConfiguring: true,
            noPluralize: false));

        Assert.Equal(
            DesignStrings.CannotScaffoldProvider("Microsoft.EntityFrameworkCore.InMemory"),
            ex.Message);
    }

    [Fact]
    public void ScaffoldContext_throws_when_provider_assembly_lacks_design_time_attribute()
    {
        var operations = CreateOperations([]);

        var ex = Assert.Throws<OperationException>(() => operations.ScaffoldContext(
            "Microsoft.EntityFrameworkCore",
            "connectionstring",
            "",
            "",
            dbContextClassName: nameof(TestContext),
            null!,
            null!,
            "FakeNamespace",
            contextNamespace: null,
            useDataAnnotations: false,
            overwriteFiles: true,
            useDatabaseNames: false,
            suppressOnConfiguring: true,
            noPluralize: false));

        Assert.Equal(
            DesignStrings.CannotScaffoldProvider("Microsoft.EntityFrameworkCore"),
            ex.Message);
    }

    [Fact]
    public void ScaffoldContext_throws_when_provider_assembly_not_found()
    {
        var operations = CreateOperations([]);

        var ex = Assert.Throws<OperationException>(() => operations.ScaffoldContext(
            "Unknown.Provider",
            "connectionstring",
            "",
            "",
            dbContextClassName: nameof(TestContext),
            null!,
            null!,
            "FakeNamespace",
            contextNamespace: null,
            useDataAnnotations: false,
            overwriteFiles: true,
            useDatabaseNames: false,
            suppressOnConfiguring: true,
            noPluralize: false));

        Assert.Equal(
            DesignStrings.CannotFindRuntimeProviderAssembly("Unknown.Provider"),
            ex.Message);
    }

    private void ValidateContextNameInReverseEngineerGenerator(string contextName)
    {
        var operations = CreateOperations([]);

        Assert.Equal(
            DesignStrings.ContextClassNotValidCSharpIdentifier(contextName),
            Assert.Throws<ArgumentException>(() => operations.ScaffoldContext(
                    "Microsoft.EntityFrameworkCore.SqlServer",
                    "connectionstring",
                    "",
                    "",
                    dbContextClassName: contextName,
                    null!,
                    null!,
                    "FakeNamespace",
                    contextNamespace: null,
                    useDataAnnotations: false,
                    overwriteFiles: true,
                    useDatabaseNames: false,
                    suppressOnConfiguring: true,
                    noPluralize: false))
                .Message);
    }

    [ConditionalFact(typeof(SqlServerTestEnvironment), nameof(SqlServerTestEnvironment.SqlServerAvailable))]
    public void ScaffoldContext_sets_environment()
    {
        var operations = CreateOperations([]);
        operations.ScaffoldContext(
            "Microsoft.EntityFrameworkCore.SqlServer",
            SqlServerTestEnvironment.DefaultConnection,
            "",
            "",
            dbContextClassName: nameof(TestContext),
            schemas: ["Empty"],
            null!,
            null,
            contextNamespace: null,
            useDataAnnotations: false,
            overwriteFiles: true,
            useDatabaseNames: false,
            suppressOnConfiguring: true,
            noPluralize: false);

        Assert.Equal("Development", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        Assert.Equal("Development", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
    }

    [Theory]
    [InlineData("Models", "RootNamespace.Models")]
    [InlineData("Data.Models", "RootNamespace.Data.Models")]
    [InlineData("Data/My Models", "RootNamespace.Data.MyModels")]
    [InlineData("My Models", "RootNamespace.MyModels")]
    [InlineData("My-Models", "RootNamespace.MyModels")]
    [InlineData("2024", "RootNamespace._2024")]
    public void ScaffoldContext_generates_valid_namespace_from_output_dir(string outputDir, string expectedNamespace)
    {
        var code = ScaffoldContextCode(outputDir, outputContextDir: null);

        Assert.Contains($"namespace {expectedNamespace};", code);
    }

    [Fact]
    public void ScaffoldContext_generates_valid_namespaces_from_output_dir_and_context_dir()
    {
        var code = ScaffoldContextCode("My Models", "My Context");

        Assert.Contains("namespace RootNamespace.MyContext;", code);
        Assert.Contains("using RootNamespace.MyModels;", code);
    }

    private static string ScaffoldContextCode(string outputDir, string? outputContextDir)
    {
        using var directory = new TempDirectory();
        var assembly = MockAssembly.Create(typeof(TestContext), typeof(FakeDatabaseModelFactoryDesignTimeServices));
        var operations = new DatabaseOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            directory.Path,
            "RootNamespace",
            "C#",
            nullable: false,
            args: []);

        var files = operations.ScaffoldContext(
            "Microsoft.EntityFrameworkCore.Sqlite",
            "Data Source=Test.db",
            outputDir,
            outputContextDir,
            dbContextClassName: nameof(TestContext),
            schemas: [],
            tables: [],
            modelNamespace: null,
            contextNamespace: null,
            useDataAnnotations: false,
            overwriteFiles: true,
            useDatabaseNames: false,
            suppressOnConfiguring: true,
            noPluralize: false);

        return File.ReadAllText(files.ContextFile);
    }

    private static DatabaseOperations CreateOperations(string[]? args)
    {
        var assembly = MockAssembly.Create(typeof(TestContext));
        var operations = new DatabaseOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: args);
        return operations;
    }

    public class TestContext : DbContext;

    private class FakeDatabaseModelFactoryDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton<IDatabaseModelFactory, FakeDatabaseModelFactory>();
    }

    private class FakeDatabaseModelFactory : IDatabaseModelFactory
    {
        public DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
            => new();

        public DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
            => new();
    }
}

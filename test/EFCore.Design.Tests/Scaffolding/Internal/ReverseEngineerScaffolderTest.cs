// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class ReverseEngineerScaffolderTest
{
    [ConditionalFact]
    public void Save_works()
    {
        using var directory = new TempDirectory();
        var scaffolder = CreateScaffolder();
        var scaffoldedModel = new ScaffoldedModel
        {
            ContextFile = new ScaffoldedFile { Path = Path.Combine("..", "Data", "TestContext.cs"), Code = "// TestContext" },
            AdditionalFiles = { new ScaffoldedFile { Path = "TestEntity.cs", Code = "// TestEntity" } }
        };

        var result = scaffolder.Save(
            scaffoldedModel,
            Path.Combine(directory.Path, "Models"),
            overwriteFiles: false);

        var contextPath = Path.Combine(directory.Path, "Data", "TestContext.cs");
        Assert.Equal(contextPath, result.ContextFile);
        Assert.Equal("// TestContext", File.ReadAllText(contextPath));

        Assert.Equal(1, result.AdditionalFiles.Count);
        var entityTypePath = Path.Combine(directory.Path, "Models", "TestEntity.cs");
        Assert.Equal(entityTypePath, result.AdditionalFiles[0]);
        Assert.Equal("// TestEntity", File.ReadAllText(entityTypePath));
    }

    [ConditionalFact]
    public void Save_throws_when_existing_files()
    {
        using var directory = new TempDirectory();
        var contextPath = Path.Combine(directory.Path, "TestContext.cs");
        File.WriteAllText(contextPath, "// Old");

        var entityTypePath = Path.Combine(directory.Path, "TestEntity.cs");
        File.WriteAllText(entityTypePath, "// Old");

        var scaffolder = CreateScaffolder();
        var scaffoldedModel = new ScaffoldedModel
        {
            ContextFile = new ScaffoldedFile { Path = "TestContext.cs", Code = "// TestContext" },
            AdditionalFiles = { new ScaffoldedFile { Path = "TestEntity.cs", Code = "// TestEntity" } }
        };

        var ex = Assert.Throws<OperationException>(
            () => scaffolder.Save(scaffoldedModel, directory.Path, overwriteFiles: false));

        Assert.Equal(
            DesignStrings.ExistingFiles(
                directory.Path,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, "TestContext.cs", "TestEntity.cs")),
            ex.Message);
    }

    [ConditionalFact]
    public void Save_works_when_overwriteFiles()
    {
        using var directory = new TempDirectory();
        var path = Path.Combine(directory.Path, "Test.cs");
        File.WriteAllText(path, "// Old");

        var scaffolder = CreateScaffolder();
        var scaffoldedModel = new ScaffoldedModel { ContextFile = new ScaffoldedFile { Path = "Test.cs", Code = "// Test" } };

        var result = scaffolder.Save(scaffoldedModel, directory.Path, overwriteFiles: true);

        Assert.Equal(path, result.ContextFile);
        Assert.Equal("// Test", File.ReadAllText(path));
    }

    [ConditionalFact]
    public void Save_throws_when_readonly_files()
    {
        using var directory = new TempDirectory();
        var contextPath = Path.Combine(directory.Path, "TestContext.cs");
        File.WriteAllText(contextPath, "// Old");

        var entityTypePath = Path.Combine(directory.Path, "TestEntity.cs");
        File.WriteAllText(entityTypePath, "// Old");

        var originalAttributes = File.GetAttributes(contextPath);
        File.SetAttributes(contextPath, originalAttributes | FileAttributes.ReadOnly);
        File.SetAttributes(entityTypePath, originalAttributes | FileAttributes.ReadOnly);
        try
        {
            var scaffolder = CreateScaffolder();
            var scaffoldedModel = new ScaffoldedModel
            {
                ContextFile = new ScaffoldedFile { Path = "TestContext.cs", Code = "// TestContext" },
                AdditionalFiles = { new ScaffoldedFile { Path = "TestEntity.cs", Code = "// TestEntity" } }
            };

            var ex = Assert.Throws<OperationException>(
                () => scaffolder.Save(scaffoldedModel, directory.Path, overwriteFiles: true));

            Assert.Equal(
                DesignStrings.ReadOnlyFiles(
                    directory.Path,
                    string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, "TestContext.cs", "TestEntity.cs")),
                ex.Message);
        }
        finally
        {
            File.SetAttributes(contextPath, originalAttributes);
            File.SetAttributes(entityTypePath, originalAttributes);
        }
    }

    private static IReverseEngineerScaffolder CreateScaffolder()
        => new DesignTimeServicesBuilder(
                typeof(ReverseEngineerScaffolderTest).Assembly,
                typeof(ReverseEngineerScaffolderTest).Assembly,
                new TestOperationReporter(), [])
            .CreateServiceCollection("Microsoft.EntityFrameworkCore.SqlServer")
            .BuildServiceProvider(validateScopes: true)
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<IReverseEngineerScaffolder>();

    [ConditionalFact]
    public void ScaffoldModel_works_with_named_connection_string()
    {
        var resolver = new TestNamedConnectionStringResolver("Data Source=Test");
        var databaseModelFactory = new TestDatabaseModelFactory();
        var scaffolder = new DesignTimeServicesBuilder(
                typeof(ReverseEngineerScaffolderTest).Assembly,
                typeof(ReverseEngineerScaffolderTest).Assembly,
                new TestOperationReporter(), [])
            .CreateServiceCollection("Microsoft.EntityFrameworkCore.SqlServer")
            .AddSingleton<IDesignTimeConnectionStringResolver>(resolver)
            .AddScoped<IDatabaseModelFactory>(p => databaseModelFactory)
            .BuildServiceProvider(validateScopes: true)
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<IReverseEngineerScaffolder>();

        var result = scaffolder.ScaffoldModel(
            "Name=DefaultConnection",
            new DatabaseModelFactoryOptions(),
            new ModelReverseEngineerOptions(),
            new ModelCodeGenerationOptions { ModelNamespace = "Foo" });

        Assert.Equal("Data Source=Test", databaseModelFactory.ConnectionString);

        Assert.Contains("Name=DefaultConnection", result.ContextFile.Code);
        Assert.DoesNotContain("Data Source=Test", result.ContextFile.Code);
        Assert.DoesNotContain("#warning", result.ContextFile.Code);
    }

    [ConditionalFact]
    public void ScaffoldModel_works_with_overridden_connection_string()
    {
        var resolver = new TestNamedConnectionStringResolver("Data Source=Test");
        var databaseModelFactory = new TestDatabaseModelFactory();
        databaseModelFactory.ScaffoldedConnectionString = "Data Source=ScaffoldedConnectionString";
        var scaffolder = new DesignTimeServicesBuilder(
                typeof(ReverseEngineerScaffolderTest).Assembly,
                typeof(ReverseEngineerScaffolderTest).Assembly,
                new TestOperationReporter(), [])
            .CreateServiceCollection("Microsoft.EntityFrameworkCore.SqlServer")
            .AddSingleton<IDesignTimeConnectionStringResolver>(resolver)
            .AddScoped<IDatabaseModelFactory>(p => databaseModelFactory)
            .BuildServiceProvider(validateScopes: true)
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<IReverseEngineerScaffolder>();

        var result = scaffolder.ScaffoldModel(
            "Name=DefaultConnection",
            new DatabaseModelFactoryOptions(),
            new ModelReverseEngineerOptions(),
            new ModelCodeGenerationOptions { ModelNamespace = "Foo" });

        Assert.Contains("Data Source=ScaffoldedConnectionString", result.ContextFile.Code);
        Assert.DoesNotContain("Name=DefaultConnection", result.ContextFile.Code);
        Assert.DoesNotContain("Data Source=Test", result.ContextFile.Code);
        Assert.DoesNotContain(ScaffoldingAnnotationNames.ConnectionString, result.ContextFile.Code);
    }

    private class TestNamedConnectionStringResolver(string resolvedConnectionString) : IDesignTimeConnectionStringResolver
    {
        private readonly string _resolvedConnectionString = resolvedConnectionString;

        public string ResolveConnectionString(string connectionString)
            => _resolvedConnectionString;
    }

    private class TestDatabaseModelFactory : IDatabaseModelFactory
    {
        public string ConnectionString { get; set; }
        public string ScaffoldedConnectionString { get; set; }

        public DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
        {
            ConnectionString = connectionString;
            var databaseModel = new DatabaseModel();
            if (ScaffoldedConnectionString != null)
            {
                databaseModel[ScaffoldingAnnotationNames.ConnectionString] = ScaffoldedConnectionString;
            }

            return databaseModel;
        }

        public DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
            => throw new NotImplementedException();
    }
}

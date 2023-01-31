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
                new TestOperationReporter(),
                new string[0])
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
                new TestOperationReporter(),
                new string[0])
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
        var databaseModelFactory = new TestDatabaseModelFactory
        {
            ScaffoldedConnectionString = "Data Source=ScaffoldedConnectionString"
        };
        var scaffolder = new DesignTimeServicesBuilder(
                typeof(ReverseEngineerScaffolderTest).Assembly,
                typeof(ReverseEngineerScaffolderTest).Assembly,
                new TestOperationReporter(),
                new string[0])
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
    }

    [ConditionalFact]
    public void ScaffoldModel_decimal_numeric_types_have_precision_scale()
    {
        var resolver = new TestNamedConnectionStringResolver("Data Source=Test");
        var databaseModelFactory = new TestDatabaseModelFactory();
        var table = new DatabaseTable { Name = "DecimalNumericColumns" };
        table.Columns.Add(new DatabaseColumn { Table = table, Name = "Id", StoreType = "int" });
        table.Columns.Add(new DatabaseColumn { Table = table, Name = "DecimalColumn", StoreType = "decimal" });
        table.Columns.Add(new DatabaseColumn { Table = table, Name = "Decimal105Column", StoreType = "decimal(10, 5)" });
        table.Columns.Add(new DatabaseColumn { Table = table, Name = "DecimalDefaultColumn", StoreType = "decimal(18, 2)" });
        table.Columns.Add(new DatabaseColumn { Table = table, Name = "NumericColumn", StoreType = "numeric" });
        table.Columns.Add(new DatabaseColumn { Table = table, Name = "Numeric152Column", StoreType = "numeric(15, 2)" });
        table.Columns.Add(new DatabaseColumn { Table = table, Name = "NumericDefaultColumn", StoreType = "numeric(18, 2)" });
        table.Columns.Add(new DatabaseColumn { Table = table, Name = "NumericDefaultPrecisionColumn", StoreType = "numeric(38, 5)" });
        databaseModelFactory.DatabaseModel.Tables.Add(table);
        var scaffolder = new DesignTimeServicesBuilder(
                typeof(ReverseEngineerScaffolderTest).Assembly,
                typeof(ReverseEngineerScaffolderTest).Assembly,
                new TestOperationReporter(),
                new string[0])
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
            new ModelCodeGenerationOptions { ModelNamespace = "Foo", UseDataAnnotations = true });

        Assert.Equal(
"""
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Foo;

[Keyless]
public partial class DecimalNumericColumn
{
    public int Id { get; set; }

    [Precision(18, 0)]
    public decimal DecimalColumn { get; set; }

    [Precision(10, 5)]
    public decimal Decimal105Column { get; set; }

    public decimal DecimalDefaultColumn { get; set; }

    [Column(TypeName = "numeric")]
    [Precision(18, 0)]
    public decimal NumericColumn { get; set; }

    [Column(TypeName = "numeric")]
    [Precision(15, 2)]
    public decimal Numeric152Column { get; set; }

    [Column(TypeName = "numeric")]
    public decimal NumericDefaultColumn { get; set; }

    [Column(TypeName = "numeric")]
    [Precision(38, 5)]
    public decimal NumericDefaultPrecisionColumn { get; set; }
}
""",
                result.AdditionalFiles.Single(a => a.Path == "DecimalNumericColumn.cs").Code.TrimEnd(),
                ignoreLineEndingDifferences: true);
    }

    private class TestNamedConnectionStringResolver : IDesignTimeConnectionStringResolver
    {
        private readonly string _resolvedConnectionString;

        public TestNamedConnectionStringResolver(string resolvedConnectionString)
        {
            _resolvedConnectionString = resolvedConnectionString;
        }

        public string ResolveConnectionString(string connectionString)
            => _resolvedConnectionString;
    }

    private class TestDatabaseModelFactory : IDatabaseModelFactory
    {
        public string ConnectionString { get; set; }
        public DatabaseModel DatabaseModel { get; set; } = new DatabaseModel();
        public string ScaffoldedConnectionString { get; set; }

        public DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
        {
            ConnectionString = connectionString;
            if (ScaffoldedConnectionString != null)
            {
                DatabaseModel[ScaffoldingAnnotationNames.ConnectionString] = ScaffoldedConnectionString;
            }

            return DatabaseModel;
        }

        public DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
            => throw new NotImplementedException();
    }
}

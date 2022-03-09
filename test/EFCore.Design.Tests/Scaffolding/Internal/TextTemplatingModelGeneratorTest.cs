// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

[PlatformSkipCondition(TestPlatform.Linux, SkipReason = "CI time out")]
public class TextTemplatingModelGeneratorTest
{
    [ConditionalFact]
    public void HasTemplates_works_when_templates()
    {
        using var projectDir = new TempDirectory();

        var template = Path.Combine(projectDir, "Templates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(template));
        File.Create(template).Close();

        var generator = CreateGenerator();

        var result = generator.HasTemplates(projectDir);

        Assert.True(result);
    }

    [ConditionalFact]
    public void HasTemplates_works_when_no_templates()
    {
        using var projectDir = new TempDirectory();

        var generator = CreateGenerator();

        var result = generator.HasTemplates(projectDir);

        Assert.False(result);
    }

    [ConditionalFact]
    public void GenerateModel_uses_templates()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "Templates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            "My DbContext template");

        File.WriteAllText(
            Path.Combine(projectDir, "Templates", "EFCore", "EntityType.t4"),
            "My entity type template");

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new()
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ProjectDir = projectDir
            });

        Assert.Equal("Context.cs", result.ContextFile.Path);
        Assert.Equal("My DbContext template", result.ContextFile.Code);

        var entityType = Assert.Single(result.AdditionalFiles);
        Assert.Equal("Entity1.cs", entityType.Path);
        Assert.Equal("My entity type template", entityType.Code);
    }

    [ConditionalFact]
    public void GenerateModel_works_when_no_entity_type_template()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "Templates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            "My DbContext template");

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new()
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ProjectDir = projectDir
            });

        Assert.Equal("Context.cs", result.ContextFile.Path);
        Assert.Equal("My DbContext template", result.ContextFile.Code);

        Assert.Empty(result.AdditionalFiles);
    }

    [ConditionalFact]
    public void GenerateModel_sets_session_variables()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "Templates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            @"Model not null: <#= Session[""Model""] != null #>
Options not null: <#= Session[""Options""] != null #>
NamespaceHint: <#= Session[""NamespaceHint""] #>
ProjectDefaultNamespace: <#= Session[""ProjectDefaultNamespace""] #>");

        File.WriteAllText(
            Path.Combine(projectDir, "Templates", "EFCore", "EntityType.t4"),
            @"EntityType not null: <#= Session[""EntityType""] != null #>
Options not null: <#= Session[""Options""] != null #>
NamespaceHint: <#= Session[""NamespaceHint""] #>
ProjectDefaultNamespace: <#= Session[""ProjectDefaultNamespace""] #>");

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new()
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ContextNamespace = "ContextNamespace",
                ModelNamespace = "ModelNamespace",
                RootNamespace = "RootNamespace",
                ProjectDir = projectDir
            });

        Assert.Equal(
            @"Model not null: True
Options not null: True
NamespaceHint: ContextNamespace
ProjectDefaultNamespace: RootNamespace",
            result.ContextFile.Code);

        var entityType = Assert.Single(result.AdditionalFiles);
        Assert.Equal(
            @"EntityType not null: True
Options not null: True
NamespaceHint: ModelNamespace
ProjectDefaultNamespace: RootNamespace",
            entityType.Code);
    }

    [ConditionalFact]
    public void GenerateModel_defaults_to_model_namespace_when_no_context_namespace()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "Templates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            @"<#= Session[""NamespaceHint""] #>");

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new()
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ModelNamespace = "ModelNamespace",
                ProjectDir = projectDir
            });

        Assert.Equal(
            "ModelNamespace",
            result.ContextFile.Code);
    }

    [ConditionalFact]
    public void GenerateModel_uses_output_extension()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "Templates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            @"<#@ output extension="".vb"" #>");

        File.WriteAllText(
            Path.Combine(projectDir, "Templates", "EFCore", "EntityType.t4"),
            @"<#@ output extension="".fs"" #>
My entity type template");

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new()
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ProjectDir = projectDir
            });

        Assert.Equal("Context.vb", result.ContextFile.Path);

        var entityType = Assert.Single(result.AdditionalFiles);
        Assert.Equal("Entity1.fs", entityType.Path);
    }

    [ConditionalFact]
    public void GenerateModel_warns_when_output_encoding()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "Templates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            @"<#@ output encoding=""us-ascii"" #>");

        var reporter = new TestOperationReporter();
        var generator = CreateGenerator(reporter);
        var model = new ModelBuilder()
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new()
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ProjectDir = projectDir
            });

        Assert.Collection(
            reporter.Messages,
            x =>
            {
                Assert.Equal(LogLevel.Warning, x.Level);
                Assert.Equal(DesignStrings.EncodingIgnored("us-ascii"), x.Message);
            });
    }

    [ConditionalFact]
    public void GenerateModel_reports_errors()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "Templates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            @"<# Warning(""This is a warning"");
Error(""This is an error""); #>");

        var reporter = new TestOperationReporter();
        var generator = CreateGenerator(reporter);
        var model = new ModelBuilder()
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new()
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ProjectDir = projectDir
            });

        Assert.Collection(
            reporter.Messages,
            x =>
            {
                Assert.Equal(LogLevel.Warning, x.Level);
                Assert.Contains("This is a warning", x.Message);
            },
            x =>
            {
                Assert.Equal(LogLevel.Error, x.Level);
                Assert.Contains("This is an error", x.Message);
            });
    }

    private static TemplatedModelGenerator CreateGenerator(IOperationReporter reporter = null)
    {
        var serviceCollection = new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices(reporter);
        new SqlServerDesignTimeServices().ConfigureDesignTimeServices(serviceCollection);

        return serviceCollection
            .BuildServiceProvider()
            .GetServices<IModelCodeGenerator>()
            .OfType<TemplatedModelGenerator>()
            .Last();
    }
}

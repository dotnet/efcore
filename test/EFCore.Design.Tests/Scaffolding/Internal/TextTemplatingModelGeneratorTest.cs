// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

[PlatformSkipCondition(TestUtilities.Xunit.TestPlatform.Linux | TestUtilities.Xunit.TestPlatform.Mac, SkipReason = "CI time out")]
public class TextTemplatingModelGeneratorTest
{
    [ConditionalFact]
    public void HasTemplates_works_when_templates()
    {
        using var projectDir = new TempDirectory();

        var template = Path.Combine(projectDir, "CodeTemplates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(template));
        File.Create(template).Close();

        var generator = CreateGenerator();

        var result = generator.HasTemplates(projectDir);

        Assert.True(result);
    }

    [ConditionalFact]
    public void HasTemplates_throws_when_configuration_but_no_context()
    {
        using var projectDir = new TempDirectory();

        var template = Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityTypeConfiguration.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(template));
        File.Create(template).Close();

        var generator = CreateGenerator();

        var ex = Assert.Throws<OperationException>(() => generator.HasTemplates(projectDir));

        Assert.Equal(DesignStrings.NoContextTemplateButConfiguration, ex.Message);
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

        var contextTemplate = Path.Combine(projectDir, "CodeTemplates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            "My DbContext template");

        File.WriteAllText(
            Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityType.t4"),
            "My entity type template");

        File.WriteAllText(
            Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityTypeConfiguration.t4"),
            "My entity type configuration template");

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new ModelCodeGenerationOptions
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ProjectDir = projectDir
            });

        Assert.Equal("Context.cs", result.ContextFile.Path);
        Assert.Equal("My DbContext template", result.ContextFile.Code);

        Assert.Equal(2, result.AdditionalFiles.Count);

        var entityType = Assert.Single(result.AdditionalFiles, f => f.Path == "Entity1.cs");
        Assert.Equal("My entity type template", entityType.Code);

        var entityTypeConfiguration = Assert.Single(result.AdditionalFiles, f => f.Path == "Entity1Configuration.cs");
        Assert.Equal("My entity type configuration template", entityTypeConfiguration.Code);
    }

    [ConditionalFact]
    public void GenerateModel_works_when_no_entity_type_template()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "CodeTemplates", "EFCore", "DbContext.t4");
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
            new ModelCodeGenerationOptions
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
    public void GenerateModel_works_when_no_context_template_and_csharp()
    {
        using var projectDir = new TempDirectory();

        var template = Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityType.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(template));
        File.WriteAllText(
            template,
            "My entity type template");

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new ModelCodeGenerationOptions
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ProjectDir = projectDir,
                Language = "C#"
            });

        Assert.NotEmpty(result.ContextFile.Code);

        var entityType = Assert.Single(result.AdditionalFiles);
        Assert.Equal("My entity type template", entityType.Code);
    }

    [ConditionalFact]
    public void GenerateModel_throws_when_no_context_template_and_not_csharp()
    {
        using var projectDir = new TempDirectory();

        var template = Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityType.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(template));
        File.Create(template).Close();

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .FinalizeModel();

        var ex = Assert.Throws<OperationException>(
            () => generator.GenerateModel(
                model,
                new ModelCodeGenerationOptions
                {
                    ContextName = "Context",
                    ConnectionString = @"Name=DefaultConnection",
                    ProjectDir = projectDir,
                    Language = "VB"
                }));

        Assert.Equal(DesignStrings.NoContextTemplate, ex.Message);
    }

    [ConditionalFact]
    public void GenerateModel_sets_session_variables()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "CodeTemplates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            """
Model not null: <#= Session["Model"] != null #>
Options not null: <#= Session["Options"] != null #>
NamespaceHint: <#= Session["NamespaceHint"] #>
ProjectDefaultNamespace: <#= Session["ProjectDefaultNamespace"] #>
""");

        File.WriteAllText(
            Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityType.t4"),
            """
EntityType not null: <#= Session["EntityType"] != null #>
Options not null: <#= Session["Options"] != null #>
NamespaceHint: <#= Session["NamespaceHint"] #>
ProjectDefaultNamespace: <#= Session["ProjectDefaultNamespace"] #>
""");

        File.WriteAllText(
            Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityTypeConfiguration.t4"),
            """
EntityType not null: <#= Session["EntityType"] != null #>
Options not null: <#= Session["Options"] != null #>
NamespaceHint: <#= Session["NamespaceHint"] #>
ProjectDefaultNamespace: <#= Session["ProjectDefaultNamespace"] #>
""");

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new ModelCodeGenerationOptions
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ContextNamespace = "ContextNamespace",
                ModelNamespace = "ModelNamespace",
                RootNamespace = "RootNamespace",
                ProjectDir = projectDir
            });

        Assert.Equal(
            """
Model not null: True
Options not null: True
NamespaceHint: ContextNamespace
ProjectDefaultNamespace: RootNamespace
""",
            result.ContextFile.Code);

        Assert.Equal(2, result.AdditionalFiles.Count);

        var entityType = Assert.Single(result.AdditionalFiles, f => f.Path == "Entity1.cs");
        Assert.Equal(
            """
EntityType not null: True
Options not null: True
NamespaceHint: ModelNamespace
ProjectDefaultNamespace: RootNamespace
""",
            entityType.Code);

        var entityTypeConfiguration = Assert.Single(result.AdditionalFiles, f => f.Path == "Entity1Configuration.cs");
        Assert.Equal(
            """
EntityType not null: True
Options not null: True
NamespaceHint: ContextNamespace
ProjectDefaultNamespace: RootNamespace
""",
            entityTypeConfiguration.Code);
    }

    [ConditionalFact]
    public void GenerateModel_defaults_to_model_namespace_when_no_context_namespace()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "CodeTemplates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            @"<#= Session[""NamespaceHint""] #>");

        File.WriteAllText(
            Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityTypeConfiguration.t4"),
            @"<#= Session[""NamespaceHint""] #>");

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new ModelCodeGenerationOptions
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ModelNamespace = "ModelNamespace",
                ProjectDir = projectDir
            });

        Assert.Equal(
            "ModelNamespace",
            result.ContextFile.Code);

        var entityTypeConfiguration = Assert.Single(result.AdditionalFiles);
        Assert.Equal(
            "ModelNamespace",
            entityTypeConfiguration.Code);
    }

    [ConditionalFact]
    public void GenerateModel_uses_output_extension()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "CodeTemplates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            @"<#@ output extension="".vb"" #>");

        File.WriteAllText(
            Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityType.t4"),
            """
<#@ output extension=".fs" #>
My entity type template
""");

        File.WriteAllText(
            Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityTypeConfiguration.t4"),
            """
<#@ output extension=".py" #>
My entity type configuration template
""");

        var generator = CreateGenerator();
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .Entity("Entity2", b => { })
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new ModelCodeGenerationOptions
            {
                ContextName = "Context",
                ConnectionString = @"Name=DefaultConnection",
                ProjectDir = projectDir
            });

        Assert.Equal("Context.vb", result.ContextFile.Path);

        Assert.Equal(4, result.AdditionalFiles.Count);
        Assert.Single(result.AdditionalFiles, f => f.Path == "Entity1.fs");
        Assert.Single(result.AdditionalFiles, f => f.Path == "Entity2.fs");
        Assert.Single(result.AdditionalFiles, f => f.Path == "Entity1Configuration.py");
        Assert.Single(result.AdditionalFiles, f => f.Path == "Entity2Configuration.py");
    }

    [ConditionalFact]
    public void GenerateModel_warns_when_output_encoding()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "CodeTemplates", "EFCore", "DbContext.t4");
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
            new ModelCodeGenerationOptions
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

        var contextTemplate = Path.Combine(projectDir, "CodeTemplates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            @"<# Error(""This is an error""); #>");

        var reporter = new TestOperationReporter();
        var generator = CreateGenerator(reporter);
        var model = new ModelBuilder()
            .FinalizeModel();

        var ex = Assert.Throws<OperationException>(
            () => generator.GenerateModel(
                model,
                new ModelCodeGenerationOptions
                {
                    ContextName = "Context",
                    ConnectionString = @"Name=DefaultConnection",
                    ProjectDir = projectDir
                }));

        Assert.Equal(DesignStrings.ErrorGeneratingOutput(contextTemplate), ex.Message);

        Assert.Collection(
            reporter.Messages,
            x =>
            {
                Assert.Equal(LogLevel.Error, x.Level);
                Assert.Contains("This is an error", x.Message);
            });
    }

    [ConditionalFact]
    public void GenerateModel_reports_warnings()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "CodeTemplates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            @"<# Warning(""Warning about DbContext""); #>");
        var entityTypeTemplate = Path.Combine(projectDir, "CodeTemplates", "EFCore", "EntityType.t4");
        File.WriteAllText(
            entityTypeTemplate,
            """
<#@ assembly name="Microsoft.EntityFrameworkCore" #>
<#@ parameter name="EntityType" type="Microsoft.EntityFrameworkCore.Metadata.IEntityType" #>
<# Warning("Warning about " + EntityType.Name); #>
""");

        var reporter = new TestOperationReporter();
        var generator = CreateGenerator(reporter);
        var model = new ModelBuilder()
            .Entity("Entity1", b => { })
            .Entity("Entity2", b => { })
            .FinalizeModel();

        var result = generator.GenerateModel(
            model,
            new ModelCodeGenerationOptions
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
                Assert.Contains("Warning about DbContext", x.Message);
            },
            x =>
            {
                Assert.Equal(LogLevel.Warning, x.Level);
                Assert.Contains("Warning about Entity1", x.Message);
            },
            x =>
            {
                Assert.Equal(LogLevel.Warning, x.Level);
                Assert.Contains("Warning about Entity2", x.Message);
            });
    }

    [ConditionalFact]
    public void GenerateModel_reports_compiler_errors()
    {
        using var projectDir = new TempDirectory();

        var contextTemplate = Path.Combine(projectDir, "CodeTemplates", "EFCore", "DbContext.t4");
        Directory.CreateDirectory(Path.GetDirectoryName(contextTemplate));
        File.WriteAllText(
            contextTemplate,
            "<# #error This is a compiler error #>");

        var reporter = new TestOperationReporter();
        var generator = CreateGenerator(reporter);
        var model = new ModelBuilder()
            .FinalizeModel();

        var ex = Assert.Throws<OperationException>(
            () => generator.GenerateModel(
                model,
                new ModelCodeGenerationOptions
                {
                    ContextName = "Context",
                    ConnectionString = @"Name=DefaultConnection",
                    ProjectDir = projectDir
                }));

        Assert.Equal(DesignStrings.ErrorGeneratingOutput(contextTemplate), ex.Message);

        Assert.Collection(
            reporter.Messages,
            x =>
            {
                Assert.Equal(LogLevel.Error, x.Level);
                Assert.Contains("DbContext.t4(1,9) : error CS1029: #error: 'This is a compiler error '", x.Message);
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
            .OfType<TextTemplatingModelGenerator>()
            .Last();
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Design.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class CSharpModelGeneratorTest
{
    [Fact]
    public void Language_works()
    {
        var generator = CreateGenerator();

        var result = generator.Language;

        Assert.Equal("C#", result);
    }

    [Fact]
    public void WriteCode_works()
    {
        var generator = CreateGenerator();
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity("TestEntity").Property<int>("Id");

        var result = generator.GenerateModel(
            modelBuilder.FinalizeModel(designTime: true),
            new ModelCodeGenerationOptions
            {
                ModelNamespace = "TestNamespace",
                ContextNamespace = "ContextNameSpace",
                ContextDir = Path.Combine("..", "TestContextDir" + Path.DirectorySeparatorChar),
                ContextName = "TestContext",
                ConnectionString = "Data Source=Test"
            });

        Assert.Equal(Path.Combine("..", "TestContextDir", "TestContext.cs"), result.ContextFile.Path);
        Assert.NotEmpty(result.ContextFile.Code);

        Assert.Equal(1, result.AdditionalFiles.Count);
        Assert.Equal("TestEntity.cs", result.AdditionalFiles[0].Path);
        Assert.NotEmpty(result.AdditionalFiles[0].Code);
    }

    [Theory]
    [InlineData(false, typeof(CSharpModelGenerator))]
    [InlineData(true, typeof(CSharpModelGenerator))]
    [InlineData(false, typeof(TextTemplatingModelGenerator))]
    [InlineData(true, typeof(TextTemplatingModelGenerator))]
    public void Generated_code_uses_only_LF_line_endings(bool useDataAnnotations, Type generatorType)
    {
        var generator = CreateGenerator(generatorType);
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity(
            "TestEntity", b =>
            {
                b.ToTable(t => t.HasComment("Entity comment\r\nover several\nlines"));
                b.Property<int>("Id");
                b.Property<string>("Name").HasMaxLength(10).IsRequired().HasComment("Property comment\r\nover several\nlines");
                b.Property<string>("Other").HasColumnName("other_name").HasDefaultValueSql("1 +\r\n2");
                b.HasIndex("Name", "Other").IsUnique().HasFilter("[Name] IS NOT NULL");
            });
        modelBuilder.Entity("Related", b => b.Property<int>("Id"));
        modelBuilder.Entity("TestEntity").HasMany("Related", "Relateds").WithMany("TestEntities");
        modelBuilder.HasSequence<int>("TestSequence").StartsAt(5).IsCyclic();

        var result = generator.GenerateModel(
            modelBuilder.FinalizeModel(designTime: true),
            new ModelCodeGenerationOptions
            {
                ModelNamespace = "TestNamespace",
                ContextName = "TestContext",
                ConnectionString = "Data Source=Test",
                Language = "C#",
                ProjectDir = Path.Combine(Path.GetTempPath(), "NoCodeTemplates"), // no custom templates here
                UseDataAnnotations = useDataAnnotations
            });

        var files = new[] { result.ContextFile }.Concat(result.AdditionalFiles).ToList();
        if (generatorType == typeof(CSharpModelGenerator))
        {
            // TextTemplatingModelGenerator only scaffolds the DbContext when there are no custom entity type templates
            Assert.True(files.Count > 2);
            Assert.Contains(files, f => f.Code.Contains("/// Entity comment"));
            Assert.Contains(files, f => f.Code.Contains("/// Property comment"));
        }

        Assert.Contains(files, f => Regex.IsMatch(f.Code, @"\r?\n\s+\.\w+\("));
        Assert.Contains(files, f => f.Code.Contains("UsingEntity"));
        Assert.Contains(files, f => f.Code.Contains("HasSequence"));
        Assert.Contains(files, f => f.Code.Contains("#warning"));

        foreach (var file in files)
        {
            Assert.False(file.Code.Contains('\r'), $"{file.Path} contains a carriage return.");
        }
    }

    private static IModelCodeGenerator CreateGenerator(Type? generatorType = null)
    {
        generatorType ??= typeof(CSharpModelGenerator);
        var testAssembly = typeof(CSharpModelGeneratorTest).Assembly;
        var reporter = new TestOperationReporter();
        return new DesignTimeServicesBuilder(testAssembly, testAssembly, reporter, [])
            .CreateServiceCollection("Microsoft.EntityFrameworkCore.SqlServer")
            .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
            .AddSingleton<IProviderConfigurationCodeGenerator, TestProviderCodeGenerator>()
            .BuildServiceProvider(validateScopes: true)
            .GetServices<IModelCodeGenerator>()
            .Last(g => g.GetType() == generatorType);
    }
}

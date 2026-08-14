// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class CSharpModelGeneratorTest
{
    [ConditionalFact]
    public void Language_works()
    {
        var generator = CreateGenerator();

        var result = generator.Language;

        Assert.Equal("C#", result);
    }

    [ConditionalFact]
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

    private static IModelCodeGenerator CreateGenerator()
    {
        var testAssembly = typeof(CSharpModelGeneratorTest).Assembly;
        var reporter = new TestOperationReporter();
        return new DesignTimeServicesBuilder(testAssembly, testAssembly, reporter, [])
            .CreateServiceCollection("Microsoft.EntityFrameworkCore.SqlServer")
            .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
            .AddSingleton<IProviderConfigurationCodeGenerator, TestProviderCodeGenerator>()
            .BuildServiceProvider(validateScopes: true)
            .GetServices<IModelCodeGenerator>()
            .Last(g => g is CSharpModelGenerator);
    }
}

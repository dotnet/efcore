// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

[Collection(nameof(ModelCodeGeneratorTestCollection))]
public abstract class ModelCodeGeneratorTestBase
{
    private readonly ModelCodeGeneratorTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    protected ModelCodeGeneratorTestBase(ModelCodeGeneratorTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    protected async Task TestAsync(
        Action<ModelBuilder> buildModel,
        ModelCodeGenerationOptions options,
        Action<ScaffoldedModel> assertScaffold,
        Action<IModel> assertModel,
        bool skipBuild = false)
    {
        var designServices = new ServiceCollection();
        AddModelServices(designServices);

        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder(customServices: designServices);
        buildModel(modelBuilder);

        var model = modelBuilder.FinalizeModel(designTime: true, skipValidation: true);

        var services = CreateServices();
        AddScaffoldingServices(services);

        var generators = services.BuildServiceProvider(validateScopes: true)
            .GetServices<IModelCodeGenerator>();
        var generator = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || Random.Shared.Next() % 12 != 0
            ? generators.Last(g => g is CSharpModelGenerator)
            : generators.Last(g => g is TextTemplatingModelGenerator);

        options.ModelNamespace ??= "TestNamespace";
        options.ContextName = "TestDbContext";
        options.ConnectionString = "Initial Catalog=TestDatabase";
        options.ProjectDir = _fixture.ProjectDir;

        var scaffoldedModel = generator.GenerateModel(
            model,
            options);
        assertScaffold(scaffoldedModel);

        var build = new BuildSource
        {
            References =
            {
                BuildReference.ByName("Microsoft.EntityFrameworkCore.Abstractions"),
                BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
                BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer")
            },
            Sources = new[] { scaffoldedModel.ContextFile }.Concat(scaffoldedModel.AdditionalFiles)
                .ToDictionary(f => f.Path, f => f.Code),
            NullableReferenceTypes = options.UseNullableReferenceTypes
        };

        if (!skipBuild)
        {
            var assembly = await build.BuildInMemoryWithWithAnalyzersAsync();

            if (assertModel != null)
            {
                var contextNamespace = options.ContextNamespace ?? options.ModelNamespace;
                var context = (DbContext)assembly.CreateInstance(
                    !string.IsNullOrEmpty(contextNamespace)
                        ? contextNamespace + "." + options.ContextName
                        : options.ContextName);

                var compiledModel = context.GetService<IDesignTimeModel>().Model;
                assertModel(compiledModel);
            }
        }
    }

    protected IServiceCollection CreateServices()
    {
        var testAssembly = MockAssembly.Create();
        var reporter = new TestOperationReporter(_output);
        var services = new DesignTimeServicesBuilder(testAssembly, testAssembly, reporter, new string[0])
            .CreateServiceCollection("Microsoft.EntityFrameworkCore.SqlServer");
        return services;
    }

    protected virtual void AddModelServices(IServiceCollection services)
    {
    }

    protected virtual void AddScaffoldingServices(IServiceCollection services)
    {
    }

    protected static void AssertFileContents(
        string expectedCode,
        ScaffoldedFile file)
        => Assert.Equal(expectedCode, file.Code, ignoreLineEndingDifferences: true);
}

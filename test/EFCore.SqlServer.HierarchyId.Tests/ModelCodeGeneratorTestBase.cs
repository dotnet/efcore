// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer;

public abstract class ModelCodeGeneratorTestBase
{
    protected void Test(
        Action<ModelBuilder> buildModel,
        ModelCodeGenerationOptions options,
        Action<ScaffoldedModel> assertScaffold)
    {
        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder(addServices: AddModelServices);
        modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
        buildModel(modelBuilder);

        var model = modelBuilder.FinalizeModel(designTime: true, skipValidation: true);

        var services = AddScaffoldingServices(CreateServices());
        var generator = services.BuildServiceProvider(validateScopes: true)
            .GetRequiredService<IModelCodeGenerator>();

        options.ModelNamespace ??= "TestNamespace";
        options.ContextName = "TestDbContext";
        options.ConnectionString = "Initial Catalog=TestDatabase";

        var scaffoldedModel = generator.GenerateModel(
            model,
            options);
        assertScaffold(scaffoldedModel);
    }

    private static IServiceCollection CreateServices()
    {
        var testAssembly = typeof(ModelCodeGeneratorTestBase).Assembly;
        var reporter = new TestOperationReporter();
        var services = new DesignTimeServicesBuilder(testAssembly, testAssembly, reporter, [])
            .CreateServiceCollection("Microsoft.EntityFrameworkCore.SqlServer");
        return services;
    }

    protected virtual IServiceCollection AddModelServices(IServiceCollection services)
        => services;

    protected virtual IServiceCollection AddScaffoldingServices(IServiceCollection services)
        => services;

    protected static void AssertFileContents(
        string expectedCode,
        ScaffoldedFile file)
        => Assert.Equal(expectedCode, file.Code, ignoreLineEndingDifferences: true);
}

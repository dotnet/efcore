// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class ReverseEngineeringConfigurationTests
{
    [ConditionalFact]
    public void Throws_exceptions_for_invalid_context_name()
    {
        ValidateContextNameInReverseEngineerGenerator("Invalid!CSharp*Class&Name");
        ValidateContextNameInReverseEngineerGenerator("1CSharpClassNameCannotStartWithNumber");
        ValidateContextNameInReverseEngineerGenerator("volatile");
    }

    private void ValidateContextNameInReverseEngineerGenerator(string contextName)
    {
        var assembly = typeof(ReverseEngineeringConfigurationTests).Assembly;
        var reverseEngineer = new DesignTimeServicesBuilder(assembly, assembly, new TestOperationReporter(), [])
            .Build("Microsoft.EntityFrameworkCore.SqlServer")
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<IReverseEngineerScaffolder>();

        Assert.Equal(
            DesignStrings.ContextClassNotValidCSharpIdentifier(contextName),
            Assert.Throws<ArgumentException>(
                    () => reverseEngineer.ScaffoldModel(
                        "connectionstring",
                        new DatabaseModelFactoryOptions(),
                        new ModelReverseEngineerOptions(),
                        new ModelCodeGenerationOptions { ModelNamespace = "FakeNamespace", ContextName = contextName }))
                .Message);
    }
}

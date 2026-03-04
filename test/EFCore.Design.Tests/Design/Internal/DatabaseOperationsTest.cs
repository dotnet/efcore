// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

public class DatabaseOperationsTest
{
    [ConditionalFact]
    public void Can_pass_null_args()
    {
        // Even though newer versions of the tools will pass an empty array
        // older versions of the tools can pass null args.
        CreateOperations(null);
    }

    [ConditionalFact]
    public void ScaffoldContext_throws_exceptions_for_invalid_context_name()
    {
        ValidateContextNameInReverseEngineerGenerator("Invalid!CSharp*Class&Name");
        ValidateContextNameInReverseEngineerGenerator("1CSharpClassNameCannotStartWithNumber");
        ValidateContextNameInReverseEngineerGenerator("volatile");
    }

    private void ValidateContextNameInReverseEngineerGenerator(string contextName)
    {
        var operations = CreateOperations([]);

        Assert.Equal(
            DesignStrings.ContextClassNotValidCSharpIdentifier(contextName),
            Assert.Throws<ArgumentException>(
                    () => operations.ScaffoldContext(
                        "Microsoft.EntityFrameworkCore.SqlServer",
                        "connectionstring",
                        "",
                        "",
                        dbContextClassName: contextName,
                        null,
                        null,
                        "FakeNamespace",
                        contextNamespace: null,
                        useDataAnnotations: false,
                        overwriteFiles: true,
                        useDatabaseNames: false,
                        suppressOnConfiguring: true,
                        noPluralize: false))
                .Message);
    }

    [ConditionalFact]
    [SqlServerConfiguredCondition]
    public void ScaffoldContext_sets_environment()
    {
        var operations = CreateOperations([]);
        operations.ScaffoldContext(
            "Microsoft.EntityFrameworkCore.SqlServer",
            TestEnvironment.DefaultConnection,
            "",
            "",
            dbContextClassName: nameof(TestContext),
            schemas: ["Empty"],
            null,
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

    private static DatabaseOperations CreateOperations(string[] args)
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
}

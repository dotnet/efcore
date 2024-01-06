// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design.Internal;

public class DatabaseOperationsTest
{
    [ConditionalFact]
    public void Can_pass_null_args()
    {
        // Even though newer versions of the tools will pass an empty array
        // older versions of the tools can pass null args.
        var assembly = MockAssembly.Create(typeof(TestContext));
        _ = new TestDatabaseOperations(
            new TestOperationReporter(),
            assembly,
            assembly,
            "projectDir",
            "RootNamespace",
            "C#",
            nullable: false,
            args: null);
    }

    private class TestContext : DbContext;
}

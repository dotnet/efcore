// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class MigrationsOperationsTest
    {
        [ConditionalFact]
        public void Can_pass_null_args()
        {
            // Even though newer versions of the tools will pass an empty array
            // older versions of the tools can pass null args.
            var assembly = MockAssembly.Create(typeof(TestContext));
            _ = new TestMigrationsOperations(
                new TestOperationReporter(),
                assembly,
                assembly,
                "projectDir",
                "RootNamespace",
                "C#",
                args: null);
        }

        private class TestContext : DbContext
        {
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.ReverseEngineering;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class ReverseEngineeringConfigurationTests
    {
        [Fact]
        public void Throws_exceptions_for_invalid_context_name()
        {
            ValidateContextNameInReverseEngineerGenerator("Invalid!CSharp*Class&Name");
            ValidateContextNameInReverseEngineerGenerator("1CSharpClassNameCannotStartWithNumber");
            ValidateContextNameInReverseEngineerGenerator("volatile");
        }

        private void ValidateContextNameInReverseEngineerGenerator(string contextName)
        {
            var reverseEngineer = new DbContextScaffolder(
                new FakeScaffoldingModelFactory(new FakeDiagnosticsLogger<DbLoggerCategory.Scaffolding>()),
                new CSharpScaffoldingGenerator(
                    new InMemoryFileService(),
                    new CSharpDbContextGenerator(new FakeScaffoldingHelper(), CSharpUtilities.Instance),
                    new CSharpEntityTypeGenerator(CSharpUtilities.Instance)),
                CSharpUtilities.Instance);

            Assert.Equal(
                DesignStrings.ContextClassNotValidCSharpIdentifier(contextName),
                Assert.Throws<ArgumentException>(
                        () => reverseEngineer.GenerateAsync(
                                connectionString: "connectionstring",
                                tableSelectionSet: TableSelectionSet.All,
                                projectPath: "FakeProjectPath",
                                outputPath: null,
                                rootNamespace: "FakeNamespace",
                                contextName: contextName,
                                useDataAnnotations: false,
                                overwriteFiles: false)
                            .Result)
                    .Message);
        }

        public class FakeScaffoldingHelper : IScaffoldingHelper
        {
            public string GetProviderOptionsBuilder(string connectionString)
            {
                throw new NotImplementedException();
            }
        }
    }
}

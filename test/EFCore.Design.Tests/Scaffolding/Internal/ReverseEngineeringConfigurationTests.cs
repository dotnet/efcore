// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ReverseEngineering;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
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
            var cSharpUtilities = new CSharpUtilities();
            var reverseEngineer = new ModelScaffolder(
                new FakeScaffoldingModelFactory(new FakeDiagnosticsLogger<DbLoggerCategory.Scaffolding>()),
                new CSharpScaffoldingGenerator(
                    new InMemoryFileService(),
                    new CSharpDbContextGenerator(new FakeScaffoldingCodeGenerator(), new FakeAnnotationCodeGenerator(), cSharpUtilities),
                    new CSharpEntityTypeGenerator(cSharpUtilities)),
                cSharpUtilities);

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

        public class FakeScaffoldingCodeGenerator : IScaffoldingProviderCodeGenerator
        {
            public string GenerateUseProvider(string connectionString)
            {
                throw new NotImplementedException();
            }

            public TypeScaffoldingInfo GetTypeScaffoldingInfo(ColumnModel columnModel)
            {
                throw new NotImplementedException();
            }
        }

        public class FakeAnnotationCodeGenerator : IAnnotationCodeGenerator
        {
            public string GenerateFluentApi(IModel model, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public string GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public string GenerateFluentApi(IKey key, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public string GenerateFluentApi(IProperty property, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public string GenerateFluentApi(IForeignKey foreignKey, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public string GenerateFluentApi(IIndex index, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public bool IsHandledByConvention(IModel model, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public bool IsHandledByConvention(IEntityType entityType, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public bool IsHandledByConvention(IKey key, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public bool IsHandledByConvention(IProperty property, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public bool IsHandledByConvention(IForeignKey foreignKey, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            public bool IsHandledByConvention(IIndex index, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }
        }
    }
}

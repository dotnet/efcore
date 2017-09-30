// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
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
            var reverseEngineer = new ReverseEngineerScaffolder(
                new FakeDatabaseModelFactory(),
                new FakeScaffoldingModelFactory(new TestOperationReporter()),
                new CSharpScaffoldingGenerator(
                    new InMemoryFileService(),
                    new CSharpDbContextGenerator(new FakeScaffoldingCodeGenerator(), new FakeAnnotationCodeGenerator(), cSharpUtilities),
                    new CSharpEntityTypeGenerator(cSharpUtilities)),
                cSharpUtilities);

            Assert.Equal(
                DesignStrings.ContextClassNotValidCSharpIdentifier(contextName),
                Assert.Throws<ArgumentException>(
                        () => reverseEngineer.Generate(
                            connectionString: "connectionstring",
                            tables: Enumerable.Empty<string>(),
                            schemas: Enumerable.Empty<string>(),
                            projectPath: "FakeProjectPath",
                            outputPath: null,
                            rootNamespace: "FakeNamespace",
                            contextName: contextName,
                            useDataAnnotations: false,
                            overwriteFiles: false,
                            useDatabaseNames: false))
                    .Message);
        }

        public class FakeScaffoldingCodeGenerator : IScaffoldingProviderCodeGenerator
        {
            public string GenerateUseProvider(string connectionString, string language)
            {
                throw new NotImplementedException();
            }

            public TypeScaffoldingInfo GetTypeScaffoldingInfo(DatabaseColumn column)
            {
                throw new NotImplementedException();
            }
        }

        public class FakeAnnotationCodeGenerator : IAnnotationCodeGenerator
        {
            public string GenerateFluentApi(IModel model, IAnnotation annotation, string language)
            {
                throw new NotImplementedException();
            }

            public string GenerateFluentApi(IEntityType entityType, IAnnotation annotation, string language)
            {
                throw new NotImplementedException();
            }

            public string GenerateFluentApi(IKey key, IAnnotation annotation, string language)
            {
                throw new NotImplementedException();
            }

            public string GenerateFluentApi(IProperty property, IAnnotation annotation, string language)
            {
                throw new NotImplementedException();
            }

            public string GenerateFluentApi(IForeignKey foreignKey, IAnnotation annotation, string language)
            {
                throw new NotImplementedException();
            }

            public string GenerateFluentApi(IIndex index, IAnnotation annotation, string language)
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

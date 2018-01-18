// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
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
            var reverseEngineer = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<IAnnotationCodeGenerator, FakeAnnotationCodeGenerator>()
                .AddSingleton<IDatabaseModelFactory, FakeDatabaseModelFactory>()
                .AddSingleton<IProviderCodeGenerator, TestProviderCodeGenerator>()
                .AddSingleton<IScaffoldingModelFactory, FakeScaffoldingModelFactory>()
                .BuildServiceProvider()
                .GetRequiredService<IReverseEngineerScaffolder>();

            Assert.Equal(
                DesignStrings.ContextClassNotValidCSharpIdentifier(contextName),
                Assert.Throws<ArgumentException>(
                        () => reverseEngineer.ScaffoldModel(
                            connectionString: "connectionstring",
                            tables: Enumerable.Empty<string>(),
                            schemas: Enumerable.Empty<string>(),
                            @namespace: "FakeNamespace",
                            language: "",
                            outputDbContextDir: null,
                            contextName: contextName,
                            useDataAnnotations: false,
                            useDatabaseNames: false))
                    .Message);
        }

        public class FakeAnnotationCodeGenerator : IAnnotationCodeGenerator
        {
            public MethodCallCodeFragment GenerateFluentApi(IModel model, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            string IAnnotationCodeGenerator.GenerateFluentApi(IModel model, IAnnotation annotation, string language)
            {
                throw new NotImplementedException();
            }

            public MethodCallCodeFragment GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            string IAnnotationCodeGenerator.GenerateFluentApi(IEntityType entityType, IAnnotation annotation, string language)
            {
                throw new NotImplementedException();
            }

            public MethodCallCodeFragment GenerateFluentApi(IKey key, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            string IAnnotationCodeGenerator.GenerateFluentApi(IKey key, IAnnotation annotation, string language)
            {
                throw new NotImplementedException();
            }

            public MethodCallCodeFragment GenerateFluentApi(IProperty property, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            string IAnnotationCodeGenerator.GenerateFluentApi(IProperty property, IAnnotation annotation, string language)
            {
                throw new NotImplementedException();
            }

            public MethodCallCodeFragment GenerateFluentApi(IForeignKey foreignKey, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            string IAnnotationCodeGenerator.GenerateFluentApi(IForeignKey foreignKey, IAnnotation annotation, string language)
            {
                throw new NotImplementedException();
            }

            public MethodCallCodeFragment GenerateFluentApi(IIndex index, IAnnotation annotation)
            {
                throw new NotImplementedException();
            }

            string IAnnotationCodeGenerator.GenerateFluentApi(IIndex index, IAnnotation annotation, string language)
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

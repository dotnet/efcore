// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpModelGeneratorTest
    {
        [Fact]
        public void Language_works()
        {
            var generator = CreateGenerator();

            var result = generator.Language;

            Assert.Equal("C#", result);
        }

        [Fact]
        public void WriteCode_works()
        {
            var generator = CreateGenerator();
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity("TestEntity").Property<int>("Id").HasAnnotation(ScaffoldingAnnotationNames.ColumnOrdinal, 0);

            var result = generator.GenerateModel(
                modelBuilder.Model,
                "TestNamespace",
                Path.Combine("..", "TestContextDir" + Path.DirectorySeparatorChar),
                "TestContext",
                "Data Source=Test",
                new ModelCodeGenerationOptions());

            Assert.Equal(Path.Combine("..", "TestContextDir", "TestContext.cs"), result.ContextFile.Path);
            Assert.NotEmpty(result.ContextFile.Code);

            Assert.Equal(1, result.AdditionalFiles.Count);
            Assert.Equal("TestEntity.cs", result.AdditionalFiles[0].Path);
            Assert.NotEmpty(result.AdditionalFiles[0].Code);
        }

        private static IModelCodeGenerator CreateGenerator()
            => new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                .AddSingleton<IProviderConfigurationCodeGenerator, TestProviderCodeGenerator>()
                .BuildServiceProvider()
                .GetRequiredService<IModelCodeGenerator>();
    }
}

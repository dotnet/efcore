// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class ReverseEngineerScaffolderTest
    {
        [Fact]
        public void Save_works()
        {
            using (var directory = new TempDirectory())
            {
                var scaffolder = CreateScaffolder();
                var scaffoldedModel = new ScaffoldedModel
                {
                    ContextFile = new ScaffoldedFile
                    {
                        Path = Path.Combine("..", "Data", "TestContext.cs"),
                        Code = "// TestContext"
                    },
                    AdditionalFiles =
                    {
                        new ScaffoldedFile
                        {
                            Path = "TestEntity.cs",
                            Code = "// TestEntity"
                        }
                    }
                };

                var result = scaffolder.Save(
                    scaffoldedModel,
                    Path.Combine(directory.Path, "Models"),
                    overwriteFiles: false);

                var contextPath = Path.Combine(directory.Path, "Data", "TestContext.cs");
                Assert.Equal(contextPath, result.ContextFile);
                Assert.Equal("// TestContext", File.ReadAllText(contextPath));

                Assert.Equal(1, result.AdditionalFiles.Count);
                var entityTypePath = Path.Combine(directory.Path, "Models", "TestEntity.cs");
                Assert.Equal(entityTypePath, result.AdditionalFiles[0]);
                Assert.Equal("// TestEntity", File.ReadAllText(entityTypePath));
            }
        }

        [Fact]
        public void Save_throws_when_existing_files()
        {
            using (var directory = new TempDirectory())
            {
                var contextPath = Path.Combine(directory.Path, "TestContext.cs");
                File.WriteAllText(contextPath, "// Old");

                var entityTypePath = Path.Combine(directory.Path, "TestEntity.cs");
                File.WriteAllText(entityTypePath, "// Old");

                var scaffolder = CreateScaffolder();
                var scaffoldedModel = new ScaffoldedModel
                {
                    ContextFile = new ScaffoldedFile
                    {
                        Path = "TestContext.cs",
                        Code = "// TestContext"
                    },
                    AdditionalFiles =
                    {
                        new ScaffoldedFile
                        {
                            Path = "TestEntity.cs",
                            Code = "// TestEntity"
                        }
                    }
                };

                var ex = Assert.Throws<OperationException>(
                    () => scaffolder.Save(scaffoldedModel, directory.Path, overwriteFiles: false));

                Assert.Equal(
                    DesignStrings.ExistingFiles(
                        directory.Path,
                        string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, "TestContext.cs", "TestEntity.cs")),
                    ex.Message);
            }
        }

        [Fact]
        public void Save_works_when_overwriteFiles()
        {
            using (var directory = new TempDirectory())
            {
                var path = Path.Combine(directory.Path, "Test.cs");
                File.WriteAllText(path, "// Old");

                var scaffolder = CreateScaffolder();
                var scaffoldedModel = new ScaffoldedModel
                {
                    ContextFile = new ScaffoldedFile
                    {
                        Path = "Test.cs",
                        Code = "// Test"
                    }
                };

                var result = scaffolder.Save(scaffoldedModel, directory.Path, overwriteFiles: true);

                Assert.Equal(path, result.ContextFile);
                Assert.Equal("// Test", File.ReadAllText(path));
            }
        }

        [Fact]
        public void Save_throws_when_readonly_files()
        {
            using (var directory = new TempDirectory())
            {
                var contextPath = Path.Combine(directory.Path, "TestContext.cs");
                File.WriteAllText(contextPath, "// Old");

                var entityTypePath = Path.Combine(directory.Path, "TestEntity.cs");
                File.WriteAllText(entityTypePath, "// Old");

                var originalAttributes = File.GetAttributes(contextPath);
                File.SetAttributes(contextPath, originalAttributes | FileAttributes.ReadOnly);
                File.SetAttributes(entityTypePath, originalAttributes | FileAttributes.ReadOnly);
                try
                {
                    var scaffolder = CreateScaffolder();
                    var scaffoldedModel = new ScaffoldedModel
                    {
                        ContextFile = new ScaffoldedFile
                        {
                            Path = "TestContext.cs",
                            Code = "// TestContext"
                        },
                        AdditionalFiles =
                        {
                            new ScaffoldedFile
                            {
                                Path = "TestEntity.cs",
                                Code = "// TestEntity"
                            }
                        }
                    };

                    var ex = Assert.Throws<OperationException>(
                        () => scaffolder.Save(scaffoldedModel, directory.Path, overwriteFiles: true));

                    Assert.Equal(
                        DesignStrings.ReadOnlyFiles(
                            directory.Path,
                            string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, "TestContext.cs", "TestEntity.cs")),
                        ex.Message);
                }
                finally
                {
                    File.SetAttributes(contextPath, originalAttributes);
                    File.SetAttributes(entityTypePath, originalAttributes);
                }
            }
        }

        private static IReverseEngineerScaffolder CreateScaffolder()
            => new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<IRelationalTypeMappingSource, TestRelationalTypeMappingSource>()
                .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                .AddSingleton<IDatabaseModelFactory, FakeDatabaseModelFactory>()
                .AddSingleton<IProviderConfigurationCodeGenerator, TestProviderCodeGenerator>()
                .AddSingleton<IScaffoldingModelFactory, FakeScaffoldingModelFactory>()
                .BuildServiceProvider()
                .GetRequiredService<IReverseEngineerScaffolder>();
    }
}

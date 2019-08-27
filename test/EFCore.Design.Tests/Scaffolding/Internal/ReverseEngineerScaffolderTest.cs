// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Globalization;
using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class ReverseEngineerScaffolderTest
    {
        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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
                .AddSingleton<LoggingDefinitions, TestRelationalLoggingDefinitions>()
                .AddSingleton<IRelationalTypeMappingSource, TestRelationalTypeMappingSource>()
                .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                .AddSingleton<IDatabaseModelFactory, FakeDatabaseModelFactory>()
                .AddSingleton<IProviderConfigurationCodeGenerator, TestProviderCodeGenerator>()
                .AddSingleton<IScaffoldingModelFactory, FakeScaffoldingModelFactory>()
                .BuildServiceProvider()
                .GetRequiredService<IReverseEngineerScaffolder>();

        [ConditionalFact]
        public void ScaffoldModel_works_with_named_connection_string()
        {
            var resolver = new TestNamedConnectionStringResolver("Data Source=Test");
            var databaseModelFactory = new TestDatabaseModelFactory();
            var scaffolder = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<INamedConnectionStringResolver>(resolver)
                .AddSingleton<IDatabaseModelFactory>(databaseModelFactory)
                .AddSingleton<IRelationalTypeMappingSource, TestRelationalTypeMappingSource>()
                .AddSingleton<LoggingDefinitions, TestRelationalLoggingDefinitions>()
                .AddSingleton<IProviderConfigurationCodeGenerator, TestProviderCodeGenerator>()
                .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                .BuildServiceProvider()
                .GetRequiredService<IReverseEngineerScaffolder>();

            var result = scaffolder.ScaffoldModel(
                "Name=DefaultConnection",
                new DatabaseModelFactoryOptions(),
                new ModelReverseEngineerOptions(),
                new ModelCodeGenerationOptions());

            Assert.Equal("Data Source=Test", databaseModelFactory.ConnectionString);

            Assert.Contains("Name=DefaultConnection", result.ContextFile.Code);
            Assert.DoesNotContain("Data Source=Test", result.ContextFile.Code);
            Assert.DoesNotContain("#warning", result.ContextFile.Code);
        }

        private class TestNamedConnectionStringResolver : INamedConnectionStringResolver
        {
            private readonly string _resolvedConnectionString;

            public TestNamedConnectionStringResolver(string resolvedConnectionString)
                => _resolvedConnectionString = resolvedConnectionString;

            public string ResolveConnectionString(string connectionString)
                => _resolvedConnectionString;
        }

        private class TestDatabaseModelFactory : IDatabaseModelFactory
        {
            public string ConnectionString { get; set; }

            public DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
            {
                ConnectionString = connectionString;

                return new DatabaseModel();
            }

            public DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
                => throw new System.NotImplementedException();
        }
    }
}

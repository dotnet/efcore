// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering;
using Microsoft.Data.Entity.Sqlite.FunctionalTests;
using Xunit;
using Xunit.Abstractions;

namespace EntityFramework.Sqlite.Design.FunctionalTests.ReverseEngineering
{
    public class SqliteCheckTemplates : E2ETestBase
    {
        public SqliteCheckTemplates(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async void It_uses_templates()
        {
            var dbContextFileName = "EntityFramework.Sqlite.Design." + ReverseEngineeringGenerator.DbContextTemplateFileName;
            var entityTypeFileName = "EntityFramework.Sqlite.Design." + ReverseEngineeringGenerator.EntityTypeTemplateFileName;
            var entityTemplate = "This is an entity type template! (For real)";
            var contextTemplate = "Also a 100% legit template";
            var outputDir = "gen";
            var templatesDir = "templates";

            using (var testStore = SqliteTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery("CREATE TABLE RealMccoy ( Col1 text PRIMARY KEY); ");

                InMemoryFiles.OutputFile(templatesDir, dbContextFileName, contextTemplate);
                InMemoryFiles.OutputFile(templatesDir, entityTypeFileName, entityTemplate);

                var config = new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.Connection.ConnectionString,
                    ProjectPath = outputDir,
                    CustomTemplatePath = templatesDir,
                    ProjectRootNamespace = "Test",
                };
                var filePaths = await Generator.GenerateAsync(config);

                var expectedLog = new LoggerMessages
                {
                    Info =
                            {
                                "Using custom template " + Path.Combine(templatesDir, dbContextFileName),
                                "Using custom template " + Path.Combine(templatesDir, entityTypeFileName)
                            }
                };
                AssertLog(expectedLog);

                Assert.Equal(2, filePaths.Count);

                foreach (var fileName in filePaths.Select(Path.GetFileName))
                {
                    var fileContents = InMemoryFiles.RetrieveFileContents(outputDir, fileName);
                    var contents = fileName.EndsWith("Context.cs") ? contextTemplate : entityTemplate;
                    Assert.Equal(contents, fileContents);
                }
            }
        }

        [Fact]
        public void It_outputs_templates()
        {
            var dbContextFileName = "EntityFramework.Sqlite.Design." + ReverseEngineeringGenerator.DbContextTemplateFileName;
            var entityTypeFileName = "EntityFramework.Sqlite.Design." + ReverseEngineeringGenerator.EntityTypeTemplateFileName;
            var outputDir = "templates/";

            var filePaths = Generator.Customize(outputDir);

            AssertLog(new LoggerMessages());

            Assert.Collection(filePaths,
                file1 => Assert.Equal(file1, Path.Combine(outputDir, dbContextFileName)),
                file2 => Assert.Equal(file2, Path.Combine(outputDir, entityTypeFileName)));

            var dbContextTemplateContents = InMemoryFiles.RetrieveFileContents(
                outputDir, dbContextFileName);
            Assert.Equal(MetadataModelProvider.DbContextTemplate, dbContextTemplateContents);

            var entityTypeTemplateContents = InMemoryFiles.RetrieveFileContents(
                outputDir, entityTypeFileName);
            Assert.Equal(MetadataModelProvider.EntityTypeTemplate, entityTypeTemplateContents);
        }

        protected override E2ECompiler GetCompiler() => new E2ECompiler
        {
            NamedReferences =
                    {
                        "EntityFramework.Core",
                        "EntityFramework.Relational",
                        "EntityFramework.Sqlite",
                        // ReSharper disable once RedundantCommaInInitializer
                        "Microsoft.Data.Sqlite",
#if DNXCORE50
                        "System.Data.Common",
                        "System.Linq.Expressions",
                        "System.Reflection"
#else
                    },
            References =
                    {
                        MetadataReference.CreateFromFile(
                            Assembly.Load(new AssemblyName(
                                "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location)
#endif
                    }
        };

        protected override string ProviderName => "EntityFramework.Sqlite.Design";
        protected override IDesignTimeMetadataProviderFactory GetFactory() => new SqliteDesignTimeMetadataProviderFactory();
    }
}

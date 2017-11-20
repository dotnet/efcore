// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class ReverseEngineeringOutputTests
    {
        [Theory]
        [InlineData("FakeOutputDir", null)]
        [InlineData("FakeOutputDir", "FakeOutputDir")]
        [InlineData("FakeOutputDir", "FakeContextOutputDir")]
        [InlineData("FakeOutputDir", "../AnotherFakeProject")]
        [InlineData("FakeOutputDir", "../AnotherFakeProject/FakeContextOutputDir")]
        [InlineData("FakeOutputDir", "rooted/AnotherFakeProject")]
        [InlineData("FakeOutputDir", "rooted/AnotherFakeProject/FakeContextOutputDir")]
        public void ReverseEngineerScaffolder_generates_separate_context_output_path(string outputDir, string outputDbContextDir)
        {
            var scaffolder = CreateScaffolder();
            if (outputDbContextDir != null && outputDbContextDir.StartsWith("rooted"))
            {
                var altDirName = outputDbContextDir.Substring(7);
                outputDbContextDir = Path.Combine(new TempDirectory().Path, altDirName);
            }
            var scaffoldedModel = ReverseEngineerContext(outputDir, outputDbContextDir);
            var projectPath = Path.Combine(new TempDirectory().Path, "FakeProjectDir");
            var files = scaffolder.Save(scaffoldedModel, projectPath, outputDir, outputDbContextDir, overwriteFiles: true);

            var contextPath = new DirectoryInfo(Path.GetDirectoryName(files.ContextFile)).Name;
            var expectedOutputPath = outputDir;
            if (outputDbContextDir != null && outputDbContextDir != outputDir)
            {
                expectedOutputPath = new DirectoryInfo(outputDbContextDir).Name;
            }

            Assert.Equal(expectedOutputPath, contextPath);
        }

        private ScaffoldedModel ReverseEngineerContext(string outputPath, string outputContextPath)
        {
            var scaffolder = CreateScaffolder();

            return scaffolder.ScaffoldModel(
                connectionString: "connectionstring",
                tables: Enumerable.Empty<string>(),
                schemas: Enumerable.Empty<string>(),
                @namespace: "FakeNamespace",
                language: "",
                contextName: "FakeContext",
                useDataAnnotations: false,
                useDatabaseNames: false);
        }

        private static IReverseEngineerScaffolder CreateScaffolder()
            => new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                .AddSingleton<IDatabaseModelFactory, FakeDatabaseModelFactory>()
                .AddSingleton<IProviderCodeGenerator, TestProviderCodeGenerator>()
                .AddSingleton<IScaffoldingModelFactory, FakeScaffoldingModelFactory>()
                .BuildServiceProvider()
                .GetRequiredService<IReverseEngineerScaffolder>();


        private class FakeScaffoldingModelFactory : RelationalScaffoldingModelFactory
        {
            public override IModel Create(DatabaseModel databaseModel, bool useDatabaseNames = false)
            {
                foreach (var sequence in databaseModel.Sequences)
                {
                    sequence.Database = databaseModel;
                }

                foreach (var table in databaseModel.Tables)
                {
                    table.Database = databaseModel;

                    foreach (var column in table.Columns)
                    {
                        column.Table = table;
                    }

                    if (table.PrimaryKey != null)
                    {
                        table.PrimaryKey.Table = table;
                    }

                    foreach (var index in table.Indexes)
                    {
                        index.Table = table;
                    }

                    foreach (var uniqueConstraints in table.UniqueConstraints)
                    {
                        uniqueConstraints.Table = table;
                    }

                    foreach (var foreignKey in table.ForeignKeys)
                    {
                        foreignKey.Table = table;
                    }
                }

                return base.Create(databaseModel, useDatabaseNames);
            }

            public FakeScaffoldingModelFactory(
                IOperationReporter reporter,
                IPluralizer pluralizer)
                : base(
                    reporter,
                    new CandidateNamingService(),
                    pluralizer,
                    new CSharpUtilities(),
                    new ScaffoldingTypeMapper(
                        new SqlServerTypeMapper(
                            new CoreTypeMapperDependencies(new ValueConverterSelector(new ValueConverterSelectorDependencies())),
                            new RelationalTypeMapperDependencies())))
            {
            }
        }

        private class FakeDatabaseModelFactory : IDatabaseModelFactory
        {
            public DatabaseModel Create(string connectionString, IEnumerable<string> tables, IEnumerable<string> schemas) => new DatabaseModel();

            public DatabaseModel Create(DbConnection connectio, IEnumerable<string> tables, IEnumerable<string> schemas) => new DatabaseModel();
        }

        private class FakeScaffoldingCodeGenerator : IProviderCodeGenerator
        {
            public string UseProviderMethod => throw new NotImplementedException();
        }
    }
}

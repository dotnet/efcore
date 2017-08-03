// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.ReverseEngineering
{
    public class SqlServerE2ETests : E2ETestBase
    {
        protected override string ProviderName => "Microsoft.EntityFrameworkCore.SqlServer.Design";

        protected override void ConfigureDesignTimeServices(IServiceCollection services)
            => new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);

        public virtual string TestNamespace => "E2ETest.Namespace";
        public virtual string TestProjectDir => Path.Combine("E2ETest", "Output");

        public SqlServerE2ETests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void Non_null_boolean_columns_with_default_constraint_become_nullable_properties()
        {
            using (var scratch = SqlServerTestStore.Create("NonNullBooleanWithDefaultConstraint"))
            {
                scratch.ExecuteNonQuery(@"
CREATE TABLE NonNullBoolWithDefault
(
     Id int NOT NULL PRIMARY KEY CLUSTERED,
     BoolWithDefaultValueSql bit NOT NULL DEFAULT (CONVERT(""bit"", GETDATE())),
     BoolWithoutDefaultValueSql bit NOT NULL
)");

                var expectedFileSet = new FileSet(new FileSystemFileService(),
                    Path.Combine("ReverseEngineering", "Expected"),
                    contents => contents.Replace("{{connectionString}}", scratch.ConnectionString))
                {
                    Files = new List<string>
                    {
                        "NonNullBoolWithDefaultContext.cs",
                        "NonNullBoolWithDefault.cs",
                    }
                };

                var filePaths = Generator.Generate(
                        scratch.ConnectionString,
                        Enumerable.Empty<string>(),
                        Enumerable.Empty<string>(),
                        TestProjectDir + Path.DirectorySeparatorChar,
                        outputPath: null, // not used for this test
                        rootNamespace: TestNamespace,
                        contextName: "NonNullBoolWithDefaultContext",
                        useDataAnnotations: false,
                        overwriteFiles: false,
                        useDatabaseNames: false);


                var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
                {
                    Files = new[] { filePaths.ContextFile }.Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };

                Assert.Contains("warn: " + DesignStrings.NonNullableBoooleanColumnHasDefaultConstraint("dbo.NonNullBoolWithDefault.BoolWithDefaultValueSql"), _reporter.Messages);
                Assert.Equal(1, _reporter.Messages.Count(m => m.StartsWith("warn: ")));

                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [ConditionalFact]
        public void Correct_arguments_to_scaffolding_typemapper()
        {
            using (var scratch = SqlServerTestStore.Create("StringKeys"))
            {
                scratch.ExecuteNonQuery(@"
CREATE TABLE [StringKeysBlogs] (
    [PrimaryKey] nvarchar(450) NOT NULL,
    [AlternateKey] nvarchar(450) NOT NULL,
    [IndexProperty] nvarchar(450) NULL,
    [RowVersion] rowversion NULL,
    CONSTRAINT [PK_StringKeysBlogs] PRIMARY KEY ([PrimaryKey]),
    CONSTRAINT [AK_StringKeysBlogs_AlternateKey] UNIQUE ([AlternateKey])
);

CREATE INDEX [IX_StringKeysBlogs_IndexProperty] ON [StringKeysBlogs] ([IndexProperty]);

CREATE TABLE [StringKeysPosts] (
    [Id] int NOT NULL IDENTITY,
    [BlogAlternateKey] nvarchar(450) NULL,
    CONSTRAINT [PK_StringKeysPosts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StringKeysPosts_StringKeysBlogs_BlogAlternateKey] FOREIGN KEY ([BlogAlternateKey]) REFERENCES [StringKeysBlogs] ([AlternateKey]) ON DELETE NO ACTION
);

CREATE INDEX [IX_StringKeysPosts_BlogAlternateKey] ON [StringKeysPosts] ([BlogAlternateKey]);
");

                var expectedFileSet = new FileSet(new FileSystemFileService(),
                    Path.Combine("ReverseEngineering", "Expected"),
                    contents => contents.Replace("{{connectionString}}", scratch.ConnectionString))
                {
                    Files = new List<string>
                    {
                        "StringKeysContext.cs",
                        "StringKeysBlogs.cs",
                        "StringKeysPosts.cs",
                    }
                };

                var filePaths = Generator.Generate(
                    scratch.ConnectionString,
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>(),
                    TestProjectDir + Path.DirectorySeparatorChar,
                    outputPath: null, // not used for this test
                    rootNamespace: TestNamespace,
                    contextName: "StringKeysContext",
                    useDataAnnotations: false,
                    overwriteFiles: false,
                    useDatabaseNames: false);

                var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
                {
                    Files = new[] { filePaths.ContextFile }.Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };

                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        protected override ICollection<BuildReference> References { get; } = new List<BuildReference>
        {
            BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational")
        };
    }
}

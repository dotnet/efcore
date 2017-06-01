// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.ReverseEngineering
{
    public class SqlServerE2ETests : E2ETestBase, IClassFixture<SqlServerE2EFixture>
    {
        protected override string ProviderName => "Microsoft.EntityFrameworkCore.SqlServer.Design";

        protected override void ConfigureDesignTimeServices(IServiceCollection services)
            => new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);

        public virtual string TestNamespace => "E2ETest.Namespace";
        public virtual string TestProjectDir => Path.Combine("E2ETest", "Output");
        public virtual string TestSubDir => "SubDir";
        public virtual string CustomizedTemplateDir => Path.Combine("E2ETest", "CustomizedTemplate", "Dir");

        public static TableSelectionSet Filter
            => new TableSelectionSet(new List<string>
            {
                "AllDataTypes",
                "PropertyConfiguration",
                "Test Spaces Keywords Table",
                "MultipleFKsDependent",
                "MultipleFKsPrincipal",
                "OneToManyDependent",
                "OneToManyPrincipal",
                "OneToOneDependent",
                "OneToOnePrincipal",
                "OneToOneSeparateFKDependent",
                "OneToOneSeparateFKPrincipal",
                "OneToOneFKToUniqueKeyDependent",
                "OneToOneFKToUniqueKeyPrincipal",
                "UnmappablePKColumn",
                "TableWithUnmappablePrimaryKeyColumn",
                "selfreferencing"
            });

        public SqlServerE2ETests(SqlServerE2EFixture fixture, ITestOutputHelper output)
            : base(output)
        {
        }

        private readonly string _connectionString =
            new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection)
            {
                InitialCatalog = "SqlServerReverseEngineerTestE2E"
            }.ConnectionString;

        private static readonly List<string> _expectedEntityTypeFiles = new List<string>
        {
            "AllDataTypes.cs",
            "MultipleFKsDependent.cs",
            "MultipleFKsPrincipal.cs",
            "OneToManyDependent.cs",
            "OneToManyPrincipal.cs",
            "OneToOneDependent.cs",
            "OneToOneFKToUniqueKeyDependent.cs",
            "OneToOneFKToUniqueKeyPrincipal.cs",
            "OneToOnePrincipal.cs",
            "OneToOneSeparateFKDependent.cs",
            "OneToOneSeparateFKPrincipal.cs",
            "PropertyConfiguration.cs",
            "SelfReferencing.cs",
            "TestSpacesKeywordsTable.cs",
            "UnmappablePKColumn.cs"
        };

        [Fact]
        [UseCulture("en-US")]
        public void E2ETest_UseAttributesInsteadOfFluentApi()
        {
            var filePaths = Generator.GenerateAsync(
                    _connectionString,
                    Filter,
                    TestProjectDir + Path.DirectorySeparatorChar, // tests that ending DirectorySeparatorChar does not affect namespace
                    TestSubDir,
                    TestNamespace,
                    contextName: "AttributesContext",
                    useDataAnnotations: true,
                    overwriteFiles: false)
                .GetAwaiter()
                .GetResult();

            var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(Path.Combine(TestProjectDir, TestSubDir)))
            {
                Files = Enumerable.Repeat(filePaths.ContextFile, 1).Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
            };

            var expectedFileSet = new FileSet(new FileSystemFileService(),
                Path.Combine("ReverseEngineering", "Expected", "Attributes"),
                contents => contents.Replace("{{connectionString}}", _connectionString))
            {
                Files = new List<string> { "AttributesContext.cs" }
                    .Concat(_expectedEntityTypeFiles).ToList()
            };

            var indexWarn = _reporter.Messages.Warn.Single(m => m.Contains("PK__Filtered__"));

            AssertLog(new LoggerMessages
            {
                Warn =
                {
                    indexWarn,
                    DesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("dbo.AllDataTypes.geographyColumn", "geography"),
                    DesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("dbo.AllDataTypes.geometryColumn", "geometry"),
                    DesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("dbo.AllDataTypes.hierarchyidColumn", "hierarchyid"),
                    DesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("dbo.AllDataTypes.sql_variantColumn", "sql_variant"),
                    RelationalStrings.LogUnableToScaffoldIndexMissingProperty.GenerateMessage("IX_UnscaffoldableIndex", "sql_variantColumn,hierarchyidColumn"),
                    //SqlServerDesignStrings.LogDataTypeDoesNotAllowSqlServerIdentityStrategy.GenerateMessage("dbo.PropertyConfiguration.PropertyConfigurationID", "tinyint"),
                    DesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("dbo.TableWithUnmappablePrimaryKeyColumn.TableWithUnmappablePrimaryKeyColumnID", "hierarchyid"),
                    DesignStrings.LogPrimaryKeyErrorPropertyNotFound.GenerateMessage("dbo.TableWithUnmappablePrimaryKeyColumn", "TableWithUnmappablePrimaryKeyColumnID"),
                    DesignStrings.LogUnableToGenerateEntityType.GenerateMessage("dbo.TableWithUnmappablePrimaryKeyColumn")
                }
            });
            AssertEqualFileContents(expectedFileSet, actualFileSet);
            AssertCompile(actualFileSet);
        }

        [Fact]
        [UseCulture("en-US")]
        public void E2ETest_AllFluentApi()
        {
            var filePaths = Generator.GenerateAsync(
                    _connectionString,
                    Filter,
                    TestProjectDir,
                    outputPath: null, // not used for this test
                    rootNamespace: TestNamespace,
                    contextName: null,
                    useDataAnnotations: false,
                    overwriteFiles: false)
                .GetAwaiter()
                .GetResult();

            var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
            {
                Files = Enumerable.Repeat(filePaths.ContextFile, 1).Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
            };

            var expectedFileSet = new FileSet(new FileSystemFileService(),
                Path.Combine("ReverseEngineering", "Expected", "AllFluentApi"),
                inputFile => inputFile.Replace("{{connectionString}}", _connectionString))
            {
                Files = new List<string> { "SqlServerReverseEngineerTestE2EContext.cs" }
                    .Concat(_expectedEntityTypeFiles).ToList()
            };

            var indexWarn = _reporter.Messages.Warn.Single(m => m.Contains("PK__Filtered__"));

            AssertLog(new LoggerMessages
            {
                Warn =
                {
                    indexWarn,
                    DesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("dbo.AllDataTypes.geographyColumn", "geography"),
                    DesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("dbo.AllDataTypes.geometryColumn", "geometry"),
                    DesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("dbo.AllDataTypes.hierarchyidColumn", "hierarchyid"),
                    DesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("dbo.AllDataTypes.sql_variantColumn", "sql_variant"),
                    RelationalStrings.LogUnableToScaffoldIndexMissingProperty.GenerateMessage("IX_UnscaffoldableIndex", "sql_variantColumn,hierarchyidColumn"),
                    //SqlServerDesignStrings.LogDataTypeDoesNotAllowSqlServerIdentityStrategy.GenerateMessage("dbo.PropertyConfiguration.PropertyConfigurationID", "tinyint"),
                    DesignStrings.LogCannotFindTypeMappingForColumn.GenerateMessage("dbo.TableWithUnmappablePrimaryKeyColumn.TableWithUnmappablePrimaryKeyColumnID", "hierarchyid"),
                    DesignStrings.LogPrimaryKeyErrorPropertyNotFound.GenerateMessage("dbo.TableWithUnmappablePrimaryKeyColumn", "TableWithUnmappablePrimaryKeyColumnID"),
                    DesignStrings.LogUnableToGenerateEntityType.GenerateMessage("dbo.TableWithUnmappablePrimaryKeyColumn")
                }
            });
            AssertEqualFileContents(expectedFileSet, actualFileSet);
            AssertCompile(actualFileSet);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public void Sequences()
        {
            using (var scratch = SqlServerTestStore.Create("SqlServerE2E"))
            {
                scratch.ExecuteNonQuery(@"
CREATE SEQUENCE CountByTwo
    START WITH 1
    INCREMENT BY 2;

CREATE SEQUENCE CyclicalCountByThree
    START WITH 6
    INCREMENT BY 3
    MAXVALUE 27
    MINVALUE 0
    CYCLE;

CREATE SEQUENCE TinyIntSequence
    AS tinyint
    START WITH 1;

CREATE SEQUENCE SmallIntSequence
    AS smallint
    START WITH 1;

CREATE SEQUENCE IntSequence
    AS int
    START WITH 1;

CREATE SEQUENCE DecimalSequence
    AS decimal;

CREATE SEQUENCE NumericSequence
    AS numeric;");


                var expectedFileSet = new FileSet(
                    new FileSystemFileService(),
                    Path.Combine("ReverseEngineering", "Expected"),
                    contents => contents.Replace("{{connectionString}}", scratch.ConnectionString))
                {
                    Files = new List<string> { "SequenceContext.cs" }
                };

                var filePaths = Generator.GenerateAsync(
                        scratch.ConnectionString,
                        TableSelectionSet.All,
                        TestProjectDir + Path.DirectorySeparatorChar,
                        outputPath: null, // not used for this test
                        rootNamespace: TestNamespace,
                        contextName: "SequenceContext",
                        useDataAnnotations: false,
                        overwriteFiles: false)
                    .GetAwaiter()
                    .GetResult();

                var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
                {
                    Files = new[] { filePaths.ContextFile }.Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };

                AssertLog(new LoggerMessages
                {
                    Warn =
                    {
                        DesignStrings.LogBadSequenceType.GenerateMessage("DecimalSequence", "decimal"),
                        DesignStrings.LogBadSequenceType.GenerateMessage("NumericSequence", "numeric")
                    }
                });

                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void PrimaryKeyWithSequence()
        {
            using (var scratch = SqlServerTestStore.Create("SqlServerE2E"))
            {
                scratch.ExecuteNonQuery(@"
CREATE SEQUENCE PrimaryKeyWithSequenceSequence
    AS int
    START WITH 1
    INCREMENT BY 1

CREATE TABLE PrimaryKeyWithSequence (
    PrimaryKeyWithSequenceId int DEFAULT(NEXT VALUE FOR PrimaryKeyWithSequenceSequence),
    PRIMARY KEY (PrimaryKeyWithSequenceId)
);
");

                var expectedFileSet = new FileSet(new FileSystemFileService(),
                    Path.Combine("ReverseEngineering", "Expected"),
                    contents => contents.Replace("{{connectionString}}", scratch.ConnectionString))
                {
                    Files = new List<string>
                    {
                        "PrimaryKeyWithSequenceContext.cs",
                        "PrimaryKeyWithSequence.cs"
                    }
                };

                var filePaths = Generator.GenerateAsync(
                        scratch.ConnectionString,
                        TableSelectionSet.All,
                        TestProjectDir + Path.DirectorySeparatorChar,
                        outputPath: null, // not used for this test
                        rootNamespace: TestNamespace,
                        contextName: "PrimaryKeyWithSequenceContext",
                        useDataAnnotations: false,
                        overwriteFiles: false)
                    .GetAwaiter()
                    .GetResult();

                var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
                {
                    Files = new[] { filePaths.ContextFile }.Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };

                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Index_with_filter()
        {
            using (var scratch = SqlServerTestStore.Create("SqlServerE2E"))
            {
                scratch.ExecuteNonQuery(@"
CREATE TABLE FilteredIndex (
    Id int,
    Number int,
    PRIMARY KEY (Id)
);

CREATE INDEX Unicorn_Filtered_Index
    ON FilteredIndex (Number) WHERE Number > 10
");

                var expectedFileSet = new FileSet(new FileSystemFileService(),
                    Path.Combine("ReverseEngineering", "Expected"),
                    contents => contents.Replace("{{connectionString}}", scratch.ConnectionString))
                {
                    Files = new List<string>
                    {
                        "FilteredIndexContext.cs",
                        "FilteredIndex.cs",
                    }
                };

                var filePaths = Generator.GenerateAsync(
                        scratch.ConnectionString,
                        TableSelectionSet.All,
                        TestProjectDir + Path.DirectorySeparatorChar,
                        outputPath: null, // not used for this test
                        rootNamespace: TestNamespace,
                        contextName: "FilteredIndexContext",
                        useDataAnnotations: false,
                        overwriteFiles: false)
                    .GetAwaiter()
                    .GetResult();

                var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
                {
                    Files = new[] { filePaths.ContextFile }.Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };

                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsHiddenColumns)]
        public void Hidden_Column()
        {
            using (var scratch = SqlServerTestStore.Create("WithHiddenColumns"))
            {
                scratch.ExecuteNonQuery(@"
CREATE TABLE dbo.SystemVersioned
(
     Id int NOT NULL PRIMARY KEY CLUSTERED,
     Name varchar(50) NOT NULL,
     SysStartTime datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
     SysEndTime datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
     PERIOD FOR SYSTEM_TIME(SysStartTime, SysEndTime)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.History));
");

                var expectedFileSet = new FileSet(new FileSystemFileService(),
                    Path.Combine("ReverseEngineering", "Expected"),
                    contents => contents.Replace("{{connectionString}}", scratch.ConnectionString))
                {
                    Files = new List<string>
                    {
                        "SystemVersionedContext.cs",
                        "SystemVersioned.cs",
                    }
                };

                var filePaths = Generator.GenerateAsync(
                        scratch.ConnectionString,
                        TableSelectionSet.All,
                        TestProjectDir + Path.DirectorySeparatorChar,
                        outputPath: null, // not used for this test
                        rootNamespace: TestNamespace,
                        contextName: "SystemVersionedContext",
                        useDataAnnotations: false,
                        overwriteFiles: false)
                    .GetAwaiter()
                    .GetResult();


                scratch.ExecuteNonQuery(@"
ALTER TABLE dbo.SystemVersioned SET (SYSTEM_VERSIONING = OFF);
DROP TABLE dbo.History;
");

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

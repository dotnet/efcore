// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Relational.Design.FunctionalTests.ReverseEngineering;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.FunctionalTests.ReverseEngineering
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
                "OneToManyDependent",
                "OneToManyPrincipal",
                "OneToOneDependent",
                "OneToOnePrincipal",
                "OneToOneSeparateFKDependent",
                "OneToOneSeparateFKPrincipal",
                "OneToOneFKToUniqueKeyDependent",
                "OneToOneFKToUniqueKeyPrincipal",
                "ReferredToByTableWithUnmappablePrimaryKeyColumn",
                "TableWithUnmappablePrimaryKeyColumn",
                "selfreferencing"
            });

        public SqlServerE2ETests(SqlServerE2EFixture fixture, ITestOutputHelper output)
            : base(output)
        {
        }

        protected override E2ECompiler GetCompiler() => new E2ECompiler
        {
            NamedReferences =
            {
                "Microsoft.EntityFrameworkCore",
                "Microsoft.EntityFrameworkCore.Relational",
                // ReSharper disable once RedundantCommaInInitializer
                "Microsoft.EntityFrameworkCore.SqlServer",
#if DNXCORE50
                        "System.Data.Common",
                        "System.Linq.Expressions",
                        "System.Reflection",
                        "System.ComponentModel.Annotations",
#else
            },
            References =
            {
                MetadataReference.CreateFromFile(
                    Assembly.Load(new AssemblyName(
                        "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location),
                MetadataReference.CreateFromFile(
                    Assembly.Load(new AssemblyName(
                        "System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")).Location)
#endif
            }
        };

        // weird extenstion method call because the compiler can't disambiguate without adding a project reference
        // ApplyConfiguration swaps out the Server if this tests are configured to run against something different that localdb.
        // ReSharper disable once InvokeAsExtensionMethod
        private readonly string _connectionString =
            new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection)
            {
                InitialCatalog = "SqlServerReverseEngineerTestE2E"
            }.ConnectionString;

        private static readonly List<string> _expectedEntityTypeFiles = new List<string>
        {
            "AllDataTypes.expected",
            "OneToManyDependent.expected",
            "OneToManyPrincipal.expected",
            "OneToOneDependent.expected",
            "OneToOneFKToUniqueKeyDependent.expected",
            "OneToOneFKToUniqueKeyPrincipal.expected",
            "OneToOnePrincipal.expected",
            "OneToOneSeparateFKDependent.expected",
            "OneToOneSeparateFKPrincipal.expected",
            "PropertyConfiguration.expected",
            "ReferredToByTableWithUnmappablePrimaryKeyColumn.expected",
            "SelfReferencing.expected",
            "Test_Spaces_Keywords_Table.expected"
        };

        [Fact]
        [UseCulture("en-US")]
        public void E2ETest_UseAttributesInsteadOfFluentApi()
        {
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = _connectionString,
                ContextClassName = "AttributesContext",
                ProjectPath = TestProjectDir + Path.DirectorySeparatorChar, // tests that ending DirectorySeparatorChar does not affect namespace
                ProjectRootNamespace = TestNamespace,
                OutputPath = TestSubDir,
                TableSelectionSet = Filter
            };

            var filePaths = Generator.GenerateAsync(configuration).GetAwaiter().GetResult();

            var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(Path.Combine(TestProjectDir, TestSubDir)))
            {
                Files = Enumerable.Repeat(filePaths.ContextFile, 1).Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
            };

            var expectedFileSet = new FileSet(new FileSystemFileService(),
                Path.Combine("ReverseEngineering", "ExpectedResults", "E2E_UseAttributesInsteadOfFluentApi"),
                contents => contents.Replace("namespace " + TestNamespace, "namespace " + TestNamespace + "." + TestSubDir)
                    .Replace("{{connectionString}}", _connectionString))
            {
                Files = new List<string> { "AttributesContext.expected" }
                    .Concat(_expectedEntityTypeFiles).ToList()
            };

            AssertLog(new LoggerMessages
            {
                Warn =
                {
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.geographyColumn", "geography"),
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.geometryColumn", "geometry"),
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.hierarchyidColumn", "hierarchyid"),
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.sql_variantColumn", "sql_variant"),
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.xmlColumn", "xml"),
                    RelationalDesignStrings.UnableToScaffoldIndexMissingProperty("IX_UnscaffoldableIndex"),
                    SqlServerDesignStrings.DataTypeDoesNotAllowSqlServerIdentityStrategy("dbo.PropertyConfiguration.PropertyConfigurationID", "tinyint"),
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.TableWithUnmappablePrimaryKeyColumn.TableWithUnmappablePrimaryKeyColumnID", "hierarchyid"),
                    RelationalDesignStrings.PrimaryKeyErrorPropertyNotFound("dbo.TableWithUnmappablePrimaryKeyColumn"),
                    RelationalDesignStrings.UnableToGenerateEntityType("dbo.TableWithUnmappablePrimaryKeyColumn")
                }
            });
            AssertEqualFileContents(expectedFileSet, actualFileSet);
            AssertCompile(actualFileSet);
        }

        [Fact]
        [UseCulture("en-US")]
        public void E2ETest_AllFluentApi()
        {
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = _connectionString,
                ProjectPath = TestProjectDir,
                ProjectRootNamespace = TestNamespace,
                OutputPath = null, // not used for this test
                UseFluentApiOnly = true,
                TableSelectionSet = Filter
            };

            var filePaths = Generator.GenerateAsync(configuration).GetAwaiter().GetResult();

            var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
            {
                Files = Enumerable.Repeat(filePaths.ContextFile, 1).Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
            };

            var expectedFileSet = new FileSet(new FileSystemFileService(),
                Path.Combine("ReverseEngineering", "ExpectedResults", "E2E_AllFluentApi"),
                inputFile => inputFile.Replace("{{connectionString}}", _connectionString))
            {
                Files = new List<string> { "SqlServerReverseEngineerTestE2EContext.expected" }
                    .Concat(_expectedEntityTypeFiles).ToList()
            };

            AssertLog(new LoggerMessages
            {
                Warn =
                {
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.geographyColumn", "geography"),
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.geometryColumn", "geometry"),
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.hierarchyidColumn", "hierarchyid"),
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.sql_variantColumn", "sql_variant"),
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.AllDataTypes.xmlColumn", "xml"),
                    RelationalDesignStrings.UnableToScaffoldIndexMissingProperty("IX_UnscaffoldableIndex"),
                    SqlServerDesignStrings.DataTypeDoesNotAllowSqlServerIdentityStrategy("dbo.PropertyConfiguration.PropertyConfigurationID", "tinyint"),
                    RelationalDesignStrings.CannotFindTypeMappingForColumn("dbo.TableWithUnmappablePrimaryKeyColumn.TableWithUnmappablePrimaryKeyColumnID", "hierarchyid"),
                    RelationalDesignStrings.PrimaryKeyErrorPropertyNotFound("dbo.TableWithUnmappablePrimaryKeyColumn"),
                    RelationalDesignStrings.UnableToGenerateEntityType("dbo.TableWithUnmappablePrimaryKeyColumn")
                }
            });
            AssertEqualFileContents(expectedFileSet, actualFileSet);
            AssertCompile(actualFileSet);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public void Sequences()
        {
            using (var scratch = SqlServerTestStore.CreateScratch())
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

                var configuration = new ReverseEngineeringConfiguration
                {
                    ConnectionString = scratch.ConnectionString,
                    ProjectPath = TestProjectDir + Path.DirectorySeparatorChar,
                    ProjectRootNamespace = TestNamespace,
                    ContextClassName = "SequenceContext"
                };
                var expectedFileSet = new FileSet(new FileSystemFileService(),
                    Path.Combine("ReverseEngineering", "ExpectedResults"),
                    contents => contents.Replace("{{connectionString}}", scratch.ConnectionString))
                {
                    Files = new List<string> { "SequenceContext.expected" }
                };

                var filePaths = Generator.GenerateAsync(configuration).GetAwaiter().GetResult();

                var actualFileSet = new FileSet(InMemoryFiles, Path.GetFullPath(TestProjectDir))
                {
                    Files = new[] { filePaths.ContextFile }.Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };

                AssertLog(new LoggerMessages
                {
                    Warn =
                    {
                        RelationalDesignStrings.BadSequenceType("DecimalSequence", "decimal"),
                        RelationalDesignStrings.BadSequenceType("NumericSequence", "numeric")
                    }
                });

                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }
    }
}

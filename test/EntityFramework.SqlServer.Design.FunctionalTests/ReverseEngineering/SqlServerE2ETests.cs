// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Data.Entity.SqlServer.Design.FunctionalTests.ReverseEngineering
{
    public class SqlServerE2ETests : E2ETestBase, IClassFixture<SqlServerE2EFixture>
    {
        protected override string ProviderName => "EntityFramework.SqlServer.Design";
        protected override IDesignTimeMetadataProviderFactory GetFactory() => new SqlServerDesignTimeMetadataProviderFactory();
        public virtual string TestNamespace => "E2ETest.Namespace";
        public virtual string TestProjectDir => Path.Combine("E2ETest", "Output");
        public virtual string TestSubDir => "SubDir";
        public virtual string CustomizedTemplateDir => Path.Combine("E2ETest", "CustomizedTemplate", "Dir");

        public SqlServerE2ETests(SqlServerE2EFixture fixture, ITestOutputHelper output)
            : base(output)
        {
        }

        protected override E2ECompiler GetCompiler() => new E2ECompiler
        {
            NamedReferences =
                    {
                        "EntityFramework.Core",
                        "EntityFramework.Relational",
                        "EntityFramework.SqlServer",
#if DNXCORE50 || NETCORE50
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
                                "System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")).Location),
#endif
                    }
        };

        private const string _connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=SqlServerReverseEngineerTestE2E;Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=30";

        private static readonly List<string> _expectedFiles = new List<string>
            {
                "SqlServerReverseEngineerTestE2EContext.expected",
                "AllDataTypes.expected",
                "OneToManyDependent.expected",
                "OneToManyPrincipal.expected",
                "OneToOneDependent.expected",
                "OneToOnePrincipal.expected",
                "OneToOneSeparateFKDependent.expected",
                "OneToOneSeparateFKPrincipal.expected",
                "PropertyConfiguration.expected",
                "ReferredToByTableWithUnmappablePrimaryKeyColumn.expected",
                "SelfReferencing.expected",
                "TableWithUnmappablePrimaryKeyColumn.expected",
                "Test_Spaces_Keywords_Table.expected"
            };

        [Fact]
        public void E2ETest_UseAttributesInsteadOfFluentApi()
        {
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = _connectionString,
                ProjectPath = TestProjectDir,
                ProjectRootNamespace = TestNamespace,
                RelativeOutputPath = TestSubDir
            };

            var filePaths = Generator.GenerateAsync(configuration).GetAwaiter().GetResult();

            var actualFileSet = new FileSet(InMemoryFiles, Path.Combine(TestProjectDir, TestSubDir))
            {
                Files = Enumerable.Repeat(filePaths.ContextFile, 1).Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
            };

            var expectedFileSet = new FileSet(new FileSystemFileService(),
                Path.Combine("ReverseEngineering", "ExpectedResults", "E2E_UseAttributesInsteadOfFluentApi"),
                contents => contents.Replace("namespace " + TestNamespace, "namespace " + TestNamespace + "." + TestSubDir))
            {
                Files = _expectedFiles
            };

            AssertLog(new LoggerMessages
            {
                Warn =
                        {
                            @"For column [dbo][AllDataTypes][hierarchyidColumn]. Could not find type mapping for SQL Server type hierarchyid. Skipping column.",
                            @"For column [dbo][AllDataTypes][sql_variantColumn]. Could not find type mapping for SQL Server type sql_variant. Skipping column.",
                            @"For column [dbo][AllDataTypes][xmlColumn]. Could not find type mapping for SQL Server type xml. Skipping column.",
                            @"For column [dbo][AllDataTypes][geographyColumn]. Could not find type mapping for SQL Server type geography. Skipping column.",
                            @"For column [dbo][AllDataTypes][geometryColumn]. Could not find type mapping for SQL Server type geometry. Skipping column.",
                            @"For column [dbo][PropertyConfiguration][PropertyConfigurationID]. This column is set up as an Identity column, but the SQL Server data type is tinyint. This will be mapped to CLR type byte which does not allow the SqlServerIdentityStrategy.IdentityColumn setting. Generating a matching Property but ignoring the Identity setting.",
                            @"For column [dbo][TableWithUnmappablePrimaryKeyColumn][TableWithUnmappablePrimaryKeyColumnID]. Could not find type mapping for SQL Server type hierarchyid. Skipping column.",
                            @"Unable to identify any primary key columns in the underlying SQL Server table [dbo].[TableWithUnmappablePrimaryKeyColumn]."
                        }
            });
            AssertEqualFileContents(expectedFileSet, actualFileSet);
            AssertCompile(actualFileSet);
        }

        [Fact]
        public void E2ETest_AllFluentApi()
        {
            var configuration = new ReverseEngineeringConfiguration
            {
                ConnectionString = _connectionString,
                ProjectPath = TestProjectDir,
                ProjectRootNamespace = TestNamespace,
                RelativeOutputPath = null, // not used for this test
                UseFluentApiOnly = true,
            };

            var filePaths = Generator.GenerateAsync(configuration).GetAwaiter().GetResult();

            var actualFileSet = new FileSet(InMemoryFiles, TestProjectDir)
            {
                Files = Enumerable.Repeat(filePaths.ContextFile, 1).Concat(filePaths.EntityTypeFiles).Select(Path.GetFileName).ToList()
            };

            var expectedFileSet = new FileSet(new FileSystemFileService(),
                Path.Combine("ReverseEngineering", "ExpectedResults", "E2E_AllFluentApi"))
            {
                Files = _expectedFiles
            };

            AssertLog(new LoggerMessages
            {
                Warn =
                        {
                            @"For column [dbo][AllDataTypes][hierarchyidColumn]. Could not find type mapping for SQL Server type hierarchyid. Skipping column.",
                            @"For column [dbo][AllDataTypes][sql_variantColumn]. Could not find type mapping for SQL Server type sql_variant. Skipping column.",
                            @"For column [dbo][AllDataTypes][xmlColumn]. Could not find type mapping for SQL Server type xml. Skipping column.",
                            @"For column [dbo][AllDataTypes][geographyColumn]. Could not find type mapping for SQL Server type geography. Skipping column.",
                            @"For column [dbo][AllDataTypes][geometryColumn]. Could not find type mapping for SQL Server type geometry. Skipping column.",
                            @"For column [dbo][PropertyConfiguration][PropertyConfigurationID]. This column is set up as an Identity column, but the SQL Server data type is tinyint. This will be mapped to CLR type byte which does not allow the SqlServerIdentityStrategy.IdentityColumn setting. Generating a matching Property but ignoring the Identity setting.",
                            @"For column [dbo][TableWithUnmappablePrimaryKeyColumn][TableWithUnmappablePrimaryKeyColumnID]. Could not find type mapping for SQL Server type hierarchyid. Skipping column.",
                            @"Unable to identify any primary key columns in the underlying SQL Server table [dbo].[TableWithUnmappablePrimaryKeyColumn]."
                        }
            });
            AssertEqualFileContents(expectedFileSet, actualFileSet);
            AssertCompile(actualFileSet);
        }
    }
}

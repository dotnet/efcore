// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Data.Entity.Commands;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Framework.Logging;
using Microsoft.Data.Entity.Relational.Design.Templating;
using Microsoft.Data.Entity.Relational.Design.Templating.Compilation;
using Xunit;
#if DNX451 || DNXCORE50
using Microsoft.Framework.Runtime.Infrastructure;
#endif

namespace EntityFramework.SqlServer.Design.ReverseEngineering.FunctionalTests
{
    public class E2ETests : IClassFixture<E2EFixture>
    {
        public const string E2EConnectionString =
            @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=SqlServerReverseEngineerTestE2E;Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=30";
        private const string TestNamespace = @"E2ETest.Namespace";
        private const string TestOutputDir = @"E2ETest\Output\Dir";
        private static readonly List<string> _E2ETestExpectedWarnings = new List<string>
            {
                @"For columnId [dbo][AllDataTypes][hierarchyidColumn]. Could not find type mapping for SQL Server type hierarchyid. Skipping column.",
                @"For columnId [dbo][AllDataTypes][sql_variantColumn]. Could not find type mapping for SQL Server type sql_variant. Skipping column.",
                @"For columnId [dbo][AllDataTypes][xmlColumn]. Could not find type mapping for SQL Server type xml. Skipping column.",
                @"For columnId [dbo][AllDataTypes][geographyColumn]. Could not find type mapping for SQL Server type geography. Skipping column.",
                @"For columnId [dbo][AllDataTypes][geometryColumn]. Could not find type mapping for SQL Server type geometry. Skipping column.",
                @"For columnId [dbo][TableWithUnmappablePrimaryKeyColumn][TableWithUnmappablePrimaryKeyColumnID]. Could not find type mapping for SQL Server type hierarchyid. Skipping column.",
                @"For columnId [dbo][PropertyConfiguration][PropertyConfigurationID]. The SQL Server data type is tinyint. This will be mapped to CLR type byte which does not allow IdentityStrategy Identity. Generating a matching Property but ignoring the Identity setting.",
                @"Unable to generate EntityType TableWithUnmappablePrimaryKeyColumn. Error message: Attempt to generate EntityType TableWithUnmappablePrimaryKeyColumn failed. Unable to identify any primary key columns in the underlying SQL Server table dbo.TableWithUnmappablePrimaryKeyColumn.",
            };
        private static readonly List<string> _E2ETestExpectedFileNames = new List<string>
            {
                @"SqlServerReverseEngineerTestE2EContext.cs",
                @"AllDataTypes.cs",
                @"OneToManyDependent.cs",
                @"OneToManyPrincipal.cs",
                @"OneToOneDependent.cs",
                @"OneToOnePrincipal.cs",
                @"OneToOneSeparateFKDependent.cs",
                @"OneToOneSeparateFKPrincipal.cs",
                @"PropertyConfiguration.cs",
                @"ReferredToByTableWithUnmappablePrimaryKeyColumn.cs",
                @"SelfReferencing.cs",
                @"TableWithUnmappablePrimaryKeyColumn.cs",
                @"Test_Spaces_Keywords_Table.cs",
            };

        [Fact]
        public void E2ETest()
        {
            // set current cultures to English because expected results for error messages
            // (both those output to the Logger and those put in comments in the .cs files)
            // are in English
#if DNXCORE50
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
#else
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
#endif

            var assembly = Assembly.Load(new AssemblyName(DatabaseTool._defaultReverseEngineeringProviderAssembly));
            var configuration = new ReverseEngineeringConfiguration
            {
                ProviderAssembly = assembly,
                ConnectionString = E2EConnectionString,
                Namespace = TestNamespace,
                OutputPath = TestOutputDir
            };

            var serviceProvider = SetupServiceProvider();
            var fileService = new InMemoryFileService();
            serviceProvider.AddService(typeof(IFileService), fileService);
            var logger = new InMemoryCommandLogger("E2ETest");
            serviceProvider.AddService(typeof(ILogger), logger);

            var expectedFileContents = InitializeExpectedFileContents();

            var generator = new ReverseEngineeringGenerator(serviceProvider);
            var filePaths = generator.GenerateAsync(configuration).Result;

            Assert.Equal(_E2ETestExpectedWarnings.Count, logger.WarningMessages.Count);
            // loop over warnings instead of using the collection form of Assert.Equal()
            // to give better error messages if it does fail. Similarly for file paths below.
            var i = 0;
            foreach (var expectedWarning in _E2ETestExpectedWarnings)
            {
                Assert.Equal(expectedWarning, logger.WarningMessages[i++]);
            }
            Assert.Equal(0, logger.InformationMessages.Count);
            Assert.Equal(0, logger.VerboseMessages.Count);

            var expectedFilePaths = _E2ETestExpectedFileNames.Select(name => TestOutputDir + @"\" + name);
            Assert.Equal(expectedFilePaths.Count(), filePaths.Count);
            i = 0;
            foreach(var expectedFilePath in expectedFilePaths)
            {
                Assert.Equal(expectedFilePath, filePaths[i++]);
            }

            foreach (var fileName in _E2ETestExpectedFileNames)
            {
                var fileContents = fileService.RetrieveFileContents(TestOutputDir, fileName);
                Assert.Equal(expectedFileContents[fileName], fileContents);
            }
        }

        private ServiceProvider SetupServiceProvider()
        {
#if DNX451 || DNXCORE50
            // provides ILibraryManager etc services
            var serviceProvider = new ServiceProvider(
                CallContextServiceLocator.Locator.ServiceProvider);
#else
            var serviceProvider = new ServiceProvider(null);
#endif
            serviceProvider.AddService(typeof(CSharpCodeGeneratorHelper), new CSharpCodeGeneratorHelper());
            serviceProvider.AddService(typeof(ModelUtilities), new ModelUtilities());
            var metadataReferencesProvider = new MetadataReferencesProvider(serviceProvider);
            serviceProvider.AddService(typeof(MetadataReferencesProvider), metadataReferencesProvider);
            var compilationService = new RoslynCompilationService();
            serviceProvider.AddService(typeof(ITemplating), new RazorTemplating(compilationService, metadataReferencesProvider));

            return serviceProvider;
        }

        private Dictionary<string, string> InitializeExpectedFileContents()
        {
            var expectedContents = new Dictionary<string, string>(); ;
            foreach (var fileName in _E2ETestExpectedFileNames)
            {
                expectedContents[fileName] = File.ReadAllText(
                    @"ReverseEngineering\ExpectedResults\E2E\" + fileName.Replace(".cs", ".expected"));
            }

            return expectedContents;
        }
    }
}

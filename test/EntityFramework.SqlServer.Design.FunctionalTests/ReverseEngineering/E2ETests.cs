// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Data.Entity.Relational.Design.Templating.Compilation;
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering;
using Xunit;
using Xunit.Abstractions;
#if DNX451 || DNXCORE50
using System;
using Microsoft.Framework.Runtime;
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

        private readonly ITestOutputHelper _output;

        public E2ETests(ITestOutputHelper output)
        {
            _output = output;
        }

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
#if DNX451 || DNXCORE50
            // provides ILibraryManager etc services
            var serviceCollection = SetupInitialServices(CallContextServiceLocator.Locator.ServiceProvider);
#else
            var serviceCollection = new ServiceCollection();
#endif

            var logger = new InMemoryCommandLogger("E2ETest");
            serviceCollection.AddTransient(typeof(ILogger), sp => logger);
            var fileService = new InMemoryFileService();
            serviceCollection.AddTransient(typeof(IFileService), sp => fileService);

            var designTimeAssembly = Assembly.Load(new AssemblyName("EntityFramework.SqlServer.Design"));
            var type = designTimeAssembly.GetExportedTypes()
                .First(t => t.FullName == "Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.SqlServerMetadataModelProvider");
            var designTimeMetadataProviderFactory = new SqlServerDesignTimeMetadataProviderFactory();
            designTimeMetadataProviderFactory.AddMetadataProviderServices(serviceCollection);
            var provider = designTimeMetadataProviderFactory.Create(serviceCollection);

            var configuration = new ReverseEngineeringConfiguration
            {
                Provider = provider,
                ConnectionString = E2EConnectionString,
                Namespace = TestNamespace,
                OutputPath = TestOutputDir
            };

            var expectedFileContents = InitializeExpectedFileContents();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
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

            var listOfFileContents = new List<string>();
            foreach (var fileName in _E2ETestExpectedFileNames)
            {
                var fileContents = fileService.RetrieveFileContents(TestOutputDir, fileName);
                Assert.Equal(expectedFileContents[fileName], fileContents);
                listOfFileContents.Add(fileContents);
            }

            // compile generated code
            var metadataReferencesProvider =
                (MetadataReferencesProvider)serviceProvider.GetService(typeof(MetadataReferencesProvider));
            var metadataReferences = SetupMetadataReferencesForCompilationOfGeneratedCode(metadataReferencesProvider);
            var roslynCompilationService = new RoslynCompilationService();
            var compilationResult =
                roslynCompilationService.Compile(listOfFileContents, metadataReferences);

            if (compilationResult.Messages.Any())
            {
                _output.WriteLine("Compilation Errors from compiling generated code");
                _output.WriteLine("================================================");
                foreach (var message in compilationResult.Messages)
                {
                    _output.WriteLine(message);
                }
                _output.WriteLine("================================================");
                Assert.Equal(string.Empty, "See Compilation Errors in Output.");
            }
        }

#if DNX451 || DNXCORE50
        private ServiceCollection SetupInitialServices(IServiceProvider serviceProvider)
        {
            var serviceCollection = new ServiceCollection();
            var manifest = serviceProvider.GetRequiredService<IServiceManifest>();
            if (manifest != null)
            {
                foreach (var service in manifest.Services)
                {
                    serviceCollection.AddTransient(
                        service, sp => serviceProvider.GetService(service));
                }
            }
            return serviceCollection;
        }
#endif

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

        private List<MetadataReference> SetupMetadataReferencesForCompilationOfGeneratedCode(
            MetadataReferencesProvider metadataReferencesProvider)
        {
            metadataReferencesProvider.AddReferenceFromName("EntityFramework.Core");
            metadataReferencesProvider.AddReferenceFromName("EntityFramework.Relational");
            metadataReferencesProvider.AddReferenceFromName("EntityFramework.SqlServer");

#if DNXCORE50 || NETCORE50
            metadataReferencesProvider.AddReferenceFromName("System.Data.Common");
            metadataReferencesProvider.AddReferenceFromName("System.Linq.Expressions");
            metadataReferencesProvider.AddReferenceFromName("System.Reflection");

            return metadataReferencesProvider.GetApplicationReferences();
#else
            var metadataReferences = metadataReferencesProvider.GetApplicationReferences();
            metadataReferences.Add(MetadataReference.CreateFromFile(
                Assembly.Load(new AssemblyName(
                    "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location));

            return metadataReferences;
#endif
        }
    }
}

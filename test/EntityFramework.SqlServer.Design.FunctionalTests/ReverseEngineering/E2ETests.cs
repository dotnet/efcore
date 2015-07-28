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
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Infrastructure;
#endif

namespace EntityFramework.SqlServer.Design.ReverseEngineering.FunctionalTests
{
    public class E2ETests : IClassFixture<E2EFixture>
    {
        public const string E2EConnectionString =
            @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=SqlServerReverseEngineerTestE2E;Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=30";
        
        private const string ProviderAssembyName = "EntityFramework.SqlServer.Design";
        private const string ProviderFullClassPath =
            "Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.SqlServerMetadataModelProvider";
        private const string ProviderDbContextTemplateName =
            ProviderAssembyName + "." + ReverseEngineeringGenerator.DbContextTemplateFileName;
        private const string ProviderEntityTypeTemplateName =
            ProviderAssembyName + "." + ReverseEngineeringGenerator.EntityTypeTemplateFileName;
        private const string TestNamespace = "E2ETest.Namespace";
        private const string TestOutputDir = @"E2ETest\Output\Dir";
        private const string CustomizedTemplateDir = @"E2ETest\CustomizedTemplate\Dir";

        private static readonly List<string> _E2ETestExpectedWarnings = new List<string>
            {
                @"For column [dbo][AllDataTypes][hierarchyidColumn]. Could not find type mapping for SQL Server type hierarchyid. Skipping column.",
                @"For column [dbo][AllDataTypes][sql_variantColumn]. Could not find type mapping for SQL Server type sql_variant. Skipping column.",
                @"For column [dbo][AllDataTypes][xmlColumn]. Could not find type mapping for SQL Server type xml. Skipping column.",
                @"For column [dbo][AllDataTypes][geographyColumn]. Could not find type mapping for SQL Server type geography. Skipping column.",
                @"For column [dbo][AllDataTypes][geometryColumn]. Could not find type mapping for SQL Server type geometry. Skipping column.",
                @"For column [dbo][PropertyConfiguration][PropertyConfigurationID]. This column is set up as an Identity column, but the SQL Server data type is tinyint. This will be mapped to CLR type byte which does not allow the SqlServerIdentityStrategy.IdentityColumn setting. Generating a matching Property but ignoring the Identity setting.",
                @"For column [dbo][TableWithUnmappablePrimaryKeyColumn][TableWithUnmappablePrimaryKeyColumnID]. Could not find type mapping for SQL Server type hierarchyid. Skipping column.",
                @"Unable to identify any primary key columns in the underlying SQL Server table [dbo].[TableWithUnmappablePrimaryKeyColumn].",
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

        private const string CustomDbContextTemplateContents =
            "This is the output from a customized DbContextTemplate";
        private const string CustomEntityTypeTemplateContents =
            "This is the output from a customized EntityTypeTemplate";
        private static readonly List<string> _CustomizedTemplatesTestExpectedInfos =
            new List<string>
            {
                "Using custom template " + Path.Combine(CustomizedTemplateDir, ProviderDbContextTemplateName),
                "Using custom template " + Path.Combine(CustomizedTemplateDir, ProviderEntityTypeTemplateName),
            };

        private readonly ITestOutputHelper _output;

        public E2ETests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void E2ETest()
        {
            SetCurrentCulture();

            var serviceCollection = SetupInitialServices();
            var logger = new InMemoryCommandLogger("E2ETest");
            serviceCollection.AddScoped(typeof(ILogger), sp => logger);
            var fileService = new InMemoryFileService();
            serviceCollection.AddScoped(typeof(IFileService), sp => fileService);

            var provider = GetMetadataModelProvider(serviceCollection);

            var configuration = new ReverseEngineeringConfiguration
            {
                Provider = provider,
                ConnectionString = E2EConnectionString,
                Namespace = TestNamespace,
                CustomTemplatePath = null, // not used for this test
                OutputPath = TestOutputDir
            };

            var expectedFileContents = InitializeE2EExpectedFileContents();

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

            var expectedFilePaths = _E2ETestExpectedFileNames.Select(name => Path.Combine(TestOutputDir, name));
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

        [Fact]
        public void Code_generation_will_use_customized_templates_if_present()
        {
            SetCurrentCulture();

            var serviceCollection = SetupInitialServices();
            var logger = new InMemoryCommandLogger("E2ETest");
            serviceCollection.AddScoped(typeof(ILogger), sp => logger);
            var fileService = new InMemoryFileService();
            serviceCollection.AddScoped(typeof(IFileService), sp => fileService);
            InitializeCustomizedTemplates(fileService);

            var provider = GetMetadataModelProvider(serviceCollection);

            var configuration = new ReverseEngineeringConfiguration
            {
                Provider = provider,
                ConnectionString = E2EConnectionString,
                Namespace = TestNamespace,
                CustomTemplatePath = CustomizedTemplateDir,
                OutputPath = TestOutputDir
            };

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

            Assert.Equal(_CustomizedTemplatesTestExpectedInfos.Count, logger.InformationMessages.Count);
            i = 0;
            foreach (var expectedInfo in _CustomizedTemplatesTestExpectedInfos)
            {
                Assert.Equal(expectedInfo, logger.InformationMessages[i++]);
            }

            Assert.Equal(0, logger.VerboseMessages.Count);

            var expectedFilePaths = _E2ETestExpectedFileNames.Select(name => Path.Combine(TestOutputDir, name));
            Assert.Equal(expectedFilePaths.Count(), filePaths.Count);
            i = 0;
            foreach (var expectedFilePath in expectedFilePaths)
            {
                Assert.Equal(expectedFilePath, filePaths[i++]);
            }

            var listOfFileContents = new List<string>();
            foreach (var fileName in _E2ETestExpectedFileNames)
            {
                var fileContents = fileService.RetrieveFileContents(TestOutputDir, fileName);
                if ("SqlServerReverseEngineerTestE2EContext.cs" == fileName)
                {
                    Assert.Equal(CustomDbContextTemplateContents, fileContents);
                }
                else
                {
                    Assert.Equal(CustomEntityTypeTemplateContents, fileContents);
                }
            }
        }

        [Fact]
        public void Can_output_templates_to_be_customized()
        {
            var serviceCollection = SetupInitialServices();
            var logger = new InMemoryCommandLogger("E2ETest");
            serviceCollection.AddScoped(typeof(ILogger), sp => logger);
            var fileService = new InMemoryFileService();
            serviceCollection.AddScoped(typeof(IFileService), sp => fileService);

            var provider = GetMetadataModelProvider(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
            var filePaths = generator.Customize(provider, TestOutputDir);

            Assert.Equal(0, logger.WarningMessages.Count);
            Assert.Equal(0, logger.InformationMessages.Count);
            Assert.Equal(0, logger.VerboseMessages.Count);
            Assert.Equal(2, filePaths.Count);
            Assert.Equal(Path.Combine(TestOutputDir, ProviderDbContextTemplateName), filePaths[0]);
            Assert.Equal(Path.Combine(TestOutputDir, ProviderEntityTypeTemplateName), filePaths[1]);

            var dbContextTemplateContents = fileService.RetrieveFileContents(
                TestOutputDir, ProviderDbContextTemplateName);
            Assert.Equal(provider.DbContextTemplate, dbContextTemplateContents);

            var entityTypeTemplateContents = fileService.RetrieveFileContents(
                TestOutputDir, ProviderEntityTypeTemplateName);
            Assert.Equal(provider.EntityTypeTemplate, entityTypeTemplateContents);
        }

        private ServiceCollection SetupInitialServices()
        {
            var serviceCollection = new ServiceCollection();
#if DNX451 || DNXCORE50
            // provides ILibraryManager etc services
            var manifest = CallContextServiceLocator.Locator.ServiceProvider.GetRequiredService<IRuntimeServices>();
            if (manifest != null)
            {
                foreach (var service in manifest.Services)
                {
                    serviceCollection.AddTransient(
                        service, serviceProvider => serviceProvider.GetService(service));
                }
            }
#endif
            return serviceCollection;
        }

        private Dictionary<string, string> InitializeE2EExpectedFileContents()
        {
            var expectedContents = new Dictionary<string, string>(); ;
            foreach (var fileName in _E2ETestExpectedFileNames)
            {
                expectedContents[fileName] = File.ReadAllText(
                    @"ReverseEngineering\ExpectedResults\E2E\" + fileName.Replace(".cs", ".expected"));
            }

            return expectedContents;
        }

        private void InitializeCustomizedTemplates(InMemoryFileService fileService)
        {
            fileService.OutputFile(CustomizedTemplateDir, ProviderDbContextTemplateName, CustomDbContextTemplateContents);
            fileService.OutputFile(CustomizedTemplateDir, ProviderEntityTypeTemplateName, CustomEntityTypeTemplateContents);
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

        private IDatabaseMetadataModelProvider GetMetadataModelProvider(IServiceCollection serviceCollection)
        {
            var designTimeAssembly = Assembly.Load(new AssemblyName(ProviderAssembyName));
            var type = designTimeAssembly.GetType(ProviderFullClassPath);
            var designTimeMetadataProviderFactory = new SqlServerDesignTimeMetadataProviderFactory();
            designTimeMetadataProviderFactory.AddMetadataProviderServices(serviceCollection);
            return serviceCollection.BuildServiceProvider().GetRequiredService<IDatabaseMetadataModelProvider>();
        }

        private void SetCurrentCulture()
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
        }
    }
}

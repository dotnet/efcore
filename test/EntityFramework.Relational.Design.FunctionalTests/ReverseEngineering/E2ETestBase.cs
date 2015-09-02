// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering
{
    public abstract class E2ETestBase
    {
        private readonly ITestOutputHelper _output;
        private InMemoryCommandLogger _logger;

        protected InMemoryFileService InMemoryFiles;
        protected readonly ReverseEngineeringGenerator Generator;
        protected readonly IDatabaseMetadataModelProvider MetadataModelProvider;

        public E2ETestBase(ITestOutputHelper output)
        {
            _output = output;

            var serviceCollection = new ServiceCollection()
                .AddScoped(typeof(ILogger), sp => _logger = new InMemoryCommandLogger("E2ETest"))
                .AddScoped(typeof(IFileService), sp => InMemoryFiles = new InMemoryFileService());

            GetFactory().AddMetadataProviderServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
            MetadataModelProvider = serviceProvider.GetRequiredService<IDatabaseMetadataModelProvider>();

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

        protected abstract E2ECompiler GetCompiler();
        protected abstract string ProviderName { get; }
        protected abstract IDesignTimeMetadataProviderFactory GetFactory();

        protected virtual void AssertEqualFileContents(FileSet expected, FileSet actual)
        {
            Assert.Equal(expected.Files.Count, actual.Files.Count);

            for (var i = 0; i < expected.Files.Count; i++)
            {
                Assert.True(actual.Exists(i), $"Could not find file '{actual.Files[i]}' in directory '{actual.Directory}'");
                try
                {
                    Assert.Equal(expected.Contents(i), actual.Contents(i));
                }
                catch (EqualException e)
                {
                    throw new XunitException($"Files did not match: '{expected.Files[i]}' and '{actual.Files[i]}'" + Environment.NewLine + $"{e.Message}");
                }
            }
        }

        protected virtual void AssertLog(LoggerMessages expected)
        {
            Assert.Equal(expected.Warn, _logger.Messages.Warn);
            Assert.Equal(expected.Info, _logger.Messages.Info);
            Assert.Equal(expected.Verbose, _logger.Messages.Verbose);
        }

        protected virtual void AssertCompile(FileSet fileSet)
        {
            var fileContents = fileSet.Files.Select(fileSet.Contents).ToList();

            var compilationResult = GetCompiler().Compile(fileContents);

            if (compilationResult.Messages.Any())
            {
                _output.WriteLine("Compilation Errors from compiling generated code");
                _output.WriteLine("================================================");
                foreach (var message in compilationResult.Messages)
                {
                    _output.WriteLine(message);
                }
                _output.WriteLine("================================================");
                Assert.True(false, "Failed to compile: see Compilation Errors in Output.");
            }
        }
    }
}

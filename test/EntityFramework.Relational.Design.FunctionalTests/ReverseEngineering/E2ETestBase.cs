// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
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
                .AddLogging();
            GetFactory().AddMetadataProviderServices(serviceCollection);
            serviceCollection.AddSingleton(typeof(IFileService), sp => InMemoryFiles = new InMemoryFileService());

            var serviceProvider = serviceCollection.BuildServiceProvider();

            _logger = new InMemoryCommandLogger("E2ETest", _output);
            serviceProvider.GetService<ILoggerFactory>().AddProvider(new TestLoggerProvider(_logger));

            Generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
            MetadataModelProvider = serviceProvider.GetRequiredService<IDatabaseMetadataModelProvider>();
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
                var expectedContents = expected.Contents(i);
                var actualContents = actual.Contents(i);

                try
                {
                    Assert.Equal(expectedContents, actualContents);
                }
                catch (EqualException e)
                {
                    var sep = new string('=', 60);
                    _output.WriteLine($"Contents of actual: '{actual.Files[i]}'");
                    _output.WriteLine(sep);
                    _output.WriteLine(actualContents);
                    _output.WriteLine(sep);

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

        private class TestLoggerProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public TestLoggerProvider(ILogger logger)
            {
                _logger = logger;
            }

            public ILogger CreateLogger(string name) => _logger;

            public void Dispose()
            {
            }
        }
    }
}

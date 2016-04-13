// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Relational.Design.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Relational.Design.FunctionalTests.ReverseEngineering
{
    public abstract class E2ETestBase
    {
        private readonly ITestOutputHelper _output;
        protected InMemoryCommandLogger _logger;
        protected InMemoryFileService InMemoryFiles;
        protected readonly ReverseEngineeringGenerator Generator;
        protected readonly IScaffoldingModelFactory ScaffoldingModelFactory;

        protected E2ETestBase(ITestOutputHelper output)
        {
            _output = output;

            var serviceProvider = ConfigureDesignTimeServices(
                new ServiceCollection()
                    .AddScaffolding()
                    .AddLogging())
                .AddSingleton(typeof(IFileService), sp => InMemoryFiles = new InMemoryFileService()).BuildServiceProvider();

            _logger = new InMemoryCommandLogger("E2ETest", _output);
            serviceProvider.GetService<ILoggerFactory>().AddProvider(new TestLoggerProvider(_logger));

            Generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
            ScaffoldingModelFactory = serviceProvider.GetRequiredService<IScaffoldingModelFactory>();
        }

        protected abstract ICollection<BuildReference> References { get; }
        protected abstract string ProviderName { get; }

        protected abstract IServiceCollection ConfigureDesignTimeServices(IServiceCollection services);

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
            AssertLoggerMessages(expected.Warn, _logger.Messages.Warn, "WARNING");
            AssertLoggerMessages(expected.Error, _logger.Messages.Error, "ERROR");
            AssertLoggerMessages(expected.Info, _logger.Messages.Info, "INFO");
        }

        protected virtual void AssertLoggerMessages(
            List<string> expected, List<string> actual, string category)
        {
            try
            {
                foreach (var message in expected)
                {
                    Assert.Contains(message, actual);
                }

                Assert.Equal(expected.Count, actual.Count);
            }
            catch (Exception)
            {
                var sep = new string('=', 60);
                _output.WriteLine($"Contents of {category} logger messages:");
                _output.WriteLine(sep);
                _output.WriteLine(string.Join(Environment.NewLine, actual));
                _output.WriteLine(sep);

                throw;
            }
        }

        protected virtual void AssertCompile(FileSet fileSet)
        {
            var fileContents = fileSet.Files.Select(fileSet.Contents).ToList();

            var source = new BuildSource
            {
                Sources = fileContents
            };
            foreach (var r in References)
            {
                source.References.Add(r);
            }
            source.BuildInMemory();
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

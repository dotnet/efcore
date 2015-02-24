// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering
{
    public class ReverseEngineeringGeneratorTest
    {
        [Fact]
        public void Generator_throws_if_no_Logger_service()
        {
            var serviceCollection = new ServiceCollection();
            var exception = Assert.Throws<InvalidOperationException>(
                () => new ReverseEngineeringGenerator(serviceCollection.BuildServiceProvider()));
            Assert.Equal("No service for type 'Microsoft.Framework.Logging.ILogger' has been registered.",
                exception.Message);
        }

        [Fact]
        public void Generator_throws_if_no_CSharpCodeGeneratorHelper_service()
        {
            var serviceCollection = new ServiceCollection()
                .AddScoped(typeof(ILogger), typeof(TestLogger));
            
            var exception = Assert.Throws<InvalidOperationException>(
                () => new ReverseEngineeringGenerator(serviceCollection.BuildServiceProvider()));
            Assert.Equal("No service for type 'Microsoft.Data.Entity.Relational.Design.CodeGeneration.CSharpCodeGeneratorHelper' has been registered.",
                exception.Message);
        }

        [Fact]
        public void Generator_does_not_throw_if_correct_services_available()
        {
            new ReverseEngineeringGenerator(CreateServiceProvider());
        }

        [Fact]
        public void Default_FileExtension_is_CSharp()
        {
            var revEngGenerator = new ReverseEngineeringGenerator(CreateServiceProvider());
            Assert.Equal(".cs", revEngGenerator.FileExtension);
        }

        [Fact]
        public void GetMetadataModel_returns_the_model_from_the_provider()
        {
            var revEngGenerator = new ReverseEngineeringGenerator(CreateServiceProvider());

            var testProvider = new TestDatabaseMetadataModelProvider();
            var configuration = new ReverseEngineeringConfiguration()
            {
                ConnectionString = "test connection string"
            };

            var metadataModel = revEngGenerator.GetMetadataModel(testProvider, configuration);
            Assert.Same(testProvider.Model, metadataModel);
        }

        [Fact]
        public void Generate_outputs_to_correct_file_names()
        {
            var expectedResult =
                "Outputting to file TestContext.cs" + Environment.NewLine
                + "Outputting to file EntityA.cs" + Environment.NewLine
                + "Outputting to file EntityB.cs" + Environment.NewLine;

            var result = new StringBuilder();

            var mockRevEngGenerator = new Mock<ReverseEngineeringGenerator>(CreateServiceProvider());
            mockRevEngGenerator.CallBase = true;
            mockRevEngGenerator
                .Setup(m => m.OutputFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>(
                    (dir, fileName, contents) => result.AppendLine("Outputting to file " + fileName));

            var testProvider = new TestDatabaseMetadataModelProvider();
            var configuration = new ReverseEngineeringConfiguration()
            {
                ContextClassName = "TestContext",
                ConnectionString = "test connection string",
                Namespace = "Test.Namespace",
                OutputPath = "TestOutputPath",
                Provider = testProvider
            };

            mockRevEngGenerator.Object.Generate(configuration);
            Assert.Equal(expectedResult, result.ToString());
        }

        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection()
                .AddScoped(typeof(ILogger), typeof(TestLogger))
                .AddScoped(typeof(CSharpCodeGeneratorHelper), typeof(CSharpCodeGeneratorHelper));
            return services.BuildServiceProvider();
        }
    }

    public class TestLogger : ILogger
    {
        public IDisposable BeginScope(object state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Write(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            // do nothing
        }
    }

}
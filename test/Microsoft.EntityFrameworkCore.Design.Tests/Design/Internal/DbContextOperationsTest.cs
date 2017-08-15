// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class DbContextOperationsTest
    {
        [Fact]
        public void CreateContext_get_service()
        {
            var assembly = MockAssembly.Create(typeof(Startup));
            var operations = new DbContextOperations(
                new TestOperationReporter(),
                assembly,
                assembly,
                "Environment1",
                @"X:\ContentRoot1");

            operations.CreateContext(typeof(TestContext).FullName);
        }

        private class Startup
        {
            public void ConfigureServices(IServiceCollection services)
                => services.AddDbContext<TestContext>();
        }

        private class TestContext : DbContext
        {
            public TestContext()
            {
                throw new Exception("This isn't the constructor you're looking for.");
            }

            public TestContext(DbContextOptions<TestContext> options)
                : base(options)
            {
            }
        }

        private class TestOperationReporter : IOperationReporter
        {
            public void WriteError(string message)
            {
            }

            public void WriteInformation(string message)
            {
            }

            public void WriteVerbose(string message)
            {
            }

            public void WriteWarning(string message)
            {
            }
        }
    }
}

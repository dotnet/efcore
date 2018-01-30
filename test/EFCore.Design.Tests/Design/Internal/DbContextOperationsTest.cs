// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class DbContextOperationsTest
    {
        [Fact]
        public void CreateContext_gets_service()
        {
            var assembly = MockAssembly.Create(typeof(TestProgram), typeof(TestContext));
            var operations = new TestDbContextOperations(
                new TestOperationReporter(),
                assembly,
                assembly,
                /* args: */ Array.Empty<string>(),
                new TestAppServiceProviderFactory(assembly, typeof(TestProgram)));

            operations.CreateContext(typeof(TestContext).FullName);
        }

        private class TestProgram
        {
            private static TestWebHost BuildWebHost(string[] args)
                => new TestWebHost(
                    new ServiceCollection()
                        .AddDbContext<TestContext>(b => b.UseInMemoryDatabase(Guid.NewGuid().ToString()))
                        .BuildServiceProvider());
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
    }
}

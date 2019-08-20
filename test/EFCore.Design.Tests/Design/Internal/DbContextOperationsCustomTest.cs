// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class DbContextOperationsCustomTest
    {
        [ConditionalFact]
        public void CreateContext_with_custom_constructor_works()
        {
            var assembly = MockAssembly.Create(typeof(TestProgram), typeof(TestContextWithCustomConstructor));
            var operations = new TestDbContextOperations(
                new TestOperationReporter(),
                assembly,
                assembly,
                /* args: */ Array.Empty<string>(),
                new TestAppServiceProviderFactory(assembly, typeof(TestProgram)));

            var result = operations.CreateContext(typeof(TestContextWithCustomConstructor).FullName);

            Assert.IsType<TestContextWithCustomConstructor>(result);
        }

        private class TestContextWithCustomConstructor : DbContext
        {
            public TestContextWithCustomConstructor(string connectionString)
                : base(new DbContextOptionsBuilder<TestContextWithCustomConstructor>().UseInMemoryDatabase(connectionString).Options)
            {
            }
        }

        private static class TestProgram
        {
#pragma warning disable RCS1213 // Remove unused member declaration.
            private static TestWebHost BuildWebHost(string[] args)
#pragma warning restore RCS1213 // Remove unused member declaration.
                => new TestWebHost(
                    new ServiceCollection()
                    .AddTransient((_) => new TestContextWithCustomConstructor(nameof(DbContextActivatorTest)))
                    .BuildServiceProvider());
        }
    }
}

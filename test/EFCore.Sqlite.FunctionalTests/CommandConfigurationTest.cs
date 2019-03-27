// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class CommandConfigurationTest : IClassFixture<CommandConfigurationTest.CommandConfigurationTestFixture>
    {
        public CommandConfigurationTest(CommandConfigurationTestFixture fixture) => Fixture = fixture;

        protected CommandConfigurationTestFixture Fixture { get; }

        [Fact]
        public void Constructed_select_query_CommandBuilder_throws_when_negative_CommandTimeout_is_used()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<ArgumentException>(() => context.Database.SetCommandTimeout(-5));
            }
        }

        protected DbContext CreateContext() => Fixture.CreateContext();

        public class CommandConfigurationTestFixture : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "Empty";
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
        }
    }
}

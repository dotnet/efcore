// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class WarningsTest : IClassFixture<InMemoryFixture>
    {
        [Fact]
        public void Should_throw_by_default_when_transaction()
        {
            var optionsBuilder
                = new DbContextOptionsBuilder()
                    .UseInMemoryDatabase()
                    .UseInternalServiceProvider(_fixture.ServiceProvider);

            using (var context = new DbContext(optionsBuilder.Options))
            {
                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        $"{nameof(InMemoryEventId)}.{nameof(InMemoryEventId.TransactionIgnoredWarning)}",
                        InMemoryStrings.TransactionsNotSupported),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Database.BeginTransaction()).Message);
            }
        }

        [Fact]
        public void Should_not_throw_by_default_when_transaction_and_ignored()
        {
            var optionsBuilder
                = new DbContextOptionsBuilder()
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .UseInMemoryDatabase()
                    .UseInternalServiceProvider(_fixture.ServiceProvider);

            using (var context = new DbContext(optionsBuilder.Options))
            {
                context.Database.BeginTransaction();
            }
        }

        private readonly InMemoryFixture _fixture;

        public WarningsTest(InMemoryFixture fixture)
        {
            _fixture = fixture;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.UpdatesModel;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class UpdatesInMemoryFixture : UpdatesFixtureBase<InMemoryTestStore>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptionsBuilder _optionsBuilder;

        public UpdatesInMemoryFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();

            _optionsBuilder = new DbContextOptionsBuilder()
                .UseInMemoryDatabase()
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .UseInternalServiceProvider(_serviceProvider);
        }

        public override InMemoryTestStore CreateTestStore()
            => InMemoryTestStore.CreateScratch(
                () =>
                    {
                        using (var context = new UpdatesContext(_optionsBuilder.Options))
                        {
                            UpdatesModelInitializer.Seed(context);
                        }
                    },
                _serviceProvider);

        public override UpdatesContext CreateContext(InMemoryTestStore testStore)
            => new UpdatesContext(_optionsBuilder.Options);
    }
}

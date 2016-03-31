// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.Storage.Internal;
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
                .BuildServiceProvider();

            _optionsBuilder = new DbContextOptionsBuilder()
                .UseInMemoryDatabase()
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
                () => { _serviceProvider.GetRequiredService<IInMemoryStore>().Clear(); });

        public override UpdatesContext CreateContext(InMemoryTestStore testStore)
            => new UpdatesContext(_optionsBuilder.Options);
    }
}

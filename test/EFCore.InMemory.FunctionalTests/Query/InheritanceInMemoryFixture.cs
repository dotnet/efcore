// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceInMemoryFixture : InheritanceFixtureBase<InMemoryTestStore>
    {
        protected virtual string DatabaseName => "InheritanceInMemoryTest";

        private readonly DbContextOptions _options;

        public InheritanceInMemoryFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider(validateScopes: true);

            _options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(DatabaseName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override InMemoryTestStore CreateTestStore()
            => InMemoryTestStore.GetOrCreateShared(
                DatabaseName,
                () =>
                    {
                        using (var context = new InheritanceContext(_options))
                        {
                            InheritanceModelInitializer.SeedData(context);
                        }
                    });

        public override InheritanceContext CreateContext(InMemoryTestStore testStore)
            => new InheritanceContext(_options);
    }
}

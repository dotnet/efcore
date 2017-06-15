// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceInMemoryFixture : InheritanceFixtureBase<InMemoryTestStore>
    {
        private const string DatabaseName = "InheritanceInMemoryTest";

        public override InMemoryTestStore CreateTestStore()
            => InMemoryTestStore.CreateScratch(
                () =>
                    {
                        using (var context = CreateContext())
                        {
                            SeedData(context);
                        }
                    },
                () =>
                    {
                        using (var context = CreateContext())
                        {
                            context.GetService<IInMemoryStoreCache>().GetStore(DatabaseName).Clear();
                        }
                    });

        public override DbContextOptions BuildOptions()
            => new DbContextOptionsBuilder()
                .UseInMemoryDatabase(DatabaseName)
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider())
                .Options;
    }
}

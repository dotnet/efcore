// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class DataAnnotationInMemoryFixture : DataAnnotationFixtureBase<InMemoryTestStore>
    {
        public static readonly string DatabaseName = "DataAnnotations";

        private readonly DbContextOptions _options;

        public DataAnnotationInMemoryFixture()
        {
            _options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(nameof(DataAnnotationInMemoryFixture))
                .UseInternalServiceProvider(new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider())
                .ConfigureWarnings(w =>
                    {
                        w.Default(WarningBehavior.Throw);
                        w.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                    })
                .Options;
        }

        public override InMemoryTestStore CreateTestStore()
            => InMemoryTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = new DataAnnotationContext(_options))
                    {
                        DataAnnotationModelInitializer.Seed(context);
                    }
                });

        public override DataAnnotationContext CreateContext(InMemoryTestStore testStore)
            => new DataAnnotationContext(_options);
    }
}

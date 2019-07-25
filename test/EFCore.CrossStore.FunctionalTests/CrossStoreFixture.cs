// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class CrossStoreFixture : FixtureBase
    {
        protected virtual string StoreName { get; } = "CrossStoreTest";

        public DbContextOptions CreateOptions(TestStore testStore)
            => AddOptions(testStore.AddProviderOptions(new DbContextOptionsBuilder()))
                .UseInternalServiceProvider(testStore.ServiceProvider)
                .Options;

        public CrossStoreContext CreateContext(TestStore testStore)
            => new CrossStoreContext(CreateOptions(testStore));

        public TestStore CreateTestStore(ITestStoreFactory testStoreFactory)
        {
            return testStoreFactory.GetOrCreate(StoreName)
                .Initialize(
                    AddServices(testStoreFactory.AddProviderServices(new ServiceCollection()))
                        .BuildServiceProvider(validateScopes: true), CreateContext, c => { }, null);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<SimpleEntity>(
                eb =>
                {
                    eb.ToTable("RelationalSimpleEntity");
                    eb.Property(typeof(string), SimpleEntity.ShadowPropertyName);
                    eb.HasKey(e => e.Id);
                    eb.Property(e => e.Id).UseIdentityColumn();
                });
        }
    }
}

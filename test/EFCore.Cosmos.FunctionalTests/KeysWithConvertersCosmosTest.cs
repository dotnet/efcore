// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class KeysWithConvertersCosmosTest(KeysWithConvertersCosmosTest.KeysWithConvertersCosmosFixture fixture)
    : KeysWithConvertersTestBase<KeysWithConvertersCosmosTest.KeysWithConvertersCosmosFixture>(fixture)
{
    public class KeysWithConvertersCosmosFixture : KeysWithConvertersFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public override bool UseInclude
            => false;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.HasRootDiscriminatorInJsonId();
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(
                builder.ConfigureWarnings(
                    w => w.Ignore(
                        CoreEventId.MappedEntityTypeIgnoredWarning,
                        CosmosEventId.NoPartitionKeyDefined,
                        CoreEventId.CollectionWithoutComparer)));
    }
}

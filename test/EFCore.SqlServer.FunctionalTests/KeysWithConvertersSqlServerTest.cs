// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class KeysWithConvertersSqlServerTest : KeysWithConvertersTestBase<
        KeysWithConvertersSqlServerTest.KeysWithConvertersSqlServerFixture>
    {
        public KeysWithConvertersSqlServerTest(KeysWithConvertersSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class KeysWithConvertersSqlServerFixture : KeysWithConvertersFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => builder.UseSqlServer(b => b.MinBatchSize(1));

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<IntWrappedKeyIdentity>(
                    b =>
                    {
                        b.Property(e => e.Id).UseIdentityColumn();
                        b.HasKey(e => e.Id);
                    });

                modelBuilder.Entity<IntWrappedKeyHiLo>(
                    b =>
                    {
                        b.Property(e => e.Id).UseHiLo();
                    });

                modelBuilder.Entity<IntWrappedKeyIdentity>(
                    b =>
                    {
                        b.Property(e => e.Id);
                    });

                modelBuilder.Entity<LongWrappedKeyIdentity>(
                    b =>
                    {
                        b.Property(e => e.Id).UseIdentityColumn();
                        b.HasKey(e => e.Id);
                    });

                modelBuilder.Entity<LongWrappedKeyHiLo>(
                    b =>
                    {
                        b.Property(e => e.Id).UseHiLo();
                    });

                modelBuilder.Entity<LongWrappedKeyIdentity>(
                    b =>
                    {
                        b.Property(e => e.Id);
                    });

                modelBuilder.Entity<ShortWrappedKeyIdentity>(
                    b =>
                    {
                        b.Property(e => e.Id).UseIdentityColumn();
                        b.HasKey(e => e.Id);
                    });

                modelBuilder.Entity<ShortWrappedKeyHiLo>(
                    b =>
                    {
                        b.Property(e => e.Id).UseHiLo();
                    });

                modelBuilder.Entity<ShortWrappedKeyIdentity>(
                    b =>
                    {
                        b.Property(e => e.Id);
                    });
            }
        }

        // TODO Delete these overrides
        public override void Can_insert_and_read_back_with_wrapped_int_key_identity()
        {
            base.Can_insert_and_read_back_with_wrapped_int_key_identity();
        }

        public override void Can_insert_and_read_back_with_wrapped_int_key_high_low()
        {
            base.Can_insert_and_read_back_with_wrapped_int_key_high_low();
        }

        public override void Can_insert_and_read_back_with_wrapped_int_key_client_gen()
        {
            base.Can_insert_and_read_back_with_wrapped_int_key_client_gen();
        }
    }
}

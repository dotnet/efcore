// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class RedisBuiltInDataTypesFixture : BuiltInDataTypesFixtureBase<RedisTestStore>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        public RedisBuiltInDataTypesFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddRedis()
                    .ServiceCollection
                    .BuildServiceProvider();
        }

        public override RedisTestStore CreateTestStore()
        {
            var store = new RedisTestStore();
            using (var context = CreateContext(store))
            {
                context.Database.EnsureCreated();
            }

            store.CleanupAction = () =>
            {
                using (var context = CreateContext(store))
                {
                    Cleanup(context);
                }
            };

            return store;
        }

        public override DbContext CreateContext(RedisTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseRedis("127.0.0.1", RedisTestConfig.RedisPort);

            return new DbContext(_serviceProvider, options);
        }

        public void Dispose()
        {
            var testStore = CreateTestStore();
            using (var context = CreateContext(testStore))
            {
                context.Database.EnsureDeleted();
            }
        }

        public override void OnModelCreating(BasicModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BuiltInNonNullableDataTypes>(b =>
            {
                b.Property(dt => dt.TestInt16);
                b.Property(dt => dt.TestUnsignedInt16);
                b.Property(dt => dt.TestUnsignedInt32);
                b.Property(dt => dt.TestUnsignedInt64);
                b.Property(dt => dt.TestCharacter);
                b.Property(dt => dt.TestSignedByte);
            });

            modelBuilder.Entity<BuiltInNullableDataTypes>(b =>
            {
                b.Property(dt => dt.TestNullableInt16);
                b.Property(dt => dt.TestNullableUnsignedInt16);
                b.Property(dt => dt.TestNullableUnsignedInt32);
                b.Property(dt => dt.TestNullableUnsignedInt64);
                b.Property(dt => dt.TestNullableCharacter);
                b.Property(dt => dt.TestNullableSignedByte);
            });
        }
    }
}

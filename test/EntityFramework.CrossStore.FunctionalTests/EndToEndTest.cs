// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.AzureTableStorage.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.InMemory.FunctionalTests;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Redis.FunctionalTests;
using Microsoft.Data.Entity.SQLite.FunctionalTests;
using Microsoft.Data.Entity.SqlServer.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class EndToEndTest<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : CrossStoreFixture<TTestStore>, new()
    {
        [Fact]
        public void Can_save_changes_and_query()
        {
            using (var context = CreateContext())
            {
                var first = context.SimpleEntities.Add(new SimpleEntity { StringProperty = "Entity 1" });
                SetPartitionId(first, context);
                 
                Assert.Equal(1, context.SaveChanges());

                var second = context.SimpleEntities.Add(new SimpleEntity { Id = 42, StringProperty = "Entity 2"});
                // TODO: Replace with
                // context.ChangeTracker.Entry(entity).Property(SimpleEntity.ShadowPropertyName).CurrentValue = "shadow";
                var property = context.Model.GetEntityType(typeof(SimpleEntity)).GetProperty(SimpleEntity.ShadowPropertyName);
                context.ChangeTracker.Entry(second).StateEntry[property] = "shadow";
                SetPartitionId(second, context);

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                Assert.Equal(2, context.SimpleEntities.Count());

                var firstEntity = context.SimpleEntities.Single(e => e.StringProperty == "Entity 1");

                var secondEntity = context.SimpleEntities.Single(e => e.Id == 42);
                Assert.Equal("Entity 2", secondEntity.StringProperty);
                Assert.Same(secondEntity, context.SimpleEntities.Single(e => e.Property<string>(SimpleEntity.ShadowPropertyName) == "shadow"));

                firstEntity.StringProperty = "first";
                context.SimpleEntities.Remove(secondEntity);

                Assert.Equal(2, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                Assert.Equal("first", context.SimpleEntities.Single().StringProperty);

                context.SimpleEntities.RemoveRange(context.SimpleEntities);
                context.SaveChanges();
            }
        }

        // TODO: Use a value generator to handle this automatically
        private void SetPartitionId(SimpleEntity entity, CrossStoreContext context)
        {
            var property = context.Model.GetEntityType(entity.GetType()).GetProperty(SimpleEntity.ShadowPartitionIdName);
            context.ChangeTracker.Entry(entity).StateEntry[property] = "Partition";
        }

        protected EndToEndTest(TFixture fixture)
        {
            Fixture = fixture;
            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; private set; }

        protected TTestStore TestStore { get; private set; }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        protected CrossStoreContext CreateContext()
        {
            return Fixture.CreateContext(TestStore);
        }
    }

    [RunIfConfigured]
    public class AtsEndToEndTest : EndToEndTest<AtsTestStore, AtsCrossStoreFixture>
    {
        public AtsEndToEndTest(AtsCrossStoreFixture fixture)
            : base(fixture)
        {
        }
    }

    public class InMemoryEndToEndTest : EndToEndTest<InMemoryTestStore, InMemoryCrossStoreFixture>
    {
        public InMemoryEndToEndTest(InMemoryCrossStoreFixture fixture)
            : base(fixture)
        {
        }
    }

    public class RedisEndToEndTest : EndToEndTest<RedisTestStore, RedisCrossStoreFixture>
    {
        public RedisEndToEndTest(RedisCrossStoreFixture fixture)
            : base(fixture)
        {
        }
    }

    public class SqLiteEndToEndTest : EndToEndTest<SqLiteTestStore, SqLiteCrossStoreFixture>
    {
        public SqLiteEndToEndTest(SqLiteCrossStoreFixture fixture)
            : base(fixture)
        {
        }
    }

    public class SqlServerEndToEndTest : EndToEndTest<SqlServerTestStore, SqlServerCrossStoreFixture>
    {
        public SqlServerEndToEndTest(SqlServerCrossStoreFixture fixture)
            : base(fixture)
        {
        }
    }
}

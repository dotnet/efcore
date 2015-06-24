// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory.FunctionalTests;
using Microsoft.Data.Entity.Sqlite.FunctionalTests;
using Microsoft.Data.Entity.SqlServer.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class EndToEndTest<TTestStore, TFixture> : IDisposable
        where TTestStore : TestStore
        where TFixture : CrossStoreFixture, new()
    {
        [Fact]
        public virtual void Can_save_changes_and_query()
        {
            using (var context = CreateContext())
            {
                var first = context.SimpleEntities.Add(new SimpleEntity { Id = 420, StringProperty = "Entity 1" }).Entity;
                SetPartitionId(first, context);

                Assert.Equal(1, context.SaveChanges());

                var second = context.SimpleEntities.Add(new SimpleEntity { Id = 42, StringProperty = "Entity 2" }).Entity;
                // TODO: Replace with
                // context.ChangeTracker.Entry(entity).Property(SimpleEntity.ShadowPropertyName).CurrentValue = "shadow";
                var property = context.Model.GetEntityType(typeof(SimpleEntity)).GetProperty(SimpleEntity.ShadowPropertyName);
                context.Entry(second).GetService()[property] = "shadow";
                SetPartitionId(second, context);

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                Assert.Equal(2, context.SimpleEntities.Count());

                var firstEntity = context.SimpleEntities.Single(e => e.StringProperty == "Entity 1");

                var secondEntity = context.SimpleEntities.Single(e => e.Id == 42);
                Assert.Equal("Entity 2", secondEntity.StringProperty);

                var thirdEntity = context.SimpleEntities.Single(e => EF.Property<string>(e, SimpleEntity.ShadowPropertyName) == "shadow");
                Assert.Same(secondEntity, thirdEntity);

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
            context.Entry(entity).GetService()[property] = "Partition";
        }

        protected EndToEndTest(TFixture fixture)
        {
            Fixture = fixture;
            TestStore = (TTestStore)Fixture.CreateTestStore(typeof(TTestStore));
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        protected CrossStoreContext CreateContext()
        {
            return Fixture.CreateContext(TestStore);
        }
    }

    public class InMemoryEndToEndTest : EndToEndTest<InMemoryTestStore, InMemoryCrossStoreFixture>, IClassFixture<InMemoryCrossStoreFixture>
    {
        public InMemoryEndToEndTest(InMemoryCrossStoreFixture fixture)
            : base(fixture)
        {
        }
    }

    public class SqlServerEndToEndTest : EndToEndTest<SqlServerTestStore, SqlServerCrossStoreFixture>, IClassFixture<SqlServerCrossStoreFixture>
    {
        public SqlServerEndToEndTest(SqlServerCrossStoreFixture fixture)
            : base(fixture)
        {
        }
    }

    public class SqliteEndToEndTest : EndToEndTest<SqliteTestStore, SqliteCrossStoreFixture>, IClassFixture<SqliteCrossStoreFixture>
    {
        public SqliteEndToEndTest(SqliteCrossStoreFixture fixture)
            : base(fixture)
        {
        }
    }

    [Collection("SharedEndToEndCollection")]
    public class SharedInMemoryEndToEndTest : EndToEndTest<InMemoryTestStore, SharedCrossStoreFixture>
    {
        public SharedInMemoryEndToEndTest(SharedCrossStoreFixture fixture)
            : base(fixture)
        {
        }
    }

    [Collection("SharedEndToEndCollection")]
    public class SharedSqlServerEndToEndTest : EndToEndTest<SqlServerTestStore, SharedCrossStoreFixture>
    {
        public SharedSqlServerEndToEndTest(SharedCrossStoreFixture fixture)
            : base(fixture)
        {
        }
    }

    [CollectionDefinition("SharedEndToEndCollection")]
    public class EndToEndCollection : ICollectionFixture<SharedCrossStoreFixture>
    {
    }
}

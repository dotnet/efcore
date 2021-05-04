// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class EndToEndTest : IDisposable
    {
        protected EndToEndTest(CrossStoreFixture fixture)
        {
            Fixture = fixture;
            TestStore = Fixture.CreateTestStore(TestStoreFactory, "CrossStoreTest");
        }

        protected CrossStoreFixture Fixture { get; }
        protected abstract ITestStoreFactory TestStoreFactory { get; }
        protected TestStore TestStore { get; }

        public void Dispose()
            => TestStore.Dispose();

        [ConditionalFact]
        public virtual void Can_save_changes_and_query()
        {
            int secondId;
            using (var context = CreateContext())
            {
                context.SimpleEntities.Add(
                    new SimpleEntity { StringProperty = "Entity 1" });

                Assert.Equal(1, context.SaveChanges());

                var second = context.SimpleEntities.Add(
                    new SimpleEntity { StringProperty = "Entity 2" }).Entity;
                context.Entry(second).Property(SimpleEntity.ShadowPropertyName).CurrentValue = "shadow";

                Assert.Equal(1, context.SaveChanges());
                secondId = second.Id;
            }

            using (var context = CreateContext())
            {
                Assert.Equal(2, context.SimpleEntities.Count());

                var firstEntity = context.SimpleEntities.Single(e => e.StringProperty == "Entity 1");

                var secondEntity = context.SimpleEntities.Single(e => e.Id == secondId);
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

                CrossStoreContext.RemoveAllEntities(context);
                context.SaveChanges();
            }
        }

        protected CrossStoreContext CreateContext()
            => Fixture.CreateContext(TestStore);
    }

    public class InMemoryEndToEndTest : EndToEndTest, IClassFixture<CrossStoreFixture>
    {
        public InMemoryEndToEndTest(CrossStoreFixture fixture)
            : base(fixture)
        {
        }

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }

    [SqlServerConfiguredCondition]
    public class SqlServerEndToEndTest : EndToEndTest, IClassFixture<CrossStoreFixture>
    {
        public SqlServerEndToEndTest(CrossStoreFixture fixture)
            : base(fixture)
        {
        }

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }

    public class SqliteEndToEndTest : EndToEndTest, IClassFixture<CrossStoreFixture>
    {
        public SqliteEndToEndTest(CrossStoreFixture fixture)
            : base(fixture)
        {
        }

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}

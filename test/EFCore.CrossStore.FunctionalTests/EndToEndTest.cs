// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public abstract class EndToEndTest : IAsyncLifetime
{
    protected EndToEndTest(CrossStoreFixture fixture)
    {
        Fixture = fixture;
    }

    protected CrossStoreFixture Fixture { get; }
    protected abstract ITestStoreFactory TestStoreFactory { get; }
    protected TestStore TestStore { get; private set; }

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

    public async Task InitializeAsync()
        => TestStore = await Fixture.CreateTestStoreAsync(TestStoreFactory, "CrossStoreTest");

    public Task DisposeAsync()
    {
        TestStore.Dispose();
        return Task.CompletedTask;
    }
}

public class InMemoryEndToEndTest(CrossStoreFixture fixture) : EndToEndTest(fixture), IClassFixture<CrossStoreFixture>
{
    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;
}

[SqlServerConfiguredCondition]
public class SqlServerEndToEndTest(CrossStoreFixture fixture) : EndToEndTest(fixture), IClassFixture<CrossStoreFixture>
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}

public class SqliteEndToEndTest(CrossStoreFixture fixture) : EndToEndTest(fixture), IClassFixture<CrossStoreFixture>
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;
}

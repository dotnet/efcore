// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

public class DefaultKeyValuesTest(DefaultKeyValuesTest.CosmosDefaultKeyValuesTestFixture fixture)
    : IClassFixture<DefaultKeyValuesTest.CosmosDefaultKeyValuesTestFixture>
{
    protected CosmosDefaultKeyValuesTestFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public async Task Single_key_value_with_single_partition_key_must_have_key_set()
    {
        using var context = CreateContext();

        context.Add(new SingleKeySinglePartitionKey());
        await AssertKeyValueNotSet(context, nameof(SingleKeySinglePartitionKey), nameof(SingleKeySinglePartitionKey.Id));

        context.Add(new SingleKeySinglePartitionKey { PartitionKey = 1 });
        await AssertKeyValueNotSet(context, nameof(SingleKeySinglePartitionKey), nameof(SingleKeySinglePartitionKey.Id));

        context.Add(new SingleKeySinglePartitionKey { Id = 1 });
        await AssertSaves(context);

        context.Add(new SingleKeySinglePartitionKey { Id = 1, PartitionKey = 1 });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Composite_key_value_with_single_partition_key_must_have_key_set()
    {
        using var context = CreateContext();

        context.Add(new CompositeKeySinglePartitionKey());
        await AssertKeyValueNotSet(context, nameof(CompositeKeySinglePartitionKey), nameof(CompositeKeySinglePartitionKey.Id3));

        context.Add(new CompositeKeySinglePartitionKey { PartitionKey = 1 });
        await AssertKeyValueNotSet(context, nameof(CompositeKeySinglePartitionKey), nameof(CompositeKeySinglePartitionKey.Id3));

        context.Add(new CompositeKeySinglePartitionKey { Id1 = 1 });
        await AssertSaves(context);

        context.Add(new CompositeKeySinglePartitionKey { Id2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeKeySinglePartitionKey { Id3 = true });
        await AssertSaves(context);

        context.Add(new CompositeKeySinglePartitionKey { Id1 = 1, Id2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeKeySinglePartitionKey { Id1 = 1, Id3 = true });
        await AssertSaves(context);

        context.Add(new CompositeKeySinglePartitionKey { Id2 = Guid.NewGuid(), Id3 = true });
        await AssertSaves(context);

        context.Add(
            new CompositeKeySinglePartitionKey
            {
                Id1 = 1,
                Id2 = Guid.NewGuid(),
                Id3 = true
            });
        await AssertSaves(context);

        context.Add(new CompositeKeySinglePartitionKey { Id1 = 1, PartitionKey = 1 });
        await AssertSaves(context);

        context.Add(new CompositeKeySinglePartitionKey { Id2 = Guid.NewGuid(), PartitionKey = 1 });
        await AssertSaves(context);

        context.Add(new CompositeKeySinglePartitionKey { Id3 = true, PartitionKey = 1 });
        await AssertSaves(context);

        context.Add(
            new CompositeKeySinglePartitionKey
            {
                Id1 = 1,
                Id2 = Guid.NewGuid(),
                PartitionKey = 1
            });
        await AssertSaves(context);

        context.Add(
            new CompositeKeySinglePartitionKey
            {
                Id1 = 1,
                Id3 = true,
                PartitionKey = 1
            });
        await AssertSaves(context);

        context.Add(
            new CompositeKeySinglePartitionKey
            {
                Id2 = Guid.NewGuid(),
                Id3 = true,
                PartitionKey = 1
            });
        await AssertSaves(context);

        context.Add(
            new CompositeKeySinglePartitionKey
            {
                Id1 = 1,
                Id2 = Guid.NewGuid(),
                Id3 = true,
                PartitionKey = 1
            });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Single_key_value_with_composite_partition_key_must_have_key_set()
    {
        using var context = CreateContext();

        context.Add(new SingleKeyCompositePartitionKey());
        await AssertKeyValueNotSet(context, nameof(SingleKeyCompositePartitionKey), nameof(SingleKeyCompositePartitionKey.Id));

        context.Add(new SingleKeyCompositePartitionKey { PartitionKey1 = 1 });
        await AssertKeyValueNotSet(context, nameof(SingleKeyCompositePartitionKey), nameof(SingleKeyCompositePartitionKey.Id));

        context.Add(new SingleKeyCompositePartitionKey { PartitionKey2 = Guid.NewGuid() });
        await AssertKeyValueNotSet(context, nameof(SingleKeyCompositePartitionKey), nameof(SingleKeyCompositePartitionKey.Id));

        context.Add(new SingleKeyCompositePartitionKey { PartitionKey3 = true });
        await AssertKeyValueNotSet(context, nameof(SingleKeyCompositePartitionKey), nameof(SingleKeyCompositePartitionKey.Id));

        context.Add(
            new SingleKeyCompositePartitionKey
            {
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertKeyValueNotSet(context, nameof(SingleKeyCompositePartitionKey), nameof(SingleKeyCompositePartitionKey.Id));

        context.Add(new SingleKeyCompositePartitionKey { Id = 1 });
        await AssertSaves(context);

        context.Add(
            new SingleKeyCompositePartitionKey
            {
                Id = 1,
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(
            new SingleKeyCompositePartitionKey
            {
                Id = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(new SingleKeyCompositePartitionKey { Id = 1, PartitionKey2 = Guid.NewGuid() });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Composite_key_value_with_composite_partition_key_must_have_key_set()
    {
        using var context = CreateContext();

        context.Add(new CompositeKeyCompositePartitionKey());
        await AssertKeyValueNotSet(context, nameof(CompositeKeyCompositePartitionKey), nameof(CompositeKeyCompositePartitionKey.Id3));

        context.Add(new CompositeKeyCompositePartitionKey { PartitionKey1 = 1 });
        await AssertKeyValueNotSet(context, nameof(CompositeKeyCompositePartitionKey), nameof(CompositeKeyCompositePartitionKey.Id3));

        context.Add(new CompositeKeyCompositePartitionKey { PartitionKey2 = Guid.NewGuid() });
        await AssertKeyValueNotSet(context, nameof(CompositeKeyCompositePartitionKey), nameof(CompositeKeyCompositePartitionKey.Id3));

        context.Add(new CompositeKeyCompositePartitionKey { PartitionKey3 = true });
        await AssertKeyValueNotSet(context, nameof(CompositeKeyCompositePartitionKey), nameof(CompositeKeyCompositePartitionKey.Id3));

        context.Add(
            new CompositeKeyCompositePartitionKey
            {
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertKeyValueNotSet(context, nameof(CompositeKeyCompositePartitionKey), nameof(CompositeKeyCompositePartitionKey.Id3));

        context.Add(new CompositeKeyCompositePartitionKey { Id1 = 1 });
        await AssertSaves(context);

        context.Add(
            new CompositeKeyCompositePartitionKey
            {
                Id1 = 1,
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(
            new CompositeKeyCompositePartitionKey
            {
                Id1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(new CompositeKeyCompositePartitionKey { Id1 = 1, PartitionKey2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeKeyCompositePartitionKey { Id2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(
            new CompositeKeyCompositePartitionKey
            {
                Id2 = Guid.NewGuid(),
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(
            new CompositeKeyCompositePartitionKey
            {
                Id2 = Guid.NewGuid(),
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(new CompositeKeyCompositePartitionKey { Id2 = Guid.NewGuid(), PartitionKey2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeKeyCompositePartitionKey { Id3 = true });
        await AssertSaves(context);

        context.Add(
            new CompositeKeyCompositePartitionKey
            {
                Id3 = true,
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(
            new CompositeKeyCompositePartitionKey
            {
                Id3 = true,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(new CompositeKeyCompositePartitionKey { Id3 = true, PartitionKey2 = Guid.NewGuid() });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Single_same_key_and_partition_key_must_have_key_set()
    {
        using var context = CreateContext();

        context.Add(new SingleSameKeyAndPartitionKey());
        await AssertKeyValueNotSet(context, nameof(SingleSameKeyAndPartitionKey), nameof(SingleSameKeyAndPartitionKey.Id));

        context.Add(new SingleSameKeyAndPartitionKey { Id = 1 });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Composite_same_key_and_partition_key_must_have_key_set()
    {
        using var context = CreateContext();

        context.Add(new CompositeSameKeyAndPartitionKey());
        await AssertKeyValueNotSet(context, nameof(CompositeSameKeyAndPartitionKey), nameof(CompositeSameKeyAndPartitionKey.Key1));

        context.Add(new CompositeSameKeyAndPartitionKey { Key1 = 1 });
        await AssertSaves(context);

        context.Add(new CompositeSameKeyAndPartitionKey { Key2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeSameKeyAndPartitionKey { Key3 = true });
        await AssertSaves(context);

        context.Add(
            new CompositeSameKeyAndPartitionKey
            {
                Key1 = 1,
                Key2 = Guid.NewGuid(),
                Key3 = true
            });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Single_key_value_with_single_partition_key_can_use_generated_value()
    {
        using var context = CreateContext();

        context.Add(new SingleGeneratedKeySinglePartitionKey());
        await AssertSaves(context);

        context.Add(new SingleGeneratedKeySinglePartitionKey { PartitionKey = 1 });
        await AssertSaves(context);

        context.Add(new SingleGeneratedKeySinglePartitionKey { Id = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new SingleGeneratedKeySinglePartitionKey { Id = Guid.NewGuid(), PartitionKey = 1 });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Composite_key_value_with_single_partition_key_can_use_generated_value()
    {
        using var context = CreateContext();

        context.Add(new CompositeGeneratedKeySinglePartitionKey());
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeySinglePartitionKey { PartitionKey = 1 });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeySinglePartitionKey { Id1 = 1 });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeySinglePartitionKey { Id2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeySinglePartitionKey { Id3 = true });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeySinglePartitionKey { Id1 = 1, Id2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeySinglePartitionKey { Id1 = 1, Id3 = true });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeySinglePartitionKey { Id2 = Guid.NewGuid(), Id3 = true });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeySinglePartitionKey
            {
                Id1 = 1,
                Id2 = Guid.NewGuid(),
                Id3 = true
            });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeySinglePartitionKey { Id1 = 1, PartitionKey = 1 });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeySinglePartitionKey { Id2 = Guid.NewGuid(), PartitionKey = 1 });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeySinglePartitionKey { Id3 = true, PartitionKey = 1 });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeySinglePartitionKey
            {
                Id1 = 1,
                Id2 = Guid.NewGuid(),
                PartitionKey = 1
            });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeySinglePartitionKey
            {
                Id1 = 1,
                Id3 = true,
                PartitionKey = 1
            });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeySinglePartitionKey
            {
                Id2 = Guid.NewGuid(),
                Id3 = true,
                PartitionKey = 1
            });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeySinglePartitionKey
            {
                Id1 = 1,
                Id2 = Guid.NewGuid(),
                Id3 = true,
                PartitionKey = 1
            });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Single_key_value_with_composite_partition_key_can_use_generated_value()
    {
        using var context = CreateContext();

        context.Add(new SingleGeneratedKeyCompositePartitionKey());
        await AssertSaves(context);

        context.Add(new SingleGeneratedKeyCompositePartitionKey { PartitionKey1 = 1 });
        await AssertSaves(context);

        context.Add(new SingleGeneratedKeyCompositePartitionKey { PartitionKey2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new SingleGeneratedKeyCompositePartitionKey { PartitionKey3 = true });
        await AssertSaves(context);

        context.Add(
            new SingleGeneratedKeyCompositePartitionKey
            {
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(new SingleGeneratedKeyCompositePartitionKey { Id = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(
            new SingleGeneratedKeyCompositePartitionKey
            {
                Id = Guid.NewGuid(),
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(
            new SingleGeneratedKeyCompositePartitionKey
            {
                Id = Guid.NewGuid(),
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(new SingleGeneratedKeyCompositePartitionKey { Id = Guid.NewGuid(), PartitionKey2 = Guid.NewGuid() });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Composite_key_value_with_composite_partition_key_can_use_generated_value()
    {
        using var context = CreateContext();

        context.Add(new CompositeGeneratedKeyCompositePartitionKey());
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeyCompositePartitionKey { PartitionKey1 = 1 });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeyCompositePartitionKey { PartitionKey2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeyCompositePartitionKey { PartitionKey3 = true });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeyCompositePartitionKey
            {
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeyCompositePartitionKey { Id1 = 1 });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeyCompositePartitionKey
            {
                Id1 = 1,
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeyCompositePartitionKey
            {
                Id1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeyCompositePartitionKey { Id1 = 1, PartitionKey2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeyCompositePartitionKey { Id2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeyCompositePartitionKey
            {
                Id2 = Guid.NewGuid(),
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeyCompositePartitionKey
            {
                Id2 = Guid.NewGuid(),
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeyCompositePartitionKey { Id2 = Guid.NewGuid(), PartitionKey2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeyCompositePartitionKey { Id3 = true });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeyCompositePartitionKey
            {
                Id3 = true,
                PartitionKey1 = 1,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(
            new CompositeGeneratedKeyCompositePartitionKey
            {
                Id3 = true,
                PartitionKey2 = Guid.NewGuid(),
                PartitionKey3 = true
            });
        await AssertSaves(context);

        context.Add(new CompositeGeneratedKeyCompositePartitionKey { Id3 = true, PartitionKey2 = Guid.NewGuid() });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Single_same_key_and_partition_key_can_use_generated_value()
    {
        using var context = CreateContext();

        context.Add(new SingleSameGeneratedKeyAndPartitionKey());
        await AssertSaves(context);

        context.Add(new SingleSameGeneratedKeyAndPartitionKey { Id = Guid.NewGuid() });
        await AssertSaves(context);
    }

    [ConditionalFact]
    public async Task Composite_same_key_and_partition_key_can_use_generated_value()
    {
        using var context = CreateContext();

        context.Add(new CompositeSameGeneratedKeyAndPartitionKey());
        await AssertSaves(context);

        context.Add(new CompositeSameGeneratedKeyAndPartitionKey { Key1 = 1 });
        await AssertSaves(context);

        context.Add(new CompositeSameGeneratedKeyAndPartitionKey { Key2 = Guid.NewGuid() });
        await AssertSaves(context);

        context.Add(new CompositeSameGeneratedKeyAndPartitionKey { Key3 = true });
        await AssertSaves(context);

        context.Add(
            new CompositeSameGeneratedKeyAndPartitionKey
            {
                Key1 = 1,
                Key2 = Guid.NewGuid(),
                Key3 = true
            });
        await AssertSaves(context);
    }

    private static async Task AssertSaves(DefaultKeyValuesTestContext context)
    {
        var entry = context.ChangeTracker.Entries().Single();

        Assert.Equal(EntityState.Added, entry.State);
        await context.SaveChangesAsync();
        Assert.Equal(EntityState.Unchanged, entry.State);

        context.ChangeTracker.Clear();
    }

    private static async Task AssertKeyValueNotSet(
        DefaultKeyValuesTestContext context,
        string entityTypeName,
        string propertyName)
    {
        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                CosmosEventId.PrimaryKeyValueNotSet.ToString(),
                CosmosResources.LogPrimaryKeyValueNotSet(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(
                    entityTypeName, propertyName),
                "CosmosEventId.PrimaryKeyValueNotSet"),
            (await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())).InnerException!.Message);

        context.ChangeTracker.Clear();
    }

    protected DefaultKeyValuesTestContext CreateContext()
        => Fixture.CreateContext();

    public class CosmosDefaultKeyValuesTestFixture : SharedStoreFixtureBase<DefaultKeyValuesTestContext>
    {
        protected override string StoreName
            => nameof(DefaultKeyValuesTest);

        protected override bool UsePooling
            => false;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
    }

    public class DefaultKeyValuesTestContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SingleKeySinglePartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(e => e.PartitionKey);
                    b.HasKey(e => new { e.Id, e.PartitionKey });
                });

            modelBuilder.Entity<CompositeKeySinglePartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(e => e.PartitionKey);
                    b.HasKey(
                        e => new
                        {
                            e.Id1,
                            e.Id2,
                            e.Id3,
                            e.PartitionKey
                        });
                });

            modelBuilder.Entity<SingleKeyCompositePartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(
                        e => new
                        {
                            e.PartitionKey1,
                            e.PartitionKey2,
                            e.PartitionKey3
                        });
                    b.HasKey(
                        e => new
                        {
                            e.Id,
                            e.PartitionKey1,
                            e.PartitionKey2,
                            e.PartitionKey3
                        });
                });

            modelBuilder.Entity<CompositeKeyCompositePartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(
                        e => new
                        {
                            e.PartitionKey1,
                            e.PartitionKey2,
                            e.PartitionKey3
                        });
                    b.HasKey(
                        e => new
                        {
                            e.Id1,
                            e.Id2,
                            e.Id3,
                            e.PartitionKey1,
                            e.PartitionKey2,
                            e.PartitionKey3
                        });
                });

            modelBuilder.Entity<SingleSameKeyAndPartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(e => e.Id);
                    b.HasKey(e => e.Id);
                });

            modelBuilder.Entity<CompositeSameKeyAndPartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(
                        e => new
                        {
                            e.Key1,
                            e.Key2,
                            e.Key3
                        });
                    b.HasKey(
                        e => new
                        {
                            e.Key1,
                            e.Key2,
                            e.Key3
                        });
                });

            modelBuilder.Entity<SingleGeneratedKeySinglePartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(e => e.PartitionKey);
                    b.HasKey(e => new { e.Id, e.PartitionKey });
                    b.Property(e => e.Id).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<CompositeGeneratedKeySinglePartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(e => e.PartitionKey);
                    b.HasKey(
                        e => new
                        {
                            e.Id1,
                            e.Id2,
                            e.Id3,
                            e.PartitionKey
                        });
                    b.Property(e => e.Id2).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<SingleGeneratedKeyCompositePartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(
                        e => new
                        {
                            e.PartitionKey1,
                            e.PartitionKey2,
                            e.PartitionKey3
                        });
                    b.HasKey(
                        e => new
                        {
                            e.Id,
                            e.PartitionKey1,
                            e.PartitionKey2,
                            e.PartitionKey3
                        });
                    b.Property(e => e.Id).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<CompositeGeneratedKeyCompositePartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(
                        e => new
                        {
                            e.PartitionKey1,
                            e.PartitionKey2,
                            e.PartitionKey3
                        });
                    b.HasKey(
                        e => new
                        {
                            e.Id1,
                            e.Id2,
                            e.Id3,
                            e.PartitionKey1,
                            e.PartitionKey2,
                            e.PartitionKey3
                        });
                    b.Property(e => e.Id2).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<SingleSameGeneratedKeyAndPartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(e => e.Id);
                    b.HasKey(e => e.Id);
                    b.Property(e => e.Id).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<CompositeSameGeneratedKeyAndPartitionKey>(
                b =>
                {
                    b.ToContainer(b.Metadata.ClrType.Name);
                    b.HasPartitionKey(
                        e => new
                        {
                            e.Key1,
                            e.Key2,
                            e.Key3
                        });
                    b.HasKey(
                        e => new
                        {
                            e.Key1,
                            e.Key2,
                            e.Key3
                        });
                    b.Property(e => e.Key2).ValueGeneratedOnAdd();
                });
        }
    }

    protected class SingleKeySinglePartitionKey
    {
        public int Id { get; set; }
        public int PartitionKey { get; set; }
    }

    protected class CompositeKeySinglePartitionKey
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
        public bool Id3 { get; set; }
        public int PartitionKey { get; set; }
    }

    protected class SingleKeyCompositePartitionKey
    {
        public int Id { get; set; }
        public int PartitionKey1 { get; set; }
        public Guid PartitionKey2 { get; set; }
        public bool PartitionKey3 { get; set; }
    }

    protected class CompositeKeyCompositePartitionKey
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
        public bool Id3 { get; set; }
        public int PartitionKey1 { get; set; }
        public Guid PartitionKey2 { get; set; }
        public bool PartitionKey3 { get; set; }
    }

    protected class SingleSameKeyAndPartitionKey
    {
        public int Id { get; set; }
    }

    protected class CompositeSameKeyAndPartitionKey
    {
        public int Key1 { get; set; }
        public Guid Key2 { get; set; }
        public bool Key3 { get; set; }
    }

    protected class SingleGeneratedKeySinglePartitionKey
    {
        public Guid Id { get; set; }
        public int PartitionKey { get; set; }
    }

    protected class CompositeGeneratedKeySinglePartitionKey
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
        public bool Id3 { get; set; }
        public int PartitionKey { get; set; }
    }

    protected class SingleGeneratedKeyCompositePartitionKey
    {
        public Guid Id { get; set; }
        public int PartitionKey1 { get; set; }
        public Guid PartitionKey2 { get; set; }
        public bool PartitionKey3 { get; set; }
    }

    protected class CompositeGeneratedKeyCompositePartitionKey
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
        public bool Id3 { get; set; }
        public int PartitionKey1 { get; set; }
        public Guid PartitionKey2 { get; set; }
        public bool PartitionKey3 { get; set; }
    }

    protected class SingleSameGeneratedKeyAndPartitionKey
    {
        public Guid Id { get; set; }
    }

    protected class CompositeSameGeneratedKeyAndPartitionKey
    {
        public int Key1 { get; set; }
        public Guid Key2 { get; set; }
        public bool Key3 { get; set; }
    }
}

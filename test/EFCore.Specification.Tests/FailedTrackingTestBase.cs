// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Tests that the context remains usable after tracking an entity fails because of a key conflict.
///     See https://github.com/dotnet/efcore/issues/29060.
/// </summary>
public abstract class FailedTrackingTestBase<TFixture>(TFixture fixture) : IClassFixture<TFixture>, IAsyncLifetime
    where TFixture : FailedTrackingTestBase<TFixture>.FailedTrackingFixtureBase
{
    protected TFixture Fixture { get; } = fixture;

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Add_with_duplicate_alternate_key_does_not_leave_entity_partially_tracked(bool async)
    {
        using var context = CreateContext();

        var first = new Widget { Id = 1, Name = "Foo" };
        context.Add(first);
        await context.SaveChangesAsync();

        var second = new Widget { Id = 2, Name = "Foo" };
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                if (async)
                {
                    await context.AddAsync(second);
                }
                else
                {
                    context.Add(second);
                }
            });

        Assert.Same(first, Assert.Single(context.ChangeTracker.Entries()).Entity);
        Assert.False(context.ChangeTracker.HasChanges());
        Assert.Same(first, await context.FindAsync<Widget>(1));
        Assert.Equal(EntityState.Detached, context.Entry(second).State);

        // The primary key of the entity that failed to be tracked must not be reserved.
        var third = context.Attach(new Widget { Id = 2, Name = "Bar" }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(third).State);

        // Removing the original entity and saving must not insert the entity that failed to be tracked.
        context.Remove(first);
        await context.SaveChangesAsync();

        using var verifyContext = CreateContext();
        Assert.Empty(await verifyContext.Set<Widget>().ToListAsync());
    }

    [Fact]
    public virtual void Attach_with_duplicate_alternate_key_does_not_leave_entity_partially_tracked()
    {
        using var context = CreateContext();

        var first = context.Attach(new Widget { Id = 1, Name = "Foo" }).Entity;

        var second = new Widget { Id = 2, Name = "Foo" };
        Assert.Throws<InvalidOperationException>(() => context.Attach(second));

        Assert.Same(first, Assert.Single(context.ChangeTracker.Entries<Widget>()).Entity);
        Assert.Equal(EntityState.Detached, context.Entry(second).State);

        // The entity that failed to be tracked can be tracked once the conflict is resolved.
        context.Remove(first);
        context.Attach(second);

        Assert.Equal(EntityState.Unchanged, context.Entry(second).State);
    }

    [Fact]
    public virtual async Task Failed_Add_keeps_deleted_entity_with_same_primary_key_replaceable()
    {
        using (var setupContext = CreateContext())
        {
            setupContext.AddRange(
                new Gadget { Id = 1, Code = "c1", Zip = "z1" },
                new Gadget { Id = 3, Code = "c3", Zip = "z3" });
            await setupContext.SaveChangesAsync();
        }

        using var context = CreateContext();
        var gadgets = await context.Set<Gadget>().ToListAsync();
        context.Remove(gadgets.Single(e => e.Id == 1));

        // Has the same primary key as the deleted gadget, but conflicts with the other gadget on its last key.
        // The keys are processed in the order Code, Id, Zip, so the entity has been registered with the first two when this fails.
        Assert.Throws<InvalidOperationException>(() => context.Add(new Gadget { Id = 1, Code = "c2", Zip = "z3" }));

        // The deleted gadget can still be replaced by a new one with the same key.
        context.Add(new Gadget { Id = 1, Code = "c4", Zip = "z4" });
        await context.SaveChangesAsync();

        using var verifyContext = CreateContext();
        var saved = await verifyContext.Set<Gadget>().OrderBy(e => e.Id).ToListAsync();
        Assert.Equal(
            new[] { (1, "c4", "z4"), (3, "c3", "z3") },
            saved.Select(e => (e.Id, e.Code, e.Zip)).ToArray());
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    public ValueTask InitializeAsync()
        => new(Fixture.ReseedAsync());

    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;

    public abstract class FailedTrackingFixtureBase : SharedStoreFixtureBase<FailedTrackingContext>
    {
        protected override string StoreName
            => "FailedTrackingTest";

        // Contexts holding conflicting entities must not be returned to the pool.
        protected override bool UsePooling
            => false;
    }

    public class FailedTrackingContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Widget>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Name).HasMaxLength(100);
                    b.HasAlternateKey(e => e.Name);
                });

            modelBuilder.Entity<Gadget>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Code).HasMaxLength(100);
                    b.Property(e => e.Zip).HasMaxLength(100);
                    b.HasAlternateKey(e => e.Code);
                    b.HasAlternateKey(e => e.Zip);
                });
        }
    }

    protected class Widget
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    protected class Gadget
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Zip { get; set; } = null!;
    }
}

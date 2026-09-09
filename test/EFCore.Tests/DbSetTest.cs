// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class DbSetTest
{
    [ConditionalFact]
    public void DbSets_are_cached()
    {
        DbSet<Category> set;

        using (var context = new EarlyLearningCenter())
        {
            set = context.Categories;
            Assert.Same(set, context.Set<Category>());
        }

        using (var context = new EarlyLearningCenter())
        {
            Assert.NotSame(set, context.Categories);
            Assert.NotSame(set, context.Set<Category>());
        }
    }

    [ConditionalFact]
    public async Task Use_of_set_throws_if_context_is_disposed()
    {
        DbSet<Category> set;

        using (var context = new EarlyLearningCenter())
        {
            set = context.Categories;
        }

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Add(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Find(77)).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Attach(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Update(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Remove(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.ToList()).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            (await Assert.ThrowsAsync<ObjectDisposedException>(() => set.AddAsync(new Category()).AsTask())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            (await Assert.ThrowsAsync<ObjectDisposedException>(() => set.FindAsync(77).AsTask())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            (await Assert.ThrowsAsync<ObjectDisposedException>(() => set.ToListAsync())).Message);
    }

    [ConditionalFact]
    public async Task Use_of_set_throws_if_obtained_from_disposed_context()
    {
        var context = new EarlyLearningCenter();
        context.Dispose();

        var set = context.Categories;

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Add(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Find(77)).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Attach(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Update(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Remove(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.ToList()).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            (await Assert.ThrowsAsync<ObjectDisposedException>(() => set.AddAsync(new Category()).AsTask())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            (await Assert.ThrowsAsync<ObjectDisposedException>(() => set.FindAsync(77).AsTask())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            (await Assert.ThrowsAsync<ObjectDisposedException>(() => set.ToListAsync())).Message);
    }

    [ConditionalFact]
    public void Direct_use_of_Set_throws_if_context_disposed()
    {
        var context = new EarlyLearningCenter();
        context.Dispose();

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.Set<Category>()).Message);
    }

    [ConditionalFact]
    public void Use_of_LocalView_throws_if_context_is_disposed()
    {
        LocalView<Category> view;

        using (var context = new EarlyLearningCenter())
        {
            view = context.Categories.Local;
        }

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => view.Add(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => view.Remove(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => view.Contains(new Category())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => view.CopyTo([], 0)).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => view.Clear()).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => view.GetEnumerator()).Message);
    }

    [ConditionalFact]
    public void Using_ignored_entity_that_has_DbSet_on_context_throws_appropriately()
    {
        using var context = new IgnoredCntext();

        Assert.Equal(
            CoreStrings.InvalidSetType(nameof(IgnoredEntity)),
            Assert.Throws<InvalidOperationException>(() => context.Ignored.ToList()).Message);
    }

    private class IgnoredCntext : DbContext
    {
        public DbSet<IgnoredEntity> Ignored { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Ignore<IgnoredEntity>();
    }

    private class IgnoredEntity
    {
        public int Id { get; set; }
    }

    [ConditionalFact]
    public Task Can_add_existing_entities_to_context_to_be_deleted()
        => TrackEntitiesTest((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);

    [ConditionalFact]
    public Task Can_add_new_entities_to_context_graph()
        => TrackEntitiesTest((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_new_entities_to_context_graph_async()
        => TrackEntitiesTest((c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_existing_entities_to_context_to_be_attached_graph()
        => TrackEntitiesTest((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Unchanged);

    [ConditionalFact]
    public Task Can_add_existing_entities_to_context_to_be_updated_graph()
        => TrackEntitiesTest((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);

    private static Task TrackEntitiesTest(
        Func<DbSet<Category>, Category, EntityEntry<Category>> categoryAdder,
        Func<DbSet<Product>, Product, EntityEntry<Product>> productAdder,
        EntityState expectedState)
        => TrackEntitiesTest(
            (c, e) => ValueTask.FromResult(categoryAdder(c, e)),
            (c, e) => ValueTask.FromResult(productAdder(c, e)),
            expectedState);

    private static async Task TrackEntitiesTest(
        Func<DbSet<Category>, Category, ValueTask<EntityEntry<Category>>> categoryAdder,
        Func<DbSet<Product>, Product, ValueTask<EntityEntry<Product>>> productAdder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter();
        var category1 = new Category { Id = 1, Name = "Beverages" };
        var category2 = new Category { Id = 2, Name = "Foods" };
        var product1 = new Product
        {
            Id = 1,
            Name = "Marmite",
            Price = 7.99m
        };
        var product2 = new Product
        {
            Id = 2,
            Name = "Bovril",
            Price = 4.99m
        };

        var categoryEntry1 = await categoryAdder(context.Categories, category1);
        var categoryEntry2 = await categoryAdder(context.Categories, category2);
        var productEntry1 = await productAdder(context.Products, product1);
        var productEntry2 = await productAdder(context.Products, product2);

        Assert.Same(category1, categoryEntry1.Entity);
        Assert.Same(category2, categoryEntry2.Entity);
        Assert.Same(product1, productEntry1.Entity);
        Assert.Same(product2, productEntry2.Entity);

        Assert.Same(category1, categoryEntry1.Entity);
        Assert.Equal(expectedState, categoryEntry2.State);
        Assert.Same(category2, categoryEntry2.Entity);
        Assert.Equal(expectedState, categoryEntry2.State);

        Assert.Same(product1, productEntry1.Entity);
        Assert.Equal(expectedState, productEntry1.State);
        Assert.Same(product2, productEntry2.Entity);
        Assert.Equal(expectedState, productEntry2.State);

        Assert.Same(categoryEntry1.GetInfrastructure(), context.Entry(category1).GetInfrastructure());
        Assert.Same(categoryEntry2.GetInfrastructure(), context.Entry(category2).GetInfrastructure());
        Assert.Same(productEntry1.GetInfrastructure(), context.Entry(product1).GetInfrastructure());
        Assert.Same(productEntry2.GetInfrastructure(), context.Entry(product2).GetInfrastructure());
    }

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_set()
        => TrackMultipleEntitiesTest(
            (c, e) => c.Categories.AddRange(e[0], e[1]),
            (c, e) => c.Products.AddRange(e[0], e[1]),
            EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_set_async()
        => TrackMultipleEntitiesTest(
            (c, e) => c.Categories.AddRangeAsync(e[0], e[1]),
            (c, e) => c.Products.AddRangeAsync(e[0], e[1]),
            EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_attached()
        => TrackMultipleEntitiesTest(
            (c, e) => c.Categories.AttachRange(e[0], e[1]),
            (c, e) => c.Products.AttachRange(e[0], e[1]),
            EntityState.Unchanged);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_updated()
        => TrackMultipleEntitiesTest(
            (c, e) => c.Categories.UpdateRange(e[0], e[1]),
            (c, e) => c.Products.UpdateRange(e[0], e[1]),
            EntityState.Modified);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_deleted()
        => TrackMultipleEntitiesTest(
            (c, e) => c.Categories.RemoveRange(e[0], e[1]),
            (c, e) => c.Products.RemoveRange(e[0], e[1]),
            EntityState.Deleted);

    private static Task TrackMultipleEntitiesTest(
        Action<EarlyLearningCenter, Category[]> categoryAdder,
        Action<EarlyLearningCenter, Product[]> productAdder,
        EntityState expectedState)
        => TrackMultipleEntitiesTest(
            (c, e) =>
            {
                categoryAdder(c, e);
                return Task.FromResult(0);
            },
            (c, e) =>
            {
                productAdder(c, e);
                return Task.FromResult(0);
            },
            expectedState);

    private static async Task TrackMultipleEntitiesTest(
        Func<EarlyLearningCenter, Category[], Task> categoryAdder,
        Func<EarlyLearningCenter, Product[], Task> productAdder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter();
        var category1 = new Category { Id = 1, Name = "Beverages" };
        var category2 = new Category { Id = 2, Name = "Foods" };
        var product1 = new Product
        {
            Id = 1,
            Name = "Marmite",
            Price = 7.99m
        };
        var product2 = new Product
        {
            Id = 2,
            Name = "Bovril",
            Price = 4.99m
        };

        await categoryAdder(context, [category1, category2]);
        await productAdder(context, [product1, product2]);

        Assert.Same(category1, context.Entry(category1).Entity);
        Assert.Same(category2, context.Entry(category2).Entity);
        Assert.Same(product1, context.Entry(product1).Entity);
        Assert.Same(product2, context.Entry(product2).Entity);

        Assert.Same(category1, context.Entry(category1).Entity);
        Assert.Equal(expectedState, context.Entry(category1).State);
        Assert.Same(category2, context.Entry(category2).Entity);
        Assert.Equal(expectedState, context.Entry(category2).State);

        Assert.Same(product1, context.Entry(product1).Entity);
        Assert.Equal(expectedState, context.Entry(product1).State);
        Assert.Same(product2, context.Entry(product2).Entity);
        Assert.Equal(expectedState, context.Entry(product2).State);
    }

    [ConditionalFact]
    public void Can_add_no_new_entities_to_set()
        => TrackNoEntitiesTest(c => c.Categories.AddRange(), c => c.Products.AddRange());

    [ConditionalFact]
    public async Task Can_add_no_new_entities_to_set_async()
    {
        using var context = new EarlyLearningCenter();
        await context.Categories.AddRangeAsync();
        await context.Products.AddRangeAsync();
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_attached()
        => TrackNoEntitiesTest(c => c.Categories.AttachRange(), c => c.Products.AttachRange());

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_updated()
        => TrackNoEntitiesTest(c => c.Categories.UpdateRange(), c => c.Products.UpdateRange());

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_deleted()
        => TrackNoEntitiesTest(c => c.Categories.RemoveRange(), c => c.Products.RemoveRange());

    private static void TrackNoEntitiesTest(Action<EarlyLearningCenter> categoryAdder, Action<EarlyLearningCenter> productAdder)
    {
        using var context = new EarlyLearningCenter();
        categoryAdder(context);
        productAdder(context);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_deleted_Enumerable()
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) => c.Categories.RemoveRange(e),
            (c, e) => c.Products.RemoveRange(e),
            EntityState.Deleted);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_set_Enumerable_graph()
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) => c.Categories.AddRange(e),
            (c, e) => c.Products.AddRange(e),
            EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_set_Enumerable_graph_async()
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) => c.Categories.AddRangeAsync(e),
            (c, e) => c.Products.AddRangeAsync(e),
            EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_attached_Enumerable_graph()
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) => c.Categories.AttachRange(e),
            (c, e) => c.Products.AttachRange(e),
            EntityState.Unchanged);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_updated_Enumerable_graph()
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) => c.Categories.UpdateRange(e),
            (c, e) => c.Products.UpdateRange(e),
            EntityState.Modified);

    private static Task TrackMultipleEntitiesTestEnumerable(
        Action<EarlyLearningCenter, IEnumerable<Category>> categoryAdder,
        Action<EarlyLearningCenter, IEnumerable<Product>> productAdder,
        EntityState expectedState)
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) =>
            {
                categoryAdder(c, e);
                return Task.FromResult(0);
            },
            (c, e) =>
            {
                productAdder(c, e);
                return Task.FromResult(0);
            },
            expectedState);

    private static async Task TrackMultipleEntitiesTestEnumerable(
        Func<EarlyLearningCenter, IEnumerable<Category>, Task> categoryAdder,
        Func<EarlyLearningCenter, IEnumerable<Product>, Task> productAdder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter();
        var category1 = new Category { Id = 1, Name = "Beverages" };
        var category2 = new Category { Id = 2, Name = "Foods" };
        var product1 = new Product
        {
            Id = 1,
            Name = "Marmite",
            Price = 7.99m
        };
        var product2 = new Product
        {
            Id = 2,
            Name = "Bovril",
            Price = 4.99m
        };

        await categoryAdder(
            context, new List<Category> { category1, category2 });
        await productAdder(
            context, new List<Product> { product1, product2 });

        Assert.Same(category1, context.Entry(category1).Entity);
        Assert.Same(category2, context.Entry(category2).Entity);
        Assert.Same(product1, context.Entry(product1).Entity);
        Assert.Same(product2, context.Entry(product2).Entity);

        Assert.Same(category1, context.Entry(category1).Entity);
        Assert.Equal(expectedState, context.Entry(category1).State);
        Assert.Same(category2, context.Entry(category2).Entity);
        Assert.Equal(expectedState, context.Entry(category2).State);

        Assert.Same(product1, context.Entry(product1).Entity);
        Assert.Equal(expectedState, context.Entry(product1).State);
        Assert.Same(product2, context.Entry(product2).Entity);
        Assert.Equal(expectedState, context.Entry(product2).State);
    }

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_deleted_Enumerable()
        => TrackNoEntitiesTestEnumerable((c, e) => c.Categories.RemoveRange(e), (c, e) => c.Products.RemoveRange(e));

    [ConditionalFact]
    public void Can_add_no_new_entities_to_set_Enumerable_graph()
        => TrackNoEntitiesTestEnumerable((c, e) => c.Categories.AddRange(e), (c, e) => c.Products.AddRange(e));

    [ConditionalFact]
    public async Task Can_add_no_new_entities_to_set_Enumerable_graph_async()
    {
        using var context = new EarlyLearningCenter();
        await context.Categories.AddRangeAsync(new HashSet<Category>());
        await context.Products.AddRangeAsync(new HashSet<Product>());
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_attached_Enumerable_graph()
        => TrackNoEntitiesTestEnumerable((c, e) => c.Categories.AttachRange(e), (c, e) => c.Products.AttachRange(e));

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_updated_Enumerable_graph()
        => TrackNoEntitiesTestEnumerable((c, e) => c.Categories.UpdateRange(e), (c, e) => c.Products.UpdateRange(e));

    private static void TrackNoEntitiesTestEnumerable(
        Action<EarlyLearningCenter, IEnumerable<Category>> categoryAdder,
        Action<EarlyLearningCenter, IEnumerable<Product>> productAdder)
    {
        using var context = new EarlyLearningCenter();
        categoryAdder(context, new HashSet<Category>());
        productAdder(context, new HashSet<Product>());
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public async Task Can_use_Add_to_change_entity_state()
    {
        await ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Detached, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Unchanged, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Deleted, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Modified, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.Categories.Add(e), EntityState.Added, EntityState.Added);
    }

    [ConditionalFact]
    public async Task Can_use_Add_to_change_entity_state_async()
    {
        await ChangeStateWithMethod(async (c, e) => await c.Categories.AddAsync(e), EntityState.Detached, EntityState.Added);
        await ChangeStateWithMethod(async (c, e) => await c.Categories.AddAsync(e), EntityState.Unchanged, EntityState.Added);
        await ChangeStateWithMethod(async (c, e) => await c.Categories.AddAsync(e), EntityState.Deleted, EntityState.Added);
        await ChangeStateWithMethod(async (c, e) => await c.Categories.AddAsync(e), EntityState.Modified, EntityState.Added);
        await ChangeStateWithMethod(async (c, e) => await c.Categories.AddAsync(e), EntityState.Added, EntityState.Added);
    }

    [ConditionalFact]
    public async Task Can_use_Attach_to_change_entity_state()
    {
        await ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Detached, EntityState.Unchanged);
        await ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Unchanged, EntityState.Unchanged);
        await ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Deleted, EntityState.Unchanged);
        await ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Modified, EntityState.Unchanged);
        await ChangeStateWithMethod((c, e) => c.Categories.Attach(e), EntityState.Added, EntityState.Unchanged);
    }

    [ConditionalFact]
    public async Task Can_use_Update_to_change_entity_state()
    {
        await ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Detached, EntityState.Modified);
        await ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Unchanged, EntityState.Modified);
        await ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Deleted, EntityState.Modified);
        await ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Modified, EntityState.Modified);
        await ChangeStateWithMethod((c, e) => c.Categories.Update(e), EntityState.Added, EntityState.Modified);
    }

    [ConditionalFact]
    public async Task Can_use_Remove_to_change_entity_state()
    {
        await ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Detached, EntityState.Deleted);
        await ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Unchanged, EntityState.Deleted);
        await ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Deleted, EntityState.Deleted);
        await ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Modified, EntityState.Deleted);
        await ChangeStateWithMethod((c, e) => c.Categories.Remove(e), EntityState.Added, EntityState.Detached);
    }

    private Task ChangeStateWithMethod(
        Action<EarlyLearningCenter, Category> action,
        EntityState initialState,
        EntityState expectedState)
        => ChangeStateWithMethod(
            (c, e) =>
            {
                action(c, e);
                return Task.FromResult(0);
            },
            initialState,
            expectedState);

    private async Task ChangeStateWithMethod(
        Func<EarlyLearningCenter, Category, Task> action,
        EntityState initialState,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter();
        var entity = new Category { Id = 1, Name = "Beverages" };
        var entry = context.Entry(entity);

        entry.State = initialState;

        await action(context, entity);

        Assert.Equal(expectedState, entry.State);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Can_add_new_entities_to_context_with_key_generation(bool async)
    {
        using var context = new EarlyLearningCenter();
        var gu1 = new TheGu { ShirtColor = "Red" };
        var gu2 = new TheGu { ShirtColor = "Still Red" };

        if (async)
        {
            Assert.Same(gu1, (await context.Gus.AddAsync(gu1)).Entity);
            Assert.Same(gu2, (await context.Gus.AddAsync(gu2)).Entity);
        }
        else
        {
            Assert.Same(gu1, context.Gus.Add(gu1).Entity);
            Assert.Same(gu2, context.Gus.Add(gu2).Entity);
        }

        Assert.NotEqual(default, gu1.Id);
        Assert.NotEqual(default, gu2.Id);
        Assert.NotEqual(gu1.Id, gu2.Id);

        var categoryEntry = context.Entry(gu1);
        Assert.Same(gu1, categoryEntry.Entity);
        Assert.Equal(EntityState.Added, categoryEntry.State);

        categoryEntry = context.Entry(gu2);
        Assert.Same(gu2, categoryEntry.Entity);
        Assert.Equal(EntityState.Added, categoryEntry.State);
    }

    [ConditionalFact]
    public void Can_get_scoped_service_provider()
    {
        using var context = new EarlyLearningCenter();
        Assert.Same(
            ((IInfrastructure<IServiceProvider>)context).Instance,
            ((IInfrastructure<IServiceProvider>)context.Products).Instance);
    }

    [ConditionalFact]
    public void Throws_when_using_with_IListSource()
    {
        using var context = new EarlyLearningCenter();
        Assert.Equal(
            CoreStrings.DataBindingWithIListSource,
            Assert.Throws<NotSupportedException>(() => ((IListSource)context.Gus).GetList()).Message);
    }

    [ConditionalFact]
    public void Throws_when_using_query_with_IListSource()
    {
        using var context = new EarlyLearningCenter();
        Assert.Equal(
            CoreStrings.DataBindingWithIListSource,
            Assert.Throws<NotSupportedException>(() => ((IListSource)context.Gus.Distinct()).GetList()).Message);
    }

    [ConditionalFact]
    public void Throws_when_using_Local_with_IListSource()
    {
        using var context = new EarlyLearningCenter();
        Assert.Equal(
            CoreStrings.DataBindingToLocalWithIListSource,
            Assert.Throws<NotSupportedException>(() => ((IListSource)context.Gus.Local).GetList()).Message);
    }

    [ConditionalFact]
    public void Can_enumerate_with_foreach()
    {
        using var context = new EarlyLearningCenter();
        foreach (var _ in context.Categories)
        {
            throw new Exception("DbSet should be empty");
        }
    }

    [ConditionalFact]
    public async Task Can_enumerate_with_await_foreach()
    {
        using var context = new EarlyLearningCenter();
        await foreach (var _ in context.Categories)
        {
            throw new Exception("DbSet should be empty");
        }
    }

    [ConditionalFact]
    public async Task Can_enumerate_with_await_foreach_with_cancellation()
    {
        using var context = new EarlyLearningCenter();
        await foreach (var _ in context.Categories.AsAsyncEnumerable().WithCancellation(default))
        {
            throw new Exception("DbSet should be empty");
        }
    }

    private class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    private class TheGu
    {
        public Guid Id { get; set; }
        public string ShirtColor { get; set; }
    }

    private class EarlyLearningCenter : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TheGu> Gus { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(InMemoryTestHelpers.Instance.CreateServiceProvider());
    }
}

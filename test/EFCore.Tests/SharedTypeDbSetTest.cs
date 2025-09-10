// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore;

public class SharedTypeDbSetTest
{
    [ConditionalFact]
    public void DbSets_are_cached()
    {
        DbSet<Category> set1;
        DbSet<Category> set2;

        using (var context = new EarlyLearningCenter())
        {
            set1 = context.Category1s;
            set2 = context.Category2s;
            Assert.Same(set1, context.Set<Category>("Category1"));
            Assert.Same(set2, context.Set<Category>("Category2"));
        }

        using (var context = new EarlyLearningCenter())
        {
            Assert.NotSame(set1, context.Category1s);
            Assert.NotSame(set2, context.Category2s);
        }
    }

    [ConditionalFact]
    public async Task Use_of_set_throws_if_context_is_disposed()
    {
        DbSet<Category> set;

        using (var context = new EarlyLearningCenter())
        {
            set = context.Set<Category>("Category1");
        }

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => set.Entry(new Category())).Message);

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
    public void Direct_use_of_Set_for_shared_type_throws_if_context_disposed()
    {
        var context = new EarlyLearningCenter();
        context.Dispose();

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.Set<Dictionary<string, object>>("SharedTypeEntityTypeName")).Message);
    }

    [ConditionalFact]
    public void Using_shared_type_entity_type_db_set_with_incorrect_return_type_throws()
    {
        using var context = new EarlyLearningCenter();

        var dbSet = context.Set<Dictionary<string, object>>("SharedEntity");

        Assert.NotNull(dbSet.Add(new Dictionary<string, object> { { "Id", 1 } }));
        Assert.NotNull(dbSet.ToList());

        var wrongDbSet = context.Set<Category>("SharedEntity");

        Assert.Equal(
            CoreStrings.DbSetIncorrectGenericType("SharedEntity", "Dictionary<string, object>", "Category"),
            Assert.Throws<InvalidOperationException>(() => wrongDbSet.Add(new Category())).Message);
        Assert.Equal(
            CoreStrings.DbSetIncorrectGenericType("SharedEntity", "Dictionary<string, object>", "Category"),
            Assert.Throws<InvalidOperationException>(() => wrongDbSet.ToList()).Message);
    }

    [ConditionalFact]
    public void Use_of_LocalView_throws_if_context_is_disposed()
    {
        LocalView<Category> view;

        using (var context = new EarlyLearningCenter())
        {
            view = context.Category1s.Local;
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
        var category11 = new Category { Id = 1, Name = "Beverages" };
        var category12 = new Category { Id = 2, Name = "Foods" };
        var product11 = new Product
        {
            Id = 1,
            Name = "Marmite",
            Price = 7.99m
        };
        var product12 = new Product
        {
            Id = 2,
            Name = "Bovril",
            Price = 4.99m
        };

        var category21 = new Category { Id = 1, Name = "Beverages" };
        var category22 = new Category { Id = 2, Name = "Foods" };
        var product21 = new Product
        {
            Id = 1,
            Name = "Marmite",
            Price = 7.99m
        };
        var product22 = new Product
        {
            Id = 2,
            Name = "Bovril",
            Price = 4.99m
        };

        var categoryEntry11 = await categoryAdder(context.Category1s, category11);
        var categoryEntry12 = await categoryAdder(context.Category1s, category12);
        var categoryEntry21 = await categoryAdder(context.Category2s, category21);
        var categoryEntry22 = await categoryAdder(context.Category2s, category22);
        var productEntry11 = await productAdder(context.Product1s, product11);
        var productEntry12 = await productAdder(context.Product1s, product12);
        var productEntry21 = await productAdder(context.Product2s, product21);
        var productEntry22 = await productAdder(context.Product2s, product22);

        Assert.Same(category11, categoryEntry11.Entity);
        Assert.Same(category12, categoryEntry12.Entity);
        Assert.Same(category21, categoryEntry21.Entity);
        Assert.Same(category22, categoryEntry22.Entity);
        Assert.Same(product11, productEntry11.Entity);
        Assert.Same(product12, productEntry12.Entity);
        Assert.Same(product21, productEntry21.Entity);
        Assert.Same(product22, productEntry22.Entity);

        Assert.Same(category11, categoryEntry11.Entity);
        Assert.Equal(expectedState, categoryEntry12.State);
        Assert.Same(category11, categoryEntry11.Entity);
        Assert.Equal(expectedState, categoryEntry12.State);
        Assert.Same(category12, categoryEntry12.Entity);
        Assert.Equal(expectedState, categoryEntry12.State);
        Assert.Same(category12, categoryEntry12.Entity);
        Assert.Equal(expectedState, categoryEntry12.State);

        Assert.Same(product21, productEntry21.Entity);
        Assert.Equal(expectedState, productEntry21.State);
        Assert.Same(product21, productEntry21.Entity);
        Assert.Equal(expectedState, productEntry21.State);
        Assert.Same(product22, productEntry22.Entity);
        Assert.Equal(expectedState, productEntry22.State);
        Assert.Same(product22, productEntry22.Entity);
        Assert.Equal(expectedState, productEntry22.State);

        Assert.Same(categoryEntry11.GetInfrastructure(), context.Entry(category11).GetInfrastructure());
        Assert.Same(categoryEntry12.GetInfrastructure(), context.Entry(category12).GetInfrastructure());
        Assert.Same(categoryEntry21.GetInfrastructure(), context.Entry(category21).GetInfrastructure());
        Assert.Same(categoryEntry22.GetInfrastructure(), context.Entry(category22).GetInfrastructure());
        Assert.Same(productEntry11.GetInfrastructure(), context.Entry(product11).GetInfrastructure());
        Assert.Same(productEntry12.GetInfrastructure(), context.Entry(product12).GetInfrastructure());
        Assert.Same(productEntry21.GetInfrastructure(), context.Entry(product21).GetInfrastructure());
        Assert.Same(productEntry22.GetInfrastructure(), context.Entry(product22).GetInfrastructure());
    }

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_set()
        => TrackMultipleEntitiesTest(
            (c, e) => c.Category1s.AddRange(e[0], e[1]),
            (c, e) => c.Product2s.AddRange(e[0], e[1]),
            EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_set_async()
        => TrackMultipleEntitiesTest(
            (c, e) => c.Category1s.AddRangeAsync(e[0], e[1]),
            (c, e) => c.Product2s.AddRangeAsync(e[0], e[1]),
            EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_attached()
        => TrackMultipleEntitiesTest(
            (c, e) => c.Category1s.AttachRange(e[0], e[1]),
            (c, e) => c.Product2s.AttachRange(e[0], e[1]),
            EntityState.Unchanged);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_updated()
        => TrackMultipleEntitiesTest(
            (c, e) => c.Category1s.UpdateRange(e[0], e[1]),
            (c, e) => c.Product2s.UpdateRange(e[0], e[1]),
            EntityState.Modified);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_deleted()
        => TrackMultipleEntitiesTest(
            (c, e) => c.Category1s.RemoveRange(e[0], e[1]),
            (c, e) => c.Product2s.RemoveRange(e[0], e[1]),
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
        => TrackNoEntitiesTest(c => c.Category1s.AddRange(), c => c.Product2s.AddRange());

    [ConditionalFact]
    public async Task Can_add_no_new_entities_to_set_async()
    {
        using var context = new EarlyLearningCenter();
        await context.Category1s.AddRangeAsync();
        await context.Product2s.AddRangeAsync();
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_attached()
        => TrackNoEntitiesTest(c => c.Category1s.AttachRange(), c => c.Product2s.AttachRange());

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_updated()
        => TrackNoEntitiesTest(c => c.Category1s.UpdateRange(), c => c.Product2s.UpdateRange());

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_deleted()
        => TrackNoEntitiesTest(c => c.Category1s.RemoveRange(), c => c.Product2s.RemoveRange());

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
            (c, e) => c.Category1s.RemoveRange(e),
            (c, e) => c.Product2s.RemoveRange(e),
            EntityState.Deleted);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_set_Enumerable_graph()
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) => c.Category1s.AddRange(e),
            (c, e) => c.Product2s.AddRange(e),
            EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_set_Enumerable_graph_async()
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) => c.Category1s.AddRangeAsync(e),
            (c, e) => c.Product2s.AddRangeAsync(e),
            EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_attached_Enumerable_graph()
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) => c.Category1s.AttachRange(e),
            (c, e) => c.Product2s.AttachRange(e),
            EntityState.Unchanged);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_set_to_be_updated_Enumerable_graph()
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) => c.Category1s.UpdateRange(e),
            (c, e) => c.Product2s.UpdateRange(e),
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
        => TrackNoEntitiesTestEnumerable((c, e) => c.Category2s.RemoveRange(e), (c, e) => c.Product1s.RemoveRange(e));

    [ConditionalFact]
    public void Can_add_no_new_entities_to_set_Enumerable_graph()
        => TrackNoEntitiesTestEnumerable((c, e) => c.Category2s.AddRange(e), (c, e) => c.Product1s.AddRange(e));

    [ConditionalFact]
    public async Task Can_add_no_new_entities_to_set_Enumerable_graph_async()
    {
        using var context = new EarlyLearningCenter();
        await context.Category2s.AddRangeAsync(new HashSet<Category>());
        await context.Product1s.AddRangeAsync(new HashSet<Product>());
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_attached_Enumerable_graph()
        => TrackNoEntitiesTestEnumerable((c, e) => c.Category2s.AttachRange(e), (c, e) => c.Product1s.AttachRange(e));

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_set_to_be_updated_Enumerable_graph()
        => TrackNoEntitiesTestEnumerable((c, e) => c.Category2s.UpdateRange(e), (c, e) => c.Product1s.UpdateRange(e));

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
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Add(e), EntityState.Detached, EntityState.Added);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Add(e), EntityState.Unchanged, EntityState.Added);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Add(e), EntityState.Deleted, EntityState.Added);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Add(e), EntityState.Modified, EntityState.Added);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Add(e), EntityState.Added, EntityState.Added);
    }

    [ConditionalFact]
    public async Task Can_use_Add_to_change_entity_state_async()
    {
        await ChangeStateWithMethod(c => c.Category2s, async (c, e) => await c.AddAsync(e), EntityState.Detached, EntityState.Added);
        await ChangeStateWithMethod(c => c.Category2s, async (c, e) => await c.AddAsync(e), EntityState.Unchanged, EntityState.Added);
        await ChangeStateWithMethod(c => c.Category2s, async (c, e) => await c.AddAsync(e), EntityState.Deleted, EntityState.Added);
        await ChangeStateWithMethod(c => c.Category2s, async (c, e) => await c.AddAsync(e), EntityState.Modified, EntityState.Added);
        await ChangeStateWithMethod(c => c.Category2s, async (c, e) => await c.AddAsync(e), EntityState.Added, EntityState.Added);
    }

    [ConditionalFact]
    public async Task Can_use_Attach_to_change_entity_state()
    {
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Attach(e), EntityState.Detached, EntityState.Unchanged);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Attach(e), EntityState.Unchanged, EntityState.Unchanged);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Attach(e), EntityState.Deleted, EntityState.Unchanged);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Attach(e), EntityState.Modified, EntityState.Unchanged);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Attach(e), EntityState.Added, EntityState.Unchanged);
    }

    [ConditionalFact]
    public async Task Can_use_Update_to_change_entity_state()
    {
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Update(e), EntityState.Detached, EntityState.Modified);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Update(e), EntityState.Unchanged, EntityState.Modified);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Update(e), EntityState.Deleted, EntityState.Modified);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Update(e), EntityState.Modified, EntityState.Modified);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Update(e), EntityState.Added, EntityState.Modified);
    }

    [ConditionalFact]
    public async Task Can_use_Remove_to_change_entity_state()
    {
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Remove(e), EntityState.Detached, EntityState.Deleted);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Remove(e), EntityState.Unchanged, EntityState.Deleted);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Remove(e), EntityState.Deleted, EntityState.Deleted);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Remove(e), EntityState.Modified, EntityState.Deleted);
        await ChangeStateWithMethod(c => c.Category2s, (c, e) => c.Remove(e), EntityState.Added, EntityState.Detached);
    }

    private Task ChangeStateWithMethod(
        Func<EarlyLearningCenter, DbSet<Category>> getSet,
        Action<DbSet<Category>, Category> action,
        EntityState initialState,
        EntityState expectedState)
        => ChangeStateWithMethod(
            getSet,
            (s, e) =>
            {
                action(s, e);
                return Task.FromResult(0);
            },
            initialState,
            expectedState);

    private async Task ChangeStateWithMethod(
        Func<EarlyLearningCenter, DbSet<Category>> getSet,
        Func<DbSet<Category>, Category, Task> action,
        EntityState initialState,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter();
        var entity = new Category { Id = 1, Name = "Beverages" };
        var set = getSet(context);
        var entry = set.Entry(entity);

        entry.State = initialState;

        await action(set, entity);

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
            Assert.Same(gu1, (await context.Gu1s.AddAsync(gu1)).Entity);
            Assert.Same(gu2, (await context.Gu1s.AddAsync(gu2)).Entity);
        }
        else
        {
            Assert.Same(gu1, context.Gu1s.Add(gu1).Entity);
            Assert.Same(gu2, context.Gu1s.Add(gu2).Entity);
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
            ((IInfrastructure<IServiceProvider>)context.Product1s).Instance);
    }

    [ConditionalFact]
    public void Throws_when_using_with_IListSource()
    {
        using var context = new EarlyLearningCenter();
        Assert.Equal(
            CoreStrings.DataBindingWithIListSource,
            Assert.Throws<NotSupportedException>(() => ((IListSource)context.Gu1s).GetList()).Message);
    }

    [ConditionalFact]
    public void Throws_when_using_query_with_IListSource()
    {
        using var context = new EarlyLearningCenter();
        Assert.Equal(
            CoreStrings.DataBindingWithIListSource,
            Assert.Throws<NotSupportedException>(() => ((IListSource)context.Gu1s.Distinct()).GetList()).Message);
    }

    [ConditionalFact]
    public void Throws_when_using_Local_with_IListSource()
    {
        using var context = new EarlyLearningCenter();
        Assert.Equal(
            CoreStrings.DataBindingToLocalWithIListSource,
            Assert.Throws<NotSupportedException>(() => ((IListSource)context.Gu1s.Local).GetList()).Message);
    }

    [ConditionalFact]
    public void Can_enumerate_with_foreach()
    {
        using var context = new EarlyLearningCenter();
        foreach (var _ in context.Category2s)
        {
            throw new Exception("DbSet should be empty");
        }
    }

    [ConditionalFact]
    public async Task Can_enumerate_with_await_foreach()
    {
        using var context = new EarlyLearningCenter();
        await foreach (var _ in context.Category2s)
        {
            throw new Exception("DbSet should be empty");
        }
    }

    [ConditionalFact]
    public async Task Can_enumerate_with_await_foreach_with_cancellation()
    {
        using var context = new EarlyLearningCenter();
        await foreach (var _ in context.Category2s.AsAsyncEnumerable().WithCancellation(default))
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
        public DbSet<Product> Product1s
            => Set<Product>("Product1");

        public DbSet<Product> Product2s
            => Set<Product>("Product2");

        public DbSet<Category> Category1s
            => Set<Category>("Category1");

        public DbSet<Category> Category2s
            => Set<Category>("Category2");

        public DbSet<TheGu> Gu1s
            => Set<TheGu>("TheGu1");

        public DbSet<TheGu> Gu2s
            => Set<TheGu>("TheGu2");

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(InMemoryTestHelpers.Instance.CreateServiceProvider());

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SharedTypeEntity<Dictionary<string, object>>("SharedEntity").IndexerProperty<int>("Id");

            modelBuilder.SharedTypeEntity<Product>("Product1");
            modelBuilder.SharedTypeEntity<Product>("Product2");

            modelBuilder.SharedTypeEntity<Category>("Category1");
            modelBuilder.SharedTypeEntity<Category>("Category2");

            modelBuilder.SharedTypeEntity<TheGu>("TheGu1");
            modelBuilder.SharedTypeEntity<TheGu>("TheGu2");
        }
    }
}

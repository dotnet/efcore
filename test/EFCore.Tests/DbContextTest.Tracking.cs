// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public partial class DbContextTest
{
    [ConditionalFact]
    public Task Can_add_existing_entities_to_context_to_be_deleted()
        => TrackEntitiesTest((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);

    [ConditionalFact]
    public Task Can_add_new_entities_to_context_with_graph_method()
        => TrackEntitiesTest((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_new_entities_to_context_with_graph_method_async()
        => TrackEntitiesTest((c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_existing_entities_to_context_to_be_attached_with_graph_method()
        => TrackEntitiesTest((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Unchanged);

    [ConditionalFact]
    public Task Can_add_existing_entities_to_context_to_be_updated_with_graph_method()
        => TrackEntitiesTest((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);

    private static Task TrackEntitiesTest(
        Func<DbContext, Category, EntityEntry<Category>> categoryAdder,
        Func<DbContext, Product, EntityEntry<Product>> productAdder,
        EntityState expectedState)
        => TrackEntitiesTest(
            (c, e) => ValueTask.FromResult(categoryAdder(c, e)),
            (c, e) => ValueTask.FromResult(productAdder(c, e)),
            expectedState);

    private static async Task TrackEntitiesTest(
        Func<DbContext, Category, ValueTask<EntityEntry<Category>>> categoryAdder,
        Func<DbContext, Product, ValueTask<EntityEntry<Product>>> productAdder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var relatedDependent = new Product
        {
            Id = 1,
            Name = "Marmite",
            Price = 7.99m,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var principal = new Category
        {
            Id = 1,
            Name = "Beverages",
            Products = [relatedDependent],
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var relatedPrincipal = new Category
        {
            Id = 2,
            Name = "Foods",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var dependent = new Product
        {
            Id = 2,
            Name = "Bovril",
            Price = 4.99m,
            Category = relatedPrincipal,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var principalEntry = await categoryAdder(context, principal);
        var dependentEntry = await productAdder(context, dependent);

        var relatedPrincipalEntry = context.Entry(relatedPrincipal);
        var relatedDependentEntry = context.Entry(relatedDependent);

        Assert.Same(principal, principalEntry.Entity);
        Assert.Same(relatedPrincipal, relatedPrincipalEntry.Entity);
        Assert.Same(relatedDependent, relatedDependentEntry.Entity);
        Assert.Same(dependent, dependentEntry.Entity);

        Assert.Same(principal, principalEntry.Entity);
        Assert.Equal(expectedState, principalEntry.State);
        Assert.Same(relatedPrincipal, relatedPrincipalEntry.Entity);
        Assert.Equal(expectedState == EntityState.Deleted ? EntityState.Unchanged : expectedState, relatedPrincipalEntry.State);

        Assert.Same(relatedDependent, relatedDependentEntry.Entity);
        Assert.Equal(expectedState, relatedDependentEntry.State);
        Assert.Same(dependent, dependentEntry.Entity);
        Assert.Equal(expectedState, dependentEntry.State);

        Assert.Same(principalEntry.GetInfrastructure(), context.Entry(principal).GetInfrastructure());
        Assert.Same(relatedPrincipalEntry.GetInfrastructure(), context.Entry(relatedPrincipal).GetInfrastructure());
        Assert.Same(relatedDependentEntry.GetInfrastructure(), context.Entry(relatedDependent).GetInfrastructure());
        Assert.Same(dependentEntry.GetInfrastructure(), context.Entry(dependent).GetInfrastructure());
    }

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_context()
        => TrackMultipleEntitiesTest((c, e) => c.AddRange(e[0], e[1]), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_context_async()
        => TrackMultipleEntitiesTest((c, e) => c.AddRangeAsync(e[0], e[1]), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_context_to_be_attached()
        => TrackMultipleEntitiesTest((c, e) => c.AttachRange(e[0], e[1]), EntityState.Unchanged);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_context_to_be_updated()
        => TrackMultipleEntitiesTest((c, e) => c.UpdateRange(e[0], e[1]), EntityState.Modified);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_context_to_be_deleted()
        => TrackMultipleEntitiesTest((c, e) => c.RemoveRange(e[0], e[1]), EntityState.Deleted);

    private static Task TrackMultipleEntitiesTest(
        Action<DbContext, object[]> adder,
        EntityState expectedState)
        => TrackMultipleEntitiesTest(
            (c, e) =>
            {
                adder(c, e);
                return Task.FromResult(0);
            },
            expectedState);

    private static async Task TrackMultipleEntitiesTest(
        Func<DbContext, object[], Task> adder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var relatedDependent = new Product
        {
            Id = 1,
            Name = "Marmite",
            Price = 7.99m,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var principal = new Category
        {
            Id = 1,
            Name = "Beverages",
            Products = [relatedDependent],
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var relatedPrincipal = new Category
        {
            Id = 2,
            Name = "Foods",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var dependent = new Product
        {
            Id = 2,
            Name = "Bovril",
            Price = 4.99m,
            Category = relatedPrincipal,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        await adder(context, [principal, dependent]);

        Assert.Same(principal, context.Entry(principal).Entity);
        Assert.Same(relatedPrincipal, context.Entry(relatedPrincipal).Entity);
        Assert.Same(relatedDependent, context.Entry(relatedDependent).Entity);
        Assert.Same(dependent, context.Entry(dependent).Entity);

        Assert.Same(principal, context.Entry(principal).Entity);
        Assert.Equal(expectedState, context.Entry(principal).State);
        Assert.Same(relatedPrincipal, context.Entry(relatedPrincipal).Entity);
        Assert.Equal(
            expectedState == EntityState.Deleted ? EntityState.Unchanged : expectedState, context.Entry(relatedPrincipal).State);

        Assert.Same(relatedDependent, context.Entry(relatedDependent).Entity);
        Assert.Equal(expectedState, context.Entry(relatedDependent).State);
        Assert.Same(dependent, context.Entry(dependent).Entity);
        Assert.Equal(expectedState, context.Entry(dependent).State);
    }

    [ConditionalFact]
    public Task Can_add_existing_entities_with_default_value_to_context_to_be_deleted()
        => TrackEntitiesDefaultValueTest((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);

    [ConditionalFact]
    public Task Can_add_new_entities_with_default_value_to_context_with_graph_method()
        => TrackEntitiesDefaultValueTest((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_new_entities_with_default_value_to_context_with_graph_method_async()
        => TrackEntitiesDefaultValueTest((c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_existing_entities_with_default_value_to_context_to_be_attached_with_graph_method()
        => TrackEntitiesDefaultValueTest((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_existing_entities_with_default_value_to_context_to_be_updated_with_graph_method()
        => TrackEntitiesDefaultValueTest((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Added);

    private static Task TrackEntitiesDefaultValueTest(
        Func<DbContext, Category, EntityEntry<Category>> categoryAdder,
        Func<DbContext, Product, EntityEntry<Product>> productAdder,
        EntityState expectedState)
        => TrackEntitiesDefaultValueTest(
            (c, e) => ValueTask.FromResult(categoryAdder(c, e)),
            (c, e) => ValueTask.FromResult(productAdder(c, e)),
            expectedState);

    // Issue #3890
    private static async Task TrackEntitiesDefaultValueTest(
        Func<DbContext, Category, ValueTask<EntityEntry<Category>>> categoryAdder,
        Func<DbContext, Product, ValueTask<EntityEntry<Product>>> productAdder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category1 = new Category
        {
            Id = 0,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product1 = new Product
        {
            Id = 0,
            Name = "Marmite",
            Price = 7.99m,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var categoryEntry1 = await categoryAdder(context, category1);
        var productEntry1 = await productAdder(context, product1);

        Assert.Same(category1, categoryEntry1.Entity);
        Assert.Same(product1, productEntry1.Entity);

        Assert.Same(category1, categoryEntry1.Entity);
        Assert.Equal(expectedState, categoryEntry1.State);

        Assert.Same(product1, productEntry1.Entity);
        Assert.Equal(expectedState, productEntry1.State);

        Assert.Same(categoryEntry1.GetInfrastructure(), context.Entry(category1).GetInfrastructure());
        Assert.Same(productEntry1.GetInfrastructure(), context.Entry(product1).GetInfrastructure());
    }

    [ConditionalFact]
    public Task Can_add_existing_entities_with_sentinel_value_to_context_to_be_deleted()
        => TrackEntitiesSentinelValueTest((c, e) => c.Remove(e), (c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);

    [ConditionalFact]
    public Task Can_add_new_entities_with_sentinel_value_to_context_with_graph_method()
        => TrackEntitiesSentinelValueTest((c, e) => c.Add(e), (c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_new_entities_with_sentinel_value_to_context_with_graph_method_async()
        => TrackEntitiesSentinelValueTest((c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_existing_entities_with_sentinel_value_to_context_to_be_attached_with_graph_method()
        => TrackEntitiesSentinelValueTest((c, e) => c.Attach(e), (c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_existing_entities_with_sentinel_value_to_context_to_be_updated_with_graph_method()
        => TrackEntitiesSentinelValueTest((c, e) => c.Update(e), (c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Added);

    private static Task TrackEntitiesSentinelValueTest(
        Func<DbContext, CategoryWithSentinel, EntityEntry<CategoryWithSentinel>> categoryAdder,
        Func<DbContext, ProductWithSentinel, EntityEntry<ProductWithSentinel>> productAdder,
        Func<DbContext, TheGuWithSentinel, EntityEntry<TheGuWithSentinel>> guAdder,
        EntityState expectedState)
        => TrackEntitiesSentinelValueTest(
            (c, e) => ValueTask.FromResult(categoryAdder(c, e)),
            (c, e) => ValueTask.FromResult(productAdder(c, e)),
            (c, e) => ValueTask.FromResult(guAdder(c, e)),
            expectedState);

    // Issue #3890
    private static async Task TrackEntitiesSentinelValueTest(
        Func<DbContext, CategoryWithSentinel, ValueTask<EntityEntry<CategoryWithSentinel>>> categoryAdder,
        Func<DbContext, ProductWithSentinel, ValueTask<EntityEntry<ProductWithSentinel>>> productAdder,
        Func<DbContext, TheGuWithSentinel, ValueTask<EntityEntry<TheGuWithSentinel>>> guAdder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category1 = new CategoryWithSentinel { Id = IntSentinel, Name = "Beverages" };
        var product1 = new ProductWithSentinel
        {
            Id = IntSentinel,
            Name = "Marmite",
            Price = 7.99m
        };
        var gu1 = new TheGuWithSentinel { Id = GuidSentinel, ShirtColor = "Red" };

        var categoryEntry1 = await categoryAdder(context, category1);
        var productEntry1 = await productAdder(context, product1);
        var guEntry1 = await guAdder(context, gu1);

        Assert.Same(category1, categoryEntry1.Entity);
        Assert.Same(product1, productEntry1.Entity);
        Assert.Same(gu1, guEntry1.Entity);

        Assert.Same(category1, categoryEntry1.Entity);
        Assert.Equal(expectedState, categoryEntry1.State);

        Assert.Same(product1, productEntry1.Entity);
        Assert.Equal(expectedState, productEntry1.State);

        Assert.Same(gu1, guEntry1.Entity);
        Assert.Equal(expectedState, guEntry1.State);

        Assert.Same(categoryEntry1.GetInfrastructure(), context.Entry(category1).GetInfrastructure());
        Assert.Same(productEntry1.GetInfrastructure(), context.Entry(product1).GetInfrastructure());
        Assert.Same(guEntry1.GetInfrastructure(), context.Entry(gu1).GetInfrastructure());
    }

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_with_default_values_to_context()
        => TrackMultipleEntitiesDefaultValuesTest((c, e) => c.AddRange(e[0]), (c, e) => c.AddRange(e[0]), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_with_default_values_to_context_async()
        => TrackMultipleEntitiesDefaultValuesTest(
            (c, e) => c.AddRangeAsync(e[0]), (c, e) => c.AddRangeAsync(e[0]), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_attached()
        => TrackMultipleEntitiesDefaultValuesTest((c, e) => c.AttachRange(e[0]), (c, e) => c.AttachRange(e[0]), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_updated()
        => TrackMultipleEntitiesDefaultValuesTest((c, e) => c.UpdateRange(e[0]), (c, e) => c.UpdateRange(e[0]), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_deleted()
        => TrackMultipleEntitiesDefaultValuesTest(
            (c, e) => c.RemoveRange(e[0]), (c, e) => c.RemoveRange(e[0]), EntityState.Deleted);

    private static Task TrackMultipleEntitiesDefaultValuesTest(
        Action<DbContext, object[]> categoryAdder,
        Action<DbContext, object[]> productAdder,
        EntityState expectedState)
        => TrackMultipleEntitiesDefaultValuesTest(
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

    // Issue #3890
    private static async Task TrackMultipleEntitiesDefaultValuesTest(
        Func<DbContext, object[], Task> categoryAdder,
        Func<DbContext, object[], Task> productAdder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category1 = new Category
        {
            Id = 0,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product1 = new Product
        {
            Id = 0,
            Name = "Marmite",
            Price = 7.99m,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        await categoryAdder(context, [category1]);
        await productAdder(context, [product1]);

        Assert.Same(category1, context.Entry(category1).Entity);
        Assert.Same(product1, context.Entry(product1).Entity);

        Assert.Same(category1, context.Entry(category1).Entity);
        Assert.Equal(expectedState, context.Entry(category1).State);

        Assert.Same(product1, context.Entry(product1).Entity);
        Assert.Equal(expectedState, context.Entry(product1).State);
    }

    [ConditionalFact]
    public void Can_add_no_new_entities_to_context()
        => TrackNoEntitiesTest(c => c.AddRange(), c => c.AddRange());

    [ConditionalFact]
    public async Task Can_add_no_new_entities_to_context_async()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        await context.AddRangeAsync();
        await context.AddRangeAsync();
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_context_to_be_attached()
        => TrackNoEntitiesTest(c => c.AttachRange(), c => c.AttachRange());

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_context_to_be_updated()
        => TrackNoEntitiesTest(c => c.UpdateRange(), c => c.UpdateRange());

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_context_to_be_deleted()
        => TrackNoEntitiesTest(c => c.RemoveRange(), c => c.RemoveRange());

    private static void TrackNoEntitiesTest(Action<DbContext> categoryAdder, Action<DbContext> productAdder)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        categoryAdder(context);
        productAdder(context);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public Task Can_add_existing_entities_to_context_to_be_deleted_non_generic()
        => TrackEntitiesTestNonGeneric((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);

    [ConditionalFact]
    public Task Can_add_new_entities_to_context_non_generic_graph()
        => TrackEntitiesTestNonGeneric((c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_new_entities_to_context_non_generic_graph_async()
        => TrackEntitiesTestNonGeneric((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_existing_entities_to_context_to_be_attached_non_generic_graph()
        => TrackEntitiesTestNonGeneric((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Unchanged);

    [ConditionalFact]
    public Task Can_add_existing_entities_to_context_to_be_updated_non_generic_graph()
        => TrackEntitiesTestNonGeneric((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);

    private static Task TrackEntitiesTestNonGeneric(
        Func<DbContext, object, EntityEntry> categoryAdder,
        Func<DbContext, object, EntityEntry> productAdder,
        EntityState expectedState)
        => TrackEntitiesTestNonGeneric(
            (c, e) => ValueTask.FromResult(categoryAdder(c, e)),
            (c, e) => ValueTask.FromResult(productAdder(c, e)),
            expectedState);

    private static async Task TrackEntitiesTestNonGeneric(
        Func<DbContext, object, ValueTask<EntityEntry>> categoryAdder,
        Func<DbContext, object, ValueTask<EntityEntry>> productAdder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var relatedDependent = new Product
        {
            Id = 1,
            Name = "Marmite",
            Price = 7.99m,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var principal = new Category
        {
            Id = 1,
            Name = "Beverages",
            Products = [relatedDependent],
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var relatedPrincipal = new Category
        {
            Id = 2,
            Name = "Foods",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var dependent = new Product
        {
            Id = 2,
            Name = "Bovril",
            Price = 4.99m,
            Category = relatedPrincipal,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var principalEntry = await categoryAdder(context, principal);
        var dependentEntry = await productAdder(context, dependent);

        var relatedPrincipalEntry = context.Entry(relatedPrincipal);
        var relatedDependentEntry = context.Entry(relatedDependent);

        Assert.Same(principal, principalEntry.Entity);
        Assert.Same(relatedPrincipal, relatedPrincipalEntry.Entity);
        Assert.Same(relatedDependent, relatedDependentEntry.Entity);
        Assert.Same(dependent, dependentEntry.Entity);

        Assert.Same(principal, principalEntry.Entity);
        Assert.Equal(expectedState, principalEntry.State);
        Assert.Same(relatedPrincipal, relatedPrincipalEntry.Entity);
        Assert.Equal(expectedState == EntityState.Deleted ? EntityState.Unchanged : expectedState, relatedPrincipalEntry.State);

        Assert.Same(relatedDependent, relatedDependentEntry.Entity);
        Assert.Equal(expectedState, relatedDependentEntry.State);
        Assert.Same(dependent, dependentEntry.Entity);
        Assert.Equal(expectedState, dependentEntry.State);

        Assert.Same(principalEntry.GetInfrastructure(), context.Entry(principal).GetInfrastructure());
        Assert.Same(relatedPrincipalEntry.GetInfrastructure(), context.Entry(relatedPrincipal).GetInfrastructure());
        Assert.Same(relatedDependentEntry.GetInfrastructure(), context.Entry(relatedDependent).GetInfrastructure());
        Assert.Same(dependentEntry.GetInfrastructure(), context.Entry(dependent).GetInfrastructure());
    }

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_context_to_be_deleted_Enumerable()
        => TrackMultipleEntitiesTestEnumerable((c, e) => c.RemoveRange(e), EntityState.Deleted);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_context_Enumerable_graph()
        => TrackMultipleEntitiesTestEnumerable((c, e) => c.AddRange(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_to_context_Enumerable_graph_async()
        => TrackMultipleEntitiesTestEnumerable((c, e) => c.AddRangeAsync(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_context_to_be_attached_Enumerable_graph()
        => TrackMultipleEntitiesTestEnumerable((c, e) => c.AttachRange(e), EntityState.Unchanged);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_to_context_to_be_updated_Enumerable_graph()
        => TrackMultipleEntitiesTestEnumerable((c, e) => c.UpdateRange(e), EntityState.Modified);

    private static Task TrackMultipleEntitiesTestEnumerable(
        Action<DbContext, IEnumerable<object>> adder,
        EntityState expectedState)
        => TrackMultipleEntitiesTestEnumerable(
            (c, e) =>
            {
                adder(c, e);
                return Task.FromResult(0);
            },
            expectedState);

    private static async Task TrackMultipleEntitiesTestEnumerable(
        Func<DbContext, IEnumerable<object>, Task> adder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var relatedDependent = new Product
        {
            Id = 1,
            Name = "Marmite",
            Price = 7.99m,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var principal = new Category
        {
            Id = 1,
            Name = "Beverages",
            Products = [relatedDependent],
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var relatedPrincipal = new Category
        {
            Id = 2,
            Name = "Foods",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var dependent = new Product
        {
            Id = 2,
            Name = "Bovril",
            Price = 4.99m,
            Category = relatedPrincipal,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        await adder(context, new object[] { principal, dependent });

        Assert.Same(principal, context.Entry(principal).Entity);
        Assert.Same(relatedPrincipal, context.Entry(relatedPrincipal).Entity);
        Assert.Same(relatedDependent, context.Entry(relatedDependent).Entity);
        Assert.Same(dependent, context.Entry(dependent).Entity);

        Assert.Same(principal, context.Entry(principal).Entity);
        Assert.Equal(expectedState, context.Entry(principal).State);
        Assert.Same(relatedPrincipal, context.Entry(relatedPrincipal).Entity);
        Assert.Equal(
            expectedState == EntityState.Deleted ? EntityState.Unchanged : expectedState, context.Entry(relatedPrincipal).State);

        Assert.Same(relatedDependent, context.Entry(relatedDependent).Entity);
        Assert.Equal(expectedState, context.Entry(relatedDependent).State);
        Assert.Same(dependent, context.Entry(dependent).Entity);
        Assert.Equal(expectedState, context.Entry(dependent).State);
    }

    [ConditionalFact]
    public Task Can_add_existing_entities_with_default_value_to_context_to_be_deleted_non_generic()
        => TrackEntitiesDefaultValuesTestNonGeneric((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);

    [ConditionalFact]
    public Task Can_add_new_entities_with_default_value_to_context_non_generic_graph()
        => TrackEntitiesDefaultValuesTestNonGeneric((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_new_entities_with_default_value_to_context_non_generic_graph_async()
        => TrackEntitiesDefaultValuesTestNonGeneric((c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_existing_entities_with_default_value_to_context_to_be_attached_non_generic_graph()
        => TrackEntitiesDefaultValuesTestNonGeneric((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_existing_entities_with_default_value_to_context_to_be_updated_non_generic_graph()
        => TrackEntitiesDefaultValuesTestNonGeneric((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Added);

    private static Task TrackEntitiesDefaultValuesTestNonGeneric(
        Func<DbContext, object, EntityEntry> categoryAdder,
        Func<DbContext, object, EntityEntry> productAdder,
        EntityState expectedState)
        => TrackEntitiesDefaultValuesTestNonGeneric(
            (c, e) => ValueTask.FromResult(categoryAdder(c, e)),
            (c, e) => ValueTask.FromResult(productAdder(c, e)),
            expectedState);

    // Issue #3890
    private static async Task TrackEntitiesDefaultValuesTestNonGeneric(
        Func<DbContext, object, ValueTask<EntityEntry>> categoryAdder,
        Func<DbContext, object, ValueTask<EntityEntry>> productAdder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category1 = new Category
        {
            Id = 0,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product1 = new Product
        {
            Id = 0,
            Name = "Marmite",
            Price = 7.99m,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var categoryEntry1 = await categoryAdder(context, category1);
        var productEntry1 = await productAdder(context, product1);

        Assert.Same(category1, categoryEntry1.Entity);
        Assert.Same(product1, productEntry1.Entity);

        Assert.Same(category1, categoryEntry1.Entity);
        Assert.Equal(expectedState, categoryEntry1.State);

        Assert.Same(product1, productEntry1.Entity);
        Assert.Equal(expectedState, productEntry1.State);

        Assert.Same(categoryEntry1.GetInfrastructure(), context.Entry(category1).GetInfrastructure());
        Assert.Same(productEntry1.GetInfrastructure(), context.Entry(product1).GetInfrastructure());
    }

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_deleted_Enumerable()
        => TrackMultipleEntitiesDefaultValueTestEnumerable(
            (c, e) => c.RemoveRange(e), (c, e) => c.RemoveRange(e), EntityState.Deleted);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_with_default_values_to_context_Enumerable_graph()
        => TrackMultipleEntitiesDefaultValueTestEnumerable((c, e) => c.AddRange(e), (c, e) => c.AddRange(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_new_entities_with_default_values_to_context_Enumerable_graph_async()
        => TrackMultipleEntitiesDefaultValueTestEnumerable(
            (c, e) => c.AddRangeAsync(e), (c, e) => c.AddRangeAsync(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_attached_Enumerable_graph()
        => TrackMultipleEntitiesDefaultValueTestEnumerable(
            (c, e) => c.AttachRange(e), (c, e) => c.AttachRange(e), EntityState.Added);

    [ConditionalFact]
    public Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_updated_Enumerable_graph()
        => TrackMultipleEntitiesDefaultValueTestEnumerable(
            (c, e) => c.UpdateRange(e), (c, e) => c.UpdateRange(e), EntityState.Added);

    private static Task TrackMultipleEntitiesDefaultValueTestEnumerable(
        Action<DbContext, IEnumerable<object>> categoryAdder,
        Action<DbContext, IEnumerable<object>> productAdder,
        EntityState expectedState)
        => TrackMultipleEntitiesDefaultValueTestEnumerable(
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

    // Issue #3890
    private static async Task TrackMultipleEntitiesDefaultValueTestEnumerable(
        Func<DbContext, IEnumerable<object>, Task> categoryAdder,
        Func<DbContext, IEnumerable<object>, Task> productAdder,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category1 = new Category
        {
            Id = 0,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product1 = new Product
        {
            Id = 0,
            Name = "Marmite",
            Price = 7.99m,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        await categoryAdder(
            context, new List<Category> { category1 });
        await productAdder(
            context, new List<Product> { product1 });

        Assert.Same(category1, context.Entry(category1).Entity);
        Assert.Same(product1, context.Entry(product1).Entity);

        Assert.Same(category1, context.Entry(category1).Entity);
        Assert.Equal(expectedState, context.Entry(category1).State);

        Assert.Same(product1, context.Entry(product1).Entity);
        Assert.Equal(expectedState, context.Entry(product1).State);
    }

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_context_to_be_deleted_Enumerable()
        => TrackNoEntitiesTestEnumerable((c, e) => c.RemoveRange(e), (c, e) => c.RemoveRange(e));

    [ConditionalFact]
    public void Can_add_no_new_entities_to_context_Enumerable_graph()
        => TrackNoEntitiesTestEnumerable((c, e) => c.AddRange(e), (c, e) => c.AddRange(e));

    [ConditionalFact]
    public async Task Can_add_no_new_entities_to_context_Enumerable_graph_async()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        await context.AddRangeAsync(new HashSet<Category>());
        await context.AddRangeAsync(new HashSet<Product>());
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_context_to_be_attached_Enumerable_graph()
        => TrackNoEntitiesTestEnumerable((c, e) => c.AttachRange(e), (c, e) => c.AttachRange(e));

    [ConditionalFact]
    public void Can_add_no_existing_entities_to_context_to_be_updated_Enumerable_graph()
        => TrackNoEntitiesTestEnumerable((c, e) => c.UpdateRange(e), (c, e) => c.UpdateRange(e));

    private static void TrackNoEntitiesTestEnumerable(
        Action<DbContext, IEnumerable<object>> categoryAdder,
        Action<DbContext, IEnumerable<object>> productAdder)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        categoryAdder(context, new HashSet<Category>());
        productAdder(context, new HashSet<Product>());
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    public async Task Can_add_new_entities_to_context_with_key_generation_graph(bool attachFirst, bool useEntry, bool async)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var gu1 = new TheGu { ShirtColor = "Red" };
        var gu2 = new TheGu { ShirtColor = "Still Red" };

        if (attachFirst)
        {
            context.Entry(gu1).State = EntityState.Unchanged;
            Assert.Equal(default, gu1.Id);
            Assert.Equal(EntityState.Unchanged, context.Entry(gu1).State);
        }

        if (async)
        {
            Assert.Same(gu1, (await context.AddAsync(gu1)).Entity);
            Assert.Same(gu2, (await context.AddAsync(gu2)).Entity);
        }
        else
        {
            if (useEntry)
            {
                context.Entry(gu1).State = EntityState.Added;
                context.Entry(gu2).State = EntityState.Added;
            }
            else
            {
                Assert.Same(gu1, context.Add(gu1).Entity);
                Assert.Same(gu2, context.Add(gu2).Entity);
            }
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
    public async Task Can_use_Remove_to_change_entity_state()
    {
        await ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Detached, EntityState.Deleted);
        await ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Unchanged, EntityState.Deleted);
        await ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Deleted, EntityState.Deleted);
        await ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Modified, EntityState.Deleted);
        await ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Added, EntityState.Detached);
    }

    [ConditionalFact]
    public async Task Can_use_graph_Add_to_change_entity_state()
    {
        await ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Detached, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Unchanged, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Deleted, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Modified, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Added, EntityState.Added);
    }

    [ConditionalFact]
    public async Task Can_use_graph_Add_to_change_entity_state_async()
    {
        await ChangeStateWithMethod((c, e) => c.AddAsync(e), EntityState.Detached, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.AddAsync(e), EntityState.Unchanged, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.AddAsync(e), EntityState.Deleted, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.AddAsync(e), EntityState.Modified, EntityState.Added);
        await ChangeStateWithMethod((c, e) => c.AddAsync(e), EntityState.Added, EntityState.Added);
    }

    [ConditionalFact]
    public async Task Can_use_graph_Attach_to_change_entity_state()
    {
        await ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Detached, EntityState.Unchanged);
        await ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Unchanged, EntityState.Unchanged);
        await ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Deleted, EntityState.Unchanged);
        await ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Modified, EntityState.Unchanged);
        await ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Added, EntityState.Unchanged);
    }

    [ConditionalFact]
    public async Task Can_use_graph_Update_to_change_entity_state()
    {
        await ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Detached, EntityState.Modified);
        await ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Unchanged, EntityState.Modified);
        await ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Deleted, EntityState.Modified);
        await ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Modified, EntityState.Modified);
        await ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Added, EntityState.Modified);
    }

    private Task ChangeStateWithMethod(
        Action<DbContext, object> action,
        EntityState initialState,
        EntityState expectedState)
        => ChangeStateWithMethod(
            (c, e) =>
            {
                action(c, e);
                return new ValueTask<EntityEntry>();
            },
            initialState,
            expectedState);

    private async Task ChangeStateWithMethod(
        Func<DbContext, object, ValueTask<EntityEntry>> action,
        EntityState initialState,
        EntityState expectedState)
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var entity = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var entry = context.Entry(entity);

        entry.State = initialState;

        await action(context, entity);

        Assert.Equal(expectedState, entry.State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_principal_first_fully_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Attach(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

        // Dependent is Unchanged here because the FK change happened before it was attached
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_dependent_first_fully_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Attach(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Attach(category);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_principal_first_collection_not_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [];

        context.Attach(category);

        Assert.Equal(7, product.CategoryId);
        Assert.Empty(category.Products);
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Attach(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_dependent_first_collection_not_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [];

        context.Attach(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Attach(category);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_principal_first_reference_not_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Null(product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Attach(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_dependent_first_reference_not_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Attach(product);

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Null(product.Category);
        Assert.Equal(EntityState.Detached, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Attach(category);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_fully_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

        // Dependent is Unchanged here because the FK change happened before it was attached
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_fully_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Detached, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_collection_not_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [];

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Empty(category.Products);
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_collection_not_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [];

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Empty(category.Products);
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Detached, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_reference_not_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Null(product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_reference_not_fixed_up()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Null(product.Category);
        Assert.Equal(EntityState.Detached, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_principal_first_fully_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Attach(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Same(product, category.Products.Single());
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_dependent_first_fully_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        category.Products = [product];

        context.Attach(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Attach(category);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_principal_first_collection_not_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        category.Products = [];

        context.Attach(category);

        Assert.Equal(7, product.CategoryId);
        Assert.Empty(category.Products);
        Assert.Same(category, product.Category);
        Assert.Empty(category.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Attach(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_dependent_first_collection_not_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        category.Products = [];

        context.Attach(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Attach(category);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_principal_first_reference_not_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Null(product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Attach(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Same(product, category.Products.Single());
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_attach_with_inconsistent_FK_dependent_first_reference_not_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Attach(product);

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category7, product.Category);
        Assert.Same(product, category7.Products.Single());
        Assert.Equal(EntityState.Detached, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Attach(category);

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_fully_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Same(product, category.Products.Single());
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_fully_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        category.Products = [product];

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);
        Assert.Equal(EntityState.Detached, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_collection_not_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [];

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Empty(category.Products);
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_collection_not_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Category = category,
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };

        category.Products = [];

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Empty(category.Products);
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Detached, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category7).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_reference_not_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Null(product.Category);
        Assert.Empty(category7.Products);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Detached, context.Entry(product).State);

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Same(product, category.Products.Single());
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalFact] // Issue #1246
    public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_reference_not_fixed_up_with_tracked_FK_match()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var category7 = context.Attach(
            new Category
            {
                Id = 7,
                Products = [],
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        var category = new Category
        {
            Id = 1,
            Name = "Beverages",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        var product = new Product
        {
            Id = 1,
            CategoryId = 7,
            Name = "Marmite",
            Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
            Tag = new Tag
            {
                Name = "Tanavast",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                Notes = ["A", "B"]
            }
        };
        category.Products = [product];

        context.Entry(product).State = EntityState.Unchanged;

        Assert.Equal(7, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category7, product.Category);
        Assert.Same(product, category7.Products.Single());
        Assert.Equal(EntityState.Detached, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

        context.Entry(category).State = EntityState.Unchanged;

        Assert.Equal(1, product.CategoryId);
        Assert.Same(product, category.Products.Single());
        Assert.Same(category, product.Category);
        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
    }

    [ConditionalTheory] // Issue #17828
    [InlineData(CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never)]
    [InlineData(CascadeTiming.OnSaveChanges)]
    public void Can_reparent_optional_without_DetectChanges(CascadeTiming cascadeTiming)
    {
        using var context = new Parent77Context();

        context.ChangeTracker.CascadeDeleteTiming = cascadeTiming;

        var parent1 = new Parent77();
        var parent2 = new Parent77();
        var child = new Optional77();

        child.Parent77 = parent1;
        context.AddRange(parent1, parent2, child);
        context.SaveChanges();

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(parent1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(child).State);

        child.Parent77 = parent2;
        context.Remove(parent1);

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Deleted, context.Entry(parent1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent2).State);
        Assert.Equal(EntityState.Modified, context.Entry(child).State);
        Assert.Same(parent2, child.Parent77);

        context.SaveChanges();

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Detached, context.Entry(parent1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(child).State);
        Assert.Same(parent2, child.Parent77);
    }

    [ConditionalTheory] // Issue #17828
    [InlineData(CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never)]
    [InlineData(CascadeTiming.OnSaveChanges)]
    public void Can_reparent_required_without_DetectChanges(CascadeTiming cascadeTiming)
    {
        using var context = new Parent77Context();

        context.ChangeTracker.CascadeDeleteTiming = cascadeTiming;

        var parent1 = new Parent77();
        var parent2 = new Parent77();
        var child = new Required77();

        child.Parent77 = parent1;
        context.AddRange(parent1, parent2, child);
        context.SaveChanges();

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(parent1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(child).State);

        child.Parent77 = parent2;
        context.Remove(parent1);

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Deleted, context.Entry(parent1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent2).State);
        Assert.Equal(EntityState.Modified, context.Entry(child).State);
        Assert.Same(parent2, child.Parent77);

        context.SaveChanges();

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Detached, context.Entry(parent1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(child).State);
        Assert.Same(parent2, child.Parent77);
    }

    private class Parent77Context : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(nameof(Parent77Context));

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Parent77>(
                b =>
                {
                    b.HasMany<Optional77>().WithOne(e => e.Parent77);
                    b.HasMany<Required77>().WithOne(e => e.Parent77);
                });
    }

    private class Parent77
    {
        public int Id { get; set; }
    }

    private class Optional77
    {
        public int Id { get; set; }

        public int? Parent77Id { get; set; }
        public Parent77 Parent77 { get; set; }
    }

    private class Required77
    {
        public int Id { get; set; }

        public int Parent77Id { get; set; }
        public Parent77 Parent77 { get; set; }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class NavigationFixerTest
{
    [ConditionalFact]
    public void Does_not_throw_if_Add_during_fixup()
    {
        using var context = new FixupContext();
        var blog1 = new Blog { Id = 1 };
        var blog2 = new Blog { Id = 2 };

        var post1 = context.Add(
            new Post { BlogId = 2 }).Entity;

        blog1.Posts.Add(post1);
        blog1.Posts.Add(
            new Post { BlogId = 2 });

        context.Add(blog2);
        context.Add(blog1);
    }

    private class FixupContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(typeof(FixupContext).FullName);
    }

    private class Blog
    {
        public int Id { get; set; }
        public HashSet<Post> Posts { get; } = [];
    }

    private class Post
    {
        private Blog _blog;
        public int Id { get; set; }
        public int BlogId { get; set; }

        public Blog Blog
        {
            get => _blog;
            set
            {
                _blog = value;
                _blog.Posts.Add(new Post());
            }
        }
    }

    [ConditionalFact]
    public void Does_fixup_of_related_principals()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Category { Id = 11 };
        var principal2 = new Category { Id = 12 };
        var dependent = new Product { Id = 21, CategoryId = 12 };

        manager.StartTracking(manager.GetOrCreateEntry(principal1));
        manager.StartTracking(manager.GetOrCreateEntry(principal2));

        var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

        dependentEntry.SetEntityState(EntityState.Added);

        Assert.Same(dependent.Category, principal2);
        Assert.Contains(dependent, principal2.Products);
        Assert.DoesNotContain(dependent, principal1.Products);
    }

    [ConditionalFact]
    public void Does_fixup_of_related_dependents()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var dependent1 = new Product { Id = 21, CategoryId = 11 };
        var dependent2 = new Product { Id = 22, CategoryId = 12 };
        var dependent3 = new Product { Id = 23, CategoryId = 11 };

        var principal = new Category { Id = 11 };

        manager.StartTracking(manager.GetOrCreateEntry(dependent1));
        manager.StartTracking(manager.GetOrCreateEntry(dependent2));
        manager.StartTracking(manager.GetOrCreateEntry(dependent3));

        var principalEntry = manager.StartTracking(manager.GetOrCreateEntry(principal));

        var fixer = CreateNavigationFixer(contextServices);
        fixer.StateChanged(principalEntry, EntityState.Detached, fromQuery: false);

        Assert.Same(dependent1.Category, principal);
        Assert.Null(dependent2.Category);
        Assert.Same(dependent3.Category, principal);

        Assert.Contains(dependent1, principal.Products);
        Assert.DoesNotContain(dependent2, principal.Products);
        Assert.Contains(dependent3, principal.Products);
    }

    [ConditionalFact]
    public void Does_fixup_of_one_to_one_relationship()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Product { Id = 21 };
        var principal2 = new Product { Id = 22 };
        var principal3 = new Product { Id = 23 };

        var dependent1 = new ProductDetail { Id = 21 };
        var dependent2 = new ProductDetail { Id = 22 };
        var dependent4 = new ProductDetail { Id = 24 };

        var principalEntry1 = manager.StartTracking(manager.GetOrCreateEntry(principal1));
        var principalEntry2 = manager.StartTracking(manager.GetOrCreateEntry(principal2));
        var principalEntry3 = manager.StartTracking(manager.GetOrCreateEntry(principal3));

        var dependentEntry1 = manager.StartTracking(manager.GetOrCreateEntry(dependent1));
        var dependentEntry2 = manager.StartTracking(manager.GetOrCreateEntry(dependent2));
        var dependentEntry4 = manager.StartTracking(manager.GetOrCreateEntry(dependent4));

        Assert.Null(principal1.Detail);
        Assert.Null(dependent1.Product);

        principalEntry1.SetEntityState(EntityState.Added);

        Assert.Same(principal1, dependent1.Product);
        Assert.Same(dependent1, principal1.Detail);

        Assert.Null(principal2.Detail);
        Assert.Null(dependent2.Product);

        dependentEntry2.SetEntityState(EntityState.Added);

        Assert.Same(principal2, dependent2.Product);
        Assert.Same(dependent2, principal2.Detail);

        Assert.Null(principal3.Detail);
        Assert.Null(dependent4.Product);

        principalEntry3.SetEntityState(EntityState.Added);
        dependentEntry4.SetEntityState(EntityState.Added);

        Assert.Null(principal3.Detail);
        Assert.Null(dependent4.Product);
    }

    [ConditionalFact]
    public void Does_fixup_of_one_to_one_self_referencing_relationship()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var entity1 = new Product { Id = 21, AlternateProductId = 22 };
        var entity2 = new Product { Id = 22, AlternateProductId = 23 };
        var entity3 = new Product { Id = 23 };

        var entry1 = manager.StartTracking(manager.GetOrCreateEntry(entity1));
        var entry2 = manager.StartTracking(manager.GetOrCreateEntry(entity2));
        var entry3 = manager.StartTracking(manager.GetOrCreateEntry(entity3));

        Assert.Null(entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Null(entity2.AlternateProduct);
        Assert.Null(entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Null(entity3.OriginalProduct);

        entry1.SetEntityState(EntityState.Added);

        Assert.Same(entity2, entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Null(entity2.AlternateProduct);
        Assert.Same(entity1, entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Null(entity3.OriginalProduct);

        entry3.SetEntityState(EntityState.Added);

        Assert.Same(entity2, entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Same(entity3, entity2.AlternateProduct);
        Assert.Same(entity1, entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Same(entity2, entity3.OriginalProduct);
    }

    [ConditionalFact]
    public void Does_fixup_of_FKs_and_related_principals_using_dependent_navigations()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Category { Id = 11 };
        var principal2 = new Category { Id = 12 };
        var dependent = new Product { Id = 21, Category = principal2 };

        manager.StartTracking(manager.GetOrCreateEntry(principal1));
        manager.StartTracking(manager.GetOrCreateEntry(principal2));

        var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

        dependentEntry.SetEntityState(EntityState.Added);

        Assert.Equal(12, dependent.CategoryId);
        Assert.Same(dependent.Category, principal2);
        Assert.Contains(dependent, principal2.Products);
        Assert.DoesNotContain(dependent, principal1.Products);
    }

    [ConditionalFact]
    public void Does_fixup_of_FKs_and_related_principals_using_principal_navigations()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Category { Id = 11 };
        var principal2 = new Category { Id = 12 };
        var dependent = new Product { Id = 21 };

        principal2.Products.Add(dependent);

        manager.StartTracking(manager.GetOrCreateEntry(principal1)).SetEntityState(EntityState.Added);
        manager.StartTracking(manager.GetOrCreateEntry(principal2)).SetEntityState(EntityState.Added);

        var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

        dependentEntry.SetEntityState(EntityState.Added);

        Assert.Equal(12, dependent.CategoryId);
        Assert.Same(dependent.Category, principal2);
        Assert.Contains(dependent, principal2.Products);
        Assert.DoesNotContain(dependent, principal1.Products);
    }

    [ConditionalFact]
    public void Does_fixup_of_FKs_and_related_dependents_using_dependent_navigations()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal = new Category { Id = 11 };

        var dependent1 = new Product { Id = 21, Category = principal };
        var dependent2 = new Product { Id = 22 };
        var dependent3 = new Product { Id = 23, Category = principal };

        manager.StartTracking(manager.GetOrCreateEntry(dependent1)).SetEntityState(EntityState.Added);
        manager.StartTracking(manager.GetOrCreateEntry(dependent2)).SetEntityState(EntityState.Added);
        manager.StartTracking(manager.GetOrCreateEntry(dependent3)).SetEntityState(EntityState.Added);

        var principalEntry = manager.StartTracking(manager.GetOrCreateEntry(principal));

        principalEntry.SetEntityState(EntityState.Added);

        Assert.Equal(11, dependent1.CategoryId);
        Assert.Equal(0, dependent2.CategoryId);
        Assert.Equal(11, dependent3.CategoryId);

        Assert.Same(dependent1.Category, principal);
        Assert.Null(dependent2.Category);
        Assert.Same(dependent3.Category, principal);

        Assert.Contains(dependent1, principal.Products);
        Assert.DoesNotContain(dependent2, principal.Products);
        Assert.Contains(dependent3, principal.Products);
    }

    [ConditionalFact]
    public void Does_fixup_of_FKs_and_related_dependents_using_principal_navigations()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal = new Category { Id = 11 };

        var dependent1 = new Product { Id = 21 };
        var dependent2 = new Product { Id = 22 };
        var dependent3 = new Product { Id = 23 };

        principal.Products.Add(dependent1);
        principal.Products.Add(dependent3);

        manager.StartTracking(manager.GetOrCreateEntry(dependent1)).SetEntityState(EntityState.Added);
        manager.StartTracking(manager.GetOrCreateEntry(dependent2)).SetEntityState(EntityState.Added);
        manager.StartTracking(manager.GetOrCreateEntry(dependent3)).SetEntityState(EntityState.Added);

        var principalEntry = manager.StartTracking(manager.GetOrCreateEntry(principal));

        principalEntry.SetEntityState(EntityState.Added);

        Assert.Equal(11, dependent1.CategoryId);
        Assert.Equal(0, dependent2.CategoryId);
        Assert.Equal(11, dependent3.CategoryId);

        Assert.Same(dependent1.Category, principal);
        Assert.Null(dependent2.Category);
        Assert.Same(dependent3.Category, principal);

        Assert.Contains(dependent1, principal.Products);
        Assert.DoesNotContain(dependent2, principal.Products);
        Assert.Contains(dependent3, principal.Products);
    }

    [ConditionalFact]
    public void Does_fixup_of_one_to_one_self_referencing_relationship_using_dependent_navigations()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var entity1 = new Product { Id = 21 };
        var entity2 = new Product { Id = 22 };
        var entity3 = new Product { Id = 23 };

        entity1.AlternateProduct = entity2;
        entity2.AlternateProduct = entity3;

        var entry1 = manager.StartTracking(manager.GetOrCreateEntry(entity1));
        var entry2 = manager.StartTracking(manager.GetOrCreateEntry(entity2));
        var entry3 = manager.StartTracking(manager.GetOrCreateEntry(entity3));

        Assert.Null(entity1.AlternateProductId);
        Assert.Null(entity2.AlternateProductId);
        Assert.Null(entity3.AlternateProductId);

        Assert.Same(entity2, entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Same(entity3, entity2.AlternateProduct);
        Assert.Null(entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Null(entity3.OriginalProduct);

        entry1.SetEntityState(EntityState.Added);

        Assert.Equal(22, entity1.AlternateProductId);
        Assert.Null(entity2.AlternateProductId);
        Assert.Null(entity3.AlternateProductId);

        Assert.Same(entity2, entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Same(entity3, entity2.AlternateProduct);
        Assert.Same(entity1, entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Null(entity3.OriginalProduct);

        entry3.SetEntityState(EntityState.Added);

        Assert.Equal(22, entity1.AlternateProductId);
        Assert.Null(entity2.AlternateProductId);
        Assert.Null(entity3.AlternateProductId);

        Assert.Same(entity2, entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Same(entity3, entity2.AlternateProduct);
        Assert.Same(entity1, entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Null(entity3.OriginalProduct);
    }

    [ConditionalFact]
    public void Does_fixup_of_one_to_one_self_referencing_relationship_using_principal_navigations()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var entity1 = new Product { Id = 21 };
        var entity2 = new Product { Id = 22 };
        var entity3 = new Product { Id = 23 };

        entity2.OriginalProduct = entity1;
        entity3.OriginalProduct = entity2;

        var entry1 = manager.StartTracking(manager.GetOrCreateEntry(entity1));
        var entry2 = manager.StartTracking(manager.GetOrCreateEntry(entity2));
        var entry3 = manager.StartTracking(manager.GetOrCreateEntry(entity3));

        Assert.Null(entity1.AlternateProductId);
        Assert.Null(entity2.AlternateProductId);
        Assert.Null(entity3.AlternateProductId);

        Assert.Null(entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Null(entity2.AlternateProduct);
        Assert.Same(entity1, entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Same(entity2, entity3.OriginalProduct);

        entry1.SetEntityState(EntityState.Added);

        Assert.Null(entity1.AlternateProductId);
        Assert.Null(entity2.AlternateProductId);
        Assert.Null(entity3.AlternateProductId);

        Assert.Null(entity2.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Null(entity2.AlternateProduct);
        Assert.Same(entity1, entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Same(entity2, entity3.OriginalProduct);

        entry3.SetEntityState(EntityState.Added);

        Assert.Null(entity1.AlternateProductId);
        Assert.Null(entity2.AlternateProductId);
        Assert.Null(entity3.AlternateProductId);

        Assert.Null(entity2.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Null(entity2.AlternateProduct);
        Assert.Same(entity1, entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Same(entity2, entity3.OriginalProduct);

        entry2.SetEntityState(EntityState.Added);

        Assert.Equal(22, entity1.AlternateProductId);
        Assert.Equal(23, entity2.AlternateProductId);
        Assert.Null(entity3.AlternateProductId);

        Assert.Same(entity2, entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Same(entity3, entity2.AlternateProduct);
        Assert.Same(entity1, entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Same(entity2, entity3.OriginalProduct);
    }

    [ConditionalFact]
    public void Does_fixup_of_related_principals_when_FK_is_set()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Category { Id = 11 };
        var principal2 = new Category { Id = 12 };
        var dependent = new Product { Id = 21, CategoryId = 0 };

        manager.StartTracking(manager.GetOrCreateEntry(principal1));
        manager.StartTracking(manager.GetOrCreateEntry(principal2));

        var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

        var fixer = CreateNavigationFixer(contextServices);
        dependentEntry.SetEntityState(EntityState.Added);

        Assert.Null(dependent.Category);
        Assert.DoesNotContain(dependent, principal2.Products);
        Assert.DoesNotContain(dependent, principal1.Products);

        dependent.CategoryId = 11;

        var productType = model.FindEntityType(typeof(Product));
        var categoryIdProperty = productType.FindProperty("CategoryId");

        fixer.KeyPropertyChanged(
            dependentEntry,
            categoryIdProperty,
            [],
            productType.GetForeignKeys().Where(k => k.Properties.Contains(categoryIdProperty)).ToList(),
            12,
            11);

        Assert.Same(dependent.Category, principal1);
        Assert.Contains(dependent, principal1.Products);
        Assert.DoesNotContain(dependent, principal2.Products);
    }

    [ConditionalFact]
    public void Does_fixup_of_related_principals_when_FK_is_cleared()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Category { Id = 11 };
        var principal2 = new Category { Id = 12 };
        var dependent = new Product { Id = 21, CategoryId = 12 };

        manager.StartTracking(manager.GetOrCreateEntry(principal1));
        manager.StartTracking(manager.GetOrCreateEntry(principal2));

        var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

        var fixer = CreateNavigationFixer(contextServices);
        dependentEntry.SetEntityState(EntityState.Added);

        Assert.Same(dependent.Category, principal2);
        Assert.Contains(dependent, principal2.Products);
        Assert.DoesNotContain(dependent, principal1.Products);

        dependent.CategoryId = 0;

        var productType = model.FindEntityType(typeof(Product));
        var categoryIdProperty = productType.FindProperty("CategoryId");

        fixer.KeyPropertyChanged(
            dependentEntry,
            categoryIdProperty,
            [],
            productType.GetForeignKeys().Where(k => k.Properties.Contains(categoryIdProperty)).ToList(),
            12,
            11);

        Assert.Null(dependent.Category);
        Assert.DoesNotContain(dependent, principal2.Products);
        Assert.DoesNotContain(dependent, principal1.Products);
    }

    [ConditionalFact]
    public void Does_fixup_of_related_principals_when_FK_is_changed()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Category { Id = 11 };
        var principal2 = new Category { Id = 12 };
        var dependent = new Product { Id = 21, CategoryId = 12 };

        manager.StartTracking(manager.GetOrCreateEntry(principal1));
        manager.StartTracking(manager.GetOrCreateEntry(principal2));

        var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

        var fixer = CreateNavigationFixer(contextServices);
        dependentEntry.SetEntityState(EntityState.Added);

        Assert.Same(dependent.Category, principal2);
        Assert.Contains(dependent, principal2.Products);
        Assert.DoesNotContain(dependent, principal1.Products);

        dependent.CategoryId = 11;

        var productType = model.FindEntityType(typeof(Product));
        var categoryIdProperty = productType.FindProperty("CategoryId");

        fixer.KeyPropertyChanged(
            dependentEntry,
            categoryIdProperty,
            [],
            productType.GetForeignKeys().Where(k => k.Properties.Contains(categoryIdProperty)).ToList(),
            12,
            11);

        Assert.Same(dependent.Category, principal1);
        Assert.Contains(dependent, principal1.Products);
        Assert.DoesNotContain(dependent, principal2.Products);
    }

    [ConditionalFact]
    public void Does_fixup_of_one_to_one_relationship_when_FK_changes()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Product { Id = 21 };
        var principal2 = new Product { Id = 22 };
        var dependent = new ProductDetail { Id = 21 };

        var principalEntry1 = manager.StartTracking(manager.GetOrCreateEntry(principal1));
        var principalEntry2 = manager.StartTracking(manager.GetOrCreateEntry(principal2));
        var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

        var fixer = CreateNavigationFixer(contextServices);

        principalEntry1.SetEntityState(EntityState.Added);
        principalEntry2.SetEntityState(EntityState.Added);
        dependentEntry.SetEntityState(EntityState.Added);

        Assert.Same(principal1, dependent.Product);
        Assert.Same(dependent, principal1.Detail);
        Assert.Null(principal2.Detail);

        dependent.Id = 22;

        var productDetailType = model.FindEntityType(typeof(ProductDetail));
        var idProperty = productDetailType.FindProperty("Id");

        fixer.KeyPropertyChanged(
            dependentEntry,
            idProperty,
            productDetailType.GetKeys().Where(k => k.Properties.Contains(idProperty)).ToList(),
            productDetailType.GetForeignKeys().Where(k => k.Properties.Contains(idProperty)).ToList(),
            21,
            22);

        Assert.Same(principal2, dependent.Product);
        Assert.Same(dependent, principal2.Detail);
        Assert.Null(principal1.Detail);
    }

    [ConditionalFact]
    public void Does_fixup_of_one_to_one_relationship_when_FK_cleared()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal = new Product { Id = 21 };
        var dependent = new ProductDetail { Id = 21 };

        var principalEntry = manager.StartTracking(manager.GetOrCreateEntry(principal));
        var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

        var fixer = CreateNavigationFixer(contextServices);

        principalEntry.SetEntityState(EntityState.Added);
        dependentEntry.SetEntityState(EntityState.Added);

        Assert.Same(principal, dependent.Product);
        Assert.Same(dependent, principal.Detail);

        dependent.Id = 0;

        var productDetailType = model.FindEntityType(typeof(ProductDetail));
        var idProperty = productDetailType.FindProperty("Id");

        fixer.KeyPropertyChanged(
            dependentEntry,
            idProperty,
            productDetailType.GetKeys().Where(k => k.Properties.Contains(idProperty)).ToList(),
            productDetailType.GetForeignKeys().Where(k => k.Properties.Contains(idProperty)).ToList(),
            21,
            0);

        Assert.Null(dependent.Product);
        Assert.Null(principal.Detail);
    }

    [ConditionalFact]
    public void Does_fixup_of_one_to_one_relationship_when_FK_set()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal = new Product { Id = 21 };
        var dependent = new ProductDetail { Id = 7 };

        var principalEntry = manager.StartTracking(manager.GetOrCreateEntry(principal));
        var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

        var fixer = CreateNavigationFixer(contextServices);

        principalEntry.SetEntityState(EntityState.Added);
        dependentEntry.SetEntityState(EntityState.Added);

        Assert.Null(dependent.Product);
        Assert.Null(principal.Detail);

        dependent.Id = 21;

        var productDetailType = model.FindEntityType(typeof(ProductDetail));
        var idProperty = productDetailType.FindProperty("Id");

        fixer.KeyPropertyChanged(
            dependentEntry,
            idProperty,
            productDetailType.GetKeys().Where(k => k.Properties.Contains(idProperty)).ToList(),
            productDetailType.GetForeignKeys().Where(k => k.Properties.Contains(idProperty)).ToList(),
            7,
            21);

        Assert.Same(principal, dependent.Product);
        Assert.Same(dependent, principal.Detail);
    }

    [ConditionalFact]
    public void Does_fixup_of_one_to_one_self_referencing_relationship_when_FK_changes()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var entity1 = new Product { Id = 21, AlternateProductId = 22 };
        var entity2 = new Product { Id = 22 };
        var entity3 = new Product { Id = 23 };

        var entry1 = manager.StartTracking(manager.GetOrCreateEntry(entity1));
        var entry2 = manager.StartTracking(manager.GetOrCreateEntry(entity2));
        var entry3 = manager.StartTracking(manager.GetOrCreateEntry(entity3));

        var fixer = CreateNavigationFixer(contextServices);

        entry1.SetEntityState(EntityState.Added);
        entry2.SetEntityState(EntityState.Added);
        entry3.SetEntityState(EntityState.Added);

        Assert.Same(entity2, entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Null(entity2.AlternateProduct);
        Assert.Same(entity1, entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Null(entity3.OriginalProduct);

        entity1.AlternateProductId = 23;

        var productType = model.FindEntityType(typeof(Product));
        var alternateProductId = productType.FindProperty("AlternateProductId");

        fixer.KeyPropertyChanged(
            entry1,
            alternateProductId,
            [],
            productType.GetForeignKeys().Where(k => k.Properties.Contains(alternateProductId)).ToList(),
            22,
            23);

        Assert.Same(entity3, entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Null(entity2.AlternateProduct);
        Assert.Null(entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Same(entity1, entity3.OriginalProduct);
    }

    [ConditionalFact]
    public void Can_steal_reference_of_one_to_one_self_referencing_relationship_when_FK_changes()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var entity1 = new Product { Id = 21, AlternateProductId = 22 };
        var entity2 = new Product { Id = 22, AlternateProductId = 23 };
        var entity3 = new Product { Id = 23 };

        var entry1 = manager.StartTracking(manager.GetOrCreateEntry(entity1));
        var entry2 = manager.StartTracking(manager.GetOrCreateEntry(entity2));
        var entry3 = manager.StartTracking(manager.GetOrCreateEntry(entity3));

        var fixer = CreateNavigationFixer(contextServices);

        entry1.SetEntityState(EntityState.Added);
        entry2.SetEntityState(EntityState.Added);
        entry3.SetEntityState(EntityState.Added);

        Assert.Same(entity2, entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Same(entity3, entity2.AlternateProduct);
        Assert.Same(entity1, entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Same(entity2, entity3.OriginalProduct);

        entity1.AlternateProductId = 23;

        var productType = model.FindEntityType(typeof(Product));
        var alternateProductId = productType.FindProperty("AlternateProductId");

        fixer.KeyPropertyChanged(
            entry1,
            alternateProductId,
            [],
            productType.GetForeignKeys().Where(k => k.Properties.Contains(alternateProductId)).ToList(),
            22,
            23);

        Assert.Same(entity3, entity1.AlternateProduct);
        Assert.Null(entity1.OriginalProduct);

        Assert.Null(entity2.AlternateProduct);
        Assert.Null(entity2.OriginalProduct);

        Assert.Null(entity3.AlternateProduct);
        Assert.Same(entity1, entity3.OriginalProduct);

        Assert.Null(entity2.AlternateProductId);
    }

    [ConditionalFact]
    public void Does_fixup_of_all_related_principals_when_part_of_overlapping_composite_FK_is_changed()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var photo1 = new ProductPhoto { ProductId = 1, PhotoId = "Photo1" };
        var photo2 = new ProductPhoto { ProductId = 1, PhotoId = "Photo2" };
        var photo3 = new ProductPhoto { ProductId = 2, PhotoId = "Photo1" };
        var photo4 = new ProductPhoto { ProductId = 2, PhotoId = "Photo2" };

        var reviewId1 = Guid.NewGuid();
        var reviewId2 = Guid.NewGuid();
        var review1 = new ProductReview { ProductId = 1, ReviewId = reviewId1 };
        var review2 = new ProductReview { ProductId = 1, ReviewId = reviewId2 };
        var review3 = new ProductReview { ProductId = 2, ReviewId = reviewId1 };
        var review4 = new ProductReview { ProductId = 2, ReviewId = reviewId2 };

        var tag1 = new ProductTag
        {
            Id = 1,
            ProductId = 1,
            PhotoId = "Photo1",
            ReviewId = reviewId1
        };
        var tag2 = new ProductTag
        {
            Id = 2,
            ProductId = 1,
            PhotoId = "Photo1",
            ReviewId = reviewId2
        };
        var tag3 = new ProductTag
        {
            Id = 3,
            ProductId = 1,
            PhotoId = "Photo2",
            ReviewId = reviewId1
        };
        var tag4 = new ProductTag
        {
            Id = 4,
            ProductId = 1,
            PhotoId = "Photo2",
            ReviewId = reviewId2
        };
        var tag5 = new ProductTag
        {
            Id = 5,
            ProductId = 2,
            PhotoId = "Photo1",
            ReviewId = reviewId1
        };
        var tag6 = new ProductTag
        {
            Id = 6,
            ProductId = 2,
            PhotoId = "Photo1",
            ReviewId = reviewId2
        };
        var tag7 = new ProductTag
        {
            Id = 7,
            ProductId = 2,
            PhotoId = "Photo2",
            ReviewId = reviewId1
        };
        var tag8 = new ProductTag
        {
            Id = 8,
            ProductId = 2,
            PhotoId = "Photo2",
            ReviewId = reviewId2
        };

        var photoEntry1 = manager.StartTracking(manager.GetOrCreateEntry(photo1));
        var photoEntry2 = manager.StartTracking(manager.GetOrCreateEntry(photo2));
        var photoEntry3 = manager.StartTracking(manager.GetOrCreateEntry(photo3));
        var photoEntry4 = manager.StartTracking(manager.GetOrCreateEntry(photo4));

        var reviewEntry1 = manager.StartTracking(manager.GetOrCreateEntry(review1));
        var reviewEntry2 = manager.StartTracking(manager.GetOrCreateEntry(review2));
        var reviewEntry3 = manager.StartTracking(manager.GetOrCreateEntry(review3));
        var reviewEntry4 = manager.StartTracking(manager.GetOrCreateEntry(review4));

        var tagEntry1 = manager.StartTracking(manager.GetOrCreateEntry(tag1));
        var tagEntry2 = manager.StartTracking(manager.GetOrCreateEntry(tag2));
        var tagEntry3 = manager.StartTracking(manager.GetOrCreateEntry(tag3));
        var tagEntry4 = manager.StartTracking(manager.GetOrCreateEntry(tag4));
        var tagEntry5 = manager.StartTracking(manager.GetOrCreateEntry(tag5));
        var tagEntry6 = manager.StartTracking(manager.GetOrCreateEntry(tag6));
        var tagEntry7 = manager.StartTracking(manager.GetOrCreateEntry(tag7));
        var tagEntry8 = manager.StartTracking(manager.GetOrCreateEntry(tag8));

        var fixer = CreateNavigationFixer(contextServices);

        photoEntry1.SetEntityState(EntityState.Added);
        photoEntry2.SetEntityState(EntityState.Added);
        photoEntry3.SetEntityState(EntityState.Added);
        photoEntry4.SetEntityState(EntityState.Added);
        reviewEntry1.SetEntityState(EntityState.Added);
        reviewEntry2.SetEntityState(EntityState.Added);
        reviewEntry3.SetEntityState(EntityState.Added);
        reviewEntry4.SetEntityState(EntityState.Added);
        tagEntry1.SetEntityState(EntityState.Added);
        tagEntry2.SetEntityState(EntityState.Added);
        tagEntry3.SetEntityState(EntityState.Added);
        tagEntry4.SetEntityState(EntityState.Added);
        tagEntry5.SetEntityState(EntityState.Added);
        tagEntry6.SetEntityState(EntityState.Added);
        tagEntry7.SetEntityState(EntityState.Added);
        tagEntry8.SetEntityState(EntityState.Added);

        Assert.Equal([tag1, tag2], photo1.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag3, tag4], photo2.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag5, tag6], photo3.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag7, tag8], photo4.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag1, tag3], review1.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag2, tag4], review2.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag5, tag7], review3.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag6, tag8], review4.ProductTags.OrderBy(t => t.Id).ToArray());

        Assert.Same(photo1, tag1.Photo);
        Assert.Same(photo1, tag2.Photo);
        Assert.Same(photo2, tag3.Photo);
        Assert.Same(photo2, tag4.Photo);
        Assert.Same(photo3, tag5.Photo);
        Assert.Same(photo3, tag6.Photo);
        Assert.Same(photo4, tag7.Photo);
        Assert.Same(photo4, tag8.Photo);

        Assert.Same(review1, tag1.Review);
        Assert.Same(review2, tag2.Review);
        Assert.Same(review1, tag3.Review);
        Assert.Same(review2, tag4.Review);
        Assert.Same(review3, tag5.Review);
        Assert.Same(review4, tag6.Review);
        Assert.Same(review3, tag7.Review);
        Assert.Same(review4, tag8.Review);

        // Changes both FK relationships
        tag1.ProductId = 2;

        var productTagType = model.FindEntityType(typeof(ProductTag));
        var productId = productTagType.FindProperty("ProductId");

        fixer.KeyPropertyChanged(
            tagEntry1,
            productId,
            [],
            productTagType.GetForeignKeys().Where(k => k.Properties.Contains(productId)).ToList(),
            1,
            2);

        Assert.Equal([tag2], photo1.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag3, tag4], photo2.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag1, tag5, tag6], photo3.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag7, tag8], photo4.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag3], review1.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag2, tag4], review2.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag1, tag5, tag7], review3.ProductTags.OrderBy(t => t.Id).ToArray());
        Assert.Equal([tag6, tag8], review4.ProductTags.OrderBy(t => t.Id).ToArray());

        Assert.Same(photo3, tag1.Photo);
        Assert.Same(photo1, tag2.Photo);
        Assert.Same(photo2, tag3.Photo);
        Assert.Same(photo2, tag4.Photo);
        Assert.Same(photo3, tag5.Photo);
        Assert.Same(photo3, tag6.Photo);
        Assert.Same(photo4, tag7.Photo);
        Assert.Same(photo4, tag8.Photo);

        Assert.Same(review3, tag1.Review);
        Assert.Same(review2, tag2.Review);
        Assert.Same(review1, tag3.Review);
        Assert.Same(review2, tag4.Review);
        Assert.Same(review3, tag5.Review);
        Assert.Same(review4, tag6.Review);
        Assert.Same(review3, tag7.Review);
        Assert.Same(review4, tag8.Review);
    }

    [ConditionalFact]
    public void Removes_dependent_from_collection_after_deletion()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Category { Id = 11 };
        var principal2 = new Category { Id = 12 };
        var dependent1 = new Product { Id = 21, CategoryId = 12 };
        var dependent2 = new Product { Id = 22, CategoryId = 12 };
        var dependent3 = new Product { Id = 23, CategoryId = 11 };

        var principal1Entry = manager.StartTracking(manager.GetOrCreateEntry(principal1));
        var principal2Entry = manager.StartTracking(manager.GetOrCreateEntry(principal2));

        var dependent1Entry = manager.StartTracking(manager.GetOrCreateEntry(dependent1));
        var dependent2Entry = manager.StartTracking(manager.GetOrCreateEntry(dependent2));
        var dependent3Entry = manager.StartTracking(manager.GetOrCreateEntry(dependent3));

        principal1Entry.SetEntityState(EntityState.Unchanged);
        principal2Entry.SetEntityState(EntityState.Unchanged);
        dependent1Entry.SetEntityState(EntityState.Unchanged);
        dependent2Entry.SetEntityState(EntityState.Unchanged);
        dependent3Entry.SetEntityState(EntityState.Unchanged);

        Assert.Same(dependent1.Category, principal2);
        Assert.Same(dependent2.Category, principal2);
        Assert.Same(dependent3.Category, principal1);
        Assert.Contains(dependent1, principal2.Products);
        Assert.Contains(dependent2, principal2.Products);
        Assert.Contains(dependent3, principal1.Products);
        Assert.Equal(dependent1.CategoryId, principal2.Id);
        Assert.Equal(dependent2.CategoryId, principal2.Id);
        Assert.Equal(dependent3.CategoryId, principal1.Id);

        dependent1Entry.SetEntityState(EntityState.Deleted);
        dependent2Entry.SetEntityState(EntityState.Deleted);

        Assert.Same(dependent1.Category, principal2);
        Assert.Same(dependent2.Category, principal2);
        Assert.Same(dependent3.Category, principal1);
        Assert.Contains(dependent1, principal2.Products);
        Assert.Contains(dependent2, principal2.Products);
        Assert.Contains(dependent3, principal1.Products);
        Assert.Equal(dependent1.CategoryId, principal2.Id);
        Assert.Equal(dependent2.CategoryId, principal2.Id);
        Assert.Equal(dependent3.CategoryId, principal1.Id);

        dependent1Entry.SetEntityState(EntityState.Detached);

        Assert.Same(dependent1.Category, principal2);
        Assert.Same(dependent2.Category, principal2);
        Assert.Same(dependent3.Category, principal1);
        Assert.DoesNotContain(dependent1, principal2.Products);
        Assert.Contains(dependent2, principal2.Products);
        Assert.Contains(dependent3, principal1.Products);
        Assert.Equal(dependent1.CategoryId, principal2.Id);
        Assert.Equal(dependent2.CategoryId, principal2.Id);
        Assert.Equal(dependent3.CategoryId, principal1.Id);

        dependent2Entry.SetEntityState(EntityState.Detached);

        Assert.Same(dependent1.Category, principal2);
        Assert.Same(dependent2.Category, principal2);
        Assert.Same(dependent3.Category, principal1);
        Assert.Empty(principal2.Products);
        Assert.Contains(dependent3, principal1.Products);
        Assert.Equal(dependent1.CategoryId, principal2.Id);
        Assert.Equal(dependent2.CategoryId, principal2.Id);
        Assert.Equal(dependent3.CategoryId, principal1.Id);
    }

    [ConditionalFact]
    public void Nulls_navigation_to_principal_after_after_deletion()
    {
        var contextServices = CreateContextServices();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Category { Id = 11 };
        var principal2 = new Category { Id = 12 };
        var dependent1 = new Product { Id = 21, CategoryId = 12 };
        var dependent2 = new Product { Id = 22, CategoryId = 12 };
        var dependent3 = new Product { Id = 23, CategoryId = 11 };

        var principal1Entry = manager.StartTracking(manager.GetOrCreateEntry(principal1));
        var principal2Entry = manager.StartTracking(manager.GetOrCreateEntry(principal2));

        var dependent1Entry = manager.StartTracking(manager.GetOrCreateEntry(dependent1));
        var dependent2Entry = manager.StartTracking(manager.GetOrCreateEntry(dependent2));
        var dependent3Entry = manager.StartTracking(manager.GetOrCreateEntry(dependent3));

        principal1Entry.SetEntityState(EntityState.Unchanged);
        principal2Entry.SetEntityState(EntityState.Unchanged);
        dependent1Entry.SetEntityState(EntityState.Unchanged);
        dependent2Entry.SetEntityState(EntityState.Unchanged);
        dependent3Entry.SetEntityState(EntityState.Unchanged);

        Assert.Same(dependent1.Category, principal2);
        Assert.Same(dependent2.Category, principal2);
        Assert.Same(dependent3.Category, principal1);
        Assert.Contains(dependent1, principal2.Products);
        Assert.Contains(dependent2, principal2.Products);
        Assert.Contains(dependent3, principal1.Products);
        Assert.Equal(dependent1.CategoryId, principal2.Id);
        Assert.Equal(dependent2.CategoryId, principal2.Id);
        Assert.Equal(dependent3.CategoryId, principal1.Id);

        principal2Entry.SetEntityState(EntityState.Deleted);

        Assert.Same(dependent1.Category, principal2);
        Assert.Same(dependent2.Category, principal2);
        Assert.Same(dependent3.Category, principal1);
        Assert.Contains(dependent1, principal2.Products);
        Assert.Contains(dependent2, principal2.Products);
        Assert.Contains(dependent3, principal1.Products);
        Assert.Equal(dependent1.CategoryId, principal2.Id);
        Assert.Equal(dependent2.CategoryId, principal2.Id);
        Assert.Equal(dependent3.CategoryId, principal1.Id);

        principal2Entry.SetEntityState(EntityState.Detached);

        Assert.Null(dependent1.Category);
        Assert.Null(dependent2.Category);
        Assert.Same(dependent3.Category, principal1);
        Assert.Contains(dependent1, principal2.Products);
        Assert.Contains(dependent2, principal2.Products);
        Assert.Contains(dependent3, principal1.Products);
        Assert.Equal(dependent1.CategoryId, principal2.Id);
        Assert.Equal(dependent2.CategoryId, principal2.Id);
        Assert.Equal(dependent3.CategoryId, principal1.Id);
    }

    [ConditionalFact]
    public void Nulls_one_to_one_navigation_to_principal_after_deletion()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Product { Id = 21 };
        var principal2 = new Product { Id = 22 };
        var dependent1 = new Product { Id = 23, AlternateProductId = 21 };
        var dependent2 = new Product { Id = 24, AlternateProductId = 22 };

        var principalEntry1 = manager.StartTracking(manager.GetOrCreateEntry(principal1));
        var principalEntry2 = manager.StartTracking(manager.GetOrCreateEntry(principal2));
        var dependentEntry1 = manager.StartTracking(manager.GetOrCreateEntry(dependent1));
        var dependentEntry2 = manager.StartTracking(manager.GetOrCreateEntry(dependent2));

        dependentEntry1.SetEntityState(EntityState.Unchanged);
        dependentEntry2.SetEntityState(EntityState.Unchanged);
        principalEntry1.SetEntityState(EntityState.Unchanged);
        principalEntry2.SetEntityState(EntityState.Unchanged);

        Assert.Same(principal1, dependent1.AlternateProduct);
        Assert.Same(dependent1, principal1.OriginalProduct);
        Assert.Same(principal2, dependent2.AlternateProduct);
        Assert.Same(dependent2, principal2.OriginalProduct);
        Assert.Equal(dependent1.AlternateProductId, principal1.Id);
        Assert.Equal(dependent2.AlternateProductId, principal2.Id);

        principalEntry1.SetEntityState(EntityState.Deleted);

        Assert.Null(dependent1.AlternateProduct);
        Assert.Same(dependent1, principal1.OriginalProduct);
        Assert.Same(principal2, dependent2.AlternateProduct);
        Assert.Same(dependent2, principal2.OriginalProduct);
        Assert.Null(dependent1.AlternateProductId);
        Assert.Equal(dependent2.AlternateProductId, principal2.Id);

        principalEntry1.SetEntityState(EntityState.Detached);

        Assert.Null(dependent1.AlternateProduct);
        Assert.Same(dependent1, principal1.OriginalProduct);
        Assert.Same(principal2, dependent2.AlternateProduct);
        Assert.Same(dependent2, principal2.OriginalProduct);
        Assert.Null(dependent1.AlternateProductId);
        Assert.Equal(dependent2.AlternateProductId, principal2.Id);
    }

    [ConditionalFact]
    public void Nulls_one_to_one_navigation_to_dependent_after_after_deletion()
    {
        var model = BuildModel();
        var contextServices = CreateContextServices(model);
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal1 = new Product { Id = 21 };
        var principal2 = new Product { Id = 22 };
        var dependent1 = new Product { Id = 23, AlternateProductId = 21 };
        var dependent2 = new Product { Id = 24, AlternateProductId = 22 };

        var principalEntry1 = manager.StartTracking(manager.GetOrCreateEntry(principal1));
        var principalEntry2 = manager.StartTracking(manager.GetOrCreateEntry(principal2));
        var dependentEntry1 = manager.StartTracking(manager.GetOrCreateEntry(dependent1));
        var dependentEntry2 = manager.StartTracking(manager.GetOrCreateEntry(dependent2));

        dependentEntry1.SetEntityState(EntityState.Unchanged);
        dependentEntry2.SetEntityState(EntityState.Unchanged);
        principalEntry1.SetEntityState(EntityState.Unchanged);
        principalEntry2.SetEntityState(EntityState.Unchanged);

        Assert.Same(principal1, dependent1.AlternateProduct);
        Assert.Same(dependent1, principal1.OriginalProduct);
        Assert.Same(principal2, dependent2.AlternateProduct);
        Assert.Same(dependent2, principal2.OriginalProduct);
        Assert.Equal(dependent1.AlternateProductId, principal1.Id);
        Assert.Equal(dependent2.AlternateProductId, principal2.Id);

        dependentEntry1.SetEntityState(EntityState.Deleted);

        Assert.Same(principal1, dependent1.AlternateProduct);
        Assert.Same(dependent1, principal1.OriginalProduct);
        Assert.Same(principal2, dependent2.AlternateProduct);
        Assert.Same(dependent2, principal2.OriginalProduct);
        Assert.Equal(dependent1.AlternateProductId, principal1.Id);
        Assert.Equal(dependent2.AlternateProductId, principal2.Id);

        dependentEntry1.SetEntityState(EntityState.Detached);

        Assert.Same(principal1, dependent1.AlternateProduct);
        Assert.Null(principal1.OriginalProduct);
        Assert.Same(principal2, dependent2.AlternateProduct);
        Assert.Same(dependent2, principal2.OriginalProduct);
        Assert.Equal(dependent1.AlternateProductId, principal1.Id);
        Assert.Equal(dependent2.AlternateProductId, principal2.Id);
    }

    private static IServiceProvider CreateContextServices(IModel model = null)
        => InMemoryTestHelpers.Instance.CreateContextServices(model ?? BuildModel());

    private class Category
    {
        public int Id { get; set; }

        public ICollection<Product> Products { get; } = new List<Product>();
    }

    private class Product
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ProductDetail Detail { get; set; }

        public int? AlternateProductId { get; set; }
        public Product AlternateProduct { get; set; }
        public Product OriginalProduct { get; set; }
    }

    private class ProductDetail : IEnumerable<Product>
    {
        public int Id { get; set; }

        public Product Product { get; set; }

        public IEnumerator<Product> GetEnumerator()
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private class ProductPhoto
    {
        public int ProductId { get; set; }
        public string PhotoId { get; set; }

        public ICollection<ProductTag> ProductTags { get; } = new HashSet<ProductTag>();
    }

    private class ProductReview
    {
        public int ProductId { get; set; }
        public Guid ReviewId { get; set; }

        public ICollection<ProductTag> ProductTags { get; } = new HashSet<ProductTag>();
    }

    private class ProductTag
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string PhotoId { get; set; }
        public Guid ReviewId { get; set; }

        public ProductPhoto Photo { get; set; }
        public ProductReview Review { get; set; }
    }

    private static IModel BuildModel()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<Product>(
            b =>
            {
                b.HasOne(e => e.AlternateProduct).WithOne(e => e.OriginalProduct)
                    .HasForeignKey<Product>(e => e.AlternateProductId);

                b.HasOne(e => e.Detail).WithOne(e => e.Product)
                    .HasForeignKey<ProductDetail>(e => e.Id);
            });

        builder.Entity<Category>().HasMany(e => e.Products).WithOne(e => e.Category);

        builder.Entity<ProductDetail>();

        builder.Entity<ProductPhoto>(
            b =>
            {
                b.HasKey(
                    e => new { e.ProductId, e.PhotoId });
                b.HasMany(e => e.ProductTags).WithOne(e => e.Photo)
                    .HasForeignKey(
                        e => new { e.ProductId, e.PhotoId });
            });

        builder.Entity<ProductReview>(
            b =>
            {
                b.HasKey(
                    e => new { e.ProductId, e.ReviewId });
                b.HasMany(e => e.ProductTags).WithOne(e => e.Review)
                    .HasForeignKey(
                        e => new { e.ProductId, e.ReviewId });
            });

        builder.Entity<ProductTag>();

        return builder.Model.FinalizeModel();
    }

    private static INavigationFixer CreateNavigationFixer(IServiceProvider contextServices)
        => contextServices.GetRequiredService<INavigationFixer>();
}

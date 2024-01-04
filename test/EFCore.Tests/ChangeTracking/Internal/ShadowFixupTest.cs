// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class ShadowFixupTest
{
    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_set_both_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: false, setFk: true, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: false, setFk: false, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: false, setFk: true, setToPrincipal: false, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: false, setFk: true, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: false, setFk: true, setToPrincipal: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: false, setFk: false, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: false, setFk: false, setToPrincipal: true, setToDependent: false);

    private void Add_principal_and_dependent_one_to_many(
        EntityState entityState,
        bool principalFirst,
        bool setFk,
        bool setToPrincipal,
        bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new Category(77);
        var dependent = new Product(78);
        var principalEntry = context.Entry(principal);
        var dependentEntry = context.Entry(dependent);
        if (setFk)
        {
            dependentEntry.Property("CategoryId").CurrentValue = principal.Id;
        }

        if (setToPrincipal)
        {
            dependentEntry.Navigation("Category").CurrentValue = principal;
        }

        if (setToDependent)
        {
            var collection = new HashSet<Product> { dependent };
            principalEntry.Collection("Products").CurrentValue = collection;
        }

        if (principalFirst)
        {
            principalEntry.State = entityState;
        }

        dependentEntry.State = entityState;
        if (!principalFirst)
        {
            principalEntry.State = entityState;
        }

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependentEntry.Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependentEntry.Navigation("Category").CurrentValue);
                Assert.Equal(new[] { dependent }, principalEntry.Collection("Products").CurrentValue);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_set_both_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: false, setFk: true, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: false, setFk: false, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: false, setFk: true, setToPrincipal: false, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: false, setFk: true, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: false, setFk: true, setToPrincipal: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: false, setFk: false, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: false, setFk: false, setToPrincipal: true, setToDependent: false);

    private void Add_principal_and_dependent_one_to_one(
        EntityState entityState,
        bool principalFirst,
        bool setFk,
        bool setToPrincipal,
        bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new Parent(77);
        var dependent = new Child(78);
        var principalEntry = context.Entry(principal);
        var dependentEntry = context.Entry(dependent);
        if (setFk)
        {
            dependentEntry.Property("ParentId").CurrentValue = principal.Id;
        }

        if (setToPrincipal)
        {
            dependentEntry.Navigation("Parent").CurrentValue = principal;
        }

        if (setToDependent)
        {
            principalEntry.Navigation("Child").CurrentValue = dependent;
        }

        if (principalFirst)
        {
            principalEntry.State = entityState;
        }

        dependentEntry.State = entityState;
        if (!principalFirst)
        {
            principalEntry.State = entityState;
        }

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependentEntry.Property("ParentId").CurrentValue);
                Assert.Same(principal, dependentEntry.Navigation("Parent").CurrentValue);
                Assert.Same(dependent, principalEntry.Navigation("Child").CurrentValue);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    private class Parent(int id)
    {
        public int Id { get; set; } = id;
    }

    private class Child(int id)
    {
        public int Id { get; set; } = id;
    }

    private class Category
    {
        public Category()
        {
        }

        public Category(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }

    private class Product
    {
        public Product()
        {
        }

        public Product(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }

    private sealed class FixupContext : DbContext
    {
        public FixupContext()
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var category = modelBuilder.Entity<Category>().Metadata;

            modelBuilder.Entity<Product>(
                b =>
                {
                    var fk = b.Metadata.AddForeignKey(
                        new[] { b.Property<int>("CategoryId").Metadata },
                        category.FindPrimaryKey(),
                        category);
                    fk.SetDependentToPrincipal("Category");
                    fk.SetPrincipalToDependent("Products");
                });

            var parent = modelBuilder.Entity<Parent>().Metadata;

            modelBuilder.Entity<Child>(
                b =>
                {
                    var fk = b.Metadata.AddForeignKey(
                        new[] { b.Property<int>("ParentId").Metadata },
                        parent.FindPrimaryKey(),
                        parent);
                    fk.IsUnique = true;
                    fk.SetDependentToPrincipal("Parent");
                    fk.SetPrincipalToDependent("Child");
                });
        }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(FixupContext));
    }

    private void AssertFixup(DbContext context, Action asserts)
    {
        asserts();
        context.ChangeTracker.DetectChanges();
        asserts();
        context.ChangeTracker.DetectChanges();
        asserts();
        context.ChangeTracker.DetectChanges();
        asserts();
    }
}

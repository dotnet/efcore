// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class StoreGeneratedFixupTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : StoreGeneratedFixupTestBase<TFixture>.StoreGeneratedFixupFixtureBase, new()
{
    protected static readonly Guid Guid77 = new("{DE390D36-DAAC-4C8B-91F7-E9F5DAA7EF01}");
    protected static readonly Guid Guid78 = new("{4C80406F-49AF-4D85-AFFB-75C146A98A70}");

    protected StoreGeneratedFixupTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_FK_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    Category = principal,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };
                principal.Products.Add(dependent);

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_FK_not_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category();
                var dependent = new Product { Category = principal };

                principal.Products.Add(dependent);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };
                principal.Products.Add(dependent);

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2,
                    Category = principal
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category();
                var dependent = new Product();
                principal.Products.Add(dependent);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category();
                var dependent = new Product { Category = principal };

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_FK_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    Category = principal,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };
                principal.Products.Add(dependent);

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_FK_not_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category();
                var dependent = new Product { Category = principal };
                principal.Products.Add(dependent);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };
                principal.Products.Add(dependent);

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2,
                    Category = principal
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category();
                var dependent = new Product();
                principal.Products.Add(dependent);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Category();
                var dependent = new Product { Category = principal };

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    private async Task AssertFixupAndSaveAsync(DbContext context, Category principal, Product dependent)
    {
        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id1).CurrentValue,
                    context.Entry(dependent).Property(e => e.CategoryId1).CurrentValue);

                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id2).CurrentValue,
                    context.Entry(dependent).Property(e => e.CategoryId2).CurrentValue);

                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(EntityState.Added, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });

        await context.SaveChangesAsync();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
            });
    }

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_prin_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductPN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_prin_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductPN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_prin_uni_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductPN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };
                principal.Products.Add(dependent);

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_prin_uni_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryPN();
                var dependent = new ProductPN();
                principal.Products.Add(dependent);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_prin_uni_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductPN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };
                principal.Products.Add(dependent);

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_prin_uni_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryPN();
                var dependent = new ProductPN();
                principal.Products.Add(dependent);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    private async Task AssertFixupAndSaveAsync(DbContext context, CategoryPN principal, ProductPN dependent)
    {
        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id1).CurrentValue,
                    context.Entry(dependent).Property(e => e.CategoryId1).CurrentValue);

                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id2).CurrentValue,
                    context.Entry(dependent).Property(e => e.CategoryId2).CurrentValue);

                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(EntityState.Added, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });

        await context.SaveChangesAsync();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
            });
    }

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_dep_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductDN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_dep_uni_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductDN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2,
                    Category = principal
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_dep_uni_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryDN();
                var dependent = new ProductDN { Category = principal };

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_dep_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductDN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_dep_uni_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductDN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2,
                    Category = principal
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_dep_uni_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryDN();
                var dependent = new ProductDN { Category = principal };

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    private async Task AssertFixupAndSaveAsync(DbContext context, CategoryDN principal, ProductDN dependent)
    {
        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id1).CurrentValue,
                    context.Entry(dependent).Property(e => e.CategoryId1).CurrentValue);

                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id2).CurrentValue,
                    context.Entry(dependent).Property(e => e.CategoryId2).CurrentValue);

                Assert.Same(principal, dependent.Category);
                Assert.Equal(EntityState.Added, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });

        await context.SaveChangesAsync();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
            });
    }

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_many_no_navs_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryNN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductNN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_many_no_navs_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new CategoryNN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductNN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    CategoryId1 = principal.Id1,
                    CategoryId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    private async Task AssertFixupAndSaveAsync(DbContext context, CategoryNN principal, ProductNN dependent)
    {
        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
                Assert.Equal(EntityState.Added, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });

        await context.SaveChangesAsync();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
                Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
            });
    }

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_FK_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    Parent = principal,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };
                principal.Child = dependent;

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_FK_not_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent();
                var dependent = new Child { Parent = principal };
                principal.Child = dependent;

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };
                principal.Child = dependent;

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2,
                    Parent = principal
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent();
                var dependent = new Child();
                principal.Child = dependent;

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent();
                var dependent = new Child { Parent = principal };

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_FK_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    Parent = principal,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };
                principal.Child = dependent;

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_FK_not_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent();
                var dependent = new Child { Parent = principal };
                principal.Child = dependent;

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };
                principal.Child = dependent;

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2,
                    Parent = principal
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent();
                var dependent = new Child();
                principal.Child = dependent;

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new Parent();
                var dependent = new Child { Parent = principal };

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    private async Task AssertFixupAndSaveAsync(DbContext context, Parent principal, Child dependent)
    {
        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id1).CurrentValue,
                    context.Entry(dependent).Property(e => e.ParentId1).CurrentValue);

                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id2).CurrentValue,
                    context.Entry(dependent).Property(e => e.ParentId2).CurrentValue);

                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(EntityState.Added, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });

        await context.SaveChangesAsync();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
            });
    }

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_prin_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildPN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_prin_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildPN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_prin_uni_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildPN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };
                principal.Child = dependent;

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_prin_uni_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentPN();
                var dependent = new ChildPN();
                principal.Child = dependent;

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_prin_uni_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildPN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };
                principal.Child = dependent;

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_prin_uni_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentPN();
                var dependent = new ChildPN();
                principal.Child = dependent;

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    private async Task AssertFixupAndSaveAsync(DbContext context, ParentPN principal, ChildPN dependent)
    {
        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id1).CurrentValue,
                    context.Entry(dependent).Property(e => e.ParentId1).CurrentValue);

                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id2).CurrentValue,
                    context.Entry(dependent).Property(e => e.ParentId2).CurrentValue);

                Assert.Same(dependent, principal.Child);
                Assert.Equal(EntityState.Added, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });

        await context.SaveChangesAsync();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
            });
    }

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_dep_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildDN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_dep_uni_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildDN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2,
                    Parent = principal
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_dep_uni_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentDN();
                var dependent = new ChildDN { Parent = principal };

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_dep_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildDN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_dep_uni_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildDN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2,
                    Parent = principal
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_dep_uni_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentDN();
                var dependent = new ChildDN { Parent = principal };

                context.Add(principal);
                context.Add(dependent);

                MarkIdsTemporary(context, dependent, principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    private async Task AssertFixupAndSaveAsync(DbContext context, ParentDN principal, ChildDN dependent)
    {
        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id1).CurrentValue,
                    context.Entry(dependent).Property(e => e.ParentId1).CurrentValue);

                Assert.Equal(
                    context.Entry(principal).Property(e => e.Id2).CurrentValue,
                    context.Entry(dependent).Property(e => e.ParentId2).CurrentValue);

                Assert.Same(principal, dependent.Parent);
                Assert.Equal(EntityState.Added, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });

        await context.SaveChangesAsync();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
            });
    }

    [ConditionalFact]
    public virtual Task Add_dependent_then_principal_one_to_one_no_navs_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentNN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildNN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);
                context.Add(principal);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    [ConditionalFact]
    public virtual Task Add_principal_then_dependent_one_to_one_no_navs_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var principal = new ParentNN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildNN
                {
                    Id1 = -78,
                    Id2 = Guid78,
                    ParentId1 = principal.Id1,
                    ParentId2 = principal.Id2
                };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);
                context.Add(dependent);

                return AssertFixupAndSaveAsync(context, principal, dependent);
            });

    private async Task AssertFixupAndSaveAsync(DbContext context, ParentNN principal, ChildNN dependent)
    {
        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
                Assert.Equal(EntityState.Added, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });

        await context.SaveChangesAsync();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
                Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
            });
    }

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_FK_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                dependent.Category = principal;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_FK_not_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category();
                var dependent = new Product();

                context.Add(dependent);

                dependent.Category = principal;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.CategoryId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.CategoryId2).CurrentValue);

                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Null(dependent.Category);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Null(dependent.Category);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Null(dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Null(dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category();
                var dependent = new Product();

                context.Add(dependent);

                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.CategoryId1);
                        Assert.Null(dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.CategoryId1);
                        Assert.Null(dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category();
                var dependent = new Product();

                context.Add(dependent);

                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.CategoryId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.CategoryId2).CurrentValue);

                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_FK_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                dependent.Category = principal;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_FK_not_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category();
                var dependent = new Product();

                context.Add(principal);

                dependent.Category = principal;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.CategoryId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.CategoryId2).CurrentValue);

                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Null(dependent.Category);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Null(dependent.Category);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category { Id1 = -77, Id2 = Guid77 };
                var dependent = new Product { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Same(principal, dependent.Category);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category();
                var dependent = new Product();

                context.Add(principal);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Category();
                var dependent = new Product();

                context.Add(principal);

                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.CategoryId1);
                        Assert.Same(principal, dependent.Category);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.CategoryId1);
                        Assert.Same(principal, dependent.Category);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_prin_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductPN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_prin_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductPN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Empty(principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_prin_uni_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductPN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_prin_uni_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryPN();
                var dependent = new ProductPN();

                context.Add(dependent);

                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.CategoryId1);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.CategoryId1);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_prin_uni_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductPN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_prin_uni_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryPN();
                var dependent = new ProductPN();

                context.Add(principal);

                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.CategoryId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.CategoryId2).CurrentValue);

                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_dep_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductDN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Null(dependent.Category);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Null(dependent.Category);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_dep_uni_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductDN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_dep_uni_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryDN();
                var dependent = new ProductDN();

                context.Add(dependent);

                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.CategoryId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.CategoryId2).CurrentValue);

                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_dep_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductDN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Null(dependent.Category);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Null(dependent.Category);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_dep_uni_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductDN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;
                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_dep_uni_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryDN();
                var dependent = new ProductDN();

                context.Add(principal);

                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.CategoryId1);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.CategoryId1);
                        Assert.Same(principal, dependent.Category);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_many_no_navs_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryNN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductNN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_many_no_navs_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new CategoryNN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ProductNN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.CategoryId1 = principal.Id1;
                dependent.CategoryId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.CategoryId1);
                        Assert.Equal(principal.Id2, dependent.CategoryId2);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_FK_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;
                dependent.Parent = principal;
                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_FK_not_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent();
                var dependent = new Child();

                context.Add(dependent);

                dependent.Parent = principal;
                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId2).CurrentValue);

                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(dependent.Parent);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(dependent.Parent);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;
                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;
                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent();
                var dependent = new Child();

                context.Add(dependent);

                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.ParentId1);
                        Assert.Null(dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.ParentId1);
                        Assert.Null(dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent();
                var dependent = new Child();

                context.Add(dependent);

                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId2).CurrentValue);

                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_FK_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;
                dependent.Parent = principal;
                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_FK_not_set_both_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent();
                var dependent = new Child();

                context.Add(principal);

                dependent.Parent = principal;
                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId2).CurrentValue);

                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(dependent.Parent);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Null(dependent.Parent);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;
                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent { Id1 = -77, Id2 = Guid77 };
                var dependent = new Child { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;
                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Same(principal, dependent.Parent);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent();
                var dependent = new Child();

                context.Add(principal);

                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId2).CurrentValue);

                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new Parent();
                var dependent = new Child();

                context.Add(principal);

                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.ParentId1);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.ParentId1);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_prin_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildPN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_prin_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildPN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Null(principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_prin_uni_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildPN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;
                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_prin_uni_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentPN();
                var dependent = new ChildPN();

                context.Add(dependent);

                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.ParentId1);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.ParentId1);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_prin_uni_FK_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentPN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildPN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;
                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_prin_uni_FK_not_set_principal_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentPN();
                var dependent = new ChildPN();

                context.Add(principal);

                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId2).CurrentValue);

                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(dependent, principal.Child);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_dep_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildDN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(dependent.Parent);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(dependent.Parent);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_dep_uni_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildDN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;
                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_dep_uni_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentDN();
                var dependent = new ChildDN();

                context.Add(dependent);

                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id1).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId1).CurrentValue);

                        Assert.Equal(
                            context.Entry(principal).Property(e => e.Id2).CurrentValue,
                            context.Entry(dependent).Property(e => e.ParentId2).CurrentValue);

                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_dep_uni_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildDN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Null(dependent.Parent);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Null(dependent.Parent);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_dep_uni_FK_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentDN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildDN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;
                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_dep_uni_FK_not_set_dependent_nav_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentDN();
                var dependent = new ChildDN();

                context.Add(principal);

                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.ParentId1);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(0, dependent.ParentId1);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_dependent_but_not_principal_one_to_one_no_navs_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentNN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildNN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(dependent);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                    });

                if (EnforcesFKs)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();
                }

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                        Assert.Equal(EnforcesFKs ? EntityState.Added : EntityState.Unchanged, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_principal_but_not_dependent_one_to_one_no_navs_FK_set_no_navs_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = new ParentNN { Id1 = -77, Id2 = Guid77 };
                var dependent = new ChildNN { Id1 = -78, Id2 = Guid78 };

                MarkIdsTemporary(context, dependent, principal);

                context.Add(principal);

                dependent.ParentId1 = principal.Id1;
                dependent.ParentId2 = principal.Id2;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id1, dependent.ParentId1);
                        Assert.Equal(principal.Id2, dependent.ParentId2);
                        Assert.Equal(EntityState.Added, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });

                await context.SaveChangesAsync();

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                    });
            });

    [ConditionalFact]
    public virtual Task Add_overlapping_graph_from_level()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var game = new Game { Id = Guid77 };
                var level = new Level { Id = -77, Game = game };
                var item = new Item { Id = 78 };
                level.Items.Add(item);

                MarkIdsTemporary(context, game, level, item);

                context.Add(level);

                return AssertFixupAndSaveAsync(context, game, level, item);
            });

    [ConditionalFact]
    public virtual Task Add_overlapping_graph_from_game()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var level = new Level { Id = -77 };
                var game = new Game { Id = Guid77 };
                game.Levels.Add(level);
                var item = new Item { Id = 78 };
                level.Items.Add(item);

                MarkIdsTemporary(context, game, level, item);

                context.Add(game);

                return AssertFixupAndSaveAsync(context, game, level, item);
            });

    [ConditionalFact]
    public virtual Task Add_overlapping_graph_from_item()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var game = new Game { Id = Guid77 };
                var level = new Level { Id = -77, Game = game };
                var item = new Item { Id = 78, Level = level };

                MarkIdsTemporary(context, game, level, item);

                context.Add(item);

                return AssertFixupAndSaveAsync(context, game, level, item);
            });

    [ConditionalFact]
    public virtual void Temporary_value_equals_database_generated_value()
    {
        using var context = CreateContext();
        var entry = context.Add(new Game { Id = Guid77 });
        entry.Property(g => g.Id).IsTemporary = true;
        var internalEntry = ((IInfrastructure<InternalEntityEntry>)entry).Instance;
        internalEntry.PrepareToSave();
        internalEntry.SetProperty(entry.Metadata.FindProperty("Id"), Guid77, false);
        internalEntry.AcceptChanges();

        Assert.Equal(EntityState.Unchanged, internalEntry.EntityState);
    }

    private async Task AssertFixupAndSaveAsync(DbContext context, Game game, Level level, Item item)
    {
        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(game.Id, context.Entry(level).Property(e => e.GameId).CurrentValue);
                Assert.Equal(game.Id, context.Entry(item).Property(e => e.GameId).CurrentValue);
                Assert.Equal(level.Id, context.Entry(item).Property(e => e.LevelId).CurrentValue);

                Assert.Same(game, level.Game);
                Assert.Same(game, item.Game);
                Assert.Same(level, item.Level);

                Assert.Equal(new[] { item }.ToList(), level.Items);
                Assert.Equal(new[] { item }.ToList(), game.Items);
                Assert.Equal(new[] { level }.ToList(), game.Levels);

                Assert.Equal(EntityState.Added, context.Entry(game).State);
                Assert.Equal(EntityState.Added, context.Entry(level).State);
                Assert.Equal(EntityState.Added, context.Entry(item).State);
            });

        await context.SaveChangesAsync();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(game.Id, level.GameId);
                Assert.Equal(game.Id, item.GameId);
                Assert.Equal(level.Id, item.LevelId);

                Assert.Same(game, level.Game);
                Assert.Same(game, item.Game);
                Assert.Same(level, item.Level);

                Assert.Equal(new[] { item }.ToList(), level.Items);
                Assert.Equal(new[] { item }.ToList(), game.Items);
                Assert.Equal(new[] { level }.ToList(), game.Levels);

                Assert.Equal(EntityState.Unchanged, context.Entry(game).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(level).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(item).State);
            });
    }

    [ConditionalFact]
    public virtual void Remove_overlapping_principal()
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;

        var game = new Game { Id = Guid77 };
        var level = new Level { Game = game };
        var item = new Item { Level = level };

        context.Add(item);

        level.Items.Remove(item);

        context.ChangeTracker.DetectChanges();

        Assert.Null(item.Level);
        Assert.Empty(level.Items);
        Assert.Empty(level.Actors);
    }

    [ConditionalFact]
    public virtual Task Multi_level_add_replace_and_save()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var firstLevel = await context.Set<FirstLevel>().SingleAsync();

                AddData(firstLevel);

                var originalSecondLevels = firstLevel.SecondLevels.ToArray();
                var originalThirdLevels = originalSecondLevels.SelectMany(e => e.ThirdLevels).ToArray();

                context.ChangeTracker.DetectChanges();

                Assert.Single(context.ChangeTracker.Entries<FirstLevel>());
                Assert.Equal(2, context.ChangeTracker.Entries<SecondLevel>().Count());
                Assert.Equal(4, context.ChangeTracker.Entries<ThirdLevel>().Count());

                AssertValidFks(context, firstLevel, tempKeys: true);

                AddData(firstLevel);

                context.ChangeTracker.DetectChanges();

                Assert.Single(context.ChangeTracker.Entries<FirstLevel>());
                Assert.Equal(2, context.ChangeTracker.Entries<SecondLevel>().Count());
                Assert.Equal(4, context.ChangeTracker.Entries<ThirdLevel>().Count());

                AssertValidFks(context, firstLevel, tempKeys: true);

                Assert.All(
                    originalSecondLevels.Select(l => context.Entry(l).State),
                    s => Assert.Equal(EntityState.Detached, s));

                Assert.All(
                    originalThirdLevels.Select(l => context.Entry(l).State),
                    s => Assert.Equal(EntityState.Detached, s));

                await context.SaveChangesAsync();

                AssertValidFks(context, firstLevel, tempKeys: false);
            });

    private static void AssertValidFks(DbContext context, FirstLevel firstLevel, bool tempKeys)
    {
        var secondLevels = firstLevel.SecondLevels.ToArray();
        var thirdLevels0 = secondLevels[0].ThirdLevels.ToArray();
        var thirdLevels1 = secondLevels[1].ThirdLevels.ToArray();
        var thirdLevels = thirdLevels0.Concat(thirdLevels1).ToArray();

        Assert.Equal(EntityState.Unchanged, context.Entry(firstLevel).State);

        var expectedState = tempKeys ? EntityState.Added : EntityState.Unchanged;

        if (context.Database.ProviderName.EndsWith("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            tempKeys = false;
        }

        Assert.All(
            secondLevels.Select(l => context.Entry(l).State),
            s => Assert.Equal(expectedState, s));

        Assert.All(
            thirdLevels.Select(l => context.Entry(l).State),
            s => Assert.Equal(expectedState, s));

        Assert.Equal(1, context.Entry(firstLevel).Property(e => e.Id).CurrentValue);

        Assert.All(
            secondLevels.Select(l => context.Entry(l).Property(e => e.Id).CurrentValue),
            s => Assert.True(tempKeys ? s < 0 : s > 0));

        Assert.All(
            thirdLevels.Select(l => context.Entry(l).Property(e => e.Id).CurrentValue),
            s => Assert.True(tempKeys ? s < 0 : s > 0));

        Assert.All(
            secondLevels.Select(l => context.Entry(l).Property(e => e.FirstLevelId).CurrentValue),
            s => Assert.Equal(1, s));

        Assert.All(
            thirdLevels0.Select(l => context.Entry(l).Property(e => e.SecondLevelId).CurrentValue),
            s => Assert.Equal(context.Entry(secondLevels[0]).Property(e => e.Id).CurrentValue, s));

        Assert.All(
            thirdLevels1.Select(l => context.Entry(l).Property(e => e.SecondLevelId).CurrentValue),
            s => Assert.Equal(context.Entry(secondLevels[1]).Property(e => e.Id).CurrentValue, s));
    }

    protected class FirstLevel
    {
        public int Id { get; set; }
        public IList<SecondLevel> SecondLevels { get; set; }
    }

    private static void AddData(FirstLevel first)
        => first.SecondLevels = new List<SecondLevel>
        {
            new() { ThirdLevels = new List<ThirdLevel> { new(), new() } }, new() { ThirdLevels = new List<ThirdLevel> { new(), new() } }
        };

    protected class SecondLevel
    {
        public int Id { get; set; }
        public int FirstLevelId { get; set; }
        public FirstLevel FirstLevel { get; set; }
        public IList<ThirdLevel> ThirdLevels { get; set; }
    }

    protected class ThirdLevel
    {
        public int Id { get; set; }
        public int SecondLevelId { get; set; }
        public SecondLevel SecondLevel { get; set; }
    }

    protected class Parent
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public Child Child { get; set; }
    }

    protected class Child
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int ParentId1 { get; set; }
        public Guid ParentId2 { get; set; }

        public Parent Parent { get; set; }
    }

    protected class ParentPN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public ChildPN Child { get; set; }
    }

    protected class ChildPN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int ParentId1 { get; set; }
        public Guid ParentId2 { get; set; }
    }

    protected class ParentDN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
    }

    protected class ChildDN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int ParentId1 { get; set; }
        public Guid ParentId2 { get; set; }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public ParentDN Parent { get; set; }
    }

    protected class ParentNN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
    }

    protected class ChildNN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int ParentId1 { get; set; }
        public Guid ParentId2 { get; set; }
    }

    protected class CategoryDN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
    }

    protected class ProductDN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int CategoryId1 { get; set; }
        public Guid CategoryId2 { get; set; }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public CategoryDN Category { get; set; }
    }

    protected class CategoryPN
    {
        public CategoryPN()
        {
            Products = new List<ProductPN>();
        }

        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public ICollection<ProductPN> Products { get; }
    }

    protected class ProductPN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int CategoryId1 { get; set; }
        public Guid CategoryId2 { get; set; }
    }

    protected class CategoryNN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
    }

    protected class ProductNN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int CategoryId1 { get; set; }
        public Guid CategoryId2 { get; set; }
    }

    protected class Category
    {
        public Category()
        {
            Products = new List<Product>();
        }

        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public ICollection<Product> Products { get; }
    }

    protected class Product
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int CategoryId1 { get; set; }
        public Guid CategoryId2 { get; set; }
        public Category Category { get; set; }
    }

    protected class Level
    {
        public virtual int Id { get; set; }
        public virtual Guid GameId { get; set; }
        public virtual Game Game { get; set; }

        public virtual ICollection<Item> Items { get; } = new List<Item>();
        public virtual ICollection<Actor> Actors { get; } = new List<Actor>();
    }

    protected abstract class GameEntity
    {
        public int Id { get; set; }

        public Guid GameId { get; set; }
        public Game Game { get; set; }

        public int LevelId { get; set; }
        public Level Level { get; set; }
    }

    protected class Item : GameEntity;

    protected class Actor : GameEntity;

    protected class Game
    {
        public virtual Guid Id { get; set; }

        public virtual ICollection<Item> Items { get; set; } = new List<Item>();

        public virtual ICollection<Actor> Actors { get; set; } = new List<Actor>();

        public virtual ICollection<Level> Levels { get; set; } = new List<Level>();
    }

    protected class TestTemp
    {
        public int Id { get; set; }
        public int NotId { get; set; }
    }

    protected void AssertFixup(DbContext context, Action asserts)
    {
        asserts();
        context.ChangeTracker.DetectChanges();
        asserts();
        context.ChangeTracker.DetectChanges();
        asserts();
        context.ChangeTracker.DetectChanges();
        asserts();
    }

    protected abstract bool EnforcesFKs { get; }

    protected virtual void MarkIdsTemporary(DbContext context, object dependent, object principal)
    {
    }

    protected virtual void MarkIdsTemporary(DbContext context, object game, object level, object item)
    {
    }

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected virtual Task ExecuteWithStrategyInTransactionAsync(Func<DbContext, Task> testOperation)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(CreateContext, UseTransaction, testOperation);

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    public abstract class StoreGeneratedFixupFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "StoreGeneratedFixup";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<TestTemp>();

            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.HasOne(e => e.Child)
                        .WithOne(e => e.Parent)
                        .HasForeignKey<Child>(e => new { e.ParentId1, e.ParentId2 });
                });

            modelBuilder.Entity<Child>(b => b.HasKey(e => new { e.Id1, e.Id2 }));

            modelBuilder.Entity<ParentPN>(
                b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.HasOne(e => e.Child)
                        .WithOne()
                        .HasForeignKey<ChildPN>(e => new { e.ParentId1, e.ParentId2 });
                });

            modelBuilder.Entity<ChildPN>(b => b.HasKey(e => new { e.Id1, e.Id2 }));

            modelBuilder.Entity<ParentDN>(
                b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.HasOne<ChildDN>()
                        .WithOne(e => e.Parent)
                        .HasForeignKey<ChildDN>(e => new { e.ParentId1, e.ParentId2 });
                });

            modelBuilder.Entity<ChildDN>(b => b.HasKey(e => new { e.Id1, e.Id2 }));

            modelBuilder.Entity<ParentNN>(
                b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.HasOne<ChildNN>()
                        .WithOne()
                        .HasForeignKey<ChildNN>(e => new { e.ParentId1, e.ParentId2 });
                });

            modelBuilder.Entity<ChildNN>(b => b.HasKey(e => new { e.Id1, e.Id2 }));

            modelBuilder.Entity<CategoryDN>(
                b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.HasMany<ProductDN>()
                        .WithOne(e => e.Category)
                        .HasForeignKey(e => new { e.CategoryId1, e.CategoryId2 });
                });

            modelBuilder.Entity<ProductDN>(b => b.HasKey(e => new { e.Id1, e.Id2 }));

            modelBuilder.Entity<CategoryPN>(
                b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.HasMany(e => e.Products)
                        .WithOne()
                        .HasForeignKey(e => new { e.CategoryId1, e.CategoryId2 });
                });

            modelBuilder.Entity<ProductPN>(b => b.HasKey(e => new { e.Id1, e.Id2 }));

            modelBuilder.Entity<CategoryNN>(
                b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.HasMany<ProductNN>()
                        .WithOne()
                        .HasForeignKey(e => new { e.CategoryId1, e.CategoryId2 });
                });

            modelBuilder.Entity<ProductNN>(b => b.HasKey(e => new { e.Id1, e.Id2 }));

            modelBuilder.Entity<Category>(
                b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.HasMany(e => e.Products)
                        .WithOne(e => e.Category)
                        .HasForeignKey(e => new { e.CategoryId1, e.CategoryId2 });
                });

            modelBuilder.Entity<Product>(b => b.HasKey(e => new { e.Id1, e.Id2 }));

            modelBuilder.Entity<Level>(
                eb =>
                {
                    eb.Property(g => g.Id).ValueGeneratedNever();
                    eb.HasKey(l => new { l.GameId, l.Id });
                });

            modelBuilder.Entity<GameEntity>();

            modelBuilder.Entity<Item>(
                eb =>
                {
                    eb.HasOne(i => i.Level)
                        .WithMany(l => l.Items)
                        .HasForeignKey(i => new { i.GameId, i.LevelId })
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity<Actor>(
                eb =>
                {
                    eb.HasOne(i => i.Level)
                        .WithMany(l => l.Actors)
                        .HasForeignKey(i => new { i.GameId, i.LevelId })
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder
                .Entity<FirstLevel>()
                .HasData(new FirstLevel { Id = 1 });
        }
    }
}

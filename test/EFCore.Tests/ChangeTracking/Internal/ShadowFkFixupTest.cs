// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class ShadowFkFixupTest
{
    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78, Category = principal };
        principal.Products.Add(dependent);

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78, Category = principal };
        principal.Products.Add(dependent);

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78 };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78 };
        principal.Products.Add(dependent);

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78, Category = principal };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78 };
        principal.Products.Add(dependent);

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78, Category = principal };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78, Category = principal };
        principal.Products.Add(dependent);

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78, Category = principal };
        principal.Products.Add(dependent);

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78 };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78 };
        principal.Products.Add(dependent);

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78, Category = principal };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78 };
        principal.Products.Add(dependent);

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id = 77 };
        var dependent = new Product { Id = 78, Category = principal };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id = 77 };
        var dependent = new ProductPN { Id = 78 };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id = 77 };
        var dependent = new ProductPN { Id = 78 };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id = 77 };
        var dependent = new ProductPN { Id = 78 };
        principal.Products.Add(dependent);

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id = 77 };
        var dependent = new ProductPN { Id = 78 };
        principal.Products.Add(dependent);

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id = 77 };
        var dependent = new ProductPN { Id = 78 };
        principal.Products.Add(dependent);

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id = 77 };
        var dependent = new ProductPN { Id = 78 };
        principal.Products.Add(dependent);

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id = 77 };
        var dependent = new ProductDN { Id = 78 };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id = 77 };
        var dependent = new ProductDN { Id = 78, Category = principal };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id = 77 };
        var dependent = new ProductDN { Id = 78, Category = principal };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id = 77 };
        var dependent = new ProductDN { Id = 78 };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id = 77 };
        var dependent = new ProductDN { Id = 78, Category = principal };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id = 77 };
        var dependent = new ProductDN { Id = 78, Category = principal };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_no_navs_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryNN { Id = 77 };
        var dependent = new ProductNN { Id = 78 };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_no_navs_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryNN { Id = 77 };
        var dependent = new ProductNN { Id = 78 };

        context.Entry(dependent).Property("CategoryId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("CategoryId").CurrentValue);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78, Parent = principal };
        principal.Child = dependent;

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78, Parent = principal };
        principal.Child = dependent;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78 };

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78 };
        principal.Child = dependent;

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78, Parent = principal };

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78 };
        principal.Child = dependent;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78, Parent = principal };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78, Parent = principal };
        principal.Child = dependent;

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78, Parent = principal };
        principal.Child = dependent;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78 };

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78 };
        principal.Child = dependent;

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78, Parent = principal };

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78 };
        principal.Child = dependent;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        var dependent = new Child { Id = 78, Parent = principal };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        var dependent = new ChildPN { Id = 78 };

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        var dependent = new ChildPN { Id = 78 };

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        var dependent = new ChildPN { Id = 78 };
        principal.Child = dependent;

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        var dependent = new ChildPN { Id = 78 };
        principal.Child = dependent;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        var dependent = new ChildPN { Id = 78 };
        principal.Child = dependent;

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        var dependent = new ChildPN { Id = 78 };
        principal.Child = dependent;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id = 77 };
        var dependent = new ChildDN { Id = 78 };

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id = 77 };
        var dependent = new ChildDN { Id = 78, Parent = principal };

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id = 77 };
        var dependent = new ChildDN { Id = 78, Parent = principal };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id = 77 };
        var dependent = new ChildDN { Id = 78 };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id = 77 };
        var dependent = new ChildDN { Id = 78, Parent = principal };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id = 77 };
        var dependent = new ChildDN { Id = 78, Parent = principal };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_no_navs_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentNN { Id = 77 };
        var dependent = new ChildNN { Id = 78 };

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_no_navs_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentNN { Id = 77 };
        var dependent = new ChildNN { Id = 78 };

        context.Entry(dependent).Property("ParentId").CurrentValue = principal.Id;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    private class Parent
    {
        public int Id { get; set; }

        public Child Child { get; set; }
    }

    private class Child
    {
        public int Id { get; set; }

        public Parent Parent { get; set; }
    }

    private class ParentPN
    {
        public int Id { get; set; }

        public ChildPN Child { get; set; }
    }

    private class ChildPN
    {
        public int Id { get; set; }
    }

    private class ParentDN
    {
        public int Id { get; set; }
    }

    private class ChildDN
    {
        public int Id { get; set; }

        public ParentDN Parent { get; set; }
    }

    private class ParentNN
    {
        public int Id { get; set; }
    }

    private class ChildNN
    {
        public int Id { get; set; }
    }

    private class CategoryDN
    {
        public int Id { get; set; }
    }

    private class ProductDN
    {
        public int Id { get; set; }

        public CategoryDN Category { get; set; }
    }

    private class CategoryPN
    {
        public CategoryPN()
        {
            Products = new List<ProductPN>();
        }

        public int Id { get; set; }

        public ICollection<ProductPN> Products { get; }
    }

    private class ProductPN
    {
        public int Id { get; set; }
    }

    private class CategoryNN
    {
        public int Id { get; set; }
    }

    private class ProductNN
    {
        public int Id { get; set; }
    }

    private class Category
    {
        public Category()
        {
            Products = new List<Product>();
        }

        public int Id { get; set; }

        public ICollection<Product> Products { get; }
    }

    private class Product
    {
        public Product()
        {
            SpecialOffers = new List<SpecialOffer>();
        }

        public int Id { get; set; }

        public Category Category { get; set; }

        // ReSharper disable once CollectionNeverUpdated.Local
        public ICollection<SpecialOffer> SpecialOffers { get; }
    }

    private class SpecialOffer
    {
        public int Id { get; set; }

        public Product Product { get; set; }
    }

    private class FixupContext : DbContext
    {
        public FixupContext()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasMany(e => e.SpecialOffers)
                .WithOne(e => e.Product)
                .HasForeignKey("ProductId");

            modelBuilder.Entity<Category>()
                .HasMany(e => e.Products)
                .WithOne(e => e.Category)
                .HasForeignKey("CategoryId");

            modelBuilder.Entity<CategoryPN>()
                .HasMany(e => e.Products)
                .WithOne()
                .HasForeignKey("CategoryId");

            modelBuilder.Entity<ProductDN>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey("CategoryId");

            modelBuilder.Entity<ProductNN>()
                .HasOne<CategoryNN>()
                .WithMany()
                .HasForeignKey("CategoryId");

            modelBuilder.Entity<Parent>()
                .HasOne(e => e.Child)
                .WithOne(e => e.Parent)
                .HasForeignKey<Child>("ParentId");

            modelBuilder.Entity<ParentPN>()
                .HasOne(e => e.Child)
                .WithOne()
                .HasForeignKey<ChildPN>("ParentId");

            modelBuilder.Entity<ChildDN>()
                .HasOne(e => e.Parent)
                .WithOne()
                .HasForeignKey<ChildDN>("ParentId");

            modelBuilder.Entity<ParentNN>()
                .HasOne<ChildNN>()
                .WithOne()
                .HasForeignKey<ChildNN>("ParentId");
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

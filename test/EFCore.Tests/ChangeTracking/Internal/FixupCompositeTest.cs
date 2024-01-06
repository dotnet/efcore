// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable MemberHidesStaticFromOuterClass

// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class FixupCompositeTest
{
    private static readonly Guid Guid77 = new("{DE390D36-DAAC-4C8B-91F7-E9F5DAA7EF01}");
    private static readonly Guid Guid78 = new("{4C80406F-49AF-4D85-AFFB-75C146A98A70}");

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_FK_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            Category = principal,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };
        principal.Products.Add(dependent);

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            Category = principal
        };
        principal.Products.Add(dependent);

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };
        principal.Products.Add(dependent);

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2,
            Category = principal
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };
        principal.Products.Add(dependent);

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            Category = principal
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            Category = principal,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };
        principal.Products.Add(dependent);

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            Category = principal
        };
        principal.Products.Add(dependent);

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };
        principal.Products.Add(dependent);

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2,
            Category = principal
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };
        principal.Products.Add(dependent);

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product
        {
            Id1 = 78,
            Id2 = Guid78,
            Category = principal
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };
        principal.Products.Add(dependent);

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN { Id1 = 78, Id2 = Guid78 };
        principal.Products.Add(dependent);

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };
        principal.Products.Add(dependent);

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN { Id1 = 78, Id2 = Guid78 };
        principal.Products.Add(dependent);

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2,
            Category = principal
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN
        {
            Id1 = 78,
            Id2 = Guid78,
            Category = principal
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2,
            Category = principal
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN
        {
            Id1 = 78,
            Id2 = Guid78,
            Category = principal
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryNN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductNN
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new CategoryNN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductNN
        {
            Id1 = 78,
            Id2 = Guid78,
            CategoryId1 = principal.Id1,
            CategoryId2 = principal.Id2
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            Parent = principal,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };
        principal.Child = dependent;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
    public void Add_dependent_then_principal_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            Parent = principal
        };
        principal.Child = dependent;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };
        principal.Child = dependent;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2,
            Parent = principal
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };
        principal.Child = dependent;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            Parent = principal
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            Parent = principal,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };
        principal.Child = dependent;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            Parent = principal
        };
        principal.Child = dependent;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };
        principal.Child = dependent;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2,
            Parent = principal
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };
        principal.Child = dependent;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child
        {
            Id1 = 78,
            Id2 = Guid78,
            Parent = principal
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };
        principal.Child = dependent;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN { Id1 = 78, Id2 = Guid78 };
        principal.Child = dependent;

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };
        principal.Child = dependent;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN { Id1 = 78, Id2 = Guid78 };
        principal.Child = dependent;

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2,
            Parent = principal
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN
        {
            Id1 = 78,
            Id2 = Guid78,
            Parent = principal
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2,
            Parent = principal
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN
        {
            Id1 = 78,
            Id2 = Guid78,
            Parent = principal
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentNN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildNN
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
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
        var principal = new ParentNN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildNN
        {
            Id1 = 78,
            Id2 = Guid78,
            ParentId1 = principal.Id1,
            ParentId2 = principal.Id2
        };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Category { Id1 = 77, Id2 = Guid77 };
        var dependent = new Product { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

        dependent.Category = principal;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(0, dependent.CategoryId1);
                Assert.Same(principal, dependent.Category);
                Assert.Empty(principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

        principal.Products.Add(dependent);

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(0, dependent.CategoryId1);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

        principal.Products.Add(dependent);

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

        dependent.Category = principal;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(0, dependent.CategoryId1);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_no_navs_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryNN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductNN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_no_navs_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryNN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ProductNN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

        dependent.CategoryId1 = principal.Id1;
        dependent.CategoryId2 = principal.Id2;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.CategoryId1);
                Assert.Equal(principal.Id2, dependent.CategoryId2);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id1 = 77, Id2 = Guid77 };
        var dependent = new Child { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

        dependent.Parent = principal;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(0, dependent.ParentId1);
                Assert.Same(principal, dependent.Parent);
                Assert.Null(principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

        principal.Child = dependent;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(0, dependent.ParentId1);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildPN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

        principal.Child = dependent;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Added, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

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
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildDN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

        dependent.Parent = principal;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(0, dependent.ParentId1);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_no_navs_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentNN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildNN { Id1 = 78, Id2 = Guid78 };

        context.Entry(dependent).State = entityState;

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
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_no_navs_FK_set_no_navs_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentNN { Id1 = 77, Id2 = Guid77 };
        var dependent = new ChildNN { Id1 = 78, Id2 = Guid78 };

        context.Entry(principal).State = entityState;

        dependent.ParentId1 = principal.Id1;
        dependent.ParentId2 = principal.Id2;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id1, dependent.ParentId1);
                Assert.Equal(principal.Id2, dependent.ParentId2);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_circular_one_to_many(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentShared { ID = 77, FavoriteChildID = 78 };
        var dependent = new ChildShared { ID = 78, ParentID = 77 };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.ID, dependent.ParentID);
                Assert.Same(principal, dependent.ParentShared);
                Assert.Equal(new[] { dependent }.ToList(), principal.Children);
                Assert.Equal(dependent.ID, principal.FavoriteChildID);
                Assert.Same(dependent, principal.FavoriteChildShared);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    private class Parent
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public Child Child { get; set; }
    }

    private class Child
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int ParentId1 { get; set; }
        public Guid ParentId2 { get; set; }

        public Parent Parent { get; set; }
    }

    private class ParentPN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public ChildPN Child { get; set; }
    }

    private class ChildPN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int ParentId1 { get; set; }
        public Guid ParentId2 { get; set; }
    }

    private class ParentDN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
    }

    private class ChildDN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int ParentId1 { get; set; }
        public Guid ParentId2 { get; set; }
        public ParentDN Parent { get; set; }
    }

    private class ParentNN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
    }

    private class ChildNN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int ParentId1 { get; set; }
        public Guid ParentId2 { get; set; }
    }

    private class CategoryDN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
    }

    private class ProductDN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int CategoryId1 { get; set; }
        public Guid CategoryId2 { get; set; }
        public CategoryDN Category { get; set; }
    }

    private class CategoryPN
    {
        public CategoryPN()
        {
            Products = new List<ProductPN>();
        }

        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public ICollection<ProductPN> Products { get; }
    }

    private class ProductPN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int CategoryId1 { get; set; }
        public Guid CategoryId2 { get; set; }
    }

    private class CategoryNN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }
    }

    private class ProductNN
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int CategoryId1 { get; set; }
        public Guid CategoryId2 { get; set; }
    }

    private class Category
    {
        public Category()
        {
            Products = new List<Product>();
        }

        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public ICollection<Product> Products { get; }
    }

    private class Product
    {
        public int Id1 { get; set; }
        public Guid Id2 { get; set; }

        public int CategoryId1 { get; set; }
        public Guid CategoryId2 { get; set; }
        public Category Category { get; set; }
    }

    public class ParentShared
    {
        public long ID { get; set; }
        public long? FavoriteChildID { get; set; }

        public virtual ChildShared FavoriteChildShared { get; set; }
        public virtual List<ChildShared> Children { get; } = [];
    }

    public class ChildShared
    {
        public long ParentID { get; set; }
        public long ID { get; set; }

        public virtual ParentShared ParentShared { get; set; }
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
            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                    b.HasOne(e => e.Child)
                        .WithOne(e => e.Parent)
                        .HasForeignKey<Child>(
                            e => new { e.ParentId1, e.ParentId2 });
                });

            modelBuilder.Entity<Child>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                });

            modelBuilder.Entity<ParentPN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                    b.HasOne(e => e.Child)
                        .WithOne()
                        .HasForeignKey<ChildPN>(
                            e => new { e.ParentId1, e.ParentId2 });
                });

            modelBuilder.Entity<ChildPN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                });

            modelBuilder.Entity<ParentDN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                    b.HasOne<ChildDN>()
                        .WithOne(e => e.Parent)
                        .HasForeignKey<ChildDN>(
                            e => new { e.ParentId1, e.ParentId2 });
                });

            modelBuilder.Entity<ChildDN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                });

            modelBuilder.Entity<ParentNN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                    b.HasOne<ChildNN>()
                        .WithOne()
                        .HasForeignKey<ChildNN>(
                            e => new { e.ParentId1, e.ParentId2 });
                });

            modelBuilder.Entity<ChildNN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                });

            modelBuilder.Entity<CategoryDN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                    b.HasMany<ProductDN>()
                        .WithOne(e => e.Category)
                        .HasForeignKey(
                            e => new { e.CategoryId1, e.CategoryId2 })
                        .IsRequired(false);
                });

            modelBuilder.Entity<ProductDN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                });

            modelBuilder.Entity<CategoryPN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                    b.HasMany(e => e.Products)
                        .WithOne()
                        .HasForeignKey(
                            e => new { e.CategoryId1, e.CategoryId2 })
                        .IsRequired(false);
                });

            modelBuilder.Entity<ProductPN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                });

            modelBuilder.Entity<CategoryNN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                    b.HasMany<ProductNN>()
                        .WithOne()
                        .HasForeignKey(
                            e => new { e.CategoryId1, e.CategoryId2 })
                        .IsRequired(false);
                });

            modelBuilder.Entity<ProductNN>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                });

            modelBuilder.Entity<Category>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                    b.HasMany(e => e.Products)
                        .WithOne(e => e.Category)
                        .HasForeignKey(
                            e => new { e.CategoryId1, e.CategoryId2 })
                        .IsRequired(false);
                });

            modelBuilder.Entity<Product>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                });

            modelBuilder.Entity<ParentShared>(
                entity =>
                {
                    entity.HasOne(d => d.FavoriteChildShared)
                        .WithOne()
                        .HasForeignKey<ParentShared>(d => new { d.ID, d.FavoriteChildID });
                });

            modelBuilder.Entity<ChildShared>(
                entity =>
                {
                    entity.HasKey(d => new { d.ParentID, d.ID });

                    entity.HasOne(d => d.ParentShared)
                        .WithMany(p => p.Children)
                        .HasForeignKey(d => d.ParentID);
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

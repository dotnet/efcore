// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class FixupTest
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

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_set_both_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: true, setFk: true, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: true, setFk: false, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: true, setFk: true, setToPrincipal: false, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: true, setFk: true, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: true, setFk: true, setToPrincipal: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: true, setFk: false, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many(
            entityState, principalFirst: true, setFk: false, setToPrincipal: true, setToDependent: false);

    private void Add_principal_and_dependent_one_to_many(
        EntityState entityState,
        bool principalFirst,
        bool setFk,
        bool setToPrincipal,
        bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new Category(77);
        var dependent = new Product(78, 0);
        if (setFk)
        {
            dependent.SetCategoryId(principal.Id);
        }

        if (setToPrincipal)
        {
            dependent.SetCategory(principal);
        }

        if (setToDependent)
        {
            principal.AddProduct(dependent);
        }

        if (principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        context.Entry(dependent).State = entityState;
        if (!principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }, principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_prin_uni(entityState, principalFirst: false, setFk: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_prin_uni(entityState, principalFirst: false, setFk: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_prin_uni(entityState, principalFirst: false, setFk: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_prin_uni(entityState, principalFirst: true, setFk: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_prin_uni(entityState, principalFirst: true, setFk: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_prin_uni(entityState, principalFirst: true, setFk: false, setToDependent: true);

    private void Add_principal_and_dependent_one_to_many_prin_uni(
        EntityState entityState,
        bool principalFirst,
        bool setFk,
        bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id = 77 };
        var dependent = new ProductPN { Id = 78 };
        if (setFk)
        {
            dependent.CategoryId = principal.Id;
        }

        if (setToDependent)
        {
            principal.Products.Add(dependent);
        }

        if (principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        context.Entry(dependent).State = entityState;
        if (!principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(new[] { dependent }, principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_dep_uni(entityState, principalFirst: false, setFk: true, setToPrincipal: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_dep_uni(entityState, principalFirst: false, setFk: true, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_dep_uni(entityState, principalFirst: false, setFk: false, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_dep_uni(entityState, principalFirst: true, setFk: true, setToPrincipal: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_dep_uni(entityState, principalFirst: true, setFk: true, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_many_dep_uni(entityState, principalFirst: true, setFk: false, setToPrincipal: true);

    private void Add_principal_and_dependent_one_to_many_dep_uni(
        EntityState entityState,
        bool principalFirst,
        bool setFk,
        bool setToPrincipal)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id = 77 };
        var dependent = new ProductDN { Id = 78 };
        if (setFk)
        {
            dependent.CategoryId = principal.Id;
        }

        if (setToPrincipal)
        {
            dependent.Category = principal;
        }

        if (principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        context.Entry(dependent).State = entityState;
        if (!principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_many_no_navs_FK_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryNN { Id = 77 };
        var dependent = new ProductNN { Id = 78, CategoryId = principal.Id };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_many_no_navs_FK_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryNN { Id = 77 };
        var dependent = new ProductNN { Id = 78, CategoryId = principal.Id };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
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

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_set_both_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: true, setFk: true, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: true, setFk: false, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: true, setFk: true, setToPrincipal: false, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: true, setFk: true, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: true, setFk: true, setToPrincipal: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: true, setFk: false, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one(
            entityState, principalFirst: true, setFk: false, setToPrincipal: true, setToDependent: false);

    private void Add_principal_and_dependent_one_to_one(
        EntityState entityState,
        bool principalFirst,
        bool setFk,
        bool setToPrincipal,
        bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new Parent(77);
        var dependent = new Child(78, 0);
        if (setFk)
        {
            dependent.SetParentId(principal.Id);
        }

        if (setToPrincipal)
        {
            dependent.SetParent(principal);
        }

        if (setToDependent)
        {
            principal.SetChild(dependent);
        }

        if (principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        context.Entry(dependent).State = entityState;
        if (!principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
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
        => Add_principal_and_dependent_one_to_one_prin_uni(entityState, principalFirst: false, setFk: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one_prin_uni(entityState, principalFirst: false, setFk: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one_prin_uni(entityState, principalFirst: false, setFk: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one_prin_uni(entityState, principalFirst: true, setFk: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one_prin_uni(entityState, principalFirst: true, setFk: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one_prin_uni(entityState, principalFirst: true, setFk: false, setToDependent: true);

    private void Add_principal_and_dependent_one_to_one_prin_uni(
        EntityState entityState,
        bool principalFirst,
        bool setFk,
        bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        var dependent = new ChildPN { Id = 78 };

        if (setFk)
        {
            dependent.ParentId = principal.Id;
        }

        if (setToDependent)
        {
            principal.Child = dependent;
        }

        if (principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        context.Entry(dependent).State = entityState;

        if (!principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
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
        => Add_principal_and_dependent_one_to_one_dep_uni(
            entityState, principalFirst: false, setFk: true, setToPrincipal: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one_dep_uni(
            entityState, principalFirst: false, setFk: true, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one_dep_uni(
            entityState, principalFirst: false, setFk: false, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one_dep_uni(
            entityState, principalFirst: true, setFk: true, setToPrincipal: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one_dep_uni(
            entityState, principalFirst: true, setFk: true, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_and_dependent_one_to_one_dep_uni(
            entityState, principalFirst: true, setFk: false, setToPrincipal: true);

    private void Add_principal_and_dependent_one_to_one_dep_uni(
        EntityState entityState,
        bool principalFirst,
        bool setFk,
        bool setToPrincipal)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id = 77 };
        var dependent = new ChildDN { Id = 78 };

        if (setFk)
        {
            dependent.ParentId = principal.Id;
        }

        if (setToPrincipal)
        {
            dependent.Parent = principal;
        }

        if (principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        context.Entry(dependent).State = entityState;

        if (!principalFirst)
        {
            context.Entry(principal).State = entityState;
        }

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_then_principal_one_to_one_no_navs_FK_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentNN { Id = 77 };
        var dependent = new ChildNN { Id = 78, ParentId = principal.Id };

        context.Entry(dependent).State = entityState;
        context.Entry(principal).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_then_dependent_one_to_one_no_navs_FK_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentNN { Id = 77 };
        var dependent = new ChildNN { Id = 78, ParentId = principal.Id };

        context.Entry(principal).State = entityState;
        context.Entry(dependent).State = entityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_set_both_navs_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many(entityState, setFk: true, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many(entityState, setFk: false, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_set_no_navs_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many(entityState, setFk: true, setToPrincipal: false, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_set_principal_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many(entityState, setFk: true, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many(entityState, setFk: true, setToPrincipal: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many(entityState, setFk: false, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many(entityState, setFk: false, setToPrincipal: true, setToDependent: false);

    private void Add_dependent_but_not_principal_one_to_many(
        EntityState entityState,
        bool setFk,
        bool setToPrincipal,
        bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new Category(77);
        var dependent = new Product(77, 0);

        context.Entry(dependent).State = entityState;

        if (setFk)
        {
            dependent.SetCategoryId(principal.Id);
        }

        if (setToPrincipal)
        {
            dependent.SetCategory(principal);
        }

        if (setToDependent)
        {
            principal.AddProduct(dependent);
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(setToPrincipal || setFk ? principal.Id : 0, dependent.CategoryId);
                Assert.Same(setToPrincipal ? principal : null, dependent.Category);
                Assert.Equal(setToPrincipal || setToDependent ? new[] { dependent } : null, principal.Products);
                Assert.Equal(setToPrincipal ? EntityState.Modified : EntityState.Detached, context.Entry(principal).State);
                Assert.Equal(
                    entityState == EntityState.Unchanged && (setToPrincipal || setFk) ? EntityState.Modified : entityState,
                    context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_set_both_navs_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many(entityState, setFk: true, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many(entityState, setFk: false, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many(entityState, setFk: true, setToPrincipal: false, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many(entityState, setFk: true, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many(entityState, setFk: true, setToPrincipal: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many(entityState, setFk: false, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many(entityState, setFk: false, setToPrincipal: true, setToDependent: false);

    private void Add_principal_but_not_dependent_one_to_many(
        EntityState entityState,
        bool setFk,
        bool setToPrincipal,
        bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new Category(77);
        var dependent = new Product(77, 0);

        context.Entry(principal).State = entityState;

        if (setFk)
        {
            dependent.SetCategoryId(principal.Id);
        }

        if (setToPrincipal)
        {
            dependent.SetCategory(principal);
        }

        if (setToDependent)
        {
            principal.AddProduct(dependent);
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(setToDependent || setFk ? principal.Id : 0, dependent.CategoryId);
                Assert.Same(setToDependent || setToPrincipal ? principal : null, dependent.Category);
                Assert.Equal(setToDependent ? new[] { dependent } : null, principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(
                    setToDependent
                        ? (entityState == EntityState.Added ? EntityState.Added : EntityState.Modified)
                        : EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many_prin_uni(entityState, setFk: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many_prin_uni(entityState, setFk: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many_prin_uni(entityState, setFk: false, setToDependent: true);

    private void Add_dependent_but_not_principal_one_to_many_prin_uni(EntityState entityState, bool setFk, bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id = 77 };
        var dependent = new ProductPN { Id = 78 };

        context.Entry(dependent).State = entityState;

        if (setFk)
        {
            dependent.CategoryId = principal.Id;
        }

        if (setToDependent)
        {
            principal.Products.Add(dependent);
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(setFk ? principal.Id : 0, dependent.CategoryId);
                Assert.Equal(setToDependent ? [dependent] : [], principal.Products);
                Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                Assert.Equal(
                    entityState == EntityState.Unchanged && setFk ? EntityState.Modified : entityState,
                    context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many_prin_uni(entityState, setFk: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many_prin_uni(entityState, setFk: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many_prin_uni(entityState, setFk: false, setToDependent: true);

    private void Add_principal_but_not_dependent_one_to_many_prin_uni(EntityState entityState, bool setFk, bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new CategoryPN { Id = 77 };
        var dependent = new ProductPN { Id = 78 };

        context.Entry(principal).State = entityState;

        if (setFk)
        {
            dependent.CategoryId = principal.Id;
        }

        if (setToDependent)
        {
            principal.Products.Add(dependent);
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(setToDependent ? [dependent] : [], principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(
                    setToDependent
                        ? (entityState == EntityState.Added ? EntityState.Added : EntityState.Modified)
                        : EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many_dep_uni(entityState, setFk: true, setToPrincipal: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many_dep_uni(entityState, setFk: true, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_many_dep_uni(entityState, setFk: false, setToPrincipal: true);

    private void Add_dependent_but_not_principal_one_to_many_dep_uni(EntityState entityState, bool setFk, bool setToPrincipal)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id = 77 };
        var dependent = new ProductDN { Id = 78 };

        context.Entry(dependent).State = entityState;

        if (setFk)
        {
            dependent.CategoryId = principal.Id;
        }

        if (setToPrincipal)
        {
            dependent.Category = principal;
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(setToPrincipal ? principal : null, dependent.Category);
                Assert.Equal(setToPrincipal ? EntityState.Modified : EntityState.Detached, context.Entry(principal).State);
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified,
                    context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many_dep_uni(entityState, setFk: true, setToPrincipal: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many_dep_uni(entityState, setFk: true, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_many_dep_uni(entityState, setFk: false, setToPrincipal: true);

    private void Add_principal_but_not_dependent_one_to_many_dep_uni(EntityState entityState, bool setFk, bool setToPrincipal)
    {
        using var context = new FixupContext();
        var principal = new CategoryDN { Id = 77 };
        var dependent = new ProductDN { Id = 78 };

        context.Entry(principal).State = entityState;

        if (setFk)
        {
            dependent.CategoryId = principal.Id;
        }

        if (setToPrincipal)
        {
            dependent.Category = principal;
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(setFk ? principal.Id : 0, dependent.CategoryId);
                Assert.Same(setToPrincipal ? principal : null, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_many_no_navs_FK_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryNN { Id = 77 };
        var dependent = new ProductNN { Id = 78 };

        context.Entry(dependent).State = entityState;

        dependent.CategoryId = principal.Id;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_many_no_navs_FK_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new CategoryNN { Id = 77 };
        var dependent = new ProductNN { Id = 78 };

        context.Entry(principal).State = entityState;

        dependent.CategoryId = principal.Id;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_set_both_navs_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one(entityState, setFk: true, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one(entityState, setFk: false, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_set_no_navs_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one(entityState, setFk: true, setToPrincipal: false, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_set_principal_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one(entityState, setFk: true, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one(entityState, setFk: true, setToPrincipal: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one(entityState, setFk: false, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one(entityState, setFk: false, setToPrincipal: true, setToDependent: false);

    private void Add_dependent_but_not_principal_one_to_one(
        EntityState entityState,
        bool setFk,
        bool setToPrincipal,
        bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new Parent(77);
        var dependent = new Child(78, 0);

        context.Entry(dependent).State = entityState;

        if (setFk)
        {
            dependent.SetParentId(principal.Id);
        }

        if (setToPrincipal)
        {
            dependent.SetParent(principal);
        }

        if (setToDependent)
        {
            principal.SetChild(dependent);
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(setToPrincipal || setFk ? principal.Id : 0, dependent.ParentId);
                Assert.Same(setToPrincipal ? principal : null, dependent.Parent);
                Assert.Same(setToPrincipal || setToDependent ? dependent : null, principal.Child);
                Assert.Equal(setToPrincipal ? EntityState.Modified : EntityState.Detached, context.Entry(principal).State);
                Assert.Equal(
                    entityState == EntityState.Unchanged && (setFk || setToPrincipal)
                        ? EntityState.Modified
                        : entityState,
                    context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_set_both_navs_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one(
            entityState, setFk: true, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one(
            entityState, setFk: false, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one(
            entityState, setFk: true, setToPrincipal: false, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one(
            entityState, setFk: true, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one(
            entityState, setFk: true, setToPrincipal: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one(
            entityState, setFk: false, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one(
            entityState, setFk: false, setToPrincipal: true, setToDependent: false);

    private void Add_principal_but_not_dependent_one_to_one(
        EntityState entityState,
        bool setFk,
        bool setToPrincipal,
        bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new Parent(77);
        var dependent = new Child(78, 0);

        context.Entry(principal).State = entityState;

        if (setFk)
        {
            dependent.SetParentId(principal.Id);
        }

        if (setToPrincipal)
        {
            dependent.SetParent(principal);
        }

        if (setToDependent)
        {
            principal.SetChild(dependent);
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(setToDependent || setFk ? principal.Id : 0, dependent.ParentId);
                Assert.Same(setToDependent || setToPrincipal ? principal : null, dependent.Parent);
                Assert.Same(setToDependent ? dependent : null, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(
                    setToDependent
                        ? (entityState == EntityState.Added ? EntityState.Added : EntityState.Modified)
                        : EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one_prin_uni(entityState, setFk: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one_prin_uni(entityState, setFk: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one_prin_uni(entityState, setFk: false, setToDependent: true);

    private void Add_dependent_but_not_principal_one_to_one_prin_uni(EntityState entityState, bool setFk, bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        var dependent = new ChildPN { Id = 78 };

        context.Entry(dependent).State = entityState;

        if (setFk)
        {
            dependent.ParentId = principal.Id;
        }

        if (setToDependent)
        {
            principal.Child = dependent;
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(setFk ? principal.Id : 0, dependent.ParentId);
                Assert.Same(setToDependent ? dependent : null, principal.Child);
                Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                Assert.Equal(
                    entityState == EntityState.Unchanged && setFk ? EntityState.Modified : entityState,
                    context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one_prin_uni(entityState, setFk: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one_prin_uni(entityState, setFk: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one_prin_uni(entityState, setFk: false, setToDependent: true);

    private void Add_principal_but_not_dependent_one_to_one_prin_uni(EntityState entityState, bool setFk, bool setToDependent)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        var dependent = new ChildPN { Id = 78 };

        context.Entry(principal).State = entityState;

        if (setFk)
        {
            dependent.ParentId = principal.Id;
        }

        if (setToDependent)
        {
            principal.Child = dependent;
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(setToDependent ? dependent : null, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(
                    setToDependent
                        ? (entityState == EntityState.Added ? EntityState.Added : EntityState.Modified)
                        : EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one_dep_uni(entityState, setFk: true, setToPrincipal: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one_dep_uni(entityState, setFk: true, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_dependent_but_not_principal_one_to_one_dep_uni(entityState, setFk: false, setToPrincipal: true);

    private void Add_dependent_but_not_principal_one_to_one_dep_uni(EntityState entityState, bool setFk, bool setToPrincipal)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id = 77 };
        var dependent = new ChildDN { Id = 78 };

        context.Entry(dependent).State = entityState;

        if (setFk)
        {
            dependent.ParentId = principal.Id;
        }

        if (setToPrincipal)
        {
            dependent.Parent = principal;
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(setToPrincipal ? principal : null, dependent.Parent);
                Assert.Equal(setToPrincipal ? EntityState.Modified : EntityState.Detached, context.Entry(principal).State);
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one_dep_uni(entityState, setFk: true, setToPrincipal: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one_dep_uni(entityState, setFk: true, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        => Add_principal_but_not_dependent_one_to_one_dep_uni(entityState, setFk: false, setToPrincipal: true);

    private void Add_principal_but_not_dependent_one_to_one_dep_uni(EntityState entityState, bool setFk, bool setToPrincipal)
    {
        using var context = new FixupContext();
        var principal = new ParentDN { Id = 77 };
        var dependent = new ChildDN { Id = 78 };

        context.Entry(principal).State = entityState;

        if (setFk)
        {
            dependent.ParentId = principal.Id;
        }

        if (setToPrincipal)
        {
            dependent.Parent = principal;
        }

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(setFk ? principal.Id : 0, dependent.ParentId);
                Assert.Same(setToPrincipal ? principal : null, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_dependent_but_not_principal_one_to_one_no_navs_FK_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentNN { Id = 77 };
        var dependent = new ChildNN { Id = 78 };

        context.Entry(dependent).State = entityState;

        dependent.ParentId = principal.Id;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                Assert.Equal(
                    entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Add_principal_but_not_dependent_one_to_one_no_navs_FK_set(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentNN { Id = 77 };
        var dependent = new ChildNN { Id = 78 };

        context.Entry(principal).State = entityState;

        dependent.ParentId = principal.Id;

        context.ChangeTracker.DetectChanges();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_FK_set_both_navs_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one(oldEntityState, newEntityState, setFk: true, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_FK_not_set_both_navs_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one(oldEntityState, newEntityState, setFk: false, setToPrincipal: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_FK_set_no_navs_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one(oldEntityState, newEntityState, setFk: true, setToPrincipal: false, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_FK_set_principal_nav_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one(oldEntityState, newEntityState, setFk: true, setToPrincipal: false, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_FK_set_dependent_nav_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one(oldEntityState, newEntityState, setFk: true, setToPrincipal: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_FK_not_set_principal_nav_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one(
            oldEntityState, newEntityState, setFk: false, setToPrincipal: false, setToDependent: true,
            detectChanges: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_FK_not_set_dependent_nav_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one(oldEntityState, newEntityState, setFk: false, setToPrincipal: true, setToDependent: false);

    private void Replace_dependent_one_to_one(
        EntityState oldEntityState,
        EntityState newEntityState,
        bool setFk,
        bool setToPrincipal,
        bool setToDependent,
        bool detectChanges = false)
    {
        using var context = new FixupContext();
        context.ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;

        var principal = new Parent(77);
        var oldDependent = new Child(78, principal.Id);
        oldDependent.SetParent(principal);
        principal.SetChild(oldDependent);

        context.Entry(principal).State = oldEntityState;
        context.Entry(oldDependent).State = oldEntityState;

        var newDependent = new Child(88, setFk ? principal.Id : 0);
        if (setToPrincipal)
        {
            newDependent.SetParent(principal);
        }

        if (setToDependent)
        {
            principal.SetChild(newDependent);
        }

        context.Entry(newDependent).State = newEntityState;

        if (detectChanges)
        {
            context.ChangeTracker.DetectChanges();
        }

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, newDependent.ParentId);
                Assert.Same(principal, newDependent.Parent);
                Assert.Same(newDependent, principal.Child);
                Assert.Null(oldDependent.Parent);
                var oldDependentEntry = (PropertyEntry)context.Entry(oldDependent).Property(c => c.ParentId);
                Assert.True(oldDependentEntry.GetInfrastructure().IsConceptualNull(oldDependentEntry.Metadata));
                Assert.Equal(newEntityState, context.Entry(newDependent).State);
                Assert.Equal(oldEntityState, context.Entry(principal).State);
                Assert.Equal(
                    oldEntityState == EntityState.Added ? EntityState.Added : EntityState.Modified,
                    context.Entry(oldDependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_prin_uni_FK_set_no_navs_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one_prin_uni(oldEntityState, newEntityState, setFk: true, setToDependent: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one_prin_uni(oldEntityState, newEntityState, setFk: true, setToDependent: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_prin_uni_FK_not_set_principal_nav_set(
        EntityState oldEntityState,
        EntityState newEntityState)
        => Replace_dependent_one_to_one_prin_uni(
            oldEntityState, newEntityState, setFk: false, setToDependent: true, detectChanges: true);

    private void Replace_dependent_one_to_one_prin_uni(
        EntityState oldEntityState,
        EntityState newEntityState,
        bool setFk,
        bool setToDependent,
        bool detectChanges = false)
    {
        using var context = new FixupContext();
        context.ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;

        var principal = new ParentPN { Id = 77 };
        var oldDependent = new ChildPN { Id = 78, ParentId = principal.Id };
        principal.Child = oldDependent;

        context.Entry(principal).State = oldEntityState;
        context.Entry(oldDependent).State = oldEntityState;

        var newDependent = new ChildPN { Id = 88, ParentId = setFk ? principal.Id : 0 };
        if (setToDependent)
        {
            principal.Child = newDependent;
        }

        context.Entry(newDependent).State = newEntityState;

        if (detectChanges)
        {
            context.ChangeTracker.DetectChanges();
        }

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, newDependent.ParentId);
                Assert.Same(newDependent, principal.Child);
                var oldDependentEntry = (PropertyEntry)context.Entry(oldDependent).Property(c => c.ParentId);
                Assert.True(oldDependentEntry.GetInfrastructure().IsConceptualNull(oldDependentEntry.Metadata));
                Assert.Equal(newEntityState, context.Entry(newDependent).State);
                Assert.Equal(oldEntityState, context.Entry(principal).State);
                Assert.Equal(
                    oldEntityState == EntityState.Added ? EntityState.Added : EntityState.Modified,
                    context.Entry(oldDependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_dep_uni_FK_set_no_navs_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one_dep_uni(oldEntityState, newEntityState, setFk: true, setToPrincipal: false);

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState oldEntityState, EntityState newEntityState)
        => Replace_dependent_one_to_one_dep_uni(oldEntityState, newEntityState, setFk: true, setToPrincipal: true);

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_dep_uni_FK_not_set_dependent_nav_set(
        EntityState oldEntityState,
        EntityState newEntityState)
        => Replace_dependent_one_to_one_dep_uni(oldEntityState, newEntityState, setFk: false, setToPrincipal: true);

    private void Replace_dependent_one_to_one_dep_uni(
        EntityState oldEntityState,
        EntityState newEntityState,
        bool setFk,
        bool setToPrincipal)
    {
        using var context = new FixupContext();
        context.ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;

        var principal = new ParentDN { Id = 77 };
        var oldDependent = new ChildDN
        {
            Id = 78,
            ParentId = principal.Id,
            Parent = principal
        };

        context.Entry(principal).State = oldEntityState;
        context.Entry(oldDependent).State = oldEntityState;

        var newDependent = new ChildDN { Id = 88, ParentId = setFk ? principal.Id : 0 };
        if (setToPrincipal)
        {
            newDependent.Parent = principal;
        }

        context.Entry(newDependent).State = newEntityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, newDependent.ParentId);
                Assert.Same(principal, newDependent.Parent);
                Assert.Null(oldDependent.Parent);
                var oldDependentEntry = (PropertyEntry)context.Entry(oldDependent).Property(c => c.ParentId);
                Assert.True(oldDependentEntry.GetInfrastructure().IsConceptualNull(oldDependentEntry.Metadata));
                Assert.Equal(newEntityState, context.Entry(newDependent).State);
                Assert.Equal(oldEntityState, context.Entry(principal).State);
                Assert.Equal(
                    oldEntityState == EntityState.Added ? EntityState.Added : EntityState.Modified,
                    context.Entry(oldDependent).State);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void Replace_dependent_one_to_one_no_navs_FK_set(EntityState oldEntityState, EntityState newEntityState)
    {
        using var context = new FixupContext();
        context.ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;

        var principal = new ParentNN { Id = 77 };
        var oldDependent = new ChildNN { Id = 78, ParentId = principal.Id };

        context.Entry(principal).State = oldEntityState;
        context.Entry(oldDependent).State = oldEntityState;

        var newDependent = new ChildNN { Id = 88, ParentId = principal.Id };

        context.Entry(newDependent).State = newEntityState;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, newDependent.ParentId);
                var oldDependentEntry = (PropertyEntry)context.Entry(oldDependent).Property(c => c.ParentId);
                Assert.True(oldDependentEntry.GetInfrastructure().IsConceptualNull(oldDependentEntry.Metadata));
                Assert.Equal(newEntityState, context.Entry(newDependent).State);
                Assert.Equal(oldEntityState, context.Entry(principal).State);
                Assert.Equal(
                    oldEntityState == EntityState.Added ? EntityState.Added : EntityState.Modified,
                    context.Entry(oldDependent).State);
            });
    }

    [ConditionalFact] // Issue #6067
    public void Collection_nav_props_remain_fixed_up_after_manual_fixup_and_DetectChanges()
    {
        using (var context = new FixupContext())
        {
            var category = new Category(77);
            var product1 = new Product(777, 0);
            var product2 = new Product(778, 0);
            product1.SetCategory(category);
            product2.SetCategory(category);

            context.Add(product1);
            context.Add(product2);
            context.Add(new Category(78));

            context.SaveChanges();
        }

        using (var context = new FixupContext())
        {
            var category = context.Set<Product>().Include(c => c.Category).ToList().First().Category;

            Assert.Equal(2, category.Products.Count);

            var category2 = context.Set<Category>().ToList().Single(a => a != category);

            Assert.Null(category2.Products);

            var product = category.Products.First();
            category.Products.Remove(product);
            product.SetCategory(category2);
            category2.AddProduct(product);
            Assert.Equal(category.Id, product.CategoryId);

            context.ChangeTracker.DetectChanges();

            Assert.Equal(category2.Id, product.CategoryId);
            Assert.Equal(category, category.Products.Single().Category);
            Assert.Equal(category2, category2.Products.Single().Category); // Throws
        }
    }

    [ConditionalFact]
    public void Navigation_fixup_happens_when_new_entities_are_tracked()
    {
        using var context = new FixupContext();
        context.Add(new Category(11));
        context.Add(new Category(12));
        context.Add(new Category(13));

        context.Add(new Product(21, 11));
        AssertAllFixedUp(context);
        context.Add(new Product(22, 11));
        AssertAllFixedUp(context);
        context.Add(new Product(23, 11));
        AssertAllFixedUp(context);
        context.Add(new Product(24, 12));
        AssertAllFixedUp(context);
        context.Add(new Product(25, 12));
        AssertAllFixedUp(context);

        context.Add(new SpecialOffer(31, 22));
        AssertAllFixedUp(context);
        context.Add(new SpecialOffer(32, 22));
        AssertAllFixedUp(context);
        context.Add(new SpecialOffer(33, 24));
        AssertAllFixedUp(context);
        context.Add(new SpecialOffer(34, 24));
        AssertAllFixedUp(context);
        context.Add(new SpecialOffer(35, 24));
        AssertAllFixedUp(context);

        Assert.Equal(3, context.ChangeTracker.Entries<Category>().Count());
        Assert.Equal(5, context.ChangeTracker.Entries<Product>().Count());
        Assert.Equal(5, context.ChangeTracker.Entries<SpecialOffer>().Count());
    }

    [ConditionalFact]
    public void Navigation_fixup_happens_when_entities_are_tracked_from_query()
    {
        using var context = new FixupContext();
        var categoryType = context.Model.FindEntityType(typeof(Category));
        var productType = context.Model.FindEntityType(typeof(Product));
        var offerType = context.Model.FindEntityType(typeof(SpecialOffer));

        var stateManager = context.GetService<IStateManager>();

        stateManager.StartTrackingFromQuery(categoryType, new Category(11), new Snapshot<int>(11));
        stateManager.StartTrackingFromQuery(categoryType, new Category(12), new Snapshot<int>(12));
        stateManager.StartTrackingFromQuery(categoryType, new Category(13), new Snapshot<int>(13));

        stateManager.StartTrackingFromQuery(productType, new Product(21, 11), new Snapshot<int, int>(21, 11));
        AssertAllFixedUp(context);
        stateManager.StartTrackingFromQuery(productType, new Product(22, 11), new Snapshot<int, int>(22, 11));
        AssertAllFixedUp(context);
        stateManager.StartTrackingFromQuery(productType, new Product(23, 11), new Snapshot<int, int>(23, 11));
        AssertAllFixedUp(context);
        stateManager.StartTrackingFromQuery(productType, new Product(24, 12), new Snapshot<int, int>(24, 12));
        AssertAllFixedUp(context);
        stateManager.StartTrackingFromQuery(productType, new Product(25, 12), new Snapshot<int, int>(25, 12));
        AssertAllFixedUp(context);

        stateManager.StartTrackingFromQuery(offerType, new SpecialOffer(31, 22), new Snapshot<int, int>(31, 22));
        AssertAllFixedUp(context);
        stateManager.StartTrackingFromQuery(offerType, new SpecialOffer(32, 22), new Snapshot<int, int>(32, 22));
        AssertAllFixedUp(context);
        stateManager.StartTrackingFromQuery(offerType, new SpecialOffer(33, 24), new Snapshot<int, int>(33, 24));
        AssertAllFixedUp(context);
        stateManager.StartTrackingFromQuery(offerType, new SpecialOffer(34, 24), new Snapshot<int, int>(34, 24));
        AssertAllFixedUp(context);
        stateManager.StartTrackingFromQuery(offerType, new SpecialOffer(35, 24), new Snapshot<int, int>(35, 24));

        AssertAllFixedUp(context);

        Assert.Equal(3, context.ChangeTracker.Entries<Category>().Count());
        Assert.Equal(5, context.ChangeTracker.Entries<Product>().Count());
        Assert.Equal(5, context.ChangeTracker.Entries<SpecialOffer>().Count());
    }

    [ConditionalFact]
    public void Navigation_fixup_is_non_destructive_to_existing_graphs()
    {
        using var context = new FixupContext();
        var category11 = new Category(11);
        var category12 = new Category(12);
        var category13 = new Category(13);

        var product21 = new Product(21, 11);
        var product22 = new Product(22, 11);
        var product23 = new Product(23, 11);
        var product24 = new Product(24, 12);
        var product25 = new Product(25, 12);

        product21.SetCategory(category11);
        product22.SetCategory(category11);
        product23.SetCategory(category11);
        product24.SetCategory(category12);
        product25.SetCategory(category12);

        category11.AddProduct(product21);
        category11.AddProduct(product22);
        category11.AddProduct(product23);
        category12.AddProduct(product24);
        category12.AddProduct(product25);

        var specialOffer31 = new SpecialOffer(31, 22);
        var specialOffer32 = new SpecialOffer(32, 22);
        var specialOffer33 = new SpecialOffer(33, 24);
        var specialOffer34 = new SpecialOffer(34, 24);
        var specialOffer35 = new SpecialOffer(35, 24);

        specialOffer31.SetProduct(product22);
        specialOffer32.SetProduct(product22);
        specialOffer33.SetProduct(product24);
        specialOffer34.SetProduct(product24);
        specialOffer35.SetProduct(product24);

        product22.AddSpecialOffer(specialOffer31);
        product22.AddSpecialOffer(specialOffer32);
        product24.AddSpecialOffer(specialOffer33);
        product24.AddSpecialOffer(specialOffer34);
        product24.AddSpecialOffer(specialOffer35);

        context.Add(category11);
        AssertAllFixedUp(context);
        context.Add(category12);
        AssertAllFixedUp(context);
        context.Add(category13);
        AssertAllFixedUp(context);

        context.Add(product21);
        AssertAllFixedUp(context);
        context.Add(product22);
        AssertAllFixedUp(context);
        context.Add(product23);
        AssertAllFixedUp(context);
        context.Add(product24);
        AssertAllFixedUp(context);
        context.Add(product25);
        AssertAllFixedUp(context);

        context.Add(specialOffer31);
        AssertAllFixedUp(context);
        context.Add(specialOffer32);
        AssertAllFixedUp(context);
        context.Add(specialOffer33);
        AssertAllFixedUp(context);
        context.Add(specialOffer34);
        AssertAllFixedUp(context);
        context.Add(specialOffer35);
        AssertAllFixedUp(context);

        Assert.Equal(3, category11.Products.Count);
        Assert.Equal(2, category12.Products.Count);
        Assert.Null(category13.Products);

        Assert.Null(product21.SpecialOffers);
        Assert.Equal(2, product22.SpecialOffers.Count);
        Assert.Null(product23.SpecialOffers);
        Assert.Equal(3, product24.SpecialOffers.Count);
        Assert.Null(product25.SpecialOffers);

        Assert.Equal(3, context.ChangeTracker.Entries<Category>().Count());
        Assert.Equal(5, context.ChangeTracker.Entries<Product>().Count());
        Assert.Equal(5, context.ChangeTracker.Entries<SpecialOffer>().Count());
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Attaching_dependent_with_duplicate_principal_resolves(bool copy, bool preserveModified, bool updateOriginal)
    {
        using var context = new FixupContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor(preserveModified, updateOriginal)
                : new IgnoringIdentityResolutionInterceptor());

        var originalCategory = new Category(1) { Value1 = "Original", Value2 = "Original" };
        var originalProduct = new Product(1, 0) { Value1 = "Original", Value2 = "Original" };
        originalCategory.AddProduct(originalProduct);
        context.Attach(originalCategory);

        if (copy)
        {
            context.Entry(originalProduct).Property(e => e.Value2).CurrentValue = "Changed";
            context.Entry(originalCategory).Property(e => e.Value2).CurrentValue = "Changed";
        }

        var newProduct = new Product(2, 0) { Value1 = "New", Value2 = "New" };
        var newCategory = new Category(1) { Value1 = "New", Value2 = "New" };
        newProduct.SetCategory(newCategory);

        context.Attach(newProduct);

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalCategory).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalProduct).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(newProduct).State);

        Assert.Equal(copy ? "New" : "Original", originalCategory.Value1);
        Assert.Equal("Original", originalProduct.Value1);
        Assert.Equal("New", newProduct.Value1);

        if (copy)
        {
            Assert.Equal(preserveModified ? "Changed" : "New", originalCategory.Value2);
            Assert.Equal("Changed", originalProduct.Value2);
            Assert.Equal("New", newProduct.Value2);

            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalCategory).Property(e => e.Value2).OriginalValue);
            Assert.Equal("Original", context.Entry(originalProduct).Property(e => e.Value2).OriginalValue);
            Assert.Equal("New", context.Entry(newProduct).Property(e => e.Value2).OriginalValue);
        }

        Assert.Equal(1, originalProduct.CategoryId);
        Assert.Equal(1, newProduct.CategoryId);
        Assert.Same(originalCategory, originalProduct.Category);
        Assert.Same(originalCategory, newProduct.Category);
        Assert.Equal(2, originalCategory.Products.Count);
        Assert.Contains(originalProduct, originalCategory.Products);
        Assert.Contains(newProduct, originalCategory.Products);
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Attaching_duplicate_dependent_with_duplicate_principal_resolves(bool copy, bool preserveModified, bool updateOriginal)
    {
        using var context = new FixupContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor(preserveModified, updateOriginal)
                : new IgnoringIdentityResolutionInterceptor());

        var originalCategory = new Category(1) { Value1 = "Original", Value2 = "Original" };
        var originalProduct = new Product(1, 0) { Value1 = "Original", Value2 = "Original" };
        originalCategory.AddProduct(originalProduct);
        context.Attach(originalCategory);

        if (copy)
        {
            context.Entry(originalProduct).Property(e => e.Value2).CurrentValue = "Changed";
            context.Entry(originalCategory).Property(e => e.Value2).CurrentValue = "Changed";
        }

        var newProduct = new Product(1, 1) { Value1 = "New", Value2 = "New" };
        var newCategory = new Category(1) { Value1 = "New", Value2 = "New" };
        newProduct.SetCategory(newCategory);

        context.Attach(newProduct);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalCategory).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalProduct).State);

        Assert.Equal(copy ? "New" : "Original", originalCategory.Value1);
        Assert.Equal(copy ? "New" : "Original", originalProduct.Value1);

        if (copy)
        {
            Assert.Equal(preserveModified ? "Changed" : "New", originalCategory.Value2);
            Assert.Equal(preserveModified ? "Changed" : "New", originalProduct.Value2);

            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalCategory).Property(e => e.Value2).OriginalValue);
            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalProduct).Property(e => e.Value2).OriginalValue);
        }

        Assert.Equal(1, originalProduct.CategoryId);
        Assert.Same(originalCategory, originalProduct.Category);
        Assert.Equal(1, originalCategory.Products.Count);
        Assert.Contains(originalProduct, originalCategory.Products);
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Attaching_principal_with_duplicate_dependent_resolves(bool copy, bool preserveModified, bool updateOriginal)
    {
        using var context = new FixupContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor(preserveModified, updateOriginal)
                : new IgnoringIdentityResolutionInterceptor());

        var originalCategory = new Category(1) { Value1 = "Original", Value2 = "Original" };
        var originalProduct = new Product(1, 0) { Value1 = "Original", Value2 = "Original" };
        originalCategory.AddProduct(originalProduct);
        context.Attach(originalCategory);

        if (copy)
        {
            context.Entry(originalProduct).Property(e => e.Value2).CurrentValue = "Changed";
            context.Entry(originalCategory).Property(e => e.Value2).CurrentValue = "Changed";
        }

        var newProduct = new Product(1, 2) { Value1 = "New", Value2 = "New" };
        var newCategory = new Category(2) { Value1 = "New", Value2 = "New" };
        newCategory.AddProduct(newProduct);

        context.Attach(newCategory);

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalCategory).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(newCategory).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalProduct).State);

        Assert.Equal("Original", originalCategory.Value1);
        Assert.Equal("New", newCategory.Value1);
        Assert.Equal(copy ? "New" : "Original", originalProduct.Value1);

        if (copy)
        {
            Assert.Equal("Changed", originalCategory.Value2);
            Assert.Equal(preserveModified ? "Changed" : "New", originalProduct.Value2);
            Assert.Equal("New", newCategory.Value2);

            Assert.Equal("Original", context.Entry(originalCategory).Property(e => e.Value2).OriginalValue);
            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalProduct).Property(e => e.Value2).OriginalValue);
            Assert.Equal("New", context.Entry(newCategory).Property(e => e.Value2).OriginalValue);

            Assert.Equal(2, originalProduct.CategoryId);
            Assert.Same(newCategory, originalProduct.Category);
            Assert.Equal(0, originalCategory.Products.Count);
            Assert.Equal(1, newCategory.Products.Count);
            Assert.Contains(originalProduct, newCategory.Products);
        }
        else
        {
            Assert.Equal(1, originalProduct.CategoryId);
            Assert.Same(originalCategory, originalProduct.Category);
            Assert.Equal(1, originalCategory.Products.Count);
            Assert.Contains(originalProduct, originalCategory.Products);
            Assert.Equal(0, newCategory.Products.Count);
        }
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Attaching_duplicate_principal_with_duplicate_dependent_resolves(bool copy, bool preserveModified, bool updateOriginal)
    {
        using var context = new FixupContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor(preserveModified, updateOriginal)
                : new IgnoringIdentityResolutionInterceptor());

        var originalCategory = new Category(1) { Value1 = "Original", Value2 = "Original" };
        var originalProduct = new Product(1, 0) { Value1 = "Original", Value2 = "Original" };
        originalCategory.AddProduct(originalProduct);
        context.Attach(originalCategory);

        if (copy)
        {
            context.Entry(originalProduct).Property(e => e.Value2).CurrentValue = "Changed";
            context.Entry(originalCategory).Property(e => e.Value2).CurrentValue = "Changed";
        }

        var newProduct = new Product(1, 1) { Value1 = "New", Value2 = "New" };
        var newCategory = new Category(1) { Value1 = "New", Value2 = "New" };
        newCategory.AddProduct(newProduct);

        context.Attach(newCategory);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalCategory).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalProduct).State);

        Assert.Equal(copy ? "New" : "Original", originalCategory.Value1);
        Assert.Equal(copy ? "New" : "Original", originalProduct.Value1);

        if (copy)
        {
            Assert.Equal(preserveModified ? "Changed" : "New", originalCategory.Value2);
            Assert.Equal(preserveModified ? "Changed" : "New", originalProduct.Value2);

            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalCategory).Property(e => e.Value2).OriginalValue);
            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalProduct).Property(e => e.Value2).OriginalValue);
        }

        Assert.Equal(1, originalProduct.CategoryId);
        Assert.Same(originalCategory, originalProduct.Category);
        Assert.Equal(1, originalCategory.Products.Count);
        Assert.Contains(originalProduct, originalCategory.Products);
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Attaching_one_to_one_dependent_with_duplicate_principal_resolves(bool copy, bool preserveModified, bool updateOriginal)
    {
        using var context = new FixupContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor(preserveModified, updateOriginal)
                : new IgnoringIdentityResolutionInterceptor());

        var originalParent = new Parent(1) { Value1 = "Original", Value2 = "Original" };
        var originalChild = new Child(1, 0) { Value1 = "Original", Value2 = "Original" };
        originalParent.SetChild(originalChild);
        context.Attach(originalParent);

        if (copy)
        {
            context.Entry(originalParent).Property(e => e.Value2).CurrentValue = "Changed";
            context.Entry(originalChild).Property(e => e.Value2).CurrentValue = "Changed";
        }

        var newChild = new Child(2, 0) { Value1 = "New", Value2 = "New" };
        var newParent = new Parent(1) { Value1 = "New", Value2 = "New" };
        newChild.SetParent(newParent);

        context.Attach(newChild);

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalParent).State);
        Assert.Equal(EntityState.Deleted, context.Entry(originalChild).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(newChild).State);

        Assert.Equal(copy ? "New" : "Original", originalParent.Value1);
        Assert.Equal("Original", originalChild.Value1);
        Assert.Equal("New", newChild.Value1);

        if (copy)
        {
            Assert.Equal(preserveModified ? "Changed" : "New", originalParent.Value2);
            Assert.Equal("Changed", originalChild.Value2);
            Assert.Equal("New", newChild.Value2);

            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalParent).Property(e => e.Value2).OriginalValue);
            Assert.Equal("Original", context.Entry(originalChild).Property(e => e.Value2).OriginalValue);
            Assert.Equal("New", context.Entry(newChild).Property(e => e.Value2).OriginalValue);
        }

        Assert.Equal(1, newChild.ParentId);
        Assert.Same(originalParent, newChild.Parent);
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Attaching_one_to_one_duplicate_dependent_with_duplicate_principal_resolves(
        bool copy,
        bool preserveModified,
        bool updateOriginal)
    {
        using var context = new FixupContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor(preserveModified, updateOriginal)
                : new IgnoringIdentityResolutionInterceptor());

        var originalParent = new Parent(1) { Value1 = "Original", Value2 = "Original" };
        var originalChild = new Child(1, 0) { Value1 = "Original", Value2 = "Original" };
        originalParent.SetChild(originalChild);
        context.Attach(originalParent);

        if (copy)
        {
            context.Entry(originalParent).Property(e => e.Value2).CurrentValue = "Changed";
            context.Entry(originalChild).Property(e => e.Value2).CurrentValue = "Changed";
        }

        var newChild = new Child(1, 1) { Value1 = "New", Value2 = "New" };
        var newParent = new Parent(1) { Value1 = "New", Value2 = "New" };
        newChild.SetParent(newParent);

        context.Attach(newChild);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalParent).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalChild).State);

        Assert.Equal(copy ? "New" : "Original", originalParent.Value1);
        Assert.Equal(copy ? "New" : "Original", originalChild.Value1);

        if (copy)
        {
            Assert.Equal(preserveModified ? "Changed" : "New", originalParent.Value2);
            Assert.Equal(preserveModified ? "Changed" : "New", originalChild.Value2);

            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalParent).Property(e => e.Value2).OriginalValue);
            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalChild).Property(e => e.Value2).OriginalValue);
        }

        Assert.Equal(1, originalChild.ParentId);
        Assert.Same(originalParent, originalChild.Parent);
        Assert.Same(originalChild, originalParent.Child);
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Attaching_one_to_one_principal_with_duplicate_dependent_resolves(bool copy, bool preserveModified, bool updateOriginal)
    {
        using var context = new FixupContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor(preserveModified, updateOriginal)
                : new IgnoringIdentityResolutionInterceptor());

        var originalParent = new Parent(1) { Value1 = "Original", Value2 = "Original" };
        var originalChild = new Child(1, 0) { Value1 = "Original", Value2 = "Original" };
        originalParent.SetChild(originalChild);
        context.Attach(originalParent);

        if (copy)
        {
            context.Entry(originalParent).Property(e => e.Value2).CurrentValue = "Changed";
            context.Entry(originalChild).Property(e => e.Value2).CurrentValue = "Changed";
        }

        var newChild = new Child(1, 0) { Value1 = "New", Value2 = "New" };
        var newParent = new Parent(2) { Value1 = "New", Value2 = "New" };
        newParent.SetChild(newChild);

        context.Attach(newParent);

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalParent).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(newParent).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalChild).State);

        Assert.Equal("Original", originalParent.Value1);
        Assert.Equal("New", newParent.Value1);
        Assert.Equal(copy ? "New" : "Original", originalChild.Value1);

        if (copy)
        {
            Assert.Equal("Changed", originalParent.Value2);
            Assert.Equal(preserveModified ? "Changed" : "New", originalChild.Value2);
            Assert.Equal("New", newParent.Value2);

            Assert.Equal("Original", context.Entry(originalParent).Property(e => e.Value2).OriginalValue);
            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalChild).Property(e => e.Value2).OriginalValue);
            Assert.Equal("New", context.Entry(newParent).Property(e => e.Value2).OriginalValue);
        }

        Assert.Equal(2, originalChild.ParentId);
        Assert.Same(newParent, originalChild.Parent);
        Assert.Same(originalChild, newParent.Child);
        Assert.Null(originalParent.Child);
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Attaching_one_to_one_duplicate_principal_with_duplicate_dependent_resolves(
        bool copy,
        bool preserveModified,
        bool updateOriginal)
    {
        using var context = new FixupContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor(preserveModified, updateOriginal)
                : new IgnoringIdentityResolutionInterceptor());

        var originalParent = new Parent(1) { Value1 = "Original", Value2 = "Original" };
        var originalChild = new Child(1, 0) { Value1 = "Original", Value2 = "Original" };
        originalParent.SetChild(originalChild);
        context.Attach(originalParent);

        if (copy)
        {
            context.Entry(originalParent).Property(e => e.Value2).CurrentValue = "Changed";
            context.Entry(originalChild).Property(e => e.Value2).CurrentValue = "Changed";
        }

        var newChild = new Child(1, 1) { Value1 = "New", Value2 = "New" };
        var newParent = new Parent(1) { Value1 = "New", Value2 = "New" };
        newParent.SetChild(newChild);

        context.Attach(newParent);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalParent).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalChild).State);

        Assert.Equal(copy ? "New" : "Original", originalParent.Value1);
        Assert.Equal(copy ? "New" : "Original", originalChild.Value1);

        if (copy)
        {
            Assert.Equal(preserveModified ? "Changed" : "New", originalParent.Value2);
            Assert.Equal(preserveModified ? "Changed" : "New", originalChild.Value2);

            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalParent).Property(e => e.Value2).OriginalValue);
            Assert.Equal(updateOriginal ? "New" : "Original", context.Entry(originalChild).Property(e => e.Value2).OriginalValue);
        }

        Assert.Equal(1, originalChild.ParentId);
        Assert.Same(originalParent, originalChild.Parent);
        Assert.Same(originalChild, originalParent.Child);
    }

    [ConditionalTheory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, false, false)]
    [InlineData(true, true, true, false)]
    [InlineData(false, false, false, true)]
    [InlineData(true, false, false, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, true, true, true)]
    public void Attaching_entity_with_duplicate_many_to_many_resolves(
        bool copy,
        bool preserveModified,
        bool updateOriginal,
        bool attachCatless)
    {
        using var context = new FixupContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor(preserveModified, updateOriginal)
                : new IgnoringIdentityResolutionInterceptor());

        var originalHumans = new Human[]
        {
            new()
            {
                Id = 1,
                Name = "Arthur",
                Age = 33
            },
            new()
            {
                Id = 2,
                Name = "Wendy",
                Age = 37
            },
            new()
            {
                Id = 3,
                Name = "Katie",
                Age = 25
            }
        };

        var originalCats = new Cat[]
        {
            new()
            {
                Id = 1,
                Name = "Baxter",
                Age = 7,
                Humans = { originalHumans[0], originalHumans[1] }
            },
            new()
            {
                Id = 2,
                Name = "Alice",
                Age = 7,
                Humans = { originalHumans[0] }
            },
            new()
            {
                Id = 3,
                Name = "Mac",
                Age = 3,
                Humans = { originalHumans[0], originalHumans[1] }
            }
        };

        context.AttachRange(originalCats);

        if (attachCatless)
        {
            context.Attach(originalHumans[2]);
        }

        Assert.Equal(attachCatless ? 11 : 10, context.ChangeTracker.Entries().Count());

        if (copy)
        {
            foreach (var human in originalHumans)
            {
                context.Entry(human).Property(e => e.Age).CurrentValue--;
            }

            foreach (var cat in originalCats)
            {
                context.Entry(cat).Property(e => e.Age).CurrentValue--;
            }
        }

        var newHumans = new Human[]
        {
            new()
            {
                Id = 1,
                Name = "AV",
                Age = 34
            },
            new()
            {
                Id = 2,
                Name = "WV",
                Age = 38
            },
            new()
            {
                Id = 3,
                Name = "KC",
                Age = 26
            }
        };

        var newCats = new Cat[]
        {
            new()
            {
                Id = 1,
                Name = "Daxter",
                Age = 8,
                Humans = { newHumans[2] }
            },
            new()
            {
                Id = 2,
                Name = "Princess",
                Age = 8,
                Humans = { newHumans[0], newHumans[1] }
            },
            new()
            {
                Id = 3,
                Name = "Macavity",
                Age = 4,
                Humans = { newHumans[0], newHumans[1] }
            }
        };

        context.AttachRange(newCats);

        Assert.Equal(13, context.ChangeTracker.Entries().Count());

        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalHumans[0]).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalHumans[1]).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalCats[0]).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalCats[1]).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalCats[2]).State);

        if (attachCatless)
        {
            Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(originalHumans[2]).State);
        }
        else
        {
            Assert.Equal(EntityState.Unchanged, context.Entry(newHumans[2]).State);
        }

        Assert.Equal(copy ? "AV" : "Arthur", originalHumans[0].Name);
        Assert.Equal(copy ? "WV" : "Wendy", originalHumans[1].Name);
        Assert.Equal(copy ? "Daxter" : "Baxter", originalCats[0].Name);
        Assert.Equal(copy ? "Princess" : "Alice", originalCats[1].Name);
        Assert.Equal(copy ? "Macavity" : "Mac", originalCats[2].Name);

        if (attachCatless)
        {
            Assert.Equal(copy ? "KC" : "Katie", originalHumans[2].Name);
        }
        else
        {
            Assert.Equal("KC", newHumans[2].Name);
        }

        if (copy)
        {
            Assert.Equal(preserveModified ? 32 : 34, originalHumans[0].Age);
            Assert.Equal(preserveModified ? 36 : 38, originalHumans[1].Age);
            Assert.Equal(preserveModified ? 6 : 8, originalCats[0].Age);
            Assert.Equal(preserveModified ? 6 : 8, originalCats[1].Age);
            Assert.Equal(preserveModified ? 2 : 4, originalCats[2].Age);

            Assert.Equal(updateOriginal ? 34 : 33, context.Entry(originalHumans[0]).Property(e => e.Age).OriginalValue);
            Assert.Equal(updateOriginal ? 38 : 37, context.Entry(originalHumans[1]).Property(e => e.Age).OriginalValue);
            Assert.Equal(updateOriginal ? 8 : 7, context.Entry(originalCats[0]).Property(e => e.Age).OriginalValue);
            Assert.Equal(updateOriginal ? 8 : 7, context.Entry(originalCats[1]).Property(e => e.Age).OriginalValue);
            Assert.Equal(updateOriginal ? 4 : 3, context.Entry(originalCats[2]).Property(e => e.Age).OriginalValue);

            if (attachCatless)
            {
                Assert.Equal(preserveModified ? 24 : 26, originalHumans[2].Age);
                Assert.Equal(updateOriginal ? 26 : 25, context.Entry(originalHumans[2]).Property(e => e.Age).OriginalValue);
            }
            else
            {
                Assert.Equal(26, newHumans[2].Age);
                Assert.Equal(26, context.Entry(newHumans[2]).Property(e => e.Age).OriginalValue);
            }
        }

        Assert.Equal(3, originalHumans[0].Cats.Count);
        Assert.Equal(3, originalHumans[1].Cats.Count);

        Assert.Equal(3, originalCats[0].Humans.Count);
        Assert.Equal(2, originalCats[1].Humans.Count);
        Assert.Equal(2, originalCats[2].Humans.Count);

        Assert.Contains(originalCats[0], originalHumans[0].Cats);
        Assert.Contains(originalCats[1], originalHumans[0].Cats);
        Assert.Contains(originalCats[2], originalHumans[0].Cats);
        Assert.Contains(originalCats[0], originalHumans[1].Cats);
        Assert.Contains(originalCats[1], originalHumans[1].Cats);
        Assert.Contains(originalCats[2], originalHumans[1].Cats);

        Assert.Contains(originalHumans[0], originalCats[0].Humans);
        Assert.Contains(originalHumans[1], originalCats[0].Humans);

        Assert.Contains(originalHumans[0], originalCats[1].Humans);
        Assert.Contains(originalHumans[1], originalCats[1].Humans);

        Assert.Contains(originalHumans[0], originalCats[2].Humans);
        Assert.Contains(originalHumans[1], originalCats[2].Humans);

        if (attachCatless)
        {
            Assert.Equal(1, originalHumans[2].Cats.Count);
            Assert.Contains(originalCats[0], originalHumans[2].Cats);
            Assert.Contains(originalHumans[2], originalCats[0].Humans);
        }
        else
        {
            Assert.Equal(1, newHumans[2].Cats.Count);
            Assert.Contains(originalCats[0], newHumans[2].Cats);
            Assert.Contains(newHumans[2], originalCats[0].Humans);
        }
    }

    [ConditionalFact]
    public void Comparable_entities_that_comply_are_tracked_correctly()
    {
        using var context = new ComparableEntitiesContext("ComparableEntities");
        var level2a = new Level2 { Name = "Foo" };

        var level2b = new Level2 { Name = "Bar" };

        var level1 = new Level1 { Children = { level2a, level2b } };

        context.Add(level1);
        context.SaveChanges();

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(level1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(level2a).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(level2b).State);

        Assert.Equal(2, level1.Children.Count);
        Assert.Contains(level2a, level1.Children);
        Assert.Contains(level2b, level1.Children);

        level1.Children.Clear();

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(level1).State);
        Assert.Equal(EntityState.Deleted, context.Entry(level2a).State);
        Assert.Equal(EntityState.Deleted, context.Entry(level2b).State);

        Assert.Equal(0, level1.Children.Count);

        var level2c = new Level2 { Name = "Foo" };

        var level2d = new Level2 { Name = "Quz" };

        level1.Children.Add(level2c);
        level1.Children.Add(level2d);

        Assert.Equal(2, level1.Children.Count);
        Assert.Contains(level2c, level1.Children);
        Assert.Contains(level2d, level1.Children);

        Assert.Equal(5, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(level1).State);
        Assert.Equal(EntityState.Deleted, context.Entry(level2a).State);
        Assert.Equal(EntityState.Deleted, context.Entry(level2b).State);
        Assert.Equal(EntityState.Added, context.Entry(level2c).State);
        Assert.Equal(EntityState.Added, context.Entry(level2d).State);

        context.SaveChanges();

        Assert.Equal(2, level1.Children.Count);
        Assert.Contains(level2c, level1.Children);
        Assert.Contains(level2d, level1.Children);

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(level1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(level2c).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(level2d).State);
    }

    private class Level1
    {
        public int Id { get; set; }

        [Required]
        public ICollection<Level2> Children { get; } = new SortedSet<Level2>();
    }

    private class Level2 : IComparable<Level2>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Level1 Level1 { get; set; }
        public int Level1Id { get; set; }

        public int CompareTo(Level2 other)
            => StringComparer.InvariantCultureIgnoreCase.Compare(Name, other.Name);
    }

    private class ComparableEntitiesContext(string databaseName) : DbContext
    {
        private readonly string _databaseName = databaseName;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(_databaseName);

        public DbSet<Level1> Level1s { get; set; }
        public DbSet<Level2> Level2s { get; set; }
    }

    [ConditionalFact]
    public void Use_correct_entity_after_SetValues()
    {
        var detachedProduct = new ProductX { Description = "Heavy Engine XT3" };

        var detachedRoom = new ContainerRoomX { Number = 1, Product = detachedProduct };

        var detachedContainer = new ContainerX { Name = "C1", Rooms = { detachedRoom } };

        using (var context = new EscapeRoom(nameof(EscapeRoom)))
        {
            context.Add(detachedContainer);
            context.SaveChanges();
        }

        using (var context = new EscapeRoom(nameof(EscapeRoom)))
        {
            var attachedProduct = new ProductX { Id = detachedProduct.Id, Description = "Heavy Engine XT3" };

            var attachedRoom = new ContainerRoomX
            {
                Id = detachedRoom.Id,
                ContainerId = detachedRoom.ContainerId,
                Number = 1,
                ProductId = detachedRoom.ProductId,
                Product = attachedProduct
            };

            var attached = new ContainerX
            {
                Id = detachedContainer.Id,
                Name = "C1",
                Rooms = { attachedRoom }
            };

            context.Attach(attached);

            detachedRoom.Product = null;
            detachedRoom.ProductId = null;

            context.Entry(attachedRoom).CurrentValues.SetValues(detachedRoom);

            context.SaveChanges();

            // Fails - see #16546
            //Assert.Equal(EntityState.Unchanged, context.Entry(attachedRoom).State);
        }
    }

    public class ContainerX
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ContainerRoomX> Rooms { get; set; } = [];
    }

    public class ContainerRoomX
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int ContainerId { get; set; }
        public ContainerX Container { get; set; }
        public int? ProductId { get; set; }
        public ProductX Product { get; set; }
    }

    public class ProductX
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public List<ContainerRoomX> Rooms { get; set; } = [];
    }

    protected class EscapeRoom(string databaseName) : DbContext
    {
        private readonly string _databaseName = databaseName;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(_databaseName);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<ContainerRoomX>()
                .HasOne(room => room.Product)
                .WithMany(product => product.Rooms)
                .HasForeignKey(room => room.ProductId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
    }

    [ConditionalFact]
    public void Replaced_duplicate_entities_are_used_even_with_bad_hash()
    {
        using (var context = new BadHashDay("BadHashDay"))
        {
            context.AddRange(
                new ParentX { Id = 101, Name = "Parent1" },
                new ChildX { Id = 201, Name = "Child1" },
                new ParentChildX
                {
                    ParentId = 101,
                    ChildId = 201,
                    SortOrder = 1
                });

            context.SaveChanges();
        }

        using (var context = new BadHashDay("BadHashDay"))
        {
            var parent = context.Set<ParentX>().Single(x => x.Id == 101);
            var join = context.Set<ParentChildX>().Single();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(join).State);

            parent.ParentChildren.Clear();

            var newJoin = new ParentChildX
            {
                ParentId = 101,
                ChildId = 201,
                SortOrder = 1
            };

            parent.ParentChildren = new List<ParentChildX> { newJoin };

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
            Assert.Equal(EntityState.Deleted, context.Entry(join).State);
            Assert.Equal(EntityState.Added, context.Entry(newJoin).State);

            context.SaveChanges();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
            Assert.Equal(EntityState.Detached, context.Entry(join).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(newJoin).State);
        }
    }

    protected class ParentX
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<ParentChildX> ParentChildren { get; set; } = new List<ParentChildX>();
    }

    protected class ParentChildX
    {
        public int ParentId { get; set; }
        public int ChildId { get; set; }
        public int SortOrder { get; set; }
        public virtual ParentX Parent { get; set; }
        public virtual ChildX Child { get; set; }

        // Bad implementation of Equals to test for regression
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var other = (ParentChildX)obj;

            if (!Equals(ParentId, other.ParentId))
            {
                return false;
            }

            if (!Equals(ChildId, other.ChildId))
            {
                return false;
            }

            return true;
        }

        // Bad implementation of GetHashCode to test for regression
        public override int GetHashCode()
        {
            var hashCode = 13;
            hashCode = (hashCode * 7) + ParentId.GetHashCode();
            hashCode = (hashCode * 7) + ChildId.GetHashCode();
            return hashCode;
        }
    }

    protected class ChildX
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<ParentChildX> ParentChildren { get; set; } = new List<ParentChildX>();
    }

    protected class BadHashDay(string databaseName) : DbContext
    {
        private readonly string _databaseName = databaseName;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(_databaseName);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParentX>()
                .HasMany(x => x.ParentChildren)
                .WithOne(op => op.Parent)
                .IsRequired();

            modelBuilder.Entity<ChildX>()
                .HasMany(x => x.ParentChildren)
                .WithOne(op => op.Child)
                .IsRequired();

            modelBuilder.Entity<ParentChildX>().HasKey(
                x => new { x.ParentId, x.ChildId });
        }
    }

    [ConditionalFact]
    public void Detached_entity_is_not_replaced_by_tracked_entity()
    {
        using var context = new BadBeeContext(nameof(BadBeeContext));
        var b1 = new EntityB { EntityBId = 1 };
        context.BEntities.Attach(b1);

        var b2 = new EntityB { EntityBId = 1 };

        var a = new EntityA { EntityAId = 1, EntityB = b2 };

        Assert.Equal(
            CoreStrings.IdentityConflict(
                nameof(EntityB),
                $"{{'{nameof(EntityB.EntityBId)}'}}"),
            Assert.Throws<InvalidOperationException>(() => context.Add(a)).Message);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Detached_entity_can_be_replaced_by_tracked_entity(bool async, bool copy)
    {
        using var context = new BadBeeContext(
            nameof(BadBeeContext), copy
                ? new UpdatingIdentityResolutionInterceptor()
                : new IgnoringIdentityResolutionInterceptor());

        var b1 = new EntityB { EntityBId = 1, Value = "b1" };
        context.BEntities.Attach(b1);

        var b2 = new EntityB { EntityBId = 1, Value = "b2" };

        var a = new EntityA { EntityAId = 1, EntityB = b2 };

        _ = async ? await context.AddAsync(a) : context.Add(a);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Added, context.Entry(a).State);
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(b1).State);

        Assert.Same(a, b1.EntityA);
        Assert.Same(b1, a.EntityB);
        Assert.Equal(b1.EntityBId, a.EntityBId);

        Assert.Equal(copy ? "b2" : "b1", b1.Value);
    }

    private class EntityB
    {
        public int EntityBId { get; set; }
        public string Value { get; set; }
        public EntityA EntityA { get; set; }
    }

    private class EntityA
    {
        public int EntityAId { get; set; }
        public int? EntityBId { get; set; }
        public EntityB EntityB { get; set; }
    }

    private class BadBeeContext(string databaseName, params IInterceptor[] interceptors) : DbContext
    {
        private readonly IInterceptor[] _interceptors = interceptors;
        private readonly string _databaseName = databaseName;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .AddInterceptors(_interceptors)
                .UseInMemoryDatabase(_databaseName);

        public DbSet<EntityA> AEntities { get; set; }
        public DbSet<EntityB> BEntities { get; set; }
    }

    protected virtual void AssertAllFixedUp(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<Product>())
        {
            var product = entry.Entity;
            if (product.CategoryId is 11 or 12)
            {
                Assert.Equal(product.CategoryId, product.Category.Id);
                Assert.Contains(product, product.Category.Products);
            }
            else
            {
                Assert.Null(product.Category);
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<SpecialOffer>())
        {
            var offer = entry.Entity;
            if (offer.ProductId is 22 or 24)
            {
                Assert.Equal(offer.ProductId, offer.Product.Id);
                Assert.Contains(offer, offer.Product.SpecialOffers);
            }
            else
            {
                Assert.Null(offer.Product);
            }
        }
    }

    private class Cat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public ICollection<Human> Humans { get; } = new List<Human>();
    }

    private class Human
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public ICollection<Cat> Cats { get; } = new List<Cat>();
    }

    private class Parent(int id)
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private readonly int _id = id;
        private Child _child;

        // ReSharper disable once ConvertToAutoProperty
        public int Id
            => _id;

        public string Value1 { get; set; }
        public string Value2 { get; set; }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public Child Child
            => _child;

        public void SetChild(Child child)
            => _child = child;
    }

    private class Child(int id, int parentId)
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private readonly int _id = id;
        private int _parentId = parentId;
        private Parent _parent;

        // ReSharper disable once ConvertToAutoProperty
        public int Id
            => _id;

        public string Value1 { get; set; }
        public string Value2 { get; set; }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public int ParentId
            => _parentId;

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public Parent Parent
            => _parent;

        public void SetParent(Parent parent)
            => _parent = parent;

        public void SetParentId(int parentId)
            => _parentId = parentId;
    }

    private class ParentPN
    {
        public int Id { get; set; }

        public ChildPN Child { get; set; }
    }

    private class ChildPN
    {
        public int Id { get; set; }

        public int ParentId { get; set; }
    }

    private class ParentDN
    {
        public int Id { get; set; }
    }

    private class ChildDN
    {
        public int Id { get; set; }
        public int ParentId { get; set; }

        public ParentDN Parent { get; set; }
    }

    private class ParentNN
    {
        public int Id { get; set; }
    }

    private class ChildNN
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
    }

    private class CategoryDN
    {
        public int Id { get; set; }
    }

    private class ProductDN
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

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
        public int CategoryId { get; set; }
    }

    private class CategoryNN
    {
        public int Id { get; set; }
    }

    private class ProductNN
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
    }

    private class Category
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private readonly int _id;
        private ICollection<Product> _products;

        // ReSharper disable once UnusedMember.Local
        public Category()
        {
        }

        public Category(int id)
        {
            _id = id;
        }

        public string Value1 { get; set; }
        public string Value2 { get; set; }

        // ReSharper disable once ConvertToAutoProperty
        public int Id
            => _id;

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public ICollection<Product> Products
            => _products;

        public void AddProduct(Product product)
            => (_products ??= new List<Product>()).Add(product);
    }

    private class Product
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private readonly int _id;
        private int _categoryId;
        private Category _category;

        // ReSharper disable once UnusedMember.Local
        public Product()
        {
        }

        public Product(int id, int categoryId)
        {
            _id = id;
            _categoryId = categoryId;
        }

        // ReSharper disable once ConvertToAutoProperty
        public int Id
            => _id;

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public int CategoryId
            => _categoryId;

        public void SetCategoryId(int categoryId)
            => _categoryId = categoryId;

        public string Value1 { get; set; }
        public string Value2 { get; set; }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public Category Category
            => _category;

        public void SetCategory(Category category)
            => _category = category;

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public ICollection<SpecialOffer> SpecialOffers
        {
            get;
            private set;
        }

        public void AddSpecialOffer(SpecialOffer specialOffer)
            => (SpecialOffers ??= new List<SpecialOffer>()).Add(specialOffer);
    }

    private class SpecialOffer
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private readonly int _id;
        private int _productId;
        private Product _product;

        // ReSharper disable once UnusedMember.Local
        public SpecialOffer()
        {
        }

        public SpecialOffer(int id, int productId)
        {
            _id = id;
            _productId = productId;
        }

        // ReSharper disable once ConvertToAutoProperty
        public int Id
            => _id;

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public int ProductId
            => _productId;

        // ReSharper disable once UnusedMember.Local
        public void SetProductId(int productId)
            => _productId = productId;

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public Product Product
            => _product;

        public void SetProduct(Product product)
            => _product = product;
    }

    private sealed class FixupContext : DbContext
    {
        private readonly IInterceptor[] _interceptors;
        private readonly string _databaseName;

        public FixupContext(params IInterceptor[] interceptors)
            : this(null, interceptors)
        {
        }

        public FixupContext(string databaseName, params IInterceptor[] interceptors)
        {
            _interceptors = interceptors;
            _databaseName = databaseName;
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.Property(e => e.CategoryId);
                    b.HasMany(e => e.SpecialOffers)
                        .WithOne(e => e.Product);
                });

            modelBuilder.Entity<Category>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.HasMany(e => e.Products)
                        .WithOne(e => e.Category);
                });

            modelBuilder.Entity<SpecialOffer>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.Property(e => e.ProductId);
                });

            modelBuilder.Entity<CategoryPN>()
                .HasMany(e => e.Products)
                .WithOne()
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<ProductDN>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<ProductNN>()
                .HasOne<CategoryNN>()
                .WithMany()
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.HasOne(e => e.Child)
                        .WithOne(e => e.Parent)
                        .HasForeignKey<Child>(e => e.ParentId);
                });

            modelBuilder.Entity<Child>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.Property(e => e.ParentId);
                });

            modelBuilder.Entity<ParentPN>()
                .HasOne(e => e.Child)
                .WithOne()
                .HasForeignKey<ChildPN>(e => e.ParentId);

            modelBuilder.Entity<ChildDN>()
                .HasOne(e => e.Parent)
                .WithOne()
                .HasForeignKey<ChildDN>(e => e.ParentId);

            modelBuilder.Entity<ParentNN>()
                .HasOne<ChildNN>()
                .WithOne()
                .HasForeignKey<ChildNN>(e => e.ParentId);

            modelBuilder.Entity<FixupBlog>();

            modelBuilder.Entity<Cat>();
        }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .AddInterceptors(_interceptors)
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(_databaseName ?? nameof(FixupContext));
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

    [ConditionalFact]
    public void Collection_nav_props_remain_fixed_up_after_DetectChanges()
    {
        using (var db = new Context4853())
        {
            var assembly = new TestAssembly { Name = "Assembly1" };
            db.Classes.Add(
                new TestClass { Assembly = assembly, Name = "Class1" });
            db.Classes.Add(
                new TestClass { Assembly = assembly, Name = "Class2" });
            db.SaveChanges();
        }

        using (var db = new Context4853())
        {
            var testClass = db.Classes.ToList().First();
            db.Entry(testClass).Reference(e => e.Assembly).Load();
            var assembly = testClass.Assembly;

            Assert.Equal(2, assembly.Classes.Count);

            db.ChangeTracker.DetectChanges();

            Assert.Equal(2, assembly.Classes.Count);
        }
    }

    private class TestAssembly
    {
        [Key]
        public string Name { get; set; }

        // ReSharper disable once CollectionNeverUpdated.Local
        public ICollection<TestClass> Classes { get; } = new List<TestClass>();
    }

    private class TestClass
    {
        public TestAssembly Assembly { get; set; }
        public string Name { get; set; }
    }

    private class Context4853 : DbContext
    {
        // ReSharper disable once UnusedMember.Local
        public DbSet<TestAssembly> Assemblies { get; set; }
        public DbSet<TestClass> Classes { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(FixupContext));

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TestClass>(
                x =>
                {
                    x.Property<string>("AssemblyName");
                    x.HasKey("AssemblyName", nameof(TestClass.Name));
                    x.HasOne(c => c.Assembly).WithMany(a => a.Classes)
                        .HasForeignKey("AssemblyName");
                });
    }

    private class Dependent
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public Principal Principal { get; set; }
    }

    private class Principal
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    private class DetachingContext : DbContext
    {
        public DbSet<Principal> Principals { get; set; }
        public DbSet<Dependent> Dependents { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(DetachingContext));
    }

    [ConditionalFact] // Issue #21949
    public void Detaching_principal_tracks_unreferenced_foreign_keys()
    {
        using var context = new DetachingContext();

        var dependent = new Dependent { Url = "http://myblog.net" };
        var principal = new Principal { Title = "Hello World" };
        dependent.Principal = principal;
        context.AddRange(dependent, principal);

        Assert.Equal(EntityState.Added, context.Entry(principal).State);
        Assert.Equal(EntityState.Added, context.Entry(dependent).State);

        var principalId = principal.Id;
        Assert.Equal(principalId, context.Entry(dependent).Property<int?>("PrincipalId").CurrentValue);
        Assert.Same(principal, dependent.Principal);

        context.Entry(principal).State = EntityState.Detached;

        Assert.Equal(EntityState.Detached, context.Entry(principal).State);
        Assert.Equal(EntityState.Added, context.Entry(dependent).State);

        principal.Id = 0; // So it re-adds
        dependent.Principal = principal;

        context.Add(principal);
        Assert.NotEqual(principalId, principal.Id);

        Assert.Equal(principal.Id, context.Entry(dependent).Property<int?>("PrincipalId").CurrentValue);
        Assert.Same(principal, dependent.Principal);

        Assert.Equal(EntityState.Added, context.Entry(principal).State);
        Assert.Equal(EntityState.Added, context.Entry(dependent).State);
    }

    public class FixupBlog
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public FixupSite FixupSite { get; set; }
        public List<FixupPost> Posts { get; } = [];
    }

    public class FixupSite
    {
        public int Id { get; set; }

        public int? FixupBlogId { get; set; }
        public FixupBlog FixupBlog { get; set; }
    }

    public class FixupPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int? FixupBlogId { get; set; }
        public FixupBlog FixupBlog { get; set; }
        public List<FixupTag> Tags { get; } = [];
    }

    public class FixupTag
    {
        public int Id { get; set; }
        public string Content { get; set; }

        public List<FixupPost> Posts { get; } = [];
    }

    [ConditionalFact]
    public void Detaching_required_one_to_many_dependent_does_not_clear_navigation_to_deleted_principal()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var principal = context.Add(new Category()).Entity;
            principal.AddProduct(new Product());
            context.ChangeTracker.DetectChanges();
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var principal = context.Set<Category>().Include(e => e.Products).Single();
            var dependent = principal.Products.First();

            context.Remove(principal);

            Assert.Same(dependent, principal.Products.FirstOrDefault());
            Assert.Same(principal, dependent.Category);
            Assert.Equal(principal.Id, dependent.CategoryId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);

            context.Entry(dependent).State = EntityState.Detached;

            Assert.Same(dependent, principal.Products.FirstOrDefault());
            Assert.Same(principal, dependent.Category);
            Assert.Equal(principal.Id, dependent.CategoryId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
        }
    }

    [ConditionalFact]
    public void Detaching_required_one_to_many_principal_does_not_clear_navigation_to_deleted_dependent()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var principal = context.Add(new Category()).Entity;
            principal.AddProduct(new Product());
            context.ChangeTracker.DetectChanges();
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var principal = context.Set<Category>().Include(e => e.Products).Single();
            var dependent = principal.Products.First();

            context.Remove(dependent);

            Assert.Same(dependent, principal.Products.FirstOrDefault());
            Assert.Same(principal, dependent.Category);
            Assert.Equal(principal.Id, dependent.CategoryId);
            Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
            Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);

            context.Entry(principal).State = EntityState.Detached;

            Assert.Same(dependent, principal.Products.FirstOrDefault());
            Assert.Same(principal, dependent.Category);
            Assert.Equal(principal.Id, dependent.CategoryId);
            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
            Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);
        }
    }

    [ConditionalFact]
    public void Detaching_required_one_to_one_dependent_does_not_clear_navigation_to_deleted_principal()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var principal = context.Add(new Parent(1)).Entity;
            principal.SetChild(new Child(2, 1));
            context.ChangeTracker.DetectChanges();
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var principal = context.Set<Parent>().Include(e => e.Child).Single();
            var dependent = principal.Child;

            context.Remove(principal);

            Assert.Same(dependent, principal.Child);
            Assert.Same(principal, dependent.Parent);
            Assert.Equal(principal.Id, dependent.ParentId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);

            context.Entry(dependent).State = EntityState.Detached;

            Assert.Same(dependent, principal.Child);
            Assert.Same(principal, dependent.Parent);
            Assert.Equal(principal.Id, dependent.ParentId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
        }
    }

    [ConditionalFact]
    public void Detaching_required_one_to_one_principal_does_not_clear_navigation_to_deleted_dependent()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var principal = context.Add(new Parent(1)).Entity;
            principal.SetChild(new Child(2, 1));
            context.ChangeTracker.DetectChanges();
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var principal = context.Set<Parent>().Include(e => e.Child).Single();
            var dependent = principal.Child;

            context.Remove(dependent);

            Assert.Same(dependent, principal.Child);
            Assert.Same(principal, dependent.Parent);
            Assert.Equal(principal.Id, dependent.ParentId);
            Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
            Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);

            context.Entry(principal).State = EntityState.Detached;

            Assert.Same(dependent, principal.Child);
            Assert.Same(principal, dependent.Parent);
            Assert.Equal(principal.Id, dependent.ParentId);
            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
            Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);
        }
    }

    [ConditionalFact]
    public void Detaching_optional_one_to_many_dependent_does_not_clear_navigation_to_deleted_principal()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(
                new FixupBlog { FixupSite = new FixupSite(), Posts = { new FixupPost { Tags = { new FixupTag() } } } });
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var principal = context.Set<FixupBlog>().Include(e => e.Posts).Single();
            var dependent = principal.Posts.First();

            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
            context.Remove(principal);

            Assert.Same(dependent, principal.Posts.FirstOrDefault());
            Assert.Same(principal, dependent.FixupBlog);
            Assert.Equal(principal.Id, dependent.FixupBlogId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);

            context.Entry(dependent).State = EntityState.Detached;

            Assert.Same(dependent, principal.Posts.FirstOrDefault());
            Assert.Same(principal, dependent.FixupBlog);
            Assert.Equal(principal.Id, dependent.FixupBlogId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
        }
    }

    [ConditionalFact]
    public void Detaching_optional_one_to_many_dependent_does_not_unclear_navigation_fixup_to_deleted_principal()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(
                new FixupBlog { FixupSite = new FixupSite(), Posts = { new FixupPost { Tags = { new FixupTag() } } } });
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var principal = context.Set<FixupBlog>().Include(e => e.Posts).Single();
            var dependent = principal.Posts.First();

            context.Remove(principal);

            Assert.Same(dependent, principal.Posts.FirstOrDefault());
            Assert.Null(dependent.FixupBlog);
            Assert.Null(dependent.FixupBlogId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Modified, context.Entry(dependent).State);

            context.Entry(dependent).State = EntityState.Detached;

            Assert.Same(dependent, principal.Posts.FirstOrDefault());
            Assert.Null(dependent.FixupBlog);
            Assert.Null(dependent.FixupBlogId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
        }
    }

    [ConditionalFact]
    public void Detaching_optional_one_to_many_principal_does_not_clear_navigation_to_deleted_dependent()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(
                new FixupBlog { FixupSite = new FixupSite(), Posts = { new FixupPost { Tags = { new FixupTag() } } } });
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var principal = context.Set<FixupBlog>().Include(e => e.Posts).Single();
            var dependent = principal.Posts.First();

            context.Remove(dependent);

            Assert.Same(dependent, principal.Posts.FirstOrDefault());
            Assert.Same(principal, dependent.FixupBlog);
            Assert.Equal(principal.Id, dependent.FixupBlogId);
            Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
            Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);

            context.Entry(principal).State = EntityState.Detached;

            Assert.Same(dependent, principal.Posts.FirstOrDefault());
            Assert.Same(principal, dependent.FixupBlog);
            Assert.Equal(principal.Id, dependent.FixupBlogId);
            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
            Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);
        }
    }

    [ConditionalFact]
    public void Detaching_optional_one_to_one_dependent_does_not_clear_navigation_to_deleted_principal()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(
                new FixupBlog { FixupSite = new FixupSite(), Posts = { new FixupPost { Tags = { new FixupTag() } } } });
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var principal = context.Set<FixupBlog>().Include(e => e.FixupSite).Single();
            var dependent = principal.FixupSite;

            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
            context.Remove(principal);

            Assert.Same(dependent, principal.FixupSite);
            Assert.Same(principal, dependent.FixupBlog);
            Assert.Equal(principal.Id, dependent.FixupBlogId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);

            context.Entry(dependent).State = EntityState.Detached;

            Assert.Same(dependent, principal.FixupSite);
            Assert.Same(principal, dependent.FixupBlog);
            Assert.Equal(principal.Id, dependent.FixupBlogId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
        }
    }

    [ConditionalFact]
    public void Detaching_optional_one_to_one_dependent_does_not_unclear_navigation_fixup_to_deleted_principal()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(
                new FixupBlog { FixupSite = new FixupSite(), Posts = { new FixupPost { Tags = { new FixupTag() } } } });
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var principal = context.Set<FixupBlog>().Include(e => e.FixupSite).Single();
            var dependent = principal.FixupSite;

            context.Remove(principal);

            Assert.Same(dependent, principal.FixupSite);
            Assert.Null(dependent.FixupBlog);
            Assert.Null(dependent.FixupBlogId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Modified, context.Entry(dependent).State);

            context.Entry(dependent).State = EntityState.Detached;

            Assert.Same(dependent, principal.FixupSite);
            Assert.Null(dependent.FixupBlog);
            Assert.Null(dependent.FixupBlogId);
            Assert.Equal(EntityState.Deleted, context.Entry(principal).State);
            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
        }
    }

    [ConditionalFact]
    public void Detaching_optional_one_to_one_principal_does_not_clear_navigation_to_deleted_dependent()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(
                new FixupBlog { FixupSite = new FixupSite(), Posts = { new FixupPost { Tags = { new FixupTag() } } } });
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var principal = context.Set<FixupBlog>().Include(e => e.FixupSite).Single();
            var dependent = principal.FixupSite;

            context.Remove(dependent);

            Assert.Same(dependent, principal.FixupSite);
            Assert.Same(principal, dependent.FixupBlog);
            Assert.Equal(principal.Id, dependent.FixupBlogId);
            Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);
            Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);

            context.Entry(principal).State = EntityState.Detached;

            Assert.Same(dependent, principal.FixupSite);
            Assert.Same(principal, dependent.FixupBlog);
            Assert.Equal(principal.Id, dependent.FixupBlogId);
            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
            Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);
        }
    }

    [ConditionalFact]
    public void Detaching_other_side_of_deleted_many_to_many_does_not_clear_navigation()
    {
        var databaseName = Guid.NewGuid().ToString();
        using (var context = new FixupContext(databaseName))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(
                new FixupBlog { FixupSite = new FixupSite(), Posts = { new FixupPost { Tags = { new FixupTag() } } } });
            context.SaveChanges();
        }

        using (var context = new FixupContext(databaseName))
        {
            var post = context.Set<FixupPost>().Include(e => e.Tags).Single();
            var tag = post.Tags.Single();

            context.Remove(tag);

            Assert.Same(tag, post.Tags.FirstOrDefault());
            Assert.Same(post, tag.Posts.FirstOrDefault());
            Assert.Equal(EntityState.Deleted, context.Entry(tag).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(post).State);

            context.Entry(post).State = EntityState.Detached;

            Assert.Same(tag, post.Tags.FirstOrDefault());
            Assert.Same(post, tag.Posts.FirstOrDefault());
            Assert.Equal(EntityState.Deleted, context.Entry(tag).State);
            Assert.Equal(EntityState.Detached, context.Entry(post).State);
        }
    }
}

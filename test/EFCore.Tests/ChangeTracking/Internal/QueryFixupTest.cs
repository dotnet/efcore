// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class QueryFixupTest
{
    [ConditionalFact]
    public void Query_dependent_include_principal()
    {
        Seed();

        using var context = new QueryFixupContext();
        var dependent = context.Set<Product>().Include(e => e.Category).Single();
        var principal = dependent.Category;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
            });
    }

    [ConditionalFact]
    public void Query_principal_include_dependent()
    {
        Seed();

        using var context = new QueryFixupContext();
        var principal = context.Set<Category>().Include(e => e.Products).Single();
        var dependent = principal.Products.Single();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
            });
    }

    [ConditionalFact]
    public void Query_dependent_include_principal_unidirectional()
    {
        Seed();

        using var context = new QueryFixupContext();
        var dependent = context.Set<ProductDN>().Include(e => e.Category).Single();
        var principal = dependent.Category;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
            });
    }

    [ConditionalFact]
    public void Query_principal_include_dependent_unidirectional()
    {
        Seed();

        using var context = new QueryFixupContext();
        var principal = context.Set<CategoryPN>().Include(e => e.Products).Single();
        var dependent = principal.Products.Single();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
            });
    }

    [ConditionalFact]
    public void Query_dependent_include_principal_one_to_one()
    {
        Seed();

        using var context = new QueryFixupContext();
        var dependent = context.Set<Child>().Include(e => e.Parent).Single();
        var principal = dependent.Parent;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
            });
    }

    [ConditionalFact]
    public void Query_principal_include_dependent_one_to_one()
    {
        Seed();

        using var context = new QueryFixupContext();
        var principal = context.Set<Parent>().Include(e => e.Child).Single();
        var dependent = principal.Child;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
            });
    }

    [ConditionalFact]
    public void Query_dependent_include_principal_unidirectional_one_to_one()
    {
        Seed();

        using var context = new QueryFixupContext();
        var dependent = context.Set<ChildDN>().Include(e => e.Parent).Single();
        var principal = dependent.Parent;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
            });
    }

    [ConditionalFact]
    public void Query_principal_include_dependent_unidirectional_one_to_one()
    {
        Seed();

        using var context = new QueryFixupContext();
        var principal = context.Set<ParentPN>().Include(e => e.Child).Single();
        var dependent = principal.Child;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(dependent, principal.Child);
            });
    }

    [ConditionalFact]
    public void Query_self_ref()
    {
        Seed();

        using var context = new QueryFixupContext();
        var widgets = context.Set<Widget>().ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Same(principal, dependent.ParentWidget);
                Assert.Equal(new[] { dependent }.ToList(), principal.ChildWidgets);
            });
    }

    [ConditionalFact]
    public void Query_dependent_include_principal_self_ref()
    {
        Seed();

        using var context = new QueryFixupContext();
        var widgets = context.Set<Widget>().Include(e => e.ParentWidget).ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Same(principal, dependent.ParentWidget);
                Assert.Equal(new[] { dependent }.ToList(), principal.ChildWidgets);
            });
    }

    [ConditionalFact]
    public void Query_principal_include_dependent_self_ref()
    {
        Seed();

        using var context = new QueryFixupContext();
        var widgets = context.Set<Widget>().Include(e => e.ChildWidgets).ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Same(principal, dependent.ParentWidget);
                Assert.Equal(new[] { dependent }.ToList(), principal.ChildWidgets);
            });
    }

    [ConditionalFact]
    public void Query_self_ref_prinipal_nav_only()
    {
        Seed();

        using var context = new QueryFixupContext();
        var widgets = context.Set<WidgetPN>().ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Equal(new[] { dependent }.ToList(), principal.ChildWidgets);
            });
    }

    [ConditionalFact]
    public void Query_self_ref_dependent_nav_only()
    {
        Seed();

        using var context = new QueryFixupContext();
        var widgets = context.Set<WidgetDN>().ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Same(principal, dependent.ParentWidget);
            });
    }

    [ConditionalFact]
    public void Query_dependent_include_principal_self_ref_unidirectional()
    {
        Seed();

        using var context = new QueryFixupContext();
        var widgets = context.Set<WidgetDN>().Include(e => e.ParentWidget).ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Same(principal, dependent.ParentWidget);
            });
    }

    [ConditionalFact]
    public void Query_principal_include_dependent_self_ref_unidirectional()
    {
        Seed();

        using var context = new QueryFixupContext();
        var widgets = context.Set<WidgetPN>().Include(e => e.ChildWidgets).ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Equal(new[] { dependent }.ToList(), principal.ChildWidgets);
            });
    }

    [ConditionalFact]
    public void Query_self_ref_one_to_one()
    {
        Seed();

        using var context = new QueryFixupContext();
        var smidgets = context.Set<Smidget>().ToList();
        var dependent = smidgets.Single(e => e.Id == 78);
        var principal = smidgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                Assert.Same(principal, dependent.ParentSmidget);
                Assert.Same(dependent, principal.ChildSmidget);
            });
    }

    [ConditionalFact]
    public void Query_dependent_include_principal_self_ref_one_to_one()
    {
        Seed();

        using var context = new QueryFixupContext();
        var smidgets = context.Set<Smidget>().Include(e => e.ParentSmidget).ToList();
        var dependent = smidgets.Single(e => e.Id == 78);
        var principal = smidgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                Assert.Same(principal, dependent.ParentSmidget);
                Assert.Same(dependent, principal.ChildSmidget);
            });
    }

    [ConditionalFact]
    public void Query_principal_include_dependent_self_ref_one_to_one()
    {
        Seed();

        using var context = new QueryFixupContext();
        var smidgets = context.Set<Smidget>().Include(e => e.ChildSmidget).ToList();
        var dependent = smidgets.Single(e => e.Id == 78);
        var principal = smidgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                Assert.Same(principal, dependent.ParentSmidget);
                Assert.Same(dependent, principal.ChildSmidget);
            });
    }

    [ConditionalFact]
    public void Query_self_ref_one_to_one_principal_nav_only()
    {
        Seed();

        using var context = new QueryFixupContext();
        var smidgets = context.Set<SmidgetPN>().ToList();
        var dependent = smidgets.Single(e => e.Id == 78);
        var principal = smidgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                Assert.Same(dependent, principal.ChildSmidget);
            });
    }

    [ConditionalFact]
    public void Query_self_ref_one_to_one_dependent_nav_only()
    {
        Seed();

        using var context = new QueryFixupContext();
        var smidgets = context.Set<SmidgetDN>().ToList();
        var dependent = smidgets.Single(e => e.Id == 78);
        var principal = smidgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                Assert.Same(principal, dependent.ParentSmidget);
            });
    }

    [ConditionalFact]
    public void Query_dependent_include_principal_self_ref_one_to_one_unidirectional()
    {
        Seed();

        using var context = new QueryFixupContext();
        var smidgets = context.Set<SmidgetDN>().Include(e => e.ParentSmidget).ToList();
        var dependent = smidgets.Single(e => e.Id == 78);
        var principal = smidgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                Assert.Same(principal, dependent.ParentSmidget);
            });
    }

    [ConditionalFact]
    public void Query_principal_include_dependent_self_ref_one_to_one_unidirectional()
    {
        Seed();

        using var context = new QueryFixupContext();
        var smidgets = context.Set<SmidgetPN>().Include(e => e.ChildSmidget).ToList();
        var dependent = smidgets.Single(e => e.Id == 78);
        var principal = smidgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                Assert.Same(dependent, principal.ChildSmidget);
            });
    }

    [ConditionalFact]
    public void Query_dependent_include_principal_multiple_relationships()
    {
        Seed();

        using var context = new QueryFixupContext();
        var dependent = context.Set<Post>().Include(e => e.Blog).Single();
        var principal = dependent.Blog;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.BlogId);
                Assert.Same(principal, dependent.Blog);
                Assert.Equal(new[] { dependent }.ToList(), principal.Posts);

                Assert.Equal(dependent.Id, principal.TopPostId);
                Assert.Same(dependent, principal.TopPost);
            });
    }

    [ConditionalFact]
    public void Query_principal_include_dependent_multiple_relationships()
    {
        Seed();

        using var context = new QueryFixupContext();
        var principal = context.Set<Blog>().Include(e => e.Posts).Single();
        var dependent = principal.Posts.Single();

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.BlogId);
                Assert.Same(principal, dependent.Blog);
                Assert.Equal(new[] { dependent }.ToList(), principal.Posts);

                Assert.Equal(dependent.Id, principal.TopPostId);
                Assert.Same(dependent, principal.TopPost);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_dependent_include_principal_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new Product { CategoryId = 77 };
        context.Entry(newDependent).State = existingState;

        var dependent = context.Set<Product>().Include(e => e.Category).Single();
        var principal = dependent.Category;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Contains(dependent, principal.Products);

                Assert.Equal(principal.Id, newDependent.CategoryId);
                Assert.Same(principal, newDependent.Category);
                Assert.Contains(newDependent, principal.Products);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_principal_include_dependent_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new Product { CategoryId = 77 };
        context.Entry(newDependent).State = existingState;

        var principal = context.Set<Category>().Include(e => e.Products).Single();
        var dependent = principal.Products.Single(e => e.Id != newDependent.Id);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Contains(dependent, principal.Products);

                Assert.Equal(principal.Id, newDependent.CategoryId);
                Assert.Same(principal, newDependent.Category);
                Assert.Contains(newDependent, principal.Products);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_dependent_include_principal_unidirectional_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new ProductDN { CategoryId = 77 };
        context.Entry(newDependent).State = existingState;

        var dependent = context.Set<ProductDN>().Include(e => e.Category).Single();
        var principal = dependent.Category;

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);

                Assert.Equal(principal.Id, newDependent.CategoryId);
                Assert.Same(principal, newDependent.Category);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_principal_include_dependent_unidirectional_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new ProductPN { CategoryId = 77 };
        context.Entry(newDependent).State = existingState;

        var principal = context.Set<CategoryPN>().Include(e => e.Products).Single();
        var dependent = principal.Products.Single(e => e.Id != newDependent.Id);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Contains(dependent, principal.Products);

                Assert.Equal(principal.Id, newDependent.CategoryId);
                Assert.Contains(newDependent, principal.Products);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_self_ref_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new Widget { ParentWidgetId = 77 };
        context.Entry(newDependent).State = existingState;

        var widgets = context.Set<Widget>().ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Same(principal, dependent.ParentWidget);
                Assert.Contains(dependent, principal.ChildWidgets);

                Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                Assert.Same(principal, newDependent.ParentWidget);
                Assert.Contains(newDependent, principal.ChildWidgets);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_dependent_include_principal_self_ref_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new Widget { ParentWidgetId = 77 };
        context.Entry(newDependent).State = existingState;

        var widgets = context.Set<Widget>().Include(e => e.ParentWidget).ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Same(principal, dependent.ParentWidget);
                Assert.Contains(dependent, principal.ChildWidgets);

                Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                Assert.Same(principal, newDependent.ParentWidget);
                Assert.Contains(newDependent, principal.ChildWidgets);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_principal_include_dependent_self_ref_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new Widget { ParentWidgetId = 77 };
        context.Entry(newDependent).State = existingState;

        var widgets = context.Set<Widget>().Include(e => e.ChildWidgets).ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Same(principal, dependent.ParentWidget);
                Assert.Contains(dependent, principal.ChildWidgets);

                Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                Assert.Same(principal, newDependent.ParentWidget);
                Assert.Contains(newDependent, principal.ChildWidgets);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_self_ref_prinipal_nav_only_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new WidgetPN { ParentWidgetId = 77 };
        context.Entry(newDependent).State = existingState;

        var widgets = context.Set<WidgetPN>().ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Contains(dependent, principal.ChildWidgets);

                Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                Assert.Contains(newDependent, principal.ChildWidgets);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_self_ref_dependent_nav_only_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new WidgetDN { ParentWidgetId = 77 };
        context.Entry(newDependent).State = existingState;

        var widgets = context.Set<WidgetDN>().ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Same(principal, dependent.ParentWidget);

                Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                Assert.Same(principal, newDependent.ParentWidget);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_dependent_include_principal_self_ref_unidirectional_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new WidgetDN { ParentWidgetId = 77 };
        context.Entry(newDependent).State = existingState;

        var widgets = context.Set<WidgetDN>().Include(e => e.ParentWidget).ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Same(principal, dependent.ParentWidget);

                Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                Assert.Same(principal, newDependent.ParentWidget);
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Query_principal_include_dependent_self_ref_unidirectional_with_existing(EntityState existingState)
    {
        Seed();

        using var context = new QueryFixupContext();
        var newDependent = new WidgetPN { ParentWidgetId = 77 };
        context.Entry(newDependent).State = existingState;

        var widgets = context.Set<WidgetPN>().Include(e => e.ChildWidgets).ToList();
        var dependent = widgets.Single(e => e.Id == 78);
        var principal = widgets.Single(e => e.Id == 77);

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, dependent.ParentWidgetId);
                Assert.Contains(dependent, principal.ChildWidgets);

                Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                Assert.Contains(newDependent, principal.ChildWidgets);
            });
    }

    [ConditionalFact]
    public void Query_ownership_navigations()
    {
        Seed();

        using var context = new QueryFixupContext();
        var principal = context.Set<Order>()
            .Single();

        AssertFixup(
            context,
            () =>
            {
                var dependent = principal.OrderDetails;
                Assert.Same(principal, dependent.Order);

                var subDependent1 = dependent.BillingAddress;
                var subDependent2 = dependent.ShippingAddress;
                Assert.Same(dependent, subDependent1.OrderDetails);
                Assert.Same(dependent, subDependent2.OrderDetails);
                Assert.Equal("BillMe", subDependent1.Street);
                Assert.Equal("ShipMe", subDependent2.Street);

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                var principalEntry = context.Entry(principal);
                Assert.Equal(EntityState.Unchanged, principalEntry.State);

                var dependentEntry = principalEntry.Reference(p => p.OrderDetails).TargetEntry;
                Assert.Equal(principal.Id, dependentEntry.Property("OrderId").CurrentValue);
                Assert.Equal(EntityState.Unchanged, dependentEntry.State);
                Assert.Equal(nameof(OrderDetails), dependentEntry.Metadata.FindOwnership().PrincipalToDependent.Name);

                var subDependent1Entry = dependentEntry.Reference(p => p.BillingAddress).TargetEntry;
                Assert.Equal(principal.Id, subDependent1Entry.Property("OrderDetailsId").CurrentValue);
                Assert.Equal(EntityState.Unchanged, subDependent1Entry.State);
                Assert.Equal(
                    typeof(OrderDetails).DisplayName()
                    + "."
                    + nameof(OrderDetails.BillingAddress)
                    + "#"
                    + typeof(Address).ShortDisplayName(), subDependent1Entry.Metadata.Name);

                var subDependent2Entry = dependentEntry.Reference(p => p.ShippingAddress).TargetEntry;
                Assert.Equal(principal.Id, subDependent2Entry.Property("OrderDetailsId").CurrentValue);
                Assert.Equal(EntityState.Unchanged, subDependent2Entry.State);
                Assert.Equal(
                    typeof(OrderDetails).DisplayName()
                    + "."
                    + nameof(OrderDetails.ShippingAddress)
                    + "#"
                    + typeof(Address).ShortDisplayName(), subDependent2Entry.Metadata.Name);
            });
    }

    [ConditionalFact]
    public void Query_owned_foreign_key()
    {
        Seed();

        using var context = new QueryFixupContext();
        var foreignKeyValue = context.Set<Order>()
            .Select(o => EF.Property<int?>(o.OrderDetails, "OrderId")).Single();
        var principal = context.Set<Order>().AsNoTracking().Single();

        AssertFixup(
            context,
            () => Assert.Equal(principal.Id, foreignKeyValue));
    }

    [ConditionalFact]
    public void Query_subowned_foreign_key()
    {
        Seed();

        using var context = new QueryFixupContext();
        var foreignKeyValue = context.Set<Order>()
            .Select(o => EF.Property<int?>(o.OrderDetails.BillingAddress, "OrderDetailsId")).Single();
        var principal = context.Set<Order>().AsNoTracking().Single();

        AssertFixup(
            context,
            () => Assert.Equal(principal.Id, foreignKeyValue));
    }

    [ConditionalFact]
    public void Query_owned()
    {
        Seed();

        using var context = new QueryFixupContext();
        var owned = context.Set<Order>().Single().OrderDetails;
        var principal = context.Set<Order>().AsNoTracking().Single();

        AssertFixup(
            context,
            () =>
            {
                var dependentEntry = context.Entry(owned);
                Assert.Equal(principal.Id, dependentEntry.Property("OrderId").CurrentValue);
                Assert.Equal(nameof(Order.OrderDetails), dependentEntry.Metadata.FindOwnership().PrincipalToDependent.Name);
            });
    }

    [ConditionalFact]
    public void Query_subowned()
    {
        Seed();

        using var context = new QueryFixupContext();

        // Owned entity without owner. Issue #24807.
        Assert.Equal(
            CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner,
            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    var subDependent1 = context.Set<Order>()
                        .Include(a => a.OrderDetails.BillingAddress.OrderDetails.Order)
                        .Select(o => o.OrderDetails.BillingAddress)
                        .Single();
                }).Message);

        // var subDependent2 = context.Set<Order>()
        //     .Include(a => a.OrderDetails.ShippingAddress.OrderDetails.Order)
        //     .Select(o => o.OrderDetails.ShippingAddress)
        //     .Single();
        //
        // AssertFixup(
        //     context,
        //     () =>
        //     {
        //         Assert.Equal("BillMe", subDependent1.Street);
        //         Assert.Equal("ShipMe", subDependent2.Street);
        //
        //         var dependent = subDependent1.OrderDetails;
        //         Assert.Same(dependent, subDependent2.OrderDetails);
        //         Assert.NotNull(dependent.Order);
        //         var principal = dependent.Order;
        //
        //         var subDependent1Entry = context.Entry(subDependent1);
        //         Assert.Equal(principal.Id, subDependent1Entry.Property("OrderDetailsId").CurrentValue);
        //         Assert.Equal(typeof(OrderDetails).DisplayName() + "." + nameof(OrderDetails.BillingAddress) + "#" + typeof(Address).ShortDisplayName(), subDependent1Entry.Metadata.Name);
        //
        //         var subDependent2Entry = context.Entry(subDependent2);
        //         Assert.Equal(principal.Id, subDependent2Entry.Property("OrderDetailsId").CurrentValue);
        //         Assert.Equal(typeof(OrderDetails).DisplayName() + "." + nameof(OrderDetails.ShippingAddress) + "#" + typeof(Address).ShortDisplayName(), subDependent2Entry.Metadata.Name);
        //     });
    }

    private static void Seed()
    {
        using var context = new QueryFixupContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        context.AddRange(
            new Blog { Id = 77, TopPostId = 78 },
            new Post { Id = 78, BlogId = 77 },
            new Widget { Id = 77 },
            new Widget { Id = 78, ParentWidgetId = 77 },
            new WidgetPN { Id = 77 },
            new WidgetPN { Id = 78, ParentWidgetId = 77 },
            new WidgetDN { Id = 77 },
            new WidgetDN { Id = 78, ParentWidgetId = 77 },
            new Smidget { Id = 77 },
            new Smidget { Id = 78, ParentSmidgetId = 77 },
            new SmidgetPN { Id = 77 },
            new SmidgetPN { Id = 78, ParentSmidgetId = 77 },
            new SmidgetDN { Id = 77 },
            new SmidgetDN { Id = 78, ParentSmidgetId = 77 },
            new Category { Id = 77 },
            new Product { Id = 78, CategoryId = 77 },
            new CategoryPN { Id = 77 },
            new ProductPN { Id = 78, CategoryId = 77 },
            new CategoryDN { Id = 77 },
            new ProductDN { Id = 78, CategoryId = 77 },
            new Parent { Id = 77 },
            new Child { Id = 78, ParentId = 77 },
            new ParentPN { Id = 77 },
            new ChildPN { Id = 78, ParentId = 77 },
            new ParentDN { Id = 77 },
            new ChildDN { Id = 78, ParentId = 77 },
            new Order
            {
                Id = 77,
                OrderDetails = new OrderDetails
                {
                    BillingAddress = new Address { Street = "BillMe" }, ShippingAddress = new Address { Street = "ShipMe" }
                }
            });

        context.SaveChanges();
    }

    private class Parent
    {
        public int Id { get; set; }

        public Child Child { get; set; }
    }

    private class Child
    {
        public int Id { get; set; }
        public int ParentId { get; set; }

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
        public int Id { get; set; }

        public ICollection<ProductPN> Products { get; } = new List<ProductPN>();
    }

    private class ProductPN
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
    }

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
    }

    private class Order
    {
        public int Id { get; set; }

        public OrderDetails OrderDetails { get; set; }
    }

    private class OrderDetails
    {
        public Order Order { get; }
        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }
    }

    private class Address
    {
        public OrderDetails OrderDetails { get; }
        public string Street { get; set; }
    }

    private class Blog
    {
        public int Id { get; set; }

        public ICollection<Post> Posts { get; } = new List<Post>();

        public int TopPostId { get; set; }
        public Post TopPost { get; set; }
    }

    private class Post
    {
        public int Id { get; set; }
        public int BlogId { get; set; }

        public Blog Blog { get; set; }
    }

    public class Widget
    {
        public int Id { get; set; }

        public int? ParentWidgetId { get; set; }
        public Widget ParentWidget { get; set; }

        public List<Widget> ChildWidgets { get; set; }
    }

    public class WidgetPN
    {
        public int Id { get; set; }

        public int? ParentWidgetId { get; set; }

        public List<WidgetPN> ChildWidgets { get; set; }
    }

    public class WidgetDN
    {
        public int Id { get; set; }

        public int? ParentWidgetId { get; set; }
        public WidgetDN ParentWidget { get; set; }
    }

    public class Smidget
    {
        public int Id { get; set; }

        public int? ParentSmidgetId { get; set; }
        public Smidget ParentSmidget { get; set; }
        public Smidget ChildSmidget { get; set; }
    }

    public class SmidgetPN
    {
        public int Id { get; set; }

        public int? ParentSmidgetId { get; set; }
        public SmidgetPN ChildSmidget { get; set; }
    }

    public class SmidgetDN
    {
        public int Id { get; set; }

        public int? ParentSmidgetId { get; set; }
        public SmidgetDN ParentSmidget { get; set; }
    }

    private class QueryFixupContext : DbContext
    {
        public QueryFixupContext()
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Widget>()
                .HasMany(e => e.ChildWidgets)
                .WithOne(e => e.ParentWidget)
                .HasForeignKey(e => e.ParentWidgetId);

            modelBuilder.Entity<WidgetPN>()
                .HasMany(e => e.ChildWidgets)
                .WithOne()
                .HasForeignKey(e => e.ParentWidgetId);

            modelBuilder.Entity<WidgetDN>()
                .HasOne(e => e.ParentWidget)
                .WithMany()
                .HasForeignKey(e => e.ParentWidgetId);

            modelBuilder.Entity<Smidget>()
                .HasOne(e => e.ParentSmidget)
                .WithOne(e => e.ChildSmidget)
                .HasForeignKey<Smidget>(e => e.ParentSmidgetId);

            modelBuilder.Entity<SmidgetPN>()
                .HasOne<SmidgetPN>()
                .WithOne(e => e.ChildSmidget)
                .HasForeignKey<SmidgetPN>(e => e.ParentSmidgetId);

            modelBuilder.Entity<SmidgetDN>()
                .HasOne(e => e.ParentSmidget)
                .WithOne()
                .HasForeignKey<SmidgetDN>(e => e.ParentSmidgetId);

            modelBuilder.Entity<Category>()
                .HasMany(e => e.Products)
                .WithOne(e => e.Category);

            modelBuilder.Entity<CategoryPN>()
                .HasMany(e => e.Products)
                .WithOne()
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<ProductDN>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<Parent>()
                .HasOne(e => e.Child)
                .WithOne(e => e.Parent)
                .HasForeignKey<Child>(e => e.ParentId);

            modelBuilder.Entity<ParentPN>()
                .HasOne(e => e.Child)
                .WithOne()
                .HasForeignKey<ChildPN>(e => e.ParentId);

            modelBuilder.Entity<ChildDN>()
                .HasOne(e => e.Parent)
                .WithOne()
                .HasForeignKey<ChildDN>(e => e.ParentId);

            modelBuilder.Entity<Blog>()
                .HasMany(e => e.Posts)
                .WithOne(e => e.Blog)
                .HasForeignKey(e => e.BlogId);

            modelBuilder.Entity<Blog>()
                .HasOne(e => e.TopPost)
                .WithOne()
                .HasForeignKey<Blog>(e => e.TopPostId);

            modelBuilder.Entity<Order>(
                pb =>
                {
                    pb.Property(p => p.Id).ValueGeneratedNever();
                    pb.OwnsOne(
                        p => p.OrderDetails, cb =>
                        {
                            cb.Property<int?>("OrderId");
                            cb.WithOwner(c => c.Order)
                                .HasForeignKey("OrderId");

                            cb.OwnsOne(c => c.BillingAddress)
                                .WithOwner(c => c.OrderDetails)
                                .HasForeignKey("OrderDetailsId");

                            cb.OwnsOne(c => c.ShippingAddress)
                                .WithOwner(c => c.OrderDetails)
                                .HasForeignKey("OrderDetailsId");
                        });
                });
        }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(QueryFixupContext));
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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public abstract class TrackGraphTestBase
{
    public class TrackGraphTest : TrackGraphTestBase
    {
        protected override IList<string> TrackGraph(DbContext context, object root, Action<EntityEntryGraphNode> callback)
        {
            var traversal = new List<string>();

            context.ChangeTracker.TrackGraph(
                root, node =>
                {
                    callback(node);

                    traversal.Add(NodeString(node));
                });

            return traversal;
        }

        [ConditionalTheory] // Issue #26461
        [InlineData(false)]
        [InlineData(true)]
        public async Task Can_iterate_over_graph_using_public_surface(bool async)
        {
            using var context = new EarlyLearningCenter(GetType().Name);
            var category = new Category
            {
                Id = 1,
                Products =
                [
                    new()
                    {
                        Id = 1,
                        CategoryId = 1,
                        Details = new ProductDetails { Id = 1 }
                    },
                    new()
                    {
                        Id = 2,
                        CategoryId = 1,
                        Details = new ProductDetails { Id = 2 }
                    },
                    new()
                    {
                        Id = 3,
                        CategoryId = 1,
                        Details = new ProductDetails { Id = 3 }
                    }
                ]
            };

            var rootEntry = context.Attach(category);

            var graphIterator = context.GetService<IEntityEntryGraphIterator>();

            var visited = new HashSet<object>();
            var traversal = new List<string>();

            bool Callback(EntityEntryGraphNode<HashSet<object>> node)
            {
                if (node.NodeState.Contains(node.Entry.Entity))
                {
                    return false;
                }

                node.NodeState.Add(node.Entry.Entity);

                traversal.Add(NodeString(node));

                return true;
            }

            if (async)
            {
                await graphIterator.TraverseGraphAsync(
                    new EntityEntryGraphNode<HashSet<object>>(rootEntry, visited, null, null),
                    (node, _) => Task.FromResult(Callback(node)));
            }
            else
            {
                graphIterator.TraverseGraph(
                    new EntityEntryGraphNode<HashSet<object>>(rootEntry, visited, null, null),
                    Callback);
            }

            Assert.Equal(
                [
                    "<None> -----> Category:1",
                    "Category:1 ---Products--> Product:1",
                    "Product:1 ---Details--> ProductDetails:1",
                    "Category:1 ---Products--> Product:2",
                    "Product:2 ---Details--> ProductDetails:2",
                    "Category:1 ---Products--> Product:3",
                    "Product:3 ---Details--> ProductDetails:3"
                ],
                traversal);

            Assert.Equal(7, visited.Count);
        }
    }

    public class TrackGraphTestWithState : TrackGraphTestBase
    {
        protected override IList<string> TrackGraph(DbContext context, object root, Action<EntityEntryGraphNode> callback)
        {
            var traversal = new List<string>();

            context.ChangeTracker.TrackGraph<EntityState>(
                root,
                default,
                node =>
                {
                    if (node.Entry.State != EntityState.Detached)
                    {
                        return false;
                    }

                    callback(node);

                    traversal.Add(NodeString(node));

                    return node.Entry.State != EntityState.Detached;
                });

            return traversal;
        }
    }

    protected abstract IList<string> TrackGraph(
        DbContext context,
        object root,
        Action<EntityEntryGraphNode> callback);

    private static string NodeString(EntityEntryGraphNode node)
        => EntryString(node.SourceEntry)
            + " ---"
            + node.InboundNavigation?.Name
            + "--> "
            + EntryString(node.Entry);

    private static string EntryString(EntityEntry entry)
        => entry == null
            ? "<None>"
            : entry.Metadata.DisplayName()
            + ":"
            + entry.Property(entry.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue;

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Can_attach_nullable_PK_parent_with_child_collection(bool useAttach, bool setKeys)
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var category = new NullbileCategory
        {
            Products =
            [
                new(),
                new(),
                new()
            ]
        };

        if (setKeys)
        {
            context.Entry(category).Property("Id").CurrentValue = 1;
            context.Entry(category.Products[0]).Property("Id").CurrentValue = 1;
            context.Entry(category.Products[1]).Property("Id").CurrentValue = 2;
            context.Entry(category.Products[2]).Property("Id").CurrentValue = 3;
        }

        if (useAttach)
        {
            context.Attach(category);
        }
        else
        {
            Assert.Equal(
                new List<string>
                {
                    "<None> -----> NullbileCategory:1",
                    "NullbileCategory:1 ---Products--> NullbileProduct:1",
                    "NullbileCategory:1 ---Products--> NullbileProduct:2",
                    "NullbileCategory:1 ---Products--> NullbileProduct:3"
                },
                TrackGraph(
                    context,
                    category, node => node.Entry.State = node.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added));
        }

        Assert.Equal(!setKeys, context.ChangeTracker.HasChanges());

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        var categoryEntry = context.Entry(category);
        var product0Entry = context.Entry(category.Products[0]);
        var product1Entry = context.Entry(category.Products[1]);
        var product2Entry = context.Entry(category.Products[2]);

        var expectedState = setKeys ? EntityState.Unchanged : EntityState.Added;
        Assert.Equal(expectedState, categoryEntry.State);
        Assert.Equal(expectedState, product0Entry.State);
        Assert.Equal(expectedState, product1Entry.State);
        Assert.Equal(expectedState, product2Entry.State);

        Assert.Same(category, category.Products[0].Category);
        Assert.Same(category, category.Products[1].Category);
        Assert.Same(category, category.Products[2].Category);

        var categoryId = categoryEntry.Property("Id").CurrentValue;
        Assert.NotNull(categoryId);

        Assert.Equal(categoryId, product0Entry.Property("CategoryId").CurrentValue);
        Assert.Equal(categoryId, product1Entry.Property("CategoryId").CurrentValue);
        Assert.Equal(categoryId, product2Entry.Property("CategoryId").CurrentValue);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Can_attach_nullable_PK_parent_with_one_to_one_children(bool useAttach, bool setKeys)
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var category = new NullbileCategory { Info = new NullbileCategoryInfo() };

        if (setKeys)
        {
            context.Entry(category).Property("Id").CurrentValue = 1;
            context.Entry(category.Info).Property("Id").CurrentValue = 1;
        }

        if (useAttach)
        {
            context.Attach(category);
        }
        else
        {
            Assert.Equal(
                new List<string> { "<None> -----> NullbileCategory:1", "NullbileCategory:1 ---Info--> NullbileCategoryInfo:1" },
                TrackGraph(
                    context,
                    category, node => node.Entry.State = node.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added));
        }

        Assert.Equal(!setKeys, context.ChangeTracker.HasChanges());

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var expectedState = setKeys ? EntityState.Unchanged : EntityState.Added;
        Assert.Equal(expectedState, context.Entry(category).State);
        Assert.Equal(expectedState, context.Entry(category.Info).State);

        Assert.Same(category, category.Info.Category);
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void Can_attach_parent_with_owned_dependent(bool useAttach, bool setPrincipalKey, bool setDependentKey)
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var sweet = new Sweet { Dreams = new Dreams { Are = new AreMade(), Made = new AreMade() } };

        if (setPrincipalKey)
        {
            sweet.Id = 1;
        }

        if (setDependentKey)
        {
            var dreamsEntry = context.Entry(sweet).Reference(e => e.Dreams).TargetEntry;
            dreamsEntry.Property("SweetId").CurrentValue = 1;
            dreamsEntry.Reference(e => e.Are).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
            dreamsEntry.Reference(e => e.Made).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
        }

        if (useAttach)
        {
            context.Attach(sweet);
        }
        else
        {
            Assert.Equal(
                new List<string>
                {
                    "<None> -----> Sweet:1",
                    "Sweet:1 ---Dreams--> Dreams:1",
                    "Dreams:1 ---Are--> Dreams.Are#AreMade:1",
                    "Dreams:1 ---Made--> Dreams.Made#AreMade:1"
                },
                TrackGraph(
                    context,
                    sweet,
                    node => node.Entry.State = node.Entry.Metadata.IsOwned()
                        ? node.SourceEntry.State
                        : node.Entry.IsKeySet
                            ? EntityState.Unchanged
                            : EntityState.Added));
        }

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        var dependentEntry = context.Entry(sweet.Dreams);
        var dependentEntry2a = context.Entry(sweet.Dreams.Are);
        var dependentEntry2b = context.Entry(sweet.Dreams.Made);

        var expectedPrincipalState = setPrincipalKey ? EntityState.Unchanged : EntityState.Added;
        var expectedDependentState = setPrincipalKey || (setDependentKey && useAttach) ? EntityState.Unchanged : EntityState.Added;

        Assert.Equal(
            expectedPrincipalState == EntityState.Added || expectedDependentState == EntityState.Added,
            context.ChangeTracker.HasChanges());

        Assert.Equal(expectedPrincipalState, context.Entry(sweet).State);
        Assert.Equal(expectedDependentState, dependentEntry.State);
        Assert.Equal(expectedDependentState, dependentEntry2a.State);
        Assert.Equal(expectedDependentState, dependentEntry2b.State);

        Assert.Equal(1, sweet.Id);
        Assert.Equal(1, dependentEntry.Property(dependentEntry.Metadata.FindPrimaryKey().Properties[0]).CurrentValue);
        Assert.Equal(1, dependentEntry2a.Property(dependentEntry2a.Metadata.FindPrimaryKey().Properties[0]).CurrentValue);
        Assert.Equal(1, dependentEntry2b.Property(dependentEntry2b.Metadata.FindPrimaryKey().Properties[0]).CurrentValue);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Can_attach_owned_dependent_with_reference_to_parent(bool useAttach, bool setDependentKey)
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var dreams = new Dreams
        {
            Sweet = new Sweet { Id = 1 },
            Are = new AreMade(),
            Made = new AreMade()
        };

        if (setDependentKey)
        {
            var dreamsEntry = context.Entry(dreams);
            dreamsEntry.Property("SweetId").CurrentValue = 1;
            dreamsEntry.Reference(e => e.Are).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
            dreamsEntry.Reference(e => e.Made).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
        }

        if (useAttach)
        {
            context.Attach(dreams);
        }
        else
        {
            Assert.Equal(
                new List<string>
                {
                    "<None> -----> Dreams:1",
                    "Dreams:1 ---Are--> Dreams.Are#AreMade:1",
                    "Dreams:1 ---Made--> Dreams.Made#AreMade:1",
                    "Dreams:1 ---Sweet--> Sweet:1"
                },
                TrackGraph(
                    context,
                    dreams,
                    node => node.Entry.State = node.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added));
        }

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        Assert.Equal(!setDependentKey, context.ChangeTracker.HasChanges());

        var dependentEntry = context.Entry(dreams);
        var dependentEntry2a = context.Entry(dreams.Are);
        var dependentEntry2b = context.Entry(dreams.Made);

        var expectedPrincipalState = EntityState.Unchanged;
        var expectedDependentState = setDependentKey ? EntityState.Unchanged : EntityState.Added;

        Assert.Equal(expectedPrincipalState, context.Entry(dreams.Sweet).State);
        Assert.Equal(expectedDependentState, dependentEntry.State);
        Assert.Equal(expectedDependentState, dependentEntry2a.State);
        Assert.Equal(expectedDependentState, dependentEntry2b.State);

        Assert.Equal(1, dreams.Sweet.Id);
        Assert.Equal(1, dependentEntry.Property(dependentEntry.Metadata.FindPrimaryKey().Properties[0]).CurrentValue);
        Assert.Equal(1, dependentEntry2a.Property(dependentEntry2a.Metadata.FindPrimaryKey().Properties[0]).CurrentValue);
        Assert.Equal(1, dependentEntry2b.Property(dependentEntry2b.Metadata.FindPrimaryKey().Properties[0]).CurrentValue);
    }

    [ConditionalFact]
    public void Can_attach_parent_with_child_collection()
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var category = new Category
        {
            Id = 1,
            Products =
            [
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 }
            ]
        };

        Assert.Equal(
            new List<string>
            {
                "<None> -----> Category:1",
                "Category:1 ---Products--> Product:1",
                "Category:1 ---Products--> Product:2",
                "Category:1 ---Products--> Product:3"
            },
            TrackGraph(
                context,
                category,
                node => node.Entry.State = EntityState.Modified));

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(EntityState.Modified, context.Entry(category).State);
        Assert.Equal(EntityState.Modified, context.Entry(category.Products[0]).State);
        Assert.Equal(EntityState.Modified, context.Entry(category.Products[1]).State);
        Assert.Equal(EntityState.Modified, context.Entry(category.Products[2]).State);

        Assert.Same(category, category.Products[0].Category);
        Assert.Same(category, category.Products[1].Category);
        Assert.Same(category, category.Products[2].Category);

        Assert.Equal(category.Id, category.Products[0].CategoryId);
        Assert.Equal(category.Id, category.Products[1].CategoryId);
        Assert.Equal(category.Id, category.Products[2].CategoryId);
    }

    [ConditionalFact]
    public void Can_attach_child_with_reference_to_parent()
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var product = new Product { Id = 1, Category = new Category { Id = 1 } };

        Assert.Equal(
            new List<string> { "<None> -----> Product:1", "Product:1 ---Category--> Category:1" },
            TrackGraph(
                context,
                product,
                node => node.Entry.State = EntityState.Modified));

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(EntityState.Modified, context.Entry(product).State);
        Assert.Equal(EntityState.Modified, context.Entry(product.Category).State);

        Assert.Same(product, product.Category.Products[0]);
        Assert.Equal(product.Category.Id, product.CategoryId);
    }

    [ConditionalFact]
    public void Can_attach_parent_with_one_to_one_children()
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var product = new Product { Id = 1, Details = new ProductDetails { Id = 1, Tag = new ProductDetailsTag { Id = 1 } } };

        Assert.Equal(
            new List<string>
            {
                "<None> -----> Product:1",
                "Product:1 ---Details--> ProductDetails:1",
                "ProductDetails:1 ---Tag--> ProductDetailsTag:1"
            },
            TrackGraph(
                context,
                product,
                node => node.Entry.State = EntityState.Unchanged));

        Assert.Equal(3, context.ChangeTracker.Entries().Count());

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product.Details).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(product.Details.Tag).State);

        Assert.Same(product, product.Details.Product);
        Assert.Same(product.Details, product.Details.Tag.Details);
    }

    [ConditionalFact]
    public void Can_attach_child_with_one_to_one_parents()
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var tag = new ProductDetailsTag { Id = 1, Details = new ProductDetails { Id = 1, Product = new Product { Id = 1 } } };

        Assert.Equal(
            new List<string>
            {
                "<None> -----> ProductDetailsTag:1",
                "ProductDetailsTag:1 ---Details--> ProductDetails:1",
                "ProductDetails:1 ---Product--> Product:1"
            },
            TrackGraph(
                context,
                tag,
                node => node.Entry.State = EntityState.Unchanged));

        Assert.Equal(3, context.ChangeTracker.Entries().Count());

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(EntityState.Unchanged, context.Entry(tag).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(tag.Details).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(tag.Details.Product).State);

        Assert.Same(tag, tag.Details.Tag);
        Assert.Same(tag.Details, tag.Details.Product.Details);
    }

    [ConditionalFact]
    public void Can_attach_entity_with_one_to_one_parent_and_child()
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var details = new ProductDetails
        {
            Id = 1,
            Product = new Product { Id = 1 },
            Tag = new ProductDetailsTag { Id = 1 }
        };

        Assert.Equal(
            new List<string>
            {
                "<None> -----> ProductDetails:1",
                "ProductDetails:1 ---Product--> Product:1",
                "ProductDetails:1 ---Tag--> ProductDetailsTag:1"
            },
            TrackGraph(
                context,
                details,
                node => node.Entry.State = EntityState.Unchanged));

        Assert.Equal(3, context.ChangeTracker.Entries().Count());

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(EntityState.Unchanged, context.Entry(details).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(details.Product).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(details.Tag).State);

        Assert.Same(details, details.Tag.Details);
        Assert.Same(details, details.Product.Details);
    }

    [ConditionalFact]
    public void Entities_that_are_already_tracked_will_not_get_attached()
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var existingProduct = context.Attach(
            new Product { Id = 2, CategoryId = 1 }).Entity;

        var category = new Category
        {
            Id = 1,
            Products =
            [
                new() { Id = 1 },
                existingProduct,
                new() { Id = 3 }
            ]
        };

        Assert.Equal(
            new List<string>
            {
                "<None> -----> Category:1",
                "Category:1 ---Products--> Product:1",
                "Category:1 ---Products--> Product:3"
            },
            TrackGraph(
                context,
                category,
                node => node.Entry.State = EntityState.Modified));

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(EntityState.Modified, context.Entry(category).State);
        Assert.Equal(EntityState.Modified, context.Entry(category.Products[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[1]).State);
        Assert.Equal(EntityState.Modified, context.Entry(category.Products[2]).State);

        Assert.Same(category, category.Products[0].Category);
        Assert.Same(category, category.Products[1].Category);
        Assert.Same(category, category.Products[2].Category);

        Assert.Equal(category.Id, category.Products[0].CategoryId);
        Assert.Equal(category.Id, category.Products[1].CategoryId);
        Assert.Equal(category.Id, category.Products[2].CategoryId);
    }

    [ConditionalFact]
    public void Further_graph_traversal_stops_if_an_entity_is_not_attached()
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var category = new Category
        {
            Id = 1,
            Products =
            [
                new()
                {
                    Id = 1,
                    CategoryId = 1,
                    Details = new ProductDetails { Id = 1 }
                },
                new()
                {
                    Id = 2,
                    CategoryId = 1,
                    Details = new ProductDetails { Id = 2 }
                },
                new()
                {
                    Id = 3,
                    CategoryId = 1,
                    Details = new ProductDetails { Id = 3 }
                }
            ]
        };

        Assert.Equal(
            new List<string>
            {
                "<None> -----> Category:1",
                "Category:1 ---Products--> Product:1",
                "Product:1 ---Details--> ProductDetails:1",
                "Category:1 ---Products--> Product:2",
                "Category:1 ---Products--> Product:3",
                "Product:3 ---Details--> ProductDetails:3"
            },
            TrackGraph(
                context,
                category,
                node =>
                {
                    if (!(node.Entry.Entity is Product product)
                        || product.Id != 2)
                    {
                        node.Entry.State = EntityState.Unchanged;
                    }
                }));

        Assert.Equal(5, context.ChangeTracker.Entries().Count(e => e.State != EntityState.Detached));

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0].Details).State);
        Assert.Equal(EntityState.Detached, context.Entry(category.Products[1]).State);
        Assert.Equal(EntityState.Detached, context.Entry(category.Products[1].Details).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2].Details).State);

        Assert.Same(category, category.Products[0].Category);
        Assert.Null(category.Products[1].Category);
        Assert.Same(category, category.Products[2].Category);

        Assert.Equal(category.Id, category.Products[0].CategoryId);
        Assert.Equal(category.Id, category.Products[1].CategoryId);
        Assert.Equal(category.Id, category.Products[2].CategoryId);

        Assert.Same(category.Products[0], category.Products[0].Details.Product);
        Assert.Null(category.Products[1].Details.Product);
        Assert.Same(category.Products[2], category.Products[2].Details.Product);
    }

    [ConditionalFact]
    public void Graph_iterator_does_not_go_visit_Apple()
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var details = new ProductDetails { Id = 1, Product = new Product { Id = 1 } };
        details.Product.Details = details;

        Assert.Equal(
            new List<string> { "<None> -----> ProductDetails:1" },
            TrackGraph(
                context,
                details,
                e => { }));

        Assert.Equal(0, context.ChangeTracker.Entries().Count(e => e.State != EntityState.Detached));

        Assert.False(context.ChangeTracker.HasChanges());
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Can_add_owned_dependent_with_reference_to_parent(bool useAdd, bool setDependentKey)
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var dreams = new Dreams
        {
            Sweet = new Sweet { Id = 1 },
            Are = new AreMade(),
            Made = new AreMade()
        };

        context.Entry(dreams.Sweet).State = EntityState.Unchanged;

        if (setDependentKey)
        {
            var dreamsEntry = context.Entry(dreams);
            dreamsEntry.Property("SweetId").CurrentValue = 1;
            dreamsEntry.Reference(e => e.Are).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
            dreamsEntry.Reference(e => e.Made).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
        }

        if (useAdd)
        {
            context.Add(dreams);
        }
        else
        {
            Assert.Equal(
                new List<string>
                {
                    "<None> -----> Dreams:1",
                    "Dreams:1 ---Are--> Dreams.Are#AreMade:1",
                    "Dreams:1 ---Made--> Dreams.Made#AreMade:1"
                },
                TrackGraph(
                    context,
                    dreams,
                    node => node.Entry.State = node.Entry.IsKeySet && !node.Entry.Metadata.IsOwned()
                        ? EntityState.Unchanged
                        : EntityState.Added));
        }

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        Assert.True(context.ChangeTracker.HasChanges());

        var dependentEntry = context.Entry(dreams);
        var dependentEntry2a = context.Entry(dreams.Are);
        var dependentEntry2b = context.Entry(dreams.Made);

        var expectedPrincipalState = EntityState.Unchanged;
        var expectedDependentState = EntityState.Added;

        Assert.Equal(expectedPrincipalState, context.Entry(dreams.Sweet).State);
        Assert.Equal(expectedDependentState, dependentEntry.State);
        Assert.Equal(expectedDependentState, dependentEntry2a.State);
        Assert.Equal(expectedDependentState, dependentEntry2b.State);

        Assert.Equal(1, dreams.Sweet.Id);
        Assert.Equal(1, dependentEntry.Property(dependentEntry.Metadata.FindPrimaryKey().Properties[0]).CurrentValue);
        Assert.Equal(1, dependentEntry2a.Property(dependentEntry2a.Metadata.FindPrimaryKey().Properties[0]).CurrentValue);
        Assert.Equal(1, dependentEntry2b.Property(dependentEntry2b.Metadata.FindPrimaryKey().Properties[0]).CurrentValue);
    }

    [ConditionalTheory] // Issue #12590
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dependents_are_detached_not_deleted_when_principal_is_detached(bool delayCascade, bool trackNewDependents)
    {
        using var context = new EarlyLearningCenter(GetType().Name);

        var category = new Category
        {
            Id = 1,
            Products =
            [
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 }
            ]
        };

        context.Attach(category);

        Assert.False(context.ChangeTracker.HasChanges());

        var categoryEntry = context.Entry(category);
        var product0Entry = context.Entry(category.Products[0]);
        var product1Entry = context.Entry(category.Products[1]);
        var product2Entry = context.Entry(category.Products[2]);

        Assert.Equal(EntityState.Unchanged, categoryEntry.State);
        Assert.Equal(EntityState.Unchanged, product0Entry.State);
        Assert.Equal(EntityState.Unchanged, product1Entry.State);
        Assert.Equal(EntityState.Unchanged, product2Entry.State);

        if (delayCascade)
        {
            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
        }

        context.Entry(category).State = EntityState.Detached;

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(EntityState.Detached, categoryEntry.State);

        if (delayCascade)
        {
            Assert.Equal(EntityState.Unchanged, product0Entry.State);
            Assert.Equal(EntityState.Unchanged, product1Entry.State);
            Assert.Equal(EntityState.Unchanged, product2Entry.State);
        }
        else
        {
            Assert.Equal(EntityState.Detached, product0Entry.State);
            Assert.Equal(EntityState.Detached, product1Entry.State);
            Assert.Equal(EntityState.Detached, product2Entry.State);
        }

        var newCategory = new Category { Id = 1, };

        if (trackNewDependents)
        {
            newCategory.Products =
            [
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 }
            ];
        }

        if (delayCascade && trackNewDependents)
        {
            Assert.Equal(
                CoreStrings.IdentityConflict(nameof(Product), "{'Id'}"),
                Assert.Throws<InvalidOperationException>(TrackGraph).Message);
        }
        else
        {
            Assert.Equal(
                trackNewDependents
                    ?
                    [
                        "<None> -----> Category:1",
                        "Category:1 ---Products--> Product:1",
                        "Category:1 ---Products--> Product:2",
                        "Category:1 ---Products--> Product:3"
                    ]
                    : new List<string> { "<None> -----> Category:1" },
                TrackGraph());

            if (trackNewDependents || delayCascade)
            {
                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                Assert.True(context.ChangeTracker.HasChanges());

                categoryEntry = context.Entry(newCategory);
                product0Entry = context.Entry(newCategory.Products[0]);
                product1Entry = context.Entry(newCategory.Products[1]);
                product2Entry = context.Entry(newCategory.Products[2]);

                Assert.Equal(EntityState.Modified, categoryEntry.State);

                if (trackNewDependents)
                {
                    Assert.Equal(EntityState.Modified, product0Entry.State);
                    Assert.Equal(EntityState.Modified, product1Entry.State);
                    Assert.Equal(EntityState.Modified, product2Entry.State);

                    Assert.NotSame(newCategory.Products[0], category.Products[0]);
                    Assert.NotSame(newCategory.Products[1], category.Products[1]);
                    Assert.NotSame(newCategory.Products[2], category.Products[2]);
                }
                else
                {
                    Assert.Equal(EntityState.Unchanged, product0Entry.State);
                    Assert.Equal(EntityState.Unchanged, product1Entry.State);
                    Assert.Equal(EntityState.Unchanged, product2Entry.State);

                    Assert.Same(newCategory.Products[0], category.Products[0]);
                    Assert.Same(newCategory.Products[1], category.Products[1]);
                    Assert.Same(newCategory.Products[2], category.Products[2]);
                }

                Assert.Same(newCategory, newCategory.Products[0].Category);
                Assert.Same(newCategory, newCategory.Products[1].Category);
                Assert.Same(newCategory, newCategory.Products[2].Category);

                Assert.Equal(newCategory.Id, product0Entry.Property("CategoryId").CurrentValue);
                Assert.Equal(newCategory.Id, product1Entry.Property("CategoryId").CurrentValue);
                Assert.Equal(newCategory.Id, product2Entry.Property("CategoryId").CurrentValue);
            }
            else
            {
                Assert.Single(context.ChangeTracker.Entries());

                categoryEntry = context.Entry(newCategory);

                Assert.Equal(EntityState.Modified, categoryEntry.State);
                Assert.Null(newCategory.Products);
            }
        }

        IList<string> TrackGraph()
            => this.TrackGraph(
                context,
                newCategory,
                node => node.Entry.State = EntityState.Modified);
    }

    [ConditionalFact]
    public void TrackGraph_overload_can_visit_a_graph_without_attaching()
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var category = new Category
        {
            Id = 1,
            Products =
            [
                new()
                {
                    Id = 1,
                    CategoryId = 1,
                    Details = new ProductDetails { Id = 1 }
                },
                new()
                {
                    Id = 2,
                    CategoryId = 1,
                    Details = new ProductDetails { Id = 2 }
                },
                new()
                {
                    Id = 3,
                    CategoryId = 1,
                    Details = new ProductDetails { Id = 3 }
                }
            ]
        };

        var visited = new HashSet<object>();
        var traversal = new List<string>();

        context.ChangeTracker.TrackGraph(
            category,
            visited,
            node =>
            {
                if (node.NodeState.Contains(node.Entry.Entity))
                {
                    return false;
                }

                node.NodeState.Add(node.Entry.Entity);

                traversal.Add(NodeString(node));

                return true;
            });

        Assert.Equal(
            [
                "<None> -----> Category:1",
                "Category:1 ---Products--> Product:1",
                "Product:1 ---Details--> ProductDetails:1",
                "Category:1 ---Products--> Product:2",
                "Product:2 ---Details--> ProductDetails:2",
                "Category:1 ---Products--> Product:3",
                "Product:3 ---Details--> ProductDetails:3"
            ],
            traversal);

        Assert.Equal(7, visited.Count);

        Assert.False(context.ChangeTracker.HasChanges());

        foreach (var entity in new object[] { category }
                     .Concat(category.Products)
                     .Concat(category.Products.Select(e => e.Details)))
        {
            Assert.Equal(EntityState.Detached, context.Entry(entity).State);
        }
    }

    [ConditionalFact]
    public void Can_attach_parent_with_some_new_and_some_existing_entities()
        => KeyValueAttachTest(
            GetType().Name,
            (category, changeTracker) =>
            {
                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> Category:77",
                        "Category:77 ---Products--> Product:77",
                        "Category:77 ---Products--> Product:1",
                        "Category:77 ---Products--> Product:78"
                    },
                    TrackGraph(
                        changeTracker.Context,
                        category,
                        node => node.Entry.State = node.Entry.Entity is Product { Id: 0 }
                            ? EntityState.Added
                            : EntityState.Unchanged));
            });

    [ConditionalFact]
    public void Can_attach_graph_using_built_in_tracker()
    {
        var tracker = new KeyValueEntityTracker(updateExistingEntities: false);

        KeyValueAttachTest(
            GetType().Name,
            (category, changeTracker) => changeTracker.TrackGraph(category, tracker.TrackEntity));
    }

    [ConditionalFact]
    public void Can_update_graph_using_built_in_tracker()
    {
        var tracker = new KeyValueEntityTracker(updateExistingEntities: true);

        KeyValueAttachTest(
            GetType().Name,
            (category, changeTracker) => changeTracker.TrackGraph(category, tracker.TrackEntity),
            expectModified: true);
    }

    private static void KeyValueAttachTest(string databaseName, Action<Category, ChangeTracker> tracker, bool expectModified = false)
    {
        using var context = new EarlyLearningCenter(databaseName);
        var category = new Category
        {
            Id = 77,
            Products =
            [
                new() { Id = 77, CategoryId = expectModified ? 0 : 77 },
                new() { Id = 0, CategoryId = expectModified ? 0 : 77 },
                new() { Id = 78, CategoryId = expectModified ? 0 : 77 }
            ]
        };

        tracker(category, context.ChangeTracker);

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        var nonAddedState = expectModified ? EntityState.Modified : EntityState.Unchanged;

        Assert.Equal(nonAddedState, context.Entry(category).State);
        Assert.Equal(nonAddedState, context.Entry(category.Products[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(category.Products[1]).State);
        Assert.Equal(nonAddedState, context.Entry(category.Products[2]).State);

        Assert.Equal(77, category.Products[0].Id);
        Assert.Equal(1, category.Products[1].Id);
        Assert.Equal(78, category.Products[2].Id);

        Assert.Same(category, category.Products[0].Category);
        Assert.Same(category, category.Products[1].Category);
        Assert.Same(category, category.Products[2].Category);

        Assert.Equal(category.Id, category.Products[0].CategoryId);
        Assert.Equal(category.Id, category.Products[1].CategoryId);
        Assert.Equal(category.Id, category.Products[2].CategoryId);
    }

    [ConditionalFact]
    public void Can_attach_graph_using_custom_delegate()
    {
        var tracker = new MyTracker(updateExistingEntities: false);

        using var context = new EarlyLearningCenter(GetType().Name);
        var category = new Category
        {
            Id = 77,
            Products =
            [
                new() { Id = 77, CategoryId = 77 },
                new() { Id = 0, CategoryId = 77 },
                new() { Id = 78, CategoryId = 77 }
            ]
        };

        context.ChangeTracker.TrackGraph(category, tracker.TrackEntity);

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(category.Products[1]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2]).State);

        Assert.Equal(77, category.Products[0].Id);
        Assert.Equal(777, category.Products[1].Id);
        Assert.Equal(78, category.Products[2].Id);

        Assert.Same(category, category.Products[0].Category);
        Assert.Same(category, category.Products[1].Category);
        Assert.Same(category, category.Products[2].Category);

        Assert.Equal(category.Id, category.Products[0].CategoryId);
        Assert.Equal(category.Id, category.Products[1].CategoryId);
        Assert.Equal(category.Id, category.Products[2].CategoryId);
    }

    private class MyTracker(bool updateExistingEntities) : KeyValueEntityTracker(updateExistingEntities)
    {
        public override EntityState DetermineState(EntityEntry entry)
        {
            if (!entry.IsKeySet)
            {
                entry.GetInfrastructure()[entry.Metadata.FindPrimaryKey().Properties.Single()] = 777;
                return EntityState.Added;
            }

            return base.DetermineState(entry);
        }
    }

    [ConditionalFact]
    public void TrackGraph_does_not_call_DetectChanges()
    {
        var provider =
            InMemoryTestHelpers.Instance.CreateServiceProvider(
                new ServiceCollection().AddScoped<IChangeDetector, ChangeDetectorProxy>());
        using var context = new EarlyLearningCenter(GetType().Name, provider);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        changeDetector.DetectChangesCalled = false;

        context.ChangeTracker.TrackGraph(CreateSimpleGraph(2), e => e.Entry.State = EntityState.Unchanged);

        Assert.False(changeDetector.DetectChangesCalled);

        context.ChangeTracker.DetectChanges();

        Assert.True(changeDetector.DetectChangesCalled);
    }

    [ConditionalFact]
    public void TrackGraph_overload_can_visit_an_already_attached_graph()
    {
        using var context = new EarlyLearningCenter(GetType().Name);
        var category = new Category
        {
            Id = 1,
            Products =
            [
                new()
                {
                    Id = 1,
                    CategoryId = 1,
                    Details = new ProductDetails { Id = 1 }
                },
                new()
                {
                    Id = 2,
                    CategoryId = 1,
                    Details = new ProductDetails { Id = 2 }
                },
                new()
                {
                    Id = 3,
                    CategoryId = 1,
                    Details = new ProductDetails { Id = 3 }
                }
            ]
        };

        context.Attach(category);

        var visited = new HashSet<object>();
        var traversal = new List<string>();

        context.ChangeTracker.TrackGraph(
            category, visited, e =>
            {
                if (e.NodeState.Contains(e.Entry.Entity))
                {
                    return false;
                }

                e.NodeState.Add(e.Entry.Entity);

                traversal.Add(NodeString(e));

                return true;
            });

        Assert.Equal(
            [
                "<None> -----> Category:1",
                "Category:1 ---Products--> Product:1",
                "Product:1 ---Details--> ProductDetails:1",
                "Category:1 ---Products--> Product:2",
                "Product:2 ---Details--> ProductDetails:2",
                "Category:1 ---Products--> Product:3",
                "Product:3 ---Details--> ProductDetails:3"
            ],
            traversal);

        Assert.Equal(7, visited.Count);
    }

    private class TheShadows : DbContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Dark>(
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd();
                    b.Property<int>("SomeInt");
                    b.Property<string>("SomeString");
                });

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(TheShadows));
    }

    private class Dark;

    private static Product CreateSimpleGraph(int id)
        => new() { Id = id, Category = new Category { Id = id } };

    private class ChangeDetectorProxy(
        IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
        ILoggingOptions loggingOptions) : ChangeDetector(logger, loggingOptions)
    {
        public bool DetectChangesCalled { get; set; }

        public override void DetectChanges(InternalEntityEntry entry)
        {
            DetectChangesCalled = true;

            base.DetectChanges(entry);
        }

        public override void DetectChanges(IStateManager stateManager)
        {
            DetectChangesCalled = true;

            base.DetectChanges(stateManager);
        }
    }

    private class Category
    {
        public int Id { get; set; }

        public List<Product> Products { get; set; }
    }

    private class Product
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ProductDetails Details { get; set; }

        // ReSharper disable once CollectionNeverUpdated.Local
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public List<OrderDetails> OrderDetails { get; set; }
    }

    private class ProductDetails
    {
        public int Id { get; set; }

        public Product Product { get; set; }

        public ProductDetailsTag Tag { get; set; }
    }

    private class ProductDetailsTag
    {
        public int Id { get; set; }

        public ProductDetails Details { get; set; }

        public ProductDetailsTagDetails TagDetails { get; set; }
    }

    private class ProductDetailsTagDetails
    {
        public int Id { get; }

        public ProductDetailsTag Tag { get; }
    }

    private class Order
    {
        public int Id { get; set; }

        // ReSharper disable once CollectionNeverUpdated.Local
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public List<OrderDetails> OrderDetails { get; }
    }

    private class OrderDetails
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public Order Order { get; set; }
        public Product Product { get; set; }
    }

    private class NullbileCategory
    {
        public List<NullbileProduct> Products { get; set; }
        public NullbileCategoryInfo Info { get; set; }
    }

    private class NullbileCategoryInfo
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public NullbileCategory Category { get; set; }
    }

    private class NullbileProduct
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public NullbileCategory Category { get; set; }
    }

    private class Sweet
    {
        public int? Id { get; set; }
        public Dreams Dreams { get; set; }
    }

    private class Dreams
    {
        public Sweet Sweet { get; set; }
        public AreMade Are { get; set; }
        public AreMade Made { get; set; }
        public OfThis OfThis { get; }
    }

    private class AreMade;

    private class OfThis : AreMade;

    private class EarlyLearningCenter : DbContext
    {
        private readonly string _databaseName;
        private readonly IServiceProvider _serviceProvider;

        public EarlyLearningCenter(string databaseName)
        {
            _databaseName = databaseName;
            _serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
        }

        public EarlyLearningCenter(string databaseName, IServiceProvider serviceProvider)
        {
            _databaseName = databaseName;
            _serviceProvider = serviceProvider;
        }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<NullbileProduct>(
                    b =>
                    {
                        b.Property<int?>("Id");
                        b.Property<int?>("CategoryId");
                        b.HasKey("Id");
                    });

            modelBuilder
                .Entity<NullbileCategoryInfo>(
                    b =>
                    {
                        b.Property<int?>("Id");
                        b.Property<int?>("CategoryId");
                        b.HasKey("Id");
                    });

            modelBuilder
                .Entity<NullbileCategory>(
                    b =>
                    {
                        b.Property<int?>("Id");
                        b.HasKey("Id");
                        b.HasMany(e => e.Products).WithOne(e => e.Category).HasForeignKey("CategoryId");
                        b.HasOne(e => e.Info).WithOne(e => e.Category).HasForeignKey<NullbileCategoryInfo>("CategoryId");
                    });

            modelBuilder.Entity<Sweet>().OwnsOne(
                e => e.Dreams, b =>
                {
                    b.WithOwner(e => e.Sweet);
                    b.OwnsOne(e => e.Are);
                    b.OwnsOne(e => e.Made);
                    b.OwnsOne(e => e.OfThis);
                });

            modelBuilder
                .Entity<Category>().HasMany(e => e.Products).WithOne(e => e.Category);

            modelBuilder
                .Entity<ProductDetailsTag>().HasOne(e => e.TagDetails).WithOne(e => e.Tag)
                .HasForeignKey<ProductDetailsTagDetails>(e => e.Id);

            modelBuilder
                .Entity<ProductDetails>().HasOne(e => e.Tag).WithOne(e => e.Details)
                .HasForeignKey<ProductDetailsTag>(e => e.Id);

            modelBuilder
                .Entity<Product>().HasOne(e => e.Details).WithOne(e => e.Product)
                .HasForeignKey<ProductDetails>(e => e.Id);

            modelBuilder.Entity<OrderDetails>(
                b =>
                {
                    b.HasKey(
                        e => new { e.OrderId, e.ProductId });
                    b.HasOne(e => e.Order).WithMany(e => e.OrderDetails).HasForeignKey(e => e.OrderId);
                    b.HasOne(e => e.Product).WithMany(e => e.OrderDetails).HasForeignKey(e => e.ProductId);
                });
        }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(_databaseName);
    }

    public class KeyValueEntityTracker(bool updateExistingEntities)
    {
        private readonly bool _updateExistingEntities = updateExistingEntities;

        public virtual void TrackEntity(EntityEntryGraphNode node)
            => node.Entry.GetInfrastructure().SetEntityState(DetermineState(node.Entry), acceptChanges: true);

        public virtual EntityState DetermineState(EntityEntry entry)
            => entry.IsKeySet
                ? (_updateExistingEntities ? EntityState.Modified : EntityState.Unchanged)
                : EntityState.Added;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class InternalComplexEntryTest
{
    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Deleted)]
    public void Complex_entry_can_change_state(EntityState entityState)
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();

        var blog = new Blog
        {
            Tags =
            [
                new Tag { Name = "Tag0" },
                new Tag { Name = "Tag1" },
                new Tag { Name = "Tag2" }
            ]
        };
        var entityEntry = stateManager.GetOrCreateEntry(blog);

        Assert.All(entityEntry.GetComplexCollectionEntries(complexProperty), Assert.Null);
        Assert.All(entityEntry.GetComplexCollectionOriginalEntries(complexProperty), Assert.Null);

        entityEntry.SetEntityState(entityState);
        var complexEntry = entityState == EntityState.Deleted
            ? entityEntry.GetComplexCollectionOriginalEntry(complexProperty, 1)
            : entityEntry.GetComplexCollectionEntry(complexProperty, 1);

        var _ = complexEntry.ToString();

        if (entityState == EntityState.Added)
        {
            Assert.Equal(-1, complexEntry.OriginalOrdinal);
            Assert.All(entityEntry.GetComplexCollectionOriginalEntries(complexProperty), Assert.Null);
        }
        else
        {
            Assert.Equal(1, complexEntry.OriginalOrdinal);
            Assert.Same(complexEntry, entityEntry.GetComplexCollectionOriginalEntries(complexProperty)[1]);
        }

        if (entityState == EntityState.Deleted)
        {
            Assert.Equal(-1, complexEntry.Ordinal);
            Assert.Equal([-1], complexEntry.GetOrdinals());
            Assert.All(entityEntry.GetComplexCollectionEntries(complexProperty), Assert.Null);
        }
        else
        {
            Assert.Equal(1, complexEntry.Ordinal);
            Assert.Equal([1], complexEntry.GetOrdinals());
            Assert.Same(complexEntry, entityEntry.GetComplexCollectionEntries(complexProperty)[1]);
        }

        Assert.Equal(entityState, complexEntry.EntityState);
        Assert.Same(entityEntry, complexEntry.ContainingEntry);
        Assert.Same(entityEntry, complexEntry.EntityEntry);
        Assert.Same(stateManager, complexEntry.StateManager);
        Assert.Same(complexProperty.ComplexType, complexEntry.ComplexType);
        Assert.Same(complexProperty, complexEntry.ComplexProperty);
        Assert.Equal(entityState != EntityState.Unchanged, entityEntry.IsModified(complexProperty));

        if (entityState == EntityState.Added)
        {
            return;
        }

        var tag = blog.Tags[1];
        blog.Tags.RemoveAt(1);
        complexEntry.SetEntityState(EntityState.Deleted);

        Assert.Equal(EntityState.Deleted, complexEntry.EntityState);
        Assert.Equal(entityState == EntityState.Unchanged ? EntityState.Modified : entityState, entityEntry.EntityState);
        Assert.Equal(-1, complexEntry.Ordinal);
        Assert.Equal(1, complexEntry.OriginalOrdinal);
        Assert.DoesNotContain(complexEntry, entityEntry.GetComplexCollectionEntries(complexProperty));
        Assert.Same(complexEntry, entityEntry.GetComplexCollectionOriginalEntries(complexProperty)[complexEntry.OriginalOrdinal]);
        Assert.Equal(entityState == EntityState.Deleted ? [-1, -1, -1] : [0, -1, 1],
            entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal(entityState == EntityState.Deleted ? [null, null, null] : [0, 2],
            entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.True(entityEntry.IsModified(complexProperty));

        if (entityState == EntityState.Deleted)
        {
            return;
        }

        blog.Tags.Insert(1, tag);
        complexEntry.Ordinal = 1;
        complexEntry.SetEntityState(EntityState.Modified);

        Assert.Equal(EntityState.Modified, complexEntry.EntityState);
        Assert.Equal(entityState == EntityState.Unchanged ? EntityState.Modified : entityState, entityEntry.EntityState);
        Assert.Equal(1, complexEntry.Ordinal);
        Assert.Equal(1, complexEntry.OriginalOrdinal);
        Assert.Same(complexEntry, entityEntry.GetComplexCollectionEntries(complexProperty)[complexEntry.Ordinal]!);
        Assert.Same(complexEntry, entityEntry.GetComplexCollectionOriginalEntries(complexProperty)[complexEntry.OriginalOrdinal]);
        Assert.Equal([0, 1, 2], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, 1, 2], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.True(entityEntry.IsModified(complexProperty));

        var originalTags = blog.Tags.ToList();
        originalTags.RemoveAt(1);
        entityEntry.SetOriginalValue(complexProperty, originalTags);
        complexEntry.SetEntityState(EntityState.Added);

        Assert.Equal(EntityState.Added, complexEntry.EntityState);
        Assert.Equal(entityState == EntityState.Unchanged ? EntityState.Modified : entityState, entityEntry.EntityState);
        Assert.Equal(1, complexEntry.Ordinal);
        Assert.Equal(-1, complexEntry.OriginalOrdinal);
        Assert.Same(complexEntry, entityEntry.GetComplexCollectionEntries(complexProperty)[complexEntry.Ordinal]!);
        Assert.DoesNotContain(complexEntry, entityEntry.GetComplexCollectionOriginalEntries(complexProperty));
        Assert.Equal([0, 2], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, -1, 1], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.True(entityEntry.IsModified(complexProperty));

        entityEntry.SetOriginalValue(complexProperty, blog.Tags.ToList());
        complexEntry.OriginalOrdinal = 1;
        complexEntry.SetEntityState(EntityState.Unchanged);

        Assert.Equal(EntityState.Unchanged, complexEntry.EntityState);
        Assert.Equal(entityState, entityEntry.EntityState);
        Assert.Equal(1, complexEntry.Ordinal);
        Assert.Equal(1, complexEntry.OriginalOrdinal);
        Assert.Same(complexEntry, entityEntry.GetComplexCollectionEntries(complexProperty)[complexEntry.Ordinal]!);
        Assert.Same(complexEntry, entityEntry.GetComplexCollectionOriginalEntries(complexProperty)[complexEntry.OriginalOrdinal]);
        Assert.Equal([0, 1, 2], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, 1, 2], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.Equal(entityState == EntityState.Modified, entityEntry.IsModified(complexProperty));

        complexEntry.SetEntityState(EntityState.Detached);

        Assert.Equal(EntityState.Detached, complexEntry.EntityState);
        Assert.Equal(entityState, entityEntry.EntityState);
        Assert.Equal(1, complexEntry.Ordinal);
        Assert.Equal(1, complexEntry.OriginalOrdinal);
        Assert.DoesNotContain(complexEntry, entityEntry.GetComplexCollectionEntries(complexProperty));
        Assert.DoesNotContain(complexEntry, entityEntry.GetComplexCollectionOriginalEntries(complexProperty));
        Assert.Equal([0, null, 2], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, null, 2], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));

        Assert.Equal(entityState == EntityState.Modified, entityEntry.IsModified(complexProperty));
    }

    [ConditionalFact]
    public void Multiple_complex_entries_state_changes_maintain_correct_ordinals()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();

        var blog = new Blog
        {
            Tags =
            [
                new Tag { Name = "Tag0" },
                new Tag { Name = "Tag1" },
                new Tag { Name = "Tag2" },
                new Tag { Name = "Tag3" },
                new Tag { Name = "Tag4" }
            ]
        };

        var originalTags = blog.Tags.ToList();
        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);

        Assert.False(entityEntry.IsModified(complexProperty));

        var entry1 = entityEntry.GetComplexCollectionEntry(complexProperty, 1);
        var entry3 = entityEntry.GetComplexCollectionEntry(complexProperty, 3);
        var tag3 = blog.Tags[3];
        blog.Tags.RemoveAt(3);
        var tag1 = blog.Tags[1];
        blog.Tags.RemoveAt(1);
        entry3.SetEntityState(EntityState.Deleted);
        entry1.SetEntityState(EntityState.Deleted);

        Assert.Equal([0, 1, 2], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, 2, 4], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.Equal([0, -1, 1, -1, 2], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, 1, 2, 3, 4], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.Equal([EntityState.Unchanged, EntityState.Unchanged, EntityState.Unchanged],
            entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.EntityState));
        Assert.Equal([EntityState.Unchanged, EntityState.Deleted, EntityState.Unchanged, EntityState.Deleted, EntityState.Unchanged],
            entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e!.EntityState));
        Assert.True(entityEntry.IsModified(complexProperty));

        blog.Tags.Insert(1, tag1);
        entry1.Ordinal = 1;
        entry1.SetEntityState(EntityState.Unchanged);

        Assert.Equal([0, 1, 2, 3], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, 1, 2, 4], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.Equal([0, 1, 2, -1, 3], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, 1, 2, 3, 4], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.Equal([EntityState.Unchanged, EntityState.Unchanged, EntityState.Unchanged, EntityState.Unchanged],
            entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.EntityState));
        Assert.Equal([EntityState.Unchanged, EntityState.Unchanged, EntityState.Unchanged, EntityState.Deleted, EntityState.Unchanged],
            entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e!.EntityState));
        Assert.True(entityEntry.IsModified(complexProperty));

        originalTags.RemoveAt(1);
        entityEntry.SetOriginalValue(complexProperty, originalTags);
        entry1.SetEntityState(EntityState.Added);

        Assert.Equal([0, 1, 2, 3], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, -1, 1, 3], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.Equal([0, 2, -1, 3], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, 1, 2, 3], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.Equal([EntityState.Unchanged, EntityState.Added, EntityState.Unchanged, EntityState.Unchanged],
            entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.EntityState));
        Assert.Equal([EntityState.Unchanged, EntityState.Unchanged, EntityState.Deleted, EntityState.Unchanged],
            entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e!.EntityState));
        Assert.True(entityEntry.IsModified(complexProperty));

        entry3.Ordinal = 3;
        blog.Tags.Insert(3, tag3);
        entry3.SetEntityState(EntityState.Unchanged);

        Assert.Equal([0, 1, 2, 3, 4], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, -1, 1, 2, 3], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.Equal([0, 2, 3, 4], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
        Assert.Equal([0, 1, 2, 3], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.OriginalOrdinal));
        Assert.Equal([EntityState.Unchanged, EntityState.Added, EntityState.Unchanged, EntityState.Unchanged, EntityState.Unchanged],
            entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.EntityState));
        Assert.Equal([EntityState.Unchanged, EntityState.Unchanged, EntityState.Unchanged, EntityState.Unchanged],
            entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e!.EntityState));
        Assert.True(entityEntry.IsModified(complexProperty));
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Deleted)]
    public void Complex_collection_detects_additions_and_deletions(EntityState entityState)
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();
        var changeDetector = serviceProvider.GetRequiredService<IChangeDetector>();

        var blog = new Blog
        {
            Tags =
            [
                new Tag { Name = "Unchanged1", Priority = 1 },
                new Tag { Name = "ToDelete1", Priority = 2 },
                new Tag { Name = "ToModify1", Priority = 3 },
                new Tag { Name = "Unchanged2", Priority = 4 },
                new Tag { Name = "ToDelete2", Priority = 5 },
                new Tag { Name = "ToModify2", Priority = 6 },
                new Tag { Name = "Unchanged3", Priority = 7 }
            ]
        };

        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(entityState);

        blog.Tags[2]!.Name = "Modified1";
        blog.Tags[5]!.Name = "Modified2";
        blog.Tags.RemoveAt(1);
        blog.Tags.RemoveAt(3);
        blog.Tags.Insert(1, new Tag { Name = "Added1", Priority = 1 });
        blog.Tags.Insert(5, new Tag { Name = "Added2", Priority = 4 });
        changeDetector.DetectChanges(entityEntry);

        if (entityState == EntityState.Added)
        {
            Assert.Equal([-1, -1, -1, -1, -1, -1, -1], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));
            Assert.All(entityEntry.GetComplexCollectionEntries(complexProperty), e => Assert.Equal(EntityState.Added, e!.EntityState));
        }
        else if (entityState == EntityState.Deleted)
        {
            Assert.Equal([-1, -1, -1, -1, -1, -1, -1], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
            Assert.All(entityEntry.GetComplexCollectionOriginalEntries(complexProperty), e => Assert.Equal(EntityState.Deleted, e!.EntityState));
        }
        else
        {
            Assert.Equal([0, 1, 2, 3, 5, 4, 6], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e?.OriginalOrdinal));
            Assert.Equal([0, 1, 2, 3, 5, 4, 6], entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e?.Ordinal));
            Assert.Equal([entityState, EntityState.Modified, EntityState.Modified, entityState, EntityState.Modified, EntityState.Modified, entityState],
                entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.EntityState));
            Assert.Equal([entityState, EntityState.Modified, EntityState.Modified, entityState, EntityState.Modified, EntityState.Modified, entityState],
                entityEntry.GetComplexCollectionOriginalEntries(complexProperty).Select(e => e!.EntityState));
        }

        Assert.True(entityEntry.IsModified(complexProperty));
    }

    [ConditionalFact]
    public void Complex_collection_detects_reference_change_as_modified()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();
        var changeDetector = serviceProvider.GetRequiredService<IChangeDetector>();

        var tag1 = new Tag { Name = "Tag1", Priority = 1 };
        var tag2 = new Tag { Name = "Tag2", Priority = 2 };
        var tag3 = new Tag { Name = "Tag3", Priority = 3 };

        var blog = new Blog
        {
            Tags = [tag1, tag2]
        };

        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);

        blog.Tags.Remove(tag1);
        blog.Tags.Insert(0, tag3);

        changeDetector.DetectChanges(entityEntry);

        var addedEntry = entityEntry.GetComplexCollectionEntry(complexProperty, 0);
        Assert.Equal(EntityState.Modified, addedEntry.EntityState);
        var deletedEntry = entityEntry.GetComplexCollectionOriginalEntry(complexProperty, 0);
        Assert.Same(addedEntry, deletedEntry);

        Assert.True(entityEntry.IsModified(complexProperty));
    }

    [ConditionalFact]
    public void Complex_collection_detects_property_modifications()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();
        var changeDetector = serviceProvider.GetRequiredService<IChangeDetector>();

        var tag1 = new Tag { Name = "Tag1", Priority = 1 };
        var tag2 = new Tag { Name = "Tag2", Priority = 2 };

        var blog = new Blog
        {
            Tags = [tag1, tag2]
        };

        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);

        tag1.Name = "Modified Tag1";

        changeDetector.DetectChanges(entityEntry);

        var allEntries = entityEntry.GetComplexCollectionEntries(complexProperty);
        Assert.Equal([0, 1], allEntries.Select(e => e!.OriginalOrdinal));
        Assert.Equal([EntityState.Modified, EntityState.Unchanged], allEntries.Select(e => e!.EntityState));
    }

    [ConditionalFact]
    public void Complex_collection_detects_reordering_without_modification()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();
        var changeDetector = serviceProvider.GetRequiredService<IChangeDetector>();

        var tag1 = new Tag { Name = "Tag1", Priority = 1 };
        var tag2 = new Tag { Name = "Tag2", Priority = 2 };
        var tag3 = new Tag { Name = "Tag3", Priority = 3 };
        var tag4 = new Tag { Name = "Tag3", Priority = 3 };

        var blog = new Blog
        {
            Tags = [tag1, tag2, tag3]
        };

        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);

        Assert.False(entityEntry.IsModified(complexProperty));

        blog.Tags.Clear();
        blog.Tags.AddRange([tag4, tag3, tag1, tag2]);

        changeDetector.DetectChanges(entityEntry);

        Assert.Equal([-1, 2, 0, 1], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.OriginalOrdinal));
        Assert.Equal([EntityState.Added, EntityState.Unchanged, EntityState.Unchanged, EntityState.Unchanged],
            entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.EntityState));
        Assert.True(entityEntry.IsModified(complexProperty));
    }

    [ConditionalFact]
    public void Complex_collection_detects_elements_replaced_with_nulls_as_modified()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();
        var changeDetector = serviceProvider.GetRequiredService<IChangeDetector>();

        var tag1 = new Tag { Name = "Tag1", Priority = 1 };
        var tag2 = new Tag { Name = "Tag2", Priority = 2 };
        var tag3 = new Tag { Name = "Tag3", Priority = 3 };

        var blog = new Blog
        {
            Tags = [tag1, tag2, tag3]
        };

        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);

        blog.Tags[2] = blog.Tags[1];
        blog.Tags[1] = null;
        blog.Tags.Add(null);

        changeDetector.DetectChanges(entityEntry);

        Assert.Equal([0, 2, 1, -1], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.OriginalOrdinal));
        Assert.Equal([EntityState.Unchanged, EntityState.Modified, EntityState.Unchanged, EntityState.Added],
            entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.EntityState));
    }

    [ConditionalFact]
    public void Complex_collection_detects_null_elements_being_replaced_as_modified()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();
        var changeDetector = serviceProvider.GetRequiredService<IChangeDetector>();

        var tag1 = new Tag { Name = "Tag1", Priority = 1 };
        var blog = new Blog
        {
            Tags = [tag1, null!]
        };

        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);

        blog.Tags[1] = new Tag { Name = "Tag2", Priority = 2 };

        changeDetector.DetectChanges(entityEntry);

        Assert.Equal([0, 1], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.OriginalOrdinal));
        Assert.Equal([EntityState.Unchanged, EntityState.Modified], entityEntry.GetComplexCollectionEntries(complexProperty).Select(e => e!.EntityState));
    }

    [ConditionalFact]
    public void Complex_collection_detects_moved_replaced_null_elements_as_modified()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();
        var changeDetector = serviceProvider.GetRequiredService<IChangeDetector>();

        var tag1 = new Tag { Name = "Tag1", Priority = 1 };
        var blog = new Blog
        {
            Tags = [tag1, null!]
        };

        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);

        blog.Tags[1] = new Tag { Name = "Tag2", Priority = 2 };
        blog.Tags.Insert(0, new Tag { Name = "Tag0", Priority = 0 });

        changeDetector.DetectChanges(entityEntry);

        var allEntries = entityEntry.GetComplexCollectionEntries(complexProperty);
        if (allEntries[0]!.EntityState == EntityState.Added)
        {
            Assert.Equal([-1, 0, 1], allEntries.Select(e => e!.OriginalOrdinal));
            Assert.Equal([EntityState.Added, EntityState.Unchanged, EntityState.Modified], allEntries.Select(e => e!.EntityState));
        }
        else
        {
            Assert.Equal([1, 0, -1], allEntries.Select(e => e!.OriginalOrdinal));
            Assert.Equal([EntityState.Modified, EntityState.Unchanged, EntityState.Added], allEntries.Select(e => e!.EntityState));
        }
    }

    [ConditionalFact]
    public void Complex_collection_detects_moved_null_elements_and_replaced_instances_as_unchanged()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();
        var changeDetector = serviceProvider.GetRequiredService<IChangeDetector>();

        var tag1 = new Tag { Name = "Tag1", Priority = 1 };
        var blog = new Blog
        {
            Tags = [tag1, null!]
        };

        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);

        blog.Tags[0] = new Tag { Name = tag1.Name, Priority = tag1.Priority };
        blog.Tags[1] = null;
        blog.Tags.Insert(0, new Tag { Name = "Tag0", Priority = 0 });

        changeDetector.DetectChanges(entityEntry);

        var allEntries = entityEntry.GetComplexCollectionEntries(complexProperty);

        if (allEntries[0]!.EntityState == EntityState.Added)
        {
            Assert.Equal([-1, 0, 1], allEntries.Select(e => e!.OriginalOrdinal));
            Assert.Equal([EntityState.Added, EntityState.Unchanged, EntityState.Modified], allEntries.Select(e => e!.EntityState));
        }
        else
        {
            Assert.Equal([1, 0, -1], allEntries.Select(e => e!.OriginalOrdinal));
            Assert.Equal([EntityState.Modified, EntityState.Unchanged, EntityState.Added], allEntries.Select(e => e!.EntityState));
        }
    }

    [ConditionalFact]
    public void Complex_collection_throws_when_not_initialized()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();

        var blog = new Blog
        {
            Tags = null!
        };
        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);

        Assert.Equal(CoreStrings.ComplexCollectionNotInitialized(nameof(Blog), nameof(Blog.Tags)),
            Assert.Throws<InvalidOperationException>(() => entityEntry.GetComplexCollectionEntry(complexProperty, 0)).Message);
    }

    [ConditionalFact]
    public void GetEntry_throws_when_accessing_original_ordinal_on_added_entity()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();
        var blog = new Blog
        {
            Tags = [new Tag { Name = "Test1" }, new Tag { Name = "Test2" }]
        };
        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Added);

        Assert.Equal(CoreStrings.ComplexCollectionOriginalEntryAddedEntity(0, "Blog", "Tags"),
            Assert.Throws<InvalidOperationException>(() => entityEntry.GetComplexCollectionOriginalEntry(complexProperty, 0)).Message);

        Assert.Equal(CoreStrings.ComplexCollectionEntryOrdinalReadOnly("Blog", "Tags"),
            Assert.Throws<InvalidOperationException>(() =>
                entityEntry.GetComplexCollectionEntry(complexProperty, 0).Ordinal = 1).Message);
    }

    [ConditionalFact]
    public void GetEntry_throws_when_accessing_original_entries_that_were_originally_null()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();

        var blog = new Blog
        {
            Tags = null!
        };
        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);
        blog.Tags = [new Tag { Name = "Test" }];
        entityEntry[complexProperty] = blog.Tags;

        var ex = Assert.Throws<InvalidOperationException>(() => entityEntry.GetComplexCollectionOriginalEntry(complexProperty, 0));
        Assert.Equal(CoreStrings.ComplexCollectionEntryOriginalNull("Blog", "Tags"), ex.Message);
    }

    [ConditionalFact]
    public void GetEntry_throws_when_accessing_invalid_original_ordinal()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();

        var blog = new Blog
        {
            Tags = [new Tag { Name = "Test" }]
        };
        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);
        var ex = Assert.Throws<InvalidOperationException>(() => entityEntry.GetComplexCollectionOriginalEntry(complexProperty, -1));
        Assert.Equal(CoreStrings.ComplexCollectionEntryOriginalOrdinalInvalid(-1, "Blog", "Tags", 1), ex.Message);

        ex = Assert.Throws<InvalidOperationException>(() => entityEntry.GetComplexCollectionOriginalEntry(complexProperty, 5));
        Assert.Equal(CoreStrings.ComplexCollectionEntryOriginalOrdinalInvalid(5, "Blog", "Tags", 1), ex.Message);
    }

    [ConditionalFact]
    public void GetEntry_throws_when_accessing_ordinal_on_deleted_entity()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();
        var blog = new Blog
        {
            Tags = [new Tag { Name = "Test1" }, new Tag { Name = "Test2" }]
        };
        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Deleted);

        Assert.Equal(CoreStrings.ComplexCollectionEntryDeletedEntity(0, "Blog", "Tags"),
            Assert.Throws<InvalidOperationException>(() => entityEntry.GetComplexCollectionEntry(complexProperty, 0)).Message);

        Assert.Equal(CoreStrings.ComplexCollectionEntryOriginalOrdinalReadOnly("Blog", "Tags"),
            Assert.Throws<InvalidOperationException>(() =>
                entityEntry.GetComplexCollectionOriginalEntry(complexProperty, 0).OriginalOrdinal = 1).Message);
    }

    [ConditionalFact]
    public void GetEntry_throws_when_accessing_invalid_current_ordinal()
    {
        var model = CreateModel();
        var entityType = model.FindEntityType(typeof(Blog))!;
        var complexProperty = entityType.FindComplexProperty(nameof(Blog.Tags))!;

        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices(model);
        var stateManager = serviceProvider.GetRequiredService<IStateManager>();

        var blog = new Blog
        {
            Tags = [new Tag { Name = "Test" }]
        };
        var entityEntry = stateManager.GetOrCreateEntry(blog);
        entityEntry.SetEntityState(EntityState.Unchanged);

        var ex = Assert.Throws<InvalidOperationException>(() => entityEntry.GetComplexCollectionEntry(complexProperty, -1));
        Assert.Equal(CoreStrings.ComplexCollectionEntryOrdinalInvalid(-1, "Blog", "Tags", 1), ex.Message);

        ex = Assert.Throws<InvalidOperationException>(() => entityEntry.GetComplexCollectionEntry(complexProperty, 5));
        Assert.Equal(CoreStrings.ComplexCollectionEntryOrdinalInvalid(5, "Blog", "Tags", 1), ex.Message);
    }

    private static IModel CreateModel()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Blog>(eb =>
        {
            eb.ComplexProperty(e => e.Details);
            eb.ComplexCollection(e => e.Tags);
            eb.ComplexCollection(e => e.OtherTags);
        });

        return modelBuilder.FinalizeModel();
    }

    private class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public BlogDetails Details { get; set; } = new();
        public List<Tag?> Tags { get; set; } = [];
        public List<Tag?> OtherTags { get; set; } = [];
    }

    private class BlogDetails
    {
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; }
    }

    private class Tag
    {
        public string Name { get; set; } = "";
        public int Priority { get; set; }
    }

    private class Category
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}

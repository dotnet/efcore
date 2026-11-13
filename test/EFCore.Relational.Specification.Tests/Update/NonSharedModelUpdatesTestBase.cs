// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable MethodHasAsyncOverload

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Update;

public abstract class NonSharedModelUpdatesTestBase(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "NonSharedModelUpdatesTestBase";

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
        => base.AddNonSharedOptions(builder)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning));

    [Theory, MemberData(nameof(IsAsyncData))] // Issue #29356
    public virtual async Task Principal_and_dependent_roundtrips_with_cycle_breaking(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<Author>(b => b.HasOne(a => a.AuthorsClub)
                    .WithMany()
                    .HasForeignKey(a => a.AuthorsClubId));

                mb.Entity<Book>(b => b.HasOne(book => book.Author)
                    .WithMany()
                    .HasForeignKey(book => book.AuthorId));
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(
                    new Book { Author = new Author { Name = "Alice", AuthorsClub = new AuthorsClub { Name = "AC South" } } });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                AuthorsClub authorsClubNorth = new() { Name = "AC North" };
                Author authorOfTheYear2023 = new() { Name = "Author of the year 2023", AuthorsClub = authorsClubNorth };
                context.Add(authorsClubNorth);
                context.Add(authorOfTheYear2023);

                var book = await context
                    .Set<Book>()
                    .Include(b => b.Author)
                    .SingleAsync();
                var authorOfTheYear2022 = book.Author!;
                book.Author = authorOfTheYear2023;

                context.Remove(authorOfTheYear2022);

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            });
    }

    private class AuthorsClub
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class Author
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int AuthorsClubId { get; set; }
        public AuthorsClub? AuthorsClub { get; set; }
    }

    private class Book
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int AuthorId { get; set; }
        public Author? Author { get; set; }
    }

    [Theory, MemberData(nameof(IsAsyncData))] // Issue #29379
    public virtual async Task DbUpdateException_Entries_is_correct_with_multiple_inserts(bool async)
    {
        var contextFactory =
            await InitializeNonSharedTest<DbContext>(onModelCreating: mb => mb.Entity<Blog>().HasIndex(b => b.Name).IsUnique());

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new Blog { Name = "Blog2" });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                context.Add(new Blog { Name = "Blog1" });
                context.Add(new Blog { Name = "Blog2" });
                context.Add(new Blog { Name = "Blog3" });

                var exception = async
                    ? await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())
                    : Assert.Throws<DbUpdateException>(() => context.SaveChanges());

                var entry = Assert.Single(exception.Entries);

                Assert.Equal("Blog2", ((Blog)entry.Entity).Name);
            });
    }

    public class Blog
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    [Theory, MemberData(nameof(IsAsyncData))] // Issue #38917
    public virtual async Task Reassigning_dependent_with_unique_FK_to_attached_entity_orders_updates_correctly(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<WorkItemGroup38917>()
                .HasOne(g => g.Color)
                .WithOne(c => c.Group)
                .HasForeignKey<RgbColor38917>("GroupId"),
            seed: async context =>
            {
                var group0 = new WorkItemGroup38917 { Name = "G0" };
                var group1 = new WorkItemGroup38917 { Name = "G1" };
                var color0 = new RgbColor38917 { Id = "0x9a79e4", DisplayName = "red" };
                var color1 = new RgbColor38917 { Id = "0xb4e6b9", DisplayName = "turquoise" };

                group0.Color = color0;
                group1.Color = color1;
                context.AddRange(group0, group1);

                await context.SaveChangesAsync();
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var g1 = await context.Set<WorkItemGroup38917>()
                    .Include(g => g.Color)
                    .SingleAsync(g => g.Name == "G1");

                var c0 = new RgbColor38917 { Id = "0x9a79e4" };
                context.Attach(c0);
                g1.Color = c0;

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var g1 = await context.Set<WorkItemGroup38917>()
                    .Include(g => g.Color)
                    .SingleAsync(g => g.Name == "G1");
                Assert.Equal("0x9a79e4", g1.Color!.Id);

                var g0 = await context.Set<WorkItemGroup38917>()
                    .Include(g => g.Color)
                    .SingleAsync(g => g.Name == "G0");
                Assert.Null(g0.Color);
            });
    }

    public class WorkItemGroup38917
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public RgbColor38917? Color { get; set; }
    }

    public class RgbColor38917
    {
        public string Id { get; set; } = "";
        public string? DisplayName { get; set; }
        public WorkItemGroup38917? Group { get; set; }
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_entity_with_not_loaded_property_excludes_column_from_SQL(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<BlogWithDescription>(b => b.Property(e => e.Description).Metadata.IsAutoLoaded = false),
            seed: async context =>
            {
                context.Add(new BlogWithDescription { Name = "EF Blog", Description = "Original description" });
                await context.SaveChangesAsync();
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var blog = new BlogWithDescription { Id = 1, Name = "Updated Blog" };
                context.Update(blog);

                var entry = context.Entry(blog);
                // Description starts as not-loaded (IsAutoLoaded = false)
                Assert.False(entry.Property(e => e.Description).IsLoaded);
                Assert.False(entry.Property(e => e.Description).IsModified);
                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var blog = await context.Set<BlogWithDescription>().SingleAsync();
                Assert.Equal("Updated Blog", blog.Name);

                // Description is not auto-loaded, so it should be null (sentinel) in the materialized entity
                Assert.Null(blog.Description);

                // Verify the data is correct in the database by explicitly projecting the non-auto-loaded property
                var description = await context.Set<BlogWithDescription>()
                    .Where(b => b.Description == "Original description")
                    .Select(b => b.Description)
                    .SingleAsync();
                Assert.Equal("Original description", description);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Save_and_query_with_partially_loaded_primitive_collection(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<BlogWithTags>(b =>
            {
                b.Property(e => e.Tags).Metadata.IsAutoLoaded = false;
                b.Property(e => e.Tags).Metadata.Sentinel = new List<string>();
            }),
            seed: async context =>
            {
                context.Add(new BlogWithTags { Name = "EF Blog", Tags = ["efcore", "dotnet"] });
                await context.SaveChangesAsync();
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var blog = new BlogWithTags { Id = 1, Name = "Updated Blog" };
                context.Update(blog);

                var entry = context.Entry(blog);
                Assert.False(entry.Property(e => e.Tags).IsLoaded);
                Assert.False(entry.Property(e => e.Tags).IsModified);

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var blog = await context.Set<BlogWithTags>().SingleAsync();
                Assert.Equal("Updated Blog", blog.Name);
                // Tags is not auto-loaded, so it should be the empty sentinel in the materialized entity
                Assert.Empty(blog.Tags);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Query_with_not_auto_loaded_property_tracked(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<BlogWithDescription>(b => b.Property(e => e.Description).Metadata.IsAutoLoaded = false),
            seed: async context =>
            {
                context.Add(new BlogWithDescription { Name = "EF Blog", Description = "Some description" });
                await context.SaveChangesAsync();
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var blog = async
                    ? await context.Set<BlogWithDescription>().SingleAsync()
                    : context.Set<BlogWithDescription>().Single();

                // The non-auto-loaded property should not have been fetched
                Assert.Null(blog.Description);

                // The change tracker should know the property is not loaded
                var entry = context.Entry(blog);
                Assert.False(entry.Property(e => e.Description).IsLoaded);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Query_with_not_auto_loaded_property_no_tracking(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<BlogWithDescription>(b => b.Property(e => e.Description).Metadata.IsAutoLoaded = false),
            seed: async context =>
            {
                context.Add(new BlogWithDescription { Name = "EF Blog", Description = "Some description" });
                await context.SaveChangesAsync();
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var blog = async
                    ? await context.Set<BlogWithDescription>().AsNoTracking().SingleAsync()
                    : context.Set<BlogWithDescription>().AsNoTracking().Single();

                // The non-auto-loaded property should not have been fetched, and retains its default/sentinel value
                Assert.Null(blog.Description);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Explicit_select_of_not_auto_loaded_property(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<BlogWithDescription>(b => b.Property(e => e.Description).Metadata.IsAutoLoaded = false),
            seed: async context =>
            {
                context.Add(new BlogWithDescription { Name = "EF Blog", Description = "Some description" });
                await context.SaveChangesAsync();
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                // Explicitly projecting the non-auto-loaded property should still work
                var description = async
                    ? await context.Set<BlogWithDescription>().Select(b => b.Description).SingleAsync()
                    : context.Set<BlogWithDescription>().Select(b => b.Description).Single();

                Assert.Equal("Some description", description);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_on_not_auto_loaded_property(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<BlogWithDescription>(b => b.Property(e => e.Description).Metadata.IsAutoLoaded = false),
            seed: async context =>
            {
                context.Add(new BlogWithDescription { Name = "EF Blog", Description = "Some description" });
                await context.SaveChangesAsync();
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                // Filtering on a non-auto-loaded property should work; the property must be available in the subquery
                var blog = async
                    ? await context.Set<BlogWithDescription>().Where(b => b.Description == "Some description").SingleAsync()
                    : context.Set<BlogWithDescription>().Where(b => b.Description == "Some description").Single();

                Assert.Equal("EF Blog", blog.Name);

                // The non-auto-loaded property should still not be in the entity projection
                Assert.Null(blog.Description);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Query_with_not_auto_loaded_primitive_collection(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<BlogWithTags>(b =>
            {
                b.Property(e => e.Tags).Metadata.IsAutoLoaded = false;
                b.Property(e => e.Tags).Metadata.Sentinel = new List<string>();
            }),
            seed: async context =>
            {
                context.Add(new BlogWithTags { Name = "EF Blog", Tags = ["efcore", "dotnet"] });
                await context.SaveChangesAsync();
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var blog = async
                    ? await context.Set<BlogWithTags>().SingleAsync()
                    : context.Set<BlogWithTags>().Single();

                // The non-auto-loaded primitive collection should not have been fetched
                Assert.Empty(blog.Tags);
            });
    }

    private class BlogWithDescription
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    private class BlogWithTags
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    [Theory, MemberData(nameof(IsAsyncData))] // Issue #36059
    public virtual async Task Replacing_owned_entity_with_FK_to_another_entity(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<Document36059>(b => b.OwnsOne(
                    d => d.File, fb =>
                    {
                        fb.Property(f => f.Id).ValueGeneratedNever();
                        fb.HasOne(f => f.Content)
                            .WithMany()
                            .HasForeignKey(f => f.ContentId)
                            .IsRequired()
                            .OnDelete(DeleteBehavior.Restrict);
                    }));

                mb.Entity<Content36059>(b => b.Property(c => c.Id).ValueGeneratedNever());
            });

        var oldContentId = Guid.NewGuid();
        var newContentId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var oldFileId = Guid.NewGuid();
        var newFileId = Guid.NewGuid();

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(
                    new Document36059
                    {
                        Id = documentId,
                        File = new File36059
                        {
                            Id = oldFileId,
                            Name = "old.jpg",
                            ContentId = oldContentId,
                            Content = new Content36059 { Id = oldContentId, Data = "initial" }
                        }
                    });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var document = await context.Set<Document36059>()
                    .Include(d => d.File)
                    .SingleAsync(d => d.Id == documentId);

                document.File = new File36059
                {
                    Id = newFileId,
                    Name = "new.png",
                    ContentId = newContentId,
                    Content = new Content36059 { Id = newContentId, Data = "updated" }
                };

                context.Set<Content36059>().Remove(new Content36059 { Id = oldContentId });

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var document = await context.Set<Document36059>()
                    .Include(d => d.File)
                    .SingleAsync(d => d.Id == documentId);

                Assert.NotNull(document.File);
                Assert.Equal(newFileId, document.File.Id);
                Assert.Equal("new.png", document.File.Name);
                Assert.Equal(newContentId, document.File.ContentId);

                Assert.Null(await context.Set<Content36059>().SingleOrDefaultAsync(c => c.Id == oldContentId));
                Assert.NotNull(await context.Set<Content36059>().SingleOrDefaultAsync(c => c.Id == newContentId));
            });
    }

    private class Document36059
    {
        public Guid Id { get; set; }
        public File36059? File { get; set; }
    }

    private class File36059
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public Guid ContentId { get; set; }
        public required Content36059 Content { get; set; }
    }

    private class Content36059
    {
        public Guid Id { get; set; }
        public string? Data { get; set; }
    }

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Nullable_json_collection_changed_from_empty_to_null_is_saved(bool alsoModifyName)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParent { Name = "A", Children = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.NotNull(parent.Children);
                Assert.Empty(parent.Children);

                if (alsoModifyName)
                {
                    parent.Name = "B";
                }

                parent.Children = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Null(parent.Children);
                Assert.Equal(alsoModifyName ? "B" : "A", parent.Name);
            });
    }

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Nullable_json_collection_changed_from_null_to_empty_is_saved(bool alsoModifyName)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParent { Name = "A", Children = null });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Null(parent.Children);

                if (alsoModifyName)
                {
                    parent.Name = "B";
                }

                parent.Children = [];
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.NotNull(parent.Children);
                Assert.Empty(parent.Children);
                Assert.Equal(alsoModifyName ? "B" : "A", parent.Name);
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Nullable_json_collection_with_elements_changed_to_null_is_saved()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParent { Name = "A", Children = [new JsonChild { Name = "C1" }] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Single(parent.Children!);

                parent.Children = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Null(parent.Children);
            });
    }

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Nullable_json_collection_nested_in_json_changed_from_empty_to_null_is_saved(bool alsoModifyReference)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonReferenceParent>().OwnsOne(
                p => p.Reference, b =>
                {
                    b.ToJson();
                    b.OwnsMany(r => r.Items);
                }));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(
                    new NullableJsonReferenceParent { Name = "A", Reference = new JsonReference { Name = "R", Items = [] } });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonReferenceParent>().SingleAsync();
                Assert.NotNull(parent.Reference!.Items);
                Assert.Empty(parent.Reference.Items);

                if (alsoModifyReference)
                {
                    parent.Reference.Name = "R2";
                }

                parent.Reference.Items = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonReferenceParent>().SingleAsync();
                Assert.Null(parent.Reference!.Items);
                Assert.Equal(alsoModifyReference ? "R2" : "R", parent.Reference.Name);
            });
    }

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Nullable_json_collection_stored_as_null_is_loaded_as_null_even_with_initializer(bool tracking)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParentWithInitializer>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParentWithInitializer { Name = "Null", Children = null });
                context.Add(new NullableJsonParentWithInitializer { Name = "Empty", Children = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var query = context.Set<NullableJsonParentWithInitializer>().OrderBy(p => p.Name);
                var parents = tracking ? await query.ToListAsync() : await query.AsNoTracking().ToListAsync();

                Assert.Equal("Empty", parents[0].Name);
                Assert.NotNull(parents[0].Children);
                Assert.Empty(parents[0].Children!);

                Assert.Equal("Null", parents[1].Name);
                Assert.Null(parents[1].Children);
            });
    }

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Update_of_a_disconnected_entity_saves_the_null_ness_of_a_nullable_json_collection(bool toNull)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson()));

        var id = 0;

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var parent = new NullableJsonParent { Name = "A", Children = toNull ? [] : null };
                context.Add(parent);
                await context.SaveChangesAsync();
                id = parent.Id;
            },
            async context =>
            {
                context.Update(new NullableJsonParent { Id = id, Name = "A", Children = toNull ? null : [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                if (toNull)
                {
                    Assert.Null(parent.Children);
                }
                else
                {
                    Assert.NotNull(parent.Children);
                    Assert.Empty(parent.Children);
                }
            });
    }

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Replacing_an_owner_with_the_same_key_saves_the_null_ness_of_a_nullable_json_collection(bool toNull)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson()));

        var id = 0;

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var parent = new NullableJsonParent { Name = "A", Children = toNull ? [] : null };
                context.Add(parent);
                await context.SaveChangesAsync();
                id = parent.Id;
            },
            async context =>
            {
                var old = await context.Set<NullableJsonParent>().SingleAsync();
                context.Remove(old);
                context.Add(new NullableJsonParent { Id = id, Name = "B", Children = toNull ? null : [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Equal("B", parent.Name);
                if (toNull)
                {
                    Assert.Null(parent.Children);
                }
                else
                {
                    Assert.NotNull(parent.Children);
                    Assert.Empty(parent.Children);
                }
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection_to_null()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParent { Name = "A", Children = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                parent.Children = null;

                var again = await context.Set<NullableJsonParent>().SingleAsync();

                Assert.Same(parent, again);
                Assert.Null(again.Children);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Null(parent.Children);
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Querying_a_tracked_entity_with_an_unchanged_nullable_json_collection_does_not_detect_a_change()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.AddRange(new NullableJsonParent { Name = "Empty", Children = [] }, new NullableJsonParent { Name = "Null" });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var first = await context.Set<NullableJsonParent>().OrderBy(p => p.Name).ToListAsync();
                var second = await context.Set<NullableJsonParent>().OrderBy(p => p.Name).ToListAsync();

                Assert.Equal(first, second);
                Assert.NotNull(first[0].Children);
                Assert.Empty(first[0].Children!);
                Assert.Null(first[1].Children);
                Assert.False(context.ChangeTracker.HasChanges());
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Json_collection_that_is_not_nullable_stored_as_null_is_still_loaded_as_empty_with_initializer()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NonNullableJsonParentWithInitializer>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NonNullableJsonParentWithInitializer { Children = null! });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NonNullableJsonParentWithInitializer>().SingleAsync();
                Assert.NotNull(parent.Children);
                Assert.Empty(parent.Children);
            });
    }

    [Theory, InlineData(ChangeTrackingStrategy.ChangedNotifications), InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    public virtual async Task Nullable_json_collection_of_a_notifying_entity_changed_from_empty_to_null_is_saved(
        ChangeTrackingStrategy strategy) // Issue #33452
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.HasChangeTrackingStrategy(strategy);
                mb.Entity<NotifyingNullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson());
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NotifyingNullableJsonParent { Name = "A", Children = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NotifyingNullableJsonParent>().SingleAsync();
                Assert.NotNull(parent.Children);
                Assert.Empty(parent.Children);

                parent.Children = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NotifyingNullableJsonParent>().SingleAsync();
                Assert.Null(parent.Children);
            });
    }

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Querying_a_tracked_entity_keeps_the_in_memory_null_ness_of_a_nullable_json_collection(bool inMemoryNull)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParent { Name = "A", Children = inMemoryNull ? null : [new JsonChild { Name = "C1" }] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Equal(inMemoryNull, parent.Children is null);

                // Changed behind the back of the context, as if by another one
                context.Database.ExecuteSqlRaw(
                    inMemoryNull
                        ? "UPDATE [NullableJsonParent] SET [Children] = '[]'"
                        : "UPDATE [NullableJsonParent] SET [Children] = NULL");

                var again = await context.Set<NullableJsonParent>().SingleAsync();

                Assert.Same(parent, again);
                if (inMemoryNull)
                {
                    Assert.Null(parent.Children);
                }
                else
                {
                    Assert.NotNull(parent.Children);
                    Assert.Single(parent.Children);
                }

                Assert.False(context.ChangeTracker.HasChanges());
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Nullable_json_collection_declared_on_a_derived_type_changed_to_null_is_saved()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<NullableJsonBase>();
                mb.Entity<NullableJsonDerived>().OwnsMany(p => p.Children, b => b.ToJson());
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonBase { Name = "Base" });
                context.Add(new NullableJsonDerived { Name = "Derived", Children = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var derived = await context.Set<NullableJsonDerived>().SingleAsync();
                Assert.NotNull(derived.Children);
                Assert.Empty(derived.Children);

                derived.Children = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var all = await context.Set<NullableJsonBase>().OrderBy(e => e.Name).ToListAsync();
                Assert.Equal(2, all.Count);
                Assert.Equal("Base", all[0].Name);
                Assert.IsType<NullableJsonDerived>(all[1]);
                Assert.Null(((NullableJsonDerived)all[1]).Children);
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Nested_nullable_json_collection_changed_to_null_is_saved()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NestedNullableJsonOwner>().OwnsMany(
                p => p.Items, b =>
                {
                    b.ToJson();
                    b.OwnsMany(o => o.Inner);
                }));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(
                    new NestedNullableJsonOwner
                    {
                        Items =
                        [
                            new NestedOuterItem { Name = "O1", Inner = [] },
                            new NestedOuterItem { Name = "O2", Inner = [] }
                        ]
                    });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var owner = await context.Set<NestedNullableJsonOwner>().SingleAsync();
                Assert.NotNull(owner.Items);
                Assert.Equal(2, owner.Items.Count);
                Assert.All(owner.Items, i => Assert.NotNull(i.Inner));

                owner.Items[0].Inner = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var owner = await context.Set<NestedNullableJsonOwner>().SingleAsync();
                Assert.NotNull(owner.Items);
                Assert.Equal(2, owner.Items.Count);
                Assert.Null(owner.Items[0].Inner);
                Assert.NotNull(owner.Items[1].Inner);
                Assert.Empty(owner.Items[1].Inner!);
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Nullable_json_collection_changed_to_null_is_saved_for_an_entity_with_a_concurrency_token()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParentWithToken>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParentWithToken { Token = "T1", Children = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParentWithToken>().SingleAsync();
                parent.Children = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParentWithToken>().SingleAsync();
                Assert.Equal("T1", parent.Token);
                Assert.Null(parent.Children);
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Mixed_changes_of_nullable_json_collections_are_saved_in_one_batch()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.AddRange(
                    new NullableJsonParent { Name = "ToNull", Children = [] },
                    new NullableJsonParent { Name = "ToEmpty", Children = null },
                    new NullableJsonParent { Name = "OnlyName", Children = [] },
                    new NullableJsonParent { Name = "Elements", Children = [new JsonChild { Name = "C1" }] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parents = await context.Set<NullableJsonParent>().ToDictionaryAsync(p => p.Name!);

                parents["ToNull"].Children = null;
                parents["ToEmpty"].Children = [];
                parents["OnlyName"].Name = "OnlyNameChanged";
                parents["Elements"].Children!.Add(new JsonChild { Name = "C2" });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parents = await context.Set<NullableJsonParent>().ToDictionaryAsync(p => p.Name!);

                Assert.Null(parents["ToNull"].Children);

                Assert.NotNull(parents["ToEmpty"].Children);
                Assert.Empty(parents["ToEmpty"].Children!);

                Assert.NotNull(parents["OnlyNameChanged"].Children);
                Assert.Empty(parents["OnlyNameChanged"].Children!);

                Assert.Equal(["C1", "C2"], parents["Elements"].Children!.Select(c => c.Name));
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Nullable_json_collection_loaded_without_tracking_keeps_null_and_empty()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParentWithInitializer>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParentWithInitializer { Name = "Null", Children = null });
                context.Add(new NullableJsonParentWithInitializer { Name = "Empty", Children = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var noTracking = await context.Set<NullableJsonParentWithInitializer>().AsNoTracking().OrderBy(p => p.Name).ToListAsync();
                Assert.Equal(2, noTracking.Count);
                Assert.Equal("Empty", noTracking[0].Name);
                Assert.NotNull(noTracking[0].Children);
                Assert.Empty(noTracking[0].Children!);
                Assert.Equal("Null", noTracking[1].Name);
                Assert.Null(noTracking[1].Children);

                var identityResolution = await context.Set<NullableJsonParentWithInitializer>()
                    .AsNoTrackingWithIdentityResolution().OrderBy(p => p.Name).ToListAsync();
                Assert.Empty(identityResolution[0].Children!);
                Assert.Null(identityResolution[1].Children);

                Assert.Empty(context.ChangeTracker.Entries());
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Nullable_json_collection_projected_with_its_entity_keeps_null_and_empty()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParentWithInitializer>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParentWithInitializer { Name = "Null", Children = null });
                context.Add(new NullableJsonParentWithInitializer { Name = "Empty", Children = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var results = await context.Set<NullableJsonParentWithInitializer>()
                    .OrderBy(p => p.Name)
                    .Select(p => new { Parent = p, p.Name })
                    .ToListAsync();

                Assert.Equal(2, results.Count);
                Assert.Empty(results[0].Parent.Children!);
                Assert.Null(results[1].Parent.Children);
                Assert.False(context.ChangeTracker.HasChanges());
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Opting_out_of_a_nullable_json_collection_keeps_the_initializer_on_load()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<NullableJsonParentWithInitializer>().OwnsMany(p => p.Children, b => b.ToJson());
                mb.Model.FindEntityType(typeof(NullableJsonParentWithInitializer))!.FindNavigation(
                    nameof(NullableJsonParentWithInitializer.Children))!.SetIsNullable(false);
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParentWithInitializer { Name = "Null", Children = null });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParentWithInitializer>().SingleAsync();

                Assert.NotNull(parent.Children);
                Assert.Empty(parent.Children);
                Assert.False(context.ChangeTracker.HasChanges());
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection_to_empty()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<NullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson()));

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParent { Name = "A", Children = null });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Null(parent.Children);

                parent.Children = [];
                context.ChangeTracker.DetectChanges();

                var again = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Same(parent, again);
                Assert.NotNull(parent.Children);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.NotNull(parent.Children);
                Assert.Empty(parent.Children);
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Nullable_json_collection_with_a_coercing_setter_loaded_as_null_has_no_changes()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<CoercingNullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson());
                mb.Entity<CoercingNullableJsonParent>().Navigation(p => p.Children).UsePropertyAccessMode(PropertyAccessMode.Property);
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new CoercingNullableJsonParent { Name = "A" });
                await context.SaveChangesAsync();
                context.Database.ExecuteSql($"UPDATE [CoercingNullableJsonParent] SET [Children] = NULL");
            },
            async context =>
            {
                var parent = await context.Set<CoercingNullableJsonParent>().SingleAsync();

                Assert.False(context.ChangeTracker.HasChanges());
                Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
            });
    }

    [Theory, InlineData(ChangeTrackingStrategy.ChangedNotifications), InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    public virtual async Task Nullable_json_collection_of_a_notifying_entity_with_an_initializer_loaded_as_null_has_no_changes(
        ChangeTrackingStrategy strategy) // Issue #33452
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.HasChangeTrackingStrategy(strategy);
                mb.Entity<InitializedNotifyingNullableJsonParent>().OwnsMany(p => p.Children, b => b.ToJson());
                mb.Entity<InitializedNotifyingNullableJsonParent>().Navigation(p => p.Children)
                    .UsePropertyAccessMode(PropertyAccessMode.Property);
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new InitializedNotifyingNullableJsonParent { Name = "A" });
                await context.SaveChangesAsync();
                context.Database.ExecuteSql($"UPDATE [InitializedNotifyingNullableJsonParent] SET [Children] = NULL");
            },
            async context =>
            {
                var parent = await context.Set<InitializedNotifyingNullableJsonParent>().SingleAsync();

                Assert.Null(parent.Children);
                Assert.False(context.ChangeTracker.HasChanges());
                Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
            });
    }

    private class CoercingNullableJsonParent
    {
        private List<JsonChild> _children = [];

        public int Id { get; set; }
        public string? Name { get; set; }

        public List<JsonChild>? Children
        {
            get => _children;
            set => _children = value ?? [];
        }
    }

    private class InitializedNotifyingNullableJsonParent : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _id;
        private string? _name;
        private ObservableHashSet<NotifyingJsonChild>? _childrenField = [];

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        public int Id
        {
            get => _id;
            set => SetValue(ref _id, value);
        }

        public string? Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        public ObservableHashSet<NotifyingJsonChild>? Children
        {
            get => _childrenField;
            set => SetValue(ref _childrenField, value);
        }

        private void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private class NotifyingNullableJsonParent : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _id;
        private string? _name;
        private ObservableHashSet<NotifyingJsonChild>? _children;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        public int Id
        {
            get => _id;
            set => SetValue(ref _id, value);
        }

        public string? Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        public ObservableHashSet<NotifyingJsonChild>? Children
        {
            get => _children;
            set => SetValue(ref _children, value);
        }

        private void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private class NotifyingJsonChild : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private string? _name;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        public string? Name
        {
            get => _name;
            set
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Name)));
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }

    private class NullableJsonBase
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class NullableJsonDerived : NullableJsonBase
    {
        public List<JsonChild>? Children { get; set; }
    }

    private class NestedNullableJsonOwner
    {
        public int Id { get; set; }
        public List<NestedOuterItem>? Items { get; set; }
    }

    private class NestedOuterItem
    {
        public string? Name { get; set; }
        public List<JsonChild>? Inner { get; set; }
    }

    private class NullableJsonParentWithToken
    {
        public int Id { get; set; }

        [ConcurrencyCheck]
        public string? Token { get; set; }

        public List<JsonChild>? Children { get; set; }
    }

    private class NullableJsonParent
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<JsonChild>? Children { get; set; }
    }

    private class NullableJsonReferenceParent
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public JsonReference? Reference { get; set; }
    }

    private class NullableJsonParentWithInitializer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<JsonChild>? Children { get; set; } = [];
    }

    private class NonNullableJsonParentWithInitializer
    {
        public int Id { get; set; }
        public List<JsonChild> Children { get; set; } = [];
    }

    private class JsonReference
    {
        public string? Name { get; set; }
        public List<JsonChild>? Items { get; set; }
    }

    private class JsonChild
    {
        public string? Name { get; set; }
    }

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        ContextFactory<DbContext> contextFactory,
        Func<DbContext, Task> testOperation,
        Func<DbContext, Task>? nestedTestOperation1 = null,
        Func<DbContext, Task>? nestedTestOperation2 = null,
        Func<DbContext, Task>? nestedTestOperation3 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            contextFactory.CreateDbContext, UseTransaction, testOperation, nestedTestOperation1, nestedTestOperation2,
            nestedTestOperation3);

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}

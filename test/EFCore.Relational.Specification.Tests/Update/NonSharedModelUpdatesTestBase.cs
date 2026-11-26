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

    [Theory, MemberData(nameof(IsAsyncData))] // Issue #39596
    public virtual async Task Optional_complex_properties_on_sibling_types_sharing_columns_can_be_set_to_null(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<SharedComplexBase>().HasDiscriminator<int>("Discriminator")
                    .HasValue<SharedComplexA>(0)
                    .HasValue<SharedComplexB>(1);
                mb.Entity<SharedComplexBase>().Property(e => e.Id).ValueGeneratedNever();
                mb.Entity<SharedComplexA>().ComplexProperty(e => e.Value, ConfigureValue);
                mb.Entity<SharedComplexB>().ComplexProperty(e => e.Value, ConfigureValue);
                mb.Entity<SingleComplexBase>().HasDiscriminator<int>("Discriminator").HasValue<SingleComplex>(0);
                mb.Entity<SingleComplexBase>().Property(e => e.Id).ValueGeneratedNever();
                mb.Entity<SingleComplex>().ComplexProperty(e => e.Value, ConfigureValue);
                mb.Entity<PlainComplex>().Property(e => e.Id).ValueGeneratedNever();
                mb.Entity<PlainComplex>().ComplexProperty(e => e.Value, ConfigureValue);
            });

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.AddRange(
                    new SharedComplexA { Id = 1, Value = new ComplexValue(1, 2, 3) },
                    new SharedComplexB { Id = 2, Value = new ComplexValue(4, 5, 6) },
                    new SharedComplexB { Id = 3, Value = new ComplexValue(7, 8, 9) },
                    new SingleComplex { Id = 4, Value = new ComplexValue(1, 2, 3) },
                    new PlainComplex { Id = 5, Value = new ComplexValue(1, 2, 3) });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                (await context.Set<SharedComplexA>().SingleAsync()).Value = null;
                (await context.Set<SharedComplexB>().SingleAsync(e => e.Id == 2)).Value = null;
                (await context.Set<SingleComplex>().SingleAsync()).Value = null;
                (await context.Set<PlainComplex>().SingleAsync()).Value = null;

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
                var a = await context.Set<SharedComplexA>().SingleAsync();
                Assert.Null(a.Value);
                Assert.Null((await context.Set<SharedComplexB>().SingleAsync(e => e.Id == 2)).Value);
                Assert.Equal(new ComplexValue(7, 8, 9), (await context.Set<SharedComplexB>().SingleAsync(e => e.Id == 3)).Value);
                Assert.Null((await context.Set<SingleComplex>().SingleAsync()).Value);
                Assert.Null((await context.Set<PlainComplex>().SingleAsync()).Value);

                a.Value = new ComplexValue(10, 11, 12);
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
                Assert.Equal(new ComplexValue(10, 11, 12), (await context.Set<SharedComplexA>().SingleAsync()).Value));

        static void ConfigureValue(ComplexPropertyBuilder<ComplexValue> builder)
        {
            builder.Property(e => e.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            builder.Property(e => e.Error).HasColumnName("Error").HasPrecision(18, 2);
            builder.Property(e => e.Precision).HasColumnName("Precision");
        }
    }

    private abstract class SharedComplexBase
    {
        public int Id { get; set; }
    }

    private sealed class SharedComplexA : SharedComplexBase
    {
        public ComplexValue? Value { get; set; }
    }

    private sealed class SharedComplexB : SharedComplexBase
    {
        public ComplexValue? Value { get; set; }
    }

    private abstract class SingleComplexBase
    {
        public int Id { get; set; }
    }

    private sealed class SingleComplex : SingleComplexBase
    {
        public ComplexValue? Value { get; set; }
    }

    private sealed class PlainComplex
    {
        public int Id { get; set; }
        public ComplexValue? Value { get; set; }
    }

    private sealed record ComplexValue(decimal Amount, decimal Error, short Precision);

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

    [Theory, InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)] // Issue #39617
    public virtual async Task Replacing_owned_entity_preserves_on_add_values(bool async, bool changeFloor)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<Customer39617>(b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.OwnsOne(e => e.Address, a =>
                {
                    a.Property(e => e.Floor).HasDefaultValue(1);
                    a.Property(e => e.NullableFloor).HasDefaultValue(1);
                    a.Property(e => e.Note).ValueGeneratedOnAdd();
                });
            }));

        var floor = changeFloor ? 4 : 3;
        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(
                    new Customer39617
                    {
                        Id = 1,
                        Address = new Address39617 { City = "Paris", Floor = 3, NullableFloor = 7, Note = "back door" }
                    });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var customer = await context.Set<Customer39617>().SingleAsync();
                customer.Address = new Address39617 { City = "Lyon", Floor = floor, NullableFloor = 7, Note = "back door" };

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                Assert.Equal(floor, customer.Address.Floor);
                Assert.Equal(7, customer.Address.NullableFloor);
                Assert.Equal("back door", customer.Address.Note);
            },
            async context =>
            {
                var customer = await context.Set<Customer39617>().SingleAsync();
                Assert.Equal("Lyon", customer.Address.City);
                Assert.Equal(floor, customer.Address.Floor);
                Assert.Equal(7, customer.Address.NullableFloor);
                Assert.Equal("back door", customer.Address.Note);
            });
    }

    [Theory, InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)] // Issue #39617
    public virtual async Task Readding_entity_with_same_key_preserves_on_add_values(bool async, bool changeQuantity)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<OrderLine39617>(b =>
            {
                b.HasKey(e => new { e.OrderId, e.ProductId });
                b.Property(e => e.Quantity).HasDefaultValue(1);
                b.Property(e => e.Note).HasDefaultValue("none");
            }));

        var quantity = changeQuantity ? 6 : 5;
        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new OrderLine39617 { OrderId = 1, ProductId = 10, Quantity = 5, Note = "original" });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                context.Remove(await context.Set<OrderLine39617>().SingleAsync());
                var line = new OrderLine39617 { OrderId = 1, ProductId = 10, Quantity = quantity, Note = "original" };
                context.Add(line);

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                Assert.Equal(quantity, line.Quantity);
                Assert.Equal("original", line.Note);
            },
            async context =>
            {
                var line = await context.Set<OrderLine39617>().SingleAsync();
                Assert.Equal(quantity, line.Quantity);
                Assert.Equal("original", line.Note);
            });
    }

    [Theory, InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)] // Issue #39617
    public virtual async Task Replacing_owned_entity_preserves_modified_shared_column_value(bool async, bool setNull)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<SharedCustomer39617>(b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Note).HasColumnName("Note").HasDefaultValue("none");
                b.OwnsOne(e => e.Address, a => a.Property(e => e.Note).HasColumnName("Note").HasDefaultValue("none"));
            }));

        var note = setNull ? null : "updated";
        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(
                    new SharedCustomer39617
                    {
                        Id = 1,
                        Note = "original",
                        Address = new SharedAddress39617 { City = "Paris", Note = "original" }
                    });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var customer = await context.Set<SharedCustomer39617>().SingleAsync();
                customer.Note = note;
                customer.Address = new SharedAddress39617 { City = "Lyon", Note = "original" };

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                Assert.Equal(note, customer.Note);
                Assert.Equal(note, customer.Address.Note);
            },
            async context =>
            {
                var customer = await context.Set<SharedCustomer39617>().SingleAsync();
                Assert.Equal("Lyon", customer.Address.City);
                Assert.Equal(note, customer.Note);
                Assert.Equal(note, customer.Address.Note);
            });
    }

    private class Customer39617
    {
        public int Id { get; set; }
        public Address39617 Address { get; set; } = null!;
    }

    private class Address39617
    {
        public string City { get; set; } = "";
        public int Floor { get; set; }
        public int? NullableFloor { get; set; }
        public string? Note { get; set; }
    }

    private class OrderLine39617
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }

    private class SharedCustomer39617
    {
        public int Id { get; set; }
        public string? Note { get; set; }
        public SharedAddress39617 Address { get; set; } = null!;
    }

    private class SharedAddress39617
    {
        public string City { get; set; } = "";
        public string? Note { get; set; }
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

    public enum NullableJsonCollectionValue
    {
        Null,
        Empty,
        WithElements
    }

    public enum NullableJsonCollectionQuery
    {
        Tracking,
        NoTracking,
        NoTrackingWithIdentityResolution,
        ProjectionWithEntity
    }

    [Theory] // Issue #33452
    [InlineData(NullableJsonCollectionValue.Empty, NullableJsonCollectionValue.Null, false)]
    [InlineData(NullableJsonCollectionValue.Empty, NullableJsonCollectionValue.Null, true)]
    [InlineData(NullableJsonCollectionValue.Null, NullableJsonCollectionValue.Empty, false)]
    [InlineData(NullableJsonCollectionValue.Null, NullableJsonCollectionValue.Empty, true)]
    [InlineData(NullableJsonCollectionValue.WithElements, NullableJsonCollectionValue.Null, false)]
    public virtual async Task Nullable_json_collection_change_is_saved(
        NullableJsonCollectionValue from,
        NullableJsonCollectionValue to,
        bool alsoModifyName)
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                // The concurrency token makes the update check it, as well as the key
                context.Add(new NullableJsonParent { Name = "A", Token = "T1", Children = CreateChildren(from) });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                AssertChildren(from, parent.Children);

                if (alsoModifyName)
                {
                    parent.Name = "B";
                }

                parent.Children = CreateChildren(to);
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                AssertChildren(to, parent.Children);
                Assert.Equal(alsoModifyName ? "B" : "A", parent.Name);
                Assert.Equal("T1", parent.Token);
            });

        static List<JsonChild>? CreateChildren(NullableJsonCollectionValue value)
            => value switch
            {
                NullableJsonCollectionValue.Null => null,
                NullableJsonCollectionValue.Empty => [],
                _ => [new JsonChild { Name = "C1" }]
            };

        static void AssertChildren(NullableJsonCollectionValue value, List<JsonChild>? children)
        {
            switch (value)
            {
                case NullableJsonCollectionValue.Null:
                    Assert.Null(children);
                    break;
                case NullableJsonCollectionValue.Empty:
                    Assert.NotNull(children);
                    Assert.Empty(children);
                    break;
                default:
                    Assert.Equal(["C1"], children!.Select(c => c.Name));
                    break;
            }
        }
    }

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Nullable_json_collection_nested_in_json_changed_from_empty_to_null_is_saved(bool alsoModifyReference)
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParent { Name = "A", Reference = new JsonReference { Name = "R", Items = [] } });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
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
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Null(parent.Reference!.Items);
                Assert.Equal(alsoModifyReference ? "R2" : "R", parent.Reference.Name);
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Nested_nullable_json_collection_changed_to_null_is_saved()
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(
                    new NullableJsonParent
                    {
                        Name = "A",
                        Outer =
                        [
                            new JsonOuterItem { Name = "O1", Inner = [] },
                            new JsonOuterItem { Name = "O2", Inner = [] }
                        ]
                    });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.NotNull(parent.Outer);
                Assert.Equal(2, parent.Outer.Count);
                Assert.All(parent.Outer, i => Assert.NotNull(i.Inner));

                parent.Outer[0].Inner = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.NotNull(parent.Outer);
                Assert.Equal(2, parent.Outer.Count);
                Assert.Null(parent.Outer[0].Inner);
                Assert.NotNull(parent.Outer[1].Inner);
                Assert.Empty(parent.Outer[1].Inner!);
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Nullable_json_collection_declared_on_a_derived_type_changed_to_null_is_saved()
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParent { Name = "Base" });
                context.Add(new NullableJsonDerivedParent { Name = "Derived", DerivedChildren = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var derived = await context.Set<NullableJsonDerivedParent>().SingleAsync();
                Assert.NotNull(derived.DerivedChildren);
                Assert.Empty(derived.DerivedChildren);

                derived.DerivedChildren = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var all = await context.Set<NullableJsonParent>().OrderBy(e => e.Name).ToListAsync();
                Assert.Equal(2, all.Count);
                Assert.Equal("Base", all[0].Name);
                Assert.IsType<NullableJsonDerivedParent>(all[1]);
                Assert.Null(((NullableJsonDerivedParent)all[1]).DerivedChildren);
            });
    }

    [Theory] // Issue #33452
    [InlineData(NullableJsonCollectionQuery.Tracking)]
    [InlineData(NullableJsonCollectionQuery.NoTracking)]
    [InlineData(NullableJsonCollectionQuery.NoTrackingWithIdentityResolution)]
    [InlineData(NullableJsonCollectionQuery.ProjectionWithEntity)]
    public virtual async Task Nullable_json_collection_stored_as_null_or_empty_is_loaded_as_null_or_empty(NullableJsonCollectionQuery queryKind)
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                // InitializedChildren is initialized to an empty collection, which must not hide a stored null
                context.Add(new NullableJsonParent { Name = "Null", Children = null, InitializedChildren = null });
                context.Add(new NullableJsonParent { Name = "Empty", Children = [], InitializedChildren = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var query = context.Set<NullableJsonParent>().OrderBy(p => p.Name);
                var parents = queryKind switch
                {
                    NullableJsonCollectionQuery.Tracking => await query.ToListAsync(),
                    NullableJsonCollectionQuery.NoTracking => await query.AsNoTracking().ToListAsync(),
                    NullableJsonCollectionQuery.NoTrackingWithIdentityResolution
                        => await query.AsNoTrackingWithIdentityResolution().ToListAsync(),
                    _ => (await query.Select(p => new { Parent = p, p.Name }).ToListAsync()).Select(r => r.Parent).ToList()
                };

                Assert.Equal(2, parents.Count);

                Assert.Equal("Empty", parents[0].Name);
                Assert.NotNull(parents[0].Children);
                Assert.Empty(parents[0].Children!);
                Assert.NotNull(parents[0].InitializedChildren);
                Assert.Empty(parents[0].InitializedChildren!);

                Assert.Equal("Null", parents[1].Name);
                Assert.Null(parents[1].Children);
                Assert.Null(parents[1].InitializedChildren);

                switch (queryKind)
                {
                    case NullableJsonCollectionQuery.Tracking:
                        // Querying the tracked entities again doesn't detect a change of the collections
                        Assert.Equal(parents, await query.ToListAsync());
                        Assert.False(context.ChangeTracker.HasChanges());
                        break;
                    case NullableJsonCollectionQuery.ProjectionWithEntity:
                        Assert.False(context.ChangeTracker.HasChanges());
                        break;
                    default:
                        Assert.Empty(context.ChangeTracker.Entries());
                        break;
                }
            });
    }

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Update_of_a_disconnected_entity_saves_the_null_ness_of_a_nullable_json_collection(bool toNull)
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

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
        var contextFactory = await InitializeNullableJsonCollectionTest();

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
    public virtual async Task Mixed_changes_of_nullable_json_collections_are_saved_in_one_batch()
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

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

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Querying_a_tracked_entity_does_not_overwrite_a_pending_change_of_a_nullable_json_collection(bool toNull)
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NullableJsonParent { Name = "A", Children = toNull ? [] : null });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Equal(!toNull, parent.Children is null);

                if (toNull)
                {
                    parent.Children = null;
                }
                else
                {
                    parent.Children = [];
                    context.ChangeTracker.DetectChanges();
                }

                var again = await context.Set<NullableJsonParent>().SingleAsync();

                Assert.Same(parent, again);
                Assert.Equal(toNull, again.Children is null);

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

    [Fact] // Issue #33452
    public virtual async Task Streamed_duplicate_of_a_tracked_entity_does_not_overwrite_a_change_to_a_nullable_json_collection()
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.AddRange(new NullableJsonParent { Name = "A", Children = null }, new NullableJsonParent { Name = "B", Children = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                // The same entity instance is returned twice by the query, with the stored null-ness being read both times.
                var query = from p in context.Set<NullableJsonParent>().Where(p => p.Name == "A")
                            from q in context.Set<NullableJsonParent>()
                            select p;

                var results = 0;
                foreach (var parent in query.AsEnumerable())
                {
                    if (results++ == 0)
                    {
                        Assert.Null(parent.Children);
                        parent.Children = [];
                    }
                    else
                    {
                        Assert.NotNull(parent.Children);
                        Assert.Empty(parent.Children);
                    }
                }

                Assert.Equal(2, results);

                var tracked = context.ChangeTracker.Entries<NullableJsonParent>().Single(e => e.Entity.Name == "A");
                Assert.Equal(EntityState.Modified, tracked.State);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync(p => p.Name == "A");
                Assert.NotNull(parent.Children);
                Assert.Empty(parent.Children);
            });
    }

    [Fact] // Issue #33452
    public virtual async Task Querying_a_tracked_entity_keeps_a_pending_null_of_a_nullable_json_collection_that_is_stored_with_elements()
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

        var id = 0;
        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                var parent = new NullableJsonParent { Name = "A", Children = [new JsonChild { Name = "C1" }, new JsonChild { Name = "C2" }] };
                context.Add(parent);
                await context.SaveChangesAsync();
                id = parent.Id;
            },
            async context =>
            {
                // The entity is tracked before the query with a null collection that is going to be saved, and none of the elements of
                // the stored collection are tracked.
                var tracked = new NullableJsonParent { Id = id, Name = "A", Children = null };
                context.Update(tracked);

                var queried = await context.Set<NullableJsonParent>().SingleAsync();

                Assert.Same(tracked, queried);
                Assert.Null(queried.Children);
                Assert.Equal(EntityState.Modified, context.Entry(queried).State);
                Assert.Empty(context.ChangeTracker.Entries<JsonChild>());

                await context.SaveChangesAsync();

                Assert.Null(queried.Children);
            },
            async context =>
            {
                var parent = await context.Set<NullableJsonParent>().SingleAsync();
                Assert.Null(parent.Children);
            });
    }

    [Theory, InlineData(true), InlineData(false)] // Issue #33452
    public virtual async Task Querying_a_tracked_entity_keeps_the_in_memory_null_ness_of_a_nullable_json_collection(bool inMemoryNull)
    {
        var contextFactory = await InitializeNullableJsonCollectionTest();

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
    public virtual async Task Json_collection_that_is_not_nullable_or_opted_out_stored_as_null_is_loaded_as_empty_with_initializer()
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<NotNullableJsonParent>(b =>
                {
                    b.OwnsMany(p => p.Children, c => c.ToJson());
                    b.OwnsMany(p => p.OptedOutChildren, c => c.ToJson());
                });

                // Opts out of the nullable collection that the nullable reference type annotation would configure
                mb.Model.FindEntityType(typeof(NotNullableJsonParent))!.FindNavigation(
                    nameof(NotNullableJsonParent.OptedOutChildren))!.SetIsNullable(false);
            },
            onConfiguring: IgnoreOwnedEntityMappedToJsonCollectionWarning);

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NotNullableJsonParent { Children = null!, OptedOutChildren = null });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NotNullableJsonParent>().SingleAsync();

                Assert.NotNull(parent.Children);
                Assert.Empty(parent.Children);
                Assert.NotNull(parent.OptedOutChildren);
                Assert.Empty(parent.OptedOutChildren);
                Assert.False(context.ChangeTracker.HasChanges());
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
            },
            onConfiguring: IgnoreOwnedEntityMappedToJsonCollectionWarning);

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

    [Theory, InlineData(ChangeTrackingStrategy.ChangedNotifications), InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications),
     InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
    public virtual async Task Nullable_json_collection_of_a_notifying_entity_changed_from_empty_to_null_is_saved(
        ChangeTrackingStrategy strategy) // Issue #33452
    {
        var contextFactory = await InitializeNotifyingNullableJsonCollectionTest(strategy);

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

    [Theory, InlineData(ChangeTrackingStrategy.ChangedNotifications), InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications),
     InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
    public virtual async Task Nullable_json_collection_of_a_notifying_entity_loaded_by_a_query_is_notified_of_added_and_removed_elements(
        ChangeTrackingStrategy strategy) // Issue #33452
    {
        var contextFactory = await InitializeNotifyingNullableJsonCollectionTest(strategy);

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(
                    new NotifyingNullableJsonParent
                    {
                        Name = "A", Children = [new NotifyingJsonChild { Name = "C1" }, new NotifyingJsonChild { Name = "C2" }]
                    });
                context.Add(new NotifyingNullableJsonParent { Name = "B", Children = [] });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parents = await context.Set<NotifyingNullableJsonParent>().OrderBy(p => p.Name).ToListAsync();

                // The collections were created by the query, and no change notification was raised for them
                parents[0].Children!.Add(new NotifyingJsonChild { Name = "C3" });
                parents[0].Children!.Remove(parents[0].Children!.Single(c => c.Name == "C1"));
                parents[1].Children!.Add(new NotifyingJsonChild { Name = "D1" });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parents = await context.Set<NotifyingNullableJsonParent>().OrderBy(p => p.Name).ToListAsync();

                Assert.Equal(["C2", "C3"], parents[0].Children!.Select(c => c.Name).Order());
                Assert.Equal(["D1"], parents[1].Children!.Select(c => c.Name));
            });
    }

    [Theory, InlineData(ChangeTrackingStrategy.ChangedNotifications), InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    public virtual async Task Nullable_json_collections_of_a_notifying_entity_stored_as_null_stay_null_when_tracked(
        ChangeTrackingStrategy strategy) // Issue #33452
    {
        var contextFactory = await InitializeNotifyingNullableJsonCollectionTest(strategy);

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(
                    new NotifyingNullableJsonParent
                    {
                        Name = "A",
                        Children = null,
                        Outer = [new NotifyingJsonOuterItem { Name = "I1", Inner = null }, new NotifyingJsonOuterItem { Name = "I2", Inner = [] }]
                    });
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var parent = await context.Set<NotifyingNullableJsonParent>().SingleAsync();

                Assert.Null(parent.Children);
                var items = parent.Outer!.OrderBy(i => i.Name).ToList();
                Assert.Null(items[0].Inner);
                Assert.NotNull(items[1].Inner);
                Assert.Empty(items[1].Inner!);
                Assert.False(context.ChangeTracker.HasChanges());

                parent.Name = "B";
                await context.SaveChangesAsync();

                Assert.Null(parent.Children);
            },
            async context =>
            {
                var parent = await context.Set<NotifyingNullableJsonParent>().SingleAsync();

                Assert.Equal("B", parent.Name);
                Assert.Null(parent.Children);
                var items = parent.Outer!.OrderBy(i => i.Name).ToList();
                Assert.Null(items[0].Inner);
                Assert.NotNull(items[1].Inner);
            });
    }

    [Theory, InlineData(ChangeTrackingStrategy.ChangedNotifications), InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    public virtual async Task Nullable_json_collection_of_a_notifying_entity_with_an_initializer_loaded_as_null_has_no_changes(
        ChangeTrackingStrategy strategy) // Issue #33452
    {
        var contextFactory = await InitializeNotifyingNullableJsonCollectionTest(strategy);

        await ExecuteWithStrategyInTransactionAsync(
            contextFactory,
            async context =>
            {
                context.Add(new NotifyingNullableJsonParent { Name = "A" });
                await context.SaveChangesAsync();
                context.Database.ExecuteSql($"UPDATE [NotifyingNullableJsonParent] SET [InitializedChildren] = NULL");
            },
            async context =>
            {
                var parent = await context.Set<NotifyingNullableJsonParent>().SingleAsync();

                Assert.Null(parent.InitializedChildren);
                Assert.False(context.ChangeTracker.HasChanges());
                Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
            });
    }

    /// <summary>
    ///     The model shared by the tests of nullable JSON collections (issue #33452) that don't need a particular entity shape: a root
    ///     collection with and without an initializer, a collection nested in a JSON reference, a collection nested in the elements
    ///     of a JSON collection, a collection declared on a derived type, and a concurrency token.
    /// </summary>
    protected virtual Task<ContextFactory<DbContext>> InitializeNullableJsonCollectionTest()
        => InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.Entity<NullableJsonParent>(b =>
                {
                    b.OwnsMany(p => p.Children, c => c.ToJson());
                    b.OwnsMany(p => p.InitializedChildren, c => c.ToJson());
                    b.OwnsOne(
                        p => p.Reference, r =>
                        {
                            r.ToJson();
                            r.OwnsMany(x => x.Items);
                        });
                    b.OwnsMany(
                        p => p.Outer, o =>
                        {
                            o.ToJson();
                            o.OwnsMany(x => x.Inner);
                        });
                });
                mb.Entity<NullableJsonDerivedParent>().OwnsMany(p => p.DerivedChildren, c => c.ToJson());
            },
            onConfiguring: IgnoreOwnedEntityMappedToJsonCollectionWarning);

    /// <summary>
    ///     The model shared by the tests of nullable JSON collections (issue #33452) on entities that notify of their changes.
    /// </summary>
    protected virtual Task<ContextFactory<DbContext>> InitializeNotifyingNullableJsonCollectionTest(ChangeTrackingStrategy strategy)
        => InitializeNonSharedTest<DbContext>(
            onModelCreating: mb =>
            {
                mb.HasChangeTrackingStrategy(strategy);
                mb.Entity<NotifyingNullableJsonParent>(b =>
                {
                    b.OwnsMany(p => p.Children, c => c.ToJson());
                    b.OwnsMany(p => p.InitializedChildren, c => c.ToJson());
                    b.Navigation(p => p.InitializedChildren).UsePropertyAccessMode(PropertyAccessMode.Property);
                    b.OwnsMany(
                        p => p.Outer, o =>
                        {
                            o.ToJson();
                            o.OwnsMany(i => i.Inner);
                        });
                });
            },
            onConfiguring: IgnoreOwnedEntityMappedToJsonCollectionWarning);

    private static void IgnoreOwnedEntityMappedToJsonCollectionWarning(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(w => w.Ignore(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning));

    private class NullableJsonParent
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        [ConcurrencyCheck]
        public string? Token { get; set; }

        public List<JsonChild>? Children { get; set; }
        public List<JsonChild>? InitializedChildren { get; set; } = [];
        public JsonReference? Reference { get; set; }
        public List<JsonOuterItem>? Outer { get; set; }
    }

    private class NullableJsonDerivedParent : NullableJsonParent
    {
        public List<JsonChild>? DerivedChildren { get; set; }
    }

    private class JsonReference
    {
        public string? Name { get; set; }
        public List<JsonChild>? Items { get; set; }
    }

    private class JsonOuterItem
    {
        public string? Name { get; set; }
        public List<JsonChild>? Inner { get; set; }
    }

    private class JsonChild
    {
        public string? Name { get; set; }
    }

    private class NotNullableJsonParent
    {
        public int Id { get; set; }
        public List<JsonChild> Children { get; set; } = [];
        public List<JsonChild>? OptedOutChildren { get; set; } = [];
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

    private class NotifyingNullableJsonParent : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _id;
        private string? _name;
        private ObservableHashSet<NotifyingJsonChild>? _children;
        private ObservableHashSet<NotifyingJsonChild>? _initializedChildren = [];
        private ObservableHashSet<NotifyingJsonOuterItem>? _outer;

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

        public ObservableHashSet<NotifyingJsonChild>? InitializedChildren
        {
            get => _initializedChildren;
            set => SetValue(ref _initializedChildren, value);
        }

        public ObservableHashSet<NotifyingJsonOuterItem>? Outer
        {
            get => _outer;
            set => SetValue(ref _outer, value);
        }

        private void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private class NotifyingJsonOuterItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private string? _name;
        private ObservableHashSet<NotifyingJsonChild>? _inner;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        public string? Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        public ObservableHashSet<NotifyingJsonChild>? Inner
        {
            get => _inner;
            set => SetValue(ref _inner, value);
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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class AdHocJsonQueryTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "AdHocJsonQueryTests";

    protected virtual void ClearLog()
        => ListLoggerFactory.Clear();

    protected virtual void ConfigureWarnings(WarningsConfigurationBuilder builder)
    {
    }

    #region 21006

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_root_with_missing_scalars(bool async)
    {
        var contextFactory = await InitializeAsync<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id < 4);

        var result = async
            ? await query.ToListAsync()
            : query.ToList()!;

        var topLevel = result.Single(x => x.Id == 2);
        var nested = result.Single(x => x.Id == 3);

        Assert.Equal(default, topLevel.OptionalReference.Number);
        Assert.Equal(default, topLevel.RequiredReference.Number);
        Assert.True(topLevel.Collection.All(x => x.Number == default));

        Assert.Equal(default, nested.RequiredReference.NestedRequiredReference.DoB);
        Assert.Equal(default, nested.RequiredReference.NestedOptionalReference.DoB);
        Assert.Equal(default, nested.OptionalReference.NestedRequiredReference.DoB);
        Assert.Equal(default, nested.OptionalReference.NestedOptionalReference.DoB);
        Assert.True(nested.Collection.SelectMany(x => x.NestedCollection).All(x => x.DoB == default));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_top_level_json_entity_with_missing_scalars(bool async)
    {
        var contextFactory = await InitializeAsync<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id < 4).Select(x => new
        {
            x.Id,
            x.OptionalReference,
            x.RequiredReference,
            x.Collection
        }).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var topLevel = result.Single(x => x.Id == 2);
        var nested = result.Single(x => x.Id == 3);

        Assert.Equal(default, topLevel.OptionalReference.Number);
        Assert.Equal(default, topLevel.RequiredReference.Number);
        Assert.True(topLevel.Collection.All(x => x.Number == default));

        Assert.Equal(default, nested.RequiredReference.NestedRequiredReference.DoB);
        Assert.Equal(default, nested.RequiredReference.NestedOptionalReference.DoB);
        Assert.Equal(default, nested.OptionalReference.NestedRequiredReference.DoB);
        Assert.Equal(default, nested.OptionalReference.NestedOptionalReference.DoB);
        Assert.True(nested.Collection.SelectMany(x => x.NestedCollection).All(x => x.DoB == default));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_nested_json_entity_with_missing_scalars(bool async)
    {
        var contextFactory = await InitializeAsync<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id < 4).Select(x => new
        {
            x.Id,
            x.OptionalReference.NestedOptionalReference,
            x.RequiredReference.NestedRequiredReference,
            x.Collection[0].NestedCollection
        }).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var topLevel = result.Single(x => x.Id == 2);
        var nested = result.Single(x => x.Id == 3);

        Assert.Equal(default, nested.NestedOptionalReference.DoB);
        Assert.Equal(default, nested.NestedRequiredReference.DoB);
        Assert.True(nested.NestedCollection.All(x => x.DoB == default));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_top_level_entity_with_null_value_required_scalars(bool async)
    {
        var contextFactory = await InitializeAsync<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id == 4).Select(x => new
        {
            x.Id,
            x.RequiredReference,
        }).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var nullScalars = result.Single();

        Assert.Equal(default, nullScalars.RequiredReference.Number);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_root_entity_with_missing_required_navigation(bool async)
    {
        var contextFactory = await InitializeAsync<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id == 5).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var missingRequiredNav = result.Single();

        Assert.Equal(default, missingRequiredNav.RequiredReference.NestedRequiredReference);
        Assert.Equal(default, missingRequiredNav.OptionalReference.NestedRequiredReference);
        Assert.True(missingRequiredNav.Collection.All(x => x.NestedRequiredReference == default));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_missing_required_navigation(bool async)
    {
        var contextFactory = await InitializeAsync<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id == 5).Select(x => x.RequiredReference.NestedRequiredReference).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var missingRequiredNav = result.Single();

        Assert.Equal(default, missingRequiredNav);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_root_entity_with_null_required_navigation(bool async)
    {
        var contextFactory = await InitializeAsync<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id == 6).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var nullRequiredNav = result.Single();

        Assert.Equal(default, nullRequiredNav.RequiredReference.NestedRequiredReference);
        Assert.Equal(default, nullRequiredNav.OptionalReference.NestedRequiredReference);
        Assert.True(nullRequiredNav.Collection.All(x => x.NestedRequiredReference == default));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_null_required_navigation(bool async)
    {
        var contextFactory = await InitializeAsync<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context21006.Entity>().Where(x => x.Id == 6).Select(x => x.RequiredReference).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var nullRequiredNav = result.Single();

        Assert.Equal(default, nullRequiredNav.NestedRequiredReference);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_missing_required_scalar(bool async)
    {
        var contextFactory = await InitializeAsync<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context21006.Entity>()
            .Where(x => x.Id == 2)
            .Select(x => new
            {
                x.Id,
                Number = (double?)x.RequiredReference.Number
            });

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Null(result.Single().Number);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_null_required_scalar(bool async)
    {
        var contextFactory = await InitializeAsync<Context21006>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating21006,
            seed: Seed21006);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context21006.Entity>()
            .Where(x => x.Id == 4)
            .Select(x => new
            {
                x.Id,
                Number = (double?)x.RequiredReference.Number,
            });

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Null(result.Single().Number);
    }

    protected virtual void OnModelCreating21006(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context21006.Entity>(
            b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsOne(
                    x => x.OptionalReference, bb =>
                    {
                        bb.OwnsOne(x => x.NestedOptionalReference);
                        bb.OwnsOne(x => x.NestedRequiredReference);
                        bb.Navigation(x => x.NestedRequiredReference).IsRequired();
                        bb.OwnsMany(x => x.NestedCollection);
                    });
                b.OwnsOne(
                    x => x.RequiredReference, bb =>
                    {
                        bb.OwnsOne(x => x.NestedOptionalReference);
                        bb.OwnsOne(x => x.NestedRequiredReference);
                        bb.Navigation(x => x.NestedRequiredReference).IsRequired();
                        bb.OwnsMany(x => x.NestedCollection);
                    });
                b.Navigation(x => x.RequiredReference).IsRequired();
                b.OwnsMany(
                    x => x.Collection, bb =>
                    {
                        bb.OwnsOne(x => x.NestedOptionalReference);
                        bb.OwnsOne(x => x.NestedRequiredReference);
                        bb.Navigation(x => x.NestedRequiredReference).IsRequired();
                        bb.OwnsMany(x => x.NestedCollection);
                    });
            });

    protected virtual async Task Seed21006(Context21006 context)
    {
        // everything
        var e1 = new Context21006.Entity
        {
            Id = 1,
            Name = "e1",
            OptionalReference = new Context21006.JsonEntity
            {
                Number = 7,
                Text = "e1 or",
                NestedOptionalReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 or nor" },
                NestedRequiredReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 or nrr" },
                NestedCollection = new List<Context21006.JsonEntityNested>
                {
                    new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 or c1" },
                    new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 or c2" },
                }
            },

            RequiredReference = new Context21006.JsonEntity
            {
                Number = 7,
                Text = "e1 rr",
                NestedOptionalReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 rr nor" },
                NestedRequiredReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 rr nrr" },
                NestedCollection = new List<Context21006.JsonEntityNested>
                {
                    new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 rr c1" },
                    new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 rr c2" },
                }
            },
            Collection = new List<Context21006.JsonEntity>
            {
                new Context21006.JsonEntity
                {
                    Number = 7,
                    Text = "e1 c1",
                    NestedOptionalReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c1 nor" },
                    NestedRequiredReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c1 nrr" },
                    NestedCollection = new List<Context21006.JsonEntityNested>
                    {
                        new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c1 c1" },
                        new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c1 c2" },
                    }
                },
                new Context21006.JsonEntity
                {
                    Number = 7,
                    Text = "e1 c2",
                    NestedOptionalReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c2 nor" },
                    NestedRequiredReference = new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c2 nrr" },
                    NestedCollection = new List<Context21006.JsonEntityNested>
                    {
                        new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c2 c1" },
                        new Context21006.JsonEntityNested { DoB = new DateTime(2000, 1, 1), Text = "e1 c2 c2" },
                    }
                },
            }
        };

        context.Add(e1);
        await context.SaveChangesAsync();
    }

    protected class Context21006(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }

        public class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public JsonEntity OptionalReference { get; set; }
            public JsonEntity RequiredReference { get; set; }
            public List<JsonEntity> Collection { get; set; }
        }

        public class JsonEntity
        {
            public string Text { get; set; }
            public double Number { get; set; }

            public JsonEntityNested NestedOptionalReference { get; set; }
            public JsonEntityNested NestedRequiredReference { get; set; }
            public List<JsonEntityNested> NestedCollection { get; set; }
        }

        public class JsonEntityNested
        {
            public DateTime DoB { get; set; }
            public string Text { get; set; }
        }
    }

    #endregion

    #region 32310

    [ConditionalFact]
    public virtual async Task Contains_on_nested_collection_with_init_only_navigation()
    {
        var contextFactory = await InitializeAsync<Context32310>(
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            onModelCreating: OnModelCreating32310,
            seed: Seed32310);

        await using var context = contextFactory.CreateContext();

        var query = context.Set<Context32310.Pub>()
            .Where(u => u.Visits.DaysVisited.Contains(new DateOnly(2023, 1, 1)));

        var result = await query.FirstOrDefaultAsync();

        Assert.Equal("FBI", result.Name);
        Assert.Equal(new DateOnly(2023, 1, 1), result.Visits.DaysVisited.Single());
    }

    protected virtual void OnModelCreating32310(ModelBuilder modelBuilder)
       => modelBuilder.Entity<Context32310.Pub>().OwnsOne(e => e.Visits);

    protected virtual async Task Seed32310(DbContext context)
    {
        var user = new Context32310.Pub
        {
            Name = "FBI",
            Visits = new Context32310.Visits { LocationTag = "tag", DaysVisited = [new DateOnly(2023, 1, 1)] }
        };

        context.Add(user);
        await context.SaveChangesAsync();
    }

    protected class Context32310(DbContextOptions options) : DbContext(options)
    {
        public class Pub
        {
            public int Id { get; set; }
            public required string Name { get; set; }
            public Visits Visits { get; set; } = null!;
        }

        public class Visits
        {
            public string LocationTag { get; set; }
            public required List<DateOnly> DaysVisited { get; init; }
        }
    }

    #endregion

    #region 29219

    [ConditionalFact]
    public virtual async Task Optional_json_properties_materialized_as_null_when_the_element_in_json_is_not_present()
    {
        var contextFactory = await InitializeAsync<Context29219>(
            onModelCreating: OnModelCreating29219,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed29219);

        using var context = contextFactory.CreateContext();
        var query = context.Set<Context29219.MyEntity>().Where(x => x.Id == 3);
        var result = await query.SingleAsync();

        Assert.Equal(3, result.Id);
        Assert.Null(result.Reference.NullableScalar);
        Assert.Null(result.Collection[0].NullableScalar);
    }

    [ConditionalFact]
    public virtual async Task Can_project_nullable_json_property_when_the_element_in_json_is_not_present()
    {
        var contextFactory = await InitializeAsync<Context29219>(
            onModelCreating: OnModelCreating29219,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed29219);

        using var context = contextFactory.CreateContext();

        var query = context.Set<Context29219.MyEntity>().OrderBy(x => x.Id).Select(x => x.Reference.NullableScalar);
        var result = await query.ToListAsync();

        Assert.Equal(3, result.Count);
        Assert.Equal(11, result[0]);
        Assert.Null(result[1]);
        Assert.Null(result[2]);
    }

    protected virtual void OnModelCreating29219(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context29219.MyEntity>(
            b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsOne(x => x.Reference);
                b.OwnsMany(x => x.Collection);
            });

    protected virtual async Task Seed29219(DbContext ctx)
    {
        var entity1 = new Context29219.MyEntity
        {
            Id = 1,
            Reference = new Context29219.MyJsonEntity { NonNullableScalar = 10, NullableScalar = 11 },
            Collection =
            [
                new Context29219.MyJsonEntity { NonNullableScalar = 100, NullableScalar = 101 },
                new Context29219.MyJsonEntity { NonNullableScalar = 200, NullableScalar = 201 },
                new Context29219.MyJsonEntity { NonNullableScalar = 300, NullableScalar = null }
            ]
        };

        var entity2 = new Context29219.MyEntity
        {
            Id = 2,
            Reference = new Context29219.MyJsonEntity { NonNullableScalar = 20, NullableScalar = null },
            Collection = [new Context29219.MyJsonEntity { NonNullableScalar = 1001, NullableScalar = null }]
        };

        ctx.AddRange(entity1, entity2);
        await ctx.SaveChangesAsync();
    }

    protected class Context29219(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public MyJsonEntity Reference { get; set; }
            public List<MyJsonEntity> Collection { get; set; }
        }

        public class MyJsonEntity
        {
            public int NonNullableScalar { get; set; }
            public int? NullableScalar { get; set; }
        }
    }

    #endregion

    #region 30028

    [ConditionalFact]
    public virtual async Task Accessing_missing_navigation_works()
    {
        var contextFactory = await InitializeAsync<Context30028>(
            onModelCreating: OnModelCreating30028,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed30028);

        using (var context = contextFactory.CreateContext())
        {
            var result = await context.Set<Context30028.MyEntity>().OrderBy(x => x.Id).ToListAsync();
            Assert.Equal(4, result.Count);
            Assert.NotNull(result[0].Json.Collection);
            Assert.NotNull(result[0].Json.OptionalReference);
            Assert.NotNull(result[0].Json.RequiredReference);

            Assert.Null(result[1].Json.Collection);
            Assert.NotNull(result[1].Json.OptionalReference);
            Assert.NotNull(result[1].Json.RequiredReference);

            Assert.NotNull(result[2].Json.Collection);
            Assert.Null(result[2].Json.OptionalReference);
            Assert.NotNull(result[2].Json.RequiredReference);

            Assert.NotNull(result[3].Json.Collection);
            Assert.NotNull(result[3].Json.OptionalReference);
            Assert.Null(result[3].Json.RequiredReference);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Missing_navigation_works_with_deduplication(bool async)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: OnModelCreating30028,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed30028);

        using (var context = contextFactory.CreateContext())
        {
            var queryable = context.Set<Context30028.MyEntity>().OrderBy(x => x.Id).Select(
                x => new
                {
                    x,
                    x.Json,
                    x.Json.OptionalReference,
                    x.Json.RequiredReference,
                    NestedOptional = x.Json.OptionalReference.Nested,
                    NestedRequired = x.Json.RequiredReference.Nested,
                    x.Json.Collection,
                }).AsNoTracking();

            var result = async ? await queryable.ToListAsync() : queryable.ToList();

            Assert.Equal(4, result.Count);
            Assert.NotNull(result[0].OptionalReference);
            Assert.NotNull(result[0].RequiredReference);
            Assert.NotNull(result[0].NestedOptional);
            Assert.NotNull(result[0].NestedRequired);
            Assert.NotNull(result[0].Collection);

            Assert.NotNull(result[1].OptionalReference);
            Assert.NotNull(result[1].RequiredReference);
            Assert.NotNull(result[1].NestedOptional);
            Assert.NotNull(result[1].NestedRequired);
            Assert.Null(result[1].Collection);

            Assert.Null(result[2].OptionalReference);
            Assert.NotNull(result[2].RequiredReference);
            Assert.Null(result[2].NestedOptional);
            Assert.NotNull(result[2].NestedRequired);
            Assert.NotNull(result[2].Collection);

            Assert.NotNull(result[3].OptionalReference);
            Assert.Null(result[3].RequiredReference);
            Assert.NotNull(result[3].NestedOptional);
            Assert.Null(result[3].NestedRequired);
            Assert.NotNull(result[3].Collection);
        }
    }

    protected virtual void OnModelCreating30028(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context30028.MyEntity>(
            b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsOne(
                    x => x.Json, nb =>
                    {
                        nb.OwnsMany(x => x.Collection, nnb => nnb.OwnsOne(x => x.Nested));
                        nb.OwnsOne(x => x.OptionalReference, nnb => nnb.OwnsOne(x => x.Nested));
                        nb.OwnsOne(x => x.RequiredReference, nnb => nnb.OwnsOne(x => x.Nested));
                        nb.Navigation(x => x.RequiredReference).IsRequired();
                    });
            });

    protected abstract Task Seed30028(DbContext ctx);

    protected class Context30028(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public MyJsonRootEntity Json { get; set; }
        }

        public class MyJsonRootEntity
        {
            public string RootName { get; set; }
            public MyJsonBranchEntity RequiredReference { get; set; }
            public MyJsonBranchEntity OptionalReference { get; set; }
            public List<MyJsonBranchEntity> Collection { get; set; }
        }

        public class MyJsonBranchEntity
        {
            public string BranchName { get; set; }
            public MyJsonLeafEntity Nested { get; set; }
        }

        public class MyJsonLeafEntity
        {
            public string LeafName { get; set; }
        }
    }

    #endregion

    #region 32939

    [ConditionalFact]
    public virtual async Task Project_json_with_no_properties()
    {
        var contextFactory = await InitializeAsync<Context32939>(
            onModelCreating: OnModelCreating32939,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed32939);

        using var context = contextFactory.CreateContext();
        await context.Set<Context32939.Entity>().ToListAsync();
    }

    protected virtual void OnModelCreating32939(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Context32939.Entity>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Context32939.Entity>().OwnsOne(x => x.Empty);
        modelBuilder.Entity<Context32939.Entity>().OwnsOne(x => x.FieldOnly);
    }

    protected Task Seed32939(DbContext ctx)
    {
        var entity = new Context32939.Entity { Empty = new Context32939.JsonEmpty(), FieldOnly = new Context32939.JsonFieldOnly() };

        ctx.Add(entity);
        return ctx.SaveChangesAsync();
    }

    protected class Context32939(DbContextOptions options) : DbContext(options)
    {
        public class Entity
        {
            public int Id { get; set; }
            public JsonEmpty Empty { get; set; }
            public JsonFieldOnly FieldOnly { get; set; }
        }

        public class JsonEmpty
        {
        }

        public class JsonFieldOnly
        {
            public int Field;
        }
    }

    #endregion

    #region 33046

    [ConditionalFact]
    public virtual async Task Query_with_nested_json_collection_mapped_to_private_field_via_IReadOnlyList()
    {
        var contextFactory = await InitializeAsync<Context33046>(
            onModelCreating: OnModelCreating33046,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed33046);

        using var context = contextFactory.CreateContext();
        var query = context.Set<Context33046.Review>().ToList();
        Assert.Equal(1, query.Count);
    }

    protected virtual void OnModelCreating33046(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context33046.Review>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsMany(x => x.Rounds, ownedBuilder =>
                {
                    ownedBuilder.OwnsMany(r => r.SubRounds);
                });
            });

    protected abstract Task Seed33046(DbContext ctx);

    protected class Context33046(DbContextOptions options) : DbContext(options)
    {
        public class Review
        {
            public int Id { get; set; }

#pragma warning disable IDE0044 // Add readonly modifier
            private List<ReviewRound> _rounds = [];
#pragma warning restore IDE0044 // Add readonly modifier
            public IReadOnlyList<ReviewRound> Rounds
                => _rounds.AsReadOnly();
        }

        public class ReviewRound
        {
            public int RoundNumber { get; set; }

#pragma warning disable IDE0044 // Add readonly modifier
            private readonly List<SubRound> _subRounds = [];
#pragma warning restore IDE0044 // Add readonly modifier
            public IReadOnlyList<SubRound> SubRounds
                => _subRounds.AsReadOnly();
        }

        public class SubRound
        {
            public int SubRoundNumber { get; set; }
        }
    }

    #endregion

    #region 34960

    [ConditionalFact]
    public virtual async Task Project_entity_with_json_null_values()
    {
        var contextFactory = await InitializeAsync<Context34960>(seed: Seed34960, onModelCreating: OnModelCreating34960);

        using var context = contextFactory.CreateContext();
        var query = await context.Entities.ToListAsync();
    }

    [ConditionalFact]
    public virtual async Task Try_project_collection_but_JSON_is_entity()
    {
        var contextFactory = await InitializeAsync<Context34960>(seed: Seed34960, onModelCreating: OnModelCreating34960);
        using var context = contextFactory.CreateContext();

        Assert.Equal(
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => context.Junk.AsNoTracking().Where(x => x.Id == 1).Select(x => x.Collection).FirstOrDefaultAsync())).Message,
            CoreStrings.JsonReaderInvalidTokenType(nameof(JsonTokenType.StartObject)));
    }

    [ConditionalFact]
    public virtual async Task Try_project_reference_but_JSON_is_collection()
    {
        var contextFactory = await InitializeAsync<Context34960>(seed: Seed34960, onModelCreating: OnModelCreating34960);
        using var context = contextFactory.CreateContext();

        Assert.Equal(
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => context.Junk.AsNoTracking().Where(x => x.Id == 2).Select(x => x.Reference).FirstOrDefaultAsync())).Message,
            CoreStrings.JsonReaderInvalidTokenType(nameof(JsonTokenType.StartArray)));
    }

    protected class Context34960(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }
        public DbSet<JunkEntity> Junk { get; set; }

        public class Entity
        {
            public int Id { get; set; }
            public JsonEntity Reference { get; set; }
            public List<JsonEntity> Collection { get; set; }
        }

        public class JsonEntity
        {
            public string Name { get; set; }
            public double Number { get; set; }

            public JsonEntityNested NestedReference { get; set; }
            public List<JsonEntityNested> NestedCollection { get; set; }
        }

        public class JsonEntityNested
        {
            public DateTime DoB { get; set; }
            public string Text { get; set; }
        }

        public class JunkEntity
        {
            public int Id { get; set; }
            public JsonEntity Reference { get; set; }
            public List<JsonEntity> Collection { get; set; }
        }
    }

    protected virtual void OnModelCreating34960(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Context34960.Entity>(
            b =>
            {
                //b.ToTable("Entities");
                b.Property(x => x.Id).ValueGeneratedNever();

                b.OwnsOne(
                    x => x.Reference, b =>
                    {
                        //b.ToJson().HasColumnType(JsonColumnType);
                        b.OwnsOne(x => x.NestedReference);
                        b.OwnsMany(x => x.NestedCollection);
                    });

                b.OwnsMany(
                    x => x.Collection, b =>
                    {
                        //b.ToJson().HasColumnType(JsonColumnType);
                        b.OwnsOne(x => x.NestedReference);
                        b.OwnsMany(x => x.NestedCollection);
                    });
            });

        modelBuilder.Entity<Context34960.JunkEntity>(
            b =>
            {
                //b.ToTable("Junk");
                b.Property(x => x.Id).ValueGeneratedNever();

                b.OwnsOne(
                    x => x.Reference, b =>
                    {
                        //b.ToJson().HasColumnType(JsonColumnType);
                        b.Ignore(x => x.NestedReference);
                        b.Ignore(x => x.NestedCollection);
                    });

                b.OwnsMany(
                    x => x.Collection, b =>
                    {
                        //b.ToJson().HasColumnType(JsonColumnType);
                        b.Ignore(x => x.NestedReference);
                        b.Ignore(x => x.NestedCollection);
                    });
            });
    }

    protected virtual async Task Seed34960(Context34960 ctx)
    {
        // everything
        var e1 = new Context34960.Entity
        {
            Id = 1,
            Reference = new Context34960.JsonEntity
            {
                Name = "ref1",
                Number = 1.5f,
                NestedReference = new Context34960.JsonEntityNested
                {
                    DoB = new DateTime(2000, 1, 1),
                    Text = "nested ref 1"
                },
                NestedCollection =
                [
                    new Context34960.JsonEntityNested
                    {
                        DoB = new DateTime(2001, 1, 1),
                        Text = "nested col 1 1"
                    },
                    new Context34960.JsonEntityNested
                    {
                        DoB = new DateTime(2001, 2, 2),
                        Text = "nested col 1 2"
                    },
                ],
            },

            Collection =
            [
                new Context34960.JsonEntity
                {
                    Name = "col 1 1",
                    Number = 2.5f,
                    NestedReference = new Context34960.JsonEntityNested
                    {
                        DoB = new DateTime(2010, 1, 1),
                        Text = "nested col 1 1 ref 1"
                    },
                    NestedCollection =
                    [
                        new Context34960.JsonEntityNested
                        {
                            DoB = new DateTime(2011, 1, 1),
                            Text = "nested col 1 1 col 1 1"
                        },
                        new Context34960.JsonEntityNested
                        {
                            DoB = new DateTime(2011, 2, 2),
                            Text = "nested col 1 1 col 1 2"
                        },
                    ],
                },
                new Context34960.JsonEntity
                {
                    Name = "col 1 2",
                    Number = 2.5f,
                    NestedReference = new Context34960.JsonEntityNested
                    {
                        DoB = new DateTime(2020, 1, 1),
                        Text = "nested col 1 2 ref 1"
                    },
                    NestedCollection =
                    [
                        new Context34960.JsonEntityNested
                        {
                            DoB = new DateTime(2021, 1, 1),
                            Text = "nested col 1 2 col 1 1"
                        },
                        new Context34960.JsonEntityNested
                        {
                            DoB = new DateTime(2021, 2, 2),
                            Text = "nested col 1 2 col 1 2"
                        },
                    ],
                },
            ],
        };

        // relational nulls
        var e2 = new Context34960.Entity
        {
            Id = 2,
            Reference = null,
            Collection = null
        };

        // nested relational nulls
        var e3 = new Context34960.Entity
        {
            Id = 3,
            Reference = new Context34960.JsonEntity
            {
                Name = "ref3",
                Number = 3.5f,
                NestedReference = null,
                NestedCollection = null
            },

            Collection =
            [
                new Context34960.JsonEntity
                {
                    Name = "col 3 1",
                    Number = 32.5f,
                    NestedReference = null,
                    NestedCollection = null,
                },
                new Context34960.JsonEntity
                {
                    Name = "col 3 2",
                    Number = 33.5f,
                    NestedReference = null,
                    NestedCollection = null,
                },
            ],
        };

        ctx.Entities.AddRange(e1, e2, e3);
        await ctx.SaveChangesAsync();
    }

    #endregion
























}

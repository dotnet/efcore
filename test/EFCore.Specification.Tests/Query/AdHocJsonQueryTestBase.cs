// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class AdHocJsonQueryTestBase(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
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
        => modelBuilder.Entity<Context21006.Entity>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsOne(x => x.OptionalReference, bb =>
                {
                    bb.OwnsOne(x => x.NestedOptionalReference);
                    bb.OwnsOne(x => x.NestedRequiredReference);
                    bb.Navigation(x => x.NestedRequiredReference).IsRequired();
                    bb.OwnsMany(x => x.NestedCollection);
                });
                b.OwnsOne(x => x.RequiredReference, bb =>
                {
                    bb.OwnsOne(x => x.NestedOptionalReference);
                    bb.OwnsOne(x => x.NestedRequiredReference);
                    bb.Navigation(x => x.NestedRequiredReference).IsRequired();
                    bb.OwnsMany(x => x.NestedCollection);
                });
                b.Navigation(x => x.RequiredReference).IsRequired();
                b.OwnsMany(x => x.Collection, bb =>
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
        => modelBuilder.Entity<Context29219.MyEntity>(b =>
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

        using var context = contextFactory.CreateContext();
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Missing_navigation_works_with_deduplication(bool async)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: OnModelCreating30028,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: Seed30028);

        using var context = contextFactory.CreateContext();
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

    protected virtual void OnModelCreating30028(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Context30028.MyEntity>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsOne(x => x.Json, nb =>
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
        var query = await context.Set<Context33046.Review>().ToListAsync();
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

        await context.Junk.AsNoTracking().Where(x => x.Id == 1).Select(x => x.Collection).FirstOrDefaultAsync();
    }

    [ConditionalFact]
    public virtual async Task Try_project_reference_but_JSON_is_collection()
    {
        var contextFactory = await InitializeAsync<Context34960>(seed: Seed34960, onModelCreating: OnModelCreating34960);
        using var context = contextFactory.CreateContext();

        await context.Junk.AsNoTracking().Where(x => x.Id == 2).Select(x => x.Reference).FirstOrDefaultAsync();
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
        modelBuilder.Entity<Context34960.Entity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();

            b.OwnsOne(x => x.Reference, b =>
            {
                b.OwnsOne(x => x.NestedReference);
                b.OwnsMany(x => x.NestedCollection);
            });

            b.OwnsMany(x => x.Collection, b =>
            {
                b.OwnsOne(x => x.NestedReference);
                b.OwnsMany(x => x.NestedCollection);
            });
        });

        modelBuilder.Entity<Context34960.JunkEntity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();

            b.OwnsOne(x => x.Reference, b =>
            {
                b.Ignore(x => x.NestedReference);
                b.Ignore(x => x.NestedCollection);
            });

            b.OwnsMany(x => x.Collection, b =>
            {
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

    #region ArrayOfPrimitives

    [ConditionalFact]
    public virtual async Task Project_json_array_of_primitives_on_reference()
    {
        var contextFactory = await InitializeAsync<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>().OrderBy(x => x.Id)
            .Select(x => new { x.Reference.IntArray, x.Reference.ListOfString });

        var result = await query.ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].IntArray.Length);
        Assert.Equal(3, result[0].ListOfString.Count);
        Assert.Equal(3, result[1].IntArray.Length);
        Assert.Equal(3, result[1].ListOfString.Count);
    }

    [ConditionalFact(Skip = "Issue #32611")]
    public virtual async Task Project_json_array_of_primitives_on_collection()
    {
        var contextFactory = await InitializeAsync<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>().OrderBy(x => x.Id)
            .Select(x => new { x.Collection[0].IntArray, x.Collection[1].ListOfString });

        var result = await query.ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].IntArray.Length);
        Assert.Equal(2, result[0].ListOfString.Count);
        Assert.Equal(3, result[1].IntArray.Length);
        Assert.Equal(2, result[1].ListOfString.Count);
    }

    [ConditionalFact]
    public virtual async Task Project_element_of_json_array_of_primitives()
    {
        var contextFactory = await InitializeAsync<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>().OrderBy(x => x.Id).Select(
            x => new { ArrayElement = x.Reference.IntArray[0], ListElement = x.Reference.ListOfString[1] });
        var result = await query.ToListAsync();
    }

    [ConditionalFact]
    public virtual async Task Predicate_based_on_element_of_json_array_of_primitives1()
    {
        var contextFactory = await InitializeAsync<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>().Where(x => x.Reference.IntArray[0] == 1);
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(1, result[0].Reference.IntArray[0]);
    }

    [ConditionalFact]
    public virtual async Task Predicate_based_on_element_of_json_array_of_primitives2()
    {
        var contextFactory = await InitializeAsync<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>().Where(x => x.Reference.ListOfString[1] == "Bar");
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal("Bar", result[0].Reference.ListOfString[1]);
    }

    [ConditionalFact]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Predicate_based_on_element_of_json_array_of_primitives3()
    {
        var contextFactory = await InitializeAsync<ContextArrayOfPrimitives>(
            onModelCreating: OnModelCreatingArrayOfPrimitives,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedArrayOfPrimitives);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextArrayOfPrimitives.MyEntity>()
            .Where(x => x.Reference.IntArray.AsQueryable().ElementAt(0) == 1
                || x.Reference.ListOfString.AsQueryable().ElementAt(1) == "Bar")
            .OrderBy(e => e.Id);
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(1, result[0].Reference.IntArray[0]);
        Assert.Equal("Bar", result[0].Reference.ListOfString[1]);
    }

    protected Task SeedArrayOfPrimitives(DbContext ctx)
    {
        var entity1 = new ContextArrayOfPrimitives.MyEntity
        {
            Id = 1,
            Reference = new ContextArrayOfPrimitives.MyJsonEntity
            {
                IntArray = [1, 2, 3],
                ListOfString =
                [
                    "Foo",
                    "Bar",
                    "Baz"
                ]
            },
            Collection =
            [
                new ContextArrayOfPrimitives.MyJsonEntity { IntArray = [111, 112, 113], ListOfString = ["Foo11", "Bar11"] },
                new ContextArrayOfPrimitives.MyJsonEntity { IntArray = [211, 212, 213], ListOfString = ["Foo12", "Bar12"] }
            ]
        };

        var entity2 = new ContextArrayOfPrimitives.MyEntity
        {
            Id = 2,
            Reference = new ContextArrayOfPrimitives.MyJsonEntity
            {
                IntArray = [10, 20, 30],
                ListOfString =
                [
                    "A",
                    "B",
                    "C"
                ]
            },
            Collection =
            [
                new ContextArrayOfPrimitives.MyJsonEntity { IntArray = [110, 120, 130], ListOfString = ["A1", "Z1"] },
                new ContextArrayOfPrimitives.MyJsonEntity { IntArray = [210, 220, 230], ListOfString = ["A2", "Z2"] }
            ]
        };

        ctx.Set<ContextArrayOfPrimitives.MyEntity>().AddRange(entity1, entity2);

        return ctx.SaveChangesAsync();
    }

    protected virtual void OnModelCreatingArrayOfPrimitives(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContextArrayOfPrimitives.MyEntity>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<ContextArrayOfPrimitives.MyEntity>().OwnsOne(x => x.Reference);
        modelBuilder.Entity<ContextArrayOfPrimitives.MyEntity>().OwnsMany(x => x.Collection);
    }

    protected class ContextArrayOfPrimitives(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public MyJsonEntity Reference { get; set; }
            public List<MyJsonEntity> Collection { get; set; }
        }

        public class MyJsonEntity
        {
            public int[] IntArray { get; set; }
            public List<string> ListOfString { get; set; }
        }
    }

    #endregion

    #region JunkInJson

    [ConditionalFact]
    public virtual async Task Junk_in_json_basic_tracking()
    {
        var contextFactory = await InitializeAsync<ContextJunkInJson>(
            onModelCreating: OnModelCreatingJunkInJson,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedJunkInJson);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextJunkInJson.MyEntity>();
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(2, result[0].Collection.Count);
        Assert.Equal(2, result[0].CollectionWithCtor.Count);
        Assert.Equal(2, result[0].Reference.NestedCollection.Count);
        Assert.NotNull(result[0].Reference.NestedReference);
        Assert.Equal(2, result[0].ReferenceWithCtor.NestedCollection.Count);
        Assert.NotNull(result[0].ReferenceWithCtor.NestedReference);
    }

    [ConditionalFact]
    public virtual async Task Junk_in_json_basic_no_tracking()
    {
        var contextFactory = await InitializeAsync<ContextJunkInJson>(
            onModelCreating: OnModelCreatingJunkInJson,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedJunkInJson);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextJunkInJson.MyEntity>().AsNoTracking();
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(2, result[0].Collection.Count);
        Assert.Equal(2, result[0].CollectionWithCtor.Count);
        Assert.Equal(2, result[0].Reference.NestedCollection.Count);
        Assert.NotNull(result[0].Reference.NestedReference);
        Assert.Equal(2, result[0].ReferenceWithCtor.NestedCollection.Count);
        Assert.NotNull(result[0].ReferenceWithCtor.NestedReference);
    }

    protected abstract Task SeedJunkInJson(DbContext ctx);

    protected virtual void OnModelCreatingJunkInJson(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextJunkInJson.MyEntity>(
            b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();

                b.OwnsOne(x => x.Reference, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });

                b.OwnsOne(x => x.ReferenceWithCtor, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });

                b.OwnsMany(x => x.Collection, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });

                b.OwnsMany(x => x.CollectionWithCtor, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });
            });

    protected class ContextJunkInJson(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public MyJsonEntity Reference { get; set; }
            public MyJsonEntityWithCtor ReferenceWithCtor { get; set; }
            public List<MyJsonEntity> Collection { get; set; }
            public List<MyJsonEntityWithCtor> CollectionWithCtor { get; set; }
        }

        public class MyJsonEntity
        {
            public string Name { get; set; }
            public double Number { get; set; }

            public MyJsonEntityNested NestedReference { get; set; }
            public List<MyJsonEntityNested> NestedCollection { get; set; }
        }

        public class MyJsonEntityNested
        {
            public DateTime DoB { get; set; }
        }

        public class MyJsonEntityWithCtor(bool myBool, string name)
        {
            public bool MyBool { get; set; } = myBool;
            public string Name { get; set; } = name;

            public MyJsonEntityWithCtorNested NestedReference { get; set; }
            public List<MyJsonEntityWithCtorNested> NestedCollection { get; set; }
        }

        public class MyJsonEntityWithCtorNested(DateTime doB)
        {
            public DateTime DoB { get; set; } = doB;
        }
    }

    #endregion

    #region TrickyBuffering

    [ConditionalFact]
    public virtual async Task Tricky_buffering_basic()
    {
        var contextFactory = await InitializeAsync<ContextTrickyBuffering>(
            onModelCreating: OnModelCreatingTrickyBuffering,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedTrickyBuffering);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextTrickyBuffering.MyEntity>();
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal("r1", result[0].Reference.Name);
        Assert.Equal(7, result[0].Reference.Number);
        Assert.Equal(new DateTime(2000, 1, 1), result[0].Reference.NestedReference.DoB);
        Assert.Equal(2, result[0].Reference.NestedCollection.Count);
    }

    protected abstract Task SeedTrickyBuffering(DbContext ctx);

    protected virtual void OnModelCreatingTrickyBuffering(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextTrickyBuffering.MyEntity>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsOne(x => x.Reference, b =>
                {
                    b.OwnsOne(x => x.NestedReference);
                    b.OwnsMany(x => x.NestedCollection);
                });
            });

    protected class ContextTrickyBuffering(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public MyJsonEntity Reference { get; set; }
        }

        public class MyJsonEntity
        {
            public string Name { get; set; }
            public int Number { get; set; }
            public MyJsonEntityNested NestedReference { get; set; }
            public List<MyJsonEntityNested> NestedCollection { get; set; }
        }

        public class MyJsonEntityNested
        {
            public DateTime DoB { get; set; }
        }
    }

    #endregion

    #region ShadowProperties

    [ConditionalFact]
    public virtual async Task Shadow_properties_basic_tracking()
    {
        var contextFactory = await InitializeAsync<ContextShadowProperties>(
            onModelCreating: OnModelCreatingShadowProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedShadowProperties);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextShadowProperties.MyEntity>();
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(2, result[0].Collection.Count);
        Assert.Equal(2, result[0].CollectionWithCtor.Count);
        Assert.NotNull(result[0].Reference);
        Assert.NotNull(result[0].ReferenceWithCtor);

        var referenceEntry = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].Reference);
        Assert.Equal("Foo", referenceEntry.Property("ShadowString").CurrentValue);

        var referenceCtorEntry = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].ReferenceWithCtor);
        Assert.Equal(143, referenceCtorEntry.Property("Shadow_Int").CurrentValue);

        var collectionEntry1 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].Collection[0]);
        var collectionEntry2 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].Collection[1]);
        Assert.Equal(5.5, collectionEntry1.Property("ShadowDouble").CurrentValue);
        Assert.Equal(20.5, collectionEntry2.Property("ShadowDouble").CurrentValue);

        var collectionCtorEntry1 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].CollectionWithCtor[0]);
        var collectionCtorEntry2 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].CollectionWithCtor[1]);
        Assert.Equal((byte)6, collectionCtorEntry1.Property("ShadowNullableByte").CurrentValue);
        Assert.Null(collectionCtorEntry2.Property("ShadowNullableByte").CurrentValue);
    }

    [ConditionalFact]
    public virtual async Task Shadow_properties_basic_no_tracking()
    {
        var contextFactory = await InitializeAsync<ContextShadowProperties>(
            onModelCreating: OnModelCreatingShadowProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedShadowProperties);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextShadowProperties.MyEntity>().AsNoTracking();
        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal(2, result[0].Collection.Count);
        Assert.Equal(2, result[0].CollectionWithCtor.Count);
        Assert.NotNull(result[0].Reference);
        Assert.NotNull(result[0].ReferenceWithCtor);
    }

    [ConditionalFact]
    public virtual async Task Project_shadow_properties_from_json_entity()
    {
        var contextFactory = await InitializeAsync<ContextShadowProperties>(
            onModelCreating: OnModelCreatingShadowProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedShadowProperties);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextShadowProperties.MyEntity>().Select(
            x => new
            {
                ShadowString = EF.Property<string>(x.Reference, "ShadowString"),
                ShadowInt = EF.Property<int>(x.ReferenceWithCtor, "Shadow_Int"),
            });

        var result = await query.ToListAsync();

        Assert.Equal(1, result.Count);
        Assert.Equal("Foo", result[0].ShadowString);
        Assert.Equal(143, result[0].ShadowInt);
    }

    protected abstract Task SeedShadowProperties(DbContext ctx);

    protected virtual void OnModelCreatingShadowProperties(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextShadowProperties.MyEntity>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();

                b.OwnsOne(x => x.Reference, b =>
                {
                    b.Property<string>("ShadowString");
                });

                b.OwnsOne(x => x.ReferenceWithCtor, b =>
                {
                    b.Property<int>("Shadow_Int");
                });

                b.OwnsMany(x => x.Collection, b =>
                {
                    b.Property<double>("ShadowDouble");
                });

                b.OwnsMany(x => x.CollectionWithCtor, b =>
                {
                    b.Property<byte?>("ShadowNullableByte");
                });
            });

    protected class ContextShadowProperties(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public MyJsonEntity Reference { get; set; }
            public List<MyJsonEntity> Collection { get; set; }
            public MyJsonEntityWithCtor ReferenceWithCtor { get; set; }
            public List<MyJsonEntityWithCtor> CollectionWithCtor { get; set; }
        }

        public class MyJsonEntity
        {
            public string Name { get; set; }
        }

        public class MyJsonEntityWithCtor(string name)
        {
            public string Name { get; set; } = name;
        }
    }

    #endregion

    #region LazyLoadingProxies

    [ConditionalFact]
    public virtual async Task Project_proxies_entity_with_json()
    {
        var contextFactory = await InitializeAsync<ContextLazyLoadingProxies>(
            onModelCreating: OnModelCreatingLazyLoadingProxies,
            seed: SeedLazyLoadingProxies,
            onConfiguring: b =>
            {
                b = b.ConfigureWarnings(ConfigureWarnings);
                OnConfiguringLazyLoadingProxies(b);
            },
            addServices: AddServicesLazyLoadingProxies);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextLazyLoadingProxies.MyEntity>();
        var result = await query.ToListAsync();

        Assert.Equal(2, result.Count);
    }

    protected void OnConfiguringLazyLoadingProxies(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseLazyLoadingProxies();

    protected IServiceCollection AddServicesLazyLoadingProxies(IServiceCollection addServices)
        => addServices.AddEntityFrameworkProxies();

    private Task SeedLazyLoadingProxies(DbContext ctx)
    {
        var r1 = new ContextLazyLoadingProxies.MyJsonEntityWithCtor("r1", 1);
        var c11 = new ContextLazyLoadingProxies.MyJsonEntity { Name = "c11", Number = 11 };
        var c12 = new ContextLazyLoadingProxies.MyJsonEntity { Name = "c12", Number = 12 };
        var c13 = new ContextLazyLoadingProxies.MyJsonEntity { Name = "c13", Number = 13 };

        var r2 = new ContextLazyLoadingProxies.MyJsonEntityWithCtor("r2", 2);
        var c21 = new ContextLazyLoadingProxies.MyJsonEntity { Name = "c21", Number = 21 };
        var c22 = new ContextLazyLoadingProxies.MyJsonEntity { Name = "c22", Number = 22 };

        var e1 = new ContextLazyLoadingProxies.MyEntity
        {
            Id = 1,
            Name = "e1",
            Reference = r1,
            Collection =
            [
                c11,
                c12,
                c13
            ]
        };

        var e2 = new ContextLazyLoadingProxies.MyEntity
        {
            Id = 2,
            Name = "e2",
            Reference = r2,
            Collection = [c21, c22]
        };

        ctx.Set<ContextLazyLoadingProxies.MyEntity>().AddRange(e1, e2);
        return ctx.SaveChangesAsync();
    }

    protected virtual void OnModelCreatingLazyLoadingProxies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContextLazyLoadingProxies.MyEntity>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<ContextLazyLoadingProxies.MyEntity>().OwnsOne(x => x.Reference);
        modelBuilder.Entity<ContextLazyLoadingProxies.MyEntity>().OwnsMany(x => x.Collection);
    }

    public class ContextLazyLoadingProxies(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public virtual MyJsonEntityWithCtor Reference { get; set; }
            public virtual List<MyJsonEntity> Collection { get; set; }
        }

        public class MyJsonEntityWithCtor(string name, int number)
        {
            public string Name { get; set; } = name;
            public int Number { get; set; } = number;
        }

        public class MyJsonEntity
        {
            public string Name { get; set; }
            public int Number { get; set; }
        }
    }

    #endregion

    #region NotICollection

    [ConditionalFact]
    public virtual async Task Not_ICollection_basic_projection()
    {
        var contextFactory = await InitializeAsync<ContextNotICollection>(
            onModelCreating: OnModelCreatingNotICollection,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedNotICollection);

        using var context = contextFactory.CreateContext();
        var query = context.Set<ContextNotICollection.MyEntity>();
        var result = await query.ToListAsync();

        Assert.Equal(2, result.Count);
    }

    protected abstract Task SeedNotICollection(DbContext ctx);

    protected virtual void OnModelCreatingNotICollection(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextNotICollection.MyEntity>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsOne(cr => cr.Json, nb =>
                {
                    nb.OwnsMany(x => x.Collection);
                });
            });


    protected class ContextNotICollection(DbContextOptions options) : DbContext(options)
    {
        public class MyEntity
        {
            public int Id { get; set; }

            public MyJsonEntity Json { get; set; }
        }

        public class MyJsonEntity
        {
            private readonly List<MyJsonNestedEntity> _collection = [];

            public IEnumerable<MyJsonNestedEntity> Collection
                => _collection.AsReadOnly();
        }

        public class MyJsonNestedEntity
        {
            public string Foo { get; set; }
            public int Bar { get; set; }
        }
    }

    #endregion

    #region BadJsonProperties

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Bad_json_properties_duplicated_navigations(bool noTracking)
    {
        var contextFactory = await InitializeAsync<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;
        var baseline = await query.SingleAsync(x => x.Scenario == "baseline");
        var dupNavs = await query.SingleAsync(x => x.Scenario == "duplicated navigations");

        // for no tracking, last one wins
        Assert.Equal(baseline.RequiredReference.NestedOptional.Text + " dupnav", dupNavs.RequiredReference.NestedOptional.Text);
        Assert.Equal(baseline.RequiredReference.NestedRequired.Text + " dupnav", dupNavs.RequiredReference.NestedRequired.Text);
        Assert.Equal(baseline.RequiredReference.NestedCollection[0].Text + " dupnav", dupNavs.RequiredReference.NestedCollection[0].Text);
        Assert.Equal(baseline.RequiredReference.NestedCollection[1].Text + " dupnav", dupNavs.RequiredReference.NestedCollection[1].Text);

        Assert.Equal(baseline.OptionalReference.NestedOptional.Text + " dupnav", dupNavs.OptionalReference.NestedOptional.Text);
        Assert.Equal(baseline.OptionalReference.NestedRequired.Text + " dupnav", dupNavs.OptionalReference.NestedRequired.Text);
        Assert.Equal(baseline.OptionalReference.NestedCollection[0].Text + " dupnav", dupNavs.OptionalReference.NestedCollection[0].Text);
        Assert.Equal(baseline.OptionalReference.NestedCollection[1].Text + " dupnav", dupNavs.OptionalReference.NestedCollection[1].Text);

        Assert.Equal(baseline.Collection[0].NestedOptional.Text + " dupnav", dupNavs.Collection[0].NestedOptional.Text);
        Assert.Equal(baseline.Collection[0].NestedRequired.Text + " dupnav", dupNavs.Collection[0].NestedRequired.Text);
        Assert.Equal(baseline.Collection[0].NestedCollection[0].Text + " dupnav", dupNavs.Collection[0].NestedCollection[0].Text);
        Assert.Equal(baseline.Collection[0].NestedCollection[1].Text + " dupnav", dupNavs.Collection[0].NestedCollection[1].Text);

        Assert.Equal(baseline.Collection[1].NestedOptional.Text + " dupnav", dupNavs.Collection[1].NestedOptional.Text);
        Assert.Equal(baseline.Collection[1].NestedRequired.Text + " dupnav", dupNavs.Collection[1].NestedRequired.Text);
        Assert.Equal(baseline.Collection[1].NestedCollection[0].Text + " dupnav", dupNavs.Collection[1].NestedCollection[0].Text);
        Assert.Equal(baseline.Collection[1].NestedCollection[1].Text + " dupnav", dupNavs.Collection[1].NestedCollection[1].Text);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Bad_json_properties_duplicated_scalars(bool noTracking)
    {
        var contextFactory = await InitializeAsync<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;

        var baseline = await query.SingleAsync(x => x.Scenario == "baseline");
        var dupProps = await query.SingleAsync(x => x.Scenario == "duplicated scalars");

        Assert.Equal(baseline.RequiredReference.NestedOptional.Text + " dupprop", dupProps.RequiredReference.NestedOptional.Text);
        Assert.Equal(baseline.RequiredReference.NestedRequired.Text + " dupprop", dupProps.RequiredReference.NestedRequired.Text);
        Assert.Equal(baseline.RequiredReference.NestedCollection[0].Text + " dupprop", dupProps.RequiredReference.NestedCollection[0].Text);
        Assert.Equal(baseline.RequiredReference.NestedCollection[1].Text + " dupprop", dupProps.RequiredReference.NestedCollection[1].Text);

        Assert.Equal(baseline.OptionalReference.NestedOptional.Text + " dupprop", dupProps.OptionalReference.NestedOptional.Text);
        Assert.Equal(baseline.OptionalReference.NestedRequired.Text + " dupprop", dupProps.OptionalReference.NestedRequired.Text);
        Assert.Equal(baseline.OptionalReference.NestedCollection[0].Text + " dupprop", dupProps.OptionalReference.NestedCollection[0].Text);
        Assert.Equal(baseline.OptionalReference.NestedCollection[1].Text + " dupprop", dupProps.OptionalReference.NestedCollection[1].Text);

        Assert.Equal(baseline.Collection[0].NestedOptional.Text + " dupprop", dupProps.Collection[0].NestedOptional.Text);
        Assert.Equal(baseline.Collection[0].NestedRequired.Text + " dupprop", dupProps.Collection[0].NestedRequired.Text);
        Assert.Equal(baseline.Collection[0].NestedCollection[0].Text + " dupprop", dupProps.Collection[0].NestedCollection[0].Text);
        Assert.Equal(baseline.Collection[0].NestedCollection[1].Text + " dupprop", dupProps.Collection[0].NestedCollection[1].Text);

        Assert.Equal(baseline.Collection[1].NestedOptional.Text + " dupprop", dupProps.Collection[1].NestedOptional.Text);
        Assert.Equal(baseline.Collection[1].NestedRequired.Text + " dupprop", dupProps.Collection[1].NestedRequired.Text);
        Assert.Equal(baseline.Collection[1].NestedCollection[0].Text + " dupprop", dupProps.Collection[1].NestedCollection[0].Text);
        Assert.Equal(baseline.Collection[1].NestedCollection[1].Text + " dupprop", dupProps.Collection[1].NestedCollection[1].Text);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Bad_json_properties_empty_navigations(bool noTracking)
    {
        var contextFactory = await InitializeAsync<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;
        var emptyNavs = await query.SingleAsync(x => x.Scenario == "empty navigation property names");

        Assert.Null(emptyNavs.RequiredReference.NestedOptional);
        Assert.Null(emptyNavs.RequiredReference.NestedRequired);
        Assert.Null(emptyNavs.RequiredReference.NestedCollection);

        Assert.Null(emptyNavs.OptionalReference.NestedOptional);
        Assert.Null(emptyNavs.OptionalReference.NestedRequired);
        Assert.Null(emptyNavs.OptionalReference.NestedCollection);

        Assert.Null(emptyNavs.Collection[0].NestedOptional);
        Assert.Null(emptyNavs.Collection[0].NestedRequired);
        Assert.Null(emptyNavs.Collection[0].NestedCollection);

        Assert.Null(emptyNavs.Collection[1].NestedOptional);
        Assert.Null(emptyNavs.Collection[1].NestedRequired);
        Assert.Null(emptyNavs.Collection[1].NestedCollection);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Bad_json_properties_empty_scalars(bool noTracking)
    {
        var contextFactory = await InitializeAsync<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;
        var emptyNavs = await query.SingleAsync(x => x.Scenario == "empty scalar property names");

        Assert.Null(emptyNavs.RequiredReference.NestedOptional.Text);
        Assert.Null(emptyNavs.RequiredReference.NestedRequired.Text);
        Assert.Null(emptyNavs.RequiredReference.NestedCollection[0].Text);
        Assert.Null(emptyNavs.RequiredReference.NestedCollection[1].Text);

        Assert.Null(emptyNavs.OptionalReference.NestedOptional.Text);
        Assert.Null(emptyNavs.OptionalReference.NestedRequired.Text);
        Assert.Null(emptyNavs.OptionalReference.NestedCollection[0].Text);
        Assert.Null(emptyNavs.OptionalReference.NestedCollection[1].Text);

        Assert.Null(emptyNavs.Collection[0].NestedOptional.Text);
        Assert.Null(emptyNavs.Collection[0].NestedRequired.Text);
        Assert.Null(emptyNavs.Collection[0].NestedCollection[0].Text);
        Assert.Null(emptyNavs.Collection[0].NestedCollection[1].Text);

        Assert.Null(emptyNavs.Collection[1].NestedOptional.Text);
        Assert.Null(emptyNavs.Collection[1].NestedRequired.Text);
        Assert.Null(emptyNavs.Collection[1].NestedCollection[0].Text);
        Assert.Null(emptyNavs.Collection[1].NestedCollection[1].Text);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Bad_json_properties_null_navigations(bool noTracking)
    {
        var contextFactory = await InitializeAsync<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;
        var _ = await query.SingleAsync(x => x.Scenario == "null navigation property names");
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Bad_json_properties_null_scalars(bool noTracking)
    {
        var contextFactory = await InitializeAsync<ContextBadJsonProperties>(
            onModelCreating: OnModelCreatingBadJsonProperties,
            onConfiguring: b => b.ConfigureWarnings(ConfigureWarnings),
            seed: SeedBadJsonProperties);

        using var context = contextFactory.CreateContext();
        var query = noTracking ? context.Entities.AsNoTracking() : context.Entities;
        var _ = await query.SingleAsync(x => x.Scenario == "null scalar property names");
    }

    protected abstract Task SeedBadJsonProperties(ContextBadJsonProperties ctx);

    protected virtual void OnModelCreatingBadJsonProperties(ModelBuilder modelBuilder)
        => modelBuilder.Entity<ContextBadJsonProperties.Entity>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();

                b.OwnsOne(x => x.RequiredReference, b =>
                {
                    b.OwnsOne(x => x.NestedOptional);
                    b.OwnsOne(x => x.NestedRequired);
                    b.OwnsMany(x => x.NestedCollection);
                });

                b.OwnsOne(x => x.OptionalReference, b =>
                {
                    b.OwnsOne(x => x.NestedOptional);
                    b.OwnsOne(x => x.NestedRequired);
                    b.OwnsMany(x => x.NestedCollection);
                });

                b.OwnsMany(x => x.Collection, b =>
                {
                    b.OwnsOne(x => x.NestedOptional);
                    b.OwnsOne(x => x.NestedRequired);
                    b.OwnsMany(x => x.NestedCollection);
                });
            });

    protected class ContextBadJsonProperties(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }

        public class Entity
        {
            public int Id { get; set; }
            public string Scenario { get; set; }
            public JsonRoot OptionalReference { get; set; }
            public JsonRoot RequiredReference { get; set; }
            public List<JsonRoot> Collection { get; set; }
        }

        public class JsonRoot
        {
            public JsonBranch NestedRequired { get; set; }
            public JsonBranch NestedOptional { get; set; }
            public List<JsonBranch> NestedCollection { get; set; }
        }

        public class JsonBranch
        {
            public string Text { get; set; }
        }
    }

    #endregion
}

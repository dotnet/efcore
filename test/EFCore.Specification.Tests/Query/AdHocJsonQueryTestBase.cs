// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
}

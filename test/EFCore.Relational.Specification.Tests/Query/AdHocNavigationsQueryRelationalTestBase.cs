// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class AdHocNavigationsQueryRelationalTestBase(NonSharedFixture fixture) : AdHocNavigationsQueryTestBase(fixture)
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    #region 39722

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Include_collection_with_complex_struct_key_39722(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context39722>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context39722.Parent>().Include(p => p.Children).OrderBy(p => p.Id.Value);
        var parents = async ? await query.ToListAsync() : query.ToList();

        Assert.Collection(
            parents,
            p => Assert.Equal([11, 12], p.Children.Select(c => c.Id).OrderBy(id => id)),
            p => Assert.Empty(p.Children));
    }

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Include_collection_with_complex_struct_key_and_filter_39722(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context39722>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var id = new Context39722.ParentId(Context39722.FirstId);
        var query = context.Set<Context39722.Parent>().Where(p => p.Id == id).Include(p => p.Children);
        var parent = async ? await query.SingleAsync() : query.Single();

        Assert.Equal(id, parent.Id);
        Assert.Equal([11, 12], parent.Children.Select(c => c.Id).OrderBy(value => value));
    }

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Include_reference_with_complex_struct_key_39722(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context39722>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<Context39722.Child>().Include(c => c.Parent).OrderBy(c => c.Id);
        var children = async ? await query.ToListAsync() : query.ToList();

        Assert.Collection(
            children,
            c => Assert.Equal(Context39722.FirstId, c.Parent!.Id.Value),
            c => Assert.Equal(Context39722.FirstId, c.Parent!.Id.Value),
            c => Assert.Null(c.Parent));
    }

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Find_with_complex_struct_key_39722(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context39722>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var result = async
            ? await context.FindAsync(typeof(Context39722.Parent), Context39722.FirstId)
            : context.Set<Context39722.Parent>().Find(Context39722.FirstId);

        var parent = Assert.IsType<Context39722.Parent>(result);
        Assert.Equal(Context39722.FirstId, parent.Id.Value);
    }

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Load_reference_with_complex_struct_key_39722(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context39722>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();
        var child = await context.Set<Context39722.Child>().SingleAsync(c => c.Id == 11);

        if (async)
        {
            await context.Entry(child).Reference(c => c.Parent).LoadAsync();
        }
        else
        {
            context.Entry(child).Reference(c => c.Parent).Load();
        }

        Assert.NotNull(child.Parent);
        Assert.Equal(Context39722.FirstId, child.Parent.Id.Value);
    }

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Include_collection_with_complex_composite_key_39722(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<CompositeContext39722>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<CompositeContext39722.Parent>().IgnoreAutoIncludes()
            .Include(p => p.Children).OrderBy(p => p.Id.Id).ThenBy(p => p.Id.TenantId);
        var parents = async ? await query.ToListAsync() : query.ToList();

        Assert.Collection(
            parents,
            p => Assert.Equal([11], p.Children.Select(c => c.Id)),
            p => Assert.Equal([21], p.Children.Select(c => c.Id)),
            p => Assert.Empty(p.Children));
    }

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Find_with_complex_composite_key_and_auto_include_39722(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<CompositeContext39722>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var parent = async
            ? await context.Set<CompositeContext39722.Parent>().FindAsync(1, "B")
            : context.Set<CompositeContext39722.Parent>().Find(1, "B");

        Assert.NotNull(parent);
        Assert.Equal(new CompositeContext39722.ParentId(1, "B"), parent.Id);
        Assert.Equal([21], parent.Children.Select(c => c.Id));
    }

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Include_and_find_with_nested_complex_key_39722(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<NestedContext39722>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<NestedContext39722.Parent>().Include(p => p.Children);
        var parent = async ? await query.SingleAsync() : query.Single();
        Assert.Equal([31], parent.Children.Select(c => c.Id));

        context.ChangeTracker.Clear();
        var found = async
            ? await context.Set<NestedContext39722.Parent>().FindAsync(3)
            : context.Set<NestedContext39722.Parent>().Find(3);

        Assert.NotNull(found);
        Assert.Equal(3, found.Id.Inner.Value);
    }

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Include_and_find_with_inherited_complex_key_type_39722(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<InheritedKeyContext39722>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<InheritedKeyContext39722.Parent>()
            .Include(p => p.Children)
            .OrderBy(p => p.Id.Value);
        var parents = async ? await query.ToListAsync() : query.ToList();

        Assert.Collection(
            parents,
            p =>
            {
                Assert.Equal(1, p.Id.Value);
                Assert.Equal(2, p.Value);
                Assert.Equal([11], p.Children.Select(c => c.Id));
            },
            p =>
            {
                Assert.Equal(2, p.Id.Value);
                Assert.Equal(1, p.Value);
                Assert.Equal([21], p.Children.Select(c => c.Id));
            });

        context.ChangeTracker.Clear();
        var found = async
            ? await context.Set<InheritedKeyContext39722.Parent>().FindAsync(1)
            : context.Set<InheritedKeyContext39722.Parent>().Find(1);

        Assert.NotNull(found);
        Assert.Equal(1, found.Id.Value);
        Assert.Equal(2, found.Value);
    }

    protected class Context39722(DbContextOptions options) : DbContext(options)
    {
        public static readonly Guid FirstId = new("00000000-0000-0000-0000-000000000001");
        public static readonly Guid SecondId = new("00000000-0000-0000-0000-000000000002");

        public readonly record struct ParentId(Guid Value);

        public class Parent
        {
            public ParentId Id { get; set; }
            public List<Child> Children { get; } = [];
        }

        public class Child
        {
            public int Id { get; set; }
            public Parent? Parent { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>(b =>
            {
                b.ComplexProperty(p => p.Id);
                b.HasKey(p => p.Id.Value);
                b.Property(p => p.Id.Value).ValueGeneratedNever();
                b.HasMany(p => p.Children).WithOne(c => c.Parent).HasForeignKey("ParentId");
            });
            modelBuilder.Entity<Child>().Property(c => c.Id).ValueGeneratedNever();
        }

        public Task SeedAsync()
        {
            AddRange(
                new Parent { Id = new ParentId(FirstId), Children = { new Child { Id = 11 }, new Child { Id = 12 } } },
                new Parent { Id = new ParentId(SecondId) },
                new Child { Id = 13 });

            return SaveChangesAsync();
        }
    }

    protected class CompositeContext39722(DbContextOptions options) : DbContext(options)
    {
        public sealed record ParentId(int Id, string TenantId);

        public class Parent
        {
            public ParentId Id { get; set; } = null!;
            public List<Child> Children { get; } = [];
        }

        public class Child
        {
            public int Id { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>(b =>
            {
                b.ComplexProperty(p => p.Id);
                b.HasKey(p => new { p.Id.Id, p.Id.TenantId });
                b.Property(p => p.Id.Id).ValueGeneratedNever();
                b.HasMany(p => p.Children).WithOne().HasForeignKey("ParentId", "ParentTenantId");
                b.Navigation(p => p.Children).AutoInclude();
            });
            modelBuilder.Entity<Child>().Property(c => c.Id).ValueGeneratedNever();
        }

        public Task SeedAsync()
        {
            AddRange(
                new Parent { Id = new ParentId(1, "A"), Children = { new Child { Id = 11 } } },
                new Parent { Id = new ParentId(1, "B"), Children = { new Child { Id = 21 } } },
                new Parent { Id = new ParentId(2, "A") });

            return SaveChangesAsync();
        }
    }

    protected class NestedContext39722(DbContextOptions options) : DbContext(options)
    {
        public sealed class ParentId
        {
            public InnerId Inner { get; set; }
        }

        public readonly record struct InnerId(int Value);

        public class Parent
        {
            public ParentId Id { get; set; } = null!;
            public List<Child> Children { get; } = [];
        }

        public class Child
        {
            public int Id { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>(b =>
            {
                b.ComplexProperty(p => p.Id, id => id.ComplexProperty(p => p.Inner));
                b.HasKey(p => p.Id.Inner.Value);
                b.Property(p => p.Id.Inner.Value).ValueGeneratedNever();
                b.HasMany(p => p.Children).WithOne().HasForeignKey("ParentId");
            });
            modelBuilder.Entity<Child>().Property(c => c.Id).ValueGeneratedNever();
        }

        public Task SeedAsync()
        {
            Add(new Parent { Id = new ParentId { Inner = new InnerId(3) }, Children = { new Child { Id = 31 } } });

            return SaveChangesAsync();
        }
    }

    protected class InheritedKeyContext39722(DbContextOptions options) : DbContext(options)
    {
        public class ParentId
        {
            public int Value { get; set; }
        }

        public class Parent : ParentId
        {
            public ParentId Id { get; set; } = null!;
            public List<Child> Children { get; } = [];
        }

        public class Child
        {
            public int Id { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<ParentId>();
            modelBuilder.Entity<Parent>(b =>
            {
                b.ComplexProperty(p => p.Id);
                b.HasKey(p => p.Id.Value);
                b.Property(p => p.Id.Value).ValueGeneratedNever();
                b.HasMany(p => p.Children).WithOne().HasForeignKey("ParentId");
            });
            modelBuilder.Entity<Child>().Property(c => c.Id).ValueGeneratedNever();
        }

        public Task SeedAsync()
        {
            AddRange(
                new Parent { Value = 2, Id = new ParentId { Value = 1 }, Children = { new Child { Id = 11 } } },
                new Parent { Value = 1, Id = new ParentId { Value = 2 }, Children = { new Child { Id = 21 } } });

            return SaveChangesAsync();
        }
    }

    #endregion

    #region 21803

    [Theory, InlineData(true, true), InlineData(true, false), InlineData(false, true), InlineData(false, false)]
    public virtual async Task Select_enumerable_navigation_backed_by_collection(bool async, bool split)
    {
        var contextFactory = await InitializeNonSharedTest<Context21803>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();
        var query = context.Set<Context21803.AppEntity>().Select(appEntity => appEntity.OtherEntities);

        if (split)
        {
            query = query.AsSplitQuery();
        }

        if (async)
        {
            await query.ToListAsync();
        }
        else
        {
            query.ToList();
        }
    }

    // Protected so that it can be used by inheriting tests, and so that things like unused setters are not removed.
    protected class Context21803(DbContextOptions options) : DbContext(options)
    {
        public DbSet<AppEntity> Entities { get; set; } = null!;

        public async Task SeedAsync()
        {
            var appEntity = new AppEntity();
            AddRange(
                new OtherEntity { AppEntity = appEntity },
                new OtherEntity { AppEntity = appEntity },
                new OtherEntity { AppEntity = appEntity },
                new OtherEntity { AppEntity = appEntity });

            await SaveChangesAsync();
        }

        public class AppEntity
        {
            private readonly List<OtherEntity> _otherEntities = [];

            public int Id { get; private set; }

            public IEnumerable<OtherEntity> OtherEntities
                => _otherEntities;
        }

        public class OtherEntity
        {
            public int Id { get; private set; }
            public AppEntity AppEntity { get; set; } = null!;
        }
    }

    #endregion

    #region ConditionalProjection

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Consecutive_selects_with_conditional_projection_should_not_include_unnecessary_joins(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<ContextConditionalProjection>(
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();

        var query = context.Users
            .Select(x => new
            {
                x.Id,
                Job = x.Job == null ? null : new { x.Job.Id, Address = new { x.Job.Address.Id, x.Job.Address.Street } }
            })
            .Select(x => new { x.Id, Job = x.Job == null ? null : new { x.Job.Id } })
            .Where(x => x.Id == 1);

        var result = async ? await query.FirstOrDefaultAsync() : query.FirstOrDefault();

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.NotNull(result.Job);
        Assert.Equal(1, result.Job!.Id);
    }

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Consecutive_selects_with_conditional_projection_null_navigation_returns_null(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<ContextConditionalProjection>(
            seed: c => c.SeedUserWithNoJobAsync());

        using var context = contextFactory.CreateDbContext();

        var query = context.Users
            .Where(x => x.JobId == null)
            .Select(x => new
            {
                x.Id,
                Job = x.Job == null ? null : new { x.Job.Id, Address = new { x.Job.Address.Id, x.Job.Address.Street } }
            })
            .Select(x => new { x.Id, Job = x.Job == null ? null : new { x.Job.Id } });

        var result = async ? await query.FirstOrDefaultAsync() : query.FirstOrDefault();

        Assert.NotNull(result);
        Assert.Null(result.Job);
    }

    [Theory, InlineData(false), InlineData(true)]
    public virtual async Task Consecutive_selects_with_conditional_projection_nested_navigation_accessed_includes_join(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<ContextConditionalProjection>(
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateDbContext();

        var query = context.Users
            .Select(x => new
            {
                x.Id,
                Job = x.Job == null ? null : new { x.Job.Id, Address = new { x.Job.Address.Id, x.Job.Address.Street } }
            })
            .Select(x => new { x.Id, Job = x.Job == null ? null : new { x.Job.Id, AddressId = x.Job.Address.Id } })
            .Where(x => x.Id == 1);

        var result = async ? await query.FirstOrDefaultAsync() : query.FirstOrDefault();

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.NotNull(result.Job);
        Assert.Equal(1, result.Job.AddressId);
    }

    protected class ContextConditionalProjection(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;

        public async Task SeedAsync()
        {
            var address = new Address { Street = "123 Main St" };
            var job = new Job { Address = address };
            var user = new User { Job = job };

            Add(user);
            await SaveChangesAsync();
        }

        public async Task SeedUserWithNoJobAsync()
        {
            var user = new User();
            Add(user);
            await SaveChangesAsync();
        }

        public class User
        {
            public long Id { get; set; }
            public long? JobId { get; set; }
            public Job? Job { get; set; }
        }

        public class Job
        {
            public long Id { get; set; }
            public long AddressId { get; set; }
            public Address Address { get; set; } = null!;
        }

        public class Address
        {
            public long Id { get; set; }
            public string Street { get; set; } = null!;
        }
    }

    #endregion
}

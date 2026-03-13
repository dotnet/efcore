// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class AdHocNavigationsQueryRelationalTestBase(NonSharedFixture fixture) : AdHocNavigationsQueryTestBase(fixture)
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    #region 21803

    [ConditionalTheory, InlineData(true, true), InlineData(true, false), InlineData(false, true), InlineData(false, false)]
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
        public DbSet<AppEntity> Entities { get; set; }

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
            public AppEntity AppEntity { get; set; }
        }
    }

    #endregion

    #region ConditionalProjection

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Consecutive_selects_with_conditional_projection_should_not_include_unnecessary_joins(bool async)
    {
        var contextFactory = await InitializeAsync<ContextConditionalProjection>(
            seed: c => c.SeedAsync());

        using var context = contextFactory.CreateContext();

        var query = context.Users
            .Select(x => new
            {
                x.Id,
                Job = x.Job == null ? null : new
                {
                    x.Job.Id,
                    Address = new
                    {
                        x.Job.Address.Id,
                        x.Job.Address.Street
                    }
                }
            })
            .Select(x => new
            {
                x.Id,
                Job = x.Job == null ? null : new
                {
                    x.Job.Id
                }
            })
            .Where(x => x.Id == 1);

        var result = async ? await query.FirstOrDefaultAsync() : query.FirstOrDefault();

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    protected class ContextConditionalProjection(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        public async Task SeedAsync()
        {
            var address = new Address { Street = "123 Main St" };
            var job = new Job { Address = address };
            var user = new User { Job = job };

            Add(user);
            await SaveChangesAsync();
        }

        public class User
        {
            public long Id { get; set; }
            public long? JobId { get; set; }
            public Job Job { get; set; }
        }

        public class Job
        {
            public long Id { get; set; }
            public long AddressId { get; set; }
            public Address Address { get; set; }
        }

        public class Address
        {
            public long Id { get; set; }
            public string Street { get; set; }
        }
    }

    #endregion
}

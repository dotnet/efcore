// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class AdHocComplexTypeQueryRelationalTestBase(NonSharedFixture fixture) : AdHocComplexTypeQueryTestBase(fixture)
{
    #region 37205

    [ConditionalFact]
    public virtual async Task Complex_json_collection_inside_left_join_subquery()
    {
        var contextFactory = await InitializeAsync<Context37205>();

        await using var context = contextFactory.CreateContext();

        _ = await context.Set<Context37205.Parent>().Include(p => p.Child).ToListAsync();
    }

    private class Context37205(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>();
            modelBuilder.Entity<Child>(b =>
            {
                b.ComplexCollection(e => e.CareNeeds, cb => cb.ToJson());
                b.HasQueryFilter(child => child.IsPublic);
            });
        }

        public class Parent
        {
            public int Id { get; set; }
            public Child? Child { get; set; }
            public int? ChildId { get; set; }
        }

        public class Child
        {
            public int Id { get; set; }
            public Parent Parent { get; set; } = null!;
            public bool IsPublic { get; set; }
            public required List<CareNeedAnswer> CareNeeds { get; set; }
        }

        public record CareNeedAnswer
        {
            public required string Topic { get; set; }
        }
    }

    #endregion 37205

    #region 35025

    [ConditionalFact]
    public virtual async Task Select_TPC_base_with_ComplexType()
    {
        var contextFactory = await InitializeAsync<Context35025>();
        using var context = contextFactory.CreateContext();

        var count = await context.Events.ToListAsync();

        // TODO: Seed data and assert materialization as well
        // Assert.Equal(0, count);
    }

    protected class Context35025(DbContextOptions options) : DbContext(options)
    {
        public DbSet<EventBase> Events { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventBase>(builder =>
            {
                builder.ComplexProperty(e => e.Knowledge).IsRequired();
                builder.UseTpcMappingStrategy();
            });
            modelBuilder.Entity<EventWithIdentification>();
            modelBuilder.Entity<RealEvent>();
        }

        public abstract class EventBase
        {
            public int Id { get; set; }
            public required Period Knowledge { get; set; }
        }

        public class EventWithIdentification : EventBase
        {
            public long ExtraId { get; set; }
        }

        public class RealEvent : EventBase
        {
        }

        public class Period
        {
            public DateTimeOffset From { get; set; }
        }
    }

    #endregion 35025
}

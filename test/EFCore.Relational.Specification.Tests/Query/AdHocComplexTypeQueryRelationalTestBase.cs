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

        var count = await context.TpcBases.ToListAsync();

        // TODO: Seed data and assert materialization as well
        // Assert.Equal(0, count);
    }

    protected class Context35025(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TpcBase> TpcBases { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TpcBase>(builder =>
            {
                builder.ComplexProperty(e => e.BaseComplexProperty);
                builder.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<TpcChild1>().ComplexProperty(e => e.ChildComplexProperty);
            modelBuilder.Entity<TpcChild2>().ComplexProperty(e => e.ChildComplexProperty);
        }

        public abstract class TpcBase
        {
            public int Id { get; set; }
            public required ComplexThing BaseComplexProperty { get; set; }
        }

        public class TpcChild1 : TpcBase
        {
            public required ComplexThing ChildComplexProperty { get; set; }
            public int ChildProperty { get; set; }
        }

        public class TpcChild2 : TpcBase
        {
            public required AnotherComplexThing ChildComplexProperty { get; set; }
            public required string ChildProperty { get; set; }
        }

        public class ComplexThing
        {
            public int PropertyInsideComplexThing { get; set; }
        }

        public class AnotherComplexThing
        {
            // Another nested property with the same name but a different type.
            // We should properly uniquify the projected name coming out of the UNION.
            public required string PropertyInsideComplexThing { get; set; }
        }
    }

    #endregion 35025

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}

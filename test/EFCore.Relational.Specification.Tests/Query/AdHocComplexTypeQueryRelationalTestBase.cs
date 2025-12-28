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

    #region 34706

    [ConditionalFact]
    public virtual async Task Complex_type_on_an_entity_mapped_to_view_and_table()
    {
        var contextFactory = await InitializeAsync<Context34706>(
            onModelCreating: mb => mb.Entity<Context34706.Blog>(eb => eb
                .ToTable("Blogs")
                .ToView("BlogsView")
                .ComplexProperty(b => b.ComplexThing)),
            seed: async context =>
            {
                var sqlGenerationHelper = context.GetService<ISqlGenerationHelper>();
                await context.Database.ExecuteSqlRawAsync($"CREATE VIEW {Q("BlogsView")} AS SELECT {Q("Id")}, {Q("ComplexThing_Prop1")}, {Q("ComplexThing_Prop2")} FROM {Q("Blogs")}");

                context.Add(
                    new Context34706.Blog
                    {
                        ComplexThing = new Context34706.ComplexThing
                        {
                            Prop1 = 1,
                            Prop2 = 2
                        }
                    });
                await context.SaveChangesAsync();

                string Q(string name) => sqlGenerationHelper.DelimitIdentifier(name);
            });

        await using var context = contextFactory.CreateContext();

        var entity = await context.Set<Context34706.Blog>().SingleAsync();

        Assert.NotNull(entity.ComplexThing);
        Assert.Equal(1, entity.ComplexThing.Prop1);
        Assert.Equal(2, entity.ComplexThing.Prop2);
    }

    private class Context34706(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Blog>().ComplexProperty(b => b.ComplexThing);

        public class Blog
        {
            public int Id { get; set; }
            public required ComplexThing ComplexThing { get; set; }
        }

        public class ComplexThing
        {
            public int Prop1 { get; set; }
            public int Prop2 { get; set; }
        }
    }

    #endregion 34706
}

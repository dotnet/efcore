// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public abstract class ComplexPropertiesCollectionTestBase<TFixture>(TFixture fixture)
    : AssociationsCollectionTestBase<TFixture>(fixture)
    where TFixture : ComplexPropertiesFixtureBase, new()
{
    #region 37926

    [ConditionalFact]
    public virtual async Task Project_struct_complex_type_with_entity_collection_navigation()
    {
        var contextFactory = await InitializeNonSharedTest<Context37926>(
            seed: async context =>
            {
                context.Add(new Context37926.Parent
                {
                    Coords = new Context37926.Coords { X = 1, Y = 2 },
                    Children = [new() { Name = "Child1" }]
                });
                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var result = await context.Set<Context37926.Parent>()
            .OrderBy(p => p.Id)
            .Select(p => new { p.Coords, p.Children })
            .FirstAsync();

        Assert.Equal(1, result.Coords.X);
        Assert.Single(result.Children);
    }

    protected class Context37926(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Parent>(b =>
            {
                b.ComplexProperty(e => e.Coords);
                b.HasMany(e => e.Children).WithOne().HasForeignKey(c => c.ParentId);
            });

        public class Parent
        {
            public int Id { get; set; }
            public Coords Coords { get; set; }
            public List<Child> Children { get; set; } = [];
        }

        public struct Coords
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class Child
        {
            public int Id { get; set; }
            public required string Name { get; set; }
            public int ParentId { get; set; }
        }
    }

    #endregion 37926
}

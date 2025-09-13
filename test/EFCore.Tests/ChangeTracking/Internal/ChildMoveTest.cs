// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class ChildMoveTest
{
    [ConditionalFact]
    public async Task Moving_child_between_parents_with_NoAction_should_not_throw()
    {
        using var context = new TestContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Arrange - Create initial data
        var fromParent = new Parent { Id = 1, Name = "From" };
        var toParent = new Parent { Id = 2, Name = "To" };
        var child = new Child { Id = 3, Name = "Child" };
        
        fromParent.Children.Add(child);
        
        context.Parents.Add(fromParent);
        context.Parents.Add(toParent);
        context.Children.Add(child);
        
        await context.SaveChangesAsync();
        
        // Act - Move child from one parent to another
        fromParent.Children.Remove(child);
        toParent.Children.Add(child);
        
        // This should not throw an exception
        await context.SaveChangesAsync();
        
        // Assert - Verify child is now associated with toParent
        var updatedChild = await context.Children.FindAsync(3);
        var updatedToParent = await context.Parents.Include(p => p.Children).FirstAsync(p => p.Id == 2);
        var updatedFromParent = await context.Parents.Include(p => p.Children).FirstAsync(p => p.Id == 1);
        
        Assert.Contains(updatedChild, updatedToParent.Children);
        Assert.DoesNotContain(updatedChild, updatedFromParent.Children);
    }

    [ConditionalFact]
    public async Task Orphaning_child_with_NoAction_should_still_throw()
    {
        using var context = new TestContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Arrange - Create initial data
        var parent = new Parent { Id = 1, Name = "Parent" };
        var child = new Child { Id = 2, Name = "Child" };
        
        parent.Children.Add(child);
        
        context.Parents.Add(parent);
        context.Children.Add(child);
        
        await context.SaveChangesAsync();
        
        // Act - Remove child from parent without adding to another parent (orphaning)
        parent.Children.Remove(child);
        
        // This should still throw an exception for truly orphaned children
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        
        Assert.Contains("association between entity types 'Parent' and 'Child' has been severed", exception.Message);
    }

    private class TestContext : DbContext
    {
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Child> Children { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase("ChildMoveTest");

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>()
                .HasMany(p => p.Children)
                .WithOne()
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    private class Child
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private class Parent
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Child> Children { get; set; } = new List<Child>();
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class PropertyValuesTest
{
    [ConditionalFact]
    public void Can_safely_get_originalvalue_and_currentvalue_with_tryget()
    {
        // Arrange
        const string NameValue = "Simple Name";
        const string NewNameValue = "A New Name";

        using var ctx = new CurrentValuesDb();
        var entity = ctx.SimpleEntities.Add(new SimpleEntity { Name = NameValue });
        ctx.SaveChanges();

        entity.Entity.Name = NewNameValue;

        // Act
        var current = entity.CurrentValues.TryGetValue<string>("Name", out var currentName);
        var original = entity.OriginalValues.TryGetValue<string>("Name", out var originalName);

        // Assert
        Assert.True(current);
        Assert.True(original);

        Assert.Equal(NameValue, originalName);
        Assert.Equal(NewNameValue, currentName);
    }

    [ConditionalFact]
    public void Should_not_throw_error_when_property_do_not_exist()
    {
        // Arrange
        const string NameValue = "Simple Name";
        const string NewNameValue = "A New Name";

        using var ctx = new CurrentValuesDb();
        var entity = ctx.SimpleEntities.Add(new SimpleEntity { Name = NameValue });
        ctx.SaveChanges();

        entity.Entity.Name = NewNameValue;

        // Act
        var current = entity.CurrentValues.TryGetValue<string>("Non_Existent_Property", out var non_existent_current);
        var original = entity.OriginalValues.TryGetValue<string>("Non_Existent_Property", out var non_existent_original);

        // Assert
        Assert.False(current);
        Assert.False(original);

        Assert.Null(non_existent_current);
        Assert.Null(non_existent_original);
    }

    private class CurrentValuesDb : DbContext
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<SimpleEntity> SimpleEntities { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase("DB1");
    }

    private class SimpleEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<RelatedEntity> RelatedEntities { get; set; }
    }

    private class RelatedEntity
    {
        public int Id { get; set; }

        public int? SimpleEntityId { get; set; }

        public SimpleEntity SimpleEntity { get; set; }

        public string Name { get; set; }
    }
}

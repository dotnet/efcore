// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class CascadeDeleteConventionTest
{
    [ConditionalFact]
    public void Cascade_delete_is_set_when_required_FK_is_added()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post>()
            .Property(e => e.BlogId)
            .IsRequired();

        var fk = modelBuilder.Entity<Blog>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog).Metadata;

        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Cascade_delete_is_not_set_when_optional_FK_is_added()
    {
        var modelBuilder = CreateModelBuilder();

        var fk = modelBuilder.Entity<Blog>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog).Metadata;

        Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Cascade_delete_is_set_when_optional_FK_is_made_required()
    {
        var modelBuilder = CreateModelBuilder();

        var fk = modelBuilder.Entity<Blog>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog).Metadata;

        modelBuilder.Entity<Post>()
            .Property(e => e.BlogId)
            .IsRequired();

        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Cascade_delete_is_not_set_when_required_FK_is_made_optional()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post>()
            .Property(e => e.BlogId)
            .IsRequired();

        var fk = modelBuilder.Entity<Blog>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog).Metadata;

        modelBuilder.Entity<Post>()
            .Property(e => e.BlogId)
            .IsRequired(false);

        Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Cascade_delete_is_not_changed_when_set_explicitly_on_added_FK()
    {
        var modelBuilder = CreateModelBuilder();

        var fk = modelBuilder.Entity<Blog>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog)
            .OnDelete(DeleteBehavior.Cascade)
            .Metadata;

        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Cascade_delete_is_not_changed_when_set_explicitly_on_FK()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post>()
            .Property(e => e.BlogId)
            .IsRequired();

        var fk = modelBuilder.Entity<Blog>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog)
            .OnDelete(DeleteBehavior.Cascade)
            .Metadata;

        modelBuilder.Entity<Post>()
            .Property(e => e.BlogId)
            .IsRequired(false);

        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    private class Blog
    {
        public int Id { get; set; }

        public ICollection<Post> Posts { get; set; }
    }

    private class Post
    {
        public int Id { get; set; }

        public Blog Blog { get; set; }
        public int? BlogId { get; set; }
    }

    private static ModelBuilder CreateModelBuilder()
        => InMemoryTestHelpers.Instance.CreateConventionBuilder();
}

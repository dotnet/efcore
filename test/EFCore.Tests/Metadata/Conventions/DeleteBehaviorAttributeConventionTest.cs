// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.



// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable InconsistentNaming

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class DeleteBehaviorAttributeConventionTest
{
    [ConditionalFact]
    public void Without_attribute_preserve_default_behavior()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post>()
            .Property(e => e.BlogId);

        var fk = modelBuilder.Entity<Blog>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog).Metadata;

        Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Correctly_set_restrict_delete_behavior_on_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_Restrict>()
            .Property(e => e.BlogId);

        var fk = modelBuilder.Entity<Blog_Restrict>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_Restrict).Metadata;

        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Correctly_set_delete_behavior_on_compound_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_Compound>()
            .Property(e => e.BlogId);
        modelBuilder.Entity<Post_Compound>()
            .Property(e => e.BlogId2);

        var fk = modelBuilder.Entity<Blog_Compound>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_Compound).Metadata;

        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Correctly_set_delete_behavior_on_two_different_foreign_keys()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_Both>()
            .Property(e => e.Blog_OneId);
        modelBuilder.Entity<Post_Both>()
            .Property(e => e.Blog_TwoId);

        var fk_One = modelBuilder.Entity<Blog_One>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_One).Metadata;

        var fk_Two = modelBuilder.Entity<Blog_Two>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_Two).Metadata;

        Assert.Equal(DeleteBehavior.Restrict, fk_One.DeleteBehavior);
        Assert.Equal(DeleteBehavior.Cascade, fk_Two.DeleteBehavior);
    }

    #region DeleteBehaviorAttribute not set
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
    #endregion
    #region DeleteBehaviourAttribute set to Restrict
    private class Blog_Restrict
    {
        public int Id { get; set; }

        public ICollection<Post_Restrict> Posts { get; set; }
    }

    private class Post_Restrict
    {
        public int Id { get; set; }

        public Blog_Restrict Blog_Restrict { get; set; }

        [DeleteBehavior(DeleteBehavior.Restrict)]
        public int? BlogId { get; set; }
    }
    #endregion
    #region DeleteBehaviourAttribute set on compound key
    private class Blog_Compound
    {
        [Key]
        [Column(Order=0)]
        public int Id { get; set; }
        [Key]
        [Column(Order=1)]
        public int Id2 { get; set; }

        public ICollection<Post_Compound> Posts { get; set; }
    }

    private class Post_Compound
    {
        public int Id { get; set; }

        [ForeignKey("BlogId, BlogId2")]
        public Blog_Compound Blog_Compound { get; set; }

        [Column(Order = 0)]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public int? BlogId { get; set; }

        [Column(Order = 1)]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public int? BlogId2 { get; set; }
    }
    #endregion
    #region DeleteBehaviourAttribute set on two different foreign keys
    private class Blog_One
    {
        public int Id { get; set; }

        public ICollection<Post_Both> Posts { get; set; }
    }
    private class Blog_Two
    {
        public int Id { get; set; }

        public ICollection<Post_Both> Posts { get; set; }
    }

    private class Post_Both
    {
        public int Id { get; set; }


        public Blog_One Blog_One { get; set; }
        public Blog_Two Blog_Two { get; set; }

        [ForeignKey("Blog_One")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public int? Blog_OneId { get; set; }

        [ForeignKey("Blog_Two")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public int? Blog_TwoId { get; set; }
    }
    #endregion

    private static ModelBuilder CreateModelBuilder()
        => InMemoryTestHelpers.Instance.CreateConventionBuilder();
}

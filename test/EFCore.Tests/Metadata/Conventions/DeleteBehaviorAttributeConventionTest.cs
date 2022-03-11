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
    public void Correctly_set_cascade_delete_behavior_on_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_Cascade>()
            .Property(e => e.BlogId);

        var fk = modelBuilder.Entity<Blog_Cascade>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_Cascade).Metadata;

        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
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
    public void Correctly_set_clientCascade_delete_behavior_on_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_ClientCascade>()
            .Property(e => e.BlogId);

        var fk = modelBuilder.Entity<Blog_ClientCascade>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_ClientCascade).Metadata;

        Assert.Equal(DeleteBehavior.ClientCascade, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Correctly_set_noAction_delete_behavior_on_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_NoAction>()
            .Property(e => e.BlogId);

        var fk = modelBuilder.Entity<Blog_NoAction>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_NoAction).Metadata;

        Assert.Equal(DeleteBehavior.NoAction, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Correctly_set_setNull_delete_behavior_on_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_SetNull>()
            .Property(e => e.BlogId);

        var fk = modelBuilder.Entity<Blog_SetNull>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_SetNull).Metadata;

        Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Correctly_set_clientNoAction_delete_behavior_on_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_ClientNoAction>()
            .Property(e => e.BlogId);

        var fk = modelBuilder.Entity<Blog_ClientNoAction>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_ClientNoAction).Metadata;

        Assert.Equal(DeleteBehavior.ClientNoAction, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Correctly_set_clientSetNull_delete_behavior_on_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_ClientSetNull>()
            .Property(e => e.BlogId);

        var fk = modelBuilder.Entity<Blog_ClientSetNull>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_ClientSetNull).Metadata;

        Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Correctly_set_delete_behavior_on_compound_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Blog_Compound>()
            .HasKey(e => new { e.Id, e.Id2 });

        modelBuilder.Entity<Post_Compound>()
            .Property(e => e.BlogId);
        modelBuilder.Entity<Post_Compound>()
            .Property(e => e.BlogId2);

        var fk = modelBuilder.Entity<Blog_Compound>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_Compound)
            .HasForeignKey(e => new {e.BlogId, e.BlogId2}).Metadata;

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
            .WithOne(e => e.Blog_One)
            .HasForeignKey(e => e.Blog_OneId).Metadata;

        var fk_Two = modelBuilder.Entity<Blog_Two>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_Two)
            .HasForeignKey(e => e.Blog_TwoId).Metadata;

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
    #region DeleteBehaviourAttribute set to Cascade
    private class Blog_Cascade
    {
        public int Id { get; set; }

        public ICollection<Post_Cascade> Posts { get; set; }
    }

    private class Post_Cascade
    {
        public int Id { get; set; }

        public Blog_Cascade Blog_Cascade { get; set; }

        [DeleteBehavior((int)DeleteBehavior.Cascade)]
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

        [DeleteBehavior((int)DeleteBehavior.Restrict)]
        public int? BlogId { get; set; }
    }
    #endregion
    #region DeleteBehaviourAttribute set to ClientCascade
    private class Blog_ClientCascade
    {
        public int Id { get; set; }

        public ICollection<Post_ClientCascade> Posts { get; set; }
    }

    private class Post_ClientCascade
    {
        public int Id { get; set; }

        public Blog_ClientCascade Blog_ClientCascade { get; set; }

        [DeleteBehavior((int)DeleteBehavior.ClientCascade)]
        public int? BlogId { get; set; }
    }
    #endregion
    #region DeleteBehaviourAttribute set to NoAction
    private class Blog_NoAction
    {
        public int Id { get; set; }

        public ICollection<Post_NoAction> Posts { get; set; }
    }

    private class Post_NoAction
    {
        public int Id { get; set; }

        public Blog_NoAction Blog_NoAction { get; set; }

        [DeleteBehavior((int)DeleteBehavior.NoAction)]
        public int? BlogId { get; set; }
    }
    #endregion
    #region DeleteBehaviourAttribute set to SetNull
    private class Blog_SetNull
    {
        public int Id { get; set; }

        public ICollection<Post_SetNull> Posts { get; set; }
    }

    private class Post_SetNull
    {
        public int Id { get; set; }

        public Blog_SetNull Blog_SetNull { get; set; }

        [DeleteBehavior((int)DeleteBehavior.SetNull)]
        public int? BlogId { get; set; }
    }
    #endregion
    #region DeleteBehaviourAttribute set to ClientNoAction
    private class Blog_ClientNoAction
    {
        public int Id { get; set; }

        public ICollection<Post_ClientNoAction> Posts { get; set; }
    }

    private class Post_ClientNoAction
    {
        public int Id { get; set; }

        public Blog_ClientNoAction Blog_ClientNoAction { get; set; }

        [DeleteBehavior((int)DeleteBehavior.ClientNoAction)]
        public int? BlogId { get; set; }
    }
    #endregion
    #region DeleteBehaviourAttribute set to ClientSetNull
    private class Blog_ClientSetNull
    {
        public int Id { get; set; }

        public ICollection<Post_ClientSetNull> Posts { get; set; }
    }

    private class Post_ClientSetNull
    {
        public int Id { get; set; }

        public Blog_ClientSetNull Blog_ClientSetNull { get; set; }

        [DeleteBehavior((int)DeleteBehavior.ClientSetNull)]
        public int? BlogId { get; set; }
    }
    #endregion
    #region DeleteBehaviourAttribute set on compound key
    private class Blog_Compound
    {
        public int Id { get; set; }
        public int Id2 { get; set; }

        public ICollection<Post_Compound> Posts { get; set; }
    }

    private class Post_Compound
    {
        public int Id { get; set; }


        public Blog_Compound Blog_Compound { get; set; }

        [DeleteBehavior((int)DeleteBehavior.Cascade)]
        public int? BlogId { get; set; }

        [DeleteBehavior((int)DeleteBehavior.Cascade)]
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

        [DeleteBehavior((int)DeleteBehavior.Restrict)]
        public int? Blog_OneId { get; set; }

        [DeleteBehavior((int)DeleteBehavior.Cascade)]
        public int? Blog_TwoId { get; set; }
    }
    #endregion

    private static ModelBuilder CreateModelBuilder()
        => InMemoryTestHelpers.Instance.CreateConventionBuilder();
}

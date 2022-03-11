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

    [ConditionalFact]
    public void Correctly_set_delete_behavior_on_foreign_key_declared_by_FluentAPI()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_Restrict>()
            .Property(e => e.BlogId);

        var fk = modelBuilder.Entity<Blog_Restrict>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_Restrict)
            .HasForeignKey(e => e.BlogId).Metadata;

        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [ConditionalFact]
    public void Correctly_set_delete_behavior_on_implicit_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_Restrict>()
            .Property(e => e.BlogId);

        var fk = modelBuilder.Entity<Blog_Restrict>()
            .HasMany(e => e.Posts)
            .WithOne(e => e.Blog_Restrict).Metadata;

        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
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

        [ForeignKey("Blog_Cascade")]
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


        [ForeignKey("Blog_Restrict")]
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

        [ForeignKey("Blog_ClientCascade")]
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


        [ForeignKey("Blog_NoAction")]
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

        [ForeignKey("Blog_SetNull")]
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

        [ForeignKey("Blog_ClientNoAction")]
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

        [ForeignKey("Blog_ClientSetNull")]
        [DeleteBehavior((int)DeleteBehavior.ClientSetNull)]
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
        [DeleteBehavior((int)DeleteBehavior.Cascade)]
        public int? BlogId { get; set; }

        [Column(Order = 1)]
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

        [ForeignKey("Blog_One")]
        [DeleteBehavior((int)DeleteBehavior.Restrict)]
        public int? Blog_OneId { get; set; }

        [ForeignKey("Blog_Two")]
        [DeleteBehavior((int)DeleteBehavior.Cascade)]
        public int? Blog_TwoId { get; set; }
    }
    #endregion
    #region DeleteBehaviourAttribute set to Restrict and foreign key defined by FluentApi
    private class Blog_Restrict_Fluent
    {
        public int Id { get; set; }

        public ICollection<Post_Restrict_Fluent> Posts { get; set; }
    }

    private class Post_Restrict_Fluent
    {
        public int Id { get; set; }

        public Blog_Restrict_Fluent Blog_Restrict_Fluent { get; set; }

        [DeleteBehavior((int)DeleteBehavior.Restrict)]
        public int? BlogId { get; set; }
    }
    #endregion
    #region DeleteBehaviourAttribute set to Restrict and implicit foreign key
    private class Blog_Restrict_Implicit
    {
        public int Id { get; set; }

        public ICollection<Post_Restrict_Implicit> Posts { get; set; }
    }

    private class Post_Restrict_Implicit
    {
        public int Id { get; set; }

        public Blog_Restrict_Implicit Blog_Restrict_Implicit { get; set; }

        [DeleteBehavior((int)DeleteBehavior.Restrict)]
        public int? BlogId { get; set; }
    }
    #endregion

    private static ModelBuilder CreateModelBuilder()
        => InMemoryTestHelpers.Instance.CreateConventionBuilder();
}

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

    [ConditionalFact]
    public void Throw_InvalidOperationException_if_attribute_was_set_on_one_of_foreign_keys_properties()
    {
        var modelBuilder = CreateModelBuilder();

        Assert.Equal(
            CoreStrings.DeleteBehaviorAttributeNotOnNavigationProperty(
                nameof(Post_On_FK_Property), nameof(Post_On_FK_Property.Blog_On_FK_PropertyId)),
            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<Post_On_FK_Property>()
                    .Property(e => e.Blog_On_FK_PropertyId)).Message
        );
    }

    [ConditionalFact]
    public void Throw_InvalidOperationException_if_attribute_was_set_on_random_property()
    {
        var modelBuilder = CreateModelBuilder();

        Assert.Equal(
            CoreStrings.DeleteBehaviorAttributeNotOnNavigationProperty(
                nameof(Post_On_Property), nameof(Post_On_Property.Id)),
            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<Post_On_Property>()
                    .Property(e => e.Blog_On_PropertyId)).Message
        );
    }

    [ConditionalFact]
    public void Throw_InvalidOperationException_if_attribute_was_set_on_principal_navigation_property()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_On_Principal>()
            .Property(e => e.Blog_On_PrincipalId);

        Assert.Equal(
            CoreStrings.DeleteBehaviorAttributeOnPrincipalProperty(
                nameof(Blog_On_Principal), nameof(Blog_On_Principal.Posts)),
            Assert.Throws<InvalidOperationException>(modelBuilder.FinalizeModel).Message
        );
    }

    [ConditionalFact]
    public void Throw_InvalidOperationException_if_attribute_was_set_on_principal_one_to_one_relationship()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Post_On_Principal_OneToOne>()
            .Property(e => e.Blog_On_PrincipalId);

        Assert.Equal(
            CoreStrings.DeleteBehaviorAttributeOnPrincipalProperty(
                nameof(Blog_On_Principal_OneToOne), nameof(Blog_On_Principal_OneToOne.Post_On_Principal_OneToOne)),
            Assert.Throws<InvalidOperationException>(modelBuilder.FinalizeModel).Message
        );
    }

    private static ModelBuilder CreateModelBuilder()
        => InMemoryTestHelpers.Instance.CreateConventionBuilder();

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

        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Blog_Restrict Blog_Restrict { get; set; }

        public int? BlogId { get; set; }
    }

    #endregion

    #region DeleteBehaviourAttribute set on compound key

    private class Blog_Compound
    {
        [Key]
        [Column(Order = 0)]
        public int Id { get; set; }

        [Key]
        [Column(Order = 1)]
        public int Id2 { get; set; }

        public ICollection<Post_Compound> Posts { get; set; }
    }

    private class Post_Compound
    {
        public int Id { get; set; }

        [ForeignKey("BlogId, BlogId2")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public Blog_Compound Blog_Compound { get; set; }

        [Column(Order = 0)]
        public int? BlogId { get; set; }

        [Column(Order = 1)]
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

        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Blog_One Blog_One { get; set; }

        [DeleteBehavior(DeleteBehavior.Cascade)]
        public Blog_Two Blog_Two { get; set; }

        public int? Blog_OneId { get; set; }

        public int? Blog_TwoId { get; set; }
    }

    #endregion

    #region DeleteBehaviourAttribute set on one of foreign key's properties

    private class Blog_On_FK_Property
    {
        public int Id { get; set; }

        public ICollection<Post_On_FK_Property> Posts { get; set; }
    }

    private class Post_On_FK_Property
    {
        public int Id { get; set; }

        public Blog_On_FK_Property Blog_On_FK_Property { get; set; }

        [DeleteBehavior(DeleteBehavior.Restrict)]
        public int? Blog_On_FK_PropertyId { get; set; }
    }

    #endregion

    #region DeleteBehaviourAttribute set on random property

    private class Blog_On_Property
    {
        public int Id { get; set; }

        public ICollection<Post_On_Property> Posts { get; set; }
    }

    private class Post_On_Property
    {
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public int Id { get; set; }

        public Blog_On_Property Blog_On_Property { get; set; }

        public int? Blog_On_PropertyId { get; set; }
    }

    #endregion

    #region DeleteBehaviourAttribute set on principal navigation property

    private class Blog_On_Principal
    {
        public int Id { get; set; }

        [DeleteBehavior(DeleteBehavior.Restrict)]
        public ICollection<Post_On_Principal> Posts { get; set; }
    }

    private class Post_On_Principal
    {
        public int Id { get; set; }

        public Blog_On_Principal Blog_On_Principal { get; set; }

        public int? Blog_On_PrincipalId { get; set; }
    }

    #endregion

    #region DeleteBehaviourAttribute set on principal 1:1 relationship

    private class Blog_On_Principal_OneToOne
    {
        public int Id { get; set; }

        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Post_On_Principal_OneToOne Post_On_Principal_OneToOne { get; set; }
    }

    private class Post_On_Principal_OneToOne
    {
        public int Id { get; set; }

        public Blog_On_Principal_OneToOne Blog_On_Principal_OneToOne { get; set; }

        public int? Blog_On_PrincipalId { get; set; }
    }

    #endregion
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class QueryFilterRewritingConventionTest
{
    [ConditionalFact]
    public virtual void QueryFilter_containing_db_set_with_not_included_type()
    {
        var modelBuilder = new InternalModelBuilder(new Model());
        Expression<Func<Blog, bool>> lambda = e => new MyContext().Set<Post>().Single().Id == e.Id;
        modelBuilder.Entity(typeof(Blog), ConfigurationSource.Explicit)
            .HasQueryFilter(lambda, ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.InvalidSetType(typeof(Post).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => RunConvention(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void QueryFilter_containing_db_set_with_shared_type_without_name()
    {
        var modelBuilder = new InternalModelBuilder(new Model());
        modelBuilder.SharedTypeEntity("Post1", typeof(Post), ConfigurationSource.Explicit);
        Expression<Func<Blog, bool>> lambda = e => new MyContext().Set<Post>().Single().Id == e.Id;
        modelBuilder.Entity(typeof(Blog), ConfigurationSource.Explicit)
            .HasQueryFilter(lambda, ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.InvalidSetSharedType(typeof(Post).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => RunConvention(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void QueryFilter_containing_db_set_of_incorrect_type()
    {
        var modelBuilder = new InternalModelBuilder(new Model());
        modelBuilder.SharedTypeEntity("Post1", typeof(Post), ConfigurationSource.Explicit);
        Expression<Func<Blog, bool>> lambda = e => new MyContext().Set<Blog>("Post1").Single().Id == e.Id;
        modelBuilder.Entity(typeof(Blog), ConfigurationSource.Explicit)
            .HasQueryFilter(lambda, ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.DbSetIncorrectGenericType("Post1", typeof(Post).ShortDisplayName(), typeof(Blog).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => RunConvention(modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void QueryFilter_containing_db_set_of_owned()
    {
        var modelBuilder = new InternalModelBuilder(new Model());
        modelBuilder.Entity(typeof(Owner), ConfigurationSource.Explicit)
            .HasOwnership(typeof(Blog), "Blog", ConfigurationSource.Explicit);

        Expression<Func<Owner, bool>> lambda = e => new MyContext().Set<Blog>().Single().Id == e.Id;
        modelBuilder.Entity(typeof(Owner), ConfigurationSource.Explicit)
            .HasQueryFilter(lambda, ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.InvalidSetTypeOwned(typeof(Blog).ShortDisplayName(), typeof(Owner).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => RunConvention(modelBuilder)).Message);
    }

    private void RunConvention(InternalModelBuilder modelBuilder)
    {
        var context = new ConventionContext<IConventionModelBuilder>(modelBuilder.Metadata.ConventionDispatcher);

        new QueryFilterRewritingConvention(CreateDependencies())
            .ProcessModelFinalizing(modelBuilder, context);

        Assert.False(context.ShouldStopProcessing());
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    protected class Blog
    {
        public int Id { get; set; }
    }

    protected class Post
    {
        public int Id { get; set; }
    }

    protected class Owner
    {
        public int Id { get; set; }
        public Blog Blog { get; set; }
    }

    protected class MyContext : DbContext;
}

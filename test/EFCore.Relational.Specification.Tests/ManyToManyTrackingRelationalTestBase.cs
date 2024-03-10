// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class ManyToManyTrackingRelationalTestBase<TFixture> : ManyToManyTrackingTestBase<TFixture>
    where TFixture : ManyToManyTrackingRelationalTestBase<TFixture>.ManyToManyTrackingRelationalFixture
{
    protected ManyToManyTrackingRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public void Many_to_many_delete_behaviors_are_set()
    {
        using var context = CreateContext();
        var model = context.Model;

        var navigations = model.GetEntityTypes().SelectMany(e => e.GetDeclaredSkipNavigations())
            .Where(e => e.ForeignKey.DeleteBehavior != DeleteBehavior.Cascade).ToList();

        var builder = new StringBuilder();
        foreach (var navigation in navigations)
        {
            builder.AppendLine($"{{ \"{navigation.DeclaringEntityType.ShortName()}.{navigation.Name}\", DeleteBehavior.ClientCascade }},");
        }

        var x = builder.ToString();

        foreach (var skipNavigation in model.GetEntityTypes().SelectMany(e => e.GetSkipNavigations()))
        {
            Assert.Equal(
                CustomDeleteBehaviors.TryGetValue(
                    $"{skipNavigation.DeclaringEntityType.ShortName()}.{skipNavigation.Name}", out var deleteBehavior)
                    ? deleteBehavior
                    : DeleteBehavior.Cascade,
                skipNavigation.ForeignKey.DeleteBehavior);
        }
    }

    protected virtual Dictionary<string, DeleteBehavior> CustomDeleteBehaviors { get; } = new();

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public abstract class ManyToManyTrackingRelationalFixture : ManyToManyTrackingFixtureBase
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<EntityTableSharing1>().ToTable("TableSharing");
            modelBuilder.Entity<EntityTableSharing2>(
                b =>
                {
                    b.HasOne<EntityTableSharing1>().WithOne().HasForeignKey<EntityTableSharing2>(e => e.Id);
                    b.ToTable("TableSharing");
                });
        }
    }
}

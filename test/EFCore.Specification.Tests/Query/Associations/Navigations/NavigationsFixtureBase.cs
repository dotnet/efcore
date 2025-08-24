// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public abstract class NavigationsFixtureBase : AssociationsQueryFixtureBase
{
    protected override string StoreName
        => "NavigationsQueryTest";

    public override bool AreCollectionsOrdered
        => false;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // Don't use database value generation since e.g. Cosmos doesn't support it.
        modelBuilder.Entity<RelatedType>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<NestedType>().Property(x => x.Id).ValueGeneratedNever();

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.HasOne(r => r.RequiredRelated)
                .WithOne(r => r.RequiredRelatedInverse)
                .HasForeignKey<RootEntity>(r => r.RequiredRelatedId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction); // TODO: Move to SQL Server

            b.HasOne(r => r.OptionalRelated)
                .WithOne(r => r.OptionalRelatedInverse)
                .HasForeignKey<RootEntity>(r => r.OptionalRelatedId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction); // TODO: Move to SQL Server

            b.HasMany(r => r.RelatedCollection)
                .WithOne(r => r.RelatedCollectionInverse)
                .HasForeignKey(r => r.CollectionRootId);
        });

        modelBuilder.Entity<RelatedType>(b =>
        {
            b.HasOne(r => r.RequiredNested)
                .WithOne(r => r.RequiredNestedInverse)
                .HasForeignKey<RelatedType>(r => r.RequiredNestedId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction); // TODO: Move to SQL Server

            b.HasOne(e => e.OptionalNested)
                .WithOne(r => r.OptionalNestedInverse)
                .HasForeignKey<RelatedType>(r => r.OptionalNestedId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction); // TODO: Move to SQL Server

            b.HasMany(r => r.NestedCollection)
                .WithOne(r => r.NestedCollectionInverse)
                .HasForeignKey(r => r.CollectionRelatedId);
        });
    }
}

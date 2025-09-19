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
        modelBuilder.Entity<AssociateType>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<NestedAssociateType>().Property(x => x.Id).ValueGeneratedNever();

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.HasOne(r => r.RequiredAssociate)
                .WithOne(r => r.RequiredAssociateInverse)
                .HasForeignKey<RootEntity>(r => r.RequiredAssociateId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction); // TODO: Move to SQL Server

            b.HasOne(r => r.OptionalAssociate)
                .WithOne(r => r.OptionalAssociateInverse)
                .HasForeignKey<RootEntity>(r => r.OptionalAssociateId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction); // TODO: Move to SQL Server

            b.HasMany(r => r.AssociateCollection)
                .WithOne(r => r.AssociateCollectionInverse)
                .HasForeignKey(r => r.CollectionRootId);
        });

        modelBuilder.Entity<AssociateType>(b =>
        {
            b.HasOne(r => r.RequiredNestedAssociate)
                .WithOne(r => r.RequiredNestedAssociateInverse)
                .HasForeignKey<AssociateType>(r => r.RequiredNestedAssociateId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction); // TODO: Move to SQL Server

            b.HasOne(e => e.OptionalNestedAssociate)
                .WithOne(r => r.OptionalNestedAssociateInverse)
                .HasForeignKey<AssociateType>(r => r.OptionalNestedAssociateId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction); // TODO: Move to SQL Server

            b.HasMany(r => r.NestedCollection)
                .WithOne(r => r.NestedCollectionInverse)
                .HasForeignKey(r => r.CollectionAssociateId);
        });
    }
}

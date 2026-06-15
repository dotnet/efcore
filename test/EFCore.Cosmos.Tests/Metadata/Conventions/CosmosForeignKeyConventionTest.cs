// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class CosmosForeignKeyConventionTest
{
    [Fact]
    public void Non_owned_foreign_keys_default_to_unconstrained()
    {
        var modelBuilder = CosmosConventionSetBuilder.CreateModelBuilder();

        modelBuilder.Entity<Principal>(b =>
        {
            b.Property<string>("PartitionId");
            b.HasPartitionKey("PartitionId");
            b.ToContainer("Principals");
        });

        modelBuilder.Entity<Dependent>(b =>
        {
            b.Property<string>("PartitionId");
            b.HasPartitionKey("PartitionId");
            b.ToContainer("Dependents");
            b.HasOne<Principal>().WithMany().HasForeignKey(e => e.PrincipalId);
        });

        var model = modelBuilder.FinalizeModel();

        var fk = model.FindEntityType(typeof(Dependent))!.GetForeignKeys().Single();
        Assert.False(fk.IsOwnership);
        Assert.False(fk.IsConstrained);
    }

    [Fact]
    public void Owned_foreign_keys_remain_constrained()
    {
        var modelBuilder = CosmosConventionSetBuilder.CreateModelBuilder();

        modelBuilder.Entity<Owner>(b =>
        {
            b.ToContainer("Owners");
            b.OwnsOne(e => e.Owned);
        });

        var model = modelBuilder.FinalizeModel();

        var ownerEntityType = model.FindEntityType(typeof(Owner))!;
        var ownedNavigation = ownerEntityType.FindNavigation(nameof(Owner.Owned))!;
        var fk = ownedNavigation.ForeignKey;
        Assert.True(fk.IsOwnership);
        Assert.True(fk.IsConstrained);
    }

    private class Principal
    {
        public int Id { get; set; }
    }

    private class Dependent
    {
        public int Id { get; set; }
        public int PrincipalId { get; set; }
    }

    private class Owner
    {
        public int Id { get; set; }
        public Owned Owned { get; set; }
    }

    private class Owned
    {
        public int Id { get; set; }
    }
}

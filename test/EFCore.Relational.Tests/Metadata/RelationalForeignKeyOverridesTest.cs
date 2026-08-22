// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

public class RelationalForeignKeyOverridesTest
{
    [Fact]
    public void Two_fragments_of_one_entity_share_a_single_model_foreign_key()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users");
            b.SplitToTable("UserLockout", s => s.Property(u => u.PasswordHash));
            b.SplitToTable("UserProfile", s => s.Property(u => u.DisplayName));
        });

        var model = modelBuilder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(User))!;

        // Spike 2 finding 1: one self-referential FK serves every fragment's linking constraint,
        // which is why override identity must be the (dependent, principal) pair.
        var linkingForeignKeys = entityType.GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType == entityType)
            .ToList();
        Assert.Single(linkingForeignKeys);
    }

    [Fact]
    public void Foreign_key_overrides_are_keyed_by_the_dependent_principal_pair()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users");
            b.SplitToTable("UserLockout", s => s.Property(u => u.PasswordHash));
            b.SplitToTable("UserProfile", s => s.Property(u => u.DisplayName));
        });

        // EntitySplittingConvention only wires up the shared self-referential linking foreign key as
        // part of model-finalizing conventions, and finalizing the model makes it (and every object
        // reachable from it) read-only — too late to attach overrides. Build the identical linking
        // relationship the convention itself would create (see EntitySplittingConvention.ProcessModelFinalizing),
        // but do it here, before finalization, so the foreign key is still mutable.
        var entityType = (IConventionEntityType)modelBuilder.Model.FindEntityType(typeof(User))!;
        var pk = entityType.FindPrimaryKey()!;
        var foreignKey = (IMutableForeignKey)entityType.Builder
            .HasRelationship(entityType, pk.Properties, pk)!
            .IsUnique(true)!
            .Metadata;

        var users = StoreObjectIdentifier.Table("Users");
        var lockoutPair = new StoreObjectPair(StoreObjectIdentifier.Table("UserLockout"), users);
        var profilePair = new StoreObjectPair(StoreObjectIdentifier.Table("UserProfile"), users);

        RelationalForeignKeyOverrides.GetOrCreate(foreignKey, lockoutPair, ConfigurationSource.Explicit)
            .SetName("fk_user_lockout", ConfigurationSource.Explicit);
        RelationalForeignKeyOverrides.GetOrCreate(foreignKey, profilePair, ConfigurationSource.Explicit)
            .SetName("fk_user_profile", ConfigurationSource.Explicit);

        // One FK object, two distinctly named constraints — impossible with single-store-object identity.
        Assert.Equal("fk_user_lockout", RelationalForeignKeyOverrides.Find(foreignKey, lockoutPair)!.Name);
        Assert.Equal("fk_user_profile", RelationalForeignKeyOverrides.Find(foreignKey, profilePair)!.Name);
    }

    [Fact]
    public void Store_object_pair_equality_is_order_sensitive()
    {
        var a = StoreObjectIdentifier.Table("A");
        var b = StoreObjectIdentifier.Table("B");

        Assert.Equal(new StoreObjectPair(a, b), new StoreObjectPair(a, b));
        Assert.NotEqual(new StoreObjectPair(a, b), new StoreObjectPair(b, a));
        Assert.NotEqual(new StoreObjectPair(a, b).GetHashCode(), new StoreObjectPair(b, a).GetHashCode());
    }

    private class User
    {
        public int Id { get; set; }

        // Split-table tests move PasswordHash and DisplayName to secondary tables; this stays on
        // the main table so the main store object keeps at least one non-key property, as required
        // by validation.
        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
    }
}

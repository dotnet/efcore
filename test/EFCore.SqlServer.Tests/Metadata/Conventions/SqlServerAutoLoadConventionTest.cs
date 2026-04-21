// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlTypes;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

#nullable enable

public class SqlServerAutoLoadConventionTest
{
    [ConditionalFact]
    public void Vector_property_configured_as_not_auto_loaded_by_convention()
    {
        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<EntityWithVector>(
            b =>
            {
                b.Property(e => e.Vector).HasColumnType("vector(3)");
                b.Property(e => e.Name);
            });

        var model = modelBuilder.FinalizeModel();

        var entityType = model.FindEntityType(typeof(EntityWithVector))!;
        Assert.False(entityType.FindProperty(nameof(EntityWithVector.Vector))!.IsAutoLoaded);
    }

    [ConditionalFact]
    public void Vector_property_can_be_manually_configured_as_not_auto_loaded()
    {
        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<EntityWithVector>(
            b =>
            {
                b.Property(e => e.Vector).HasColumnType("vector(3)");
                b.Property(e => e.Name);
            });

        var model = modelBuilder.Model;
        var property = model.FindEntityType(typeof(EntityWithVector))!.FindProperty(nameof(EntityWithVector.Vector))!;
        property.IsAutoLoaded = false;

        var finalModel = modelBuilder.FinalizeModel();
        Assert.False(finalModel.FindEntityType(typeof(EntityWithVector))!.FindProperty(nameof(EntityWithVector.Vector))!.IsAutoLoaded);
    }

    [ConditionalFact]
    public void Explicit_auto_load_overrides_convention()
    {
        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<EntityWithVector>(
            b =>
            {
                b.Property(e => e.Vector).HasColumnType("vector(3)");
                b.Property(e => e.Name);
            });

        var model = modelBuilder.Model;
        var property = model.FindEntityType(typeof(EntityWithVector))!.FindProperty(nameof(EntityWithVector.Vector))!;
        property.IsAutoLoaded = true;

        var finalModel = modelBuilder.FinalizeModel();
        Assert.True(finalModel.FindEntityType(typeof(EntityWithVector))!.FindProperty(nameof(EntityWithVector.Vector))!.IsAutoLoaded);
    }

    [ConditionalFact]
    public void Non_vector_property_remains_auto_loaded()
    {
        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<EntityWithVector>(
            b =>
            {
                b.Property(e => e.Vector).HasColumnType("vector(3)");
                b.Property(e => e.Name);
            });

        var model = modelBuilder.FinalizeModel();

        var entityType = model.FindEntityType(typeof(EntityWithVector))!;
        Assert.True(entityType.FindProperty(nameof(EntityWithVector.Name))!.IsAutoLoaded);
    }

    private class EntityWithVector
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public SqlVector<float> Vector { get; set; }
    }
}

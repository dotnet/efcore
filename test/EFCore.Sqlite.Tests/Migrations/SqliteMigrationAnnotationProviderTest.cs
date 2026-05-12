// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

public class SqliteMigrationAnnotationProviderTest
{
    private readonly TestHelpers.TestModelBuilder _modelBuilder = SqliteTestHelpers.Instance.CreateConventionBuilder();
    private readonly SqliteAnnotationProvider _provider = new(new RelationalAnnotationProviderDependencies());
    private readonly Annotation _autoincrement = new(SqliteAnnotationNames.Autoincrement, true);

    [ConditionalFact]
    public void Does_not_add_Autoincrement_for_OnAdd_integer_property_non_key()
    {
        var property = (IProperty)_modelBuilder.Entity<Entity>().Property(e => e.IntProp).ValueGeneratedOnAdd().Metadata;
        FinalizeModel();

        Assert.DoesNotContain(
            _provider.For(property.GetTableColumnMappings().Single().Column, true),
            a => a.Name == _autoincrement.Name && (bool)a.Value!);
    }

    [ConditionalFact]
    public void Adds_Autoincrement_for_OnAdd_integer_property_primary_key()
    {
        var property = (IProperty)_modelBuilder.Entity<Entity>().Property(e => e.IntProp).ValueGeneratedOnAdd().Metadata;
        _modelBuilder.Entity<Entity>().HasKey(e => e.IntProp);
        FinalizeModel();

        Assert.Contains(
            _provider.For(property.GetTableColumnMappings().Single().Column, true),
            a => a.Name == _autoincrement.Name && (bool)a.Value!);
    }

    [ConditionalFact]
    public void Does_not_add_Autoincrement_for_OnAddOrUpdate_integer_property()
    {
        var property = (IProperty)_modelBuilder.Entity<Entity>().Property(e => e.IntProp).ValueGeneratedOnAddOrUpdate().Metadata;
        FinalizeModel();

        Assert.DoesNotContain(
            _provider.For(property.GetTableColumnMappings().Single().Column, true),
            a => a.Name == _autoincrement.Name);
    }

    [ConditionalFact]
    public void Does_not_add_Autoincrement_for_OnUpdate_integer_property()
    {
        var property = (IProperty)_modelBuilder.Entity<Entity>().Property(e => e.IntProp).ValueGeneratedOnUpdate().Metadata;
        FinalizeModel();

        Assert.DoesNotContain(
            _provider.For(property.GetTableColumnMappings().Single().Column, true),
            a => a.Name == _autoincrement.Name);
    }

    [ConditionalFact]
    public void Does_not_add_Autoincrement_for_Never_value_generated_integer_property()
    {
        var property = (IProperty)_modelBuilder.Entity<Entity>().Property(e => e.IntProp).ValueGeneratedNever().Metadata;
        FinalizeModel();

        Assert.DoesNotContain(
            _provider.For(property.GetTableColumnMappings().Single().Column, true),
            a => a.Name == _autoincrement.Name);
    }

    [ConditionalFact]
    public void Does_not_add_Autoincrement_for_default_integer_property()
    {
        var property = (IProperty)_modelBuilder.Entity<Entity>().Property(e => e.IntProp).Metadata;
        FinalizeModel();

        Assert.DoesNotContain(
            _provider.For(property.GetTableColumnMappings().Single().Column, true),
            a => a.Name == _autoincrement.Name);
    }

    [ConditionalFact]
    public void Does_not_add_Autoincrement_for_non_integer_OnAdd_property()
    {
        var property = (IProperty)_modelBuilder.Entity<Entity>().Property(e => e.StringProp).ValueGeneratedOnAdd().Metadata;
        FinalizeModel();

        Assert.DoesNotContain(
            _provider.For(property.GetTableColumnMappings().Single().Column, true),
            a => a.Name == _autoincrement.Name);
    }

    private IModel FinalizeModel()
        => _modelBuilder.FinalizeModel();

    private class Entity
    {
        public int Id { get; set; }
        public long IntProp { get; set; }
        public string? StringProp { get; set; }
    }
}

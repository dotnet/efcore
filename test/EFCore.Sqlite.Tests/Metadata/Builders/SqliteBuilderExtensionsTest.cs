// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SqliteBuilderExtensionsTest
{
    [ConditionalFact]
    public void Can_set_srid()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Geometry)
            .HasSrid(1);

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Geometry)
            .Metadata;

        Assert.Equal(1, property.GetSrid());
    }

    [ConditionalFact]
    public void Can_set_srid_non_generic()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property<string>("Geometry")
            .HasSrid(1);

        var property = modelBuilder
            .Entity<Customer>()
            .Property<string>("Geometry")
            .Metadata;

        Assert.Equal(1, property.GetSrid());
    }

    [ConditionalFact]
    public void Can_set_srid_convention()
    {
        var modelBuilder = ((IConventionModel)CreateConventionModelBuilder().Model).Builder;

        modelBuilder
            .Entity(typeof(Customer))!
            .Property(typeof(string), "Geometry")!
            .HasSrid(1);

        var property = modelBuilder
            .Entity(typeof(Customer))!
            .Property(typeof(string), "Geometry")!
            .Metadata;

        Assert.Equal(1, property.GetSrid());
    }

    #region UseSqlReturningClause

    [ConditionalFact]
    public void Can_set_UseSqlReturningClause()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Customer>();
        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer))!;

        Assert.True(entityType.IsSqlReturningClauseUsed());

        modelBuilder
            .Entity<Customer>()
            .ToTable(tb => tb.UseSqlReturningClause(false));

        Assert.False(entityType.IsSqlReturningClauseUsed());

        modelBuilder
            .Entity<Customer>()
            .ToTable(tb => tb.UseSqlReturningClause());

        Assert.True(entityType.IsSqlReturningClauseUsed());
    }

    [ConditionalFact]
    public void Can_set_UseSqlReturningClause_with_table_name_and_one_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .ToTable("foo");
        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer))!;
        var tableIdentifier = StoreObjectIdentifier.Table("foo");

        Assert.True(entityType.IsSqlReturningClauseUsed(tableIdentifier));
        Assert.True(entityType.IsSqlReturningClauseUsed());

        modelBuilder
            .Entity<Customer>()
            .ToTable("foo", tb => tb.UseSqlReturningClause(false));

        Assert.False(entityType.IsSqlReturningClauseUsed(tableIdentifier));
        Assert.False(entityType.IsSqlReturningClauseUsed());

        modelBuilder
            .Entity<Customer>()
            .ToTable("foo", tb => tb.UseSqlReturningClause());

        Assert.True(entityType.IsSqlReturningClauseUsed(tableIdentifier));
        Assert.True(entityType.IsSqlReturningClauseUsed());
    }

    [ConditionalFact]
    public void Can_set_UseSqlReturningClause_with_table_name_and_two_tables()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .ToTable("foo")
            .SplitToTable("bar", tb => tb.Property(c => c.Geometry));

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer))!;
        var fooTableIdentifier = StoreObjectIdentifier.Table("foo");
        var barTableIdentifier = StoreObjectIdentifier.Table("bar");

        Assert.True(entityType.IsSqlReturningClauseUsed(fooTableIdentifier));
        Assert.True(entityType.IsSqlReturningClauseUsed(barTableIdentifier));
        Assert.True(entityType.IsSqlReturningClauseUsed());

        modelBuilder
            .Entity<Customer>()
            .SplitToTable("bar", tb => tb.UseSqlReturningClause(false));

        Assert.False(entityType.IsSqlReturningClauseUsed(barTableIdentifier));
        Assert.True(entityType.IsSqlReturningClauseUsed(fooTableIdentifier));
        Assert.True(entityType.IsSqlReturningClauseUsed());

        modelBuilder
            .Entity<Customer>()
            .SplitToTable("bar", tb => tb.UseSqlReturningClause());

        Assert.True(entityType.IsSqlReturningClauseUsed(barTableIdentifier));
        Assert.True(entityType.IsSqlReturningClauseUsed(fooTableIdentifier));
        Assert.True(entityType.IsSqlReturningClauseUsed());
    }

    [ConditionalFact]
    public void Can_set_UseSqlReturningClause_non_generic()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity(typeof(Customer));
        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer))!;

        Assert.True(entityType.IsSqlReturningClauseUsed());

        modelBuilder
            .Entity(typeof(Customer))
            .ToTable(tb => tb.UseSqlReturningClause(false));

        Assert.False(entityType.IsSqlReturningClauseUsed());

        modelBuilder
            .Entity(typeof(Customer))
            .ToTable(tb => tb.UseSqlReturningClause());

        Assert.True(entityType.IsSqlReturningClauseUsed());
    }

    #endregion UseSqlReturningClause

    protected virtual ModelBuilder CreateConventionModelBuilder()
        => SqliteTestHelpers.Instance.CreateConventionBuilder();

    private class Customer
    {
        public int Id { get; set; }
        public string? Geometry { get; set; }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class SharedTableConventionTest
{
    [Theory]
    [InlineData(128)]
    [InlineData(20)]
    [InlineData(12)]
    public virtual void Complex_property_columns_are_uniquified_including_nested_properties(int maxLength)
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            modelBuilder.Model.SetMaxIdentifierLength(maxLength);
            var entity = modelBuilder.Entity<ColumnEntity>().ToTable("Columns");
            entity.HasKey(e => e.Id);
            var scalar = entity.Property(e => e.Details_Value).Metadata;
            var nestedScalar = entity.Property(e => e.Details_Nested_Value).Metadata;
            var complex = entity.ComplexProperty(e => e.Details).IsRequired();
            var property = complex.Property(e => e.Value).Metadata;
            var nestedProperty = complex.ComplexProperty(e => e.Nested).IsRequired().Property(e => e.Value).Metadata;

            modelBuilder.Model.FinalizeModel();

            var table = StoreObjectIdentifier.Table("Columns");
            var columnNames = new[] { scalar, nestedScalar, property, nestedProperty }
                .Select(p => p.GetColumnName(table)).ToList();
            Assert.All(columnNames, n =>
            {
                Assert.NotNull(n);
                Assert.InRange(n.Length, 1, maxLength);
            });
            Assert.Equal(4, columnNames.Distinct().Count());
            Assert.Equal(scalar.GetDefaultColumnName(table), columnNames[0]);
            Assert.Equal(nestedScalar.GetDefaultColumnName(table), columnNames[1]);
        }
    }

    [Fact]
    public virtual void Json_complex_properties_do_not_uniquify_scalar_columns()
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            var entity = modelBuilder.Entity<ColumnEntity>().ToTable("Columns");
            entity.HasKey(e => e.Id);
            var scalar = entity.Property(e => e.Details_Value).Metadata;
            var complex = entity.ComplexProperty(e => e.Details).IsRequired().ToJson();
            var property = complex.Property(e => e.Value).Metadata;
            var nestedProperty = complex.ComplexProperty(e => e.Nested).IsRequired().Property(e => e.Value).Metadata;

            modelBuilder.Model.FinalizeModel();

            var table = StoreObjectIdentifier.Table("Columns");
            Assert.Equal(nameof(ColumnEntity.Details_Value), scalar.GetColumnName(table));
            Assert.Null(property.GetColumnName(table));
            Assert.Null(nestedProperty.GetColumnName(table));
        }
    }

    [Fact]
    public virtual void Inherited_complex_properties_with_same_clr_type_have_unique_columns_in_tpc()
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            var entity = modelBuilder.Entity<TpcColumnBase>().UseTpcMappingStrategy().ToTable((string?)null);
            entity.HasKey(e => e.Id);
            var firstProperty = entity.ComplexProperty(e => e.First).IsRequired().Property(e => e.Value).Metadata;
            var secondProperty = entity.ComplexProperty(e => e.Second).IsRequired().Property(e => e.Value).Metadata;
            modelBuilder.Entity<TpcColumnDerived>().HasBaseType<TpcColumnBase>().ToTable("Columns");

            modelBuilder.Model.FinalizeModel();

            var table = StoreObjectIdentifier.Table("Columns");
            var firstColumnName = firstProperty.GetColumnName(table);
            var secondColumnName = secondProperty.GetColumnName(table);
            Assert.NotNull(firstColumnName);
            Assert.NotEmpty(firstColumnName);
            Assert.NotNull(secondColumnName);
            Assert.NotEmpty(secondColumnName);
            Assert.NotEqual(firstColumnName, secondColumnName);
        }
    }

    private abstract class TpcColumnBase
    {
        public int Id { get; set; }
        public ColumnLeaf First { get; set; } = null!;
        public ColumnLeaf Second { get; set; } = null!;
    }

    private class TpcColumnDerived : TpcColumnBase;

    private class ColumnEntity
    {
        public int Id { get; set; }
        public int Details_Value { get; set; }
        public int Details_Nested_Value { get; set; }
        public ColumnDetails Details { get; set; } = null!;
    }

    private class ColumnDetails
    {
        public int Value { get; set; }
        public ColumnLeaf Nested { get; set; } = null!;
    }

    private class ColumnLeaf
    {
        public int Value { get; set; }
    }

    [Fact]
    public virtual void Keys_are_not_uniquified_across_schemas_when_KeysUniqueAcrossSchemas_is_false()
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            modelBuilder.Entity<Order>().ToTable("MyTable", "Schema1").HasKey(e => e.Id);
            modelBuilder.Entity<Customer>().ToTable("MyTable", "Schema2").HasKey(e => e.Id);

            var finalizedModel = modelBuilder.Model.FinalizeModel();

            var orderEntityType = finalizedModel.FindEntityType(typeof(Order))!;
            var customerEntityType = finalizedModel.FindEntityType(typeof(Customer))!;

            var orderPkName = orderEntityType.FindPrimaryKey()!.GetName(
                StoreObjectIdentifier.Table("MyTable", "Schema1"));
            var customerPkName = customerEntityType.FindPrimaryKey()!.GetName(
                StoreObjectIdentifier.Table("MyTable", "Schema2"));

            Assert.Equal("PK_MyTable", orderPkName);
            Assert.Equal("PK_MyTable", customerPkName);
        }
    }

    [Fact]
    public virtual void Keys_are_uniquified_across_schemas_when_KeysUniqueAcrossSchemas_is_true()
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: true);
        using (context)
        {
            modelBuilder.Entity<Order>().ToTable("MyTable", "Schema1").HasKey(e => e.Id);
            modelBuilder.Entity<Customer>().ToTable("MyTable", "Schema2").HasKey(e => e.Id);

            var finalizedModel = modelBuilder.Model.FinalizeModel();

            var orderEntityType = finalizedModel.FindEntityType(typeof(Order))!;
            var customerEntityType = finalizedModel.FindEntityType(typeof(Customer))!;

            var orderPkName = orderEntityType.FindPrimaryKey()!.GetName(
                StoreObjectIdentifier.Table("MyTable", "Schema1"));
            var customerPkName = customerEntityType.FindPrimaryKey()!.GetName(
                StoreObjectIdentifier.Table("MyTable", "Schema2"));

            Assert.Equal("PK_MyTable", orderPkName);
            Assert.Equal("PK_MyTable1", customerPkName);
        }
    }

    private class Order
    {
        public int Id { get; set; }
    }

    private class Customer
    {
        public int Id { get; set; }
    }

    private class TestSharedTableConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : SharedTableConvention(dependencies, relationalDependencies)
    {
        protected override bool KeysUniqueAcrossTables
            => true;

        protected override bool KeysUniqueAcrossSchemas
            => false;
    }

    private (ModelBuilder, DbContext) GetModelBuilder(bool keysUniqueAcrossSchemas)
    {
        var conventionSet = new ConventionSet();

        var context = new DbContext(new DbContextOptions<DbContext>());
        var dependencies = CreateDependencies()
            .With(new CurrentDbContext(context));
        var relationalDependencies = CreateRelationalDependencies();

        if (keysUniqueAcrossSchemas)
        {
            conventionSet.ModelFinalizingConventions.Add(
                new KeysUniqueAcrossTablesSharedTableConvention(dependencies, relationalDependencies));
        }
        else
        {
            conventionSet.ModelFinalizingConventions.Add(
                new TestSharedTableConvention(dependencies, relationalDependencies));
        }

        return (new ModelBuilder(conventionSet), context);
    }

    private class KeysUniqueAcrossTablesSharedTableConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : SharedTableConvention(dependencies, relationalDependencies)
    {
        protected override bool KeysUniqueAcrossTables
            => true;
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => FakeRelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    private RelationalConventionSetBuilderDependencies CreateRelationalDependencies()
        => FakeRelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<RelationalConventionSetBuilderDependencies>();
}

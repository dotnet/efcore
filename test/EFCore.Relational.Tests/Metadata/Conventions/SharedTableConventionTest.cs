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
            Assert.Equal("Value", firstColumnName);
            Assert.Equal("TpcColumnBase_Value", secondColumnName);
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Concurrency_token_columns_on_same_entity_are_uniquified(bool complexDuplicate)
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            IMutableProperty firstProperty;
            IMutableProperty secondProperty;
            if (complexDuplicate)
            {
                var entity = modelBuilder.Entity<TpcColumnBase>().UseTpcMappingStrategy().ToTable((string?)null);
                entity.HasKey(e => e.Id);
                firstProperty = entity.ComplexProperty(e => e.First).IsRequired()
                    .Property(e => e.Value).IsConcurrencyToken().Metadata;
                secondProperty = entity.ComplexProperty(e => e.Second).IsRequired()
                    .Property(e => e.Value).IsConcurrencyToken().Metadata;
                modelBuilder.Entity<TpcColumnDerived>().HasBaseType<TpcColumnBase>().ToTable("Columns");
            }
            else
            {
                var entity = modelBuilder.Entity<ColumnEntity>().ToTable("Columns");
                entity.HasKey(e => e.Id);
                firstProperty = entity.Property(e => e.Details_Value).IsConcurrencyToken().Metadata;
                secondProperty = entity.ComplexProperty(e => e.Details).IsRequired()
                    .Property(e => e.Value).IsConcurrencyToken().Metadata;
            }

            var table = StoreObjectIdentifier.Table("Columns");
            Assert.Equal(firstProperty.GetColumnName(table), secondProperty.GetColumnName(table));

            modelBuilder.Model.FinalizeModel();

            Assert.NotEqual(firstProperty.GetColumnName(table), secondProperty.GetColumnName(table));
            Assert.Equal(ValueGenerated.Never, firstProperty.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, secondProperty.ValueGenerated);
        }
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public virtual void Concurrency_token_columns_are_not_uniquified_when_either_property_is_shadow(
        bool scalarShadow,
        bool complexShadow)
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            var entity = modelBuilder.Entity<ColumnEntity>().ToTable("Columns");
            entity.HasKey(e => e.Id);
            var scalarProperty = (scalarShadow
                ? entity.Property<int>("Shadow")
                : entity.Property(e => e.Details_Value)).IsConcurrencyToken().ValueGeneratedOnUpdate().Metadata;
            var complex = entity.ComplexProperty(e => e.Details).IsRequired();
            var complexProperty = (complexShadow
                ? complex.Property<int>("Shadow")
                : complex.Property(e => e.Value)).IsConcurrencyToken().ValueGeneratedOnUpdate().Metadata;
            ((IConventionProperty)scalarProperty).Builder.HasColumnName("Shared");
            ((IConventionProperty)complexProperty).Builder.HasColumnName("Shared");

            modelBuilder.Model.FinalizeModel();

            var table = StoreObjectIdentifier.Table("Columns");
            Assert.Equal("Shared", scalarProperty.GetColumnName(table));
            Assert.Equal("Shared", complexProperty.GetColumnName(table));
            Assert.Equal(ValueGenerated.OnUpdate, scalarProperty.ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdate, complexProperty.ValueGenerated);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public virtual void Complex_columns_in_entity_hierarchy_preserve_base_column(bool tpc, bool derivedExplicit)
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            var entity = modelBuilder.Entity<TpcColumnBase>().ToTable("Columns");
            entity.HasKey(e => e.Id);
            var baseProperty = entity.ComplexProperty(e => e.First).IsRequired().Property(e => e.Value).Metadata;
            var derivedProperty = modelBuilder.Entity<TpcColumnDerived>().HasBaseType<TpcColumnBase>().ToTable("Columns")
                .Property<int>("Duplicate").Metadata;
            ((IConventionProperty)baseProperty).Builder.HasColumnName("Shared");
            ((IConventionProperty)derivedProperty).Builder.HasColumnName("Shared");
            if (derivedExplicit)
            {
                derivedProperty.SetColumnName("Shared");
            }

            if (tpc)
            {
                entity.UseTpcMappingStrategy().ToTable((string?)null);
                modelBuilder.Entity<TpcColumnDerived>().ToTable("Columns");
            }

            modelBuilder.Model.FinalizeModel();

            var table = StoreObjectIdentifier.Table("Columns");
            Assert.Equal(derivedExplicit ? "ColumnLeaf_Shared" : "Shared", baseProperty.GetColumnName(table));
            Assert.Equal(derivedExplicit ? "Shared" : "TpcColumnDerived_Shared", derivedProperty.GetColumnName(table));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Composite_primary_key_columns_on_same_entity_are_uniquified(bool complexKey)
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            var entity = modelBuilder.Entity<ColumnEntity>().ToTable("Columns");
            var firstProperty = entity.Property(e => e.Id).Metadata;
            var secondProperty = complexKey
                ? entity.ComplexProperty(e => e.Details).IsRequired().Property(e => e.Value).Metadata
                : entity.Property(e => e.Details_Value).Metadata;
            entity.Metadata.SetPrimaryKey([firstProperty, secondProperty]);
            ((IConventionProperty)firstProperty).Builder.HasColumnName("Shared");
            ((IConventionProperty)secondProperty).Builder.HasColumnName("Shared");

            modelBuilder.Model.FinalizeModel();

            var table = StoreObjectIdentifier.Table("Columns");
            Assert.NotEqual(firstProperty.GetColumnName(table), secondProperty.GetColumnName(table));
        }
    }

    [Fact]
    public virtual void Scalar_and_complex_properties_with_same_inherited_member_are_uniquified()
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            var entity = modelBuilder.Entity<InheritedColumnBase>().ToTable("Columns");
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.Value);
            var complexProperty = entity.ComplexProperty(e => e.Details).IsRequired().Property(e => e.Value).Metadata;
            var scalarProperty = modelBuilder.Entity<InheritedColumnDerived>().HasBaseType<InheritedColumnBase>()
                .ToTable("Columns").Property(e => e.Value).Metadata;
            ((IConventionProperty)complexProperty).Builder.HasColumnName("Shared");
            ((IConventionProperty)scalarProperty).Builder.HasColumnName("Shared");

            modelBuilder.Model.FinalizeModel();

            var table = StoreObjectIdentifier.Table("Columns");
            Assert.Equal("Shared", complexProperty.GetColumnName(table));
            Assert.Equal("InheritedColumnDerived_Shared", scalarProperty.GetColumnName(table));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Complex_concurrency_columns_on_shared_table_entities_are_not_uniquified(bool siblings)
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            var first = modelBuilder.Entity<TpcColumnDerived>().ToTable("Columns");
            var second = modelBuilder.Entity<ColumnSibling>().ToTable("Columns");
            if (siblings)
            {
                modelBuilder.Entity<TpcColumnBase>().ToTable("Columns").HasKey(e => e.Id);
                first.HasBaseType<TpcColumnBase>();
                second.HasBaseType<TpcColumnBase>();
            }
            else
            {
                first.HasKey(e => e.Id);
                second.HasKey(e => e.Id);
                second.HasOne<TpcColumnDerived>().WithOne().HasForeignKey<ColumnSibling>(e => e.Id);
            }

            var firstProperty = first.ComplexProperty<ColumnLeaf>("Details").IsRequired()
                .Property(e => e.Value).IsConcurrencyToken().Metadata;
            var secondProperty = second.ComplexProperty<ColumnLeaf>("Details").IsRequired()
                .Property(e => e.Value).IsConcurrencyToken().Metadata;
            ((IConventionProperty)firstProperty).Builder.HasColumnName("Shared");
            ((IConventionProperty)secondProperty).Builder.HasColumnName("Shared");

            modelBuilder.Model.FinalizeModel();

            var table = StoreObjectIdentifier.Table("Columns");
            Assert.Equal("Shared", firstProperty.GetColumnName(table));
            Assert.Equal("Shared", secondProperty.GetColumnName(table));
            Assert.Equal(
                first.Metadata.FindProperty(nameof(TpcColumnBase.Id))!.GetColumnName(table),
                second.Metadata.FindProperty(nameof(TpcColumnBase.Id))!.GetColumnName(table));
        }
    }

    private class InheritedColumnBase : ColumnLeaf
    {
        public int Id { get; set; }
        public ColumnLeaf Details { get; set; } = null!;
    }

    private class InheritedColumnDerived : InheritedColumnBase;

    private class ColumnSibling : TpcColumnBase;

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public virtual void Complex_column_uniquification_preserves_explicit_names(bool complexExplicit, bool tableSpecific)
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            var table = StoreObjectIdentifier.Table("Columns");
            var entity = modelBuilder.Entity<ColumnEntity>().ToTable(table.Name);
            entity.HasKey(e => e.Id);
            var scalarProperty = entity.Property(e => e.Details_Value).Metadata;
            var complexProperty = entity.ComplexProperty(e => e.Details).IsRequired().Property(e => e.Value).Metadata;
            var explicitProperty = complexExplicit ? complexProperty : scalarProperty;
            var conventionProperty = complexExplicit ? scalarProperty : complexProperty;
            if (tableSpecific)
            {
                explicitProperty.SetColumnName("Shared", table);
            }
            else
            {
                explicitProperty.SetColumnName("Shared");
            }

            ((IConventionProperty)conventionProperty).Builder.HasColumnName("Shared");

            modelBuilder.Model.FinalizeModel();

            Assert.Equal("Shared", explicitProperty.GetColumnName(table));
            Assert.NotEqual("Shared", conventionProperty.GetColumnName(table));
        }
    }

    [Fact]
    public virtual void Nested_complex_columns_on_different_paths_are_uniquified_after_truncation()
    {
        var (modelBuilder, context) = GetModelBuilder(keysUniqueAcrossSchemas: false);
        using (context)
        {
            modelBuilder.Model.SetMaxIdentifierLength(12);
            var customer = modelBuilder.Entity<Customer>().ToTable("Columns");
            customer.HasKey(e => e.Id);
            var order = modelBuilder.Entity<Order>().ToTable("Columns");
            order.HasKey(e => e.Id);
            order.HasOne<Customer>().WithOne().HasForeignKey<Order>(e => e.Id).IsRequired();
            var firstProperty = customer.ComplexProperty<ColumnDetails>("LongAddressBilling").IsRequired()
                .ComplexProperty(e => e.Nested).IsRequired().Property(e => e.Value).Metadata;
            var secondProperty = order.ComplexProperty<ColumnDetails>("LongAddressShipping").IsRequired()
                .ComplexProperty(e => e.Nested).IsRequired().Property(e => e.Value).Metadata;
            var table = StoreObjectIdentifier.Table("Columns");
            Assert.Equal(firstProperty.GetColumnName(table), secondProperty.GetColumnName(table));

            modelBuilder.Model.FinalizeModel();

            Assert.NotEqual(firstProperty.GetColumnName(table), secondProperty.GetColumnName(table));
            Assert.Equal(ValueGenerated.Never, firstProperty.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, secondProperty.ValueGenerated);
        }
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

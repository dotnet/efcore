// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class SharedTableConventionTest
{
    [ConditionalFact]
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

    [ConditionalFact]
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

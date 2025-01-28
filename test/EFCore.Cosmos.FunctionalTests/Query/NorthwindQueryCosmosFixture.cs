// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Product = Microsoft.EntityFrameworkCore.TestModels.Northwind.Product;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindQueryCosmosFixture<TModelCustomizer> : NorthwindQueryFixtureBase<TModelCustomizer>
    where TModelCustomizer : ITestModelCustomizer, new()
{
    protected override ITestStoreFactory TestStoreFactory
        => CosmosNorthwindTestStoreFactory.Instance;

    protected override bool UsePooling
        => false;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Query.Name;

    public Task NoSyncTest(bool async, Func<bool, Task> testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(async, testCode);

    public void NoSyncTest(Action testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(testCode);

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(
            builder.ConfigureWarnings(
                w => w.Ignore(CosmosEventId.NoPartitionKeyDefined)));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Customer>().ToContainer("Customers");
        modelBuilder.Entity<Employee>().ToContainer("Employees");

        modelBuilder.Entity<Order>()
            .HasRootDiscriminatorInJsonId()
            .ToContainer("ProductsAndOrders");

        modelBuilder.Entity<OrderDetail>()
            .HasRootDiscriminatorInJsonId()
            .ToContainer("ProductsAndOrders");

        modelBuilder.Entity<Product>()
            .HasRootDiscriminatorInJsonId()
            .ToContainer("ProductsAndOrders");

        modelBuilder.Entity<OrderQuery>()
            .ToContainer("ProductsAndOrders")
            .HasRootDiscriminatorInJsonId()
            .HasDiscriminator<string>("$type").HasValue("Order");

        modelBuilder
            .Entity<ProductQuery>()
            .ToContainer("ProductsAndOrders")
            .HasRootDiscriminatorInJsonId()
            .HasDiscriminator<string>("$type").HasValue("Product");

        modelBuilder
            .Entity<ProductView>()
            .ToContainer("ProductsAndOrders")
            .HasRootDiscriminatorInJsonId()
            .HasDiscriminator<string>("$type").HasValue("ProductView");

        modelBuilder
            .Entity<CustomerQueryWithQueryFilter>()
            .ToContainer("Customers")
            .HasDiscriminator<string>("$type").HasValue("Customer");

        modelBuilder
            .Entity<CustomerQuery>()
            .ToContainer("Customers")
            .HasDiscriminator<string>("$type").HasValue("Customer");

        modelBuilder.Entity<Customer>().Metadata.RemoveIndex(
            modelBuilder.Entity<Customer>().Property(e => e.City).Metadata.GetContainingIndexes().Single());

        modelBuilder.Entity<Customer>().Metadata.RemoveIndex(
            modelBuilder.Entity<Customer>().Property(e => e.CompanyName).Metadata.GetContainingIndexes().Single());

        modelBuilder.Entity<Customer>().Metadata.RemoveIndex(
            modelBuilder.Entity<Customer>().Property(e => e.PostalCode).Metadata.GetContainingIndexes().Single());

        modelBuilder.Entity<Customer>().Metadata.RemoveIndex(
            modelBuilder.Entity<Customer>().Property(e => e.Region).Metadata.GetContainingIndexes().Single());

        modelBuilder.Entity<Order>().Metadata.RemoveIndex(
            modelBuilder.Entity<Order>().Property(e => e.OrderDate).Metadata.GetContainingIndexes().Single());

        modelBuilder.Entity<Product>().Metadata.RemoveIndex(
            modelBuilder.Entity<Product>().Property(e => e.ProductName).Metadata.GetContainingIndexes().Single());
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable StringStartsWithIsCultureSpecific

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public class NorthwindContext(DbContextOptions options) : PoolableDbContext(options)
{
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderDetail> OrderDetails { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<CustomerQuery> CustomerQueries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(
            e =>
            {
                e.Ignore(em => em.Address);
                e.Ignore(em => em.BirthDate);
                e.Ignore(em => em.Extension);
                e.Ignore(em => em.HireDate);
                e.Ignore(em => em.HomePhone);
                e.Ignore(em => em.LastName);
                e.Ignore(em => em.Notes);
                e.Ignore(em => em.Photo);
                e.Ignore(em => em.PhotoPath);
                e.Ignore(em => em.PostalCode);
                e.Ignore(em => em.Region);
                e.Ignore(em => em.TitleOfCourtesy);

                e.HasOne(e1 => e1.Manager).WithMany().HasForeignKey(e1 => e1.ReportsTo);
            });

        modelBuilder.Entity<Customer>(
            e =>
            {
                e.HasIndex(e => e.City);
                e.HasIndex(e => e.CompanyName);
                e.HasIndex(e => e.PostalCode);
                e.HasIndex(e => e.Region);
            });

        modelBuilder.Entity<Product>(
            e =>
            {
                e.Ignore(p => p.CategoryID);
                e.Ignore(p => p.QuantityPerUnit);
                e.Ignore(p => p.ReorderLevel);
                e.Ignore(p => p.UnitsOnOrder);

                e.HasIndex(e => e.ProductName);
            });

        modelBuilder.Entity<Order>(
            e =>
            {
                e.Ignore(o => o.Freight);
                e.Ignore(o => o.RequiredDate);
                e.Ignore(o => o.ShipAddress);
                e.Ignore(o => o.ShipCity);
                e.Ignore(o => o.ShipCountry);
                e.Ignore(o => o.ShipName);
                e.Ignore(o => o.ShipPostalCode);
                e.Ignore(o => o.ShipRegion);
                e.Ignore(o => o.ShipVia);
                e.Ignore(o => o.ShippedDate);

                e.HasIndex(e => e.OrderDate);
            });

        modelBuilder.Entity<OrderDetail>(
            e =>
            {
                e.HasKey(
                    od => new { od.OrderID, od.ProductID });
            });

        modelBuilder.Entity<CustomerQuery>().HasNoKey();
        modelBuilder.Entity<OrderQuery>().HasNoKey();
        modelBuilder.Entity<ProductQuery>().HasNoKey();
        modelBuilder.Entity<ProductView>().HasNoKey();
        modelBuilder.Entity<CustomerQueryWithQueryFilter>().HasNoKey();
    }

    public string TenantPrefix { get; set; } = "B";

    private readonly short _quantity = 50;
    public readonly string SearchTerm = "A";

    public void ConfigureFilters(ModelBuilder modelBuilder)
    {
        // Called explicitly from filter test fixtures. Code is here
        // so we can capture TenantPrefix in filter exprs (simulates OnModelCreating).

        modelBuilder.Entity<Customer>().HasQueryFilter(c => c.CompanyName.StartsWith(TenantPrefix));
        modelBuilder.Entity<Order>().HasQueryFilter(o => o.Customer != null && o.Customer.CompanyName != null);
        modelBuilder.Entity<OrderDetail>().HasQueryFilter(od => od.Order != null && EF.Property<short>(od, "Quantity") > _quantity);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => e.Address.StartsWith("A"));
        modelBuilder.Entity<Product>().HasQueryFilter(p => ClientMethod(p));
        modelBuilder.Entity<CustomerQueryWithQueryFilter>().HasQueryFilter(cq => cq.CompanyName.StartsWith(SearchTerm));
    }

    private static bool ClientMethod(Product product)
        => !product.Discontinued;

    public bool ClientMethod(Customer customer)
        => !customer.IsLondon;
}

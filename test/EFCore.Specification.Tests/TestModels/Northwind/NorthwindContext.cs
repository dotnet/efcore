// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;

// ReSharper disable StringStartsWithIsCultureSpecific

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class NorthwindContext : PoolableDbContext
    {
        public NorthwindContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }

#pragma warning disable CS0618 // Type or member is obsolete
        public virtual DbQuery<CustomerView> CustomerQueries { get; set; }
#pragma warning restore CS0618 // Type or member is obsolete

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

            modelBuilder.Entity<Product>(
                e =>
                {
                    e.Ignore(p => p.CategoryID);
                    e.Ignore(p => p.QuantityPerUnit);
                    e.Ignore(p => p.ReorderLevel);
                    e.Ignore(p => p.UnitsOnOrder);
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
                });

            modelBuilder.Entity<OrderDetail>(
                e =>
                {
                    e.HasKey(
                        od => new
                        {
                            od.OrderID,
                            od.ProductID
                        });
                });

#pragma warning disable CS0618 // Type or member is obsolete
            modelBuilder
                .Query<CustomerView>()
                .ToQuery(
                    () => Customers
                        .Select(
                            c => new CustomerView
                            {
                                Address = c.Address,
                                City = c.City,
                                CompanyName = c.CompanyName,
                                ContactName = c.ContactName,
                                ContactTitle = c.ContactTitle
                            }));

            modelBuilder
                .Query<OrderQuery>()
                .ToQuery(
                    () => Orders
                        .Select(
                            o => new OrderQuery
                            {
                                CustomerID = o.CustomerID
                            }));

            modelBuilder
                .Query<ProductQuery>()
                .ToQuery(
                    () => Products
                        .Where(p => !p.Discontinued)
                        .Select(
                            p => new ProductQuery
                            {
                                ProductID = p.ProductID,
                                ProductName = p.ProductName,
                                CategoryName = "Food"
                            }));

            modelBuilder
                .Query<CustomerQuery>()
                .HasQueryFilter(cq => cq.CompanyName.StartsWith(_searchTerm))
                .ToQuery(
                    () =>
                        Customers
                            .Include(c => c.Orders) // ignored
                            .Select(
                                c =>
                                    new CustomerQuery
                                    {
                                        CompanyName = c.CompanyName,
                                        OrderCount = c.Orders.Count(),
                                        SearchTerm = _searchTerm
                                    }));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public string TenantPrefix { get; set; } = "B";

        private readonly short _quantity = 50;
        private readonly string _searchTerm = "A";

        public void ConfigureFilters(ModelBuilder modelBuilder)
        {
            // Called explicitly from filter test fixtures. Code is here
            // so we can capture TenantPrefix in filter exprs (simulates OnModelCreating).

            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.CompanyName.StartsWith(TenantPrefix));
            modelBuilder.Entity<Order>().HasQueryFilter(o => o.Customer.CompanyName != null);
            modelBuilder.Entity<OrderDetail>().HasQueryFilter(od => EF.Property<short>(od, "Quantity") > _quantity);
            modelBuilder.Entity<Employee>().HasQueryFilter(e => e.Address.StartsWith("A"));
            modelBuilder.Entity<Product>().HasQueryFilter(p => ClientMethod(p));
        }

        private static bool ClientMethod(Product product)
            => !product.Discontinued;
    }
}

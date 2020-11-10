// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindODataContext : PoolableDbContext
    {
        public NorthwindODataContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }

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
                        od => new { od.OrderID, od.ProductID });
                });

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerID)
                .HasColumnType("nchar(5)");

            modelBuilder.Entity<Employee>(
                b =>
                {
                    b.Property(c => c.EmployeeID).HasColumnType("int");
                    b.Property(c => c.ReportsTo).HasColumnType("int");
                });

            modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(o => o.EmployeeID).HasColumnType("int");
                    b.Property(o => o.OrderDate).HasColumnType("datetime");
                });

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasColumnType("money");

            modelBuilder.Entity<Product>(
                b =>
                {
                    b.Property(p => p.UnitPrice).HasColumnType("money");
                    b.Property(p => p.UnitsInStock).HasColumnType("smallint");
                });
        }
    }
}

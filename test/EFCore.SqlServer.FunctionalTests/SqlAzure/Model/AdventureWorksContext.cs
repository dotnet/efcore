// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model;

#nullable disable

public class AdventureWorksContext(DbContextOptions options) : PoolableDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(
            entity =>
            {
                entity.HasIndex(
                    e => new
                    {
                        e.AddressLine1,
                        e.AddressLine2,
                        e.City,
                        e.StateProvince,
                        e.PostalCode,
                        e.CountryRegion
                    },
                    "IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion");

                entity.HasIndex(e => e.StateProvince, "IX_Address_StateProvince");

                entity.HasIndex(e => e.rowguid, "AK_Address_rowguid")
                    .IsUnique();

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<Customer>(
            entity =>
            {
                entity.HasIndex(e => e.EmailAddress, "IX_Customer_EmailAddress");

                entity.HasIndex(e => e.rowguid, "AK_Customer_rowguid")
                    .IsUnique();

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.PasswordHash).HasColumnType("varchar(128)");

                entity.Property(e => e.PasswordSalt).HasColumnType("varchar(10)");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<CustomerAddress>(
            entity =>
            {
                entity.HasKey(
                        e => new { e.CustomerID, e.AddressID })
                    .HasName("PK_CustomerAddress_CustomerID_AddressID");

                entity.HasIndex(e => e.rowguid, "AK_CustomerAddress_rowguid")
                    .IsUnique();

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<Product>(
            entity =>
            {
                entity.HasIndex(e => e.ProductNumber, "AK_Product_ProductNumber")
                    .IsUnique();

                entity.HasIndex(e => e.rowguid, "AK_Product_rowguid")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "AK_Product_Name")
                    .IsUnique();

                entity.Property(e => e.DiscontinuedDate).HasColumnType("datetime");

                entity.Property(e => e.ListPrice).HasColumnType("money");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.SellEndDate).HasColumnType("datetime");

                entity.Property(e => e.SellStartDate).HasColumnType("datetime");

                entity.Property(e => e.StandardCost).HasColumnType("money");

                entity.Property(e => e.ThumbNailPhoto).HasColumnType("varbinary(max)");

                entity.Property(e => e.Weight).HasColumnType("decimal");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<ProductCategory>(
            entity =>
            {
                entity.HasIndex(e => e.Name, "AK_ProductCategory_Name")
                    .IsUnique();

                entity.HasIndex(e => e.rowguid, "AK_ProductCategory_rowguid")
                    .IsUnique();

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<ProductDescription>(
            entity =>
            {
                entity.HasIndex(e => e.rowguid, "AK_ProductDescription_rowguid")
                    .IsUnique();

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<ProductModel>(
            entity =>
            {
                entity.HasIndex(e => e.Name, "AK_ProductModel_Name")
                    .IsUnique();

                entity.HasIndex(e => e.rowguid, "AK_ProductModel_rowguid")
                    .IsUnique();

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<ProductModelProductDescription>(
            entity =>
            {
                entity.HasKey(
                        e => new
                        {
                            e.ProductModelID,
                            e.ProductDescriptionID,
                            e.Culture
                        })
                    .HasName("PK_ProductModelProductDescription_ProductModelID_ProductDescriptionID_Culture");

                entity.HasIndex(e => e.rowguid, "AK_ProductModelProductDescription_rowguid")
                    .IsUnique();

                entity.Property(e => e.Culture).HasColumnType("nchar(6)");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<SalesOrderDetail>(
            entity =>
            {
                entity.HasKey(
                        e => new { e.SalesOrderID, e.SalesOrderDetailID })
                    .HasName("PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID");

                entity.HasIndex(e => e.ProductID, "IX_SalesOrderDetail_ProductID");

                entity.HasIndex(e => e.rowguid, "AK_SalesOrderDetail_rowguid")
                    .IsUnique();

                entity.Property(e => e.SalesOrderDetailID).ValueGeneratedOnAdd();

                entity.Property(e => e.LineTotal)
                    .HasColumnType("numeric")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.UnitPrice).HasColumnType("money");

                entity.Property(e => e.UnitPriceDiscount)
                    .HasColumnType("money")
                    .HasDefaultValueSql("0.0");

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.Entity<SalesOrder>(
            entity =>
            {
                entity.HasKey(e => e.SalesOrderID)
                    .HasName("PK_SalesOrderHeader_SalesOrderID");

                entity.HasIndex(e => e.CustomerID, "IX_SalesOrderHeader_CustomerID");

                entity.HasIndex(
                        e => e.SalesOrderNumber,
                        "AK_SalesOrderHeader_SalesOrderNumber")
                    .IsUnique();

                entity.HasIndex(
                        e => e.rowguid,
                        "AK_SalesOrderHeader_rowguid")
                    .IsUnique();

                entity.Property(e => e.SalesOrderID).UseHiLo("SalesOrderNumber", "SalesLT");

                entity.Property(e => e.CreditCardApprovalCode).HasColumnType("varchar(15)");

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.Freight)
                    .HasColumnType("money")
                    .HasDefaultValueSql("0.00");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.OrderDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()");

                entity.Property(e => e.RevisionNumber).HasDefaultValueSql("0");

                entity.Property(e => e.SalesOrderNumber).ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ShipDate).HasColumnType("datetime");

                entity.Property(e => e.Status).HasDefaultValueSql("1");

                entity.Property(e => e.SubTotal)
                    .HasColumnType("money")
                    .HasDefaultValueSql("0.00");

                entity.Property(e => e.TaxAmt)
                    .HasColumnType("money")
                    .HasDefaultValueSql("0.00");

                entity.Property(e => e.TotalDue)
                    .HasColumnType("money")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
            });

        modelBuilder.HasSequence<int>("SalesOrderNumber", "SalesLT");
    }

    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductCategory> ProductCategories { get; set; }
    public virtual DbSet<ProductDescription> ProductDescriptions { get; set; }
    public virtual DbSet<ProductModel> ProductModels { get; set; }
    public virtual DbSet<ProductModelProductDescription> ProductModelProductDescriptions { get; set; }
    public virtual DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
    public virtual DbSet<SalesOrder> SalesOrders { get; set; }
}

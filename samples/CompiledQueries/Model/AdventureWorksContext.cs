// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Samples.Model
{
    public class AdventureWorksContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "data source=(localdb)\\mssqllocaldb;initial catalog=AdventureWorks2014;integrated security=True;MultipleActiveResultSets=True;ConnectRetryCount=0");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(
                entity =>
                    {
                        entity.ToTable("Customer", "Sales");

                        entity.HasIndex(e => e.AccountNumber)
                            .HasName("AK_Customer_AccountNumber")
                            .IsUnique();

                        entity.HasIndex(e => e.TerritoryID)
                            .HasName("IX_Customer_TerritoryID");

                        entity.HasIndex(e => e.rowguid)
                            .HasName("AK_Customer_rowguid")
                            .IsUnique();

                        entity.Property(e => e.AccountNumber)
                            .IsRequired()
                            .HasColumnType("varchar(10)")
                            .ValueGeneratedOnAddOrUpdate();

                        entity.Property(e => e.ModifiedDate)
                            .HasColumnType("datetime")
                            .HasDefaultValueSql("getdate()");

                        entity.Property(e => e.rowguid).HasDefaultValueSql("newid()");
                    });
        }

        public virtual DbSet<Customer> Customers { get; set; }
    }
}

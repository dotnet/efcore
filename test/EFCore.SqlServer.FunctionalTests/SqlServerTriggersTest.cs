// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerTriggersTest : IClassFixture<SqlServerTriggersTest.SqlServerTriggersFixture>
    {
        public SqlServerTriggersTest(SqlServerTriggersFixture fixture)
            => Fixture = fixture;

        private SqlServerTriggersFixture Fixture { get; }

        [ConditionalFact]
        public void Triggers_run_on_insert_update_and_delete()
        {
            using var context = CreateContext();
            var product = new Product { Name = "blah" };
            context.Products.Add(product);
            context.SaveChanges();

            var firstVersion = product.Version;
            var productBackup = context.ProductBackups.AsNoTracking().Single();
            AssertEqual(product, productBackup);

            product.Name = "fooh";
            context.SaveChanges();

            Assert.NotEqual(firstVersion, product.Version);
            productBackup = context.ProductBackups.AsNoTracking().Single();
            AssertEqual(product, productBackup);

            context.Products.Remove(product);
            context.SaveChanges();

            Assert.Empty(context.Products);
            Assert.Empty(context.ProductBackups);
        }

        [ConditionalFact]
        public void Triggers_work_with_batch_operations()
        {
            using var context = CreateContext();
            var productToBeUpdated1 = new Product { Name = "u1" };
            var productToBeUpdated2 = new Product { Name = "u2" };
            context.Products.Add(productToBeUpdated1);
            context.Products.Add(productToBeUpdated2);

            var productToBeDeleted1 = new Product { Name = "d1" };
            var productToBeDeleted2 = new Product { Name = "d2" };
            context.Products.Add(productToBeDeleted1);
            context.Products.Add(productToBeDeleted2);

            context.SaveChanges();

            var productToBeAdded1 = new Product { Name = "a1" };
            var productToBeAdded2 = new Product { Name = "a2" };
            context.Products.Add(productToBeAdded1);
            context.Products.Add(productToBeAdded2);

            productToBeUpdated1.Name = "n1";
            productToBeUpdated2.Name = "n2";

            context.Products.Remove(productToBeDeleted1);
            context.Products.Remove(productToBeDeleted2);

            context.SaveChanges();

            var productBackups = context.ProductBackups.ToList();
            Assert.Equal(4, productBackups.Count);

            AssertEqual(productToBeAdded1, productBackups.Single(p => p.Name == "a1"));
            AssertEqual(productToBeAdded2, productBackups.Single(p => p.Name == "a2"));
            AssertEqual(productToBeUpdated1, productBackups.Single(p => p.Name == "n1"));
            AssertEqual(productToBeUpdated2, productBackups.Single(p => p.Name == "n2"));

            context.Products.RemoveRange(context.Products);

            context.SaveChanges();
        }

        private static void AssertEqual(Product product, ProductBackup productBackup)
        {
            Assert.Equal(product.Id, productBackup.Id);
            Assert.Equal(product.Name, productBackup.Name);
            Assert.Equal(product.Version, productBackup.Version);
        }

        protected TriggersContext CreateContext()
            => (TriggersContext)Fixture.CreateContext();

        protected class TriggersContext : PoolableDbContext
        {
            public TriggersContext(DbContextOptions options)
                : base(options)
            {
            }

            public virtual DbSet<Product> Products { get; set; }
            public virtual DbSet<ProductBackup> ProductBackups { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Product>(
                    eb =>
                    {
                        eb.Property(e => e.Version)
                            .ValueGeneratedOnAddOrUpdate()
                            .IsConcurrencyToken();
                        eb.Ignore(e => e.StoreUpdated);
                    });

                modelBuilder.Entity<ProductBackup>()
                    .Property(e => e.Id).ValueGeneratedNever();
            }
        }

        protected class Product
        {
            public virtual int Id { get; set; }
            public virtual byte[] Version { get; set; }
            public virtual string Name { get; set; }
            public virtual int StoreUpdated { get; set; }
        }

        protected class ProductBackup
        {
            public virtual int Id { get; set; }
            public virtual byte[] Version { get; set; }
            public virtual string Name { get; set; }
        }

        public class SqlServerTriggersFixture : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "SqlServerTriggers";
            protected override Type ContextType { get; } = typeof(TriggersContext);

            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            protected override void Seed(PoolableDbContext context)
            {
                context.Database.EnsureCreatedResiliently();

                context.Database.ExecuteSqlRaw(
                    @"
CREATE TRIGGER TRG_InsertProduct
ON Products
AFTER INSERT AS
BEGIN
    IF @@ROWCOUNT = 0
        return
    SET nocount on;

    INSERT INTO ProductBackups
    SELECT * FROM INSERTED;
END");

                context.Database.ExecuteSqlRaw(
                    @"
CREATE TRIGGER TRG_UpdateProduct
ON Products
AFTER UPDATE AS
BEGIN
    IF @@ROWCOUNT = 0
        return
    SET nocount on;

    UPDATE b
    SET b.Name = p.Name, b.Version = p.Version
    FROM ProductBackups b
    INNER JOIN Products p
        ON b.Id = p.Id
    WHERE p.Id IN(SELECT INSERTED.Id FROM INSERTED);
END");

                context.Database.ExecuteSqlRaw(
                    @"
CREATE TRIGGER TRG_DeleteProduct
ON Products
AFTER DELETE AS
BEGIN
    IF @@ROWCOUNT = 0
        return
    SET nocount on;

    DELETE FROM ProductBackups
    WHERE Id IN(SELECT DELETED.Id FROM DELETED);
END");
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class SqlServerTriggersTest : IClassFixture<SqlServerTriggersTest.SqlServerTriggersFixture>
    {
        [Fact]
        public void Triggers_run_on_insert_update_and_delete()
        {
            using (var testStore = GetTriggersTestStore())
            {
                using (var context = CreateTriggersContext(testStore))
                {
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
            }
        }

        [Fact]
        public void Triggers_work_with_batch_operations()
        {
            using (var testStore = GetTriggersTestStore())
            {
                using (var context = CreateTriggersContext(testStore))
                {
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
                }
            }
        }

        private static void AssertEqual(Product product, ProductBackup productBackup)
        {
            Assert.Equal(product.Id, productBackup.Id);
            Assert.Equal(product.Name, productBackup.Name);
            Assert.Equal(product.Version, productBackup.Version);
        }

        [Fact]
        public void Triggers_with_subqueries_run_on_insert_update_and_delete()
        {
            using (var testStore = GetQueryTriggersTestStore())
            {
                using (var context = CreateQueryTriggersContext(testStore))
                {
                    var product = new Product { Name = "blah" };
                    context.Products.Add(product);
                    context.SaveChanges();

                    Assert.Equal(1, product.StoreUpdated);

                    product.Name = "fooh";
                    context.SaveChanges();

                    Assert.Equal(2, product.StoreUpdated);

                    context.Products.Remove(product);
                    context.SaveChanges();

                    Assert.Empty(context.Products);
                }
            }
        }

        [Fact]
        public void Triggers_with_subqueries_work_with_batch_operations()
        {
            using (var testStore = GetQueryTriggersTestStore())
            {
                using (var context = CreateQueryTriggersContext(testStore))
                {
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

                    Assert.Equal(1, productToBeAdded1.StoreUpdated);
                    Assert.Equal(1, productToBeAdded2.StoreUpdated);
                    Assert.Equal(2, productToBeUpdated1.StoreUpdated);
                    Assert.Equal(2, productToBeUpdated2.StoreUpdated);

                    var productBackups = context.Products.ToList();
                    Assert.Equal(4, productBackups.Count);
                }
            }
        }

        private TriggersContext CreateTriggersContext(SqlServerTestStore testStore)
            => new TriggersContext(new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(_fixture.ServiceProvider)
                .UseSqlServer(testStore.Connection, b => b.ApplyConfiguration()).Options);

        private QueryTriggersContext CreateQueryTriggersContext(SqlServerTestStore testStore)
            => new QueryTriggersContext(new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(_fixture.ServiceProvider)
                .UseSqlServer(testStore.Connection, b => b.ApplyConfiguration()).Options);

        private SqlServerTestStore GetTriggersTestStore()
        {
            var testStore = SqlServerTestStore.Create("SqlServerTriggers");
            using (var context = CreateTriggersContext(testStore))
            {
                context.Database.EnsureCreated();
            }

            testStore.ExecuteNonQuery(@"
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

            testStore.ExecuteNonQuery(@"
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

            testStore.ExecuteNonQuery(@"
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
            return testStore;
        }

        private SqlServerTestStore GetQueryTriggersTestStore()
        {
            var testStore = SqlServerTestStore.Create("SqlServerTriggers");
            using (var context = CreateQueryTriggersContext(testStore))
            {
                context.Database.EnsureCreated();
            }

            testStore.ExecuteNonQuery(@"
CREATE TRIGGER TRG_InsertUpdateProduct
ON UpdatedProducts
AFTER INSERT, UPDATE AS
BEGIN
	IF @@ROWCOUNT = 0
		return
	SET nocount on;

    UPDATE UpdatedProducts set StoreUpdated = StoreUpdated + 1
    WHERE Id IN(SELECT INSERTED.Id FROM INSERTED);
END");
            return testStore;
        }

        public class TriggersContext : DbContext
        {
            public TriggersContext(DbContextOptions options)
                : base(options)
            {
            }

            public virtual DbSet<Product> Products { get; set; }
            public virtual DbSet<ProductBackup> ProductBackups { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Product>(eb =>
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

        public class QueryTriggersContext : DbContext
        {
            public QueryTriggersContext(DbContextOptions options)
                : base(options)
            {
            }

            public virtual DbSet<Product> Products { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Product>(eb =>
                    {
                        eb.Property(e => e.StoreUpdated)
                            .HasDefaultValue(0)
                            .ValueGeneratedOnAddOrUpdate();
                        eb.ToTable("UpdatedProducts");
                    });
            }
        }

        public class Product
        {
            public virtual int Id { get; set; }
            public virtual byte[] Version { get; set; }
            public virtual string Name { get; set; }
            public virtual int StoreUpdated { get; set; }
        }

        public class ProductBackup
        {
            public virtual int Id { get; set; }
            public virtual byte[] Version { get; set; }
            public virtual string Name { get; set; }
        }

        private readonly SqlServerTriggersFixture _fixture;

        public SqlServerTriggersTest(SqlServerTriggersFixture fixture)
        {
            _fixture = fixture;
        }

        public class SqlServerTriggersFixture
        {
            public IServiceProvider ServiceProvider { get; }

            public SqlServerTriggersFixture()
            {
                ServiceProvider
                    = new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .BuildServiceProvider();
            }
        }
    }
}

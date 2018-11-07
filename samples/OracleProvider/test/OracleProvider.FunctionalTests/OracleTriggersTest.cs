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
    public class OracleTriggersTest : IClassFixture<OracleTriggersTest.OracleTriggersFixture>
    {
        public OracleTriggersTest(OracleTriggersFixture fixture) => Fixture = fixture;

        private OracleTriggersFixture Fixture { get; }

        [Fact]
        public void Triggers_run_on_insert_update_and_delete()
        {
            using (var context = CreateContext())
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

        [Fact]
        public void Triggers_work_with_batch_operations()
        {
            using (var context = CreateContext())
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

                context.Products.RemoveRange(context.Products);

                context.SaveChanges();
            }
        }

        private static void AssertEqual(Product product, ProductBackup productBackup)
        {
            Assert.Equal(product.Id, productBackup.Id);
            Assert.Equal(product.Name, productBackup.Name);
            Assert.Equal(product.Version, productBackup.Version);
        }

        protected TriggersContext CreateContext() => (TriggersContext)Fixture.CreateContext();

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

        public class OracleTriggersFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "OracleTriggers";
            protected override Type ContextType { get; } = typeof(TriggersContext);
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

            protected override void Seed(DbContext context)
            {
                context.Database.EnsureCreated();

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE TRIGGER ""TRG_InsertProduct""
AFTER INSERT ON ""Products"" FOR EACH ROW
BEGIN
  INSERT INTO ""ProductBackups""(""Id"", ""Name"", ""Version"")
  VALUES (:NEW.""Id"", :NEW.""Name"", :NEW.""Version"");
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE TRIGGER ""TRG_UpdateProduct""
AFTER UPDATE ON ""Products"" FOR EACH ROW
BEGIN
  UPDATE ""ProductBackups"" b
  SET b.""Name"" = :NEW.""Name"", b.""Version"" = :NEW.""Version""
  WHERE b.""Id"" = :NEW.""Id"";
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE TRIGGER ""TRG_DeleteProduct""
AFTER DELETE ON ""Products"" FOR EACH ROW
BEGIN
  DELETE FROM ""ProductBackups""
  WHERE ""Id"" = :OLD.""Id"";
END;");
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqlServerQueryTriggersTest(SqlServerQueryTriggersTest.SqlServerTriggersFixture fixture)
    : IClassFixture<SqlServerQueryTriggersTest.SqlServerTriggersFixture>
{
    private SqlServerTriggersFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public void Triggers_with_subqueries_run_on_insert_update_and_delete()
    {
        using var context = CreateContext();
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

    [ConditionalFact]
    public void Triggers_with_subqueries_work_with_batch_operations()
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

        Assert.Equal(1, productToBeAdded1.StoreUpdated);
        Assert.Equal(1, productToBeAdded2.StoreUpdated);
        Assert.Equal(2, productToBeUpdated1.StoreUpdated);
        Assert.Equal(2, productToBeUpdated2.StoreUpdated);

        var products = context.Products.ToList();
        Assert.Equal(4, products.Count);

        context.Products.RemoveRange(products);

        context.SaveChanges();
    }

    protected QueryTriggersContext CreateContext()
        => (QueryTriggersContext)Fixture.CreateContext();

    protected class QueryTriggersContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public virtual DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Product>(
                eb =>
                {
                    eb.Property(e => e.StoreUpdated)
                        .HasDefaultValue(0)
                        .ValueGeneratedOnAddOrUpdate();
                    eb.ToTable("UpdatedProducts", tb => tb.HasTrigger("TRG_InsertUpdateProduct"));
                });
    }

    protected class Product
    {
        public virtual int Id { get; set; }
        public virtual byte[] Version { get; set; }
        public virtual string Name { get; set; }
        public virtual int StoreUpdated { get; set; }
    }

    public class SqlServerTriggersFixture : SharedStoreFixtureBase<DbContext>
    {
        protected override string StoreName
            => "SqlServerQueryTriggers";

        protected override Type ContextType { get; } = typeof(QueryTriggersContext);

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override async Task SeedAsync(DbContext context)
        {
            await context.Database.EnsureCreatedResilientlyAsync();

            await context.Database.ExecuteSqlRawAsync(
                @"
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
        }
    }
}

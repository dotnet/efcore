// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public abstract class UpdatesRelationalTestBase<TFixture> : UpdatesTestBase<TFixture>
    where TFixture : UpdatesRelationalTestBase<TFixture>.UpdatesRelationalFixture
{
    protected UpdatesRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public virtual Task SaveChanges_works_for_entities_also_mapped_to_view()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var category = await context.Categories.SingleAsync();

                context.Add(
                    new ProductTableWithView
                    {
                        Id = Guid.NewGuid(),
                        Name = "Pear Cider",
                        Price = 1.39M,
                        DependentId = category.Id
                    });
                context.Add(
                    new ProductViewTable
                    {
                        Id = Guid.NewGuid(),
                        Name = "Pear Cobler",
                        Price = 2.39M,
                        DependentId = category.Id
                    });

                await context.SaveChangesAsync();
            }, async context =>
            {
                var viewProduct = await context.Set<ProductTableWithView>().SingleAsync();
                var tableProduct = await context.Set<ProductTableView>().SingleAsync();

                Assert.Equal("Pear Cider", tableProduct.Name);
                Assert.Equal("Pear Cobler", viewProduct.Name);
            });

    [ConditionalFact]
    public virtual Task SaveChanges_throws_for_entities_only_mapped_to_view()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var category = await context.Categories.SingleAsync();
                context.Add(
                    new ProductTableView
                    {
                        Id = Guid.NewGuid(),
                        Name = "Pear Cider",
                        Price = 1.39M,
                        DependentId = category.Id
                    });

                Assert.Equal(
                    RelationalStrings.ReadonlyEntitySaved(nameof(ProductTableView)),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    [ConditionalFact]
    public virtual Task Save_with_shared_foreign_key()
    {
        Guid productId = default;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var product = new ProductWithBytes();
                context.Add(product);

                await context.SaveChangesAsync();

                productId = product.Id;
            }, async context =>
            {
                var product = (await context.ProductWithBytes.FindAsync(productId))!;
                var category = new SpecialCategory { PrincipalId = 777 };
                var productCategory = new ProductCategory { Category = category };
                product.ProductCategories = new List<ProductCategory> { productCategory };

                await context.SaveChangesAsync();

                Assert.True(category.Id > 0);
                Assert.Equal(category.Id, productCategory.CategoryId);
            }, async context =>
            {
                var product = await context.Set<ProductBase>()
                    .Include(p => ((ProductWithBytes)p).ProductCategories)
                    .Include(p => ((Product)p).ProductCategories)
                    .OfType<ProductWithBytes>()
                    .SingleAsync();
                var productCategory = product.ProductCategories.Single();
                Assert.Equal(productCategory.CategoryId, (await context.Set<ProductCategory>().SingleAsync()).CategoryId);
                Assert.Equal(productCategory.CategoryId, (await context.Set<SpecialCategory>().SingleAsync(c => c.PrincipalId == 777)).Id);
            });
    }

    [ConditionalFact]
    public virtual Task Can_use_shared_columns_with_conversion()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var person = new Person("1", null)
                {
                    Address = new Address { Country = Country.Eswatini, City = "Bulembu" }, Country = "Eswatini"
                };

                context.Add(person);

                return context.SaveChangesAsync();
            }, async context =>
            {
                var person = await context.Set<Person>().SingleAsync();
                person.Address = new Address
                {
                    Country = Country.Türkiye,
                    City = "Konya",
                    ZipCode = 42100
                };

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var person = await context.Set<Person>().SingleAsync();

                Assert.Equal(Country.Türkiye, person.Address!.Country);
                Assert.Equal("Konya", person.Address.City);
                Assert.Equal(42100, person.Address.ZipCode);
                Assert.Equal("Türkiye", person.Country);
                Assert.Equal("42100", person.ZipCode);
            });

    [ConditionalFact]
    public virtual Task Swap_filtered_unique_index_values()
    {
        var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
        var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var product1 = (await context.Products.FindAsync(productId1))!;
                var product2 = (await context.Products.FindAsync(productId2))!;

                product2.Name = null;
                product2.Price = product1.Price;

                await context.SaveChangesAsync();
            }, async context =>
            {
                var product1 = (await context.Products.FindAsync(productId1))!;
                var product2 = (await context.Products.FindAsync(productId2))!;

                product2.Name = product1.Name;
                product1.Name = null;

                await context.SaveChangesAsync();
            }, async context =>
            {
                var product1 = (await context.Products.FindAsync(productId1))!;
                var product2 = (await context.Products.FindAsync(productId2))!;

                Assert.Equal(1.49M, product1.Price);
                Assert.Null(product1.Name);
                Assert.Equal(1.49M, product2.Price);
                Assert.Equal("Apple Cider", product2.Name);
            });
    }

    [ConditionalFact]
    public virtual Task Update_non_indexed_values()
    {
        var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
        var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var product1 = (await context.Products.FindAsync(productId1))!;
                var product2 = (await context.Products.FindAsync(productId2))!;

                product2.Price = product1.Price;

                await context.SaveChangesAsync();
            },
            context =>
            {
                var product1 = new Product
                {
                    Id = productId1,
                    Name = "",
                    Price = 1.49M
                };
                var product2 = new Product
                {
                    Id = productId2,
                    Name = "",
                    Price = 1.49M
                };

                context.Attach(product1).Property(p => p.DependentId).IsModified = true;
                context.Attach(product2).Property(p => p.DependentId).IsModified = true;

                return context.SaveChangesAsync();
            }, async context =>
            {
                var product1 = (await context.Products.FindAsync(productId1))!;
                var product2 = (await context.Products.FindAsync(productId2))!;

                Assert.Equal(1.49M, product1.Price);
                Assert.Null(product1.DependentId);
                Assert.Equal(1.49M, product2.Price);
                Assert.Null(product2.DependentId);
            });
    }

    [ConditionalFact]
    public abstract void Identifiers_are_generated_correctly();

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override string UpdateConcurrencyMessage
        => RelationalStrings.UpdateConcurrencyException(1, 0);

    protected override string UpdateConcurrencyTokenMessage
        => RelationalStrings.UpdateConcurrencyException(1, 0);

    public abstract class UpdatesRelationalFixture : UpdatesFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<ProductViewTable>().HasBaseType((string)null).ToTable("ProductView");
            modelBuilder.Entity<ProductTableWithView>().HasBaseType((string)null).ToView("ProductView").ToTable("ProductTable");
            modelBuilder.Entity<ProductTableView>().HasBaseType((string)null).ToView("ProductTable");

            modelBuilder.Entity<Product>().HasIndex(p => new { p.Name, p.Price }).IsUnique();

            modelBuilder.Entity<Person>(
                pb =>
                {
                    pb.Property(p => p.Country)
                        .HasColumnName("Country");
                    pb.Property(p => p.ZipCode)
                        .HasColumnName("ZipCode");
                    pb.OwnsOne(p => p.Address)
                        .Property(p => p.Country)
                        .HasColumnName("Country");
                    pb.OwnsOne(p => p.Address)
                        .Property(p => p.ZipCode)
                        .HasColumnName("ZipCode");
                });

            modelBuilder
                .Entity<
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
                >(
                    eb =>
                    {
                        eb.HasKey(
                                l => new { l.ProfileId })
                            .HasName("PK_LoginDetails");

                        eb.HasOne(d => d.Login).WithOne()
                            .HasConstraintName("FK_LoginDetails_Login");
                    });
        }
    }
}

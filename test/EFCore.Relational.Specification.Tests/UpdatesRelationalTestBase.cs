// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public abstract class UpdatesRelationalTestBase<TFixture> : UpdatesTestBase<TFixture>
    where TFixture : UpdatesRelationalTestBase<TFixture>.UpdatesRelationalFixture
{
    protected UpdatesRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public virtual void SaveChanges_works_for_entities_also_mapped_to_view()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var category = context.Categories.Single();

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

                context.SaveChanges();
            },
            context =>
            {
                var viewProduct = context.Set<ProductTableWithView>().Single();
                var tableProduct = context.Set<ProductTableView>().Single();

                Assert.Equal("Pear Cider", tableProduct.Name);
                Assert.Equal("Pear Cobler", viewProduct.Name);
            });

    [ConditionalFact]
    public virtual void SaveChanges_throws_for_entities_only_mapped_to_view()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var category = context.Categories.Single();
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
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            });

    [ConditionalFact]
    public virtual void Save_with_shared_foreign_key()
    {
        Guid productId = default;
        ExecuteWithStrategyInTransaction(
            context =>
            {
                var product = new ProductWithBytes();
                context.Add(product);

                context.SaveChanges();

                productId = product.Id;
            },
            context =>
            {
                var product = context.ProductWithBytes.Find(productId)!;
                var category = new SpecialCategory { PrincipalId = 777 };
                var productCategory = new ProductCategory { Category = category };
                product.ProductCategories = new List<ProductCategory> { productCategory };

                context.SaveChanges();

                Assert.True(category.Id > 0);
                Assert.Equal(category.Id, productCategory.CategoryId);
            },
            context =>
            {
                var product = context.Set<ProductBase>()
                    .Include(p => ((ProductWithBytes)p).ProductCategories)
                    .Include(p => ((Product)p).ProductCategories)
                    .OfType<ProductWithBytes>()
                    .Single();
                var productCategory = product.ProductCategories.Single();
                Assert.Equal(productCategory.CategoryId, context.Set<ProductCategory>().Single().CategoryId);
                Assert.Equal(productCategory.CategoryId, context.Set<SpecialCategory>().Single(c => c.PrincipalId == 777).Id);
            });
    }

    [ConditionalFact]
    public virtual void Swap_filtered_unique_index_values()
    {
        var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
        var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

        ExecuteWithStrategyInTransaction(
            context =>
            {
                var product1 = context.Products.Find(productId1)!;
                var product2 = context.Products.Find(productId2)!;

                product2.Name = null;
                product2.Price = product1.Price;

                context.SaveChanges();
            },
            context =>
            {
                var product1 = context.Products.Find(productId1)!;
                var product2 = context.Products.Find(productId2)!;

                product2.Name = product1.Name;
                product1.Name = null;

                context.SaveChanges();
            },
            context =>
            {
                var product1 = context.Products.Find(productId1)!;
                var product2 = context.Products.Find(productId2)!;

                Assert.Equal(1.49M, product1.Price);
                Assert.Null(product1.Name);
                Assert.Equal(1.49M, product2.Price);
                Assert.Equal("Apple Cider", product2.Name);
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

            modelBuilder.Entity<Product>().HasIndex(p => new { p.Name, p.Price }).IsUnique().HasFilter("Name IS NOT NULL");

            modelBuilder.Entity<Person>()
                .Property(p => p.Country)
                .HasColumnName("Country");

            modelBuilder.Entity<Person>()
                .OwnsOne(p => p.Address)
                .Property(p => p.Country)
                .HasColumnName("Country");

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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public abstract partial class ModelBuilderTest
{
    public abstract class ManyToManyTestBase : ModelBuilderTestBase
    {
        protected ManyToManyTestBase(ModelBuilderFixtureBase fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public virtual void Discovers_navigations()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Category>().Ignore(c => c.ProductCategories);
            modelBuilder.Entity<Product>();
            modelBuilder.Entity<CategoryBase>();

            var sharedTypeName = nameof(Category) + nameof(Product);

            modelBuilder.SharedTypeEntity<Dictionary<string, object>>(sharedTypeName);

            var model = modelBuilder.FinalizeModel();

            var productType = model.FindEntityType(typeof(Product))!;
            var categoryType = model.FindEntityType(typeof(Category))!;

            var categoriesNavigation = productType.GetSkipNavigations().Single();
            var productsNavigation = categoryType.GetSkipNavigations().Single();

            var categoriesFk = categoriesNavigation.ForeignKey;
            var productsFk = productsNavigation.ForeignKey;
            var productCategoryType = categoriesFk.DeclaringEntityType;

            Assert.Equal(typeof(Dictionary<string, object>), productCategoryType.ClrType);
            Assert.Equal(sharedTypeName, productCategoryType.Name);
            Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
            Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
            Assert.Equal(2, productCategoryType.GetForeignKeys().Count());
        }

        [ConditionalFact]
        public virtual void Finds_existing_navigations_and_uses_associated_FK()
        {
            var modelBuilder = CreateModelBuilder();
            var model = (IReadOnlyModel)modelBuilder.Model;

            modelBuilder.Entity<Category>().Ignore(c => c.Products);
            modelBuilder.Entity<Product>().Ignore(p => p.Categories);

            modelBuilder.Entity<Category>()
                .HasMany(o => o.Products).WithMany(c => c.Categories)
                .UsingEntity<ProductCategory>(
                    pcb => pcb.HasOne(pc => pc.Product).WithMany(),
                    pcb => pcb.HasOne(pc => pc.Category).WithMany(c => c.ProductCategories))
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            var productType = model.FindEntityType(typeof(Product))!;
            var categoryType = model.FindEntityType(typeof(Category))!;
            var productCategoryType = model.FindEntityType(typeof(ProductCategory))!;

            var categoriesNavigation = productType.GetSkipNavigations().Single();
            var productsNavigation = categoryType.GetSkipNavigations().Single();

            var categoriesFk = categoriesNavigation.ForeignKey;
            var productsFk = productsNavigation.ForeignKey;

            Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
            Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
            Assert.Equal(2, productCategoryType.GetForeignKeys().Count());

            modelBuilder.Entity<Category>()
                .HasMany(o => o.Products).WithMany(c => c.Categories)
                .UsingEntity<ProductCategory>(
                    pcb => pcb.HasOne(pc => pc.Product).WithMany(),
                    pcb => pcb.HasOne(pc => pc.Category).WithMany(c => c.ProductCategories));

            model = modelBuilder.FinalizeModel();

            Assert.Same(categoriesNavigation, productType.GetSkipNavigations().Single());
            Assert.Same(productsNavigation, categoryType.GetSkipNavigations().Single());
            Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
            Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
            Assert.Equal(2, productCategoryType.GetForeignKeys().Count());
        }

        [ConditionalFact]
        public virtual void Finds_existing_navigations_and_uses_associated_FK_with_implicit_relationships()
        {
            var modelBuilder = CreateModelBuilder();
            var model = (IReadOnlyModel)modelBuilder.Model;

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Categories).WithMany(c => c.Products)
                .UsingEntity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Categories).WithMany(c => c.Products)
                .UsingEntity<ProductCategory>(pcb => pcb.HasKey(pc => new { pc.ProductId, pc.CategoryId }));

            var productType = model.FindEntityType(typeof(Product))!;
            var categoryType = model.FindEntityType(typeof(Category))!;
            var productCategoryType = model.FindEntityType(typeof(ProductCategory))!;

            var categoriesNavigation = productType.GetSkipNavigations().Single();
            var productsNavigation = categoryType.GetSkipNavigations().Single();

            var categoriesFk = categoriesNavigation.ForeignKey;
            var productsFk = productsNavigation.ForeignKey;

            Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
            Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
            Assert.Equal(2, productCategoryType.GetForeignKeys().Count());

            modelBuilder.Entity<Category>()
                .HasMany(o => o.Products).WithMany(c => c.Categories)
                .UsingEntity<ProductCategory>(
                    pcb => pcb.HasOne(pc => pc.Product).WithMany(),
                    pcb => pcb.HasOne(pc => pc.Category).WithMany(c => c.ProductCategories));

            model = modelBuilder.FinalizeModel();

            Assert.Same(categoriesNavigation, productType.GetSkipNavigations().Single());
            Assert.Same(productsNavigation, categoryType.GetSkipNavigations().Single());
            Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
            Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
            Assert.Equal(2, productCategoryType.GetForeignKeys().Count());
        }

        [ConditionalFact]
        public virtual void Finds_existing_navigations_and_uses_associated_FK_with_fields()
        {
            var modelBuilder = CreateModelBuilder();
            var model = (IReadOnlyModel)modelBuilder.Model;

            modelBuilder.Entity<ManyToManyPrincipalWithField>()
                .HasMany(p => p.Dependents)
                .WithMany(d => d.ManyToManyPrincipals)
                .UsingEntity<ManyToManyJoinWithFields>(
                    jwf => jwf.HasOne(j => j.DependentWithField)
                        .WithMany(),
                    jwf => jwf.HasOne(j => j.ManyToManyPrincipalWithField)
                        .WithMany())
                .HasKey(j => new { j.DependentWithFieldId, j.ManyToManyPrincipalWithFieldId });

            modelBuilder.Entity<ManyToManyPrincipalWithField>(
                e =>
                {
                    e.Property(p => p.Id);
                    e.Property(p => p.Name);
                    e.HasKey(p => p.Id);
                });
            modelBuilder.Entity<DependentWithField>(
                e =>
                {
                    e.Property(d => d.DependentWithFieldId);
                    e.Property(d => d.AnotherOneToManyPrincipalId);
                    e.Ignore(d => d.OneToManyPrincipal);
                    e.Ignore(d => d.OneToOnePrincipal);
                    e.HasKey(d => d.DependentWithFieldId);
                });

            var principalEntityType = model.FindEntityType(typeof(ManyToManyPrincipalWithField))!;
            var dependentEntityType = model.FindEntityType(typeof(DependentWithField))!;
            var joinEntityType = model.FindEntityType(typeof(ManyToManyJoinWithFields))!;

            var principalToJoinNav = principalEntityType.GetSkipNavigations().Single();
            var dependentToJoinNav = dependentEntityType.GetSkipNavigations().Single();

            var principalToDependentFk = principalToJoinNav.ForeignKey;
            var dependentToPrincipalFk = dependentToJoinNav.ForeignKey;

            Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            Assert.Same(principalToDependentFk, joinEntityType.GetForeignKeys().Last());
            Assert.Same(dependentToPrincipalFk, joinEntityType.GetForeignKeys().First());

            modelBuilder.Entity<ManyToManyPrincipalWithField>()
                .HasMany(p => p.Dependents)
                .WithMany(d => d.ManyToManyPrincipals)
                .UsingEntity<ManyToManyJoinWithFields>(
                    jwf => jwf.HasOne(j => j.DependentWithField)
                        .WithMany(),
                    jwf => jwf.HasOne(j => j.ManyToManyPrincipalWithField)
                        .WithMany());

            model = modelBuilder.FinalizeModel();

            Assert.Same(principalToJoinNav, principalEntityType.GetSkipNavigations().Single());
            Assert.Same(dependentToJoinNav, dependentEntityType.GetSkipNavigations().Single());
            Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            Assert.Same(principalToDependentFk, joinEntityType.GetForeignKeys().Last());
            Assert.Same(dependentToPrincipalFk, joinEntityType.GetForeignKeys().First());
        }

        [ConditionalFact]
        public virtual void Join_type_is_automatically_configured_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ImplicitManyToManyA>();

            var model = modelBuilder.FinalizeModel();

            var manyToManyA = model.FindEntityType(typeof(ImplicitManyToManyA))!;
            var manyToManyB = model.FindEntityType(typeof(ImplicitManyToManyB))!;
            var joinEntityType = model.GetEntityTypes()
                .Where(et => et.ClrType == Model.DefaultPropertyBagType)
                .Single();
            Assert.Equal("ImplicitManyToManyAImplicitManyToManyB", joinEntityType.Name);

            var navigationOnManyToManyA = manyToManyA.GetSkipNavigations().Single();
            var navigationOnManyToManyB = manyToManyB.GetSkipNavigations().Single();
            Assert.Equal("Bs", navigationOnManyToManyA.Name);
            Assert.Equal("As", navigationOnManyToManyB.Name);
            Assert.Same(navigationOnManyToManyA.Inverse, navigationOnManyToManyB);
            Assert.Same(navigationOnManyToManyB.Inverse, navigationOnManyToManyA);

            var manyToManyAForeignKey = navigationOnManyToManyA.ForeignKey;
            var manyToManyBForeignKey = navigationOnManyToManyB.ForeignKey;
            Assert.NotNull(manyToManyAForeignKey);
            Assert.NotNull(manyToManyBForeignKey);
            Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            Assert.Equal(manyToManyAForeignKey.DeclaringEntityType, joinEntityType);
            Assert.Equal(manyToManyBForeignKey.DeclaringEntityType, joinEntityType);

            var key = joinEntityType.FindPrimaryKey()!;
            Assert.Equal(
                new[]
                {
                    nameof(ImplicitManyToManyB.As) + nameof(ImplicitManyToManyA.Id),
                    nameof(ImplicitManyToManyA.Bs) + nameof(ImplicitManyToManyB.Id)
                },
                key.Properties.Select(p => p.Name));

            Assert.DoesNotContain(joinEntityType.GetProperties(), p => !p.IsIndexerProperty());
        }

        [ConditionalFact]
        public virtual void Join_type_is_not_automatically_configured_when_navigations_are_ambiguous()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Hob>();

            var hob = model.FindEntityType(typeof(Hob))!;
            var nob = model.FindEntityType(typeof(Nob))!;
            Assert.Empty(
                model.GetEntityTypes()
                    .Where(et => ((EntityType)et).IsImplicitlyCreatedJoinEntityType));

            Assert.Empty(hob.GetSkipNavigations());
            if (nob != null)
            {
                Assert.Empty(nob.GetSkipNavigations());
            }
        }

        [ConditionalFact]
        public virtual void Can_configure_join_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Category>().Ignore(c => c.Products);
            modelBuilder.Entity<Product>().Ignore(p => p.Categories);

            var manyToMany = modelBuilder.Entity<Category>()
                .HasMany(o => o.Products).WithMany(c => c.Categories)
                .UsingEntity<ProductCategory>(
                    pcb => pcb.HasOne(pc => pc.Product).WithMany(),
                    pcb => pcb.HasOne(pc => pc.Category).WithMany(c => c.ProductCategories),
                    pcb => pcb.HasKey(pc => new { pc.ProductId, pc.CategoryId }));

            Assert.Equal(typeof(Category), manyToMany.Metadata.ClrType);

            var model = modelBuilder.FinalizeModel();

            var productType = model.FindEntityType(typeof(Product))!;
            var categoryType = model.FindEntityType(typeof(Category))!;
            var productCategoryType = model.FindEntityType(typeof(ProductCategory))!;

            var categoriesNavigation = productType.GetSkipNavigations().Single();
            var productsNavigation = categoryType.GetSkipNavigations().Single();

            var categoriesFk = categoriesNavigation.ForeignKey;
            var productsFk = productsNavigation.ForeignKey;

            Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
            Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
            Assert.Equal(2, productCategoryType.GetForeignKeys().Count());

            var key = productCategoryType.FindPrimaryKey()!;
            Assert.Equal(
                new[] { nameof(ProductCategory.ProductId), nameof(ProductCategory.CategoryId) },
                key.Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void Can_configure_join_type_with_implicit_join_relationships()
        {
            var modelBuilder = CreateModelBuilder();

            var manyToMany = modelBuilder.Entity<Product>()
                .HasMany(p => p.Categories).WithMany(c => c.Products)
                .UsingEntity<ProductCategory>(
                    pcb => pcb.HasKey(pc => new { pc.ProductId, pc.CategoryId }));

            Assert.Equal(typeof(Product), manyToMany.Metadata.ClrType);

            var model = modelBuilder.FinalizeModel();

            var productType = model.FindEntityType(typeof(Product))!;
            var categoryType = model.FindEntityType(typeof(Category))!;
            var productCategoryType = model.FindEntityType(typeof(ProductCategory))!;

            var categoriesNavigation = productType.GetSkipNavigations().Single();
            var productsNavigation = categoryType.GetSkipNavigations().Single();

            var categoriesFk = categoriesNavigation.ForeignKey;
            var productsFk = productsNavigation.ForeignKey;

            Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
            Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
            Assert.Equal(2, productCategoryType.GetForeignKeys().Count());

            var key = productCategoryType.FindPrimaryKey()!;
            Assert.Equal(
                new[] { nameof(ProductCategory.ProductId), nameof(ProductCategory.CategoryId) },
                key.Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void Can_configure_shared_join_type_with_implicit_join_relationships()
        {
            var modelBuilder = CreateModelBuilder();

            var manyToMany = modelBuilder.Entity<Product>()
                .HasMany(p => p.Categories).WithMany(c => c.Products)
                .UsingEntity<ProductCategory>(
                    "SharedProductCategory",
                    pcb =>
                    {
                        pcb.Ignore(pc => pc.Category);
                        pcb.Ignore(pc => pc.Product);
                        pcb.HasKey(pc => new { pc.ProductId, pc.CategoryId });
                    });

            Assert.Equal(typeof(Product), manyToMany.Metadata.ClrType);

            modelBuilder.Entity<Category>().Ignore(c => c.ProductCategories);

            var model = modelBuilder.FinalizeModel();

            var productType = model.FindEntityType(typeof(Product))!;
            var categoryType = model.FindEntityType(typeof(Category))!;
            Assert.Null(model.FindEntityType(typeof(ProductCategory)));
            var productCategoryType = model.FindEntityType("SharedProductCategory")!;

            var categoriesNavigation = productType.GetSkipNavigations().Single();
            var productsNavigation = categoryType.GetSkipNavigations().Single();

            var categoriesFk = categoriesNavigation.ForeignKey;
            var productsFk = productsNavigation.ForeignKey;

            Assert.Same(categoriesFk, productCategoryType.GetForeignKeys().Last());
            Assert.Same(productsFk, productCategoryType.GetForeignKeys().First());
            Assert.Equal(2, productCategoryType.GetForeignKeys().Count());

            var key = productCategoryType.FindPrimaryKey()!;
            Assert.Equal(
                new[] { nameof(ProductCategory.ProductId), nameof(ProductCategory.CategoryId) },
                key.Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void Can_ignore_existing_navigations()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Category>()
                .HasMany(p => p.Products).WithMany(c => c.Categories);

            modelBuilder.Entity<Category>().Ignore(c => c.Products);
            modelBuilder.Entity<Product>().Ignore(p => p.Categories);

            // Issue #19550
            modelBuilder.Ignore<ProductCategory>();

            var model = modelBuilder.FinalizeModel();

            var productType = model.FindEntityType(typeof(Product))!;
            var categoryType = model.FindEntityType(typeof(Category))!;

            Assert.Empty(productType.GetSkipNavigations());
            Assert.Empty(categoryType.GetSkipNavigations());
        }

        [ConditionalFact]
        public virtual void Throws_for_conflicting_many_to_one_on_left()
        {
            var modelBuilder = CreateModelBuilder();

            // make sure we do not set up the automatic many-to-many relationship
            modelBuilder.Entity<Category>().Ignore(e => e.Products);

            modelBuilder.Entity<Category>()
                .HasMany(o => o.Products).WithOne();

            Assert.Equal(
                CoreStrings.ConflictingRelationshipNavigation(
                    nameof(Category) + "." + nameof(Category.Products),
                    nameof(Product) + "." + nameof(Product.Categories),
                    nameof(Category) + "." + nameof(Category.Products),
                    nameof(Product)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<Category>()
                        .HasMany(o => o.Products).WithMany(c => c.Categories)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_conflicting_many_to_one_on_right()
        {
            var modelBuilder = CreateModelBuilder();

            // make sure we do not set up the automatic many-to-many relationship
            modelBuilder.Entity<Category>().Ignore(e => e.Products);

            modelBuilder.Entity<Category>()
                .HasMany(o => o.Products).WithOne();

            Assert.Equal(
                CoreStrings.ConflictingRelationshipNavigation(
                    nameof(Product) + "." + nameof(Product.Categories),
                    nameof(Category) + "." + nameof(Category.Products),
                    nameof(Category) + "." + nameof(Category.Products),
                    nameof(Product)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<Product>()
                        .HasMany(o => o.Categories).WithMany(c => c.Products)).Message);
        }

        [ConditionalFact]
        public virtual void Many_to_many_with_only_Has_navigation_configured()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<UniCategory>().HasMany(e => e.Products).WithMany();

            var model = modelBuilder.FinalizeModel();

            var manyToManyA = model.FindEntityType(typeof(UniCategory))!;
            var manyToManyB = model.FindEntityType(typeof(UniProduct))!;
            var joinEntityType = model.GetEntityTypes()
                .Where(et => et.ClrType == Model.DefaultPropertyBagType)
                .Single();
            Assert.Equal("UniCategoryUniProduct", joinEntityType.Name);

            var navigationOnManyToManyA = manyToManyA.GetSkipNavigations().Single();
            var navigationOnManyToManyB = manyToManyB.GetSkipNavigations().Single();
            Assert.Equal("Products", navigationOnManyToManyA.Name);
            Assert.Equal("UniCategory", navigationOnManyToManyB.Name);
            Assert.Same(navigationOnManyToManyA.Inverse, navigationOnManyToManyB);
            Assert.Same(navigationOnManyToManyB.Inverse, navigationOnManyToManyA);
            Assert.False(navigationOnManyToManyA.IsShadowProperty());
            Assert.True(navigationOnManyToManyB.IsShadowProperty());

            var manyToManyAForeignKey = navigationOnManyToManyA.ForeignKey;
            var manyToManyBForeignKey = navigationOnManyToManyB.ForeignKey;
            Assert.NotNull(manyToManyAForeignKey);
            Assert.NotNull(manyToManyBForeignKey);
            Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            Assert.Equal(manyToManyAForeignKey.DeclaringEntityType, joinEntityType);
            Assert.Equal(manyToManyBForeignKey.DeclaringEntityType, joinEntityType);

            var key = joinEntityType.FindPrimaryKey()!;
            Assert.Equal(
                new[] { "ProductsId", "UniCategoryId" },
                key.Properties.Select(p => p.Name));

            Assert.DoesNotContain(joinEntityType.GetProperties(), p => !p.IsIndexerProperty());
        }

        [ConditionalFact]
        public virtual void Many_to_many_with_no_navigations_configured()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<NoCategory>().HasMany<NoProduct>().WithMany();

            var model = modelBuilder.FinalizeModel();

            var manyToManyA = model.FindEntityType(typeof(NoCategory))!;
            var manyToManyB = model.FindEntityType(typeof(NoProduct))!;
            var joinEntityType = model.GetEntityTypes()
                .Where(et => et.ClrType == Model.DefaultPropertyBagType)
                .Single();
            Assert.Equal("NoCategoryNoProduct", joinEntityType.Name);

            var navigationOnManyToManyA = manyToManyA.GetSkipNavigations().Single();
            var navigationOnManyToManyB = manyToManyB.GetSkipNavigations().Single();
            Assert.Equal("NoProduct", navigationOnManyToManyA.Name);
            Assert.Equal("NoCategory", navigationOnManyToManyB.Name);
            Assert.Same(navigationOnManyToManyA.Inverse, navigationOnManyToManyB);
            Assert.Same(navigationOnManyToManyB.Inverse, navigationOnManyToManyA);
            Assert.True(navigationOnManyToManyA.IsShadowProperty());
            Assert.True(navigationOnManyToManyB.IsShadowProperty());

            var manyToManyAForeignKey = navigationOnManyToManyA.ForeignKey;
            var manyToManyBForeignKey = navigationOnManyToManyB.ForeignKey;
            Assert.NotNull(manyToManyAForeignKey);
            Assert.NotNull(manyToManyBForeignKey);
            Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            Assert.Equal(manyToManyAForeignKey.DeclaringEntityType, joinEntityType);
            Assert.Equal(manyToManyBForeignKey.DeclaringEntityType, joinEntityType);

            var key = joinEntityType.FindPrimaryKey()!;
            Assert.Equal(
                new[] { "NoCategoryId", "NoProductId" },
                key.Properties.Select(p => p.Name));

            Assert.DoesNotContain(joinEntityType.GetProperties(), p => !p.IsIndexerProperty());
        }

        [ConditionalFact]
        public virtual void Many_to_many_with_a_shadow_navigation()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToOneNavPrincipal>();
            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Entity<NavDependent>().Ignore(d => d.ManyToManyPrincipals);

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(d => d.Dependents)
                .WithMany("Shadow");

            var model = modelBuilder.FinalizeModel();

            var manyToManyA = model.FindEntityType(typeof(ManyToManyNavPrincipal))!;
            var manyToManyB = model.FindEntityType(typeof(NavDependent))!;
            var joinEntityType = model.GetEntityTypes()
                .Where(et => et.ClrType == Model.DefaultPropertyBagType)
                .Single();
            Assert.Equal("ManyToManyNavPrincipalNavDependent", joinEntityType.Name);

            var navigationOnManyToManyA = manyToManyA.GetSkipNavigations().Single();
            var navigationOnManyToManyB = manyToManyB.GetSkipNavigations().Single();
            Assert.Equal("Dependents", navigationOnManyToManyA.Name);
            Assert.Equal("Shadow", navigationOnManyToManyB.Name);
            Assert.Same(navigationOnManyToManyA.Inverse, navigationOnManyToManyB);
            Assert.Same(navigationOnManyToManyB.Inverse, navigationOnManyToManyA);
            Assert.False(navigationOnManyToManyA.IsShadowProperty());
            Assert.True(navigationOnManyToManyB.IsShadowProperty());

            var manyToManyAForeignKey = navigationOnManyToManyA.ForeignKey;
            var manyToManyBForeignKey = navigationOnManyToManyB.ForeignKey;
            Assert.NotNull(manyToManyAForeignKey);
            Assert.NotNull(manyToManyBForeignKey);
            Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            Assert.Equal(manyToManyAForeignKey.DeclaringEntityType, joinEntityType);
            Assert.Equal(manyToManyBForeignKey.DeclaringEntityType, joinEntityType);

            var key = joinEntityType.FindPrimaryKey()!;
            Assert.Equal(
                new[] { "DependentsId", "ShadowId" },
                key.Properties.Select(p => p.Name));

            Assert.DoesNotContain(joinEntityType.GetProperties(), p => !p.IsIndexerProperty());
        }

        [ConditionalFact]
        public virtual void Many_to_many_with_only_With_navigation_configured()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<UniProduct>().HasMany<UniCategory>().WithMany(c => c.Products);

            var model = modelBuilder.FinalizeModel();

            var manyToManyA = model.FindEntityType(typeof(UniProduct))!;
            var manyToManyB = model.FindEntityType(typeof(UniCategory))!;
            var joinEntityType = model.GetEntityTypes()
                .Where(et => et.ClrType == Model.DefaultPropertyBagType)
                .Single();
            Assert.Equal("UniCategoryUniProduct", joinEntityType.Name);

            var navigationOnManyToManyA = manyToManyA.GetSkipNavigations().Single();
            var navigationOnManyToManyB = manyToManyB.GetSkipNavigations().Single();
            Assert.Equal("UniCategory", navigationOnManyToManyA.Name);
            Assert.Equal("Products", navigationOnManyToManyB.Name);
            Assert.Same(navigationOnManyToManyA.Inverse, navigationOnManyToManyB);
            Assert.Same(navigationOnManyToManyB.Inverse, navigationOnManyToManyA);
            Assert.True(navigationOnManyToManyA.IsShadowProperty());
            Assert.False(navigationOnManyToManyB.IsShadowProperty());

            var manyToManyAForeignKey = navigationOnManyToManyA.ForeignKey;
            var manyToManyBForeignKey = navigationOnManyToManyB.ForeignKey;
            Assert.NotNull(manyToManyAForeignKey);
            Assert.NotNull(manyToManyBForeignKey);
            Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            Assert.Equal(manyToManyAForeignKey.DeclaringEntityType, joinEntityType);
            Assert.Equal(manyToManyBForeignKey.DeclaringEntityType, joinEntityType);

            var key = joinEntityType.FindPrimaryKey()!;
            Assert.Equal(
                new[] { "ProductsId", "UniCategoryId" },
                key.Properties.Select(p => p.Name));

            Assert.DoesNotContain(joinEntityType.GetProperties(), p => !p.IsIndexerProperty());
        }

        [ConditionalFact]
        public virtual void Throws_for_self_ref_with_same_navigation()
        {
            var modelBuilder = CreateModelBuilder();

            Assert.Equal(
                CoreStrings.ManyToManyOneNav(nameof(SelfRefManyToOne), nameof(SelfRefManyToOne.SelfRef2)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder
                        .Entity<SelfRefManyToOne>()
                        .HasMany(e => e.SelfRef2)
                        .WithMany(e => e.SelfRef2)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_self_ref_using_self()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<SelfRefManyToOne>().Ignore(s => s.Id);
            modelBuilder.Entity<SelfRefManyToOne>().HasMany(t => t.Relateds)
                .WithMany(t => t.RelatedSelfRefs)
                .UsingEntity<SelfRefManyToOne>(
                    t => t.HasOne(a => a.Related).WithMany(b => b.DirectlyRelatedSelfRefs),
                    t => t.HasOne(a => a.SelfRef1).WithMany(b => b.SelfRef2));

            Assert.Equal(
                CoreStrings.EntityRequiresKey(nameof(SelfRefManyToOne)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void ForeignKeyAttribute_configures_the_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CategoryWithAttribute>();

            var model = modelBuilder.FinalizeModel();

            var category = model.FindEntityType(typeof(CategoryWithAttribute))!;
            var productsNavigation = category.GetSkipNavigations().Single();
            var categoryFk = productsNavigation.ForeignKey;
            Assert.Equal("CategoriesID", categoryFk.Properties.Single().Name);

            var categoryNavigation = productsNavigation.TargetEntityType.GetSkipNavigations().Single();
            var productFk = categoryNavigation.ForeignKey;
            Assert.Equal("ProductKey", productFk.Properties.Single().Name);

            var joinEntityType = categoryFk.DeclaringEntityType;
            Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
        }

        [ConditionalFact]
        public virtual void Overrides_ForeignKeyAttribute()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CategoryWithAttribute>()
                .HasMany(e => e.Products)
                .WithMany(e => e.Categories)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductCategory",
                    e => e.HasOne<ProductWithAttribute>().WithMany().HasForeignKey("ProductWithAttributeId"),
                    e => e.HasOne<CategoryWithAttribute>().WithMany().HasForeignKey("CategoryWithAttributeId"));

            var model = modelBuilder.FinalizeModel();

            var category = model.FindEntityType(typeof(CategoryWithAttribute))!;
            var productsNavigation = category.GetSkipNavigations().Single();
            var categoryFk = productsNavigation.ForeignKey;
            Assert.Equal("CategoryWithAttributeId", categoryFk.Properties.Single().Name);

            var categoryNavigation = productsNavigation.TargetEntityType.GetSkipNavigations().Single();
            var productFk = categoryNavigation.ForeignKey;
            Assert.Equal("ProductWithAttributeId", productFk.Properties.Single().Name);

            var joinEntityType = categoryFk.DeclaringEntityType;
            Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
            Assert.Equal(
                new[] { "CategoryWithAttributeId", "ProductWithAttributeId" },
                joinEntityType.FindPrimaryKey()!.Properties.Select(p => p.Name));
        }

        protected class ProductWithAttribute
        {
            public int ID { get; set; }

            [ForeignKey("ProductKey")]
            public virtual ICollection<CategoryWithAttribute>? Categories { get; set; }
        }

        protected class CategoryWithAttribute
        {
            public int ID { get; set; }
            public virtual ICollection<ProductWithAttribute>? Products { get; set; }
        }

        [ConditionalFact] // Issue #27990
        public virtual void ForeignKeyAttribute_does_not_force_convention_join_table_inclusion_matching_key_names()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<MotorArtMatching>(
                entity =>
                {
                    entity.HasMany(d => d.MotorBauArtMatching)
                        .WithMany(p => p.MotorArtMatching)
                        .UsingEntity<Dictionary<string, object>>("MotorArtXMotorBauartMatching");
                });

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(3, model.GetEntityTypes().Count());

            Assert.Collection(model.GetEntityTypes(),
                e =>
                {
                    Assert.Equal("MotorArtMatching", e.ShortName());
                    AssertEqual(new[] { e.FindProperty("MotorArtMatchingId")! }, e.GetProperties());
                    Assert.Empty(e.GetForeignKeys());
                    Assert.Empty(e.GetNavigations());
                    Assert.Collection(e.GetSkipNavigations(), n => Assert.Equal("MotorBauArtMatching", n.Name));
                },
                e =>
                {
                    Assert.Equal("MotorBauartMatching", e.ShortName());
                    AssertEqual(new[] { e.FindProperty("MotorBauartMatchingId")! }, e.GetProperties());
                    Assert.Empty(e.GetForeignKeys());
                    Assert.Empty(e.GetNavigations());
                    Assert.Collection(e.GetSkipNavigations(), n => Assert.Equal("MotorArtMatching", n.Name));
                },
                e =>
                {
                    Assert.Equal("MotorArtXMotorBauartMatching", e.ShortName());
                    Assert.Collection(e.GetForeignKeys(),
                        k =>
                        {
                            Assert.Equal("MotorArtMatching", k.PrincipalEntityType.ShortName());
                            Assert.Collection(k.Properties, p => Assert.Equal("MotorArtMatchingId", p.Name));
                            Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("MotorArtMatchingId", p.Name));
                        },
                        k =>
                        {
                            Assert.Equal("MotorBauartMatching", k.PrincipalEntityType.ShortName());
                            Assert.Collection(k.Properties, p => Assert.Equal("MotorBauartMatchingId", p.Name));
                            Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("MotorBauartMatchingId", p.Name));
                        });
                    AssertEqual(e.GetForeignKeys().SelectMany(fk => fk.Properties), e.GetProperties());
                    Assert.Empty(e.GetNavigations());
                    Assert.Empty(e.GetSkipNavigations());
                });
        }

        protected class MotorArtMatching
        {
            public int MotorArtMatchingId { get; set; }

            [ForeignKey("MotorArtMatchingId")]
            public virtual ICollection<MotorBauartMatching> MotorBauArtMatching { get; set; } = null!;
        }

        protected class MotorBauartMatching
        {
            public int MotorBauartMatchingId { get; set; }

            [ForeignKey("MotorBauartMatchingId")]
            public virtual ICollection<MotorArtMatching> MotorArtMatching { get; set; } = null!;
        }

        [ConditionalFact] // Issue #27990
        public virtual void ForeignKeyAttribute_does_not_force_convention_join_table_inclusion_mismatching_key_names()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<MotorArtMismatching>(
                entity =>
                {
                    entity.HasMany(d => d.MotorBauArtMismatching)
                        .WithMany(p => p.MotorArtMismatching)
                        .UsingEntity<Dictionary<string, object>>("MotorArtXMotorBauartMismatching");
                });

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(3, model.GetEntityTypes().Count());

            Assert.Collection(model.GetEntityTypes(),
                e =>
                {
                    Assert.Equal("MotorArtMismatching", e.ShortName());
                    AssertEqual(new[] { e.FindProperty("MotorArtMismatchingId")! }, e.GetProperties());
                    Assert.Empty(e.GetForeignKeys());
                    Assert.Empty(e.GetNavigations());
                    Assert.Collection(e.GetSkipNavigations(), n => Assert.Equal("MotorBauArtMismatching", n.Name));
                },
                e =>
                {
                    Assert.Equal("MotorBauartMismatching", e.ShortName());
                    AssertEqual(new[] { e.FindProperty("MotorBauartMismatchingId")! }, e.GetProperties());
                    Assert.Empty(e.GetForeignKeys());
                    Assert.Empty(e.GetNavigations());
                    Assert.Collection(e.GetSkipNavigations(), n => Assert.Equal("MotorArtMismatching", n.Name));
                },
                e =>
                {
                    Assert.Equal("MotorArtXMotorBauartMismatching", e.ShortName());
                    Assert.Collection(e.GetForeignKeys(),
                        k =>
                        {
                            Assert.Equal("MotorArtMismatching", k.PrincipalEntityType.ShortName());
                            Assert.Collection(k.Properties, p => Assert.Equal("MotorArtMismatchingKey", p.Name));
                            Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("MotorArtMismatchingId", p.Name));
                        },
                        k =>
                        {
                            Assert.Equal("MotorBauartMismatching", k.PrincipalEntityType.ShortName());
                            Assert.Collection(k.Properties, p => Assert.Equal("MotorBauArtMismatchingKey", p.Name));
                            Assert.Collection(k.PrincipalKey.Properties, p => Assert.Equal("MotorBauartMismatchingId", p.Name));
                        });
                    AssertEqual(e.GetForeignKeys().SelectMany(fk => fk.Properties), e.GetProperties());
                    Assert.Empty(e.GetNavigations());
                    Assert.Empty(e.GetSkipNavigations());
                });
        }

        protected class MotorArtMismatching
        {
            public int MotorArtMismatchingId { get; set; }

            [ForeignKey("MotorArtMismatchingKey")]
            public virtual ICollection<MotorBauartMismatching> MotorBauArtMismatching { get; set; } = null!;
        }

        protected class MotorBauartMismatching
        {
            public int MotorBauartMismatchingId { get; set; }

            [ForeignKey("MotorBauArtMismatchingKey")]
            public virtual ICollection<MotorArtMismatching> MotorArtMismatching { get; set; } = null!;
        }

        [ConditionalFact]
        public virtual void Navigation_properties_can_set_access_mode()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals);

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .Navigation(e => e.Dependents)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<OneToManyNavPrincipal>();
            modelBuilder.Entity<NavDependent>()
                .Navigation(e => e.ManyToManyPrincipals)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            modelBuilder.Entity<NavDependent>()
                .Ignore(n => n.OneToOnePrincipal);

            var model = modelBuilder.FinalizeModel();

            var principal = model.FindEntityType(typeof(ManyToManyNavPrincipal))!;
            var dependent = model.FindEntityType(typeof(NavDependent))!;

            Assert.Equal(PropertyAccessMode.Field, principal.FindSkipNavigation("Dependents")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, dependent.FindSkipNavigation("ManyToManyPrincipals")!.GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Skip_navigation_field_can_be_set_via_attribute()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();
            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals);

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(
                "_randomField", model.FindEntityType(typeof(ManyToManyNavPrincipal))!.FindSkipNavigation("Dependents")!.GetFieldName());
        }

        [ConditionalFact]
        public virtual void IsRequired_throws()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(n => n.Dependents)
                .WithMany(n => n.ManyToManyPrincipals);

            Assert.Equal(
                CoreStrings.RequiredSkipNavigation(nameof(ManyToManyNavPrincipal), nameof(ManyToManyNavPrincipal.Dependents)),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Entity<ManyToManyNavPrincipal>()
                        .Navigation(p => p.Dependents)
                        .IsRequired()).Message);
        }

        [ConditionalFact]
        public virtual void Can_use_shared_type_as_join_entity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<Dictionary<string, object>>(
                    "Shared1",
                    e => e.HasOne<NavDependent>().WithMany(),
                    e => e.HasOne<ManyToManyNavPrincipal>().WithMany());

            modelBuilder.Entity<ManyToManyPrincipalWithField>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<Dictionary<string, object>>(
                    "Shared2",
                    e => e.HasOne<DependentWithField>().WithMany(),
                    e => e.HasOne<ManyToManyPrincipalWithField>().WithMany(),
                    e => e.IndexerProperty<int>("Payload"));

            modelBuilder.Entity<ManyToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToOnePrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<DependentWithField>().HasKey(d => d.DependentWithFieldId);

            var model = modelBuilder.Model;

            var shared1 = model.FindEntityType("Shared1")!;
            Assert.Equal(2, shared1.GetForeignKeys().Count());
            Assert.Equal(
                new[]
                {
                    nameof(ManyToManyNavPrincipal.Dependents) + nameof(NavDependent.Id),
                    nameof(NavDependent.ManyToManyPrincipals) + nameof(ManyToManyNavPrincipal.Id)
                },
                shared1.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.True(shared1.HasSharedClrType);
            Assert.Equal(typeof(Dictionary<string, object>), shared1.ClrType);

            var shared2 = model.FindEntityType("Shared2")!;
            Assert.Equal(2, shared2.GetForeignKeys().Count());
            Assert.Equal(
                new[]
                {
                    nameof(ManyToManyPrincipalWithField.Dependents) + nameof(DependentWithField.DependentWithFieldId),
                    nameof(DependentWithField.ManyToManyPrincipals) + nameof(ManyToManyPrincipalWithField.Id)
                },
                shared2.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.NotNull(shared2.FindProperty("Payload"));
            Assert.True(shared2.HasSharedClrType);
            Assert.Equal(typeof(Dictionary<string, object>), shared2.ClrType);

            Assert.Equal(
                CoreStrings.ClashingSharedType(typeof(Dictionary<string, object>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<Dictionary<string, object>>()).Message);

            modelBuilder.FinalizeModel();
        }

        [ConditionalFact]
        public virtual void Can_use_implicit_shared_type_as_join_entity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity(
                    "Shared1",
                    e => e.HasOne<NavDependent>().WithMany(),
                    e => e.HasOne<ManyToManyNavPrincipal>().WithMany());

            modelBuilder.Entity<ManyToManyPrincipalWithField>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity(
                    "Shared2",
                    e => e.HasOne<DependentWithField>().WithMany(),
                    e => e.HasOne<ManyToManyPrincipalWithField>().WithMany(),
                    e => e.IndexerProperty<int>("Payload"));

            modelBuilder.Entity<ManyToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToOnePrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<DependentWithField>().HasKey(d => d.DependentWithFieldId);

            var model = modelBuilder.Model;

            var shared1 = model.FindEntityType("Shared1")!;
            Assert.Equal(2, shared1.GetForeignKeys().Count());
            Assert.Equal(
                new[]
                {
                    nameof(ManyToManyNavPrincipal.Dependents) + nameof(NavDependent.Id),
                    nameof(NavDependent.ManyToManyPrincipals) + nameof(ManyToManyNavPrincipal.Id)
                },
                shared1.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.True(shared1.HasSharedClrType);
            Assert.Equal(typeof(Dictionary<string, object>), shared1.ClrType);

            var shared2 = model.FindEntityType("Shared2")!;
            Assert.Equal(2, shared2.GetForeignKeys().Count());
            Assert.Equal(
                new[]
                {
                    nameof(ManyToManyPrincipalWithField.Dependents) + nameof(DependentWithField.DependentWithFieldId),
                    nameof(DependentWithField.ManyToManyPrincipals) + nameof(ManyToManyPrincipalWithField.Id)
                },
                shared2.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.NotNull(shared2.FindProperty("Payload"));
            Assert.True(shared2.HasSharedClrType);
            Assert.Equal(typeof(Dictionary<string, object>), shared2.ClrType);

            Assert.Equal(
                CoreStrings.ClashingSharedType(typeof(Dictionary<string, object>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<Dictionary<string, object>>()).Message);

            modelBuilder.FinalizeModel();
        }

        [ConditionalFact]
        public virtual void Can_use_implicit_shared_type_with_default_name_as_join_entity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity(
                    e => e.HasOne<NavDependent>().WithMany(),
                    e => e.HasOne<ManyToManyNavPrincipal>().WithMany());

            modelBuilder.Entity<ManyToManyPrincipalWithField>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity(
                    e => e.HasOne<DependentWithField>().WithMany(),
                    e => e.HasOne<ManyToManyPrincipalWithField>().WithMany(),
                    e => e.IndexerProperty<int>("Payload"));

            modelBuilder.Entity<ManyToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToOnePrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<DependentWithField>().HasKey(d => d.DependentWithFieldId);

            var model = modelBuilder.Model;

            var shared1 = model.FindEntityType(typeof(ManyToManyNavPrincipal))!
                .FindSkipNavigation(nameof(ManyToManyNavPrincipal.Dependents))!.JoinEntityType!;
            Assert.Equal(2, shared1.GetForeignKeys().Count());
            Assert.Equal(
                new[]
                {
                    nameof(ManyToManyNavPrincipal.Dependents) + nameof(NavDependent.Id),
                    nameof(NavDependent.ManyToManyPrincipals) + nameof(ManyToManyNavPrincipal.Id)
                },
                shared1.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.True(shared1.HasSharedClrType);
            Assert.Equal(typeof(Dictionary<string, object>), shared1.ClrType);
            Assert.Equal("ManyToManyNavPrincipalNavDependent", shared1.Name);

            var shared2 = model.FindEntityType(typeof(ManyToManyPrincipalWithField))!
                .FindSkipNavigation(nameof(ManyToManyPrincipalWithField.Dependents))!.JoinEntityType!;
            Assert.Equal(2, shared2.GetForeignKeys().Count());
            Assert.Equal(
                new[]
                {
                    nameof(ManyToManyPrincipalWithField.Dependents) + nameof(DependentWithField.DependentWithFieldId),
                    nameof(DependentWithField.ManyToManyPrincipals) + nameof(ManyToManyPrincipalWithField.Id)
                },
                shared2.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.NotNull(shared2.FindProperty("Payload"));
            Assert.True(shared2.HasSharedClrType);
            Assert.Equal(typeof(Dictionary<string, object>), shared2.ClrType);
            Assert.Equal("DependentWithFieldManyToManyPrincipalWithField", shared2.Name);

            Assert.Equal(
                CoreStrings.ClashingSharedType(typeof(Dictionary<string, object>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<Dictionary<string, object>>()).Message);

            modelBuilder.FinalizeModel();
        }

        [ConditionalFact]
        public virtual void Can_use_implicit_shared_type_with_implicit_relationships_as_join_entity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity("Shared1");

            modelBuilder.Entity<ManyToManyPrincipalWithField>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity(
                    "Shared2",
                    e => e.IndexerProperty<int>("Payload"));

            modelBuilder.Entity<ManyToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToOnePrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<DependentWithField>().HasKey(d => d.DependentWithFieldId);

            var model = modelBuilder.Model;

            var shared1 = model.FindEntityType("Shared1")!;
            Assert.Equal(2, shared1.GetForeignKeys().Count());
            Assert.Equal(
                new[]
                {
                    nameof(ManyToManyNavPrincipal.Dependents) + nameof(NavDependent.Id),
                    nameof(NavDependent.ManyToManyPrincipals) + nameof(ManyToManyNavPrincipal.Id)
                },
                shared1.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.True(shared1.HasSharedClrType);
            Assert.Equal(typeof(Dictionary<string, object>), shared1.ClrType);

            var shared2 = model.FindEntityType("Shared2")!;
            Assert.Equal(2, shared2.GetForeignKeys().Count());
            Assert.Equal(
                new[]
                {
                    nameof(ManyToManyPrincipalWithField.Dependents) + nameof(DependentWithField.DependentWithFieldId),
                    nameof(DependentWithField.ManyToManyPrincipals) + nameof(ManyToManyPrincipalWithField.Id)
                },
                shared2.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.NotNull(shared2.FindProperty("Payload"));
            Assert.True(shared2.HasSharedClrType);
            Assert.Equal(typeof(Dictionary<string, object>), shared2.ClrType);

            Assert.Equal(
                CoreStrings.ClashingSharedType(typeof(Dictionary<string, object>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(modelBuilder.Entity<Dictionary<string, object>>).Message);

            modelBuilder.FinalizeModel();
        }

        [ConditionalFact]
        public virtual void Can_use_implicit_shared_type_with_default_name_and_implicit_relationships_as_join_entity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();
            modelBuilder.Ignore<OneToManyPrincipalWithField>();
            modelBuilder.Ignore<OneToOnePrincipalWithField>();

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<Dictionary<string, object>>("ManyToManyNavPrincipalNavDependentOld", j => j.HasAnnotation("Foo", "Bar"));

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<Dictionary<string, object>>("ManyToManyNavPrincipalNavDependent");

            modelBuilder.Entity<ManyToManyPrincipalWithField>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity(e => e.IndexerProperty<int>("Payload"));

            modelBuilder.Entity<ManyToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<DependentWithField>().HasKey(d => d.DependentWithFieldId);

            IReadOnlyModel model = modelBuilder.Model;

            var shared1 = model.FindEntityType(typeof(ManyToManyNavPrincipal))!
                .FindSkipNavigation(nameof(ManyToManyNavPrincipal.Dependents))!.JoinEntityType!;
            Assert.Equal(2, shared1.GetForeignKeys().Count());
            Assert.Equal(
                new[]
                {
                    nameof(ManyToManyNavPrincipal.Dependents) + nameof(NavDependent.Id),
                    nameof(NavDependent.ManyToManyPrincipals) + nameof(ManyToManyNavPrincipal.Id)
                },
                shared1.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.True(shared1.HasSharedClrType);
            Assert.Equal(typeof(Dictionary<string, object>), shared1.ClrType);
            Assert.Equal("ManyToManyNavPrincipalNavDependent", shared1.Name);
            Assert.Equal("Bar", shared1["Foo"]);

            var shared2 = model.FindEntityType(typeof(ManyToManyPrincipalWithField))!
                .FindSkipNavigation(nameof(ManyToManyPrincipalWithField.Dependents))!.JoinEntityType!;
            Assert.Equal(2, shared2.GetForeignKeys().Count());
            Assert.Equal(
                new[]
                {
                    nameof(ManyToManyPrincipalWithField.Dependents) + nameof(DependentWithField.DependentWithFieldId),
                    nameof(DependentWithField.ManyToManyPrincipals) + nameof(ManyToManyPrincipalWithField.Id)
                },
                shared2.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.NotNull(shared2.FindProperty("Payload"));
            Assert.True(shared2.HasSharedClrType);
            Assert.Equal(typeof(Dictionary<string, object>), shared2.ClrType);
            Assert.Equal("DependentWithFieldManyToManyPrincipalWithField", shared2.Name);

            Assert.Equal(
                CoreStrings.ClashingSharedType(typeof(Dictionary<string, object>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(modelBuilder.Entity<Dictionary<string, object>>).Message);

            model = modelBuilder.FinalizeModel();
            AssertEqual(modelBuilder.Model, model);

            Assert.Equal(6, model.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void FK_properties_matching_navigations_are_discovered_on_explicit_join_entity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<NavDependentManyToManyNavPrincipalWithNavigationIds>();

            var model = modelBuilder.FinalizeModel();

            var joinType = model.FindEntityType(typeof(NavDependentManyToManyNavPrincipalWithNavigationIds))!;
            Assert.Equal(2, joinType.GetForeignKeys().Count());
            Assert.Equal(
                joinType.GetForeignKeys().SelectMany(fk => fk.Properties),
                joinType.FindPrimaryKey()!.Properties);
        }

        [ConditionalFact]
        public virtual void FK_properties_matching_types_are_discovered_on_explicit_join_entity()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder
                .Entity<NavDependentManyToManyNavPrincipalWithTypeIds>()
                .HasKey(e => new { e.NavDependentId, e.ManyToManyNavPrincipalId });

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<NavDependentManyToManyNavPrincipalWithTypeIds>();

            var model = modelBuilder.FinalizeModel();

            var joinType = model.FindEntityType(typeof(NavDependentManyToManyNavPrincipalWithTypeIds))!;
            Assert.Equal(2, joinType.GetForeignKeys().Count());
            Assert.Equal(
                joinType.GetForeignKeys().SelectMany(fk => fk.Properties).Reverse(),
                joinType.FindPrimaryKey()!.Properties);
        }

        [ConditionalFact]
        public virtual void UsingEntity_with_shared_type_passed_when_marked_as_shared_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.SharedTypeEntity<ManyToManyJoinWithFields>("Shared");

            var joinEntityType = modelBuilder.Entity<ManyToManyPrincipalWithField>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<ManyToManyJoinWithFields>(
                    "Shared",
                    r => r.HasOne<DependentWithField>().WithMany(),
                    l => l.HasOne<ManyToManyPrincipalWithField>().WithMany()).Metadata;

            modelBuilder.Entity<ManyToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToOnePrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<DependentWithField>().HasKey(d => d.DependentWithFieldId);

            var model = modelBuilder.FinalizeModel();

            Assert.True(joinEntityType.HasSharedClrType);
            Assert.Equal("Shared", joinEntityType.Name);
            Assert.Equal(typeof(ManyToManyJoinWithFields), joinEntityType.ClrType);
        }

        [ConditionalFact]
        public virtual void UsingEntity_with_shared_type_passes_when_configured_as_shared()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.SharedTypeEntity<ManyToManyJoinWithFields>("Shared");

            var joinEntityType = modelBuilder.Entity<ManyToManyPrincipalWithField>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<ManyToManyJoinWithFields>(
                    "Shared",
                    r => r.HasOne<DependentWithField>().WithMany(),
                    l => l.HasOne<ManyToManyPrincipalWithField>().WithMany()).Metadata;

            modelBuilder.Entity<ManyToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToManyPrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<OneToOnePrincipalWithField>().HasKey(d => d.Id);
            modelBuilder.Entity<DependentWithField>().HasKey(d => d.DependentWithFieldId);

            var model = modelBuilder.FinalizeModel();

            Assert.True(joinEntityType.HasSharedClrType);
            Assert.Equal("Shared", joinEntityType.Name);
            Assert.Equal(typeof(ManyToManyJoinWithFields), joinEntityType.ClrType);
        }

        [ConditionalFact]
        public virtual void Unconfigured_many_to_many_navigations_throw()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<AmbiguousManyToManyImplicitLeft>();

            Assert.Equal(
                CoreStrings.NavigationNotAdded(
                    typeof(AmbiguousManyToManyImplicitLeft).DisplayName(fullName: false), "Navigation1",
                    typeof(List<AmbiguousManyToManyImplicitRight>).DisplayName(fullName: false)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }
    }
}

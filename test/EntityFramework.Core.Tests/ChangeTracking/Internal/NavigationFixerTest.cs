// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class NavigationFixerTest
    {
        [Fact]
        public void Does_fixup_of_related_principals()
        {
            var contextServices = CreateContextServices();
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal1 = new Category { Id = 11 };
            var principal2 = new Category { Id = 12 };
            var dependent = new Product { Id = 21, CategoryId = 12 };

            manager.StartTracking(manager.GetOrCreateEntry(principal1));
            manager.StartTracking(manager.GetOrCreateEntry(principal2));

            var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

            dependentEntry.SetEntityState(EntityState.Added);

            Assert.Same(dependent.Category, principal2);
            Assert.Contains(dependent, principal2.Products);
            Assert.DoesNotContain(dependent, principal1.Products);
        }

        [Fact]
        public void Does_fixup_of_related_dependents()
        {
            var contextServices = CreateContextServices();
            var manager = contextServices.GetRequiredService<IStateManager>();

            var dependent1 = new Product { Id = 21, CategoryId = 11 };
            var dependent2 = new Product { Id = 22, CategoryId = 12 };
            var dependent3 = new Product { Id = 23, CategoryId = 11 };

            var principal = new Category { Id = 11 };

            manager.StartTracking(manager.GetOrCreateEntry(dependent1));
            manager.StartTracking(manager.GetOrCreateEntry(dependent2));
            manager.StartTracking(manager.GetOrCreateEntry(dependent3));

            var principalEntry = manager.StartTracking(manager.GetOrCreateEntry(principal));

            var fixer = CreateNavigationFixer(contextServices);
            fixer.StateChanged(principalEntry, EntityState.Detached);

            Assert.Same(dependent1.Category, principal);
            Assert.Null(dependent2.Category);
            Assert.Same(dependent3.Category, principal);

            Assert.Contains(dependent1, principal.Products);
            Assert.DoesNotContain(dependent2, principal.Products);
            Assert.Contains(dependent3, principal.Products);
        }

        [Fact]
        public void Does_fixup_of_one_to_one_relationship()
        {
            var contextServices = CreateContextServices();
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal1 = new Product { Id = 21 };
            var principal2 = new Product { Id = 22 };
            var principal3 = new Product { Id = 23 };

            var dependent1 = new ProductDetail { Id = 21 };
            var dependent2 = new ProductDetail { Id = 22 };
            var dependent4 = new ProductDetail { Id = 24 };

            var principalEntry1 = manager.StartTracking(manager.GetOrCreateEntry(principal1));
            var principalEntry2 = manager.StartTracking(manager.GetOrCreateEntry(principal2));
            var principalEntry3 = manager.StartTracking(manager.GetOrCreateEntry(principal3));

            var dependentEntry1 = manager.StartTracking(manager.GetOrCreateEntry(dependent1));
            var dependentEntry2 = manager.StartTracking(manager.GetOrCreateEntry(dependent2));
            var dependentEntry4 = manager.StartTracking(manager.GetOrCreateEntry(dependent4));

            Assert.Null(principal1.Detail);
            Assert.Null(dependent1.Product);

            principalEntry1.SetEntityState(EntityState.Added);

            Assert.Same(principal1, dependent1.Product);
            Assert.Same(dependent1, principal1.Detail);

            Assert.Null(principal2.Detail);
            Assert.Null(dependent2.Product);

            dependentEntry2.SetEntityState(EntityState.Added);

            Assert.Same(principal2, dependent2.Product);
            Assert.Same(dependent2, principal2.Detail);

            Assert.Null(principal3.Detail);
            Assert.Null(dependent4.Product);

            principalEntry3.SetEntityState(EntityState.Added);
            dependentEntry4.SetEntityState(EntityState.Added);

            Assert.Null(principal3.Detail);
            Assert.Null(dependent4.Product);
        }

        [Fact]
        public void Does_fixup_of_one_to_one_self_referencing_relationship()
        {
            var contextServices = CreateContextServices();
            var manager = contextServices.GetRequiredService<IStateManager>();

            var entity1 = new Product { Id = 21, AlternateProductId = 22 };
            var entity2 = new Product { Id = 22, AlternateProductId = 23 };
            var entity3 = new Product { Id = 23 };

            var entry1 = manager.StartTracking(manager.GetOrCreateEntry(entity1));
            var entry2 = manager.StartTracking(manager.GetOrCreateEntry(entity2));
            var entry3 = manager.StartTracking(manager.GetOrCreateEntry(entity3));

            Assert.Null(entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Null(entity2.AlternateProduct);
            Assert.Null(entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Null(entity3.OriginalProduct);

            entry1.SetEntityState(EntityState.Added);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Null(entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Null(entity3.OriginalProduct);

            entry3.SetEntityState(EntityState.Added);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Same(entity3, entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Same(entity2, entity3.OriginalProduct);
        }

        [Fact]
        public void Does_fixup_of_FKs_and_related_principals_using_dependent_navigations()
        {
            var contextServices = CreateContextServices();
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal1 = new Category { Id = 11 };
            var principal2 = new Category { Id = 12 };
            var dependent = new Product { Id = 21, Category = principal2 };

            manager.StartTracking(manager.GetOrCreateEntry(principal1));
            manager.StartTracking(manager.GetOrCreateEntry(principal2));

            var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

            dependentEntry.SetEntityState(EntityState.Added);

            Assert.Equal(12, dependent.CategoryId);
            Assert.Same(dependent.Category, principal2);
            Assert.Contains(dependent, principal2.Products);
            Assert.DoesNotContain(dependent, principal1.Products);
        }

        [Fact]
        public void Does_fixup_of_FKs_and_related_principals_using_principal_navigations()
        {
            var contextServices = CreateContextServices();
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal1 = new Category { Id = 11 };
            var principal2 = new Category { Id = 12 };
            var dependent = new Product { Id = 21 };

            principal2.Products.Add(dependent);

            manager.StartTracking(manager.GetOrCreateEntry(principal1));
            manager.StartTracking(manager.GetOrCreateEntry(principal2));

            var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

            dependentEntry.SetEntityState(EntityState.Added);

            Assert.Equal(12, dependent.CategoryId);
            Assert.Same(dependent.Category, principal2);
            Assert.Contains(dependent, principal2.Products);
            Assert.DoesNotContain(dependent, principal1.Products);
        }

        [Fact]
        public void Does_fixup_of_FKs_and_related_dependents_using_dependent_navigations()
        {
            var contextServices = CreateContextServices();
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal = new Category { Id = 11 };

            var dependent1 = new Product { Id = 21, Category = principal };
            var dependent2 = new Product { Id = 22 };
            var dependent3 = new Product { Id = 23, Category = principal };

            manager.StartTracking(manager.GetOrCreateEntry(dependent1));
            manager.StartTracking(manager.GetOrCreateEntry(dependent2));
            manager.StartTracking(manager.GetOrCreateEntry(dependent3));

            var principalEntry = manager.StartTracking(manager.GetOrCreateEntry(principal));

            principalEntry.SetEntityState(EntityState.Added);

            Assert.Equal(11, dependent1.CategoryId);
            Assert.Equal(0, dependent2.CategoryId);
            Assert.Equal(11, dependent3.CategoryId);

            Assert.Same(dependent1.Category, principal);
            Assert.Null(dependent2.Category);
            Assert.Same(dependent3.Category, principal);

            Assert.Contains(dependent1, principal.Products);
            Assert.DoesNotContain(dependent2, principal.Products);
            Assert.Contains(dependent3, principal.Products);
        }

        [Fact]
        public void Does_fixup_of_FKs_and_related_dependents_using_principal_navigations()
        {
            var contextServices = CreateContextServices();
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal = new Category { Id = 11 };

            var dependent1 = new Product { Id = 21 };
            var dependent2 = new Product { Id = 22 };
            var dependent3 = new Product { Id = 23 };

            principal.Products.Add(dependent1);
            principal.Products.Add(dependent3);

            manager.StartTracking(manager.GetOrCreateEntry(dependent1));
            manager.StartTracking(manager.GetOrCreateEntry(dependent2));
            manager.StartTracking(manager.GetOrCreateEntry(dependent3));

            var principalEntry = manager.StartTracking(manager.GetOrCreateEntry(principal));

            principalEntry.SetEntityState(EntityState.Added);

            Assert.Equal(11, dependent1.CategoryId);
            Assert.Equal(0, dependent2.CategoryId);
            Assert.Equal(11, dependent3.CategoryId);

            Assert.Same(dependent1.Category, principal);
            Assert.Null(dependent2.Category);
            Assert.Same(dependent3.Category, principal);

            Assert.Contains(dependent1, principal.Products);
            Assert.DoesNotContain(dependent2, principal.Products);
            Assert.Contains(dependent3, principal.Products);
        }

        [Fact]
        public void Does_fixup_of_one_to_one_self_referencing_relationship_using_dependent_navigations()
        {
            var contextServices = CreateContextServices();
            var manager = contextServices.GetRequiredService<IStateManager>();

            var entity1 = new Product { Id = 21 };
            var entity2 = new Product { Id = 22 };
            var entity3 = new Product { Id = 23 };

            entity1.AlternateProduct = entity2;
            entity2.AlternateProduct = entity3;

            var entry1 = manager.StartTracking(manager.GetOrCreateEntry(entity1));
            var entry2 = manager.StartTracking(manager.GetOrCreateEntry(entity2));
            var entry3 = manager.StartTracking(manager.GetOrCreateEntry(entity3));

            Assert.Null(entity1.AlternateProductId);
            Assert.Null(entity2.AlternateProductId);
            Assert.Null(entity3.AlternateProductId);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Same(entity3, entity2.AlternateProduct);
            Assert.Null(entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Null(entity3.OriginalProduct);

            entry1.SetEntityState(EntityState.Added);

            Assert.Equal(22, entity1.AlternateProductId);
            Assert.Null(entity2.AlternateProductId);
            Assert.Null(entity3.AlternateProductId);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Same(entity3, entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Null(entity3.OriginalProduct);

            entry3.SetEntityState(EntityState.Added);

            Assert.Equal(22, entity1.AlternateProductId);
            Assert.Equal(23, entity2.AlternateProductId);
            Assert.Null(entity3.AlternateProductId);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Same(entity3, entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Same(entity2, entity3.OriginalProduct);
        }

        [Fact]
        public void Does_fixup_of_one_to_one_self_referencing_relationship_using_principal_navigations()
        {
            var contextServices = CreateContextServices();
            var manager = contextServices.GetRequiredService<IStateManager>();

            var entity1 = new Product { Id = 21 };
            var entity2 = new Product { Id = 22 };
            var entity3 = new Product { Id = 23 };

            entity2.OriginalProduct = entity1;
            entity3.OriginalProduct = entity2;

            var entry1 = manager.StartTracking(manager.GetOrCreateEntry(entity1));
            var entry2 = manager.StartTracking(manager.GetOrCreateEntry(entity2));
            var entry3 = manager.StartTracking(manager.GetOrCreateEntry(entity3));

            Assert.Null(entity1.AlternateProductId);
            Assert.Null(entity2.AlternateProductId);
            Assert.Null(entity3.AlternateProductId);

            Assert.Null(entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Null(entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Same(entity2, entity3.OriginalProduct);

            entry1.SetEntityState(EntityState.Added);

            Assert.Equal(22, entity1.AlternateProductId);
            Assert.Null(entity2.AlternateProductId);
            Assert.Null(entity3.AlternateProductId);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Null(entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Same(entity2, entity3.OriginalProduct);

            entry3.SetEntityState(EntityState.Added);

            Assert.Equal(22, entity1.AlternateProductId);
            Assert.Equal(23, entity2.AlternateProductId);
            Assert.Null(entity3.AlternateProductId);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Same(entity3, entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Same(entity2, entity3.OriginalProduct);
        }

        [Fact]
        public void Does_fixup_of_related_principals_when_FK_is_set()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal1 = new Category { Id = 11 };
            var principal2 = new Category { Id = 12 };
            var dependent = new Product { Id = 21, CategoryId = 0 };

            manager.StartTracking(manager.GetOrCreateEntry(principal1));
            manager.StartTracking(manager.GetOrCreateEntry(principal2));

            var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

            var fixer = CreateNavigationFixer(contextServices);
            dependentEntry.SetEntityState(EntityState.Added);

            Assert.Null(dependent.Category);
            Assert.DoesNotContain(dependent, principal2.Products);
            Assert.DoesNotContain(dependent, principal1.Products);

            dependent.CategoryId = 11;

            fixer.ForeignKeyPropertyChanged(dependentEntry, model.GetEntityType(typeof(Product)).GetProperty("CategoryId"), 12, 11);

            Assert.Same(dependent.Category, principal1);
            Assert.Contains(dependent, principal1.Products);
            Assert.DoesNotContain(dependent, principal2.Products);
        }

        [Fact]
        public void Does_fixup_of_related_principals_when_FK_is_cleared()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal1 = new Category { Id = 11 };
            var principal2 = new Category { Id = 12 };
            var dependent = new Product { Id = 21, CategoryId = 12 };

            manager.StartTracking(manager.GetOrCreateEntry(principal1));
            manager.StartTracking(manager.GetOrCreateEntry(principal2));

            var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

            var fixer = CreateNavigationFixer(contextServices);
            dependentEntry.SetEntityState(EntityState.Added);

            Assert.Same(dependent.Category, principal2);
            Assert.Contains(dependent, principal2.Products);
            Assert.DoesNotContain(dependent, principal1.Products);

            dependent.CategoryId = 0;

            fixer.ForeignKeyPropertyChanged(dependentEntry, model.GetEntityType(typeof(Product)).GetProperty("CategoryId"), 12, 11);

            Assert.Null(dependent.Category);
            Assert.DoesNotContain(dependent, principal2.Products);
            Assert.DoesNotContain(dependent, principal1.Products);
        }

        [Fact]
        public void Does_fixup_of_related_principals_when_FK_is_changed()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal1 = new Category { Id = 11 };
            var principal2 = new Category { Id = 12 };
            var dependent = new Product { Id = 21, CategoryId = 12 };

            manager.StartTracking(manager.GetOrCreateEntry(principal1));
            manager.StartTracking(manager.GetOrCreateEntry(principal2));

            var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

            var fixer = CreateNavigationFixer(contextServices);
            dependentEntry.SetEntityState(EntityState.Added);

            Assert.Same(dependent.Category, principal2);
            Assert.Contains(dependent, principal2.Products);
            Assert.DoesNotContain(dependent, principal1.Products);

            dependent.CategoryId = 11;

            fixer.ForeignKeyPropertyChanged(dependentEntry, model.GetEntityType(typeof(Product)).GetProperty("CategoryId"), 12, 11);

            Assert.Same(dependent.Category, principal1);
            Assert.Contains(dependent, principal1.Products);
            Assert.DoesNotContain(dependent, principal2.Products);
        }

        [Fact]
        public void Does_fixup_of_one_to_one_relationship_when_FK_changes()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal1 = new Product { Id = 21 };
            var principal2 = new Product { Id = 22 };
            var dependent = new ProductDetail { Id = 21 };

            var principalEntry1 = manager.StartTracking(manager.GetOrCreateEntry(principal1));
            var principalEntry2 = manager.StartTracking(manager.GetOrCreateEntry(principal2));
            var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

            var fixer = CreateNavigationFixer(contextServices);

            principalEntry1.SetEntityState(EntityState.Added);

            Assert.Same(principal1, dependent.Product);
            Assert.Same(dependent, principal1.Detail);
            Assert.Null(principal2.Detail);

            dependent.Id = 22;

            fixer.ForeignKeyPropertyChanged(dependentEntry, model.GetEntityType(typeof(ProductDetail)).GetProperty("Id"), 21, 22);

            Assert.Same(principal2, dependent.Product);
            Assert.Same(dependent, principal2.Detail);
            Assert.Null(principal1.Detail);
        }

        [Fact]
        public void Does_fixup_of_one_to_one_relationship_when_FK_cleared()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal = new Product { Id = 21 };
            var dependent = new ProductDetail { Id = 21 };

            var principalEntry = manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

            var fixer = CreateNavigationFixer(contextServices);

            principalEntry.SetEntityState(EntityState.Added);

            Assert.Same(principal, dependent.Product);
            Assert.Same(dependent, principal.Detail);

            dependent.Id = 0;

            fixer.ForeignKeyPropertyChanged(dependentEntry, model.GetEntityType(typeof(ProductDetail)).GetProperty("Id"), 21, 0);

            Assert.Null(dependent.Product);
            Assert.Null(principal.Detail);
        }

        [Fact]
        public void Does_fixup_of_one_to_one_relationship_when_FK_set()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal = new Product { Id = 21 };
            var dependent = new ProductDetail { Id = 7 };

            var principalEntry = manager.StartTracking(manager.GetOrCreateEntry(principal));
            var dependentEntry = manager.StartTracking(manager.GetOrCreateEntry(dependent));

            var fixer = CreateNavigationFixer(contextServices);

            principalEntry.SetEntityState(EntityState.Added);

            Assert.Null(dependent.Product);
            Assert.Null(principal.Detail);

            dependent.Id = 21;

            fixer.ForeignKeyPropertyChanged(dependentEntry, model.GetEntityType(typeof(ProductDetail)).GetProperty("Id"), 7, 21);

            Assert.Same(principal, dependent.Product);
            Assert.Same(dependent, principal.Detail);
        }

        [Fact]
        public void Can_steal_reference_of_one_to_one_relationship_when_FK_changes()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var principal1 = new Product { Id = 21 };
            var principal2 = new Product { Id = 22 };
            var dependent1 = new ProductDetail { Id = 21 };
            var dependent2 = new ProductDetail { Id = 22 };

            var principalEntry1 = manager.StartTracking(manager.GetOrCreateEntry(principal1));
            var principalEntry2 = manager.StartTracking(manager.GetOrCreateEntry(principal2));
            var dependentEntry1 = manager.StartTracking(manager.GetOrCreateEntry(dependent1));
            var dependentEntry2 = manager.StartTracking(manager.GetOrCreateEntry(dependent2));

            var fixer = CreateNavigationFixer(contextServices);

            principalEntry1.SetEntityState(EntityState.Added);
            principalEntry2.SetEntityState(EntityState.Added);

            Assert.Same(principal1, dependent1.Product);
            Assert.Same(dependent1, principal1.Detail);
            Assert.Same(principal2, dependent2.Product);
            Assert.Same(dependent2, principal2.Detail);

            dependent1.Id = 22;

            fixer.ForeignKeyPropertyChanged(dependentEntry1, model.GetEntityType(typeof(ProductDetail)).GetProperty("Id"), 21, 22);

            Assert.Same(principal2, dependent1.Product);
            Assert.Same(dependent1, principal2.Detail);
            Assert.Null(dependent2.Product);
            Assert.Null(principal1.Detail);
        }

        [Fact]
        public void Does_fixup_of_one_to_one_self_referencing_relationship_when_FK_changes()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var entity1 = new Product { Id = 21, AlternateProductId = 22 };
            var entity2 = new Product { Id = 22 };
            var entity3 = new Product { Id = 23 };

            var entry1 = manager.StartTracking(manager.GetOrCreateEntry(entity1));
            var entry2 = manager.StartTracking(manager.GetOrCreateEntry(entity2));
            var entry3 = manager.StartTracking(manager.GetOrCreateEntry(entity3));

            var fixer = CreateNavigationFixer(contextServices);

            entry1.SetEntityState(EntityState.Added);
            entry1.SetEntityState(EntityState.Added);
            entry3.SetEntityState(EntityState.Added);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Null(entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Null(entity3.OriginalProduct);

            entity1.AlternateProductId = 23;

            fixer.ForeignKeyPropertyChanged(entry1, model.GetEntityType(typeof(Product)).GetProperty("AlternateProductId"), 22, 23);

            Assert.Same(entity3, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Null(entity2.AlternateProduct);
            Assert.Null(entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Same(entity1, entity3.OriginalProduct);
        }

        [Fact]
        public void Can_steal_reference_of_one_to_one_self_referencing_relationship_when_FK_changes()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var entity1 = new Product { Id = 21, AlternateProductId = 22 };
            var entity2 = new Product { Id = 22, AlternateProductId = 23 };
            var entity3 = new Product { Id = 23 };

            var entry1 = manager.StartTracking(manager.GetOrCreateEntry(entity1));
            var entry2 = manager.StartTracking(manager.GetOrCreateEntry(entity2));
            var entry3 = manager.StartTracking(manager.GetOrCreateEntry(entity3));

            var fixer = CreateNavigationFixer(contextServices);

            entry1.SetEntityState(EntityState.Added);
            entry1.SetEntityState(EntityState.Added);
            entry3.SetEntityState(EntityState.Added);

            Assert.Same(entity2, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Same(entity3, entity2.AlternateProduct);
            Assert.Same(entity1, entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Same(entity2, entity3.OriginalProduct);

            entity1.AlternateProductId = 23;

            fixer.ForeignKeyPropertyChanged(entry1, model.GetEntityType(typeof(Product)).GetProperty("AlternateProductId"), 22, 23);

            Assert.Same(entity3, entity1.AlternateProduct);
            Assert.Null(entity1.OriginalProduct);

            Assert.Null(entity2.AlternateProduct);
            Assert.Null(entity2.OriginalProduct);

            Assert.Null(entity3.AlternateProduct);
            Assert.Same(entity1, entity3.OriginalProduct);

            Assert.Null(entity2.AlternateProductId);
        }

        [Fact]
        public void Does_fixup_of_all_related_principals_when_part_of_overlapping_composite_FK_is_changed()
        {
            var model = BuildModel();
            var contextServices = CreateContextServices(model);
            var manager = contextServices.GetRequiredService<IStateManager>();

            var photo1 = new ProductPhoto { ProductId = 1, PhotoId = "Photo1" };
            var photo2 = new ProductPhoto { ProductId = 1, PhotoId = "Photo2" };
            var photo3 = new ProductPhoto { ProductId = 2, PhotoId = "Photo1" };
            var photo4 = new ProductPhoto { ProductId = 2, PhotoId = "Photo2" };

            var reviewId1 = Guid.NewGuid();
            var reviewId2 = Guid.NewGuid();
            var review1 = new ProductReview { ProductId = 1, ReviewId = reviewId1 };
            var review2 = new ProductReview { ProductId = 1, ReviewId = reviewId2 };
            var review3 = new ProductReview { ProductId = 2, ReviewId = reviewId1 };
            var review4 = new ProductReview { ProductId = 2, ReviewId = reviewId2 };

            var tag1 = new ProductTag { Id = 1, ProductId = 1, PhotoId = "Photo1", ReviewId = reviewId1 };
            var tag2 = new ProductTag { Id = 2, ProductId = 1, PhotoId = "Photo1", ReviewId = reviewId2 };
            var tag3 = new ProductTag { Id = 3, ProductId = 1, PhotoId = "Photo2", ReviewId = reviewId1 };
            var tag4 = new ProductTag { Id = 4, ProductId = 1, PhotoId = "Photo2", ReviewId = reviewId2 };
            var tag5 = new ProductTag { Id = 5, ProductId = 2, PhotoId = "Photo1", ReviewId = reviewId1 };
            var tag6 = new ProductTag { Id = 6, ProductId = 2, PhotoId = "Photo1", ReviewId = reviewId2 };
            var tag7 = new ProductTag { Id = 7, ProductId = 2, PhotoId = "Photo2", ReviewId = reviewId1 };
            var tag8 = new ProductTag { Id = 8, ProductId = 2, PhotoId = "Photo2", ReviewId = reviewId2 };

            var photoEntry1 = manager.StartTracking(manager.GetOrCreateEntry(photo1));
            var photoEntry2 = manager.StartTracking(manager.GetOrCreateEntry(photo2));
            var photoEntry3 = manager.StartTracking(manager.GetOrCreateEntry(photo3));
            var photoEntry4 = manager.StartTracking(manager.GetOrCreateEntry(photo4));

            var reviewEntry1 = manager.StartTracking(manager.GetOrCreateEntry(review1));
            var reviewEntry2 = manager.StartTracking(manager.GetOrCreateEntry(review2));
            var reviewEntry3 = manager.StartTracking(manager.GetOrCreateEntry(review3));
            var reviewEntry4 = manager.StartTracking(manager.GetOrCreateEntry(review4));

            var tagEntry1 = manager.StartTracking(manager.GetOrCreateEntry(tag1));
            var tagEntry2 = manager.StartTracking(manager.GetOrCreateEntry(tag2));
            var tagEntry3 = manager.StartTracking(manager.GetOrCreateEntry(tag3));
            var tagEntry4 = manager.StartTracking(manager.GetOrCreateEntry(tag4));
            var tagEntry5 = manager.StartTracking(manager.GetOrCreateEntry(tag5));
            var tagEntry6 = manager.StartTracking(manager.GetOrCreateEntry(tag6));
            var tagEntry7 = manager.StartTracking(manager.GetOrCreateEntry(tag7));
            var tagEntry8 = manager.StartTracking(manager.GetOrCreateEntry(tag8));

            var fixer = CreateNavigationFixer(contextServices);

            photoEntry1.SetEntityState(EntityState.Added);
            photoEntry2.SetEntityState(EntityState.Added);
            photoEntry3.SetEntityState(EntityState.Added);
            photoEntry4.SetEntityState(EntityState.Added);
            reviewEntry1.SetEntityState(EntityState.Added);
            reviewEntry2.SetEntityState(EntityState.Added);
            reviewEntry3.SetEntityState(EntityState.Added);
            reviewEntry4.SetEntityState(EntityState.Added);
            tagEntry1.SetEntityState(EntityState.Added);
            tagEntry2.SetEntityState(EntityState.Added);
            tagEntry3.SetEntityState(EntityState.Added);
            tagEntry4.SetEntityState(EntityState.Added);
            tagEntry5.SetEntityState(EntityState.Added);
            tagEntry6.SetEntityState(EntityState.Added);
            tagEntry7.SetEntityState(EntityState.Added);
            tagEntry8.SetEntityState(EntityState.Added);

            Assert.Equal(new[] { tag1, tag2 }, photo1.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag3, tag4 }, photo2.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag5, tag6 }, photo3.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag7, tag8 }, photo4.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag1, tag3 }, review1.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag2, tag4 }, review2.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag5, tag7 }, review3.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag6, tag8 }, review4.ProductTags.OrderBy(t => t.Id).ToArray());

            Assert.Same(photo1, tag1.Photo);
            Assert.Same(photo1, tag2.Photo);
            Assert.Same(photo2, tag3.Photo);
            Assert.Same(photo2, tag4.Photo);
            Assert.Same(photo3, tag5.Photo);
            Assert.Same(photo3, tag6.Photo);
            Assert.Same(photo4, tag7.Photo);
            Assert.Same(photo4, tag8.Photo);

            Assert.Same(review1, tag1.Review);
            Assert.Same(review2, tag2.Review);
            Assert.Same(review1, tag3.Review);
            Assert.Same(review2, tag4.Review);
            Assert.Same(review3, tag5.Review);
            Assert.Same(review4, tag6.Review);
            Assert.Same(review3, tag7.Review);
            Assert.Same(review4, tag8.Review);

            // Changes both FK relationships
            tag1.ProductId = 2;

            fixer.ForeignKeyPropertyChanged(tagEntry1, model.GetEntityType(typeof(ProductTag)).GetProperty("ProductId"), 1, 2);

            Assert.Equal(new[] { tag2 }, photo1.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag3, tag4 }, photo2.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag1, tag5, tag6 }, photo3.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag7, tag8 }, photo4.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag3 }, review1.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag2, tag4 }, review2.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag1, tag5, tag7 }, review3.ProductTags.OrderBy(t => t.Id).ToArray());
            Assert.Equal(new[] { tag6, tag8 }, review4.ProductTags.OrderBy(t => t.Id).ToArray());

            Assert.Same(photo3, tag1.Photo);
            Assert.Same(photo1, tag2.Photo);
            Assert.Same(photo2, tag3.Photo);
            Assert.Same(photo2, tag4.Photo);
            Assert.Same(photo3, tag5.Photo);
            Assert.Same(photo3, tag6.Photo);
            Assert.Same(photo4, tag7.Photo);
            Assert.Same(photo4, tag8.Photo);

            Assert.Same(review3, tag1.Review);
            Assert.Same(review2, tag2.Review);
            Assert.Same(review1, tag3.Review);
            Assert.Same(review2, tag4.Review);
            Assert.Same(review3, tag5.Review);
            Assert.Same(review4, tag6.Review);
            Assert.Same(review3, tag7.Review);
            Assert.Same(review4, tag8.Review);
        }

        private static IServiceProvider CreateContextServices(IModel model = null)
        {
            return TestHelpers.Instance.CreateContextServices(model ?? BuildModel());
        }

        private class Category
        {
            public int Id { get; set; }

            public ICollection<Product> Products { get; } = new List<Product>();
        }

        private class Product
        {
            public int Id { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }

            public ProductDetail Detail { get; set; }

            public int? AlternateProductId { get; set; }
            public Product AlternateProduct { get; set; }
            public Product OriginalProduct { get; set; }
        }

        private class ProductDetail : IEnumerable<Product>
        {
            public int Id { get; set; }

            public Product Product { get; set; }

            public IEnumerator<Product> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class ProductPhoto
        {
            public int ProductId { get; set; }
            public string PhotoId { get; set; }

            public ICollection<ProductTag> ProductTags { get; } = new HashSet<ProductTag>();
        }

        private class ProductReview
        {
            public int ProductId { get; set; }
            public Guid ReviewId { get; set; }

            public ICollection<ProductTag> ProductTags { get; } = new HashSet<ProductTag>();
        }

        private class ProductTag
        {
            public int Id { get; set; }

            public int ProductId { get; set; }
            public string PhotoId { get; set; }
            public Guid ReviewId { get; set; }

            public ProductPhoto Photo { get; set; }
            public ProductReview Review { get; set; }
        }

        private static IModel BuildModel()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<Product>(b =>
                {
                    b.Reference(e => e.AlternateProduct).InverseReference(e => e.OriginalProduct)
                        .ForeignKey<Product>(e => e.AlternateProductId);

                    b.Reference(e => e.Detail).InverseReference(e => e.Product)
                        .ForeignKey<ProductDetail>(e => e.Id);
                });

            builder.Entity<Category>().Collection(e => e.Products).InverseReference(e => e.Category);

            builder.Entity<ProductDetail>();

            builder.Entity<ProductPhoto>(b =>
                {
                    b.Key(e => new { e.ProductId, e.PhotoId });
                    b.Collection(e => e.ProductTags).InverseReference(e => e.Photo)
                        .ForeignKey(e => new { e.ProductId, e.PhotoId });
                });

            builder.Entity<ProductReview>(b =>
                {
                    b.Key(e => new { e.ProductId, e.ReviewId });
                    b.Collection(e => e.ProductTags).InverseReference(e => e.Review)
                        .ForeignKey(e => new { e.ProductId, e.ReviewId });
                });

            builder.Entity<ProductTag>();

            return builder.Model;
        }

        private static INavigationFixer CreateNavigationFixer(IServiceProvider contextServices)
        {
            return (INavigationFixer)contextServices.GetRequiredService<IEntityStateListener>();
        }
    }
}

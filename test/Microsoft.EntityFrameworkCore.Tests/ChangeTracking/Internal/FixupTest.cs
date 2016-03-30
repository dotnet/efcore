// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.ChangeTracking.Internal
{
    public class FixupTest
    {
        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_FK_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, Category = principal, CategoryId = principal.Id };
                principal.Products.Add(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, Category = principal };
                principal.Products.Add(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);

                if (entityState == EntityState.Unchanged)
                {
                    // Dependent FK gets modified when principal is attached 
                    entityState = EntityState.Modified;
                }
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, CategoryId = principal.Id };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, CategoryId = principal.Id };
                principal.Products.Add(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, CategoryId = principal.Id, Category = principal };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78 };
                principal.Products.Add(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);

                if (entityState == EntityState.Unchanged)
                {
                    // Dependent FK gets modified when principal is attached 
                    entityState = EntityState.Modified;
                }
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, Category = principal };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);

                if (entityState == EntityState.Unchanged)
                {
                    // Dependent FK gets modified when principal is attached 
                    entityState = EntityState.Modified;
                }
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_FK_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, Category = principal, CategoryId = principal.Id };
                principal.Products.Add(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, Category = principal };
                principal.Products.Add(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, CategoryId = principal.Id };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, CategoryId = principal.Id };
                principal.Products.Add(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, CategoryId = principal.Id, Category = principal };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78 };
                principal.Products.Add(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category { Id = 77 };
                var dependent = new Product { Id = 78, Category = principal };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78, CategoryId = principal.Id };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78, CategoryId = principal.Id };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78, CategoryId = principal.Id };
                principal.Products.Add(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78 };
                principal.Products.Add(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);

                if (entityState == EntityState.Unchanged)
                {
                    // Dependent FK gets modified when principal is attached 
                    entityState = EntityState.Modified;
                }
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78, CategoryId = principal.Id };
                principal.Products.Add(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78 };
                principal.Products.Add(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78, CategoryId = principal.Id };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78, CategoryId = principal.Id, Category = principal };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78, Category = principal };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);

                if (entityState == EntityState.Unchanged)
                {
                    // Dependent FK gets modified when principal is attached 
                    entityState = EntityState.Modified;
                }
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78, CategoryId = principal.Id };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78, CategoryId = principal.Id, Category = principal };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78, Category = principal };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Same(principal, dependent.Category);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_many_no_navs_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryNN { Id = 77 };
                var dependent = new ProductNN { Id = 78, CategoryId = principal.Id };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_many_no_navs_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryNN { Id = 77 };
                var dependent = new ProductNN { Id = 78, CategoryId = principal.Id };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.CategoryId);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_FK_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, Parent = principal, ParentId = principal.Id };
                principal.Child = dependent;

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, Parent = principal };
                principal.Child = dependent;

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);

                if (entityState == EntityState.Unchanged)
                {
                    // Dependent FK gets modified when principal is attached 
                    entityState = EntityState.Modified;
                }
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, ParentId = principal.Id };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, ParentId = principal.Id };
                principal.Child = dependent;

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, ParentId = principal.Id, Parent = principal };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78 };
                principal.Child = dependent;

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);

                if (entityState == EntityState.Unchanged)
                {
                    // Dependent FK gets modified when principal is attached 
                    entityState = EntityState.Modified;
                }
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, Parent = principal };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);

                if (entityState == EntityState.Unchanged)
                {
                    // Dependent FK gets modified when principal is attached 
                    entityState = EntityState.Modified;
                }
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_FK_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, Parent = principal, ParentId = principal.Id };
                principal.Child = dependent;

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, Parent = principal };
                principal.Child = dependent;

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, ParentId = principal.Id };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, ParentId = principal.Id };
                principal.Child = dependent;

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, ParentId = principal.Id, Parent = principal };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78 };
                principal.Child = dependent;

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent { Id = 77 };
                var dependent = new Child { Id = 78, Parent = principal };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78, ParentId = principal.Id };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78, ParentId = principal.Id };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78, ParentId = principal.Id };
                principal.Child = dependent;

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78 };
                principal.Child = dependent;

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);

                if (entityState == EntityState.Unchanged)
                {
                    // Dependent FK gets modified when principal is attached 
                    entityState = EntityState.Modified;
                }
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78, ParentId = principal.Id };
                principal.Child = dependent;

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78 };
                principal.Child = dependent;

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(dependent, principal.Child);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78, ParentId = principal.Id };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78, ParentId = principal.Id, Parent = principal };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78, Parent = principal };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);

                if (entityState == EntityState.Unchanged)
                {
                    // Dependent FK gets modified when principal is attached 
                    entityState = EntityState.Modified;
                }
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78, ParentId = principal.Id };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78, ParentId = principal.Id, Parent = principal };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78, Parent = principal };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_then_principal_one_to_one_no_navs_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentNN { Id = 77 };
                var dependent = new ChildNN { Id = 78, ParentId = principal.Id };

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_then_dependent_one_to_one_no_navs_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentNN { Id = 77 };
                var dependent = new ChildNN { Id = 78, ParentId = principal.Id };

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                Assert.Equal(principal.Id, dependent.ParentId);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState, context.Entry(dependent).State);
            }
        }

        [Fact]
        public void Navigation_fixup_happens_when_new_entities_are_tracked()
        {
            using (var context = new FixupContext())
            {
                context.Add(new Category { Id = 11 });
                context.Add(new Category { Id = 12 });
                context.Add(new Category { Id = 13 });

                context.Add(new Product { Id = 21, CategoryId = 11 });
                AssertAllFixedUp(context);
                context.Add(new Product { Id = 22, CategoryId = 11 });
                AssertAllFixedUp(context);
                context.Add(new Product { Id = 23, CategoryId = 11 });
                AssertAllFixedUp(context);
                context.Add(new Product { Id = 24, CategoryId = 12 });
                AssertAllFixedUp(context);
                context.Add(new Product { Id = 25, CategoryId = 12 });
                AssertAllFixedUp(context);

                context.Add(new SpecialOffer { Id = 31, ProductId = 22 });
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer { Id = 32, ProductId = 22 });
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer { Id = 33, ProductId = 24 });
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer { Id = 34, ProductId = 24 });
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer { Id = 35, ProductId = 24 });
                AssertAllFixedUp(context);

                Assert.Equal(3, context.ChangeTracker.Entries<Category>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Product>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<SpecialOffer>().Count());
            }
        }

        [Fact]
        public void Navigation_fixup_happens_when_entities_are_tracked_from_query()
        {
            using (var context = new FixupContext())
            {
                var categoryType = context.Model.FindEntityType(typeof(Category));
                var productType = context.Model.FindEntityType(typeof(Product));
                var offerType = context.Model.FindEntityType(typeof(SpecialOffer));

                var stateManager = context.ChangeTracker.GetInfrastructure();

                stateManager.StartTrackingFromQuery(categoryType, new Category { Id = 11 }, new ValueBuffer(new object[] { 11 }));
                stateManager.StartTrackingFromQuery(categoryType, new Category { Id = 12 }, new ValueBuffer(new object[] { 12 }));
                stateManager.StartTrackingFromQuery(categoryType, new Category { Id = 13 }, new ValueBuffer(new object[] { 13 }));

                stateManager.StartTrackingFromQuery(productType, new Product { Id = 21, CategoryId = 11 }, new ValueBuffer(new object[] { 21, 11 }));
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(productType, new Product { Id = 22, CategoryId = 11 }, new ValueBuffer(new object[] { 22, 11 }));
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(productType, new Product { Id = 23, CategoryId = 11 }, new ValueBuffer(new object[] { 23, 11 }));
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(productType, new Product { Id = 24, CategoryId = 12 }, new ValueBuffer(new object[] { 24, 12 }));
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(productType, new Product { Id = 25, CategoryId = 12 }, new ValueBuffer(new object[] { 25, 12 }));
                AssertAllFixedUp(context);

                stateManager.StartTrackingFromQuery(offerType, new SpecialOffer { Id = 31, ProductId = 22 }, new ValueBuffer(new object[] { 31, 22 }));
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(offerType, new SpecialOffer { Id = 32, ProductId = 22 }, new ValueBuffer(new object[] { 32, 22 }));
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(offerType, new SpecialOffer { Id = 33, ProductId = 24 }, new ValueBuffer(new object[] { 33, 24 }));
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(offerType, new SpecialOffer { Id = 34, ProductId = 24 }, new ValueBuffer(new object[] { 34, 24 }));
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(offerType, new SpecialOffer { Id = 35, ProductId = 24 }, new ValueBuffer(new object[] { 35, 24 }));

                AssertAllFixedUp(context);

                Assert.Equal(3, context.ChangeTracker.Entries<Category>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Product>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<SpecialOffer>().Count());
            }
        }

        [Fact]
        public void Navigation_fixup_is_non_destructive_to_existing_graphs()
        {
            using (var context = new FixupContext())
            {
                var category11 = new Category { Id = 11 };
                var category12 = new Category { Id = 12 };
                var category13 = new Category { Id = 13 };

                var product21 = new Product { Id = 21, CategoryId = 11, Category = category11 };
                var product22 = new Product { Id = 22, CategoryId = 11, Category = category11 };
                var product23 = new Product { Id = 23, CategoryId = 11, Category = category11 };
                var product24 = new Product { Id = 24, CategoryId = 12, Category = category12 };
                var product25 = new Product { Id = 25, CategoryId = 12, Category = category12 };

                category11.Products.Add(product21);
                category11.Products.Add(product22);
                category11.Products.Add(product23);
                category12.Products.Add(product24);
                category12.Products.Add(product25);

                var specialOffer31 = new SpecialOffer { Id = 31, ProductId = 22, Product = product22 };
                var specialOffer32 = new SpecialOffer { Id = 32, ProductId = 22, Product = product22 };
                var specialOffer33 = new SpecialOffer { Id = 33, ProductId = 24, Product = product24 };
                var specialOffer34 = new SpecialOffer { Id = 34, ProductId = 24, Product = product24 };
                var specialOffer35 = new SpecialOffer { Id = 35, ProductId = 24, Product = product24 };

                product22.SpecialOffers.Add(specialOffer31);
                product22.SpecialOffers.Add(specialOffer32);
                product24.SpecialOffers.Add(specialOffer33);
                product24.SpecialOffers.Add(specialOffer34);
                product24.SpecialOffers.Add(specialOffer35);

                context.Add(category11);
                AssertAllFixedUp(context);
                context.Add(category12);
                AssertAllFixedUp(context);
                context.Add(category13);
                AssertAllFixedUp(context);

                context.Add(product21);
                AssertAllFixedUp(context);
                context.Add(product22);
                AssertAllFixedUp(context);
                context.Add(product23);
                AssertAllFixedUp(context);
                context.Add(product24);
                AssertAllFixedUp(context);
                context.Add(product25);
                AssertAllFixedUp(context);

                context.Add(specialOffer31);
                AssertAllFixedUp(context);
                context.Add(specialOffer32);
                AssertAllFixedUp(context);
                context.Add(specialOffer33);
                AssertAllFixedUp(context);
                context.Add(specialOffer34);
                AssertAllFixedUp(context);
                context.Add(specialOffer35);
                AssertAllFixedUp(context);

                Assert.Equal(3, category11.Products.Count);
                Assert.Equal(2, category12.Products.Count);
                Assert.Equal(0, category13.Products.Count);

                Assert.Equal(0, product21.SpecialOffers.Count);
                Assert.Equal(2, product22.SpecialOffers.Count);
                Assert.Equal(0, product23.SpecialOffers.Count);
                Assert.Equal(3, product24.SpecialOffers.Count);
                Assert.Equal(0, product25.SpecialOffers.Count);

                Assert.Equal(3, context.ChangeTracker.Entries<Category>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Product>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<SpecialOffer>().Count());
            }
        }

        public void AssertAllFixedUp(DbContext context)
        {
            foreach (var entry in context.ChangeTracker.Entries<Product>())
            {
                var product = entry.Entity;
                if (product.CategoryId == 11
                    || product.CategoryId == 12)
                {
                    Assert.Equal(product.CategoryId, product.Category.Id);
                    Assert.Contains(product, product.Category.Products);
                }
                else
                {
                    Assert.Null(product.Category);
                }
            }

            foreach (var entry in context.ChangeTracker.Entries<SpecialOffer>())
            {
                var offer = entry.Entity;
                if (offer.ProductId == 22
                    || offer.ProductId == 24)
                {
                    Assert.Equal(offer.ProductId, offer.Product.Id);
                    Assert.Contains(offer, offer.Product.SpecialOffers);
                }
                else
                {
                    Assert.Null(offer.Product);
                }
            }
        }

        private class Parent
        {
            public int Id { get; set; }

            public Child Child { get; set; }
        }

        private class Child
        {
            public int Id { get; set; }
            public int ParentId { get; set; }

            public Parent Parent { get; set; }
        }

        private class ParentPN
        {
            public int Id { get; set; }

            public ChildPN Child { get; set; }
        }

        private class ChildPN
        {
            public int Id { get; set; }

            public int ParentId { get; set; }
        }

        private class ParentDN
        {
            public int Id { get; set; }
        }

        private class ChildDN
        {
            public int Id { get; set; }
            public int ParentId { get; set; }

            public ParentDN Parent { get; set; }
        }

        private class ParentNN
        {
            public int Id { get; set; }
        }

        private class ChildNN
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
        }

        private class CategoryDN
        {
            public int Id { get; set; }
        }

        private class ProductDN
        {
            public int Id { get; set; }
            public int CategoryId { get; set; }

            public CategoryDN Category { get; set; }
        }

        private class CategoryPN
        {
            public CategoryPN()
            {
                Products = new List<ProductPN>();
            }

            public int Id { get; set; }

            public ICollection<ProductPN> Products { get; }
        }

        private class ProductPN
        {
            public int Id { get; set; }
            public int CategoryId { get; set; }
        }

        private class CategoryNN
        {
            public int Id { get; set; }
        }

        private class ProductNN
        {
            public int Id { get; set; }
            public int CategoryId { get; set; }
        }

        private class Category
        {
            public Category()
            {
                Products = new List<Product>();
            }

            public int Id { get; set; }

            public ICollection<Product> Products { get; }
        }

        private class Product
        {
            public Product()
            {
                SpecialOffers = new List<SpecialOffer>();
            }

            public int Id { get; set; }
            public int CategoryId { get; set; }

            public Category Category { get; set; }
            public ICollection<SpecialOffer> SpecialOffers { get; }
        }

        private class SpecialOffer
        {
            public int Id { get; set; }
            public int ProductId { get; set; }

            public Product Product { get; set; }
        }

        private class FixupContext : DbContext
        {
            public FixupContext()
            {
                ChangeTracker.AutoDetectChangesEnabled = false;
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Product>()
                    .HasMany(e => e.SpecialOffers)
                    .WithOne(e => e.Product);

                modelBuilder.Entity<Category>()
                    .HasMany(e => e.Products)
                    .WithOne(e => e.Category);

                modelBuilder.Entity<CategoryPN>()
                    .HasMany(e => e.Products)
                    .WithOne()
                    .HasForeignKey(e => e.CategoryId);

                modelBuilder.Entity<ProductDN>()
                    .HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId);

                modelBuilder.Entity<ProductNN>()
                    .HasOne<CategoryNN>()
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId);

                modelBuilder.Entity<Parent>()
                    .HasOne(e => e.Child)
                    .WithOne(e => e.Parent)
                    .HasForeignKey<Child>(e => e.ParentId);

                modelBuilder.Entity<ParentPN>()
                    .HasOne(e => e.Child)
                    .WithOne()
                    .HasForeignKey<ChildPN>(e => e.ParentId);

                modelBuilder.Entity<ChildDN>()
                    .HasOne(e => e.Parent)
                    .WithOne()
                    .HasForeignKey<ChildDN>(e => e.ParentId);

                modelBuilder.Entity<ParentNN>()
                    .HasOne<ChildNN>()
                    .WithOne()
                    .HasForeignKey<ChildNN>(e => e.ParentId);
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase();
        }
    }
}

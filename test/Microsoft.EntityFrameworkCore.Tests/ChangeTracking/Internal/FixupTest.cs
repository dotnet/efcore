// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
                var principal = new Category(77);
                var dependent = new Product(78, principal.Id);
                dependent.SetCategory(principal);
                principal.AddProduct(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, 0);
                dependent.SetCategory(principal);
                principal.AddProduct(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, principal.Id);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, principal.Id);
                principal.AddProduct(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, principal.Id);
                dependent.SetCategory(principal);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, 0);
                principal.AddProduct(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, 0);
                dependent.SetCategory(principal);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList().ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, principal.Id);
                dependent.SetCategory(principal);
                principal.AddProduct(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, 0);
                dependent.SetCategory(principal);
                principal.AddProduct(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, principal.Id);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, principal.Id);
                principal.AddProduct(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, principal.Id);
                dependent.SetCategory(principal);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, 0);
                principal.AddProduct(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Category(77);
                var dependent = new Product(78, 0);
                dependent.SetCategory(principal);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, principal.Id);
                dependent.SetParent(principal);
                principal.SetChild(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, 0);
                dependent.SetParent(principal);
                principal.SetChild(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, principal.Id);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, principal.Id);
                principal.SetChild(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, principal.Id);
                dependent.SetParent(principal);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, 0);
                principal.SetChild(dependent);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, 0);
                dependent.SetParent(principal);

                context.Entry(dependent).State = entityState;
                context.Entry(principal).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, principal.Id);
                dependent.SetParent(principal);
                principal.SetChild(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, 0);
                dependent.SetParent(principal);
                principal.SetChild(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, principal.Id);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, principal.Id);
                principal.SetChild(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, principal.Id);
                dependent.SetParent(principal);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, 0);
                principal.SetChild(dependent);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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
                var principal = new Parent(77);
                var dependent = new Child(78, 0);
                dependent.SetParent(principal);

                context.Entry(principal).State = entityState;
                context.Entry(dependent).State = entityState;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
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

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_FK_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetCategoryId(principal.Id);
                dependent.SetCategory(principal);
                principal.AddProduct(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetCategory(principal);
                principal.AddProduct(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetCategoryId(principal.Id);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Null(dependent.Category);
                            Assert.Null(principal.Products);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetCategoryId(principal.Id);
                principal.AddProduct(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Null(dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetCategoryId(principal.Id);
                dependent.SetCategory(principal);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(dependent).State = entityState;

                principal.AddProduct(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(0, dependent.CategoryId);
                            Assert.Null(dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetCategory(principal);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_FK_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(principal).State = entityState;

                dependent.SetCategoryId(principal.Id);
                dependent.SetCategory(principal);
                principal.AddProduct(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_FK_not_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(principal).State = entityState;

                dependent.SetCategory(principal);
                principal.AddProduct(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(principal).State = entityState;

                dependent.SetCategoryId(principal.Id);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Null(dependent.Category);
                            Assert.Null(principal.Products);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(principal).State = entityState;

                dependent.SetCategoryId(principal.Id);
                principal.AddProduct(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(principal).State = entityState;

                dependent.SetCategoryId(principal.Id);
                dependent.SetCategory(principal);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Null(principal.Products);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(principal).State = entityState;

                dependent.SetCategoryId(principal.Id);
                principal.AddProduct(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Category(77);
                var dependent = new Product(77, 0);

                context.Entry(principal).State = entityState;

                dependent.SetCategory(principal);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(0, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Null(principal.Products);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.CategoryId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Empty(principal.Products);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_prin_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.CategoryId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Empty(principal.Products);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.CategoryId = principal.Id;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78 };

                context.Entry(dependent).State = entityState;

                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(0, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.CategoryId = principal.Id;
                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryPN { Id = 77 };
                var dependent = new ProductPN { Id = 78 };

                context.Entry(principal).State = entityState;

                principal.Products.Add(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products.ToList());
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.CategoryId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Null(dependent.Category);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.CategoryId = principal.Id;
                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_dep_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.CategoryId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Null(dependent.Category);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.CategoryId = principal.Id;
                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryDN { Id = 77 };
                var dependent = new ProductDN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.Category = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(0, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_many_no_navs_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryNN { Id = 77 };
                var dependent = new ProductNN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.CategoryId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_many_no_navs_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new CategoryNN { Id = 77 };
                var dependent = new ProductNN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.CategoryId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_FK_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetParentId(principal.Id);
                dependent.SetParent(principal);
                principal.SetChild(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetParent(principal);
                principal.SetChild(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetParentId(principal.Id);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Null(dependent.Parent);
                            Assert.Null(principal.Child);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetParentId(principal.Id);
                principal.SetChild(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Null(dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetParentId(principal.Id);
                dependent.SetParent(principal);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(dependent).State = entityState;

                principal.SetChild(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(0, dependent.ParentId);
                            Assert.Null(dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(dependent).State = entityState;

                dependent.SetParent(principal);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_FK_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(principal).State = entityState;

                dependent.SetParentId(principal.Id);
                dependent.SetParent(principal);
                principal.SetChild(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_FK_not_set_both_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(principal).State = entityState;

                dependent.SetParent(principal);
                principal.SetChild(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(principal).State = entityState;

                dependent.SetParentId(principal.Id);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Null(dependent.Parent);
                            Assert.Null(principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(principal).State = entityState;

                dependent.SetParentId(principal.Id);
                principal.SetChild(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(principal).State = entityState;

                dependent.SetParentId(principal.Id);
                dependent.SetParent(principal);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Null(principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(principal).State = entityState;

                principal.SetChild(dependent);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent(77);
                var dependent = new Child(78, 0);

                context.Entry(principal).State = entityState;

                dependent.SetParent(principal);

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(0, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Null(principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.ParentId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Null(principal.Child);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_prin_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.ParentId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Null(principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.ParentId = principal.Id;
                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78 };

                context.Entry(dependent).State = entityState;

                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(0, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_prin_uni_FK_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.ParentId = principal.Id;
                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_prin_uni_FK_not_set_principal_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN { Id = 77 };
                var dependent = new ChildPN { Id = 78 };

                context.Entry(principal).State = entityState;

                principal.Child = dependent;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Added, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.ParentId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Null(dependent.Parent);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.ParentId = principal.Id;
                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Equal(EntityState.Added, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_dep_uni_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.ParentId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Null(dependent.Parent);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_dep_uni_FK_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.ParentId = principal.Id;
                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_dep_uni_FK_not_set_dependent_nav_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentDN { Id = 77 };
                var dependent = new ChildDN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.Parent = principal;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(0, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_dependent_but_not_principal_one_to_one_no_navs_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentNN { Id = 77 };
                var dependent = new ChildNN { Id = 78 };

                context.Entry(dependent).State = entityState;

                dependent.ParentId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Equal(EntityState.Detached, context.Entry(principal).State);
                            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, context.Entry(dependent).State);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Add_principal_but_not_dependent_one_to_one_no_navs_FK_set_no_navs_set(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentNN { Id = 77 };
                var dependent = new ChildNN { Id = 78 };

                context.Entry(principal).State = entityState;

                dependent.ParentId = principal.Id;

                context.ChangeTracker.DetectChanges();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Equal(entityState, context.Entry(principal).State);
                            Assert.Equal(EntityState.Detached, context.Entry(dependent).State);
                        });
            }
        }

        [Fact] // Issue #6067
        public void Collection_nav_props_remain_fixed_up_after_manual_fixup_and_DetectChanges()
        {
            using (var context = new FixupContext())
            {
                var category = new Category(77);
                var product1 = new Product(777, 0);
                var product2 = new Product(778, 0);
                product1.SetCategory(category);
                product2.SetCategory(category);

                context.Add(product1);
                context.Add(product2);
                context.Add(new Category(78));

                context.SaveChanges();
            }

            using (var context = new FixupContext())
            {
                var category = context.Set<Product>().Include(c => c.Category).ToList().First().Category;

                Assert.Equal(2, category.Products.Count);

                var category2 = context.Set<Category>().ToList().Single(a => a != category);

                Assert.Null(category2.Products);

                var product = category.Products.First();
                category.Products.Remove(product);
                product.SetCategory(category2);
                category2.AddProduct(product);
                Assert.Equal(category.Id, product.CategoryId);

                context.ChangeTracker.DetectChanges();

                Assert.Equal(category2.Id, product.CategoryId);
                Assert.Equal(category, category.Products.Single().Category);
                Assert.Equal(category2, category2.Products.Single().Category); // Throws
            }
        }

        [Fact]
        public void Navigation_fixup_happens_when_new_entities_are_tracked()
        {
            using (var context = new FixupContext())
            {
                context.Add(new Category(11));
                context.Add(new Category(12));
                context.Add(new Category(13));

                context.Add(new Product(21, 11));
                AssertAllFixedUp(context);
                context.Add(new Product(22, 11));
                AssertAllFixedUp(context);
                context.Add(new Product(23, 11));
                AssertAllFixedUp(context);
                context.Add(new Product(24, 12));
                AssertAllFixedUp(context);
                context.Add(new Product(25, 12));
                AssertAllFixedUp(context);

                context.Add(new SpecialOffer(31, 22));
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer(32, 22));
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer(33, 24));
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer(34, 24));
                AssertAllFixedUp(context);
                context.Add(new SpecialOffer(35, 24));
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

                stateManager.BeginTrackingQuery();

                stateManager.StartTrackingFromQuery(categoryType, new Category(11), new ValueBuffer(new object[] { 11 }), null);
                stateManager.StartTrackingFromQuery(categoryType, new Category(12), new ValueBuffer(new object[] { 12 }), null);
                stateManager.StartTrackingFromQuery(categoryType, new Category(13), new ValueBuffer(new object[] { 13 }), null);

                stateManager.BeginTrackingQuery();

                stateManager.StartTrackingFromQuery(productType, new Product(21, 11), new ValueBuffer(new object[] { 21, 11 }), null);
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(productType, new Product(22, 11), new ValueBuffer(new object[] { 22, 11 }), null);
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(productType, new Product(23, 11), new ValueBuffer(new object[] { 23, 11 }), null);
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(productType, new Product(24, 12), new ValueBuffer(new object[] { 24, 12 }), null);
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(productType, new Product(25, 12), new ValueBuffer(new object[] { 25, 12 }), null);
                AssertAllFixedUp(context);

                stateManager.BeginTrackingQuery();

                stateManager.StartTrackingFromQuery(offerType, new SpecialOffer(31, 22), new ValueBuffer(new object[] { 31, 22 }), null);
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(offerType, new SpecialOffer(32, 22), new ValueBuffer(new object[] { 32, 22 }), null);
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(offerType, new SpecialOffer(33, 24), new ValueBuffer(new object[] { 33, 24 }), null);
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(offerType, new SpecialOffer(34, 24), new ValueBuffer(new object[] { 34, 24 }), null);
                AssertAllFixedUp(context);
                stateManager.StartTrackingFromQuery(offerType, new SpecialOffer(35, 24), new ValueBuffer(new object[] { 35, 24 }), null);

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
                var category11 = new Category(11);
                var category12 = new Category(12);
                var category13 = new Category(13);

                var product21 = new Product(21, 11);
                var product22 = new Product(22, 11);
                var product23 = new Product(23, 11);
                var product24 = new Product(24, 12);
                var product25 = new Product(25, 12);

                product21.SetCategory(category11);
                product22.SetCategory(category11);
                product23.SetCategory(category11);
                product24.SetCategory(category12);
                product25.SetCategory(category12);

                category11.AddProduct(product21);
                category11.AddProduct(product22);
                category11.AddProduct(product23);
                category12.AddProduct(product24);
                category12.AddProduct(product25);

                var specialOffer31 = new SpecialOffer(31, 22);
                var specialOffer32 = new SpecialOffer(32, 22);
                var specialOffer33 = new SpecialOffer(33, 24);
                var specialOffer34 = new SpecialOffer(34, 24);
                var specialOffer35 = new SpecialOffer(35, 24);

                specialOffer31.SetProduct(product22);
                specialOffer32.SetProduct(product22);
                specialOffer33.SetProduct(product24);
                specialOffer34.SetProduct(product24);
                specialOffer35.SetProduct(product24);

                product22.AddSpecialOffer(specialOffer31);
                product22.AddSpecialOffer(specialOffer32);
                product24.AddSpecialOffer(specialOffer33);
                product24.AddSpecialOffer(specialOffer34);
                product24.AddSpecialOffer(specialOffer35);

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
                Assert.Null(category13.Products);

                Assert.Null(product21.SpecialOffers);
                Assert.Equal(2, product22.SpecialOffers.Count);
                Assert.Null(product23.SpecialOffers);
                Assert.Equal(3, product24.SpecialOffers.Count);
                Assert.Null(product25.SpecialOffers);

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
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private int _id;
            private Child _child;

            public Parent(int id)
            {
                _id = id;
            }

            // ReSharper disable once ConvertToAutoProperty
            public int Id => _id;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public Child Child => _child;

            public void SetChild(Child child) => _child = child;
        }

        private class Child
        {
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private int _id;
            private int _parentId;
            private Parent _parent;

            public Child(int id, int parentId)
            {
                _id = id;
                _parentId = parentId;
            }

            // ReSharper disable once ConvertToAutoProperty
            public int Id => _id;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public int ParentId => _parentId;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public Parent Parent => _parent;

            public void SetParent(Parent parent) => _parent = parent;

            public void SetParentId(int parentId) => _parentId = parentId;
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
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private int _id;
            private ICollection<Product> _products;

            public Category()
            {
            }

            public Category(int id)
            {
                _id = id;
            }

            // ReSharper disable once ConvertToAutoProperty
            public int Id => _id;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public ICollection<Product> Products => _products;

            public void AddProduct(Product product)
                => (_products ?? (_products = new List<Product>())).Add(product);
        }

        private class Product
        {
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private int _id;
            private int _categoryId;
            private Category _category;
            private ICollection<SpecialOffer> _specialOffers;

            public Product()
            {
            }

            public Product(int id, int categoryId)
            {
                _id = id;
                _categoryId = categoryId;
            }

            // ReSharper disable once ConvertToAutoProperty
            public int Id => _id;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public int CategoryId => _categoryId;

            public void SetCategoryId(int categoryId) => _categoryId = categoryId;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public Category Category => _category;

            public void SetCategory(Category category) => _category = category;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public ICollection<SpecialOffer> SpecialOffers => _specialOffers;

            public void AddSpecialOffer(SpecialOffer specialOffer)
                => (_specialOffers ?? (_specialOffers = new List<SpecialOffer>())).Add(specialOffer);
        }

        private class SpecialOffer
        {
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private int _id;
            private int _productId;
            private Product _product;

            public SpecialOffer()
            {
            }

            public SpecialOffer(int id, int productId)
            {
                _id = id;
                _productId = productId;
            }

            // ReSharper disable once ConvertToAutoProperty
            public int Id => _id;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public int ProductId => _productId;

            public void SetProductId(int productId) => _productId = productId;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public Product Product => _product;

            public void SetProduct(Product product) => _product = product;
        }

        private class FixupContext : DbContext
        {
            public FixupContext()
            {
                ChangeTracker.AutoDetectChangesEnabled = false;
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Product>(b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.CategoryId);
                        b.HasMany(e => e.SpecialOffers)
                            .WithOne(e => e.Product);
                    });

                modelBuilder.Entity<Category>(b =>
                    {
                        b.HasKey(e => e.Id);
                        b.HasMany(e => e.Products)
                            .WithOne(e => e.Category);
                    });

                modelBuilder.Entity<SpecialOffer>(b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.ProductId);
                    });

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

                modelBuilder.Entity<Parent>(b =>
                    {
                        b.HasKey(e => e.Id);
                        b.HasOne(e => e.Child)
                            .WithOne(e => e.Parent)
                            .HasForeignKey<Child>(e => e.ParentId);
                    });

                modelBuilder.Entity<Child>(b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.ParentId);
                    });

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

        private void AssertFixup(DbContext context, Action asserts)
        {
            asserts();
            context.ChangeTracker.DetectChanges();
            asserts();
            context.ChangeTracker.DetectChanges();
            asserts();
            context.ChangeTracker.DetectChanges();
            asserts();
        }

        [Fact] // Issue #4853
        public void Collection_nav_props_remain_fixed_up_after_DetectChanges()
        {
            using (var db = new Context4853())
            {
                var assembly = new TestAssembly { Name = "Assembly1" };
                db.Classes.Add(new TestClass { Assembly = assembly, Name = "Class1" });
                db.Classes.Add(new TestClass { Assembly = assembly, Name = "Class2" });
                db.SaveChanges();
            }

            using (var db = new Context4853())
            {
                var assembly = db.Classes.Include(c => c.Assembly).ToList().First().Assembly;

                Assert.Equal(2, assembly.Classes.Count);

                db.ChangeTracker.DetectChanges();

                Assert.Equal(2, assembly.Classes.Count);
            }
        }

        private class TestAssembly
        {
            [Key]
            public string Name { get; set; }

            public ICollection<TestClass> Classes { get; } = new List<TestClass>();
        }

        private class TestClass
        {
            public TestAssembly Assembly { get; set; }
            public string Name { get; set; }
        }

        private class Context4853 : DbContext
        {
            public DbSet<TestAssembly> Assemblies { get; set; }
            public DbSet<TestClass> Classes { get; set; }

            protected internal override void OnConfiguring(DbContextOptionsBuilder options)
                => options.UseInMemoryDatabase();

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TestClass>(
                    x =>
                        {
                            x.Property<string>("AssemblyName");
                            x.HasKey("AssemblyName", nameof(TestClass.Name));
                            x.HasOne(c => c.Assembly).WithMany(a => a.Classes)
                                .HasForeignKey("AssemblyName");
                        });
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.ChangeTracking.Internal
{
    public class QueryFixupTest
    {
        [Fact]
        public void Query_dependent_include_principal()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var dependent = context.Set<Product>().Include(e => e.Category).Single();
                var principal = dependent.Category;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        });
            }
        }

        [Fact]
        public void Query_principal_include_dependent()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var principal = context.Set<Category>().Include(e => e.Products).Single();
                var dependent = principal.Products.Single();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        });
            }
        }

        [Fact]
        public void Query_dependent_include_principal_unidirectional()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var dependent = context.Set<ProductDN>().Include(e => e.Category).Single();
                var principal = dependent.Category;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                        });
            }
        }

        [Fact]
        public void Query_principal_include_dependent_unidirectional()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var principal = context.Set<CategoryPN>().Include(e => e.Products).Single();
                var dependent = principal.Products.Single();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Products);
                        });
            }
        }

        [Fact]
        public void Query_dependent_include_principal_one_to_one()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var dependent = context.Set<Child>().Include(e => e.Parent).Single();
                var principal = dependent.Parent;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                        });
            }
        }

        [Fact]
        public void Query_principal_include_dependent_one_to_one()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var principal = context.Set<Parent>().Include(e => e.Child).Single();
                var dependent = principal.Child;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                            Assert.Same(dependent, principal.Child);
                        });
            }
        }

        [Fact]
        public void Query_dependent_include_principal_unidirectional_one_to_one()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var dependent = context.Set<ChildDN>().Include(e => e.Parent).Single();
                var principal = dependent.Parent;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(principal, dependent.Parent);
                        });
            }
        }

        [Fact]
        public void Query_principal_include_dependent_unidirectional_one_to_one()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var principal = context.Set<ParentPN>().Include(e => e.Child).Single();
                var dependent = principal.Child;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentId);
                            Assert.Same(dependent, principal.Child);
                        });
            }
        }

        [Fact]
        public void Query_self_ref()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var widgets = context.Set<Widget>().ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Same(principal, dependent.ParentWidget);
                            Assert.Equal(new[] { dependent }.ToList(), principal.ChildWidgets);
                        });
            }
        }

        [Fact]
        public void Query_dependent_include_principal_self_ref()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var widgets = context.Set<Widget>().Include(e => e.ParentWidget).ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Same(principal, dependent.ParentWidget);
                            Assert.Equal(new[] { dependent }.ToList(), principal.ChildWidgets);
                        });
            }
        }

        [Fact]
        public void Query_principal_include_dependent_self_ref()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var widgets = context.Set<Widget>().Include(e => e.ChildWidgets).ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Same(principal, dependent.ParentWidget);
                            Assert.Equal(new[] { dependent }.ToList(), principal.ChildWidgets);
                        });
            }
        }

        [Fact]
        public void Query_self_ref_prinipal_nav_only()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var widgets = context.Set<WidgetPN>().ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.ChildWidgets);
                        });
            }
        }

        [Fact]
        public void Query_self_ref_dependent_nav_only()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var widgets = context.Set<WidgetDN>().ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Same(principal, dependent.ParentWidget);
                        });
            }
        }

        [Fact]
        public void Query_dependent_include_principal_self_ref_unidirectional()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var widgets = context.Set<WidgetDN>().Include(e => e.ParentWidget).ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Same(principal, dependent.ParentWidget);
                        });
            }
        }

        [Fact]
        public void Query_principal_include_dependent_self_ref_unidirectional()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var widgets = context.Set<WidgetPN>().Include(e => e.ChildWidgets).ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Equal(new[] { dependent }.ToList(), principal.ChildWidgets);
                        });
            }
        }

        [Fact]
        public void Query_self_ref_one_to_one()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var smidgets = context.Set<Smidget>().ToList();
                var dependent = smidgets.Single(e => e.Id == 78);
                var principal = smidgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                            Assert.Same(principal, dependent.ParentSmidget);
                            Assert.Same(dependent, principal.ChildSmidget);
                        });
            }
        }

        [Fact]
        public void Query_dependent_include_principal_self_ref_one_to_one()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var smidgets = context.Set<Smidget>().Include(e => e.ParentSmidget).ToList();
                var dependent = smidgets.Single(e => e.Id == 78);
                var principal = smidgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                            Assert.Same(principal, dependent.ParentSmidget);
                            Assert.Same(dependent, principal.ChildSmidget);
                        });
            }
        }

        [Fact]
        public void Query_principal_include_dependent_self_ref_one_to_one()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var smidgets = context.Set<Smidget>().Include(e => e.ChildSmidget).ToList();
                var dependent = smidgets.Single(e => e.Id == 78);
                var principal = smidgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                            Assert.Same(principal, dependent.ParentSmidget);
                            Assert.Same(dependent, principal.ChildSmidget);
                        });
            }
        }

        [Fact]
        public void Query_self_ref_one_to_one_principal_nav_only()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var smidgets = context.Set<SmidgetPN>().ToList();
                var dependent = smidgets.Single(e => e.Id == 78);
                var principal = smidgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                            Assert.Same(dependent, principal.ChildSmidget);
                        });
            }
        }

        [Fact]
        public void Query_self_ref_one_to_one_dependent_nav_only()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var smidgets = context.Set<SmidgetDN>().ToList();
                var dependent = smidgets.Single(e => e.Id == 78);
                var principal = smidgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                            Assert.Same(principal, dependent.ParentSmidget);
                        });
            }
        }

        [Fact]
        public void Query_dependent_include_principal_self_ref_one_to_one_unidirectional()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var smidgets = context.Set<SmidgetDN>().Include(e => e.ParentSmidget).ToList();
                var dependent = smidgets.Single(e => e.Id == 78);
                var principal = smidgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                            Assert.Same(principal, dependent.ParentSmidget);
                        });
            }
        }

        [Fact]
        public void Query_principal_include_dependent_self_ref_one_to_one_unidirectional()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var smidgets = context.Set<SmidgetPN>().Include(e => e.ChildSmidget).ToList();
                var dependent = smidgets.Single(e => e.Id == 78);
                var principal = smidgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentSmidgetId);
                            Assert.Same(dependent, principal.ChildSmidget);
                        });
            }
        }

        [Fact]
        public void Query_dependent_include_principal_multiple_relationsships()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var dependent = context.Set<Post>().Include(e => e.Blog).Single();
                var principal = dependent.Blog;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.BlogId);
                            Assert.Same(principal, dependent.Blog);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Posts);

                            Assert.Equal(dependent.Id, principal.TopPostId);
                            Assert.Same(dependent, principal.TopPost);
                        });
            }
        }

        [Fact]
        public void Query_principal_include_dependent_multiple_relationsships()
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var principal = context.Set<Blog>().Include(e => e.Posts).Single();
                var dependent = principal.Posts.Single();

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.BlogId);
                            Assert.Same(principal, dependent.Blog);
                            Assert.Equal(new[] { dependent }.ToList(), principal.Posts);

                            Assert.Equal(dependent.Id, principal.TopPostId);
                            Assert.Same(dependent, principal.TopPost);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_dependent_include_principal_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new Product { CategoryId = 77 };
                context.Entry(newDependent).State = existingState;

                var dependent = context.Set<Product>().Include(e => e.Category).Single();
                var principal = dependent.Category;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Contains(dependent, principal.Products);

                            Assert.Equal(principal.Id, newDependent.CategoryId);
                            Assert.Same(principal, newDependent.Category);
                            Assert.Contains(newDependent, principal.Products);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_principal_include_dependent_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new Product { CategoryId = 77 };
                context.Entry(newDependent).State = existingState;

                var principal = context.Set<Category>().Include(e => e.Products).Single();
                var dependent = principal.Products.Single(e => e.Id != newDependent.Id);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);
                            Assert.Contains(dependent, principal.Products);

                            Assert.Equal(principal.Id, newDependent.CategoryId);
                            Assert.Same(principal, newDependent.Category);
                            Assert.Contains(newDependent, principal.Products);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_dependent_include_principal_unidirectional_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new ProductDN { CategoryId = 77 };
                context.Entry(newDependent).State = existingState;

                var dependent = context.Set<ProductDN>().Include(e => e.Category).Single();
                var principal = dependent.Category;

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Same(principal, dependent.Category);

                            Assert.Equal(principal.Id, newDependent.CategoryId);
                            Assert.Same(principal, newDependent.Category);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_principal_include_dependent_unidirectional_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new ProductPN { CategoryId = 77 };
                context.Entry(newDependent).State = existingState;

                var principal = context.Set<CategoryPN>().Include(e => e.Products).Single();
                var dependent = principal.Products.Single(e => e.Id != newDependent.Id);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.CategoryId);
                            Assert.Contains(dependent, principal.Products);

                            Assert.Equal(principal.Id, newDependent.CategoryId);
                            Assert.Contains(newDependent, principal.Products);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_self_ref_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new Widget { ParentWidgetId = 77 };
                context.Entry(newDependent).State = existingState;

                var widgets = context.Set<Widget>().ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Same(principal, dependent.ParentWidget);
                            Assert.Contains(dependent, principal.ChildWidgets);

                            Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                            Assert.Same(principal, newDependent.ParentWidget);
                            Assert.Contains(newDependent, principal.ChildWidgets);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_dependent_include_principal_self_ref_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new Widget { ParentWidgetId = 77 };
                context.Entry(newDependent).State = existingState;

                var widgets = context.Set<Widget>().Include(e => e.ParentWidget).ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Same(principal, dependent.ParentWidget);
                            Assert.Contains(dependent, principal.ChildWidgets);

                            Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                            Assert.Same(principal, newDependent.ParentWidget);
                            Assert.Contains(newDependent, principal.ChildWidgets);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_principal_include_dependent_self_ref_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new Widget { ParentWidgetId = 77 };
                context.Entry(newDependent).State = existingState;

                var widgets = context.Set<Widget>().Include(e => e.ChildWidgets).ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Same(principal, dependent.ParentWidget);
                            Assert.Contains(dependent, principal.ChildWidgets);

                            Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                            Assert.Same(principal, newDependent.ParentWidget);
                            Assert.Contains(newDependent, principal.ChildWidgets);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_self_ref_prinipal_nav_only_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new WidgetPN { ParentWidgetId = 77 };
                context.Entry(newDependent).State = existingState;

                var widgets = context.Set<WidgetPN>().ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Contains(dependent, principal.ChildWidgets);

                            Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                            Assert.Contains(newDependent, principal.ChildWidgets);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_self_ref_dependent_nav_only_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new WidgetDN { ParentWidgetId = 77 };
                context.Entry(newDependent).State = existingState;

                var widgets = context.Set<WidgetDN>().ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Same(principal, dependent.ParentWidget);

                            Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                            Assert.Same(principal, newDependent.ParentWidget);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_dependent_include_principal_self_ref_unidirectional_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new WidgetDN { ParentWidgetId = 77 };
                context.Entry(newDependent).State = existingState;

                var widgets = context.Set<WidgetDN>().Include(e => e.ParentWidget).ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Same(principal, dependent.ParentWidget);

                            Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                            Assert.Same(principal, newDependent.ParentWidget);
                        });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Query_principal_include_dependent_self_ref_unidirectional_with_existing(EntityState existingState)
        {
            Seed();

            using (var context = new QueryFixupContext())
            {
                var newDependent = new WidgetPN { ParentWidgetId = 77 };
                context.Entry(newDependent).State = existingState;

                var widgets = context.Set<WidgetPN>().Include(e => e.ChildWidgets).ToList();
                var dependent = widgets.Single(e => e.Id == 78);
                var principal = widgets.Single(e => e.Id == 77);

                AssertFixup(
                    context,
                    () =>
                        {
                            Assert.Equal(principal.Id, dependent.ParentWidgetId);
                            Assert.Contains(dependent, principal.ChildWidgets);

                            Assert.Equal(principal.Id, newDependent.ParentWidgetId);
                            Assert.Contains(newDependent, principal.ChildWidgets);
                        });
            }
        }

        private static void Seed()
        {
            using (var context = new QueryFixupContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.AddRange(
                    new Blog { Id = 77, TopPostId = 78 },
                    new Post { Id = 78, BlogId = 77 },
                    new Widget { Id = 77 },
                    new Widget { Id = 78, ParentWidgetId = 77 },
                    new WidgetPN { Id = 77 },
                    new WidgetPN { Id = 78, ParentWidgetId = 77 },
                    new WidgetDN { Id = 77 },
                    new WidgetDN { Id = 78, ParentWidgetId = 77 },
                    new Smidget { Id = 77 },
                    new Smidget { Id = 78, ParentSmidgetId = 77 },
                    new SmidgetPN { Id = 77 },
                    new SmidgetPN { Id = 78, ParentSmidgetId = 77 },
                    new SmidgetDN { Id = 77 },
                    new SmidgetDN { Id = 78, ParentSmidgetId = 77 },
                    new Category { Id = 77 },
                    new Product { Id = 78, CategoryId = 77 },
                    new CategoryPN { Id = 77 },
                    new ProductPN { Id = 78, CategoryId = 77 },
                    new CategoryDN { Id = 77 },
                    new ProductDN { Id = 78, CategoryId = 77 },
                    new Parent { Id = 77 },
                    new Child { Id = 78, ParentId = 77 },
                    new ParentPN { Id = 77 },
                    new ChildPN { Id = 78, ParentId = 77 },
                    new ParentDN { Id = 77 },
                    new ChildDN { Id = 78, ParentId = 77 });

                context.SaveChanges();
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
            public int Id { get; set; }

            public ICollection<ProductPN> Products { get; } = new List<ProductPN>();
        }

        private class ProductPN
        {
            public int Id { get; set; }
            public int CategoryId { get; set; }
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
        }

        private class Blog
        {
            public int Id { get; set; }

            public ICollection<Post> Posts { get; } = new List<Post>();

            public int TopPostId { get; set; }
            public Post TopPost { get; set; }
        }

        private class Post
        {
            public int Id { get; set; }
            public int BlogId { get; set; }

            public Blog Blog { get; set; }
        }

        public class Widget
        {
            public int Id { get; set; }

            public int? ParentWidgetId { get; set; }
            public Widget ParentWidget { get; set; }

            public List<Widget> ChildWidgets { get; set; }
        }

        public class WidgetPN
        {
            public int Id { get; set; }

            public int? ParentWidgetId { get; set; }

            public List<WidgetPN> ChildWidgets { get; set; }
        }

        public class WidgetDN
        {
            public int Id { get; set; }

            public int? ParentWidgetId { get; set; }
            public WidgetDN ParentWidget { get; set; }
        }

        public class Smidget
        {
            public int Id { get; set; }

            public int? ParentSmidgetId { get; set; }
            public Smidget ParentSmidget { get; set; }
            public Smidget ChildSmidget { get; set; }
        }

        public class SmidgetPN
        {
            public int Id { get; set; }

            public int? ParentSmidgetId { get; set; }
            public SmidgetPN ChildSmidget { get; set; }
        }

        public class SmidgetDN
        {
            public int Id { get; set; }

            public int? ParentSmidgetId { get; set; }
            public SmidgetDN ParentSmidget { get; set; }
        }

        private class QueryFixupContext : DbContext
        {
            public QueryFixupContext()
            {
                ChangeTracker.AutoDetectChangesEnabled = false;
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Widget>()
                    .HasMany(e => e.ChildWidgets)
                    .WithOne(e => e.ParentWidget)
                    .HasForeignKey(e => e.ParentWidgetId);

                modelBuilder.Entity<WidgetPN>()
                    .HasMany(e => e.ChildWidgets)
                    .WithOne()
                    .HasForeignKey(e => e.ParentWidgetId);

                modelBuilder.Entity<WidgetDN>()
                    .HasOne(e => e.ParentWidget)
                    .WithMany()
                    .HasForeignKey(e => e.ParentWidgetId);

                modelBuilder.Entity<Smidget>()
                    .HasOne(e => e.ParentSmidget)
                    .WithOne(e => e.ChildSmidget)
                    .HasForeignKey<Smidget>(e => e.ParentSmidgetId);

                modelBuilder.Entity<SmidgetPN>()
                    .HasOne<SmidgetPN>()
                    .WithOne(e => e.ChildSmidget)
                    .HasForeignKey<SmidgetPN>(e => e.ParentSmidgetId);

                modelBuilder.Entity<SmidgetDN>()
                    .HasOne(e => e.ParentSmidget)
                    .WithOne()
                    .HasForeignKey<SmidgetDN>(e => e.ParentSmidgetId);

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

                modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasForeignKey(e => e.BlogId);

                modelBuilder.Entity<Blog>()
                    .HasOne(e => e.TopPost)
                    .WithOne()
                    .HasForeignKey<Blog>(e => e.TopPostId);
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase("QueryFixup");
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
    }
}

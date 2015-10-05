// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class ManyToOneTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Finds_existing_navigations_and_uses_associated_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>()
                    .HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasForeignKey(e => e.CustomerId);
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var navToPrincipal = dependentType.GetOrAddNavigation("Customer", fk, pointsToPrincipal: true);
                var navToDependent = principalType.GetOrAddNavigation("Orders", fk, pointsToPrincipal: false);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Same(navToPrincipal, dependentType.Navigations.Single());
                Assert.Same(navToDependent, principalType.Navigations.Single());
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Finds_existing_navigation_to_principal_and_uses_associated_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder
                    .Entity<Order>().HasOne(o => o.Customer).WithMany()
                    .HasForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

                var newFk = dependentType.Navigations.Single().ForeignKey;
                Assert.Equal(nameof(Order.CustomerId), newFk.Properties.Single().Name);
                Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Finds_existing_navigation_to_dependent_and_uses_associated_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>()
                    .HasOne<Customer>().WithMany(e => e.Orders)
                    .HasForeignKey(e => e.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();
                var navigation = fk.PrincipalToDependent;

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

                var newFk = dependentType.GetForeignKeys().Single();
                AssertEqual(fk.Properties, newFk.Properties);
                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal(navigation.Name, principalType.Navigations.Single().Name);
                Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_does_not_use_existing_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>().HasOne<Customer>().WithMany().HasForeignKey(e => e.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

                var newFk = dependentType.Navigations.Single().ForeignKey;
                Assert.Equal(nameof(Order.Customer), newFk.DependentToPrincipal.Name);
                Assert.Equal(nameof(Customer.Orders), newFk.PrincipalToDependent.Name);

                Assert.NotNull(dependentType.GetForeignKeys().Single(foreignKey => foreignKey != newFk));
                Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_creates_new_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty("CustomerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_relationship_with_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty(nameof(Order.CustomerId));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany();

                var fk = dependentType.Navigations.Single().ForeignKey;
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal(nameof(Order.Customer), fk.DependentToPrincipal.Name);
                Assert.Null(fk.PrincipalToDependent);
                Assert.NotSame(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_relationship_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty("CustomerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne<Customer>().WithMany(e => e.Orders);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Empty(dependentType.Navigations);
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_relationship_with_no_navigations()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty("CustomerId");
                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne<Customer>().WithMany();

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != fk));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != fk));
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, newFk.Properties.Single().Name, dependentKey.Properties.Single().Name },
                    dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_uses_specified_FK_even_if_found_by_convention()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty("CustomerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasForeignKey(e => e.CustomerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_with_existing_FK_not_found_by_convention()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                var property = dependentType.GetOrAddProperty(Ingredient.BurgerIdProperty);
                var fk = dependentType.AddForeignKey(property, principalKey, principalType);
                fk.IsUnique = false;

                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                    .HasForeignKey(e => e.BurgerId);

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_creates_FK_specified()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetProperty("BurgerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                    .HasForeignKey(e => e.BurgerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_specified_FK_with_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany()
                    .HasForeignKey(e => e.BurgerId);

                var fk = dependentType.Navigations.Single().ForeignKey;
                Assert.Same(dependentType.GetProperty(nameof(Pickle.BurgerId)), fk.Properties.Single());

                Assert.Equal(nameof(Pickle.BigMak), fk.DependentToPrincipal.Name);
                Assert.Null(fk.PrincipalToDependent);
                Assert.NotSame(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_specified_FK_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetProperty("BurgerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Pickle>().HasOne<BigMak>().WithMany(e => e.Pickles)
                    .HasForeignKey(e => e.BurgerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Empty(dependentType.Navigations);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_relationship_with_no_navigations_and_specified_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetProperty("BurgerId");
                var fk = dependentType.GetForeignKeys().SingleOrDefault();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Pickle>().HasOne<BigMak>().WithMany()
                    .HasForeignKey(e => e.BurgerId);

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);
                Assert.Same(fkProperty, newFk.Properties.Single());

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != fk));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != fk));
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_creates_shadow_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles);

                var fk = dependentType.GetForeignKeys().Single();
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.Equal("BigMakId", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_shadow_FK_with_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Pickle>().HasOne(e => e.BigMak).WithMany();

                var fk = dependentType.Navigations.Single().ForeignKey;
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.Equal("BigMakId", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Equal(nameof(Pickle.BigMak), dependentType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.NotSame(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_shadow_FK_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Pickle>().HasOne<BigMak>().WithMany(e => e.Pickles);

                var fk = dependentType.GetForeignKeys().Single();
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.Equal("BigMakId", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Empty(dependentType.Navigations);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_shadow_FK_with_no_navigations_with()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                var fk = dependentType.GetForeignKeys().SingleOrDefault();

                modelBuilder.Entity<Pickle>().HasOne<BigMak>().WithMany();

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);
                var fkProperty = (IProperty)newFk.Properties.Single();

                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != fk));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != fk));
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_matches_shadow_FK_by_convention()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>().Property<int>("BigMakId");
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                var fkProperty = dependentType.GetProperty("BigMakId");

                modelBuilder.Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_overrides_existing_FK_if_uniqueness_does_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithOne()
                    .HasForeignKey<Pickle>(c => c.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                    .HasForeignKey(e => e.BurgerId);
                
                Assert.Equal(1, dependentType.GetForeignKeys().Count());
                var newFk = (IForeignKey)dependentType.GetForeignKeys().Single();

                Assert.False(newFk.IsUnique);
                Assert.Equal(nameof(Pickle.BigMak), dependentType.Navigations.Single().Name);
                Assert.Equal(nameof(BigMak.Pickles), principalType.Navigations.Single().Name);
                Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_use_explicitly_specified_PK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty("CustomerId");
                var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasPrincipalKey(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_use_non_PK_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var principalProperty = principalType.GetProperty("AlternateKey");

                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasPrincipalKey(e => e.AlternateKey);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);

                Assert.Empty(principalType.GetForeignKeys());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Equal(1, dependentType.GetKeys().Count());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());

                expectedPrincipalProperties.Add(fk.PrincipalKey.Properties.Single());
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.Properties);
            }

            [Fact]
            public virtual void Can_have_both_convention_properties_specified()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty("CustomerId");
                var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasForeignKey(e => e.CustomerId)
                    .HasPrincipalKey(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_have_both_convention_properties_specified_in_any_order()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty("CustomerId");
                var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasPrincipalKey(e => e.Id)
                    .HasForeignKey(e => e.CustomerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_have_FK_by_convention_specified_with_explicit_principal_key()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasForeignKey(e => e.AnotherCustomerId)
                    .HasPrincipalKey(e => e.AlternateKey);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Equal("AnotherCustomerId", fk.Properties.Single().Name);
                Assert.Equal("AlternateKey", fk.PrincipalKey.Properties.Single().Name);

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Equal(1, dependentType.GetKeys().Count());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());

                expectedPrincipalProperties.Add(fk.PrincipalKey.Properties.Single());
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.Properties);
            }

            [Fact]
            public virtual void Can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasPrincipalKey(e => e.AlternateKey)
                    .HasForeignKey(e => e.AnotherCustomerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Equal("AnotherCustomerId", fk.Properties.Single().Name);
                Assert.Equal("AlternateKey", fk.PrincipalKey.Properties.Single().Name);

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Equal(1, dependentType.GetKeys().Count());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());

                expectedPrincipalProperties.Add(fk.PrincipalKey.Properties.Single());
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.Properties);
            }

            [Fact]
            public virtual void Can_have_principal_key_by_convention_specified_with_explicit_PK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetProperty("BurgerId");
                var principalProperty = principalType.GetProperty("AlternateKey");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                    .HasForeignKey(e => e.BurgerId)
                    .HasPrincipalKey(e => e.AlternateKey);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetProperty("BurgerId");
                var principalProperty = principalType.GetProperty("AlternateKey");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                    .HasPrincipalKey(e => e.AlternateKey)
                    .HasForeignKey(e => e.BurgerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_finds_existing_composite_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany()
                    .HasForeignKey(c => new { c.BurgerId1, c.BurgerId2 });
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(Tomato));
                var principalType = model.GetEntityType(typeof(Whoopper));
                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.First().Name == "BurgerId1");
                dependentType.RemoveNavigation(fk.DependentToPrincipal);

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_creates_composite_FK_specified()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(Tomato));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetPrimaryKey();

                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_use_alternate_composite_key()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>(b => b.HasKey(c => new { c.Id1, c.Id2 }));
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(Tomato));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");
                var principalProperty1 = principalType.GetProperty("AlternateKey1");
                var principalProperty2 = principalType.GetProperty("AlternateKey2");

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                    .HasPrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);
                Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
                Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_use_alternate_composite_key_in_any_order()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>(b => b.HasKey(c => new { c.Id1, c.Id2 }));
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(Tomato));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");
                var principalProperty1 = principalType.GetProperty("AlternateKey1");
                var principalProperty2 = principalType.GetProperty("AlternateKey2");

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetPrimaryKey();

                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                    .HasPrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);
                Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
                Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_specified_composite_FK_with_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(Tomato));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty(nameof(Tomato.BurgerId1));
                var fkProperty2 = dependentType.GetProperty(nameof(Tomato.BurgerId2));

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany()
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.Navigations.Single().ForeignKey;
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal(nameof(Tomato.Whoopper), fk.DependentToPrincipal.Name);
                Assert.Null(fk.PrincipalToDependent);
                Assert.NotSame(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_specified_composite_FK_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(Tomato));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Tomato>().HasOne<Whoopper>().WithMany(e => e.Tomatoes)
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Empty(dependentType.Navigations);
                Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_relationship_with_no_navigations_and_specified_composite_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Whoopper>().HasMany(w => w.Tomatoes).WithOne(t => t.Whoopper);
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(Tomato));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var navigationForeignKey = dependentType.GetForeignKeys().SingleOrDefault();

                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Tomato>().HasOne<Whoopper>().WithMany()
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");
                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != navigationForeignKey);
                Assert.Same(fkProperty1, newFk.Properties[0]);
                Assert.Same(fkProperty2, newFk.Properties[1]);

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != navigationForeignKey));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != navigationForeignKey));
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                AssertEqual(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Equal(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Finds_and_removes_existing_one_to_one_relationship()
            {
                var modelBuilder = HobNobBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob);

                var dependentType = model.GetEntityType(typeof(Hob));
                var principalType = model.GetEntityType(typeof(Nob));
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.False(fk.IsUnique);
                Assert.Equal(1, dependentType.Navigations.Count());
                Assert.Equal(1, principalType.Navigations.Count());
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                AssertEqual(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_add_annotations()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));

                var builder = modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);
                builder = builder.HasAnnotation("Fus", "Ro");

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fk, builder.Metadata);
                Assert.Equal("Ro", fk["Fus"]);
            }

            [Fact]
            public virtual void Nullable_FK_are_optional_by_default()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                    .HasForeignKey(e => new { e.HobId1, e.HobId2 });

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
                var fk = entityType.GetForeignKeys().Single();
                Assert.False(fk.IsRequired);
                var fkProperty1 = entityType.GetProperty(nameof(Nob.HobId1));
                var fkProperty2 = entityType.GetProperty(nameof(Nob.HobId2));
                Assert.True(fkProperty1.IsNullable);
                Assert.True(fkProperty2.IsNullable);
                Assert.Contains(fkProperty1, fk.Properties);
                Assert.Contains(fkProperty2, fk.Properties);
            }

            [Fact]
            public virtual void Non_nullable_FK_are_required_by_default()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs)
                    .HasForeignKey(e => new { e.NobId1, e.NobId2 });

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
                var fk = entityType.GetForeignKeys().Single();
                Assert.True(fk.IsRequired);
                var fkProperty1 = entityType.GetProperty(nameof(Hob.NobId1));
                var fkProperty2 = entityType.GetProperty(nameof(Hob.NobId2));
                Assert.False(fkProperty1.IsNullable);
                Assert.False(fkProperty2.IsNullable);
                Assert.Contains(fkProperty1, fk.Properties);
                Assert.Contains(fkProperty2, fk.Properties);
            }

            [Fact]
            public virtual void Nullable_FK_can_be_made_required()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                    .HasForeignKey(e => new { e.HobId1, e.HobId2 })
                    .IsRequired();

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
                var fk = entityType.GetForeignKeys().Single();
                Assert.True(fk.IsRequired);
                var fkProperty1 = entityType.GetProperty(nameof(Nob.HobId1));
                var fkProperty2 = entityType.GetProperty(nameof(Nob.HobId2));
                Assert.False(fkProperty1.IsNullable);
                Assert.False(fkProperty2.IsNullable);
                Assert.Contains(fkProperty1, fk.Properties);
                Assert.Contains(fkProperty2, fk.Properties);
            }

            [Fact]
            public virtual void Non_nullable_FK_can_be_made_optional()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs)
                    .HasForeignKey(e => new { e.NobId1, e.NobId2 })
                    .IsRequired(false);

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
                var fk = entityType.GetForeignKeys().Single();
                Assert.False(fk.IsRequired);
                var fkProperty1 = entityType.GetProperty(nameof(Hob.NobId1));
                var fkProperty2 = entityType.GetProperty(nameof(Hob.NobId2));
                Assert.False(fkProperty1.IsNullable);
                Assert.False(fkProperty2.IsNullable);
                Assert.DoesNotContain(fkProperty1, fk.Properties);
                Assert.DoesNotContain(fkProperty2, fk.Properties);
            }

            [Fact]
            public virtual void Can_turn_cascade_delete_on_and_off()
            {
                var modelBuilder = HobNobBuilder();
                var dependentType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

                modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                    .WillCascadeOnDelete();

                Assert.Equal(DeleteBehavior.Cascade, dependentType.GetForeignKeys().Single().DeleteBehavior);

                modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                    .WillCascadeOnDelete(false);

                Assert.Equal(DeleteBehavior.None, dependentType.GetForeignKeys().Single().DeleteBehavior);
            }

            [Fact]
            public virtual void Can_set_foreign_key_property_when_matching_property_added()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<PrincipalEntity>();

                var foreignKey = model.GetEntityType(typeof(DependentEntity)).GetForeignKeys().Single();
                Assert.Equal("NavId", foreignKey.Properties.Single().Name);

                modelBuilder.Entity<DependentEntity>().Property(et => et.PrincipalEntityId);

                var newForeignKey = model.GetEntityType(typeof(DependentEntity)).GetForeignKeys().Single();
                Assert.Equal("PrincipalEntityId", newForeignKey.Properties.Single().Name);
            }
        }
    }
}

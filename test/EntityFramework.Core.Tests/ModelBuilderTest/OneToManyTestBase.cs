// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class OneToManyTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Finds_existing_navigations_and_uses_associated_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>()
                    .HasOne(o => o.Customer).WithMany(c => c.Orders)
                    .HasForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var navToPrincipal = dependentType.GetNavigation("Customer");
                var navToDependent = principalType.GetNavigation("Orders");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal(navToPrincipal.Name, dependentType.Navigations.Single().Name);
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
                    .Entity<Order>().HasOne(c => c.Customer).WithMany()
                    .HasForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
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
                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne()
                    .HasForeignKey(e => e.CustomerId);
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();
                var navigation = fk.PrincipalToDependent;

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

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
            public virtual void Creates_both_navigations_and_uses_existing_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
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
            public virtual void Creates_relationship_with_both_navigations()
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

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

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

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne();

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
                
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                
                modelBuilder.Entity<Customer>().HasMany<Order>().WithOne(e => e.Customer);

                var fk = dependentType.Navigations.Single().ForeignKey;
                Assert.Same(dependentType.GetProperty(nameof(Order.CustomerId)), fk.Properties.Single());
                Assert.Equal(nameof(Order.Customer), fk.DependentToPrincipal.Name);
                Assert.Null(fk.PrincipalToDependent);
                Assert.NotSame(fk, principalType.Navigations.Single().ForeignKey);
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

                modelBuilder.Entity<Customer>().HasMany<Order>().WithOne();

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);

                Assert.Equal(fk.PrincipalKey.Properties, newFk.PrincipalKey.Properties);
                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == newFk));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == newFk));
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
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
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
            public virtual void Creates_both_navigations_and_uses_existing_FK_not_found_by_convention()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany()
                    .HasForeignKey(c => c.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));
                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.Single().Name == "BurgerId");
                dependentType.RemoveNavigation(fk.DependentToPrincipal);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
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
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasForeignKey(e => e.BurgerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

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
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne()
                    .HasForeignKey(e => e.BurgerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Empty(dependentType.Navigations);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
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
                    .Entity<BigMak>().HasMany<Pickle>().WithOne(e => e.BigMak)
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
                    .Entity<BigMak>().HasMany<Pickle>().WithOne()
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

                modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak);

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
                AssertEqual(new[] { fk.Properties.Single().Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
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

                modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne();

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
                AssertEqual(new[] { fk.Properties.Single().Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
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

                modelBuilder.Entity<BigMak>().HasMany<Pickle>().WithOne(e => e.BigMak);

                var fk = dependentType.Navigations.Single().ForeignKey;
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.Equal("BigMakId", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Equal(nameof(Pickle.BigMak), fk.DependentToPrincipal.Name);
                Assert.Null(fk.PrincipalToDependent);
                Assert.NotSame(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }
            
            [Fact]
            public virtual void Creates_shadow_FK_with_no_navigation()
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
                var existingFk = dependentType.GetForeignKeys().Single();

                modelBuilder.Entity<BigMak>().HasMany<Pickle>().WithOne();

                var foreignKey = dependentType.GetForeignKeys().Single(fk => fk != existingFk);
                var fkProperty = (IProperty)foreignKey.Properties.Single();

                Assert.Equal("BigMakId1", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey != existingFk));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey != existingFk));
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { existingFk.Properties.Single().Name, fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name },
                    dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_matches_shadow_FK_property_by_convention()
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

                modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name },
                    dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_overrides_existing_FK_when_uniqueness_does_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>().HasOne<Pickle>().WithOne()
                    .HasForeignKey<Pickle>(e => e.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                var fk = dependentType.GetForeignKeys()
                    .Single(foreignKey => foreignKey.Properties.Any(p => p.Name == "BurgerId"));
                Assert.True(((IForeignKey)fk).IsUnique);

                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasForeignKey(e => e.BurgerId);

                Assert.Equal(1, dependentType.GetForeignKeys().Count());
                Assert.False(fk.IsUnique);
                
                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name },
                    dependentType.Properties.Select(p => p.Name));
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
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasPrincipalKey(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

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
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasPrincipalKey(e => e.AlternateKey);

                var fk = dependentType.GetForeignKeys().Single();
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
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
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
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
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
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasPrincipalKey(e => e.Id)
                    .HasForeignKey(e => e.CustomerId);

                var foreignKey = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, foreignKey.Properties.Single());
                Assert.Same(principalProperty, foreignKey.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(foreignKey, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(foreignKey, principalType.Navigations.Single().ForeignKey);
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
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
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
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
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
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
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
                AssertEqual(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
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
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
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
                AssertEqual(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
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
            public virtual void Can_have_principal_key_by_convention_replaced_with_primary_key()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasForeignKey(e => e.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var dependentKey = dependentType.GetKeys().SingleOrDefault();

                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder.Entity<BigMak>().HasKey(e => e.AlternateKey);
                
                var fk = dependentType.GetForeignKeys().Single();
                var principalProperty = principalType.GetProperty("AlternateKey");

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                AssertEqual(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());

                var principalKey = principalType.GetKeys().Single();
                Assert.Same(principalProperty, principalKey.Properties.Single());
                Assert.Same(principalKey, fk.PrincipalKey);

                Assert.Same(dependentKey, dependentType.GetKeys().SingleOrDefault());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            }

            [Fact]
            public virtual void Principal_key_by_convention_is_not_replaced_with_new_incompatible_primary_key()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasForeignKey(e => new { e.BurgerId, e.Id });
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var modelClone = modelBuilder.Model.Clone();
                var nonPrimaryPrincipalKey = modelClone.GetEntityType(typeof(BigMak).FullName)
                    .GetKeys().Single(k => !k.IsPrimaryKey());
                var dependentKey = dependentType.GetKeys().SingleOrDefault();

                var expectedDependentProperties = dependentType.Properties.ToList();
                var expectedPrincipalProperties = principalType.Properties.ToList();

                modelBuilder.Entity<BigMak>().HasKey(e => e.AlternateKey);

                var principalProperty = principalType.GetProperty("AlternateKey");
                var fk = dependentType.GetForeignKeys().Single();
                Assert.Equal(2, fk.Properties.Count);
                AssertEqual(nonPrimaryPrincipalKey.Properties, fk.PrincipalKey.Properties,
                    new PropertyComparer(compareAnnotations: false));

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(expectedPrincipalProperties.Select(p => p.Name), principalType.Properties.Select(p => p.Name));
                AssertEqual(expectedDependentProperties.Select(p => p.Name), dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());

                var primaryPrincipalKey = principalType.GetPrimaryKey();
                Assert.Same(principalProperty, primaryPrincipalKey.Properties.Single());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.True(principalType.GetKeys().Contains(fk.PrincipalKey));

                Assert.Same(dependentKey, dependentType.GetKeys().SingleOrDefault());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            }

            [Fact]
            public virtual void Explicit_principal_key_is_not_replaced_with_new_primary_key()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasPrincipalKey(e => new { e.Id });
                modelBuilder.Ignore<Bun>();

                var principalType = model.GetEntityType(typeof(BigMak));
                var dependentType = model.GetEntityType(typeof(Pickle));

                var nonPrimaryPrincipalKey = principalType.GetKeys().Single();

                var dependentKey = dependentType.GetKeys().SingleOrDefault();

                var modelClone = model.Clone();
                var expectedPrincipalProperties = modelClone.GetEntityType(typeof(BigMak)).GetProperties().ToList();
                var expectedDependentProperties = modelClone.GetEntityType(typeof(Pickle)).GetProperties().ToList();

                modelBuilder.Entity<BigMak>().HasKey(e => e.AlternateKey);

                var principalProperty = principalType.GetProperty("AlternateKey");
                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(nonPrimaryPrincipalKey, fk.PrincipalKey);

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(expectedPrincipalProperties.Select(p => p.Name), principalType.Properties.Select(p => p.Name));
                AssertEqual(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());

                var primaryPrincipalKey = principalType.GetPrimaryKey();
                Assert.Same(principalProperty, primaryPrincipalKey.Properties.Single());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.True(principalType.GetKeys().Contains(nonPrimaryPrincipalKey));
                var oldKeyProperty = principalType.GetProperty(nameof(BigMak.Id));
                var newKeyProperty = principalType.GetProperty(nameof(BigMak.AlternateKey));
                Assert.True(oldKeyProperty.RequiresValueGenerator);
                Assert.Null(oldKeyProperty.ValueGenerated);
                Assert.True(newKeyProperty.RequiresValueGenerator);
                Assert.Equal(ValueGenerated.OnAdd, newKeyProperty.ValueGenerated);
                Assert.Same(dependentKey, dependentType.GetKeys().SingleOrDefault());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_uses_existing_composite_FK()
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

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Equal(nameof(Tomato.Whoopper), dependentType.Navigations.Single().Name);
                Assert.Equal(nameof(Whoopper.Tomatoes), principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
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
                modelBuilder.Entity<Whoopper>(b => b.HasKey(c => new { c.Id1, c.Id2 }));
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
                    .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
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
                    .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
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
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
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
            public virtual void Creates_specified_composite_FK_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Tomato>(b =>
                    {
                        b.Property(e => e.BurgerId1);
                        b.Property(e => e.BurgerId2);
                    });
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(Tomato));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne()
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
                
                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().HasMany<Tomato>().WithOne(e => e.Whoopper)
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.Navigations.Single().ForeignKey;
                Assert.Same(dependentType.GetProperty(nameof(Tomato.BurgerId1)), fk.Properties[0]);
                Assert.Same(dependentType.GetProperty(nameof(Tomato.BurgerId2)), fk.Properties[1]);

                Assert.Equal(nameof(Tomato.Whoopper), fk.DependentToPrincipal.Name);
                Assert.Null(fk.PrincipalToDependent);
                Assert.NotSame(fk, principalType.Navigations.Single().ForeignKey);
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
                modelBuilder.Ignore<Moostard>();
                modelBuilder.Ignore<ToastedBun>();

                var dependentType = model.GetEntityType(typeof(Tomato));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fk = dependentType.GetForeignKeys().SingleOrDefault();
                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetPrimaryKey();

                modelBuilder
                    .Entity<Whoopper>().HasMany<Tomato>().WithOne()
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);
                Assert.Same(fkProperty1, newFk.Properties[0]);
                Assert.Same(fkProperty2, newFk.Properties[1]);

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == newFk));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == newFk));
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name },
                    principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name, fk.Properties[0].Name, fk.Properties[1].Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
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

                modelBuilder.Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob);

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

                var builder = modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);
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
                    .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
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
                    .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
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
                    .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
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
                    .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
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
                    .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                    .WillCascadeOnDelete();

                Assert.Equal(DeleteBehavior.Cascade, dependentType.GetForeignKeys().Single().DeleteBehavior);

                modelBuilder
                    .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
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

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
        public abstract class OneToManyTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Finds_existing_navigations_and_uses_associated_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>()
                    .Reference(o => o.Customer).InverseCollection(c => c.Orders)
                    .ForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var navToPrincipal = dependentType.GetNavigation("Customer");
                var navToDependent = principalType.GetNavigation("Orders");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer);

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
                    .Entity<Order>().Reference(c => c.Customer).InverseCollection()
                    .ForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var navigation = dependentType.GetNavigation("Customer");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer);

                Assert.Equal(1, dependentType.GetForeignKeys().Count());
                Assert.Equal("Orders", principalType.Navigations.Single().Name);
                Assert.Same(fk.PrincipalKey, principalType.Navigations.Single().ForeignKey.PrincipalKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
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
                modelBuilder.Entity<Order>().Metadata.GetOrAddForeignKey(
                    model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                    model.GetEntityType(typeof(Customer)).GetPrimaryKey());
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var navigation = principalType.GetOrAddNavigation("Orders", fk, pointsToPrincipal: false);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer);

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Same(navigation, principalType.Navigations.Single());
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
            public virtual void Creates_both_navigations_and_uses_existing_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder
                    .Entity<Order>().Reference(e => e.Customer).InverseCollection(e => e.Orders)
                    .ForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer);

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

                modelBuilder.Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer);

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

                modelBuilder.Entity<Customer>().Collection(e => e.Orders).InverseReference();

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

                var fkProperty = dependentType.GetProperty("CustomerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                // Passing null as the first arg is not super-compelling, but it is consistent
                modelBuilder.Entity<Customer>().Collection<Order>().InverseReference(e => e.Customer);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Empty(principalType.Navigations);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
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

                modelBuilder.Entity<Customer>().Collection<Order>().InverseReference();

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
                    .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                    .ForeignKey(e => e.CustomerId);

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
                    .Entity<Pickle>().Reference(e => e.BigMak).InverseCollection()
                    .ForeignKey(c => c.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));
                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.Single().Name == "BurgerId");
                dependentType.RemoveNavigation(fk.DependentToPrincipal);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().Collection(e => e.Pickles).InverseReference(e => e.BigMak)
                    .ForeignKey(e => e.BurgerId);

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
                    .Entity<BigMak>().Collection(e => e.Pickles).InverseReference(e => e.BigMak)
                    .ForeignKey(e => e.BurgerId);

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
                    .Entity<BigMak>().Collection(e => e.Pickles).InverseReference()
                    .ForeignKey(e => e.BurgerId);

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

                var fkProperty = dependentType.GetProperty("BurgerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().Collection<Pickle>().InverseReference(e => e.BigMak)
                    .ForeignKey(e => e.BurgerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Empty(principalType.Navigations);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
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
                    .Entity<BigMak>().Collection<Pickle>().InverseReference()
                    .ForeignKey(e => e.BurgerId);

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

                modelBuilder.Entity<BigMak>().Collection(e => e.Pickles).InverseReference(e => e.BigMak);

                var fk = dependentType.GetForeignKeys().Single();
                var fkProperty = fk.Properties.Single();

                Assert.Equal("BigMakId", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.EntityType);

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

                modelBuilder.Entity<BigMak>().Collection(e => e.Pickles).InverseReference();

                var fk = dependentType.GetForeignKeys().Single();
                var fkProperty = fk.Properties.Single();

                Assert.Equal("BigMakId", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.EntityType);

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

                modelBuilder.Entity<BigMak>().Collection<Pickle>().InverseReference(e => e.BigMak);

                var fk = dependentType.GetForeignKeys().Single();
                var fkProperty = fk.Properties.Single();

                Assert.Equal("BigMakId", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.EntityType);

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Empty(principalType.Navigations);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
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

                modelBuilder.Entity<BigMak>().Collection<Pickle>().InverseReference();

                var foreignKey = dependentType.GetForeignKeys().Single(fk => fk != existingFk);
                var fkProperty = foreignKey.Properties.Single();

                Assert.Equal("BigMakId1", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.EntityType);

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

                modelBuilder.Entity<BigMak>().Collection(e => e.Pickles).InverseReference(e => e.BigMak);

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
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                var fk = dependentType.AddForeignKey(dependentType.GetOrAddProperty("BurgerId", typeof(int)), principalKey);
                fk.IsUnique = true;

                modelBuilder
                    .Entity<BigMak>().Collection(e => e.Pickles).InverseReference(e => e.BigMak)
                    .ForeignKey(e => e.BurgerId);

                Assert.Equal(1, dependentType.GetForeignKeys().Count());
                var newFk = (IForeignKey)dependentType.GetForeignKeys().Single(k => k != fk);

                Assert.False(newFk.IsUnique);

                Assert.NotSame(fk, newFk);
                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { newFk.Properties.Single().Name, dependentKey.Properties.Single().Name },
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
                    .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                    .PrincipalKey(e => e.Id);

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
                    .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                    .PrincipalKey(e => e.AlternateKey);

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
                    .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                    .ForeignKey(e => e.CustomerId)
                    .PrincipalKey(e => e.Id);

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
                    .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                    .PrincipalKey(e => e.Id)
                    .ForeignKey(e => e.CustomerId);

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
                    .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                    .ForeignKey(e => e.AnotherCustomerId)
                    .PrincipalKey(e => e.AlternateKey);

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
                    .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                    .PrincipalKey(e => e.AlternateKey)
                    .ForeignKey(e => e.AnotherCustomerId);

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
                    .Entity<BigMak>().Collection(e => e.Pickles).InverseReference(e => e.BigMak)
                    .ForeignKey(e => e.BurgerId)
                    .PrincipalKey(e => e.AlternateKey);

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
                    .Entity<BigMak>().Collection(e => e.Pickles).InverseReference(e => e.BigMak)
                    .PrincipalKey(e => e.AlternateKey)
                    .ForeignKey(e => e.BurgerId);

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
                    .Entity<BigMak>().Collection(e => e.Pickles).InverseReference(e => e.BigMak)
                    .ForeignKey(e => e.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetOrAddProperty("BurgerId", typeof(int));
                var principalProperty = principalType.GetOrAddProperty("AlternateKey", typeof(int));

                var dependentKey = dependentType.GetKeys().SingleOrDefault();

                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder.Entity<BigMak>().Key(e => e.AlternateKey);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

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
                    .Entity<BigMak>().Collection(e => e.Pickles).InverseReference(e => e.BigMak)
                    .ForeignKey(e => new { e.BurgerId, e.Id });
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var nonPrimaryPrincipalKey = principalType.GetKeys().Single(k => !k.IsPrimaryKey());
                var principalProperty = principalType.GetOrAddProperty("AlternateKey", typeof(int));

                var dependentKey = dependentType.GetKeys().SingleOrDefault();

                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder.Entity<BigMak>().Key(e => e.AlternateKey);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Equal(2, fk.Properties.Count);
                Assert.Same(nonPrimaryPrincipalKey, fk.PrincipalKey);

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                AssertEqual(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());

                var primaryPrincipalKey = principalType.GetPrimaryKey();
                Assert.Same(principalProperty, primaryPrincipalKey.Properties.Single());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.True(principalType.GetKeys().Contains(nonPrimaryPrincipalKey));

                Assert.Same(dependentKey, dependentType.GetKeys().SingleOrDefault());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            }

            [Fact]
            public virtual void Explicit_principal_key_is_not_replaced_with_new_primary_key()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder
                    .Entity<BigMak>().Collection(e => e.Pickles).InverseReference(e => e.BigMak)
                    .PrincipalKey(e => new { e.Id });
                modelBuilder.Ignore<Bun>();

                var dependentType = model.GetEntityType(typeof(Pickle));
                var principalType = model.GetEntityType(typeof(BigMak));

                var nonPrimaryPrincipalKey = principalType.GetKeys().Single();
                var principalProperty = principalType.GetOrAddProperty("AlternateKey", typeof(int));

                var dependentKey = dependentType.GetKeys().SingleOrDefault();

                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder.Entity<BigMak>().Key(e => e.AlternateKey);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(nonPrimaryPrincipalKey, fk.PrincipalKey);

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Pickles", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                AssertEqual(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());

                var primaryPrincipalKey = principalType.GetPrimaryKey();
                Assert.Same(principalProperty, primaryPrincipalKey.Properties.Single());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.True(principalType.GetKeys().Contains(nonPrimaryPrincipalKey));
                Assert.Same(dependentKey, dependentType.GetKeys().SingleOrDefault());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_uses_existing_composite_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder
                    .Entity<Tomato>().Reference(e => e.Whoopper).InverseCollection()
                    .ForeignKey(c => new { c.BurgerId1, c.BurgerId2 });
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(Tomato));
                var principalType = model.GetEntityType(typeof(Whoopper));
                var fk = dependentType.GetForeignKeys().Single();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().Collection(e => e.Tomatoes).InverseReference(e => e.Whoopper)
                    .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                Assert.Equal(1, dependentType.GetForeignKeys().Count());
                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
                Assert.Same(fk.PrincipalKey, principalType.Navigations.Single().ForeignKey.PrincipalKey);
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
                modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
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
                    .Entity<Whoopper>().Collection(e => e.Tomatoes).InverseReference(e => e.Whoopper)
                    .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

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
                modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
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
                    .Entity<Whoopper>().Collection(e => e.Tomatoes).InverseReference(e => e.Whoopper)
                    .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                    .PrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 });

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
                modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
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
                    .Entity<Whoopper>().Collection(e => e.Tomatoes).InverseReference(e => e.Whoopper)
                    .PrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                    .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

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
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
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
                    .Entity<Whoopper>().Collection(e => e.Tomatoes).InverseReference()
                    .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

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
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
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
                    .Entity<Whoopper>().Collection<Tomato>().InverseReference(e => e.Whoopper)
                    .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Empty(principalType.Navigations);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
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
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Whoopper>().Collection(w => w.Tomatoes).InverseReference(t => t.Whoopper);
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
                    .Entity<Whoopper>().Collection<Tomato>().InverseReference()
                    .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

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
                modelBuilder.Entity<Nob>().Reference(e => e.Hob).InverseReference(e => e.Nob);

                var dependentType = model.GetEntityType(typeof(Hob));
                var principalType = model.GetEntityType(typeof(Nob));
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Nob>().Collection(e => e.Hobs).InverseReference(e => e.Nob);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.False(fk.IsUnique);
                Assert.Equal(1, dependentType.Navigations.Count);
                Assert.Equal(1, principalType.Navigations.Count);
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

                var builder = modelBuilder.Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer);
                builder = builder.Annotation("Fus", "Ro");

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fk, builder.Metadata);
                Assert.Equal("Ro", fk["Fus"]);
            }

            [Fact]
            public virtual void Nullable_FK_are_optional_by_default()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Hob>().Collection(e => e.Nobs).InverseReference(e => e.Hob)
                    .ForeignKey(e => new { e.HobId1, e.HobId2 });

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

                Assert.True(entityType.GetProperty("HobId1").IsNullable);
                Assert.True(entityType.GetProperty("HobId1").IsNullable);
                Assert.False(entityType.GetForeignKeys().Single().IsRequired);
            }

            [Fact]
            public virtual void Non_nullable_FK_are_required_by_default()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Nob>().Collection(e => e.Hobs).InverseReference(e => e.Nob)
                    .ForeignKey(e => new { e.NobId1, e.NobId2 });

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

                Assert.False(entityType.GetProperty("NobId1").IsNullable);
                Assert.False(entityType.GetProperty("NobId1").IsNullable);
                Assert.True(entityType.GetForeignKeys().Single().IsRequired);
            }

            [Fact]
            public virtual void Nullable_FK_can_be_made_required()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Hob>().Collection(e => e.Nobs).InverseReference(e => e.Hob)
                    .ForeignKey(e => new { e.HobId1, e.HobId2 })
                    .Required();

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

                Assert.False(entityType.GetProperty("HobId1").IsNullable);
                Assert.False(entityType.GetProperty("HobId1").IsNullable);
                Assert.True(entityType.GetForeignKeys().Single().IsRequired);
            }

            [Fact]
            public virtual void Non_nullable_FK_cannot_be_made_optional()
            {
                var modelBuilder = HobNobBuilder();

                Assert.Equal(
                    Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder
                        .Entity<Nob>().Collection(e => e.Hobs).InverseReference(e => e.Nob)
                        .ForeignKey(e => new { e.NobId1, e.NobId2 })
                        .Required(false)).Message);

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

                Assert.False(entityType.GetProperty("NobId1").IsNullable);
                Assert.False(entityType.GetProperty("NobId1").IsNullable);
                Assert.True(entityType.GetForeignKeys().Single().IsRequired);
            }
        }
    }
}

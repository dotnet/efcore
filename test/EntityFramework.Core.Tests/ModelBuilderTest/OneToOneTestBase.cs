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
        public abstract class OneToOneTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Finds_existing_navigations_and_uses_associated_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder
                    .Entity<CustomerDetails>().Reference(d => d.Customer).InverseReference(c => c.Details)
                    .ForeignKey<CustomerDetails>(c => c.Id);
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var navToPrincipal = dependentType.GetNavigation("Customer");
                var navToDependent = principalType.GetNavigation("Details");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().Reference(e => e.Details).InverseReference(e => e.Customer);

                Assert.Equal(1, dependentType.GetForeignKeys().Count());
                Assert.Same(navToPrincipal, dependentType.Navigations.Single());
                Assert.Same(navToDependent, principalType.Navigations.Single());
                Assert.Same(fk.PrincipalKey, principalType.Navigations.Single().ForeignKey.PrincipalKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Replaces_existing_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<CustomerDetails>().Reference(c => c.Customer).InverseReference();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().Reference(e => e.Details).InverseReference(e => e.Customer);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                AssertEqual(expectedDependentProperties, dependentType.Properties);
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
                modelBuilder.Entity<Customer>().Reference(c => c.Details).InverseReference()
                    .ForeignKey<CustomerDetails>(c => c.Id);
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var navigation = principalType.GetNavigation("Details");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().Reference(e => e.Details).InverseReference(e => e.Customer);

                Assert.Equal(1, dependentType.GetForeignKeys().Count());
                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Same(fk.PrincipalKey, principalType.Navigations.Single().ForeignKey.PrincipalKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_shadow_FK_if_existing_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>()
                    .Reference<CustomerDetails>()
                    .InverseReference()
                    .ForeignKey<CustomerDetails>(e => e.Id);
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder.Entity<CustomerDetails>().Reference(e => e.Customer).InverseReference(e => e.Details)
                    .PrincipalKey<Customer>(e => e.Id);
                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);

                Assert.NotSame(fk.Properties.Single(), newFk.Properties.Single());
                Assert.Same(newFk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(newFk.PrincipalToDependent, principalType.Navigations.Single());
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                expectedDependentProperties.Add(newFk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_new_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Entity<Customer>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder.Entity<CustomerDetails>().Reference(e => e.Customer).InverseReference(e => e.Details);

                var fk = dependentType.GetForeignKeys().Single();

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_removes_existing_FK_when_not_specified()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder
                    .Entity<Order>()
                    .Reference(e => e.Details)
                    .InverseReference()
                    .ForeignKey<OrderDetails>(c => c.Id);
                modelBuilder.Ignore<Customer>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));
                var existingFk = dependentType.GetForeignKeys().Single();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder.Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.NotSame(existingFk, fk);

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_creates_new_FK_when_not_specified()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<Customer>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));

                var fkProperty = dependentType.GetProperty("OrderId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Order", dependentType.Navigations.Single().Name);
                Assert.Equal("Details", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AnotherCustomerId", "CustomerId", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { dependentKey.Properties.Single().Name, fkProperty.Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_relationship_with_navigation_to_dependent_and_new_FK_from_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(Customer));
                var principalType = model.GetEntityType(typeof(CustomerDetails));
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().Reference(e => e.Details).InverseReference();

                var fk = dependentType.Navigations.Single().ForeignKey;
                if (fk.EntityType == dependentType)
                {
                    Assert.Empty(principalType.GetForeignKeys());
                }
                else
                {
                    Assert.Empty(dependentType.GetForeignKeys());
                }

                Assert.Empty(principalType.Navigations);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_relationship_with_navigation_to_dependent_and_new_FK_from_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                foreach (var navigation in principalType.Navigations.ToList())
                {
                    var inverse = navigation.FindInverse();
                    inverse?.EntityType.RemoveNavigation(inverse);
                    principalType.RemoveNavigation(navigation);

                    var foreignKey = navigation.ForeignKey;
                    foreignKey.EntityType.RemoveForeignKey(foreignKey);
                }

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder.Entity<CustomerDetails>().Reference<Customer>().InverseReference(e => e.Details);

                var fk = principalType.Navigations.Single().ForeignKey;

                Assert.Empty(dependentType.Navigations);
                AssertEqual(expectedPrincipalProperties, principalType.Properties);
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.Properties);
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
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder.Entity<CustomerDetails>().Reference<Customer>().InverseReference();

                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.PrincipalToDependent == null);

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == fk));
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.Properties);
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
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Ignore<Customer>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));

                var fkProperty = dependentType.GetProperty("OrderId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().Reference(e => e.Details).InverseReference(e => e.Order)
                    .ForeignKey<OrderDetails>(e => e.OrderId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Order", dependentType.Navigations.Single().Name);
                Assert.Equal("Details", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AnotherCustomerId", "CustomerId", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { dependentKey.Properties.Single().Name, fkProperty.Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_uses_specified_FK_even_if_PK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Customer>().Reference(e => e.Details).InverseReference(e => e.Customer)
                    .ForeignKey<CustomerDetails>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Details", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, "Name" }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
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
                modelBuilder.Entity<Bun>().Metadata.AddForeignKey(
                    model.GetEntityType(typeof(Bun)).GetProperty("BurgerId"),
                    model.GetEntityType(typeof(BigMak)).GetPrimaryKey());
                modelBuilder.Ignore<Pickle>();

                var dependentType = model.GetEntityType(typeof(Bun));
                var principalType = model.GetEntityType(typeof(BigMak));
                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.All(p => p.Name == "BurgerId"));
                fk.IsUnique = true;

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().Reference(e => e.Bun).InverseReference(e => e.BigMak)
                    .ForeignKey<Bun>(e => e.BurgerId);

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Bun", principalType.Navigations.Single().Name);
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
            public virtual void Creates_both_navigations_and_specified_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Bun>();
                modelBuilder.Ignore<Pickle>();

                var dependentType = model.GetEntityType(typeof(Bun));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetProperty("BurgerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().Reference(e => e.Bun).InverseReference(e => e.BigMak)
                    .ForeignKey<Bun>(e => e.BurgerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Bun", principalType.Navigations.Single().Name);
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
            public virtual void Creates_relationship_with_specified_FK_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Bun>();
                modelBuilder.Ignore<Pickle>();

                var dependentType = model.GetEntityType(typeof(Bun));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetProperty("BurgerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().Reference(e => e.Bun).InverseReference()
                    .ForeignKey<Bun>(e => e.BurgerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Empty(dependentType.Navigations);
                Assert.Equal("Bun", principalType.Navigations.Single().Name);
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
            public virtual void Creates_relationship_with_specified_FK_with_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Bun>();
                modelBuilder.Ignore<Pickle>();

                var dependentType = model.GetEntityType(typeof(Bun));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetProperty("BurgerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().Reference<Bun>().InverseReference(e => e.BigMak)
                    .ForeignKey<Bun>(e => e.BurgerId);

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
            public virtual void Creates_relationship_with_specified_FK_with_no_navigations()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Bun>();
                modelBuilder.Ignore<Pickle>();

                var dependentType = model.GetEntityType(typeof(Bun));
                var principalType = model.GetEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<BigMak>().Reference<Bun>().InverseReference()
                    .ForeignKey<Bun>(e => e.BurgerId);

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.All(p => p.Name == "BurgerId"));

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == newFk));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == newFk));
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigationss_and_overrides_existing_FK_when_uniqueness_does_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Bun>().Metadata.AddForeignKey(
                    model.GetEntityType(typeof(Bun)).GetProperty("BurgerId"),
                    model.GetEntityType(typeof(BigMak)).GetPrimaryKey());
                modelBuilder.Ignore<Pickle>();

                var dependentType = (IEntityType)model.GetEntityType(typeof(Bun));
                var principalType = model.GetEntityType(typeof(BigMak));
                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.All(p => p.Name == "BurgerId"));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().Reference(e => e.Bun).InverseReference(e => e.BigMak)
                    .ForeignKey<Bun>(e => e.BurgerId);

                Assert.Equal(1, dependentType.GetForeignKeys().Count());
                var newFk = dependentType.GetForeignKeys().Single(k => k != fk);

                Assert.False(fk.IsUnique);
                Assert.True(newFk.IsUnique);

                Assert.Equal(fk.Properties.ToList(), newFk.Properties.ToList());
                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Bun", principalType.Navigations.Single().Name);
                Assert.Same(newFk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { newFk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Removes_existing_FK_when_specified()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder
                    .Entity<OrderDetails>().Reference<Order>().InverseReference()
                    .ForeignKey<OrderDetails>(c => c.Id);
                modelBuilder.Ignore<Customer>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));
                var existingFk = dependentType.GetForeignKeys().Single(fk => fk.Properties.All(p => p.Name == "Id"));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                    .ForeignKey<OrderDetails>(e => e.Id);

                var newFk = dependentType.GetForeignKeys().Single();
                Assert.Equal(existingFk.Properties, newFk.Properties);
                Assert.Equal(existingFk.PrincipalKey.Properties, newFk.PrincipalKey.Properties);
                Assert.Equal("Order", dependentType.Navigations.Single().Name);
                Assert.Equal("Details", principalType.Navigations.Single().Name);
                Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AnotherCustomerId", "CustomerId", principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { existingFk.Properties.Single().Name, "OrderId" }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_specified_on_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Ignore<Customer>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));

                var fkProperty = dependentType.GetProperty("OrderId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                    .ForeignKey<OrderDetails>(e => e.OrderId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_specified_on_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Ignore<Customer>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(OrderDetails));

                var fkProperty = dependentType.GetProperty("OrderId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                    .ForeignKey<Order>(e => e.OrderId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_specified_on_principal_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(Customer));
                var principalType = model.GetEntityType(typeof(CustomerDetails));

                var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.Where(p => !p.IsShadowProperty).ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<CustomerDetails>().Reference(e => e.Customer).InverseReference()
                    .ForeignKey<Customer>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Empty(dependentType.Navigations);
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_specified_on_dependent_with_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.Where(p => !p.IsShadowProperty).ToList();

                modelBuilder
                    .Entity<CustomerDetails>().Reference(e => e.Customer).InverseReference()
                    .ForeignKey<CustomerDetails>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Empty(principalType.Navigations);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_specified_on_principal_with_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(Customer));
                var principalType = model.GetEntityType(typeof(CustomerDetails));

                var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.Where(p => !p.IsShadowProperty).ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<CustomerDetails>().Reference<Customer>().InverseReference(e => e.Details)
                    .ForeignKey<Customer>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Empty(principalType.Navigations);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_specified_on_dependent_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.Where(p => !p.IsShadowProperty).ToList();

                modelBuilder
                    .Entity<CustomerDetails>().Reference<Customer>().InverseReference(e => e.Details)
                    .ForeignKey<CustomerDetails>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Empty(dependentType.Navigations);
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_specified_on_dependent_with_no_navigations()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                var principalFk = principalType.GetForeignKeys().SingleOrDefault();
                var existingFk = dependentType.GetForeignKeys().SingleOrDefault();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<CustomerDetails>().Reference<Customer>().InverseReference()
                    .ForeignKey<CustomerDetails>(e => e.Id);

                var newForeignKey = dependentType.GetForeignKeys().Single(fk => fk != existingFk);
                Assert.Same(fkProperty, newForeignKey.Properties.Single());

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == newForeignKey));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == newForeignKey));
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Same(principalFk, principalType.GetForeignKeys().SingleOrDefault());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_specified_on_principal_with_no_navigations()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(Customer));
                var principalType = model.GetEntityType(typeof(CustomerDetails));

                var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                var principalFk = principalType.GetForeignKeys().SingleOrDefault();
                var existingFk = dependentType.GetForeignKeys().SingleOrDefault();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<CustomerDetails>().Reference<Customer>().InverseReference()
                    .ForeignKey<Customer>(e => e.Id);

                var newForeignKey = dependentType.GetForeignKeys().Single(fk => fk != existingFk);
                Assert.Same(fkProperty, newForeignKey.Properties.Single());

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == newForeignKey));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == newForeignKey));
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Same(principalFk, principalType.GetForeignKeys().SingleOrDefault());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_use_PK_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<CustomerDetails>().Reference(e => e.Customer).InverseReference(e => e.Details)
                    .ForeignKey<CustomerDetails>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Customer", dependentType.Navigations.Single().Name);
                Assert.Equal("Details", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, "Name" }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void OneToOne_can_have_PK_explicitly_specified()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<Customer>().Reference(e => e.Details).InverseReference(e => e.Customer)
                    .PrincipalKey<Customer>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_use_alternate_principal_key()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));
                var principalProperty = principalType.GetProperty("AlternateKey");
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.Where(p => !p.IsShadowProperty).ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Customer>().Reference(e => e.Details).InverseReference(e => e.Customer)
                    .PrincipalKey<Customer>(e => e.AlternateKey);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
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
            public virtual void Can_have_both_keys_specified_explicitly()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Ignore<Customer>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));

                var fkProperty = dependentType.GetProperty("OrderId");
                var principalProperty = principalType.GetProperty("OrderId");

                var principalPropertyCount = principalType.PropertyCount;
                var dependentPropertyCount = dependentType.PropertyCount;
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().Reference(e => e.Details).InverseReference(e => e.Order)
                    .ForeignKey<OrderDetails>(e => e.OrderId)
                    .PrincipalKey<Order>(e => e.OrderId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Order", dependentType.Navigations.Single().Name);
                Assert.Equal("Details", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(principalPropertyCount, principalType.PropertyCount);
                Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_have_both_keys_specified_explicitly_in_any_order()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Ignore<Customer>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));

                var fkProperty = dependentType.GetProperty("OrderId");
                var principalProperty = principalType.GetProperty("OrderId");

                var principalPropertyCount = principalType.PropertyCount;
                var dependentPropertyCount = dependentType.PropertyCount;
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().Reference(e => e.Details).InverseReference(e => e.Order)
                    .PrincipalKey<Order>(e => e.OrderId)
                    .ForeignKey<OrderDetails>(e => e.OrderId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Order", dependentType.Navigations.Single().Name);
                Assert.Equal("Details", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(principalPropertyCount, principalType.PropertyCount);
                Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Can_have_both_alternate_keys_specified_explicitly()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Bun>();
                modelBuilder.Ignore<Pickle>();

                var dependentType = model.GetEntityType(typeof(Bun));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetProperty("BurgerId");
                var principalProperty = principalType.GetProperty("AlternateKey");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().Reference(e => e.Bun).InverseReference(e => e.BigMak)
                    .ForeignKey<Bun>(e => e.BurgerId)
                    .PrincipalKey<BigMak>(e => e.AlternateKey);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Bun", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { principalProperty.Name, principalKey.Properties.Single().Name }, principalType.Properties.Select(p => p.Name));
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
            public virtual void Can_have_both_alternate_keys_specified_explicitly_in_any_order()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Bun>();
                modelBuilder.Ignore<Pickle>();

                var dependentType = model.GetEntityType(typeof(Bun));
                var principalType = model.GetEntityType(typeof(BigMak));

                var fkProperty = dependentType.GetProperty("BurgerId");
                var principalProperty = principalType.GetProperty("AlternateKey");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().Reference(e => e.Bun).InverseReference(e => e.BigMak)
                    .PrincipalKey<BigMak>(e => e.AlternateKey)
                    .ForeignKey<Bun>(e => e.BurgerId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
                Assert.Equal("Bun", principalType.Navigations.Single().Name);
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
            public virtual void Does_not_use_existing_FK_when_principal_key_specified()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>()
                    .Reference<Order>().InverseReference()
                    .ForeignKey<OrderDetails>(e => e.Id);
                modelBuilder.Ignore<Customer>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));
                var existingFk = dependentType.GetForeignKeys().Single(fk => fk.Properties.All(p => p.Name == "Id"));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                    .PrincipalKey<Order>(e => e.OrderId);

                var newFk = dependentType.GetForeignKeys().Single(fk => fk != existingFk);
                Assert.NotEqual(existingFk.Properties, newFk.Properties);
                Assert.Equal("Order", dependentType.Navigations.Single().Name);
                Assert.Equal("Details", principalType.Navigations.Single().Name);
                Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_principal_key_when_specified_on_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Ignore<Customer>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));

                var keyProperty = principalType.GetProperty("OrderId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                    .PrincipalKey<Order>(e => e.OrderId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(keyProperty, fk.PrincipalKey.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_principal_key_when_specified_on_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Ignore<Customer>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(OrderDetails));

                var keyProperty = principalType.GetProperty("OrderId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                    .PrincipalKey<OrderDetails>(e => e.OrderId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(keyProperty, fk.PrincipalKey.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(fk.PrincipalKey, principalType.GetKeys().Single(k => k != principalKey));
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_principal_and_foreign_key_specified_on_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Ignore<Customer>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));

                var fkProperty = dependentType.GetProperty("OrderId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                    .ForeignKey<OrderDetails>(e => e.OrderId)
                    .PrincipalKey<Order>(e => e.OrderId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_principal_and_foreign_key_specified_on_dependent_in_reverse_order()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Ignore<Customer>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(OrderDetails));
                var principalType = model.GetEntityType(typeof(Order));

                var fkProperty = dependentType.GetProperty("OrderId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                    .PrincipalKey<Order>(e => e.OrderId)
                    .ForeignKey<OrderDetails>(e => e.OrderId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_FK_when_principal_and_foreign_key_specified_on_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>();
                modelBuilder.Ignore<Customer>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.GetEntityType(typeof(Order));
                var principalType = model.GetEntityType(typeof(OrderDetails));

                var fkProperty = dependentType.GetProperty("OrderId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                    .ForeignKey<Order>(e => e.OrderId)
                    .PrincipalKey<OrderDetails>(e => e.OrderId);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(fk.PrincipalKey, principalType.GetKeys().Single(k => k != principalKey));
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Principal_and_dependent_cannot_be_flipped_twice()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>()
                    .Reference(e => e.Order).InverseReference(e => e.Details)
                    .PrincipalKey<OrderDetails>(e => e.Id);
                modelBuilder.Ignore<Customer>();
                modelBuilder.Ignore<CustomerDetails>();

                Assert.Equal(Strings.RelationshipCannotBeInverted,
                    Assert.Throws<InvalidOperationException>(() => modelBuilder
                        .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                        .ForeignKey<OrderDetails>(e => e.OrderId)
                        .PrincipalKey<OrderDetails>(e => e.OrderId)).Message);
            }

            [Fact]
            public virtual void Principal_and_dependent_cannot_be_flipped_twice_in_reverse_order()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Order>();
                modelBuilder.Entity<OrderDetails>()
                    .Reference(e => e.Order).InverseReference(e => e.Details)
                    .PrincipalKey<OrderDetails>(e => e.Id);
                modelBuilder.Ignore<Customer>();
                modelBuilder.Ignore<CustomerDetails>();

                Assert.Equal(Strings.RelationshipCannotBeInverted,
                    Assert.Throws<InvalidOperationException>(() => modelBuilder
                        .Entity<OrderDetails>().Reference(e => e.Order).InverseReference(e => e.Details)
                        .PrincipalKey<OrderDetails>(e => e.OrderId)
                        .ForeignKey<OrderDetails>(e => e.OrderId)).Message);
            }

            [Fact]
            public virtual void Creates_principal_key_when_specified_on_principal_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<Customer>().Reference(e => e.Details).InverseReference()
                    .PrincipalKey<Customer>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());

                Assert.Empty(dependentType.Navigations);
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_principal_key_when_specified_on_dependent_with_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<CustomerDetails>().Reference(e => e.Customer).InverseReference()
                    .PrincipalKey<Customer>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Empty(principalType.Navigations);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_principal_key_when_specified_on_principal_with_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<Customer>().Reference<CustomerDetails>().InverseReference(e => e.Customer)
                    .PrincipalKey<Customer>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());

                Assert.Same(fk.DependentToPrincipal, dependentType.Navigations.Single());
                Assert.Empty(principalType.Navigations);
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_principal_key_when_specified_on_dependent_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<CustomerDetails>().Reference<Customer>().InverseReference(e => e.Details)
                    .PrincipalKey<Customer>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());

                Assert.Empty(dependentType.Navigations);
                Assert.Same(fk.PrincipalToDependent, principalType.Navigations.Single());
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                Assert.Equal(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_principal_key_when_specified_on_principal_with_no_navigations()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var existingFk = dependentType.GetForeignKeys().SingleOrDefault();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<Customer>().Reference<CustomerDetails>().InverseReference()
                    .PrincipalKey<Customer>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
                Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == fk));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == fk));
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_principal_key_when_specified_on_dependent_with_no_navigations()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));
                var principalType = model.GetEntityType(typeof(Customer));

                var existingFk = dependentType.GetForeignKeys().SingleOrDefault();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();

                modelBuilder
                    .Entity<CustomerDetails>().Reference<Customer>().InverseReference()
                    .PrincipalKey<Customer>(e => e.Id);

                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
                Assert.Same(principalKey.Properties.Single(), fk.PrincipalKey.Properties.Single());

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == fk));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == fk));
                Assert.Equal(expectedPrincipalProperties, principalType.Properties);
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.Properties);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_uses_existing_composite_FK()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
                var dependentType = model.GetEntityType(typeof(ToastedBun));
                modelBuilder.Entity<ToastedBun>().Metadata.AddForeignKey(
                    new[] { dependentType.GetProperty("BurgerId1"), dependentType.GetProperty("BurgerId2") },
                    model.GetEntityType(typeof(Whoopper)).GetPrimaryKey());
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Moostard>();

                var principalType = model.GetEntityType(typeof(Whoopper));
                var fk = dependentType.GetForeignKeys().Single();
                fk.IsUnique = true;

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetPrimaryKey();

                modelBuilder
                    .Entity<Whoopper>().Reference(e => e.ToastedBun).InverseReference(e => e.Whoopper)
                    .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
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
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<ToastedBun>();
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(ToastedBun));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().Reference(e => e.ToastedBun).InverseReference(e => e.Whoopper)
                    .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
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
                modelBuilder.Entity<ToastedBun>();
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(ToastedBun));
                var principalType = model.GetEntityType(typeof(Whoopper));
                var principalProperty1 = principalType.GetProperty("AlternateKey1");
                var principalProperty2 = principalType.GetProperty("AlternateKey2");

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().Reference(e => e.ToastedBun).InverseReference(e => e.Whoopper)
                    .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 })
                    .PrincipalKey<Whoopper>(e => new { e.AlternateKey1, e.AlternateKey2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);
                Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
                Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
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
                modelBuilder.Entity<ToastedBun>();
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(ToastedBun));
                var principalType = model.GetEntityType(typeof(Whoopper));
                var principalProperty1 = principalType.GetProperty("AlternateKey1");
                var principalProperty2 = principalType.GetProperty("AlternateKey2");

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().Reference(e => e.ToastedBun).InverseReference(e => e.Whoopper)
                    .PrincipalKey<Whoopper>(e => new { e.AlternateKey1, e.AlternateKey2 })
                    .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);
                Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
                Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
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
            public virtual void Uses_composite_PK_for_FK_by_convention()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<ToastedBun>();

                var dependentType = model.GetEntityType(typeof(Moostard));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("Id1");
                var fkProperty2 = dependentType.GetProperty("Id2");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetPrimaryKey();

                modelBuilder
                    .Entity<Moostard>().Reference(e => e.Whoopper).InverseReference(e => e.Moostard)
                    .ForeignKey<Moostard>(e => new { e.Id1, e.Id2 });

                var fk = dependentType.GetForeignKeys().Single();

                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("Moostard", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { dependentKey.Properties[0].Name, dependentKey.Properties[1].Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Principal_and_dependent_can_be_flipped_and_composite_PK_is_still_used_by_convention()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<ToastedBun>();

                var dependentType = model.GetEntityType(typeof(Moostard));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("Id1");
                var fkProperty2 = dependentType.GetProperty("Id2");

                var principalKey = principalType.GetPrimaryKey();
                var dependentKey = dependentType.GetPrimaryKey();

                modelBuilder
                    .Entity<Moostard>().Reference(e => e.Whoopper).InverseReference(e => e.Moostard)
                    .ForeignKey<Moostard>(e => new { e.Id1, e.Id2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("Moostard", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { dependentKey.Properties[0].Name, dependentKey.Properties[1].Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Principal_and_dependent_can_be_flipped_using_principal_and_composite_PK_is_still_used_by_convention()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<ToastedBun>();

                var dependentType = model.GetEntityType(typeof(Moostard));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("Id1");
                var fkProperty2 = dependentType.GetProperty("Id2");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetPrimaryKey();

                modelBuilder
                    .Entity<Moostard>().Reference(e => e.Whoopper).InverseReference(e => e.Moostard)
                    .PrincipalKey<Whoopper>(e => new { e.Id1, e.Id2 })
                    .Required();

                var fk = dependentType.GetForeignKeys().Single();

                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Equal("Moostard", principalType.Navigations.Single().Name);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { dependentKey.Properties[0].Name, dependentKey.Properties[1].Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_composite_FK_when_specified_on_principal_with_navigation_to_dependent()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<ToastedBun>();
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(ToastedBun));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().Reference(e => e.ToastedBun).InverseReference()
                    .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Empty(dependentType.Navigations);
                Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
                Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_composite_FK_when_specified_on_principal_with_navigation_to_principal()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<ToastedBun>();
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(ToastedBun));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().Reference<ToastedBun>().InverseReference(e => e.Whoopper)
                    .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
                Assert.Empty(principalType.Navigations);
                Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Creates_composite_FK_when_specified_on_principal_with_no_navigations()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<ToastedBun>();
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.GetEntityType(typeof(ToastedBun));
                var principalType = model.GetEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.GetProperty("BurgerId1");
                var fkProperty2 = dependentType.GetProperty("BurgerId2");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().Reference<ToastedBun>().InverseReference()
                    .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Empty(dependentType.Navigations.Where(nav => nav.ForeignKey == fk));
                Assert.Empty(principalType.Navigations.Where(nav => nav.ForeignKey == fk));
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name, principalType.GetForeignKeys().Single().Properties.Single().Name },
                    principalType.Properties.Select(p => p.Name));
                AssertEqual(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name }, dependentType.Properties.Select(p => p.Name));
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.GetPrimaryKey());
                Assert.Same(dependentKey, dependentType.GetPrimaryKey());
            }

            [Fact]
            public virtual void Principal_and_dependent_can_be_flipped_when_self_referencing()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder
                    .Entity<SelfRef>().Reference(e => e.SelfRef1).InverseReference(e => e.SelfRef2);

                var entityType = modelBuilder.Model.GetEntityType(typeof(SelfRef));
                var fk = entityType.GetForeignKeys().Single();

                var navigationToPrincipal = fk.DependentToPrincipal;
                var navigationToDependent = fk.PrincipalToDependent;

                modelBuilder
                    .Entity<SelfRef>().Reference(e => e.SelfRef1).InverseReference(e => e.SelfRef2);

                Assert.Same(fk, entityType.GetForeignKeys().Single());
                Assert.Equal(navigationToDependent.Name, fk.PrincipalToDependent.Name);
                Assert.Equal(navigationToPrincipal.Name, fk.DependentToPrincipal.Name);
                Assert.True(((IForeignKey)fk).IsRequired);

                modelBuilder
                    .Entity<SelfRef>().Reference(e => e.SelfRef2).InverseReference(e => e.SelfRef1);

                var newFk = entityType.GetForeignKeys().Single();

                Assert.Equal(fk.Properties, newFk.Properties);
                Assert.Equal(fk.PrincipalKey, newFk.PrincipalKey);
                Assert.Equal(navigationToPrincipal.Name, newFk.PrincipalToDependent.Name);
                Assert.Equal(navigationToDependent.Name, newFk.DependentToPrincipal.Name);
                Assert.True(((IForeignKey)newFk).IsRequired);
            }

            [Fact]
            public virtual void Creates_self_referencing_FK_with_navigation_to_principal()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<SelfRef>(eb =>
                    {
                        eb.Key(e => e.Id);
                        eb.Property(e => e.SelfRefId);
                    });

                var entityType = modelBuilder.Model.GetEntityType(typeof(SelfRef));
                var expectedProperties = entityType.Properties.ToList();

                modelBuilder.Entity<SelfRef>().Reference(e => e.SelfRef1).InverseReference();

                var fk = entityType.GetForeignKeys().Single();
                var navigationToPrincipal = fk.DependentToPrincipal;
                var navigationToDependent = fk.PrincipalToDependent;

                Assert.NotEqual(fk.Properties, entityType.GetPrimaryKey().Properties);
                Assert.Equal(fk.PrincipalKey, entityType.GetPrimaryKey());
                Assert.Equal(null, navigationToDependent?.Name);
                Assert.Equal("SelfRef1", navigationToPrincipal?.Name);
                Assert.Same(navigationToPrincipal, entityType.Navigations.Single());
                AssertEqual(expectedProperties, entityType.Properties);
                Assert.True(((IForeignKey)fk).IsRequired);
            }

            [Fact]
            public virtual void Creates_self_referencing_FK_with_navigation_to_dependent()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<SelfRef>(eb =>
                    {
                        eb.Key(e => e.Id);
                        eb.Property(e => e.SelfRefId);
                    });

                var entityType = modelBuilder.Model.GetEntityType(typeof(SelfRef));
                var expectedProperties = entityType.Properties.ToList();

                modelBuilder.Entity<SelfRef>().Reference<SelfRef>().InverseReference(e => e.SelfRef1);

                var fk = entityType.GetForeignKeys().Single();
                var navigationToPrincipal = fk.DependentToPrincipal;
                var navigationToDependent = fk.PrincipalToDependent;

                Assert.NotEqual(fk.Properties, entityType.GetPrimaryKey().Properties);
                Assert.Equal(fk.PrincipalKey, entityType.GetPrimaryKey());
                Assert.Equal("SelfRef1", navigationToDependent?.Name);
                Assert.Equal(null, navigationToPrincipal?.Name);
                Assert.Same(navigationToDependent, entityType.Navigations.Single());
                AssertEqual(expectedProperties, entityType.Properties);
                Assert.True(((IForeignKey)fk).IsRequired);
            }

            [Fact]
            public virtual void Throws_on_duplicate_navigation_when_self_referencing()
            {
                var modelBuilder = CreateModelBuilder();

                Assert.Equal(Strings.NavigationToSelfDuplicate("SelfRef1"),
                    Assert.Throws<InvalidOperationException>(() =>
                        modelBuilder.Entity<SelfRef>().Reference(e => e.SelfRef1).InverseReference(e => e.SelfRef1)).Message);
            }

            [Fact]
            public virtual void Throws_if_specified_FK_types_do_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
                modelBuilder.Ignore<Order>();

                Assert.Equal(Strings.ForeignKeyTypeMismatch("{'GuidProperty'}", typeof(CustomerDetails).FullName, typeof(Customer).FullName),
                    Assert.Throws<InvalidOperationException>(() =>
                        modelBuilder
                            .Entity<Customer>().Reference(c => c.Details).InverseReference(d => d.Customer)
                            .PrincipalKey(typeof(Customer), "Id")
                            .ForeignKey(typeof(CustomerDetails), "GuidProperty")).Message);
            }

            [Fact]
            public virtual void Throws_if_specified_PK_types_do_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
                modelBuilder.Ignore<Order>();

                Assert.Equal(Strings.ForeignKeyTypeMismatch("{'GuidProperty'}", typeof(CustomerDetails).FullName, typeof(Customer).FullName),
                    Assert.Throws<InvalidOperationException>(() =>
                        modelBuilder
                            .Entity<Customer>().Reference(c => c.Details).InverseReference(d => d.Customer)
                            .ForeignKey(typeof(CustomerDetails), "GuidProperty")
                            .PrincipalKey(typeof(Customer), "Id")).Message);
            }

            [Fact]
            public virtual void Throws_if_specified_FK_count_does_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
                modelBuilder.Ignore<Order>();

                Assert.Equal(Strings.ForeignKeyCountMismatch("{'Id', 'GuidProperty'}", typeof(CustomerDetails).FullName, "{'Id'}", typeof(Customer).FullName),
                    Assert.Throws<InvalidOperationException>(() =>
                        modelBuilder
                            .Entity<Customer>().Reference(c => c.Details).InverseReference(d => d.Customer)
                            .PrincipalKey(typeof(Customer), "Id")
                            .ForeignKey(typeof(CustomerDetails), "Id", "GuidProperty")).Message);
            }

            [Fact]
            public virtual void Throws_if_specified_PK_count_does_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<CustomerDetails>().Property<Guid>("GuidProperty");
                modelBuilder.Ignore<Order>();

                Assert.Equal(Strings.ForeignKeyCountMismatch("{'Id', 'GuidProperty'}", typeof(CustomerDetails).FullName, "{'Id'}", typeof(Customer).FullName),
                    Assert.Throws<InvalidOperationException>(() =>
                        modelBuilder
                            .Entity<Customer>().Reference(c => c.Details).InverseReference(d => d.Customer)
                            .ForeignKey(typeof(CustomerDetails), "Id", "GuidProperty")
                            .PrincipalKey(typeof(Customer), "Id")).Message);
            }

            [Fact]
            public virtual void Finds_and_removes_existing_many_to_one_relationship()
            {
                var modelBuilder = HobNobBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Hob>().Reference(e => e.Nob).InverseCollection(e => e.Hobs);

                var dependentType = model.GetEntityType(typeof(Nob));
                var principalType = model.GetEntityType(typeof(Hob));
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Nob>().Reference(e => e.Hob).InverseReference(e => e.Nob);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.True(fk.IsUnique);
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
            public virtual void Finds_and_removes_existing_one_to_many_relationship()
            {
                var modelBuilder = HobNobBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Hob>().Collection(e => e.Nobs).InverseReference(e => e.Hob);

                var dependentType = model.GetEntityType(typeof(Nob));
                var principalType = model.GetEntityType(typeof(Hob));
                var expectedPrincipalProperties = principalType.Properties.ToList();
                var expectedDependentProperties = dependentType.Properties.ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Nob>().Reference(e => e.Hob).InverseReference(e => e.Nob);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.True(fk.IsUnique);
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
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Entity<Customer>();
                modelBuilder.Ignore<Order>();

                var dependentType = model.GetEntityType(typeof(CustomerDetails));

                var builder = modelBuilder.Entity<CustomerDetails>().Reference(e => e.Customer).InverseReference(e => e.Details);
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
                    .Entity<Hob>().Reference(e => e.Nob).InverseReference(e => e.Hob)
                    .ForeignKey<Nob>(e => new { e.HobId1, e.HobId2 });

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
                    .Entity<Nob>().Reference(e => e.Hob).InverseReference(e => e.Nob)
                    .ForeignKey<Hob>(e => new { e.NobId1, e.NobId2 });

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

                Assert.False(entityType.GetProperty("NobId1").IsNullable);
                Assert.False(entityType.GetProperty("NobId1").IsNullable);
                Assert.True(entityType.GetForeignKeys().Single().IsRequired);
            }

            [Fact]
            public virtual void Nullable_FK_can_be_made_required()
            {
                var modelBuilder = HobNobBuilder();
                var principalType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
                var dependentType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
                var expectedPrincipalProperties = principalType.GetProperties().ToList();
                var expectedDependentProperties = dependentType.GetProperties().ToList();

                modelBuilder
                    .Entity<Hob>().Reference(e => e.Nob).InverseReference(e => e.Hob)
                    .Required()
                    .ForeignKey<Nob>(e => new { e.HobId1, e.HobId2 });

                Assert.False(dependentType.GetProperty("HobId1").IsNullable);
                Assert.False(dependentType.GetProperty("HobId2").IsNullable);
                Assert.True(dependentType.GetForeignKeys().Single().IsRequired);

                AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
                AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            }

            [Fact]
            public virtual void Non_nullable_FK_cannot_be_made_optional()
            {
                var modelBuilder = HobNobBuilder();

                Assert.Equal(
                    Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder
                        .Entity<Nob>().Reference(e => e.Hob).InverseReference(e => e.Nob)
                        .ForeignKey<Hob>(e => new { e.NobId1, e.NobId2 })
                        .Required(false)).Message);

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

                Assert.False(entityType.GetProperty("NobId1").IsNullable);
                Assert.False(entityType.GetProperty("NobId1").IsNullable);
                Assert.True(entityType.GetForeignKeys().Single().IsRequired);
            }

            [Fact]
            public virtual void Can_use_non_nullable_FK_if_optional()
            {
                var modelBuilder = HobNobBuilder();

                Assert.Equal(
                    Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder
                        .Entity<Nob>().Reference(e => e.Hob).InverseReference(e => e.Nob)
                        .Required(false)
                        .ForeignKey<Hob>(e => new { e.NobId1, e.NobId2 })).Message);

                var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

                Assert.False(entityType.GetProperty("NobId1").IsNullable);
                Assert.False(entityType.GetProperty("NobId1").IsNullable);
                Assert.True(entityType.GetForeignKeys().Single().IsRequired);
            }

            [Fact]
            public virtual void Unspecified_FK_can_be_made_optional()
            {
                var modelBuilder = HobNobBuilder();
                var principalType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
                var dependentType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
                var expectedPrincipalProperties = principalType.GetProperties().ToList();
                var expectedDependentProperties = dependentType.GetProperties().ToList();

                modelBuilder
                    .Entity<Hob>().Reference(e => e.Nob).InverseReference(e => e.Hob)
                    .Required(false)
                    .PrincipalKey<Nob>(e => new { e.Id1, e.Id2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.False(fk.IsRequired);

                AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
                expectedDependentProperties.AddRange(fk.Properties);
                AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            }

            [Fact]
            public virtual void One_to_one_relationships_with_unspecified_keys_can_be_made_required()
            {
                var modelBuilder = HobNobBuilder();
                var principalType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
                var dependentType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
                var expectedPrincipalProperties = principalType.GetProperties().ToList();
                var expectedDependentProperties = dependentType.GetProperties().ToList();

                modelBuilder
                    .Entity<Hob>().Reference(e => e.Nob).InverseReference(e => e.Hob)
                    .Required()
                    .PrincipalKey<Nob>(e => new { e.Id1, e.Id2 });

                var fk = dependentType.GetForeignKeys().Single();
                Assert.True(fk.IsRequired);

                AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
                expectedDependentProperties.AddRange(fk.Properties);
                AssertEqual(expectedDependentProperties, dependentType.GetProperties());
            }

            [Fact]
            public virtual void Can_be_defined_before_the_PK_from_principal()
            {
                var modelBuilder = CreateModelBuilder(new Model());

                modelBuilder
                    .Entity<Hob>(eb =>
                        {
                            eb.Reference(e => e.Nob).InverseReference(e => e.Hob)
                                .ForeignKey<Nob>(e => new { e.HobId1, e.HobId2 })
                                .PrincipalKey<Hob>(e => new { e.Id1, e.Id2 });
                            eb.Key(e => new { e.Id1, e.Id2 });
                        });

                modelBuilder.Entity<Nob>().Key(e => new { e.Id1, e.Id2 });

                var dependentEntityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
                var fk = dependentEntityType.GetForeignKeys().Single();
                AssertEqual(fk.PrincipalKey.Properties, dependentEntityType.GetKeys().Single().Properties);
                Assert.False(fk.IsRequired);

                var principalEntityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
                AssertEqual(fk.PrincipalKey.Properties, principalEntityType.GetKeys().Single().Properties);
            }

            [Fact]
            public virtual void Can_be_defined_before_the_PK_from_dependent()
            {
                var modelBuilder = CreateModelBuilder(new Model());

                modelBuilder
                    .Entity<Hob>(eb =>
                        {
                            eb.Reference(e => e.Nob).InverseReference(e => e.Hob)
                                .ForeignKey<Hob>(e => new { e.NobId1, e.NobId2 })
                                .PrincipalKey<Nob>(e => new { e.Id1, e.Id2 });
                            eb.Key(e => new { e.Id1, e.Id2 });
                        });

                modelBuilder.Entity<Nob>().Key(e => new { e.Id1, e.Id2 });

                var dependentEntityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));
                var fk = dependentEntityType.GetForeignKeys().Single();
                AssertEqual(fk.PrincipalKey.Properties, dependentEntityType.GetKeys().Single().Properties);
                Assert.True(fk.IsRequired);

                var principalEntityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));
                AssertEqual(fk.PrincipalKey.Properties, principalEntityType.GetKeys().Single().Properties);
            }
        }
    }
}

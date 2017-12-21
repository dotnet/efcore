// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class OneToManyTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Finds_existing_navigations_and_uses_associated_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>()
                    .HasOne(o => o.Customer).WithMany(c => c.Orders)
                    .HasForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<BackOrder>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var navToPrincipal = dependentType.FindNavigation("Customer");
                var navToDependent = principalType.FindNavigation("Orders");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

                modelBuilder.Validate();

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal(navToPrincipal.Name, dependentType.GetNavigations().Single().Name);
                Assert.Same(navToDependent, principalType.GetNavigations().Single());
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Finds_existing_navigation_to_principal_and_uses_associated_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder
                    .Entity<Order>().HasOne(c => c.Customer).WithMany()
                    .HasForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Finds_existing_navigation_to_dependent_and_uses_associated_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne()
                    .HasForeignKey(e => e.CustomerId);
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));
                var fk = principalType.GetNavigations().Single().ForeignKey;

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

                modelBuilder.Validate();

                var newFk = principalType.GetNavigations().Single().ForeignKey;
                AssertEqual(fk.Properties, newFk.Properties);
                Assert.Same(newFk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Equal(nameof(Order.Customer), newFk.DependentToPrincipal.Name);
                Assert.Equal(nameof(Customer.Orders), newFk.PrincipalToDependent.Name);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_uses_existing_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<BackOrder>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

                modelBuilder.Validate();

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_relationship_with_both_navigations()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<BackOrder>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var fkProperty = dependentType.FindProperty("CustomerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_relationship_with_navigation_to_dependent()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var fkProperty = dependentType.FindProperty(nameof(Order.CustomerId));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne();

                var fk = principalType.GetNavigations().Single().ForeignKey;
                Assert.Equal(nameof(Customer.Orders), fk.PrincipalToDependent.Name);
                Assert.Null(fk.DependentToPrincipal);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.NotNull(dependentType.FindForeignKeys(fkProperty).SingleOrDefault());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_relationship_with_navigation_to_principal()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var fkProperty = dependentType.FindProperty(nameof(Order.CustomerId));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany<Order>().WithOne(e => e.Customer);

                var fk = dependentType.GetNavigations().Single().ForeignKey;
                Assert.Equal(nameof(Order.Customer), fk.DependentToPrincipal.Name);
                Assert.Null(fk.PrincipalToDependent);
                Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.NotNull(dependentType.FindForeignKeys(fkProperty).SingleOrDefault());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_relationship_with_no_navigations()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<BackOrder>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var fkProperty = dependentType.FindProperty("CustomerId");
                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Customer>().HasMany<Order>().WithOne();

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);

                Assert.Equal(fk.PrincipalKey.Properties, newFk.PrincipalKey.Properties);
                Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == newFk));
                Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey == newFk));
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { "AnotherCustomerId", fk.Properties.Single().Name, newFk.Properties.Single().Name, dependentKey.Properties.Single().Name },
                    dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_uses_specified_FK_even_if_found_by_convention()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<BackOrder>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var fkProperty = dependentType.FindProperty("CustomerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasForeignKey(e => e.CustomerId);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_uses_existing_FK_not_found_by_convention()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany()
                    .HasForeignKey(c => c.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));
                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.Properties.Single().Name == "BurgerId");
                fk.HasDependentToPrincipal((string)null);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.Validate();

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_creates_FK_specified()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var fkProperty = dependentType.FindProperty("BurgerId");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_specified_FK_with_navigation_to_dependent()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var fkProperty = dependentType.FindProperty(nameof(Pickle.BurgerId));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne()
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.Validate();

                var fk = principalType.GetNavigations().Single().ForeignKey;
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Equal(nameof(BigMak.Pickles), fk.PrincipalToDependent.Name);
                Assert.Null(fk.DependentToPrincipal);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_specified_FK_with_navigation_to_principal()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().HasMany<Pickle>().WithOne(e => e.BigMak)
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.Validate();

                var fk = dependentType.GetNavigations().Single().ForeignKey;
                Assert.Same(dependentType.FindProperty(nameof(Pickle.BurgerId)), fk.Properties.Single());
                Assert.Equal(nameof(Pickle.BigMak), fk.DependentToPrincipal.Name);
                Assert.Null(fk.PrincipalToDependent);
                Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_relationship_with_no_navigations_and_specified_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var fkProperty = dependentType.FindProperty("BurgerId");
                var fk = dependentType.GetForeignKeys().SingleOrDefault();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().HasMany<Pickle>().WithOne()
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.Validate();

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);
                Assert.Same(fkProperty, newFk.Properties.Single());

                Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey != fk));
                Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey != fk));
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_creates_shadow_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.Equal("BigMakId", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, "BurgerId", dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_shadow_FK_with_navigation_to_dependent()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne();

                modelBuilder.Validate();

                var fk = principalType.GetNavigations().Single().ForeignKey;
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Equal(nameof(BigMak.Pickles), fk.PrincipalToDependent.Name);
                Assert.Null(fk.DependentToPrincipal);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_shadow_FK_with_navigation_to_principal()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<BigMak>().HasMany<Pickle>().WithOne(e => e.BigMak);

                modelBuilder.Validate();

                var fk = dependentType.GetNavigations().Single().ForeignKey;
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Equal(nameof(Pickle.BigMak), fk.DependentToPrincipal.Name);
                Assert.Null(fk.PrincipalToDependent);
                Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_shadow_FK_with_no_navigation()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();
                var existingFk = dependentType.GetForeignKeys().Single();

                modelBuilder.Entity<BigMak>().HasMany<Pickle>().WithOne();

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.Equal("BigMakId1", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty);
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey != existingFk));
                Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey != existingFk));
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { existingFk.Properties.Single().Name, fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name },
                    dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_matches_shadow_FK_property_by_convention()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>().Property<int>("BigMakId");
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                var fkProperty = dependentType.FindProperty("BigMakId");

                modelBuilder.Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name },
                    dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_overrides_existing_FK_when_uniqueness_does_not_match()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>().HasOne<Pickle>().WithOne()
                    .HasForeignKey<Pickle>(e => e.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                var fk = dependentType.GetForeignKeys()
                    .Single(foreignKey => foreignKey.Properties.Any(p => p.Name == "BurgerId"));
                Assert.True(((IForeignKey)fk).IsUnique);

                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.Validate();

                Assert.Equal(1, dependentType.GetForeignKeys().Count());
                Assert.False(fk.IsUnique);

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name },
                    dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Resolves_ambiguous_navigations()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Friendship>().HasOne(e => e.ApplicationUser).WithMany(e => e.Friendships)
                    .HasForeignKey(e => e.ApplicationUserId);

                modelBuilder.Validate();

                Assert.Equal(2, model.FindEntityType(typeof(Friendship)).GetNavigations().Count());
            }

            [Fact]
            public virtual void Can_use_explicitly_specified_PK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<BackOrder>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var fkProperty = dependentType.FindProperty("CustomerId");
                var principalProperty = principalType.FindProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasPrincipalKey(e => e.Id);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Can_use_non_PK_principal()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));
                var expectedPrincipalProperties = principalType.GetProperties().ToList();
                var expectedDependentProperties = dependentType.GetProperties().ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasPrincipalKey(e => e.AlternateKey);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Equal("AlternateKey", fk.PrincipalKey.Properties.Single().Name);

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Equal(1, dependentType.GetKeys().Count());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                expectedPrincipalProperties.Add(fk.PrincipalKey.Properties.Single());
                AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.GetProperties());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Can_have_both_convention_properties_specified()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<BackOrder>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var fkProperty = dependentType.FindProperty("CustomerId");
                var principalProperty = principalType.FindProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasForeignKey(e => e.CustomerId)
                    .HasPrincipalKey(e => e.Id);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Can_have_both_convention_properties_specified_in_any_order()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<BackOrder>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var fkProperty = dependentType.FindProperty("CustomerId");
                var principalProperty = principalType.FindProperty(Customer.IdProperty.Name);

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasPrincipalKey(e => e.Id)
                    .HasForeignKey(e => e.CustomerId);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Can_have_FK_by_convention_specified_with_explicit_principal_key()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));
                var expectedPrincipalProperties = principalType.GetProperties().ToList();
                var expectedDependentProperties = dependentType.GetProperties().ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasForeignKey(e => e.AnotherCustomerId)
                    .HasPrincipalKey(e => e.AlternateKey);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Equal("AnotherCustomerId", fk.Properties.Single().Name);
                Assert.Equal("AlternateKey", fk.PrincipalKey.Properties.Single().Name);

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Equal(1, dependentType.GetKeys().Count());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                expectedPrincipalProperties.Add(fk.PrincipalKey.Properties.Single());
                AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.GetProperties());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));
                var expectedPrincipalProperties = principalType.GetProperties().ToList();
                var expectedDependentProperties = dependentType.GetProperties().ToList();
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasPrincipalKey(e => e.AlternateKey)
                    .HasForeignKey(e => e.AnotherCustomerId);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Equal("AnotherCustomerId", fk.Properties.Single().Name);
                Assert.Equal("AlternateKey", fk.PrincipalKey.Properties.Single().Name);

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Equal(1, dependentType.GetKeys().Count());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                expectedPrincipalProperties.Add(fk.PrincipalKey.Properties.Single());
                AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
                expectedDependentProperties.Add(fk.Properties.Single());
                AssertEqual(expectedDependentProperties, dependentType.GetProperties());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Can_have_principal_key_by_convention_specified_with_explicit_PK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var fkProperty = dependentType.FindProperty("BurgerId");
                var principalProperty = principalType.FindProperty("AlternateKey");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasForeignKey(e => e.BurgerId)
                    .HasPrincipalKey(e => e.AlternateKey);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>();
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var fkProperty = dependentType.FindProperty("BurgerId");
                var principalProperty = principalType.FindProperty("AlternateKey");

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasPrincipalKey(e => e.AlternateKey)
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { fk.Properties.Single().Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Can_have_principal_key_by_convention_replaced_with_primary_key()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasForeignKey(e => e.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var dependentKey = dependentType.GetKeys().SingleOrDefault();

                var expectedPrincipalProperties = principalType.GetProperties().ToList();
                var expectedDependentProperties = dependentType.GetProperties().ToList();

                modelBuilder.Entity<BigMak>().HasKey(e => e.AlternateKey);

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                var principalProperty = principalType.FindProperty("AlternateKey");

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
                AssertEqual(expectedDependentProperties, dependentType.GetProperties());
                Assert.Empty(principalType.GetForeignKeys());

                var principalKey = principalType.GetKeys().Single();
                Assert.Same(principalProperty, principalKey.Properties.Single());
                Assert.Same(principalKey, fk.PrincipalKey);

                Assert.Same(dependentKey, dependentType.GetKeys().SingleOrDefault());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Principal_key_by_convention_is_not_replaced_with_new_incompatible_primary_key()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasForeignKey(e => new { e.BurgerId, e.Id });
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var modelClone = modelBuilder.Model.Clone();
                var nonPrimaryPrincipalKey = modelClone.FindEntityType(typeof(BigMak).FullName)
                    .GetKeys().Single(k => !k.IsPrimaryKey());
                var dependentKey = dependentType.GetKeys().SingleOrDefault();

                var expectedDependentProperties = dependentType.GetProperties().ToList();
                var expectedPrincipalProperties = principalType.GetProperties().ToList();

                modelBuilder.Entity<BigMak>().HasKey(e => e.AlternateKey);

                var principalProperty = principalType.FindProperty("AlternateKey");
                var fk = dependentType.GetForeignKeys().Single();
                Assert.Equal(2, fk.Properties.Count);
                AssertEqual(
                    nonPrimaryPrincipalKey.Properties, fk.PrincipalKey.Properties,
                    new PropertyComparer(false));

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(expectedPrincipalProperties.Select(p => p.Name), principalType.GetProperties().Select(p => p.Name));
                AssertEqual(expectedDependentProperties.Select(p => p.Name), dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());

                var primaryPrincipalKey = principalType.FindPrimaryKey();
                Assert.Same(principalProperty, primaryPrincipalKey.Properties.Single());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.True(principalType.GetKeys().Contains(fk.PrincipalKey));

                Assert.Same(dependentKey, dependentType.GetKeys().SingleOrDefault());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Explicit_principal_key_is_not_replaced_with_new_primary_key()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder
                    .Entity<BigMak>().HasMany(e => e.Pickles).WithOne(e => e.BigMak)
                    .HasPrincipalKey(e => new { e.Id });
                modelBuilder.Ignore<Bun>();

                var principalType = model.FindEntityType(typeof(BigMak));
                var dependentType = model.FindEntityType(typeof(Pickle));

                var nonPrimaryPrincipalKey = principalType.GetKeys().Single();

                var dependentKey = dependentType.GetKeys().SingleOrDefault();

                var modelClone = model.Clone();
                var expectedPrincipalProperties = modelClone.FindEntityType(typeof(BigMak)).GetProperties().ToList();
                var expectedDependentProperties = modelClone.FindEntityType(typeof(Pickle)).GetProperties().ToList();

                modelBuilder.Entity<BigMak>().HasKey(e => e.AlternateKey);

                var principalProperty = principalType.FindProperty("AlternateKey");
                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(nonPrimaryPrincipalKey, fk.PrincipalKey);

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(expectedPrincipalProperties.Select(p => p.Name), principalType.GetProperties().Select(p => p.Name));
                AssertEqual(expectedDependentProperties, dependentType.GetProperties());
                Assert.Empty(principalType.GetForeignKeys());

                var primaryPrincipalKey = principalType.FindPrimaryKey();
                Assert.Same(principalProperty, primaryPrincipalKey.Properties.Single());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.True(principalType.GetKeys().Contains(nonPrimaryPrincipalKey));
                var oldKeyProperty = principalType.FindProperty(nameof(BigMak.Id));
                var newKeyProperty = principalType.FindProperty(nameof(BigMak.AlternateKey));
                Assert.False(oldKeyProperty.RequiresValueGenerator());
                Assert.Equal(ValueGenerated.Never, oldKeyProperty.ValueGenerated);
                Assert.True(newKeyProperty.RequiresValueGenerator());
                Assert.Equal(ValueGenerated.OnAdd, newKeyProperty.ValueGenerated);
                Assert.Same(dependentKey, dependentType.GetKeys().SingleOrDefault());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_uses_existing_composite_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany()
                    .HasForeignKey(c => new { c.BurgerId1, c.BurgerId2 });
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Equal(nameof(Tomato.Whoopper), dependentType.GetNavigations().Single().Name);
                Assert.Equal(nameof(Whoopper.Tomatoes), principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_both_navigations_and_creates_composite_FK_specified()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>(b => b.HasKey(c => new { c.Id1, c.Id2 }));
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.FindProperty("BurgerId1");
                var fkProperty2 = dependentType.FindProperty("BurgerId2");

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Tomatoes", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Can_use_alternate_composite_key()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>(b => b.HasKey(c => new { c.Id1, c.Id2 }));
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.FindProperty("BurgerId1");
                var fkProperty2 = dependentType.FindProperty("BurgerId2");
                var principalProperty1 = principalType.FindProperty("AlternateKey1");
                var principalProperty2 = principalType.FindProperty("AlternateKey2");

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                    .HasPrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 });

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);
                Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
                Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

                Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Tomatoes", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Can_use_alternate_composite_key_in_any_order()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>(b => b.HasKey(c => new { c.Id1, c.Id2 }));
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.FindProperty("BurgerId1");
                var fkProperty2 = dependentType.FindProperty("BurgerId2");
                var principalProperty1 = principalType.FindProperty("AlternateKey1");
                var principalProperty2 = principalType.FindProperty("AlternateKey2");

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne(e => e.Whoopper)
                    .HasPrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.Validate();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);
                Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
                Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

                Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Tomatoes", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());

                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_specified_composite_FK_with_navigation_to_dependent()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Tomato>(
                    b =>
                        {
                            b.Property(e => e.BurgerId1);
                            b.Property(e => e.BurgerId2);
                        });
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.FindProperty(nameof(Tomato.BurgerId1));
                var fkProperty2 = dependentType.FindProperty(nameof(Tomato.BurgerId2));

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().HasMany(e => e.Tomatoes).WithOne()
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.Validate();

                var fk = principalType.GetNavigations().Single().ForeignKey;
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal(nameof(Whoopper.Tomatoes), fk.PrincipalToDependent.Name);
                Assert.Null(fk.DependentToPrincipal);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_specified_composite_FK_with_navigation_to_principal()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Whoopper>().HasMany<Tomato>().WithOne(e => e.Whoopper)
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.Validate();

                var fk = dependentType.GetNavigations().Single().ForeignKey;
                Assert.Same(dependentType.FindProperty(nameof(Tomato.BurgerId1)), fk.Properties[0]);
                Assert.Same(dependentType.FindProperty(nameof(Tomato.BurgerId2)), fk.Properties[1]);

                Assert.Equal(nameof(Tomato.Whoopper), fk.DependentToPrincipal.Name);
                Assert.Null(fk.PrincipalToDependent);
                Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_relationship_with_no_navigations_and_specified_composite_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Whoopper>().HasMany(w => w.Tomatoes).WithOne(t => t.Whoopper);
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<Moostard>();
                modelBuilder.Ignore<ToastedBun>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var fk = dependentType.GetForeignKeys().SingleOrDefault();
                var fkProperty1 = dependentType.FindProperty("BurgerId1");
                var fkProperty2 = dependentType.FindProperty("BurgerId2");

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.FindPrimaryKey();

                modelBuilder
                    .Entity<Whoopper>().HasMany<Tomato>().WithOne()
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.Validate();

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);
                Assert.Same(fkProperty1, newFk.Properties[0]);
                Assert.Same(fkProperty2, newFk.Properties[1]);

                Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey == newFk));
                Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey == newFk));
                AssertEqual(
                    new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { fkProperty1.Name, fkProperty2.Name, dependentKey.Properties.Single().Name, fk.Properties[0].Name, fk.Properties[1].Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [Fact]
            public virtual void Creates_relationship_on_existing_FK_is_using_different_principal_key()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                    .HasForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.FindEntityType(typeof(ToastedBun));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.FindPrimaryKey();

                modelBuilder
                    .Entity<Whoopper>().HasMany<ToastedBun>().WithOne()
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                    .HasPrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 });

                var navigation = dependentType.GetNavigations().Single();
                var existingFk = navigation.ForeignKey;
                Assert.Same(existingFk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Equal(nameof(ToastedBun.Whoopper), navigation.Name);
                Assert.Equal(nameof(Whoopper.ToastedBun), navigation.FindInverse().Name);
                Assert.Equal(existingFk.DeclaringEntityType == dependentType ? 0 : 1, principalType.GetForeignKeys().Count());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
                Assert.NotSame(principalKey, fk.PrincipalKey);
                Assert.NotEqual(existingFk.Properties, fk.Properties);
                Assert.Equal(principalType.GetForeignKeys().Count(), principalType.GetIndexes().Count());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.True(existingFk.DeclaringEntityType.FindIndex(existingFk.Properties).IsUnique);
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);

                Assert.Equal(
                    CoreStrings.AmbiguousOneToOneRelationship(
                        existingFk.DeclaringEntityType.DisplayName() + "." + existingFk.DependentToPrincipal.Name,
                        existingFk.PrincipalEntityType.DisplayName() + "." + existingFk.PrincipalToDependent.Name),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Validate()).Message);
            }

            [Fact]
            public virtual void Creates_relationship_on_existing_FK_is_using_different_principal_key_different_order()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                    .HasForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Moostard>();

                var dependentType = model.FindEntityType(typeof(ToastedBun));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.FindPrimaryKey();

                modelBuilder
                    .Entity<Whoopper>().HasMany<ToastedBun>().WithOne()
                    .HasPrincipalKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                    .HasForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

                var existingFk = dependentType.GetNavigations().Single().ForeignKey;
                Assert.Same(existingFk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Equal(nameof(Tomato.Whoopper), existingFk.DependentToPrincipal.Name);
                Assert.Equal(nameof(Whoopper.ToastedBun), existingFk.PrincipalToDependent.Name);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
                Assert.NotSame(principalKey, fk.PrincipalKey);
                Assert.Equal(existingFk.Properties, fk.Properties);
                Assert.Empty(principalType.GetIndexes());
                Assert.Equal(1, dependentType.GetIndexes().Count());
                Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            }

            [Fact]
            public virtual void Throws_on_existing_one_to_one_relationship()
            {
                var modelBuilder = HobNobBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob);

                var dependentType = model.FindEntityType(typeof(Hob));
                var principalType = model.FindEntityType(typeof(Nob));

                Assert.Equal(
                    CoreStrings.ConflictingRelationshipNavigation(
                        principalType.DisplayName(),
                        nameof(Nob.Hobs),
                        dependentType.DisplayName(),
                        nameof(Hob.Nob),
                        dependentType.DisplayName(),
                        nameof(Hob.Nob),
                        principalType.DisplayName(),
                        nameof(Nob.Hob)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            modelBuilder.Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)).Message);
            }

            [Fact]
            public virtual void Removes_existing_unidirectional_one_to_one_relationship()
            {
                var modelBuilder = HobNobBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Nob>().HasOne(e => e.Hob).WithOne(e => e.Nob);

                modelBuilder.Entity<Nob>().HasOne<Hob>().WithOne(e => e.Nob);

                var dependentType = model.FindEntityType(typeof(Hob));
                var principalType = model.FindEntityType(typeof(Nob));
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob);

                var fk = dependentType.GetForeignKeys().Single();
                Assert.False(fk.IsUnique);
                Assert.Equal(nameof(Nob.Hobs), fk.PrincipalToDependent.Name);
                Assert.Equal(nameof(Hob.Nob), fk.DependentToPrincipal.Name);
                var otherFk = principalType.GetForeignKeys().Single();
                Assert.False(fk.IsUnique);
                Assert.Equal(nameof(Hob.Nobs), otherFk.PrincipalToDependent.Name);
                Assert.Equal(nameof(Nob.Hob), otherFk.DependentToPrincipal.Name);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            }

            [Fact]
            public virtual void Can_add_annotations()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));

                var builder = modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);
                builder = builder.HasAnnotation("Fus", "Ro");

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fk, builder.Metadata);
                Assert.Equal("Ro", fk["Fus"]);
            }

            [Fact]
            public virtual void Annotations_are_preserved_when_rebuilding()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var builder = modelBuilder.Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer);
                builder = builder.HasAnnotation("Fus", "Ro");
                builder = builder.HasForeignKey("ShadowFK");

                Assert.Equal("Ro", builder.Metadata["Fus"]);
            }

            [Fact]
            public virtual void Nullable_FK_are_optional_by_default()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                    .HasForeignKey(e => new { e.HobId1, e.HobId2 });

                modelBuilder.Validate();

                var entityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));
                var fk = entityType.GetForeignKeys().Single();
                Assert.False(fk.IsRequired);
                var fkProperty1 = entityType.FindProperty(nameof(Nob.HobId1));
                var fkProperty2 = entityType.FindProperty(nameof(Nob.HobId2));
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

                modelBuilder.Validate();

                var entityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
                var fk = entityType.GetForeignKeys().Single();
                Assert.True(fk.IsRequired);
                var fkProperty1 = entityType.FindProperty(nameof(Hob.NobId1));
                var fkProperty2 = entityType.FindProperty(nameof(Hob.NobId2));
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

                modelBuilder.Validate();

                var entityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));
                var fk = entityType.GetForeignKeys().Single();
                Assert.True(fk.IsRequired);
                var fkProperty1 = entityType.FindProperty(nameof(Nob.HobId1));
                var fkProperty2 = entityType.FindProperty(nameof(Nob.HobId2));
                Assert.False(fkProperty1.IsNullable);
                Assert.False(fkProperty2.IsNullable);
                Assert.Contains(fkProperty1, fk.Properties);
                Assert.Contains(fkProperty2, fk.Properties);
            }

            [Fact]
            public virtual void Non_nullable_FK_cannot_be_made_optional()
            {
                var modelBuilder = HobNobBuilder();

                Assert.Equal(
                    CoreStrings.ForeignKeyCannotBeOptional("{'NobId1', 'NobId2'}", typeof(Hob).Name),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder
                            .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
                            .HasForeignKey(e => new { e.NobId1, e.NobId2 })
                            .IsRequired(false)).Message);
            }

            [Fact]
            public virtual void Non_nullable_FK_can_be_made_optional_separetely()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
                    .HasForeignKey(e => new { e.NobId1, e.NobId2 });

                modelBuilder
                    .Entity<Nob>().HasMany(e => e.Hobs).WithOne(e => e.Nob)
                    .IsRequired(false);

                modelBuilder.Validate();

                var entityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
                var fk = entityType.GetForeignKeys().Single();
                Assert.False(fk.IsRequired);
                var fkProperty1 = entityType.FindProperty(nameof(Hob.NobId1));
                var fkProperty2 = entityType.FindProperty(nameof(Hob.NobId2));
                Assert.False(fkProperty1.IsNullable);
                Assert.False(fkProperty2.IsNullable);
                Assert.DoesNotContain(fkProperty1, fk.Properties);
                Assert.DoesNotContain(fkProperty2, fk.Properties);
            }

            [Fact]
            public virtual void Can_change_delete_behavior()
            {
                var modelBuilder = HobNobBuilder();
                var dependentType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));

                modelBuilder
                    .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                    .OnDelete(DeleteBehavior.Cascade);

                Assert.Equal(DeleteBehavior.Cascade, dependentType.GetForeignKeys().Single().DeleteBehavior);

                modelBuilder
                    .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                    .OnDelete(DeleteBehavior.Restrict);

                Assert.Equal(DeleteBehavior.Restrict, dependentType.GetForeignKeys().Single().DeleteBehavior);

                modelBuilder
                    .Entity<Hob>().HasMany(e => e.Nobs).WithOne(e => e.Hob)
                    .OnDelete(DeleteBehavior.SetNull);

                Assert.Equal(DeleteBehavior.SetNull, dependentType.GetForeignKeys().Single().DeleteBehavior);
            }

            [Fact]
            public virtual void Can_set_foreign_key_property_when_matching_property_added()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<PrincipalEntity>();

                var foreignKey = model.FindEntityType(typeof(DependentEntity)).GetForeignKeys().Single();
                Assert.Equal("NavId", foreignKey.Properties.Single().Name);

                modelBuilder.Entity<DependentEntity>().Property(et => et.PrincipalEntityId);

                var newForeignKey = model.FindEntityType(typeof(DependentEntity)).GetForeignKeys().Single();
                Assert.Equal("PrincipalEntityId", newForeignKey.Properties.Single().Name);
            }

            [Fact]
            public virtual void Creates_shadow_property_for_foreign_key_according_to_navigation_to_principal_name_when_present()
            {
                var modelBuilder = CreateModelBuilder();
                var entityB = modelBuilder.Entity<Beta>().Metadata;

                Assert.Equal("FirstNavId", entityB.FindNavigation("FirstNav").ForeignKey.Properties.First().Name);
                Assert.Equal("SecondNavId", entityB.FindNavigation("SecondNav").ForeignKey.Properties.First().Name);
            }

            [Fact]
            public virtual void Creates_shadow_property_for_foreign_key_according_to_target_type_when_navigation_to_principal_name_not_present()
            {
                var modelBuilder = CreateModelBuilder();
                var gamma = modelBuilder.Entity<Gamma>().Metadata;

                Assert.Equal("GammaId", gamma.FindNavigation("Alphas").ForeignKey.Properties.First().Name);
            }

            [Fact]
            public virtual void Creates_shadow_FK_property_with_non_shadow_PK()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Alpha>();
                modelBuilder.Entity<Beta>(
                    b =>
                        {
                            b.HasOne(e => e.FirstNav)
                                .WithMany()
                                .HasForeignKey("ShadowId");
                        });

                modelBuilder.Validate();

                Assert.Equal("ShadowId", modelBuilder.Model.FindEntityType(typeof(Beta)).FindNavigation("FirstNav").ForeignKey.Properties.Single().Name);
            }

            [Fact]
            public virtual void Creates_shadow_FK_property_with_shadow_PK()
            {
                var modelBuilder = CreateModelBuilder();

                var entityA = modelBuilder.Entity<Alpha>();
                entityA.Property<int>("ShadowPK");
                entityA.HasKey("ShadowPK");

                var entityB = modelBuilder.Entity<Beta>();

                entityB.HasOne(e => e.FirstNav).WithMany().HasForeignKey("ShadowId");

                modelBuilder.Validate();

                Assert.Equal("ShadowId", modelBuilder.Model.FindEntityType(typeof(Beta)).FindNavigation("FirstNav").ForeignKey.Properties.Single().Name);
            }

            [Fact]
            public virtual void Handles_identity_correctly_while_removing_navigation()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Delta>();
                modelBuilder.Entity<Epsilon>().HasOne<Alpha>().WithMany(b => b.Epsilons);

                modelBuilder.Validate();

                var property = modelBuilder.Model.FindEntityType(typeof(Epsilon)).FindProperty("Id");
                Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
            }

            [Fact]
            public virtual void Throws_when_foreign_key_references_shadow_key()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders).HasForeignKey(e => e.AnotherCustomerId);

                Assert.Equal(
                    CoreStrings.ReferencedShadowKey(
                        typeof(Order).Name + "." + nameof(Order.Customer),
                        typeof(Customer).Name + "." + nameof(Customer.Orders),
                        "{'AnotherCustomerId' : Guid}",
                        "{'Id' : int}"),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Validate()).Message);
            }

            [Fact]
            public virtual void Can_exclude_navigation_pointed_by_foreign_key_attribute_from_explicit_configuration()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Delta>();
                modelBuilder.Entity<Epsilon>().HasOne<Alpha>().WithMany(b => b.Epsilons);

                modelBuilder.Validate();

                var model = modelBuilder.Model;

                var alphaFk = model.FindEntityType(typeof(Epsilon)).FindNavigation(nameof(Epsilon.Alpha)).ForeignKey;
                Assert.Null(alphaFk.PrincipalToDependent);
                Assert.False(alphaFk.IsUnique);
                Assert.Equal(nameof(Epsilon.Id), alphaFk.Properties.First().Name);

                var epsilonFk = model.FindEntityType(typeof(Alpha)).FindNavigation(nameof(Alpha.Epsilons)).ForeignKey;
                Assert.Null(epsilonFk.DependentToPrincipal);
                Assert.False(epsilonFk.IsUnique);
                Assert.Equal(nameof(Alpha) + nameof(Alpha.Id), epsilonFk.Properties.First().Name);

                var etaFk = model.FindEntityType(typeof(Alpha)).FindNavigation(nameof(Alpha.Etas)).ForeignKey;
                Assert.Equal(nameof(Eta.Alpha), etaFk.DependentToPrincipal.Name);
                Assert.False(etaFk.IsUnique);
                Assert.Equal("Id", etaFk.Properties.First().Name);

                var kappaFk = model.FindEntityType(typeof(Alpha)).FindNavigation(nameof(Alpha.Kappas)).ForeignKey;
                Assert.Equal(nameof(Kappa.Alpha), kappaFk.DependentToPrincipal.Name);
                Assert.False(kappaFk.IsUnique);
                Assert.Equal("Id", kappaFk.Properties.First().Name);
            }

            [Fact]
            public virtual void Can_exclude_navigation_with_foreign_key_attribute_from_explicit_configuration()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Delta>();
                modelBuilder.Entity<Eta>().HasOne<Alpha>().WithMany(b => b.Etas);

                modelBuilder.Validate();

                var model = modelBuilder.Model;

                var alphaFk = model.FindEntityType(typeof(Eta)).FindNavigation(nameof(Eta.Alpha)).ForeignKey;
                Assert.Null(alphaFk.PrincipalToDependent);
                Assert.False(alphaFk.IsUnique);
                Assert.Equal(nameof(Eta.Id), alphaFk.Properties.Single().Name);

                var etasFk = model.FindEntityType(typeof(Alpha)).FindNavigation(nameof(Alpha.Etas)).ForeignKey;
                Assert.Null(etasFk.DependentToPrincipal);
                Assert.False(etasFk.IsUnique);
                Assert.NotSame(alphaFk, etasFk);
                Assert.Equal(nameof(Alpha) + nameof(Alpha.Id), etasFk.Properties.First().Name);
            }

            [Fact]
            public virtual void Can_exclude_navigation_with_foreign_key_attribute_on_principal_type_from_explicit_configuration()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Delta>();
                modelBuilder.Entity<Theta>().HasOne(e => e.Alpha).WithMany();

                modelBuilder.Validate();

                var model = modelBuilder.Model;

                var thetasFk = model.FindEntityType(typeof(Alpha)).FindNavigation(nameof(Alpha.Thetas)).ForeignKey;
                Assert.Null(thetasFk.DependentToPrincipal);
                Assert.False(thetasFk.IsUnique);
                Assert.Equal("Id", thetasFk.Properties.Single().Name);
                Assert.True(thetasFk.Properties.Single().IsShadowProperty);

                var alphaFk = model.FindEntityType(typeof(Theta)).FindNavigation(nameof(Theta.Alpha)).ForeignKey;
                Assert.Null(alphaFk.PrincipalToDependent);
                Assert.False(alphaFk.IsUnique);
                Assert.NotSame(alphaFk, thetasFk);
                Assert.Equal(nameof(Alpha) + nameof(Alpha.Id), alphaFk.Properties.First().Name);
            }

            [Fact]
            public virtual void Creates_one_to_many_relationship_with_single_ref_as_dependent_to_principal_if_no_matching_properties_either_side()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<OneToOnePrincipalEntity>(
                    b =>
                        {
                            b.Ignore(e => e.NavOneToOneDependentEntityId);
                            b.Ignore(e => e.OneToOneDependentEntityId);
                        });
                modelBuilder.Entity<OneToOneDependentEntity>(
                    b =>
                        {
                            b.Ignore(e => e.NavOneToOnePrincipalEntityId);
                            b.Ignore(e => e.OneToOnePrincipalEntityId);
                        });

                modelBuilder.Entity<OneToOneDependentEntity>().HasOne(e => e.NavOneToOnePrincipalEntity);

                modelBuilder.Validate();

                var fk = modelBuilder.Model.FindEntityType(typeof(OneToOneDependentEntity)).FindNavigation(OneToOneDependentEntity.NavigationProperty).ForeignKey;

                Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
                Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
                Assert.False(fk.IsUnique);
                Assert.Null(fk.PrincipalToDependent);
                Assert.True(fk.Properties.Single().IsShadowProperty);
            }

            [Fact]
            public virtual void Creates_one_to_many_relationship_with_single_ref_as_dependent_to_principal_if_matching_navigation_name_properties_are_on_navigation_side()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<OneToOnePrincipalEntity>(
                    b =>
                        {
                            b.Ignore(e => e.NavOneToOneDependentEntityId);
                            b.Ignore(e => e.OneToOneDependentEntityId);
                        });
                modelBuilder.Entity<OneToOneDependentEntity>(b => { b.Ignore(e => e.OneToOnePrincipalEntityId); });

                modelBuilder.Entity<OneToOneDependentEntity>().HasOne(e => e.NavOneToOnePrincipalEntity);

                modelBuilder.Validate();

                var fk = modelBuilder.Model.FindEntityType(typeof(OneToOneDependentEntity)).FindNavigation(OneToOneDependentEntity.NavigationProperty).ForeignKey;

                Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
                Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
                Assert.False(fk.IsUnique);
                Assert.Null(fk.PrincipalToDependent);
                Assert.False(fk.Properties.Single().IsShadowProperty);
                Assert.Equal(OneToOneDependentEntity.NavigationMatchingProperty.Name, fk.Properties.Single().Name);
            }

            [Fact]
            public virtual void Creates_one_to_many_relationship_with_single_ref_as_dependent_to_principal_if_matching_entity_name_properties_are_on_navigation_side()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<OneToOnePrincipalEntity>(
                    b =>
                        {
                            b.Ignore(e => e.NavOneToOneDependentEntityId);
                            b.Ignore(e => e.OneToOneDependentEntityId);
                        });
                modelBuilder.Entity<OneToOneDependentEntity>(b => { b.Ignore(e => e.NavOneToOnePrincipalEntityId); });

                modelBuilder.Entity<OneToOneDependentEntity>().HasOne(e => e.NavOneToOnePrincipalEntity);

                modelBuilder.Validate();

                var fk = modelBuilder.Model.FindEntityType(typeof(OneToOneDependentEntity)).FindNavigation(OneToOneDependentEntity.NavigationProperty).ForeignKey;

                Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
                Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
                Assert.False(fk.IsUnique);
                Assert.Null(fk.PrincipalToDependent);
                Assert.False(fk.Properties.Single().IsShadowProperty);
                Assert.Equal(OneToOneDependentEntity.EntityMatchingProperty.Name, fk.Properties.Single().Name);
            }

            [Fact]
            public virtual void Creates_one_to_many_relationship_with_single_ref_as_dependent_to_principal_if_matching_properties_are_on_both_sides()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<OneToOnePrincipalEntity>(b => { b.Ignore(e => e.NavOneToOneDependentEntityId); });
                modelBuilder.Entity<OneToOneDependentEntity>(b => { b.Ignore(e => e.NavOneToOnePrincipalEntityId); });

                modelBuilder.Entity<OneToOneDependentEntity>().HasOne(e => e.NavOneToOnePrincipalEntity);

                modelBuilder.Validate();

                var fk = modelBuilder.Model.FindEntityType(typeof(OneToOneDependentEntity)).FindNavigation(OneToOneDependentEntity.NavigationProperty).ForeignKey;

                Assert.Equal(typeof(OneToOnePrincipalEntity), fk.PrincipalEntityType.ClrType);
                Assert.Equal(typeof(OneToOneDependentEntity), fk.DeclaringEntityType.ClrType);
                Assert.False(fk.IsUnique);
                Assert.Null(fk.PrincipalToDependent);
                Assert.True(fk.Properties.Single().IsShadowProperty);
            }

            [Fact]
            public virtual void Ambiguous_relationship_candidate_does_not_block_creating_further_relationships()
            {
                var modelBuilder = CreateModelBuilder();
                var theta = modelBuilder.Entity<Theta>().Metadata;

                Assert.NotNull(theta.FindNavigation("NavTheta"));
                Assert.NotNull(theta.FindNavigation("InverseNavThetas"));
                Assert.Same(theta.FindNavigation("NavTheta").ForeignKey, theta.FindNavigation("InverseNavThetas").ForeignKey);
            }

            [Fact]
            public virtual void Shadow_property_created_for_foreign_key_is_nullable()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Customer>().HasMany(c => c.Orders).WithOne(o => o.Customer).HasForeignKey("MyShadowFk");

                Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).FindProperty("MyShadowFk").IsNullable);
                Assert.Equal(typeof(int?), modelBuilder.Model.FindEntityType(typeof(Order)).FindProperty("MyShadowFk").ClrType);
            }

            [Fact]
            public virtual void One_to_many_relationship_has_no_ambiguity_convention()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Alpha>();
                modelBuilder.Entity<Kappa>();

                Assert.Equal("KappaId", modelBuilder.Model.FindEntityType(typeof(Kappa)).FindNavigation(nameof(Kappa.Omegas)).ForeignKey.Properties.Single().Name);
            }

            [Fact]
            public virtual void One_to_many_relationship_has_no_ambiguity_explicit()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Kappa>().Ignore(e => e.Omegas);
                modelBuilder.Entity<Omega>().HasOne(e => e.Kappa).WithMany();

                modelBuilder.Validate();

                Assert.Equal("KappaId", modelBuilder.Model.FindEntityType(typeof(Omega)).FindNavigation(nameof(Omega.Kappa)).ForeignKey.Properties.Single().Name);
            }

            [Fact]
            public virtual void RemoveKey_does_not_add_back_foreign_key_pointing_to_the_same_key()
            {
                var modelBuilder = CreateModelBuilder();
                var entityTypeBuilder = modelBuilder.Entity<Alpha>();

                Assert.Equal(nameof(Alpha.Id), entityTypeBuilder.Metadata.FindPrimaryKey().Properties.Single().Name);

                entityTypeBuilder.Property(e => e.Id).IsRequired(false);

                Assert.Null(entityTypeBuilder.Metadata.FindPrimaryKey());

                entityTypeBuilder.HasKey(e => e.AnotherId);

                Assert.Equal(nameof(Alpha.AnotherId), entityTypeBuilder.Metadata.FindPrimaryKey().Properties.Single().Name);
            }

            [Fact] // Issue #3376
            public virtual void Can_use_self_referencing_overlapping_FK_PK()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<ModifierGroupHeader>()
                    .HasKey(x => new { x.GroupHeaderId, x.AccountId });

                modelBuilder.Entity<ModifierGroupHeader>()
                    .HasOne(x => x.ModifierGroupHeader2)
                    .WithMany(x => x.ModifierGroupHeader1)
                    .HasForeignKey(x => new { x.LinkedGroupHeaderId, x.AccountId });

                var contextOptions = new DbContextOptionsBuilder()
                    .UseModel(modelBuilder.Model)
                    .UseInMemoryDatabase("Can_use_self_referencing_overlapping_FK_PK")
                    .Options;

                using (var context = new DbContext(contextOptions))
                {
                    var parent = context.Add(new ModifierGroupHeader { GroupHeaderId = 77, AccountId = 90 }).Entity;
                    var child1 = context.Add(new ModifierGroupHeader { GroupHeaderId = 78, AccountId = 90 }).Entity;
                    var child2 = context.Add(new ModifierGroupHeader { GroupHeaderId = 79, AccountId = 90 }).Entity;

                    child1.ModifierGroupHeader2 = parent;
                    child2.ModifierGroupHeader2 = parent;

                    context.SaveChanges();

                    AssertGraph(parent, child1, child2);
                }

                using (var context = new DbContext(contextOptions))
                {
                    var parent = context.Set<ModifierGroupHeader>().Single(e => e.GroupHeaderId == 77);
                    var child1 = context.Set<ModifierGroupHeader>().Single(e => e.GroupHeaderId == 78);
                    var child2 = context.Set<ModifierGroupHeader>().Single(e => e.GroupHeaderId == 79);

                    AssertGraph(parent, child1, child2);
                }
            }

            private static void AssertGraph(
                ModifierGroupHeader parent,
                ModifierGroupHeader child1,
                ModifierGroupHeader child2)
            {
                Assert.Equal(new[] { child1, child2 }, parent.ModifierGroupHeader1.ToArray());
                Assert.Same(parent, child1.ModifierGroupHeader2);
                Assert.Same(parent, child2.ModifierGroupHeader2);

                Assert.Equal(77, parent.GroupHeaderId);
                Assert.Equal(78, child1.GroupHeaderId);
                Assert.Equal(79, child2.GroupHeaderId);
                Assert.Equal(90, parent.AccountId);
                Assert.Equal(90, child1.AccountId);
                Assert.Equal(90, child2.AccountId);
                Assert.Null(parent.LinkedGroupHeaderId);
                Assert.Equal(77, child1.LinkedGroupHeaderId);
                Assert.Equal(77, child2.LinkedGroupHeaderId);
            }

            [Table("ModifierGroupHeader")]
            private class ModifierGroupHeader
            {
                [Key]
                [Column(Order = 0)]
                public int GroupHeaderId { get; set; }

                [Key]
                [Column(Order = 1)]
                [DatabaseGenerated(DatabaseGeneratedOption.None)]
                public int AccountId { get; set; }

                [Required]
                [StringLength(50)]
                public string GroupBatchName { get; set; }

                [StringLength(200)]
                public string GroupBatchNameAlt { get; set; }

                public int MaxModifierSelectCount { get; set; }

                public int? LinkedGroupHeaderId { get; set; }

                public bool Enabled { get; set; }

                public DateTime CreatedDate { get; set; }

                [Required]
                [StringLength(50)]
                public string CreatedBy { get; set; }

                public DateTime ModifiedDate { get; set; }

                [Required]
                [StringLength(50)]
                public string ModifiedBy { get; set; }

                public bool? IsFollowSet { get; set; }

                public virtual ICollection<ModifierGroupHeader> ModifierGroupHeader1 { get; set; }
                    = new HashSet<ModifierGroupHeader>();

                public virtual ModifierGroupHeader ModifierGroupHeader2 { get; set; }
            }
        }
    }
}

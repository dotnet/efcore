// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class ManyToOneTestBase : ModelBuilderTestBase
        {
            [ConditionalFact]
            public virtual void Finds_existing_navigations_and_uses_associated_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>()
                    .HasMany(e => e.Orders).WithOne(e => e.Customer)
                    .HasForeignKey(e => e.CustomerId);
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<BackOrder>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));
                var fk = dependentType.GetForeignKeys().Single();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

                modelBuilder.FinalizeModel();

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { nameof(Customer.AlternateKey), principalKey.Properties.Single().Name, Customer.NameProperty.Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { nameof(Order.AnotherCustomerId), fk.Properties.Single().Name, dependentKey.Properties.Single().Name },
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

            [ConditionalFact]
            public virtual void Finds_existing_navigation_to_principal_and_uses_associated_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder
                    .Entity<Order>().HasOne(o => o.Customer).WithMany()
                    .HasForeignKey(c => c.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetNavigations().Single().ForeignKey;
                Assert.Equal(nameof(Order.CustomerId), fk.Properties.Single().Name);
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

            [ConditionalFact]
            public virtual void Finds_existing_navigation_to_dependent_and_uses_associated_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>()
                    .HasOne<Customer>().WithMany(e => e.Orders)
                    .HasForeignKey(e => e.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));
                var fk = principalType.GetNavigations().Single().ForeignKey;

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

                modelBuilder.FinalizeModel();

                var newFk = principalType.GetNavigations().Single().ForeignKey;
                AssertEqual(fk.Properties, newFk.Properties);
                Assert.Same(newFk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Equal(nameof(Order.Customer), fk.DependentToPrincipal.Name);
                Assert.Equal(nameof(Customer.Orders), fk.PrincipalToDependent.Name);
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [ConditionalFact]
            public virtual void Creates_both_navigations_and_does_not_use_existing_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>().HasOne<Customer>().WithMany().HasForeignKey(e => e.CustomerId);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));
                var principalType = model.FindEntityType(typeof(Customer));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetNavigations().Single().ForeignKey;
                Assert.Equal(nameof(Order.Customer), fk.DependentToPrincipal.Name);
                Assert.Equal(nameof(Customer.Orders), fk.PrincipalToDependent.Name);

                Assert.NotNull(dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk));
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [ConditionalFact]
            public virtual void Creates_both_navigations_and_creates_new_FK()
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

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name },
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

            [ConditionalFact]
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

                modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany();

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

            [ConditionalFact]
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

                modelBuilder.Entity<Order>().HasOne<Customer>().WithMany(e => e.Orders);

                var fk = principalType.GetNavigations().Single().ForeignKey;
                Assert.Equal(nameof(Customer.Orders), fk.PrincipalToDependent.Name);
                Assert.Null(fk.DependentToPrincipal);
                Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.NotNull(dependentType.FindForeignKeys(fkProperty).SingleOrDefault());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [ConditionalFact]
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
                var existingFk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, existingFk.Properties.Single());

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Order>().HasOne<Customer>().WithMany();

                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);

                Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey != existingFk));
                Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey != existingFk));
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[]
                    {
                        "AnotherCustomerId",
                        existingFk.Properties.Single().Name,
                        fk.Properties.Single().Name,
                        dependentKey.Properties.Single().Name
                    },
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

            [ConditionalFact]
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
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasForeignKey(e => e.CustomerId);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { "AnotherCustomerId", fk.Properties.Single().Name, dependentKey.Properties.Single().Name },
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

            [ConditionalFact]
            public virtual void Creates_both_navigations_with_existing_FK_not_found_by_convention()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BigMak>();
                modelBuilder.Entity<Pickle>().HasOne<BigMak>().WithMany()
                    .HasForeignKey(e => e.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey.DependentToPrincipal == null);

                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.FinalizeModel();

                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal(nameof(Pickle.BigMak), dependentType.GetNavigations().Single().Name);
                Assert.Equal(nameof(BigMak.Pickles), principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
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

            [ConditionalFact]
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
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [ConditionalFact]
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
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany()
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.FinalizeModel();

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

            [ConditionalFact]
            public virtual void Creates_specified_FK_with_navigation_to_dependent()
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
                    .Entity<Pickle>().HasOne<BigMak>().WithMany(e => e.Pickles)
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.FinalizeModel();

                var fk = principalType.GetNavigations().Single().ForeignKey;
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal(nameof(BigMak.Pickles), fk.PrincipalToDependent.Name);
                Assert.Null(fk.DependentToPrincipal);
                Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [ConditionalFact]
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
                    .Entity<Pickle>().HasOne<BigMak>().WithMany()
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.FinalizeModel();

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);
                Assert.Same(fkProperty, newFk.Properties.Single());

                Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey != fk));
                Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey != fk));
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fk.Properties.Single().Name, fkProperty.Name, dependentKey.Properties.Single().Name },
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

            [ConditionalFact]
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

                modelBuilder.Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.Equal("BigMakId", fkProperty.Name);
                Assert.True(fkProperty.IsShadowProperty());
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
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

            [ConditionalFact]
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

                modelBuilder.Entity<Pickle>().HasOne(e => e.BigMak).WithMany();

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetNavigations().Single().ForeignKey;
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.True(fkProperty.IsShadowProperty());
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Equal(nameof(Pickle.BigMak), dependentType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.NotSame(fk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [ConditionalFact]
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

                modelBuilder.Entity<Pickle>().HasOne<BigMak>().WithMany(e => e.Pickles);

                modelBuilder.FinalizeModel();

                var fk = principalType.GetNavigations().Single().ForeignKey;
                var fkProperty = (IProperty)fk.Properties.Single();

                Assert.True(fkProperty.IsShadowProperty());
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Equal(nameof(BigMak.Pickles), fk.PrincipalToDependent.Name);
                Assert.Null(fk.DependentToPrincipal);
                Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [ConditionalFact]
            public virtual void Creates_shadow_FK_with_no_navigations_with()
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

                var fk = dependentType.GetForeignKeys().SingleOrDefault();

                modelBuilder.Entity<Pickle>().HasOne<BigMak>().WithMany();

                modelBuilder.FinalizeModel();

                var newFk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != fk);
                var fkProperty = (IProperty)newFk.Properties.Single();

                Assert.True(fkProperty.IsShadowProperty());
                Assert.Same(typeof(int?), fkProperty.ClrType);
                Assert.Same(dependentType, fkProperty.DeclaringEntityType);

                Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey != fk));
                Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey != fk));
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fk.Properties.Single().Name, fkProperty.Name, "BurgerId", dependentKey.Properties.Single().Name },
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

            [ConditionalFact]
            public virtual void Creates_both_navigations_and_matches_shadow_FK_by_convention()
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

                modelBuilder.Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
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

            [ConditionalFact]
            public virtual void Creates_both_navigations_and_overrides_existing_FK_if_uniqueness_does_not_match()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithOne()
                    .HasForeignKey<Pickle>(c => c.BurgerId);
                modelBuilder.Ignore<Bun>();

                var dependentType = model.FindEntityType(typeof(Pickle));
                var principalType = model.FindEntityType(typeof(BigMak));

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.FinalizeModel();

                Assert.Single(dependentType.GetForeignKeys());
                var fk = (IForeignKey)dependentType.GetForeignKeys().Single();

                Assert.False(fk.IsUnique);
                Assert.Equal(nameof(Pickle.BigMak), dependentType.GetNavigations().Single().Name);
                Assert.Equal(nameof(BigMak.Pickles), principalType.GetNavigations().Single().Name);
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

            [ConditionalFact]
            public virtual void Resolves_ambiguous_navigations()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<ApplicationUser>().HasMany(e => e.Friendships).WithOne(e => e.ApplicationUser)
                    .HasForeignKey(e => e.ApplicationUserId);

                modelBuilder.FinalizeModel();

                Assert.Equal(2, model.FindEntityType(typeof(Friendship)).GetNavigations().Count());
            }

            [ConditionalFact]
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
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasPrincipalKey(e => e.Id);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name },
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

            [ConditionalFact]
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

                var principalProperty = principalType.FindProperty("AlternateKey");

                var expectedPrincipalProperties = principalType.GetProperties().ToList();
                var expectedDependentProperties = dependentType.GetProperties().ToList();

                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasPrincipalKey(e => e.AlternateKey);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);

                Assert.Empty(principalType.GetForeignKeys());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Contains(principalKey, principalType.GetKeys());
                Assert.Contains(fk.PrincipalKey, principalType.GetKeys());
                Assert.NotSame(principalKey, fk.PrincipalKey);

                Assert.Single(dependentType.GetKeys());
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

            [ConditionalFact]
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
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasForeignKey(e => e.CustomerId)
                    .HasPrincipalKey(e => e.Id);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name },
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

            [ConditionalFact]
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
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasPrincipalKey(e => e.Id)
                    .HasForeignKey(e => e.CustomerId);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("Customer", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Orders", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name, Customer.NameProperty.Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { "AnotherCustomerId", fkProperty.Name, dependentKey.Properties.Single().Name },
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

            [ConditionalFact]
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
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasForeignKey(e => e.AnotherCustomerId)
                    .HasPrincipalKey(e => e.AlternateKey);

                modelBuilder.FinalizeModel();

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

                Assert.Single(dependentType.GetKeys());
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

            [ConditionalFact]
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
                    .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                    .HasPrincipalKey(e => e.AlternateKey)
                    .HasForeignKey(e => e.AnotherCustomerId);

                modelBuilder.FinalizeModel();

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

                Assert.Single(dependentType.GetKeys());
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

            [ConditionalFact]
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
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                    .HasForeignKey(e => e.BurgerId)
                    .HasPrincipalKey(e => e.AlternateKey);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
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

            [ConditionalFact]
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
                    .Entity<Pickle>().HasOne(e => e.BigMak).WithMany(e => e.Pickles)
                    .HasPrincipalKey(e => e.AlternateKey)
                    .HasForeignKey(e => e.BurgerId);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty, fk.Properties.Single());
                Assert.Same(principalProperty, fk.PrincipalKey.Properties.Single());

                Assert.Equal("BigMak", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Pickles", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey", principalKey.Properties.Single().Name }, principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fkProperty.Name, dependentKey.Properties.Single().Name }, dependentType.GetProperties().Select(p => p.Name));
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

            [ConditionalFact]
            public virtual void Creates_both_navigations_and_finds_existing_composite_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(
                    c => new { c.Id1, c.Id2 });
                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany()
                    .HasForeignKey(
                        c => new { c.BurgerId1, c.BurgerId2 });
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Mustard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                    .HasForeignKey(
                        e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fk, dependentType.GetForeignKeys().Single());
                Assert.Equal(nameof(Tomato.Whoopper), dependentType.GetNavigations().Single().Name);
                Assert.Equal(nameof(Whoopper.Tomatoes), principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name },
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

            [ConditionalFact]
            public virtual void Creates_both_navigations_and_creates_composite_FK_specified()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(
                    c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Mustard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.FindProperty("BurgerId1");
                var fkProperty2 = dependentType.FindProperty("BurgerId2");

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.FindPrimaryKey();

                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                    .HasForeignKey(
                        e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Tomatoes", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name },
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

            [ConditionalFact]
            public virtual void Can_use_alternate_composite_key()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>(
                    b => b.HasKey(
                        c => new { c.Id1, c.Id2 }));
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Mustard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.FindProperty("BurgerId1");
                var fkProperty2 = dependentType.FindProperty("BurgerId2");
                var principalProperty1 = principalType.FindProperty("AlternateKey1");
                var principalProperty2 = principalType.FindProperty("AlternateKey2");

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                    .HasForeignKey(
                        e => new { e.BurgerId1, e.BurgerId2 })
                    .HasPrincipalKey(
                        e => new { e.AlternateKey1, e.AlternateKey2 });

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);
                Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
                Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

                Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Tomatoes", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name },
                    dependentType.GetProperties().Select(p => p.Name));
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

            [ConditionalFact]
            public virtual void Can_use_alternate_composite_key_in_any_order()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>(
                    b => b.HasKey(
                        c => new { c.Id1, c.Id2 }));
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Mustard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.FindProperty("BurgerId1");
                var fkProperty2 = dependentType.FindProperty("BurgerId2");
                var principalProperty1 = principalType.FindProperty("AlternateKey1");
                var principalProperty2 = principalType.FindProperty("AlternateKey2");

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.FindPrimaryKey();

                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany(e => e.Tomatoes)
                    .HasPrincipalKey(
                        e => new { e.AlternateKey1, e.AlternateKey2 })
                    .HasForeignKey(
                        e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);
                Assert.Same(principalProperty1, fk.PrincipalKey.Properties[0]);
                Assert.Same(principalProperty2, fk.PrincipalKey.Properties[1]);

                Assert.Equal("Whoopper", dependentType.GetNavigations().Single().Name);
                Assert.Equal("Tomatoes", principalType.GetNavigations().Single().Name);
                Assert.Same(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single().ForeignKey);
                AssertEqual(
                    new[] { "AlternateKey1", "AlternateKey2", principalKey.Properties[0].Name, principalKey.Properties[1].Name },
                    principalType.GetProperties().Select(p => p.Name));
                AssertEqual(
                    new[] { fk.Properties[0].Name, fk.Properties[1].Name, dependentKey.Properties.Single().Name },
                    dependentType.GetProperties().Select(p => p.Name));
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

            [ConditionalFact]
            public virtual void Creates_specified_composite_FK_with_navigation_to_principal()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(
                    c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Mustard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.FindProperty(nameof(Tomato.BurgerId1));
                var fkProperty2 = dependentType.FindProperty(nameof(Tomato.BurgerId2));

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Tomato>().HasOne(e => e.Whoopper).WithMany()
                    .HasForeignKey(
                        e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetNavigations().Single().ForeignKey;
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

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

            [ConditionalFact]
            public virtual void Creates_specified_composite_FK_with_navigation_to_dependent()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(
                    c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Mustard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var fkProperty1 = dependentType.FindProperty(nameof(Tomato.BurgerId1));
                var fkProperty2 = dependentType.FindProperty(nameof(Tomato.BurgerId2));

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Tomato>().HasOne<Whoopper>().WithMany(e => e.Tomatoes)
                    .HasForeignKey(
                        e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.FinalizeModel();

                var fk = principalType.GetNavigations().Single().ForeignKey;
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Equal(nameof(Whoopper.Tomatoes), fk.PrincipalToDependent.Name);
                Assert.Null(fk.DependentToPrincipal);
                Assert.NotSame(fk, dependentType.GetNavigations().Single().ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [ConditionalFact]
            public virtual void Creates_relationship_with_no_navigations_and_specified_composite_FK()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(
                    c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Whoopper>().HasMany(w => w.Tomatoes).WithOne(t => t.Whoopper);
                modelBuilder.Entity<Tomato>();
                modelBuilder.Ignore<ToastedBun>();
                modelBuilder.Ignore<Mustard>();

                var dependentType = model.FindEntityType(typeof(Tomato));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var existingFk = dependentType.GetForeignKeys().SingleOrDefault();

                var expectedPrincipalProperties = principalType.GetProperties().ToList();
                var expectedDependentProperties = dependentType.GetProperties().ToList();

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder
                    .Entity<Tomato>().HasOne<Whoopper>().WithMany()
                    .HasForeignKey(
                        e => new { e.BurgerId1, e.BurgerId2 });

                modelBuilder.FinalizeModel();

                var fkProperty1 = dependentType.FindProperty("BurgerId1");
                var fkProperty2 = dependentType.FindProperty("BurgerId2");
                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
                Assert.Same(fkProperty1, fk.Properties[0]);
                Assert.Same(fkProperty2, fk.Properties[1]);

                Assert.Empty(dependentType.GetNavigations().Where(nav => nav.ForeignKey != existingFk));
                Assert.Empty(principalType.GetNavigations().Where(nav => nav.ForeignKey != existingFk));
                AssertEqual(expectedPrincipalProperties, principalType.GetProperties());
                AssertEqual(expectedDependentProperties, dependentType.GetProperties());
                Assert.Empty(principalType.GetForeignKeys());
                Assert.Equal(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
                Assert.Empty(principalType.GetIndexes());
            }

            [ConditionalFact]
            public virtual void Creates_relationship_on_existing_FK_is_using_different_principal_key()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(
                    c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                    .HasForeignKey<ToastedBun>(
                        e => new { e.BurgerId1, e.BurgerId2 });
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Mustard>();

                var dependentType = model.FindEntityType(typeof(ToastedBun));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.FindPrimaryKey();

                modelBuilder
                    .Entity<ToastedBun>().HasOne<Whoopper>().WithMany()
                    .HasForeignKey(
                        e => new { e.BurgerId1, e.BurgerId2 })
                    .HasPrincipalKey(
                        e => new { e.AlternateKey1, e.AlternateKey2 });

                var navigation = dependentType.GetNavigations().Single();
                var existingFk = navigation.ForeignKey;
                Assert.Same(existingFk, principalType.GetNavigations().Single().ForeignKey);
                Assert.Equal(nameof(ToastedBun.Whoopper), navigation.Name);
                Assert.Equal(nameof(Whoopper.ToastedBun), navigation.FindInverse().Name);
                Assert.Equal(existingFk.DeclaringEntityType == dependentType ? 0 : 1, principalType.GetForeignKeys().Count());
                Assert.Equal(2, principalType.GetKeys().Count());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());

                var fk = dependentType.GetForeignKeys().Single(foreignKey => foreignKey != existingFk);
                Assert.NotSame(principalKey, fk.PrincipalKey);
                Assert.Equal(principalType.GetForeignKeys().Count(), principalType.GetIndexes().Count());
                Assert.Equal(dependentType.GetForeignKeys().Count(), dependentType.GetIndexes().Count());
                Assert.True(existingFk.DeclaringEntityType.FindIndex(existingFk.Properties).IsUnique);
                Assert.False(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);

                Assert.Equal(
                    CoreStrings.AmbiguousOneToOneRelationship(
                        existingFk.DeclaringEntityType.DisplayName() + "." + existingFk.DependentToPrincipal.Name,
                        existingFk.PrincipalEntityType.DisplayName() + "." + existingFk.PrincipalToDependent.Name),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
            }

            [ConditionalFact]
            public virtual void Creates_relationship_on_existing_FK_is_using_different_principal_key_different_order()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Whoopper>().HasKey(
                    c => new { c.Id1, c.Id2 });
                modelBuilder.Entity<Whoopper>().HasOne(e => e.ToastedBun).WithOne(e => e.Whoopper)
                    .HasForeignKey<ToastedBun>(
                        e => new { e.BurgerId1, e.BurgerId2 });
                modelBuilder.Ignore<Tomato>();
                modelBuilder.Ignore<Mustard>();

                var dependentType = model.FindEntityType(typeof(ToastedBun));
                var principalType = model.FindEntityType(typeof(Whoopper));

                var principalKey = principalType.FindPrimaryKey();
                var dependentKey = dependentType.FindPrimaryKey();

                modelBuilder
                    .Entity<ToastedBun>().HasOne<Whoopper>().WithMany()
                    .HasPrincipalKey(
                        e => new { e.AlternateKey1, e.AlternateKey2 })
                    .HasForeignKey(
                        e => new { e.BurgerId1, e.BurgerId2 });

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
                Assert.Empty(principalType.GetIndexes());
                Assert.Single(dependentType.GetIndexes());
                Assert.True(fk.DeclaringEntityType.FindIndex(fk.Properties).IsUnique);
            }

            [ConditionalFact]
            public virtual void Throws_on_existing_one_to_one_relationship()
            {
                var modelBuilder = HobNobBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Nob>()
                    .HasOne(e => e.Hob).WithOne(e => e.Nob)
                    .HasForeignKey<Nob>(
                        e => new { e.HobId1, e.HobId2 });

                var dependentType = model.FindEntityType(typeof(Hob));
                var principalType = model.FindEntityType(typeof(Nob));

                Assert.Equal(
                    CoreStrings.ConflictingRelationshipNavigation(
                        principalType.DisplayName(),
                        nameof(Nob.Hobs),
                        dependentType.DisplayName(),
                        nameof(Hob.Nob),
                        principalType.DisplayName(),
                        nameof(Nob.Hob),
                        dependentType.DisplayName(),
                        nameof(Hob.Nob)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            modelBuilder.Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs)).Message);
            }

            [ConditionalFact]
            public virtual void Removes_existing_unidirectional_one_to_one_relationship()
            {
                var modelBuilder = HobNobBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Nob>()
                    .HasOne(e => e.Hob).WithOne(e => e.Nob)
                    .HasForeignKey<Nob>(
                        e => new { e.HobId1, e.HobId2 });

                modelBuilder.Entity<Nob>().HasOne<Hob>().WithOne(e => e.Nob);

                var dependentType = model.FindEntityType(typeof(Hob));
                var principalType = model.FindEntityType(typeof(Nob));
                var principalKey = principalType.GetKeys().Single();
                var dependentKey = dependentType.GetKeys().Single();

                modelBuilder.Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs);

                modelBuilder.FinalizeModel();

                var fk = dependentType.GetForeignKeys().Single();
                Assert.False(fk.IsUnique);
                Assert.Same(fk, dependentType.GetNavigations().Single(n => n.Name == nameof(Hob.Nob)).ForeignKey);
                Assert.Same(fk, principalType.GetNavigations().Single(n => n.Name == nameof(Nob.Hobs)).ForeignKey);
                Assert.Same(principalKey, principalType.GetKeys().Single());
                Assert.Same(dependentKey, dependentType.GetKeys().Single());
                Assert.Same(principalKey, principalType.FindPrimaryKey());
                Assert.Same(dependentKey, dependentType.FindPrimaryKey());

                var oldFk = principalType.GetForeignKeys().Single();
                AssertEqual(new[] { nameof(Nob.HobId1), nameof(Nob.HobId2) }, oldFk.Properties.Select(p => p.Name));
                Assert.Same(oldFk, dependentType.GetNavigations().Single(n => n.Name == nameof(Hob.Nobs)).ForeignKey);
                Assert.Same(oldFk, principalType.GetNavigations().Single(n => n.Name == nameof(Nob.Hob)).ForeignKey);
                Assert.False(oldFk.IsUnique);
            }

            [ConditionalFact]
            public virtual void Can_add_annotations()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();
                modelBuilder.Entity<Order>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();

                var dependentType = model.FindEntityType(typeof(Order));

                var builder = modelBuilder.Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders);
                builder = builder.HasAnnotation("Fus", "Ro");

                var fk = dependentType.GetForeignKeys().Single();
                Assert.Same(fk, builder.Metadata);
                Assert.Equal("Ro", fk["Fus"]);
            }

            [ConditionalFact]
            public virtual void Nullable_FK_are_optional_by_default()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                    .HasForeignKey(
                        e => new { e.HobId1, e.HobId2 });

                modelBuilder.FinalizeModel();

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

            [ConditionalFact]
            public virtual void Non_nullable_FK_are_required_by_default()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs)
                    .HasForeignKey(
                        e => new { e.NobId1, e.NobId2 });

                modelBuilder.FinalizeModel();

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

            [ConditionalFact]
            public virtual void Nullable_FK_can_be_made_required()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                    .HasForeignKey(
                        e => new { e.HobId1, e.HobId2 })
                    .IsRequired();

                modelBuilder.FinalizeModel();

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

            [ConditionalFact]
            public virtual void Non_nullable_FK_can_be_made_optional()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs)
                    .HasForeignKey(
                        e => new { e.NobId1, e.NobId2 })
                    .IsRequired(false);

                modelBuilder.FinalizeModel();

                var entityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
                var fk = entityType.GetForeignKeys().Single();
                Assert.False(fk.IsRequired);
                var fkProperty1 = entityType.FindProperty(nameof(Hob.NobId1));
                var fkProperty2 = entityType.FindProperty(nameof(Hob.NobId2));
                Assert.False(fkProperty1.IsNullable);
                Assert.False(fkProperty2.IsNullable);
                Assert.Contains(fkProperty1, fk.Properties);
                Assert.Contains(fkProperty2, fk.Properties);
            }

            [ConditionalFact]
            public virtual void Non_nullable_FK_can_be_made_optional_separately()
            {
                var modelBuilder = HobNobBuilder();

                modelBuilder
                    .Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs)
                    .HasForeignKey(
                        e => new { e.NobId1, e.NobId2 });

                modelBuilder
                    .Entity<Hob>().HasOne(e => e.Nob).WithMany(e => e.Hobs)
                    .IsRequired(false);

                modelBuilder.FinalizeModel();

                var entityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Hob));
                var fk = entityType.GetForeignKeys().Single();
                Assert.False(fk.IsRequired);
                var fkProperty1 = entityType.FindProperty(nameof(Hob.NobId1));
                var fkProperty2 = entityType.FindProperty(nameof(Hob.NobId2));
                Assert.False(fkProperty1.IsNullable);
                Assert.False(fkProperty2.IsNullable);
                Assert.Contains(fkProperty1, fk.Properties);
                Assert.Contains(fkProperty2, fk.Properties);
            }

            [ConditionalFact]
            public virtual void Can_change_delete_behavior()
            {
                var modelBuilder = HobNobBuilder();
                var dependentType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(Nob));

                modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                    .OnDelete(DeleteBehavior.Cascade);

                Assert.Equal(DeleteBehavior.Cascade, dependentType.GetForeignKeys().Single().DeleteBehavior);

                modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                    .OnDelete(DeleteBehavior.Restrict);

                Assert.Equal(DeleteBehavior.Restrict, dependentType.GetForeignKeys().Single().DeleteBehavior);

                modelBuilder
                    .Entity<Nob>().HasOne(e => e.Hob).WithMany(e => e.Nobs)
                    .OnDelete(DeleteBehavior.SetNull);

                Assert.Equal(DeleteBehavior.SetNull, dependentType.GetForeignKeys().Single().DeleteBehavior);
            }

            [ConditionalFact]
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

            [ConditionalFact]
            public virtual void Creates_shadow_property_for_foreign_key_according_to_navigation_to_principal_name_when_present()
            {
                var modelBuilder = CreateModelBuilder();
                var beta = modelBuilder.Entity<Beta>().Metadata;

                Assert.Equal("FirstNavId", beta.FindNavigation("FirstNav").ForeignKey.Properties.First().Name);
                Assert.Equal("SecondNavId", beta.FindNavigation("SecondNav").ForeignKey.Properties.First().Name);
            }

            [ConditionalFact]
            public virtual void
                Creates_shadow_property_for_foreign_key_according_to_target_type_when_navigation_to_principal_name_not_present()
            {
                var modelBuilder = CreateModelBuilder();
                var gamma = modelBuilder.Entity<Gamma>().Metadata;

                Assert.Equal("GammaId", gamma.FindNavigation("Alphas").ForeignKey.Properties.First().Name);
            }

            [ConditionalFact]
            public virtual void Creates_shadow_FK_property_with_non_shadow_PK()
            {
                var modelBuilder = CreateModelBuilder();

                // For NonGenericStringTest
                modelBuilder.Entity<Beta>();

                modelBuilder.Entity<Alpha>(
                    b =>
                    {
                        b.HasMany<Beta>()
                            .WithOne(e => e.FirstNav)
                            .HasForeignKey("ShadowId");
                    });

                modelBuilder.FinalizeModel();

                Assert.Equal(
                    "ShadowId",
                    modelBuilder.Model.FindEntityType(typeof(Beta)).FindNavigation("FirstNav").ForeignKey.Properties.Single().Name);
            }

            [ConditionalFact]
            public virtual void Creates_shadow_FK_property_with_shadow_PK()
            {
                var modelBuilder = CreateModelBuilder();

                // For NonGenericStringTest
                modelBuilder.Entity<Beta>();

                var entityA = modelBuilder.Entity<Alpha>();
                entityA.Property<int>("ShadowPK");
                entityA.HasKey("ShadowPK");

                entityA.HasMany<Beta>().WithOne(e => e.FirstNav)
                    .HasForeignKey("ShadowId");

                modelBuilder.FinalizeModel();

                Assert.Equal(
                    "ShadowId",
                    modelBuilder.Model.FindEntityType(typeof(Beta)).FindNavigation("FirstNav").ForeignKey.Properties.Single().Name);
            }

            [ConditionalFact]
            public virtual void Handles_identity_correctly_while_removing_navigation()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<Delta>();
                modelBuilder.Entity<Alpha>().HasMany(b => b.Epsilons).WithOne();

                modelBuilder.FinalizeModel();

                var property = modelBuilder.Model.FindEntityType(typeof(Epsilon)).FindProperty("Id");
                Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
            }

            [ConditionalFact]
            public virtual void One_to_many_relationship_has_no_ambiguity_explicit()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Kappa>().Ignore(e => e.Omegas);
                modelBuilder.Entity<Kappa>().HasMany<Omega>().WithOne(e => e.Kappa);

                modelBuilder.FinalizeModel();

                Assert.Equal(
                    "KappaId",
                    modelBuilder.Model.FindEntityType(typeof(Omega)).FindNavigation(nameof(Omega.Kappa)).ForeignKey.Properties.Single()
                        .Name);
            }
        }
    }
}

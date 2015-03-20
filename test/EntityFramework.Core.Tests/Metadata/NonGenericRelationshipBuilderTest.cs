// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    // TODO: Fix when merging with ModelBuilderTest
    // Issue #1102
    internal class NonGenericRelationshipBuilderTest
    {
        [Fact]
        public void OneToMany_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navToPrincipal = dependentType.AddNavigation("Customer", fk, true);
            var navToDependent = principalType.AddNavigation("Orders", fk, false);

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = dependentType.AddNavigation("Customer", fk, true);

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navigation, dependentType.Navigations.Single());
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = principalType.AddNavigation("Orders", fk, false);

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne(null);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            // Passing null as the first arg is not super-compelling, but it is consistent
            modelBuilder.Entity(typeof(Customer)).HasMany(typeof(Order), null).WithOne("Customer");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasMany(typeof(Order), null).WithOne(null);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer")
                .ForeignKey("CustomerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Pickle)).GetProperty("BurgerId"),
                model.GetEntityType(typeof(BigMak)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasMany(typeof(Pickle), "Pickles").WithOne("BigMak")
                .ForeignKey("BurgerId");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasMany(typeof(Pickle), "Pickles").WithOne("BigMak")
                .ForeignKey("BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasMany(typeof(Pickle), "Pickles").WithOne(null)
                .ForeignKey("BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasMany(typeof(Pickle), null).WithOne("BigMak")
                .ForeignKey("BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasMany(typeof(Pickle), null).WithOne(null)
                .ForeignKey("BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(BigMak)).HasMany(typeof(Pickle), "Pickles").WithOne("BigMak");

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(BigMak)).HasMany(typeof(Pickle), "Pickles").WithOne(null);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(BigMak)).HasMany(typeof(Pickle), null).WithOne("BigMak");

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(BigMak)).HasMany(typeof(Pickle), null).WithOne(null);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_matches_shadow_FK_property_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property<int>("BigMakId");

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var fkProperty = dependentType.GetProperty("BigMakId");

            modelBuilder.Entity(typeof(BigMak)).HasMany(typeof(Pickle), "Pickles").WithOne("BigMak");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_new_FK_when_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Pickle)).GetProperty("BurgerId"),
                model.GetEntityType(typeof(BigMak)).GetPrimaryKey()).IsUnique = true;

            var dependentType = (IEntityType)model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasMany(typeof(Pickle), "Pickles").WithOne("BigMak")
                .ForeignKey("BurgerId");

            Assert.Equal(2, dependentType.ForeignKeys.Count);
            var newFk = dependentType.ForeignKeys.Single(k => k != fk);

            Assert.True(fk.IsUnique);
            Assert.False(newFk.IsUnique);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_explicitly_specified_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer")
                .ReferencedKey("Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_non_PK_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer")
                .ReferencedKey("AlternateKey");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_both_convention_properties_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer")
                .ForeignKey("CustomerId")
                .ReferencedKey("Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_both_convention_properties_specified_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer")
                .ReferencedKey("Id")
                .ForeignKey("CustomerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_FK_by_convention_specified_with_explicit_principal_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer")
                .ForeignKey("CustomerId")
                .ReferencedKey("AlternateKey");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Customer)).HasMany(typeof(Order), "Orders").WithOne("Customer")
                .ReferencedKey("AlternateKey")
                .ForeignKey("CustomerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_principal_key_by_convention_specified_with_explicit_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasMany(typeof(Pickle), "Pickles").WithOne("BigMak")
                .ForeignKey("BurgerId")
                .ReferencedKey("AlternateKey");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasMany(typeof(Pickle), "Pickles").WithOne("BigMak")
                .ReferencedKey("AlternateKey")
                .ForeignKey("BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navToPrincipal = dependentType.AddNavigation("Customer", fk, true);
            var navToDependent = principalType.AddNavigation("Orders", fk, false);

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = dependentType.AddNavigation("Customer", fk, true);

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navigation, dependentType.Navigations.Single());
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = principalType.AddNavigation("Orders", fk, false);

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_existing_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Order)).GetProperty("CustomerId"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            // Passing null as the first arg is not super-compelling, but it is consistent
            modelBuilder.Entity(typeof(Order)).HasOne(typeof(Customer), null).WithMany("Orders");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany(null);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Order)).HasOne(typeof(Customer), null).WithMany(null);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders")
                .ForeignKey("CustomerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Pickle)).GetProperty("BurgerId"),
                model.GetEntityType(typeof(BigMak)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Pickle)).HasOne(typeof(BigMak), "BigMak").WithMany("Pickles")
                .ForeignKey("BurgerId");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Pickle)).HasOne(typeof(BigMak), "BigMak").WithMany("Pickles")
                .ForeignKey("BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Pickle)).HasOne(typeof(BigMak), null).WithMany("Pickles")
                .ForeignKey("BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Pickle)).HasOne(typeof(BigMak), "BigMak").WithMany(null)
                .ForeignKey("BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Pickle)).HasOne(typeof(BigMak), null).WithMany(null)
                .ForeignKey("BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Pickle)).HasOne(typeof(BigMak), "BigMak").WithMany("Pickles");

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Pickle)).HasOne(typeof(BigMak), null).WithMany("Pickles");

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Pickle)).HasOne(typeof(BigMak), "BigMak").WithMany(null);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Pickle)).HasOne(typeof(BigMak), null).WithMany(null);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_matches_shadow_FK_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property<int>("BigMakId");

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var fkProperty = dependentType.GetProperty("BigMakId");

            modelBuilder.Entity(typeof(Pickle)).HasOne(typeof(BigMak), "BigMak").WithMany("Pickles");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_new_FK_if_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Pickle)).GetProperty("BurgerId"),
                model.GetEntityType(typeof(BigMak)).GetPrimaryKey()).IsUnique = true;

            var dependentType = (IEntityType)model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Pickle)).HasOne(typeof(BigMak), "BigMak").WithMany("Pickles")
                .ForeignKey("BurgerId");

            Assert.Equal(2, dependentType.ForeignKeys.Count);
            var newFk = dependentType.ForeignKeys.Single(k => k != fk);

            Assert.True(fk.IsUnique);
            Assert.False(newFk.IsUnique);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_explicitly_specified_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders")
                .ReferencedKey("Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_non_PK_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders")
                .ReferencedKey("AlternateKey");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_both_convention_properties_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders")
                .ForeignKey("CustomerId")
                .ReferencedKey("Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_both_convention_properties_specified_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders")
                .ReferencedKey("Id")
                .ForeignKey("CustomerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_FK_by_convention_specified_with_explicit_principal_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders")
                .ForeignKey("CustomerId")
                .ReferencedKey("AlternateKey");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Order)).HasOne(typeof(Customer), "Customer").WithMany("Orders")
                .ReferencedKey("AlternateKey")
                .ForeignKey("CustomerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_principal_key_by_convention_specified_with_explicit_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Pickle)).HasOne(typeof(BigMak), "BigMak").WithMany("Pickles")
                .ForeignKey("BurgerId")
                .ReferencedKey("AlternateKey");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Pickle)).HasOne(typeof(BigMak), "BigMak").WithMany("Pickles")
                .ReferencedKey("AlternateKey")
                .ForeignKey("BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(CustomerDetails)).GetProperty("Id"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var navToPrincipal = dependentType.AddNavigation("Customer", fk, true);
            var navToDependent = principalType.AddNavigation("Details", fk, false);

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), "Details").WithOne("Customer");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(CustomerDetails)).GetProperty("Id"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var navigation = dependentType.AddNavigation("Customer", fk, true);

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), "Details").WithOne("Customer");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navigation, dependentType.Navigations.Single());
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(CustomerDetails)).GetProperty("Id"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var navigation = principalType.AddNavigation("Details", fk, false);

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), "Details").WithOne("Customer");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);
            modelBuilder.Entity<CustomerDetails>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(CustomerDetails)).GetProperty("Id"),
                model.GetEntityType(typeof(Customer)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), "Details").WithOne("Customer");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), "Details").WithOne("Customer");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_FK_when_not_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>().Key(e => e.Id);
            modelBuilder.Entity<OrderDetails>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(OrderDetails)).GetProperty("Id"),
                model.GetEntityType(typeof(Order)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Order)).HasOne(typeof(OrderDetails), "Details").WithOne("Order");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_new_FK_when_not_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Order)).HasOne(typeof(OrderDetails), "Details").WithOne("Order");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), "Details").WithOne(null);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            // Passing null as the first arg is not super-compelling, but it is consistent
            modelBuilder.Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), null).WithOne("Customer");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), null).WithOne(null);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Order)).HasOne(typeof(OrderDetails), "Details").WithOne("Order")
                .ForeignKey(typeof(OrderDetails), "OrderId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_specified_FK_even_if_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), "Details").WithOne("Customer")
                .ForeignKey(typeof(CustomerDetails), "Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Bun>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Bun)).GetProperty("BurgerId"),
                model.GetEntityType(typeof(BigMak)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasOne(typeof(Bun), "Bun").WithOne("BigMak")
                .ForeignKey(typeof(Bun), "BurgerId");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasOne(typeof(Bun), "Bun").WithOne("BigMak")
                .ForeignKey(typeof(Bun), "BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasOne(typeof(Bun), "Bun").WithOne(null)
                .ForeignKey(typeof(Bun), "BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasOne(typeof(Bun), null).WithOne("BigMak")
                .ForeignKey(typeof(Bun), "BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasOne(typeof(Bun), null).WithOne(null)
                .ForeignKey(typeof(Bun), "BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_new_FK_when_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Bun>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(Bun)).GetProperty("BurgerId"),
                model.GetEntityType(typeof(BigMak)).GetPrimaryKey());

            var dependentType = (IEntityType)model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasOne(typeof(Bun), "Bun").WithOne("BigMak")
                .ForeignKey(typeof(Bun), "BurgerId");

            Assert.Equal(2, dependentType.ForeignKeys.Count);
            var newFk = dependentType.ForeignKeys.Single(k => k != fk);

            Assert.False(fk.IsUnique);
            Assert.True(newFk.IsUnique);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount + 1, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_with_existing_FK_still_used()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>().Key(e => e.Id);
            modelBuilder.Entity<OrderDetails>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(OrderDetails)).GetProperty("Id"),
                model.GetEntityType(typeof(Order)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(OrderDetails)).HasOne(typeof(Order), "Order").WithOne("Details")
                .ForeignKey(typeof(OrderDetails), "Id");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_with_FK_still_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(OrderDetails)).HasOne(typeof(Order), "Order").WithOne("Details")
                .ForeignKey(typeof(OrderDetails), "OrderId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_from_other_end_OneToOne_principal_and_dependent_can_be_flipped()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(typeof(Customer), "Customer").WithOne(null)
                .ForeignKey(typeof(CustomerDetails), "Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_OneToOne_principal_and_dependent_can_be_flipped()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(typeof(Customer), null).WithOne("Details")
                .ForeignKey(typeof(CustomerDetails), "Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void No_navigation_OneToOne_principal_and_dependent_can_be_flipped()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(typeof(Customer), null).WithOne(null)
                .ForeignKey(typeof(CustomerDetails), "Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_with_PK_FK_still_used()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(typeof(Customer), "Customer").WithOne("Details")
                .ForeignKey(typeof(CustomerDetails), "Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_PK_explicitly_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), "Details").WithOne("Customer")
                .ReferencedKey(typeof(Customer), "Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_use_alternate_principal_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Customer)).HasOne(typeof(CustomerDetails), "Details").WithOne("Customer")
                .ReferencedKey(typeof(Customer), "AlternateKey");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_convention_keys_specified_explicitly()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");
            var principalProperty = principalType.GetProperty("OrderId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Order)).HasOne(typeof(OrderDetails), "Details").WithOne("Order")
                .ForeignKey(typeof(OrderDetails), "OrderId")
                .ReferencedKey(typeof(Order), "OrderId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_convention_keys_specified_explicitly_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");
            var principalProperty = principalType.GetProperty("OrderId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Order)).HasOne(typeof(OrderDetails), "Details").WithOne("Order")
                .ReferencedKey(typeof(Order), "OrderId")
                .ForeignKey(typeof(OrderDetails), "OrderId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_keys_specified_explicitly()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasOne(typeof(Bun), "Bun").WithOne("BigMak")
                .ForeignKey(typeof(Bun), "BurgerId")
                .ReferencedKey(typeof(BigMak), "AlternateKey");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_keys_specified_explicitly_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(BigMak)).HasOne(typeof(Bun), "Bun").WithOne("BigMak")
                .ReferencedKey(typeof(BigMak), "AlternateKey")
                .ForeignKey(typeof(Bun), "BurgerId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_using_principal_with_existing_FK_still_used()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>().Key(e => e.Id);
            modelBuilder.Entity<OrderDetails>().Metadata.AddForeignKey(
                model.GetEntityType(typeof(OrderDetails)).GetProperty("Id"),
                model.GetEntityType(typeof(Order)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(OrderDetails)).HasOne(typeof(Order), "Order").WithOne("Details")
                .ReferencedKey(typeof(Order), "OrderId");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_using_principal_with_FK_still_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(OrderDetails)).HasOne(typeof(Order), "Order").WithOne("Details")
                .ReferencedKey(typeof(Order), "OrderId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_in_both_ways()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(OrderDetails)).HasOne(typeof(Order), "Order").WithOne("Details")
                .ForeignKey(typeof(OrderDetails), "OrderId")
                .ReferencedKey(typeof(Order), "OrderId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_in_both_ways_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(OrderDetails)).HasOne(typeof(Order), "Order").WithOne("Details")
                .ReferencedKey(typeof(Order), "OrderId")
                .ForeignKey(typeof(OrderDetails), "OrderId");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_from_other_end_OneToOne_principal_and_dependent_can_be_flipped_using_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(typeof(Customer), "Customer").WithOne(null)
                .ReferencedKey(typeof(Customer), "Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_OneToOne_principal_and_dependent_can_be_flipped_using_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(typeof(Customer), null).WithOne("Details")
                .ReferencedKey(typeof(Customer), "Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void No_navigation_OneToOne_principal_and_dependent_can_be_flipped_using_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>().HasOne(typeof(Customer), null).WithOne(null)
                .ReferencedKey(typeof(Customer), "Id");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        private class BigMak
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }

            public IEnumerable<Pickle> Pickles { get; set; }

            public Bun Bun { get; set; }
        }

        private class Pickle
        {
            public int Id { get; set; }

            public int BurgerId { get; set; }
            public BigMak BigMak { get; set; }
        }

        private class Bun
        {
            public int Id { get; set; }

            public int BurgerId { get; set; }
            public BigMak BigMak { get; set; }
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            var tomatoType = model.GetEntityType(typeof(Tomato));
            modelBuilder.Entity<Tomato>().Metadata.AddForeignKey(
                new[] { tomatoType.GetProperty("BurgerId1"), tomatoType.GetProperty("BurgerId2") },
                model.GetEntityType(typeof(Whoopper)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasMany(typeof(Tomato), "Tomatoes").WithOne("Whoopper")
                .ForeignKey("BurgerId1", "BurgerId2");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasMany(typeof(Tomato), "Tomatoes").WithOne("Whoopper")
                .ForeignKey("BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasMany(typeof(Tomato), "Tomatoes").WithOne("Whoopper")
                .ForeignKey("BurgerId1", "BurgerId2")
                .ReferencedKey("AlternateKey1", "AlternateKey2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasMany(typeof(Tomato), "Tomatoes").WithOne("Whoopper")
                .ReferencedKey("AlternateKey1", "AlternateKey2")
                .ForeignKey("BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasMany(typeof(Tomato), "Tomatoes").WithOne(null)
                .ForeignKey("BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasMany(typeof(Tomato), null).WithOne("Whoopper")
                .ForeignKey("BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasMany(typeof(Tomato), null).WithOne(null)
                .ForeignKey("BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            var tomatoType = model.GetEntityType(typeof(Tomato));
            modelBuilder.Entity<Tomato>().Metadata.AddForeignKey(
                new[] { tomatoType.GetProperty("BurgerId1"), tomatoType.GetProperty("BurgerId2") },
                model.GetEntityType(typeof(Whoopper)).GetPrimaryKey());

            var dependentType = tomatoType;
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Tomato)).HasOne(typeof(Whoopper), "Whoopper").WithMany("Tomatoes")
                .ForeignKey("BurgerId1", "BurgerId2");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Tomato)).HasOne(typeof(Whoopper), "Whoopper").WithMany("Tomatoes")
                .ForeignKey("BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Tomato)).HasOne(typeof(Whoopper), "Whoopper").WithMany("Tomatoes")
                .ForeignKey("BurgerId1", "BurgerId2")
                .ReferencedKey("AlternateKey1", "AlternateKey2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Tomato)).HasOne(typeof(Whoopper), "Whoopper").WithMany("Tomatoes")
                .ReferencedKey("AlternateKey1", "AlternateKey2")
                .ForeignKey("BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Tomato)).HasOne(typeof(Whoopper), null).WithMany("Tomatoes")
                .ForeignKey("BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Tomato)).HasOne(typeof(Whoopper), "Whoopper").WithMany(null)
                .ForeignKey("BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Tomato)).HasOne(typeof(Whoopper), null).WithMany(null)
                .ForeignKey("BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            var bunType = model.GetEntityType(typeof(ToastedBun));
            modelBuilder.Entity<ToastedBun>().Metadata.AddForeignKey(
                new[] { bunType.GetProperty("BurgerId1"), bunType.GetProperty("BurgerId2") },
                model.GetEntityType(typeof(Whoopper)).GetPrimaryKey());

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasOne(typeof(ToastedBun), "ToastedBun").WithOne("Whoopper")
                .ForeignKey(typeof(ToastedBun), "BurgerId1", "BurgerId2");

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasOne(typeof(ToastedBun), "ToastedBun").WithOne("Whoopper")
                .ForeignKey(typeof(ToastedBun), "BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasOne(typeof(ToastedBun), "ToastedBun").WithOne("Whoopper")
                .ForeignKey(typeof(ToastedBun), "BurgerId1", "BurgerId2")
                .ReferencedKey(typeof(Whoopper), "AlternateKey1", "AlternateKey2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasOne(typeof(ToastedBun), "ToastedBun").WithOne("Whoopper")
                .ReferencedKey(typeof(Whoopper), "AlternateKey1", "AlternateKey2")
                .ForeignKey(typeof(ToastedBun), "BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_uses_composite_PK_for_FK_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasOne(typeof(Moostard), "Moostard").WithOne("Whoopper");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_be_flipped_and_composite_PK_is_still_used_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Moostard)).HasOne(typeof(Whoopper), "Whoopper").WithOne("Moostard")
                .ForeignKey(typeof(Moostard), "Id1", "Id2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_be_flipped_using_principal_and_composite_PK_is_still_used_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Moostard)).HasOne(typeof(Whoopper), "Whoopper").WithOne("Moostard")
                .ReferencedKey(typeof(Whoopper), "Id1", "Id2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasOne(typeof(ToastedBun), "ToastedBun").WithOne(null)
                .ForeignKey(typeof(ToastedBun), "BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasOne(typeof(ToastedBun), null).WithOne("Whoopper")
                .ForeignKey(typeof(ToastedBun), "BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(new ConventionSet(), model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.PropertyCount;
            var dependentPropertyCount = dependentType.PropertyCount;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity(typeof(Whoopper)).HasOne(typeof(ToastedBun), null).WithOne(null)
                .ForeignKey(typeof(ToastedBun), "BurgerId1", "BurgerId2");

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.PropertyCount);
            Assert.Equal(dependentPropertyCount, dependentType.PropertyCount);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        private class Whoopper
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public int AlternateKey1 { get; set; }
            public int AlternateKey2 { get; set; }

            public IEnumerable<Tomato> Tomatoes { get; set; }

            public ToastedBun ToastedBun { get; set; }

            public Moostard Moostard { get; set; }
        }

        private class Tomato
        {
            public int Id { get; set; }

            public int BurgerId1 { get; set; }
            public int BurgerId2 { get; set; }
            public Whoopper Whoopper { get; set; }
        }

        private class ToastedBun
        {
            public int Id { get; set; }

            public int BurgerId1 { get; set; }
            public int BurgerId2 { get; set; }
            public Whoopper Whoopper { get; set; }
        }

        private class Moostard
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public Whoopper Whoopper { get; set; }
        }

        private class Customer
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }
            public Guid GuidKey { get; set; }
            public string Name { get; set; }

            public IEnumerable<Order> Orders { get; set; }

            public CustomerDetails Details { get; set; }
        }

        private class CustomerDetails
        {
            public int Id { get; set; }

            public Customer Customer { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }

            public int CustomerId { get; set; }
            public int AnotherCustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        private class Quarks : INotifyPropertyChanging, INotifyPropertyChanged
        {
            public int Id { get; set; }

            public int Up { get; set; }
            public string Down { get; set; }
            public int Charm { get; set; }
            public string Strange { get; set; }
            public int Top { get; set; }
            public string Bottom { get; set; }

#pragma warning disable 67
            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        [Fact]
        public void One_to_many_relationships_with_nullable_keys_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity(typeof(Hob)).HasMany(typeof(Nob), "Nobs").WithOne("Hob")
                .ForeignKey("HobId1", "HobId2");

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_many_relationships_with_non_nullable_keys_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity(typeof(Nob)).HasMany(typeof(Hob), "Hobs").WithOne("Nob")
                .ForeignKey("NobId1", "NobId2");

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_nullable_keys_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity(typeof(Nob)).HasOne(typeof(Hob), "Hob").WithMany("Nobs")
                .ForeignKey("HobId1", "HobId2");

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_non_nullable_keys_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>().HasOne(typeof(Nob), "Nob").WithMany("Hobs")
                .ForeignKey("NobId1", "NobId2");

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_nullable_keys_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity(typeof(Hob)).HasOne(typeof(Nob), "Nob").WithOne("Hob")
                .ForeignKey(typeof(Nob), "HobId1", "HobId2");

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_non_nullable_keys_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity(typeof(Nob)).HasOne(typeof(Hob), "Hob").WithOne("Nob")
                .ForeignKey(typeof(Hob), "NobId1", "NobId2");

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_many_relationships_with_nullable_keys_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity(typeof(Hob)).HasMany(typeof(Nob), "Nobs").WithOne("Hob")
                .ForeignKey("HobId1", "HobId2")
                .Required();

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_many_relationships_with_non_nullable_keys_cannot_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            Assert.Equal(
                Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity(typeof(Nob)).HasMany(typeof(Hob), "Hobs").WithOne("Nob")
                    .ForeignKey("NobId1", "NobId2")
                    .Required(false)).Message);

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_nullable_keys_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity(typeof(Nob)).HasOne(typeof(Hob), "Hob").WithMany("Nobs")
                .ForeignKey("HobId1", "HobId2")
                .Required();

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_non_nullable_keys_cannot_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            Assert.Equal(
                Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity(typeof(Hob)).HasOne(typeof(Nob), "Nob").WithMany("Hobs")
                    .ForeignKey("NobId1", "NobId2")
                    .Required(false)).Message);

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_nullable_keys_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity(typeof(Hob)).HasOne(typeof(Nob), "Nob").WithOne("Hob")
                .ForeignKey(typeof(Nob), "HobId1", "HobId2")
                .Required();

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_non_nullable_keys_cannot_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            Assert.Equal(
                Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity(typeof(Nob)).HasOne(typeof(Hob), "Hob").WithOne("Nob")
                    .ForeignKey(typeof(Hob), "NobId1", "NobId2")
                    .Required(false)).Message);

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        private class Hob
        {
            public string Id1 { get; set; }
            public string Id2 { get; set; }

            public int NobId1 { get; set; }
            public int NobId2 { get; set; }

            public Nob Nob { get; set; }
            public ICollection<Nob> Nobs { get; set; }
        }

        private class Nob
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public string HobId1 { get; set; }
            public string HobId2 { get; set; }

            public Hob Hob { get; set; }
            public ICollection<Hob> Hobs { get; set; }
        }

        private ModelBuilder HobNobBuilder()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<Hob>().Key(e => new { e.Id1, e.Id2 });
            builder.Entity<Nob>().Key(e => new { e.Id1, e.Id2 });

            return builder;
        }
    }
}

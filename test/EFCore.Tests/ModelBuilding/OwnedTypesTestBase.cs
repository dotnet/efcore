// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class OwnedTypesTestBase : ModelBuilderTestBase
        {
            [ConditionalFact]
            public virtual void Can_configure_owned_type()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>()
                    .OwnsOne(
                        c => c.Details, db =>
                        {
                            db.WithOwner(d => d.Customer)
                                .HasPrincipalKey(c => c.AlternateKey);
                            db.Property(d => d.CustomerId);
                            db.HasIndex(d => d.CustomerId);
                        });

                var model = modelBuilder.FinalizeModel();

                var owner = model.FindEntityType(typeof(Customer));
                Assert.Equal(typeof(Customer).FullName, owner.Name);
                var ownership = owner.FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.Equal(nameof(Customer.Details), ownership.PrincipalToDependent.Name);
                Assert.Equal(nameof(CustomerDetails.Customer), ownership.DependentToPrincipal.Name);
                Assert.Equal("CustomerAlternateKey", ownership.Properties.Single().Name);
                Assert.Equal(nameof(Customer.AlternateKey), ownership.PrincipalKey.Properties.Single().Name);
                var owned = ownership.DeclaringEntityType;
                Assert.Single(owned.GetForeignKeys());
                Assert.Equal(nameof(CustomerDetails.CustomerId), owned.GetIndexes().Single().Properties.Single().Name);
                Assert.Equal(
                    new[] { "CustomerAlternateKey", nameof(CustomerDetails.CustomerId), nameof(CustomerDetails.Id) },
                    owned.GetProperties().Select(p => p.Name));
                Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_using_nested_closure()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().OwnsOne(
                    c => c.Details,
                    r => r.HasAnnotation("foo", "bar")
                        .WithOwner(d => d.Customer)
                        .HasAnnotation("bar", "foo"));

                var model = modelBuilder.FinalizeModel();

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                var owned = ownership.DeclaringEntityType;
                Assert.True(ownership.IsOwnership);
                Assert.Equal("bar", owned.FindAnnotation("foo").Value);
                Assert.Single(owned.GetForeignKeys());
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_inverse()
            {
                var modelBuilder = CreateModelBuilder();
                IModel model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);

                var owner = model.FindEntityType(typeof(Customer));
                var ownee = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Equal(nameof(CustomerDetails.CustomerId), ownee.FindPrimaryKey().Properties.Single().Name);

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                    .HasOne(d => d.Customer);

                model = modelBuilder.FinalizeModel();

                var ownership = owner.FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.Equal(nameof(CustomerDetails.Customer), ownership.DependentToPrincipal.Name);
                Assert.Same(ownee, ownership.DeclaringEntityType);
                Assert.Equal(nameof(CustomerDetails.CustomerId), ownee.FindPrimaryKey().Properties.Single().Name);
                Assert.Single(ownership.DeclaringEntityType.GetForeignKeys());
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_properties()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                    .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction)
                    .HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications)
                    .Ignore(d => d.Id)
                    .Property<int>("foo");

                var model = modelBuilder.FinalizeModel();

                var owner = model.FindEntityType(typeof(Customer));
                var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Null(owner.FindProperty("foo"));
                Assert.Equal(new[] { nameof(CustomerDetails.CustomerId), "foo" }, owned.GetProperties().Select(p => p.Name).ToArray());
                Assert.Equal(PropertyAccessMode.FieldDuringConstruction, owned.GetPropertyAccessMode());
                Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, owned.GetChangeTrackingStrategy());
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_key()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                    .HasKey(c => c.Id);

                var model = modelBuilder.FinalizeModel();

                var owner = model.FindEntityType(typeof(Customer));
                var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Equal(
                    new[] { nameof(CustomerDetails.Id), nameof(CustomerDetails.CustomerId) },
                    owned.GetProperties().Select(p => p.Name).ToArray());
                Assert.Equal(nameof(CustomerDetails.Id), owned.FindPrimaryKey().Properties.Single().Name);
            }

            [ConditionalFact]
            public virtual void Can_configure_ownership_foreign_key()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>()
                    .OwnsOne(c => c.Details)
                    .WithOwner(d => d.Customer)
                    .HasForeignKey(c => c.Id);

                var model = modelBuilder.FinalizeModel();

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.Equal(nameof(CustomerDetails.Id), ownership.Properties.Single().Name);
                Assert.Equal(nameof(CustomerDetails.Id), ownership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);
                Assert.Single(ownership.DeclaringEntityType.GetForeignKeys());
            }

            [ConditionalFact]
            public virtual void Can_configure_another_relationship_to_owner()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().OwnsOne(
                    c => c.Details,
                    r =>
                    {
                        r.WithOwner();
                        r.HasOne(d => d.Customer)
                            .WithMany();
                    });

                var model = modelBuilder.FinalizeModel();

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                var owned = ownership.DeclaringEntityType;
                Assert.True(ownership.IsOwnership);
                Assert.Equal(nameof(Customer.Details), ownership.PrincipalToDependent.Name);
                Assert.Null(ownership.DependentToPrincipal);
                Assert.Equal("CustomerId", ownership.Properties.Single().Name);

                var otherFk = owned.GetForeignKeys().Single(fk => fk != ownership);
                Assert.Null(otherFk.PrincipalToDependent);
                Assert.Equal(nameof(CustomerDetails.Customer), otherFk.DependentToPrincipal.Name);
                Assert.Equal("CustomerId1", otherFk.Properties.Single().Name);
                Assert.False(otherFk.IsOwnership);
                Assert.False(otherFk.IsUnique);

                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
            }

            [ConditionalFact]
            public virtual void Changing_ownership_uniqueness_throws()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().OwnsOne(
                    c => c.Details,
                    r =>
                    {
                        r.HasOne(d => d.Customer)
                            .WithMany();
                    });

                Assert.Equal(
                    CoreStrings.NavigationNotAdded(nameof(Customer), nameof(Customer.Details), nameof(CustomerDetails)),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
            }

            [ConditionalFact]
            public virtual void Can_configure_multiple_ownerships()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<OtherCustomer>().OwnsOne(c => c.Details);
                modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details);

                var model = modelBuilder.FinalizeModel();

                var ownership1 = model.FindEntityType(typeof(OtherCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                var ownership2 = model.FindEntityType(typeof(SpecialCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.Equal(typeof(CustomerDetails), ownership1.DeclaringEntityType.ClrType);
                Assert.Equal(typeof(CustomerDetails), ownership2.DeclaringEntityType.ClrType);
                Assert.NotSame(ownership1.DeclaringEntityType, ownership2.DeclaringEntityType);
                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
                Assert.Single(ownership1.DeclaringEntityType.GetForeignKeys());
                Assert.Single(ownership2.DeclaringEntityType.GetForeignKeys());
            }

            [ConditionalFact]
            public virtual void Can_configure_one_to_one_relationship_from_an_owned_type()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<SpecialCustomer>();
                modelBuilder.Entity<OtherCustomer>().OwnsOne(c => c.Details)
                    .HasOne<SpecialCustomer>()
                    .WithOne()
                    .HasPrincipalKey<SpecialCustomer>();

                Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(CustomerDetails)));

                modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details);

                var model = modelBuilder.FinalizeModel();

                var ownership = model.FindEntityType(typeof(OtherCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                var foreignKey = model.FindEntityType(typeof(SpecialCustomer)).GetReferencingForeignKeys()
                    .Single(
                        fk => fk.DeclaringEntityType.ClrType == typeof(CustomerDetails)
                            && fk.PrincipalToDependent == null);
                Assert.Same(ownership.DeclaringEntityType, foreignKey.DeclaringEntityType);
                Assert.NotEqual(ownership.Properties.Single().Name, foreignKey.Properties.Single().Name);
                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
                Assert.Equal(2, ownership.DeclaringEntityType.GetForeignKeys().Count());
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_collection_from_an_owned_type()
            {
                var modelBuilder = CreateModelBuilder();
                IModel model = modelBuilder.Model;

                modelBuilder.Ignore<OrderDetails>();
                var entityBuilder = modelBuilder.Entity<CustomerDetails>().OwnsOne(o => o.Customer)
                    .OwnsMany(c => c.Orders);

                var ownership = model.FindEntityType(typeof(CustomerDetails)).FindNavigation(nameof(CustomerDetails.Customer)).ForeignKey;
                var owned = ownership.DeclaringEntityType;
                var chainedOwnership = owned.FindNavigation(nameof(Customer.Orders)).ForeignKey;
                var chainedOwned = chainedOwnership.DeclaringEntityType;
                Assert.Equal(
                    new[] { nameof(Order.CustomerId), nameof(Order.OrderId) },
                    chainedOwned.FindPrimaryKey().Properties.Select(p => p.Name));

                entityBuilder.HasKey(o => o.OrderId);

                model = modelBuilder.FinalizeModel();

                Assert.True(ownership.IsOwnership);
                Assert.True(ownership.IsUnique);
                Assert.Equal(nameof(Customer.Details), ownership.DependentToPrincipal.Name);
                Assert.Equal("DetailsId", ownership.Properties.Single().Name);
                Assert.Equal("DetailsId", owned.FindPrimaryKey().Properties.Single().Name);
                Assert.Empty(owned.GetIndexes());
                Assert.True(chainedOwnership.IsOwnership);
                Assert.False(chainedOwnership.IsUnique);
                Assert.Equal(nameof(Order.Customer), chainedOwnership.DependentToPrincipal.Name);
                Assert.Equal(nameof(Order.CustomerId), chainedOwnership.Properties.Single().Name);
                Assert.Equal(nameof(Order.OrderId), chainedOwned.FindPrimaryKey().Properties.Single().Name);
                Assert.Single(chainedOwned.GetForeignKeys());
                Assert.Equal(nameof(Order.CustomerId), chainedOwned.GetIndexes().Single().Properties.Single().Name);
                Assert.Same(entityBuilder.OwnedEntityType, chainedOwned);

                Assert.Equal(3, model.GetEntityTypes().Count());
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_collection()
            {
                var modelBuilder = CreateModelBuilder();

                var entityBuilder = modelBuilder.Entity<Customer>().OwnsMany(c => c.Orders)
                    .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction)
                    .HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications)
                    .Ignore(nameof(Order.OrderId))
                    .Ignore(o => o.OrderCombination)
                    .Ignore(o => o.Details);
                entityBuilder.Property<int>("foo");
                entityBuilder.HasIndex("foo");
                entityBuilder.HasKey(o => o.AnotherCustomerId);
                entityBuilder.WithOwner(o => o.Customer)
                    .HasPrincipalKey(c => c.AlternateKey);

                var model = modelBuilder.FinalizeModel();

                var owner = model.FindEntityType(typeof(Customer));
                var ownership = owner.FindNavigation(nameof(Customer.Orders)).ForeignKey;
                var owned = ownership.DeclaringEntityType;
                Assert.Same(entityBuilder.OwnedEntityType, owned);
                Assert.True(ownership.IsOwnership);
                Assert.Equal("CustomerAlternateKey", ownership.Properties.Single().Name);
                Assert.Equal(nameof(Customer.AlternateKey), ownership.PrincipalKey.Properties.Single().Name);
                Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal.Name);

                Assert.Null(owner.FindProperty("foo"));
                Assert.Equal(nameof(Order.AnotherCustomerId), owned.FindPrimaryKey().Properties.Single().Name);
                Assert.Equal(2, owned.GetIndexes().Count());
                Assert.Equal("CustomerAlternateKey", owned.GetIndexes().First().Properties.Single().Name);
                Assert.Equal("foo", owned.GetIndexes().Last().Properties.Single().Name);
                Assert.Equal(
                    new[] { nameof(Order.AnotherCustomerId), "CustomerAlternateKey", nameof(Order.CustomerId), "foo" },
                    owned.GetProperties().Select(p => p.Name).ToArray());
                Assert.Equal(PropertyAccessMode.FieldDuringConstruction, owned.GetPropertyAccessMode());
                Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, owned.GetChangeTrackingStrategy());

                Assert.NotNull(model.FindEntityType(typeof(Order)));
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(Order)));
                Assert.Single(owned.GetForeignKeys());
                Assert.Single(owned.GetNavigations());
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_collection_using_nested_closure()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().OwnsMany(
                    c => c.Orders,
                    r =>
                    {
                        r.HasAnnotation("foo", "bar");
                        r.Property<uint>("Id");
                        r.HasKey("Id");
                        r.HasIndex(o => o.AnotherCustomerId);
                        r.Property(o => o.AnotherCustomerId).IsRequired();
                        r.Ignore(o => o.OrderCombination);
                        r.Ignore(o => o.Details);
                        r.WithOwner(o => o.Customer)
                            .HasAnnotation("bar", "foo")
                            .HasForeignKey("DifferentCustomerId");
                    });

                var model = modelBuilder.FinalizeModel();

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
                var owned = ownership.DeclaringEntityType;
                Assert.True(ownership.IsOwnership);
                Assert.Equal("DifferentCustomerId", ownership.Properties.Single().Name);
                Assert.Equal("bar", owned.FindAnnotation("foo").Value);
                Assert.Single(owned.GetForeignKeys());
                Assert.Equal("Id", owned.FindPrimaryKey().Properties.Single().Name);
                Assert.Equal(2, owned.GetIndexes().Count());
                Assert.Equal(nameof(Order.AnotherCustomerId), owned.GetIndexes().First().Properties.Single().Name);
                Assert.Equal("DifferentCustomerId", owned.GetIndexes().Last().Properties.Single().Name);
                Assert.False(owned.FindProperty(nameof(Order.AnotherCustomerId)).IsNullable);
                Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal.Name);
            }

            [ConditionalFact]
            public virtual void Can_configure_one_to_one_relationship_from_an_owned_type_collection()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Customer>();
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Entity<OtherCustomer>().OwnsMany(
                    c => c.Orders, ob =>
                    {
                        ob.HasKey(o => o.OrderId);
                        ob.HasOne<SpecialCustomer>()
                            .WithOne()
                            .HasPrincipalKey<SpecialCustomer>();
                    });

                Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(Order)));

                modelBuilder.Entity<SpecialCustomer>().OwnsMany(c => c.Orders)
                    .HasKey(o => o.OrderId);

                var model = modelBuilder.FinalizeModel();

                Assert.Null(model.FindEntityType(typeof(Order)));
                var ownership1 = model.FindEntityType(typeof(OtherCustomer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
                var ownership2 = model.FindEntityType(typeof(SpecialCustomer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
                Assert.Equal(typeof(Order), ownership1.DeclaringEntityType.ClrType);
                Assert.Equal(typeof(Order), ownership2.DeclaringEntityType.ClrType);
                Assert.NotSame(ownership1.DeclaringEntityType, ownership2.DeclaringEntityType);
                Assert.Equal(nameof(Order.Customer), ownership1.DependentToPrincipal.Name);
                Assert.Equal(nameof(Order.Customer), ownership2.DependentToPrincipal.Name);
                Assert.Equal("CustomerId", ownership1.Properties.Single().Name);
                Assert.Equal("CustomerId", ownership2.Properties.Single().Name);

                var foreignKey = model.FindEntityType(typeof(SpecialCustomer)).GetReferencingForeignKeys()
                    .Single(
                        fk => fk.DeclaringEntityType.ClrType == typeof(Order)
                            && fk.PrincipalToDependent == null);
                Assert.Same(ownership1.DeclaringEntityType, foreignKey.DeclaringEntityType);
                Assert.Null(foreignKey.PrincipalToDependent);
                Assert.NotEqual(ownership1.Properties.Single().Name, foreignKey.Properties.Single().Name);
                Assert.Equal(5, model.GetEntityTypes().Count());
                Assert.Equal(2, model.GetEntityTypes(typeof(Order)).Count);
                Assert.Equal(2, ownership1.DeclaringEntityType.GetForeignKeys().Count());

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(Order)));
                Assert.Equal(2, ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Single(ownership2.DeclaringEntityType.GetForeignKeys());
                Assert.Null(model.FindEntityType(typeof(SpecialOrder)));
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_from_an_owned_type_collection()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Entity<Customer>().OwnsMany(
                    c => c.Orders, ob =>
                    {
                        ob.HasKey(o => o.OrderId);
                        ob.OwnsOne(o => o.Details)
                            .HasData(
                                new OrderDetails { OrderId = -1 });
                    });

                var model = modelBuilder.FinalizeModel();

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
                var owned = ownership.DeclaringEntityType;
                Assert.Equal(nameof(Order.CustomerId), ownership.Properties.Single().Name);
                Assert.Single(ownership.DeclaringEntityType.GetForeignKeys());
                var chainedOwnership = owned.FindNavigation(nameof(Order.Details)).ForeignKey;
                var chainedOwned = chainedOwnership.DeclaringEntityType;
                Assert.True(chainedOwnership.IsOwnership);
                Assert.True(chainedOwnership.IsUnique);
                Assert.Equal(nameof(OrderDetails.OrderId), chainedOwned.FindPrimaryKey().Properties.Single().Name);
                Assert.Empty(chainedOwned.GetIndexes());
                Assert.Equal(-1, chainedOwned.GetSeedData().Single()[nameof(OrderDetails.OrderId)]);
                Assert.Equal(nameof(OrderDetails.OrderId), chainedOwnership.Properties.Single().Name);
                Assert.Equal(nameof(OrderDetails.Order), chainedOwnership.DependentToPrincipal.Name);

                Assert.Equal(4, model.GetEntityTypes().Count());
            }

            [ConditionalFact]
            public virtual void Can_chain_owned_type_collection_configurations()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Entity<Customer>().OwnsMany(
                    c => c.Orders, ob =>
                    {
                        ob.HasKey(o => o.OrderId);
                        ob.HasData(
                            new Order { OrderId = -2, CustomerId = -1 });
                        ob.OwnsMany(o => o.Products)
                            .HasKey(p => p.Id);
                    });

                var model = modelBuilder.FinalizeModel();

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
                var owned = ownership.DeclaringEntityType;
                Assert.Single(ownership.DeclaringEntityType.GetForeignKeys());
                var seedData = owned.GetSeedData().Single();
                Assert.Equal(-2, seedData[nameof(Order.OrderId)]);
                Assert.Equal(-1, seedData[nameof(Order.CustomerId)]);
                var chainedOwnership = owned.FindNavigation(nameof(Order.Products)).ForeignKey;
                var chainedOwned = chainedOwnership.DeclaringEntityType;
                Assert.True(chainedOwnership.IsOwnership);
                Assert.False(chainedOwnership.IsUnique);
                Assert.Equal("OrderId", chainedOwnership.Properties.Single().Name);
                Assert.Equal(nameof(Product.Id), chainedOwned.FindPrimaryKey().Properties.Single().Name);
                Assert.Equal(
                    "OrderId",
                    chainedOwned.GetIndexes().Single().Properties.Single().Name);
                Assert.Equal(nameof(Product.Order), chainedOwnership.DependentToPrincipal.Name);

                Assert.Equal(4, model.GetEntityTypes().Count());
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_collection_without_explicit_key()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().OwnsMany(
                    c => c.Orders,
                    r =>
                    {
                        r.Ignore(o => o.OrderCombination);
                        r.Ignore(o => o.Details);
                        r.OwnsMany(o => o.Products);
                    });

                var model = modelBuilder.FinalizeModel();

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
                var owned = ownership.DeclaringEntityType;
                Assert.True(ownership.IsOwnership);
                Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal.Name);
                Assert.Equal(nameof(Order.CustomerId), ownership.Properties.Single().Name);
                Assert.Single(owned.GetForeignKeys());
                var pk = owned.FindPrimaryKey();
                Assert.Equal(new[] { nameof(Order.CustomerId), nameof(Order.OrderId) }, pk.Properties.Select(p => p.Name));
                Assert.Equal(ValueGenerated.OnAdd, pk.Properties.Last().ValueGenerated);
                Assert.Empty(owned.GetIndexes());

                var chainedOwnership = owned.FindNavigation(nameof(Order.Products)).ForeignKey;
                var chainedOwned = chainedOwnership.DeclaringEntityType;
                Assert.True(chainedOwnership.IsOwnership);
                Assert.False(chainedOwnership.IsUnique);
                Assert.Equal(nameof(Product.Order), chainedOwnership.DependentToPrincipal.Name);
                Assert.Equal(new[] { "OrderCustomerId", "OrderId" }, chainedOwnership.Properties.Select(p => p.Name));
                var chainedPk = chainedOwned.FindPrimaryKey();
                Assert.Equal(new[] { "OrderCustomerId", "OrderId", nameof(Product.Id) }, chainedPk.Properties.Select(p => p.Name));
                Assert.Equal(ValueGenerated.OnAdd, chainedPk.Properties.Last().ValueGenerated);
                Assert.Empty(chainedOwned.GetIndexes());

                Assert.Equal(4, model.GetEntityTypes().Count());
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_collection_without_explicit_key_or_candidate()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().OwnsMany(
                    c => c.Orders,
                    r =>
                    {
                        r.Ignore(o => o.OrderCombination);
                        r.Ignore(o => o.Details);
                        r.Ignore(o => o.OrderId);
                        r.OwnsMany(o => o.Products)
                            .Ignore(p => p.Id);
                    });

                var model = modelBuilder.FinalizeModel();

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Orders)).ForeignKey;
                var owned = ownership.DeclaringEntityType;
                Assert.True(ownership.IsOwnership);
                Assert.False(ownership.IsUnique);
                Assert.Equal(nameof(Order.Customer), ownership.DependentToPrincipal.Name);
                Assert.Equal(nameof(Order.CustomerId), ownership.Properties.Single().Name);
                Assert.Single(owned.GetForeignKeys());
                var pk = owned.FindPrimaryKey();
                Assert.Equal(new[] { nameof(Order.CustomerId), "Id" }, pk.Properties.Select(p => p.Name));
                Assert.Equal(ValueGenerated.OnAdd, pk.Properties.Last().ValueGenerated);
                Assert.Empty(owned.GetIndexes());

                var chainedOwnership = owned.FindNavigation(nameof(Order.Products)).ForeignKey;
                var chainedOwned = chainedOwnership.DeclaringEntityType;
                Assert.True(chainedOwnership.IsOwnership);
                Assert.False(chainedOwnership.IsUnique);
                Assert.Equal(nameof(Product.Order), chainedOwnership.DependentToPrincipal.Name);
                Assert.Equal(new[] { "OrderCustomerId", "OrderId" }, chainedOwnership.Properties.Select(p => p.Name));
                var chainedPk = chainedOwned.FindPrimaryKey();
                Assert.Equal(new[] { "OrderCustomerId", "OrderId", "Id1" }, chainedPk.Properties.Select(p => p.Name));
                Assert.Equal(ValueGenerated.OnAdd, chainedPk.Properties.Last().ValueGenerated);
                Assert.Empty(chainedOwned.GetIndexes());

                Assert.Equal(4, model.GetEntityTypes().Count());
            }

            [ConditionalFact]
            public virtual void Ambiguous_relationship_between_owned_types_throws()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Owned<Whoopper>();
                modelBuilder.Owned<Mustard>();
                modelBuilder.Entity<ToastedBun>();
                modelBuilder.Ignore<Tomato>();

                Assert.Equal(
                    CoreStrings.AmbiguousOwnedNavigation(nameof(Whoopper), nameof(Mustard)),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_collection_with_one_call()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Owned<Order>();
                modelBuilder.Entity<Customer>()
                    .OwnsMany(c => c.Orders)
                    .HasKey(o => o.OrderId);

                var specialCustomer = modelBuilder.Entity<SpecialCustomer>()
                    .OwnsMany(
                        c => c.SpecialOrders, so =>
                        {
                            so.HasKey(o => o.SpecialOrderId);
                            so.Ignore(o => o.Customer);
                            so.OwnsOne(o => o.BackOrder);
                        }).Metadata;

                var model = modelBuilder.FinalizeModel();

                var customer = model.FindEntityType(typeof(Customer));

                var ownership = customer.FindNavigation(nameof(Customer.Orders)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.False(ownership.IsUnique);
                Assert.Equal(nameof(Order.OrderId), ownership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);
                var specialOwnership = specialCustomer.FindNavigation(nameof(SpecialCustomer.SpecialOrders)).ForeignKey;
                Assert.True(specialOwnership.IsOwnership);
                Assert.False(specialOwnership.IsUnique);
                Assert.Equal(
                    nameof(SpecialOrder.SpecialOrderId), specialOwnership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);

                Assert.Equal(9, modelBuilder.Model.GetEntityTypes().Count());
                Assert.Equal(2, modelBuilder.Model.GetEntityTypes(typeof(Order)).Count);
                Assert.Equal(7, modelBuilder.Model.GetEntityTypes().Count(e => !e.HasDefiningNavigation()));
                Assert.Equal(5, modelBuilder.Model.GetEntityTypes().Count(e => e.IsOwned()));

                var conventionModel = (IConventionModel)modelBuilder.Model;
                Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(Order)));
                Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(SpecialOrder)));
                Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(Customer)));
                Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(SpecialCustomer)));
            }

            [ConditionalFact]
            public virtual void Can_configure_owned_type_collection_with_one_call_afterwards()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Owned<SpecialOrder>();

                modelBuilder.Entity<SpecialCustomer>();
                var specialCustomer = modelBuilder.Entity<SpecialCustomer>().OwnsMany(
                    c => c.SpecialOrders, so =>
                    {
                        so.HasKey(o => o.SpecialOrderId);
                        so.Ignore(o => o.Customer);
                        so.OwnsOne(o => o.BackOrder);
                    }).Metadata;

                modelBuilder.Owned<Order>();

                modelBuilder.Entity<Customer>()
                    .OwnsMany(c => c.Orders)
                    .HasKey(o => o.OrderId);

                var model = modelBuilder.FinalizeModel();

                var customer = model.FindEntityType(typeof(Customer));

                var ownership = customer.FindNavigation(nameof(Customer.Orders)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.False(ownership.IsUnique);
                Assert.Equal(nameof(Order.OrderId), ownership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);
                var specialOwnership = specialCustomer.FindNavigation(nameof(SpecialCustomer.SpecialOrders)).ForeignKey;
                Assert.True(specialOwnership.IsOwnership);
                Assert.False(specialOwnership.IsUnique);
                Assert.Equal(
                    nameof(SpecialOrder.SpecialOrderId), specialOwnership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);

                Assert.Equal(9, modelBuilder.Model.GetEntityTypes().Count());
                Assert.Equal(2, modelBuilder.Model.GetEntityTypes(typeof(Order)).Count);
                Assert.Equal(7, modelBuilder.Model.GetEntityTypes().Count(e => !e.HasDefiningNavigation()));
                Assert.Equal(5, modelBuilder.Model.GetEntityTypes().Count(e => e.IsOwned()));

                var conventionModel = (IConventionModel)modelBuilder.Model;
                Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(Order)));
                Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(SpecialOrder)));
                Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(Customer)));
                Assert.Null(conventionModel.FindIgnoredConfigurationSource(typeof(SpecialCustomer)));
            }

            [ConditionalFact]
            public virtual void Can_configure_single_owned_type_using_attribute()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<SpecialOrder>();

                var model = modelBuilder.FinalizeModel();

                var owner = model.FindEntityType(typeof(SpecialOrder));
                var ownership = owner.FindNavigation(nameof(SpecialOrder.ShippingAddress)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.NotNull(ownership.DeclaringEntityType.FindProperty(nameof(StreetAddress.Street)));
            }

            [ConditionalFact]
            public virtual void Can_configure_fk_on_multiple_ownerships()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<AnotherBookLabel>();
                modelBuilder.Ignore<SpecialBookLabel>();
                modelBuilder.Ignore<BookDetails>();

                modelBuilder.Entity<Book>().OwnsOne(
                    b => b.Label, lb =>
                    {
                        lb.WithOwner()
                            .HasForeignKey("BookLabelId")
                            .HasAnnotation("Foo", "Bar");
                    });
                modelBuilder.Entity<Book>()
                    .OwnsOne(b => b.AlternateLabel)
                    .WithOwner()
                    .HasForeignKey("BookLabelId");

                IModel model = modelBuilder.Model;

                var bookOwnership1 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Equal(typeof(int), bookOwnership1.DeclaringEntityType.GetForeignKeys().Single().Properties.Single().ClrType);
                Assert.Equal(typeof(int), bookOwnership1.DeclaringEntityType.GetForeignKeys().Single().Properties.Single().ClrType);
                Assert.Equal("Bar", bookOwnership1["Foo"]);

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(3, model.GetEntityTypes().Count());

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label).Ignore(l => l.Book);
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel).Ignore(l => l.Book);

                modelBuilder.FinalizeModel();
            }

            [ConditionalFact]
            public virtual void Can_map_base_of_owned_type()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<BookLabel>();
                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);
                modelBuilder.Entity<BookDetailsBase>();
                modelBuilder.Ignore<SpecialBookLabel>();

                var model = modelBuilder.FinalizeModel();

                Assert.NotNull(model.FindEntityType(typeof(BookDetailsBase)));
                var owner = model.FindEntityType(typeof(Customer));
                var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Null(owned.BaseType);
                Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
                Assert.Single(owned.GetForeignKeys());
            }

            [ConditionalFact]
            public virtual void Can_map_base_of_owned_type_first()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<BookLabel>();
                modelBuilder.Entity<BookDetailsBase>();
                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);
                modelBuilder.Ignore<SpecialBookLabel>();

                var model = modelBuilder.FinalizeModel();

                Assert.NotNull(model.FindEntityType(typeof(BookDetailsBase)));
                var owner = model.FindEntityType(typeof(Customer));
                var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Null(owned.BaseType);
                Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
                Assert.Single(owned.GetForeignKeys());
            }

            [ConditionalFact]
            public virtual void Can_map_derived_of_owned_type()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<OrderCombination>().OwnsOne(c => c.Details);
                modelBuilder.Entity<Customer>();

                IModel model = modelBuilder.Model;

                var owner = model.FindEntityType(typeof(OrderCombination));
                var owned = owner.FindNavigation(nameof(OrderCombination.Details)).ForeignKey.DeclaringEntityType;
                Assert.NotEmpty(owned.GetDirectlyDerivedTypes());
                var navigationsToDerived = model.GetEntityTypes().SelectMany(e => e.GetDeclaredNavigations()).Where(
                    n =>
                    {
                        var targetType = n.GetTargetType().ClrType;
                        return targetType != typeof(DetailsBase) && typeof(DetailsBase).IsAssignableFrom(targetType);
                    });
                Assert.Empty(navigationsToDerived);
                Assert.Single(owned.GetForeignKeys());
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(DetailsBase)));
                Assert.Same(owned, model.FindEntityType(typeof(CustomerDetails)).BaseType);

                modelBuilder.Entity<Customer>().Ignore(c => c.Details);
                modelBuilder.Entity<Order>().Ignore(c => c.Details);
            }

            [ConditionalFact]
            public virtual void Can_map_derived_of_owned_type_first()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<OrderCombination>().OwnsOne(c => c.Details);

                IModel model = modelBuilder.Model;

                var owner = model.FindEntityType(typeof(OrderCombination));
                var owned = owner.FindNavigation(nameof(OrderCombination.Details)).ForeignKey.DeclaringEntityType;
                Assert.NotEmpty(owned.GetDirectlyDerivedTypes());
                Assert.Empty(
                    model.GetEntityTypes().SelectMany(e => e.GetDeclaredNavigations()).Where(
                        n =>
                        {
                            var targetType = n.GetTargetType().ClrType;
                            return targetType != typeof(DetailsBase) && typeof(DetailsBase).IsAssignableFrom(targetType);
                        }));
                Assert.Single(owned.GetForeignKeys());
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(DetailsBase)));
                Assert.Same(owned, model.FindEntityType(typeof(CustomerDetails)).BaseType);

                modelBuilder.Entity<Customer>().Ignore(c => c.Details);
                modelBuilder.Entity<Order>().Ignore(c => c.Details);

                modelBuilder.FinalizeModel();
            }

            [ConditionalFact]
            public virtual void Throws_on_FK_matching_two_relationships()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<SpecialBookLabel>();
                modelBuilder.Entity<Book>();

                Assert.Equal(
                    CoreStrings.AmbiguousForeignKeyPropertyCandidates(
                        nameof(BookLabel) + "." + nameof(BookLabel.Book),
                        nameof(Book) + "." + nameof(Book.Label),
                        nameof(BookLabel),
                        nameof(Book) + "." + nameof(Book.AlternateLabel),
                        "{'" + nameof(BookLabel.BookId) + "'}"),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.FinalizeModel()).Message);
            }

            [ConditionalFact]
            public virtual void Can_configure_chained_ownerships()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Book>().OwnsOne(
                    b => b.Label, bb =>
                    {
                        bb.Ignore(l => l.Book);
                        bb.OwnsOne(
                            l => l.AnotherBookLabel, ab =>
                            {
                                ab.Ignore(l => l.Book);
                                ab.OwnsOne(l => l.SpecialBookLabel).Ignore(l => l.Book).Ignore(s => s.BookLabel);
                            });
                        bb.OwnsOne(
                            l => l.SpecialBookLabel, sb =>
                            {
                                sb.Ignore(l => l.Book);
                                sb.OwnsOne(l => l.AnotherBookLabel).Ignore(l => l.Book);
                            });
                    });

                modelBuilder.Entity<Book>().OwnsOne(
                    b => b.AlternateLabel, bb =>
                    {
                        bb.Ignore(l => l.Book);
                        bb.OwnsOne(
                            l => l.SpecialBookLabel, sb =>
                            {
                                sb.Ignore(l => l.Book);
                                sb.OwnsOne(l => l.AnotherBookLabel).Ignore(l => l.Book);
                            });
                        bb.OwnsOne(
                            l => l.AnotherBookLabel, ab =>
                            {
                                ab.Ignore(l => l.Book);
                                ab.OwnsOne(l => l.SpecialBookLabel).Ignore(l => l.Book).Ignore(s => s.BookLabel);
                            });
                    });

                var model = modelBuilder.FinalizeModel();

                VerifyOwnedBookLabelModel(model);
            }

            [ConditionalFact]
            public virtual void Can_configure_chained_ownerships_different_order()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Book>().OwnsOne(
                    b => b.Label, bb =>
                    {
                        bb.OwnsOne(
                            l => l.AnotherBookLabel, ab =>
                            {
                                ab.OwnsOne(l => l.SpecialBookLabel).Ignore(s => s.BookLabel).Ignore(l => l.Book);
                                ab.Ignore(l => l.Book);
                            });
                        bb.Ignore(l => l.Book);
                    });

                modelBuilder.Entity<Book>().OwnsOne(
                    b => b.AlternateLabel, bb =>
                    {
                        bb.OwnsOne(
                            l => l.AnotherBookLabel, ab =>
                            {
                                ab.OwnsOne(l => l.SpecialBookLabel).Ignore(s => s.BookLabel).Ignore(l => l.Book);
                                ab.Ignore(l => l.Book);
                            });
                        bb.Ignore(l => l.Book);
                    });

                modelBuilder.Entity<Book>().OwnsOne(
                    b => b.Label, bb =>
                    {
                        bb.OwnsOne(
                            l => l.SpecialBookLabel, sb =>
                            {
                                sb.OwnsOne(l => l.AnotherBookLabel).Ignore(l => l.Book);
                                sb.Ignore(l => l.Book);
                            });
                    });

                modelBuilder.Entity<Book>().OwnsOne(
                    b => b.AlternateLabel, bb =>
                    {
                        bb.OwnsOne(
                            l => l.SpecialBookLabel, sb =>
                            {
                                sb.OwnsOne(l => l.AnotherBookLabel).Ignore(l => l.Book);
                                sb.Ignore(l => l.Book);
                            });
                    });

                var model = modelBuilder.FinalizeModel();

                VerifyOwnedBookLabelModel(model);
            }

            [ConditionalFact]
            public virtual void Can_configure_hierarchy_with_reference_navigations_as_owned()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Owned<BookLabel>();
                modelBuilder.Entity<Book>();

                // SpecialBookLabel has an inverse to BookLabel making it ambiguous
                modelBuilder.Entity<Book>()
                    .OwnsOne(
                        b => b.Label, lb =>
                        {
                            lb.Ignore(l => l.Book);

                            lb.OwnsOne(b => b.AnotherBookLabel)
                                .Ignore(l => l.Book)
                                .OwnsOne(b => b.SpecialBookLabel)
                                .Ignore(l => l.Book)
                                .Ignore(l => l.BookLabel);
                            lb.OwnsOne(b => b.SpecialBookLabel)
                                .Ignore(l => l.Book)
                                .OwnsOne(b => b.AnotherBookLabel)
                                .Ignore(l => l.Book);
                        });

                modelBuilder.Entity<Book>()
                    .OwnsOne(
                        b => b.AlternateLabel, al =>
                        {
                            al.Ignore(l => l.Book);

                            al.OwnsOne(b => b.AnotherBookLabel)
                                .Ignore(l => l.Book)
                                .OwnsOne(b => b.SpecialBookLabel)
                                .Ignore(l => l.Book)
                                .Ignore(l => l.BookLabel);
                            al.OwnsOne(b => b.SpecialBookLabel)
                                .Ignore(l => l.Book)
                                .OwnsOne(b => b.AnotherBookLabel)
                                .Ignore(l => l.Book);
                        });

                var model = modelBuilder.FinalizeModel();

                VerifyOwnedBookLabelModel(model);
            }

            [ConditionalFact]
            public virtual void Can_configure_hierarchy_with_reference_navigations_as_owned_afterwards()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Book>();
                modelBuilder.Owned<BookLabel>();

                modelBuilder.Entity<Book>()
                    .OwnsOne(
                        b => b.Label, lb =>
                        {
                            lb.Ignore(l => l.Book);

                            lb.OwnsOne(b => b.AnotherBookLabel)
                                .Ignore(l => l.Book)
                                .OwnsOne(b => b.SpecialBookLabel)
                                .Ignore(l => l.Book)
                                .Ignore(l => l.BookLabel);
                            lb.OwnsOne(b => b.SpecialBookLabel)
                                .Ignore(l => l.Book)
                                .OwnsOne(b => b.AnotherBookLabel)
                                .Ignore(l => l.Book);
                        });

                modelBuilder.Entity<Book>()
                    .OwnsOne(
                        b => b.AlternateLabel, al =>
                        {
                            al.Ignore(l => l.Book);

                            al.OwnsOne(b => b.AnotherBookLabel)
                                .Ignore(l => l.Book)
                                .OwnsOne(b => b.SpecialBookLabel)
                                .Ignore(l => l.Book)
                                .Ignore(l => l.BookLabel);
                            al.OwnsOne(b => b.SpecialBookLabel)
                                .Ignore(l => l.Book)
                                .OwnsOne(b => b.AnotherBookLabel)
                                .Ignore(l => l.Book);
                        });

                var model = modelBuilder.FinalizeModel();

                VerifyOwnedBookLabelModel(model);
            }

            protected virtual void VerifyOwnedBookLabelModel(IModel model)
            {
                var bookOwnership1 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;

                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Single(bookOwnership1.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookOwnership1.DeclaringEntityType.GetForeignKeys());
                Assert.Null(bookOwnership1.DependentToPrincipal);
                Assert.Null(bookOwnership2.DependentToPrincipal);

                var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.SpecialBookLabel)).ForeignKey;

                Assert.Null(bookLabel1Ownership1.DependentToPrincipal);
                Assert.Equal(nameof(SpecialBookLabel.BookLabel), bookLabel1Ownership2.DependentToPrincipal.Name);
                Assert.Null(bookLabel2Ownership1.DependentToPrincipal);
                Assert.Equal(nameof(SpecialBookLabel.BookLabel), bookLabel2Ownership2.DependentToPrincipal.Name);

                var bookLabel1Ownership1Subownership = bookLabel1Ownership1.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel1Ownership2Subownership = bookLabel1Ownership2.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership1Subownership = bookLabel2Ownership1.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership2Subownership = bookLabel2Ownership2.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.AnotherBookLabel)).ForeignKey;

                Assert.NotSame(bookLabel1Ownership1.DeclaringEntityType, bookLabel2Ownership1.DeclaringEntityType);
                Assert.NotSame(bookLabel1Ownership2.DeclaringEntityType, bookLabel2Ownership2.DeclaringEntityType);
                Assert.Single(bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel1Ownership1Subownership.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel1Ownership2Subownership.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel2Ownership1Subownership.DeclaringEntityType.GetForeignKeys());
                Assert.Single(bookLabel2Ownership2Subownership.DeclaringEntityType.GetForeignKeys());
                Assert.Equal(nameof(SpecialBookLabel.AnotherBookLabel), bookLabel1Ownership1Subownership.DependentToPrincipal.Name);
                Assert.Equal(nameof(AnotherBookLabel.SpecialBookLabel), bookLabel1Ownership2Subownership.DependentToPrincipal.Name);
                Assert.Equal(nameof(SpecialBookLabel.AnotherBookLabel), bookLabel2Ownership1Subownership.DependentToPrincipal.Name);
                Assert.Equal(nameof(AnotherBookLabel.SpecialBookLabel), bookLabel2Ownership2Subownership.DependentToPrincipal.Name);

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));
            }

            [ConditionalFact]
            public virtual void Can_configure_self_ownership()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Book>();
                modelBuilder.Ignore<SpecialBookLabel>();
                modelBuilder.Entity<BookLabel>().OwnsOne(l => l.AnotherBookLabel, ab => ab.OwnsOne(l => l.AnotherBookLabel));

                var model = modelBuilder.FinalizeModel();

                var bookLabelOwnership = model.FindEntityType(typeof(BookLabel)).FindNavigation(nameof(BookLabel.AnotherBookLabel))
                    .ForeignKey;
                var selfOwnership = bookLabelOwnership.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                Assert.NotSame(selfOwnership.PrincipalEntityType, selfOwnership.DeclaringEntityType);
                Assert.Equal(selfOwnership.PrincipalEntityType.Name, selfOwnership.DeclaringEntityType.Name);
                Assert.True(selfOwnership.IsOwnership);
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
            }

            [ConditionalFact]
            public virtual void Reconfiguring_entity_type_as_owned_throws()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<CustomerDetails>();

                Assert.Equal(
                    CoreStrings.ClashingNonOwnedEntityType(nameof(CustomerDetails)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details)).Message);
            }

            [ConditionalFact]
            public virtual void Reconfiguring_owned_type_as_non_owned_throws()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Customer>();
                var entityType = modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details).OwnedEntityType;

                Assert.Equal(
                    CoreStrings.ClashingOwnedEntityType(nameof(CustomerDetails)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<SpecialCustomer>().HasOne(c => c.Details)).Message);
            }

            [ConditionalFact]
            public virtual void Deriving_from_owned_type_throws()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Book>()
                    .Ignore(b => b.AlternateLabel)
                    .Ignore(b => b.Details)
                    .OwnsOne(
                        b => b.Label, lb =>
                        {
                            lb.Ignore(l => l.AnotherBookLabel);
                            lb.Ignore(l => l.SpecialBookLabel);
                        });

                Assert.Equal(
                    CoreStrings.ClashingOwnedEntityType(nameof(AnotherBookLabel)),
                    Assert.Throws<InvalidOperationException>(
                        () => modelBuilder.Entity<AnotherBookLabel>()).Message);
            }

            [ConditionalFact]
            public virtual void Configuring_base_type_as_owned_throws()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<AnotherBookLabel>();

                modelBuilder.Entity<Book>()
                    .Ignore(b => b.AlternateLabel)
                    .Ignore(b => b.Details);

                Assert.Equal(
                    CoreStrings.ClashingNonOwnedDerivedEntityType(nameof(BookLabel), nameof(AnotherBookLabel)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            modelBuilder.Entity<Book>().OwnsOne(c => c.Label)).Message);
            }

            [ConditionalFact]
            public virtual void CLR_base_type_can_be_owned_when_not_in_hierarchy()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<AnotherBookLabel>()
                    .HasBaseType(null)
                    .Ignore(l => l.Book)
                    .Ignore(l => l.SpecialBookLabel)
                    .Ignore(l => l.AnotherBookLabel);

                modelBuilder.Entity<Book>()
                    .Ignore(b => b.AlternateLabel)
                    .Ignore(b => b.Details)
                    .OwnsOne(
                        c => c.Label, lb =>
                        {
                            lb.Ignore(l => l.AnotherBookLabel);
                            lb.Ignore(l => l.SpecialBookLabel);
                        });

                var model = modelBuilder.FinalizeModel();

                var bookLabelOwnership = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label))
                    .ForeignKey;

                Assert.True(bookLabelOwnership.IsOwnership);
                Assert.Equal(nameof(BookLabel.Book), bookLabelOwnership.DependentToPrincipal.Name);

                Assert.Null(model.FindEntityType(typeof(AnotherBookLabel)).BaseType);
            }

            [ConditionalFact]
            public virtual void OwnedType_can_derive_from_Collection()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<PrincipalEntity>().OwnsOne(o => o.InverseNav);

                var model = modelBuilder.FinalizeModel();

                Assert.Single(model.GetEntityTypes(typeof(List<DependentEntity>)));
            }

            [ConditionalFact]
            public virtual void Weak_types_with_FK_to_another_entity_works()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Country>();
                var ownerEntityTypeBuilder = modelBuilder.Entity<BillingOwner>();
                ownerEntityTypeBuilder.OwnsOne(
                    e => e.Bill1,
                    o => o.HasOne<Country>().WithMany().HasPrincipalKey(c => c.Name).HasForeignKey(d => d.Country));

                ownerEntityTypeBuilder.OwnsOne(
                    e => e.Bill2,
                    o => o.HasOne<Country>().WithMany().HasPrincipalKey(c => c.Name).HasForeignKey(d => d.Country));

                var model = modelBuilder.FinalizeModel();

                Assert.Equal(4, model.GetEntityTypes().Count());
            }

            [ConditionalFact]
            public virtual void Inheritance_where_base_has_multiple_owned_types_works()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<BaseOwner>();
                modelBuilder.Entity<DerivedOwner>();

                var model = modelBuilder.FinalizeModel();

                Assert.Equal(4, model.GetEntityTypes().Count());
            }
        }
    }
}

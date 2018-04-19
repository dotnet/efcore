// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class OwnedTypesTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Can_declare_owned_type()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var entityBuilder = modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);
                entityBuilder.Property(d => d.CustomerId);

                modelBuilder.Validate();

                var owner = model.FindEntityType(typeof(Customer));
                var ownership = owner.FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.Equal(typeof(Customer).FullName, owner.Name);
                Assert.Same(entityBuilder.OwnedEntityType, ownership.DeclaringEntityType);
                Assert.Equal(nameof(Customer.Details), ownership.PrincipalToDependent.Name);
                Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
                Assert.Equal(1, ownership.DeclaringEntityType.GetForeignKeys().Count());
            }

            [Fact]
            public virtual void Can_use_nested_closure()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().OwnsOne(
                    c => c.Details,
                    r => r.HasEntityTypeAnnotation("foo", "bar")
                        .HasForeignKeyAnnotation("bar", "foo"));

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.Equal("bar", ownership.DeclaringEntityType.FindAnnotation("foo").Value);
                Assert.Equal(1, ownership.DeclaringEntityType.GetForeignKeys().Count());
            }

            [Fact]
            public virtual void Can_configure_inverse()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);

                modelBuilder.Validate();

                var owner = model.FindEntityType(typeof(Customer));
                var ownee = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Equal(nameof(CustomerDetails.CustomerId), ownee.FindPrimaryKey().Properties.Single().Name);

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                    .HasOne(d => d.Customer);

                modelBuilder.Validate();

                var ownership = owner.FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.Equal(nameof(CustomerDetails.Customer), ownership.DependentToPrincipal.Name);
                Assert.Same(ownee, ownership.DeclaringEntityType);
                Assert.Equal(nameof(CustomerDetails.CustomerId), ownee.FindPrimaryKey().Properties.Single().Name);
                Assert.Equal(1, ownership.DeclaringEntityType.GetForeignKeys().Count());
            }

            [Fact]
            public virtual void Can_configure_owned_type_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                    .Ignore(d => d.Id)
                    .Property<int>("foo");

                modelBuilder.Validate();

                var owner = model.FindEntityType(typeof(Customer));
                var ownee = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Null(owner.FindProperty("foo"));
                Assert.Equal(new[] { nameof(CustomerDetails.CustomerId), "foo" }, ownee.GetProperties().Select(p => p.Name).ToArray());
            }

            [Fact]
            public virtual void Can_configure_single_owned_type_using_attribute()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<SpecialOrder>();

                modelBuilder.Validate();

                var owner = model.FindEntityType(typeof(SpecialOrder));
                var ownership = owner.FindNavigation(nameof(SpecialOrder.ShippingAddress)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.NotNull(ownership.DeclaringEntityType.FindProperty(nameof(StreetAddress.Street)));
            }

            [Fact]
            public virtual void Can_configure_ownership_foreign_key()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                    .HasForeignKey(c => c.Id);

                modelBuilder.Validate();

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.Equal(nameof(CustomerDetails.Id), ownership.Properties.Single().Name);
                Assert.Equal(nameof(CustomerDetails.Id), ownership.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);
                Assert.Equal(1, ownership.DeclaringEntityType.GetForeignKeys().Count());
            }

            [Fact]
            public virtual void Can_configure_multiple_ownerships()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<OtherCustomer>().OwnsOne(c => c.Details);
                modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details);

                modelBuilder.Validate();

                var ownership1 = model.FindEntityType(typeof(OtherCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                var ownership2 = model.FindEntityType(typeof(SpecialCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.Equal(typeof(CustomerDetails), ownership1.DeclaringEntityType.ClrType);
                Assert.Equal(typeof(CustomerDetails), ownership2.DeclaringEntityType.ClrType);
                Assert.NotSame(ownership1.DeclaringEntityType, ownership2.DeclaringEntityType);
                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
                Assert.Equal(1, ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, ownership2.DeclaringEntityType.GetForeignKeys().Count());
            }

            [Fact]
            public virtual void Can_configure_one_to_one_relationship_from_an_owned_type()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<OtherCustomer>().OwnsOne(c => c.Details)
                    .HasOne<SpecialCustomer>()
                    .WithOne()
                    .HasPrincipalKey<SpecialCustomer>();

                Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));

                modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details);

                modelBuilder.Validate();

                var ownership = model.FindEntityType(typeof(OtherCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                var foreignKey = model.FindEntityType(typeof(SpecialCustomer)).GetReferencingForeignKeys()
                    .Single(fk => fk.DeclaringEntityType.ClrType == typeof(CustomerDetails)
                                  && fk.PrincipalToDependent == null);
                Assert.Same(ownership.DeclaringEntityType, foreignKey.DeclaringEntityType);
                Assert.NotEqual(ownership.Properties.Single().Name, foreignKey.Properties.Single().Name);
                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
                Assert.Equal(2, ownership.DeclaringEntityType.GetForeignKeys().Count());
            }

            [Fact]
            public virtual void Can_configure_fk_on_multiple_ownerships()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Ignore<AnotherBookLabel>();
                modelBuilder.Ignore<SpecialBookLabel>();
                modelBuilder.Ignore<BookDetails>();

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label, l =>
                {
                    l.HasForeignKey("BookLabelId");
                    l.HasForeignKeyAnnotation("Foo", "Bar");
                });
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel).HasForeignKey("BookLabelId");

                modelBuilder.Validate();

                var bookOwnership1 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Equal(typeof(int), bookOwnership1.DeclaringEntityType.GetForeignKeys().Single().Properties.Single().ClrType);
                Assert.Equal(typeof(int), bookOwnership1.DeclaringEntityType.GetForeignKeys().Single().Properties.Single().ClrType);
                Assert.Equal("Bar", bookOwnership1["Foo"]);

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(3, model.GetEntityTypes().Count());
            }

            [Fact]
            public virtual void Can_map_base_of_owned_type()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);
                modelBuilder.Entity<BookDetailsBase>();
                modelBuilder.Ignore<SpecialBookLabel>();

                modelBuilder.Validate();

                Assert.NotNull(model.FindEntityType(typeof(BookDetailsBase)));
                var owner = model.FindEntityType(typeof(Customer));
                var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Null(owned.BaseType);
                Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
                Assert.Equal(1, owned.GetForeignKeys().Count());
            }

            [Fact]
            public virtual void Can_map_base_of_owned_type_first()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<BookDetailsBase>();
                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);
                modelBuilder.Ignore<SpecialBookLabel>();

                modelBuilder.Validate();

                Assert.NotNull(model.FindEntityType(typeof(BookDetailsBase)));
                var owner = model.FindEntityType(typeof(Customer));
                var owned = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Null(owned.BaseType);
                Assert.NotNull(model.FindEntityType(typeof(CustomerDetails)));
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
                Assert.Equal(1, owned.GetForeignKeys().Count());
            }

            [Fact]
            public virtual void Can_map_derived_of_owned_type()
            {
                var modelBuilder = CreateModelBuilder();
                var model = (Model)modelBuilder.Model;

                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<OrderCombination>().OwnsOne(c => c.Details);
                modelBuilder.Entity<Customer>();

                var owner = model.FindEntityType(typeof(OrderCombination));
                var owned = owner.FindNavigation(nameof(OrderCombination.Details)).ForeignKey.DeclaringEntityType;
                Assert.NotEmpty(owned.GetDirectlyDerivedTypes());
                Assert.Empty(model.GetEntityTypes().SelectMany(e => e.GetDeclaredNavigations()).Where(n =>
                {
                    var targetType = n.GetTargetType().ClrType;
                    return targetType != typeof(DetailsBase) && typeof(DetailsBase).IsAssignableFrom(targetType);
                }));
                Assert.Equal(1, owned.GetForeignKeys().Count());
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(DetailsBase)));
                Assert.Same(owned, model.FindEntityType(typeof(CustomerDetails)).BaseType);

                modelBuilder.Entity<Customer>().Ignore(c => c.Details);
                modelBuilder.Entity<Order>().Ignore(c => c.Details);
                modelBuilder.Validate();
            }

            [Fact]
            public virtual void Can_map_derived_of_owned_type_first()
            {
                var modelBuilder = CreateModelBuilder();
                var model = (Model)modelBuilder.Model;

                modelBuilder.Entity<OrderCombination>().OwnsOne(c => c.Details);

                var owner = model.FindEntityType(typeof(OrderCombination));
                var owned = owner.FindNavigation(nameof(OrderCombination.Details)).ForeignKey.DeclaringEntityType;
                Assert.NotEmpty(owned.GetDirectlyDerivedTypes());
                Assert.Empty(model.GetEntityTypes().SelectMany(e => e.GetDeclaredNavigations()).Where(n =>
                {
                    var targetType = n.GetTargetType().ClrType;
                    return targetType != typeof(DetailsBase) && typeof(DetailsBase).IsAssignableFrom(targetType);
                }));
                Assert.Equal(1, owned.GetForeignKeys().Count());
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(DetailsBase)));
                Assert.Same(owned, model.FindEntityType(typeof(CustomerDetails)).BaseType);

                modelBuilder.Entity<Customer>().Ignore(c => c.Details);
                modelBuilder.Entity<Order>().Ignore(c => c.Details);
                modelBuilder.Validate();
            }

            [Fact]
            public virtual void Can_configure_chained_ownerships()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label, bb =>
                {
                    bb.OwnsOne(l => l.AnotherBookLabel, ab =>
                    {
                        ab.OwnsOne(l => l.SpecialBookLabel);
                    });
                    bb.OwnsOne(l => l.SpecialBookLabel, sb =>
                    {
                        sb.OwnsOne(l => l.AnotherBookLabel);
                    });
                });

                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel, bb =>
                {
                    bb.OwnsOne(l => l.SpecialBookLabel, sb =>
                    {
                        sb.OwnsOne(l => l.AnotherBookLabel);
                    });
                    bb.OwnsOne(l => l.AnotherBookLabel, ab =>
                    {
                        ab.OwnsOne(l => l.SpecialBookLabel);
                    });
                });

                modelBuilder.Validate();

                VerifyOwnedBookLabelModel(modelBuilder.Model);
            }

            [Fact]
            public virtual void Can_configure_chained_ownerships_different_order()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label, bb =>
                {
                    bb.OwnsOne(l => l.AnotherBookLabel, ab =>
                    {
                        ab.OwnsOne(l => l.SpecialBookLabel);
                    });
                });

                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel, bb =>
                {
                    bb.OwnsOne(l => l.AnotherBookLabel, ab =>
                    {
                        ab.OwnsOne(l => l.SpecialBookLabel);
                    });
                });

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label, bb =>
                {
                    bb.OwnsOne(l => l.SpecialBookLabel, sb =>
                    {
                        sb.OwnsOne(l => l.AnotherBookLabel);
                    });
                });

                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel, bb =>
                {
                    bb.OwnsOne(l => l.SpecialBookLabel, sb =>
                    {
                        sb.OwnsOne(l => l.AnotherBookLabel);
                    });
                });

                modelBuilder.Validate();

                VerifyOwnedBookLabelModel(modelBuilder.Model);
            }

            [Fact]
            public virtual void Can_configure_all_ownerships_with_one_call()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Owned<BookLabel>();
                modelBuilder.Entity<Book>().OwnsOne(b => b.Label);
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel);

                modelBuilder.Validate();

                VerifyOwnedBookLabelModel(modelBuilder.Model);
            }

            [Fact]
            public virtual void Can_configure_all_ownerships_with_one_call_afterwards()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Book>();
                modelBuilder.Owned<BookLabel>();
                modelBuilder.Entity<Book>().OwnsOne(b => b.Label);
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel);

                modelBuilder.Validate();

                VerifyOwnedBookLabelModel(modelBuilder.Model);
            }

            [Fact]
            public virtual void Collection_navigation_to_owned_entity_type_is_ignored()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Owned<Order>();
                modelBuilder.Owned<SpecialOrder>();
                var customerBuilder = modelBuilder.Entity<SpecialCustomer>();

                Assert.Equal(CoreStrings.NavigationNotAdded(
                    nameof(Customer), nameof(Customer.Orders), "IEnumerable<Order>"),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Validate()).Message);

                Assert.Null(customerBuilder.Metadata.FindNavigation(nameof(SpecialCustomer.Orders)));
                Assert.Null(customerBuilder.Metadata.FindNavigation(nameof(SpecialCustomer.SpecialOrders)));
                Assert.Equal(0, modelBuilder.Model.GetEntityTypes(typeof(Order)).Count);
                Assert.Null(((Model)modelBuilder.Model).FindIgnoredTypeConfigurationSource(typeof(Order)));
                Assert.Null(((Model)modelBuilder.Model).FindIgnoredTypeConfigurationSource(typeof(SpecialOrder)));
            }

            protected virtual void VerifyOwnedBookLabelModel(IMutableModel model)
            {
                var bookOwnership1 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Equal(1, bookOwnership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookOwnership1.DeclaringEntityType.GetForeignKeys().Count());

                var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(
                    nameof(BookLabel.SpecialBookLabel)).ForeignKey;

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
                Assert.Equal(1, bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel1Ownership1Subownership.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel1Ownership2Subownership.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership1Subownership.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership2Subownership.DeclaringEntityType.GetForeignKeys().Count());

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));
            }

            [Fact]
            public virtual void Can_configure_self_ownership()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Book>();
                modelBuilder.Ignore<SpecialBookLabel>();
                modelBuilder.Entity<BookLabel>().OwnsOne(l => l.AnotherBookLabel, ab => { ab.OwnsOne(l => l.AnotherBookLabel); });

                modelBuilder.Validate();

                var model = modelBuilder.Model;

                var bookLabelOwnership = model.FindEntityType(typeof(BookLabel)).FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var selfOwnership = bookLabelOwnership.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                Assert.NotSame(selfOwnership.PrincipalEntityType, selfOwnership.DeclaringEntityType);
                Assert.Equal(selfOwnership.PrincipalEntityType.Name, selfOwnership.DeclaringEntityType.Name);
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
            }

            [Fact]
            public virtual void Reconfiguring_entity_type_as_owned_throws()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Customer>();
                modelBuilder.Entity<CustomerDetails>();
                modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details);
                modelBuilder.Entity<OtherCustomer>().OwnsOne(c => c.Details);

                Assert.Equal(2, modelBuilder.Model.GetEntityTypes(typeof(CustomerDetails)).Count);
            }

            [Fact]
            public virtual void OwnedType_can_derive_from_Collection()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<PrincipalEntity>().OwnsOne(o => o.InverseNav);

                Assert.Single(modelBuilder.Model.GetEntityTypes(typeof(List<DependentEntity>)));
            }
        }
    }
}

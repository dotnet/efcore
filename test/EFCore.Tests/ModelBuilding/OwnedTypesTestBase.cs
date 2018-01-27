// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
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

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label).HasForeignKey("BookLabelId");
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel).HasForeignKey("BookLabelId");

                modelBuilder.Validate();

                var bookOwnership1 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Equal(typeof(int), bookOwnership1.DeclaringEntityType.GetForeignKeys().Single().Properties.Single().ClrType);
                Assert.Equal(typeof(int), bookOwnership1.DeclaringEntityType.GetForeignKeys().Single().Properties.Single().ClrType);

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(3, model.GetEntityTypes().Count());
            }

            [Fact]
            public virtual void Can_configure_chained_ownerships()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label, bb =>
                {
                    bb.OwnsOne(l => l.AnotherBookLabel, ab =>
                    {
                        ab.Ignore(a => a.Book);
                        ab.OwnsOne(l => l.SpecialBookLabel)
                            .Ignore(s => s.Book);
                    });
                    bb.OwnsOne(l => l.SpecialBookLabel, sb =>
                    {
                        sb.Ignore(s => s.Book);
                        sb.OwnsOne(l => l.AnotherBookLabel)
                            .Ignore(a => a.Book);
                    });
                });
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel, bb =>
                {
                    bb.OwnsOne(l => l.SpecialBookLabel, sb =>
                    {
                        sb.Ignore(s => s.Book);
                        sb.OwnsOne(l => l.AnotherBookLabel)
                            .Ignore(s => s.Book);
                    });
                    bb.OwnsOne(l => l.AnotherBookLabel, ab =>
                    {
                        ab.Ignore(a => a.Book);
                        ab.OwnsOne(l => l.SpecialBookLabel)
                            .Ignore(a => a.Book);
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
                        ab.Ignore(a => a.Book);
                        ab.OwnsOne(l => l.SpecialBookLabel)
                            .Ignore(s => s.Book);
                    });
                });
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel, bb =>
                {
                    bb.OwnsOne(l => l.AnotherBookLabel, ab =>
                    {
                        ab.Ignore(a => a.Book);
                        ab.OwnsOne(l => l.SpecialBookLabel)
                            .Ignore(s => s.Book);
                    });
                });

                modelBuilder.Entity<Book>().OwnsOne(b => b.Label, bb =>
                {
                    bb.OwnsOne(l => l.SpecialBookLabel, sb =>
                    {
                        sb.Ignore(s => s.Book);
                        sb.OwnsOne(l => l.AnotherBookLabel)
                            .Ignore(a => a.Book);
                    });
                });
                modelBuilder.Entity<Book>().OwnsOne(b => b.AlternateLabel, bb =>
                {
                    bb.OwnsOne(l => l.SpecialBookLabel, sb =>
                    {
                        sb.Ignore(s => s.Book);
                        sb.OwnsOne(l => l.AnotherBookLabel)
                            .Ignore(a => a.Book);
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
                modelBuilder.Owned<SpecialBookLabel>();
                modelBuilder.Owned<AnotherBookLabel>();
                modelBuilder.Entity<Book>().OwnsOne(b => b.Label);

                modelBuilder.Validate();

                VerifyOwnedBookLabelModel(modelBuilder.Model);
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
                modelBuilder.Entity<BookLabel>().OwnsOne(l => l.AnotherBookLabel, ab =>
                {
                    ab.OwnsOne(l => l.AnotherBookLabel);
                });

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
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Tests
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
                Assert.Null(model.FindEntityType(typeof(CustomerDetails)));
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
                    .WithOne(c => c.Details)
                    .HasPrincipalKey<SpecialCustomer>();

                modelBuilder.Validate();

                var ownership = model.FindEntityType(typeof(OtherCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                var foreignKey = model.FindEntityType(typeof(SpecialCustomer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.Same(ownership.DeclaringEntityType, foreignKey.DeclaringEntityType);
                Assert.NotEqual(ownership.Properties.Single().Name, foreignKey.Properties.Single().Name);
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
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
                var model = modelBuilder.Model;

                modelBuilder.Entity<Book>()
                    .OwnsOne(b => b.Label)
                    .OwnsOne(l => l.AnotherBookLabel);
                modelBuilder.Entity<Book>()
                    .OwnsOne(b => b.AlternateLabel)
                    .OwnsOne(l => l.SpecialBookLabel);

                modelBuilder.Validate();

                var bookOwnership1 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.Label)).ForeignKey;
                var bookOwnership2 = model.FindEntityType(typeof(Book)).FindNavigation(nameof(Book.AlternateLabel)).ForeignKey;
                Assert.NotSame(bookOwnership1.DeclaringEntityType, bookOwnership2.DeclaringEntityType);
                Assert.Equal(1, bookOwnership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookOwnership1.DeclaringEntityType.GetForeignKeys().Count());

                var bookLabel1Ownership1 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel1Ownership2 = bookOwnership1.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                var bookLabel2Ownership1 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.AnotherBookLabel)).ForeignKey;
                var bookLabel2Ownership2 = bookOwnership2.DeclaringEntityType.FindNavigation(nameof(BookLabel.SpecialBookLabel)).ForeignKey;
                Assert.NotSame(bookLabel1Ownership1.DeclaringEntityType, bookLabel2Ownership1.DeclaringEntityType);
                Assert.NotSame(bookLabel1Ownership2.DeclaringEntityType, bookLabel2Ownership2.DeclaringEntityType);
                Assert.Equal(1, bookLabel1Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel1Ownership2.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership1.DeclaringEntityType.GetForeignKeys().Count());
                Assert.Equal(1, bookLabel2Ownership2.DeclaringEntityType.GetForeignKeys().Count());

                Assert.Equal(2, model.GetEntityTypes().Count(e => e.ClrType == typeof(BookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(AnotherBookLabel)));
                Assert.Equal(4, model.GetEntityTypes().Count(e => e.ClrType == typeof(SpecialBookLabel)));
            }
        }
    }
}

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
            //[Fact]
            public virtual void Can_declare_owned_type()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var entityBuilder = modelBuilder.Entity<Customer>().OwnsOne(c => c.Details);

                var owner = model.FindEntityType(typeof(Customer));
                var ownership = owner.FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.Equal(typeof(Customer).FullName, owner.Name);
                Assert.Same(entityBuilder.Metadata, ownership.DeclaringEntityType);
                Assert.Equal(nameof(Customer.Details), ownership.PrincipalToDependent.Name);
                Assert.Null(model.FindEntityType(typeof(CustomerDetails)));
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(CustomerDetails)));
            }

            //[Fact]
            public virtual void Can_use_nested_closure()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().OwnsOne(
                    c => c.Details,
                    r => r.HasEntityTypeAnnotation("foo", "bar")
                        .HasForeignKeyAnnotation("bar", "foo"));

                var ownership = model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Details)).ForeignKey;
                Assert.Equal("bar", ownership.DeclaringEntityType.FindAnnotation("foo").Value);
                Assert.Equal("foo", ownership.FindAnnotation("bar").Value);
            }

            //[Fact]
            public virtual void Can_configure_owned_type_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().OwnsOne(c => c.Details)
                    .Property<int>("foo");

                var owner = model.FindEntityType(typeof(Customer));
                var ownee = owner.FindNavigation(nameof(Customer.Details)).ForeignKey.DeclaringEntityType;
                Assert.Null(owner.FindProperty("foo"));
                Assert.NotNull(ownee.FindProperty("foo"));
            }
        }
    }
}

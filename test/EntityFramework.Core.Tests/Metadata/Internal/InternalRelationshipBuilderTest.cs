// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.Internal
{
    public class InternalRelationshipBuilderTest
    {
        [Fact]
        public void ForeignKey_returns_same_instance_for_same_properties()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            orderEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true)
                .HasForeignKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(relationshipBuilder, customerEntityBuilder.Relationship(typeof(Order), typeof(Customer), null, null, ConfigurationSource.Convention, true)
                .HasForeignKey(typeof(Order), new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void ForeignKey_overrides_incompatible_lower_or_equal_source_required()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true);
            relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);

            var nullableId = orderEntityBuilder.Property("NullableId", typeof(int?), ConfigurationSource.Explicit);
            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { nullableId.Metadata.Name }, ConfigurationSource.Convention);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.Equal(new[] { nullableId.Metadata.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            Assert.Null(relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty }, ConfigurationSource.Convention));
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.Equal(new[] { nullableId.Metadata.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.Equal(new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));
        }

        [Fact]
        public void ForeignKey_overrides_incompatible_lower_or_equal_source_principal_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true);
            relationshipBuilder = relationshipBuilder.HasPrincipalKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);

            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty }, ConfigurationSource.Convention);
            Assert.Equal(new[] { Customer.IdProperty.Name },
                relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
            Assert.Equal(new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            Assert.Null(relationshipBuilder.HasForeignKey(new[] { Order.CustomerUniqueProperty }, ConfigurationSource.Convention));
            Assert.Equal(new[] { Customer.IdProperty.Name },
                relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
            Assert.Equal(new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation);
            Assert.NotEqual(new[] { Customer.IdProperty.Name },
                relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
            Assert.Equal(new[] { Order.CustomerUniqueProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));
        }

        [Fact]
        public void PrincipalKey_does_not_return_same_instance_for_same_properties()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            orderEntityBuilder.PrimaryKey(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.DataAnnotation, true)
                .HasPrincipalKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation);

            Assert.NotNull(relationshipBuilder);
            Assert.NotSame(relationshipBuilder, orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.DataAnnotation, true)
                .HasPrincipalKey(typeof(Order), new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name }, ConfigurationSource.DataAnnotation));

            Assert.Equal(2, customerEntityBuilder.Metadata.GetForeignKeys().Count());
        }

        [Fact]
        public void PrincipalKey_overrides_incompatible_lower_or_equal_source_dependent_properties()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true);
            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty, Order.CustomerUniqueProperty }, ConfigurationSource.DataAnnotation);

            relationshipBuilder = relationshipBuilder.HasPrincipalKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Convention);
            Assert.Equal(new[] { Customer.IdProperty.Name, Customer.UniqueProperty.Name },
                relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
            Assert.Equal(new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            Assert.Null(relationshipBuilder.HasPrincipalKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention));
            Assert.Equal(new[] { Customer.IdProperty.Name, Customer.UniqueProperty.Name },
                relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
            Assert.Equal(new[] { Order.CustomerIdProperty.Name, Order.CustomerUniqueProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            relationshipBuilder = relationshipBuilder.HasPrincipalKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);
            Assert.Equal(new[] { Customer.IdProperty.Name },
                relationshipBuilder.Metadata.PrincipalKey.Properties.Select(p => p.Name));
            Assert.NotEqual(new[] { Order.CustomerUniqueProperty.Name, Order.CustomerUniqueProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));
        }

        [Fact]
        public void Can_only_override_lower_source_Unique()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true, true);
            Assert.True(relationshipBuilder.Metadata.IsUnique.Value);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.IsUnique(false, ConfigurationSource.DataAnnotation);
            Assert.NotNull(relationshipBuilder);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            Assert.Null(relationshipBuilder.IsUnique(true, ConfigurationSource.Convention));
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsUnique);
        }

        [Fact]
        public void Can_only_override_existing_Unique_value_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var customerKeyBuilder = customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var foreignKey = orderEntityBuilder.Metadata.AddForeignKey(
                new[]
                    {
                        orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata,
                        orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata
                    },
                customerKeyBuilder.Metadata,
                customerEntityBuilder.Metadata);
            foreignKey.IsUnique = true;

            var relationshipBuilder = orderEntityBuilder.Relationship(foreignKey, true, ConfigurationSource.Convention);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            Assert.Null(relationshipBuilder.IsUnique(false, ConfigurationSource.Convention));
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.IsUnique(false, ConfigurationSource.Explicit);
            Assert.NotNull(relationshipBuilder);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsUnique);
        }
        
        [Fact]
        public void Unique_overrides_incompatible_lower_or_equal_source_principalToDependent()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), "Customer", "NotCollectionOrders", ConfigurationSource.DataAnnotation);
            Assert.True(relationshipBuilder.Metadata.IsUnique.Value);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);

            relationshipBuilder = relationshipBuilder.IsUnique(true, ConfigurationSource.Convention);
            Assert.True(relationshipBuilder.Metadata.IsUnique.Value);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);
            Assert.NotNull(relationshipBuilder.Metadata.PrincipalToDependent);

            Assert.Null(relationshipBuilder.IsUnique(false, ConfigurationSource.Convention));
            Assert.True(relationshipBuilder.Metadata.IsUnique.Value);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsUnique);
            Assert.NotNull(relationshipBuilder.Metadata.PrincipalToDependent);

            relationshipBuilder = relationshipBuilder.IsUnique(false, ConfigurationSource.DataAnnotation);
            Assert.False(relationshipBuilder.Metadata.IsUnique.Value);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsUnique);
            Assert.Null(relationshipBuilder.Metadata.PrincipalToDependent);
        }
        
        [Fact]
        public void Can_only_override_lower_source_Required()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true);
            Assert.Null(relationshipBuilder.Metadata.IsRequired);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            relationshipBuilder = relationshipBuilder.IsRequired(true, ConfigurationSource.Convention);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);

            Assert.Null(relationshipBuilder.IsRequired(true, ConfigurationSource.Convention));
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
        }

        [Fact]
        public void Can_only_override_existing_Required_value_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var pk = customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit).Metadata;
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            var customerIdProperty = orderEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata;
            var customerUniqueProperty = orderEntityBuilder.Property(Order.CustomerUniqueProperty, ConfigurationSource.Convention).Metadata;
            var fk = orderEntityBuilder.Metadata.AddForeignKey(
                new[] { customerIdProperty, customerUniqueProperty },
                pk,
                customerEntityBuilder.Metadata);
            fk.IsRequired = true;

            var relationshipBuilder = orderEntityBuilder.Relationship(fk, true, ConfigurationSource.Explicit);
            Assert.Null(relationshipBuilder.IsRequired(false, ConfigurationSource.Convention));
            Assert.True(fk.IsRequired);
            Assert.False(customerIdProperty.IsNullable);
            Assert.False(customerUniqueProperty.IsNullable);

            relationshipBuilder = relationshipBuilder.IsRequired(true, ConfigurationSource.Convention);
            Assert.NotNull(relationshipBuilder);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.False(customerIdProperty.IsNullable);
            Assert.False(customerUniqueProperty.IsNullable);

            relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.Explicit);
            Assert.NotNull(relationshipBuilder);
            fk = relationshipBuilder.Metadata;
            Assert.False(fk.IsRequired);
            Assert.False(customerIdProperty.IsNullable);
            Assert.True(customerUniqueProperty.IsNullable);
            Assert.Same(customerIdProperty, fk.Properties[0]);
            Assert.Same(customerUniqueProperty, fk.Properties[1]);
        }

        [Fact]
        public void Required_overrides_incompatible_lower_or_equal_source_properties()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder.Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, true);
            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation);
            Assert.Null(relationshipBuilder.Metadata.IsRequired);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.Equal(new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            relationshipBuilder = relationshipBuilder.IsRequired(true, ConfigurationSource.Convention);
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.Equal(new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            Assert.Null(relationshipBuilder.IsRequired(false, ConfigurationSource.Convention));
            Assert.True(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.Equal(new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));

            relationshipBuilder = relationshipBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);
            Assert.False(((IForeignKey)relationshipBuilder.Metadata).IsRequired);
            Assert.NotEqual(new[] { Order.CustomerIdProperty.Name },
                relationshipBuilder.Metadata.Properties.Select(p => p.Name));
        }

        [Fact]
        public void Can_only_invert_same_or_lower_source()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var customerEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            customerEntityBuilder.PrimaryKey(new[] { Customer.IdProperty, Customer.UniqueProperty }, ConfigurationSource.Explicit);
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var relationshipBuilder = orderEntityBuilder
                .Relationship(typeof(Customer), typeof(Order), null, null, ConfigurationSource.Convention, isUnique: true);

            Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

            relationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.Convention);
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

            relationshipBuilder = relationshipBuilder.HasPrincipalKey(
                orderEntityBuilder.Metadata.GetKeys().Single().Properties,
                ConfigurationSource.Convention);

            Assert.Null(relationshipBuilder.Invert(ConfigurationSource.Convention));
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

            relationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.DataAnnotation);
            Assert.Same(orderEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

            relationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.DataAnnotation);
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

            Assert.Null(relationshipBuilder.Invert(ConfigurationSource.Convention));
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);

            relationshipBuilder = relationshipBuilder.HasForeignKey(new[] { Customer.IdProperty }, ConfigurationSource.DataAnnotation);

            Assert.Null(relationshipBuilder.Invert(ConfigurationSource.DataAnnotation));
            Assert.Same(customerEntityBuilder.Metadata, relationshipBuilder.Metadata.DeclaringEntityType);
        }

        private InternalModelBuilder CreateInternalModelBuilder() => new InternalModelBuilder(new Model(), new ConventionSet());

        private class Order
        {
            public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");
            public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty("CustomerUnique");
            public static readonly PropertyInfo CustomerProperty = typeof(Order).GetProperty("Customer");

            public int Id { get; set; }
            public int CustomerId { get; set; }
            public Guid? CustomerUnique { get; set; }
            public Customer Customer { get; set; }

            public Order OrderCustomer { get; set; }
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");
            public static readonly PropertyInfo UniqueProperty = typeof(Customer).GetProperty("Unique");
            public static readonly PropertyInfo OrdersProperty = typeof(Customer).GetProperty("Orders");

            public int Id { get; set; }
            public Guid Unique { get; set; }
            public string Name { get; set; }
            public string Mane { get; set; }
            public ICollection<Order> Orders { get; set; }

            public IEnumerable<Order> EnumerableOrders { get; set; }
            public Order NotCollectionOrders { get; set; }
        }
    }
}

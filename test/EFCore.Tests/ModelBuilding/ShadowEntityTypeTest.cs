// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ShadowEntityTypeTest
    {
        [ConditionalFact]
        public virtual void Can_create_two_shadow_weak_owned_types()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity(
                "Customer", b =>
                {
                    b.Property<string>("CustomerId");

                    b.HasKey("CustomerId");

                    b.OwnsOne(
                        "CustomerDetails", "Details", b1 =>
                        {
                            b1.Property<string>("CustomerId");

                            b1.HasKey("CustomerId");

                            b1.HasOne("Customer")
                                .WithOne("Details")
                                .HasForeignKey("CustomerDetails", "CustomerId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                    b.OwnsOne(
                        "CustomerDetails", "AdditionalDetails", b1 =>
                        {
                            b1.Property<string>("CustomerId");

                            b1.HasKey("CustomerId");

                            b1.HasOne("Customer")
                                .WithOne("AdditionalDetails")
                                .HasForeignKey("CustomerDetails", "CustomerId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            var model = modelBuilder.Model;
            var ownership1 = model.FindEntityType("Customer").FindNavigation("Details").ForeignKey;
            var ownership2 = model.FindEntityType("Customer").FindNavigation("AdditionalDetails").ForeignKey;
            Assert.True(ownership1.IsRequired);
            Assert.True(ownership2.IsRequired);
            Assert.NotEqual(ownership1.DeclaringEntityType, ownership2.DeclaringEntityType);
            Assert.Equal(ownership1.Properties.Single().Name, ownership2.Properties.Single().Name);
            Assert.Equal(
                ownership1.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name,
                ownership2.DeclaringEntityType.FindPrimaryKey().Properties.Single().Name);
            Assert.Equal(2, model.GetEntityTypes().Count(e => e.Name == "CustomerDetails"));
        }

        [ConditionalFact]
        public virtual void Can_create_One_to_One_shadow_navigations_between_shadow_entity_types()
        {
            var modelBuilder = CreateModelBuilder();
            var foreignKey = modelBuilder.Entity("Order")
                .HasOne("OrderDetails", "OrderDetails")
                .WithOne("Order")
                .HasForeignKey("OrderDetails", "OrderId")
                .Metadata;

            Assert.Equal("OrderDetails", modelBuilder.Model.FindEntityType("Order")?.GetNavigations().Single().Name);
            Assert.Equal("Order", modelBuilder.Model.FindEntityType("OrderDetails")?.GetNavigations().Single().Name);
            Assert.True(foreignKey.IsUnique);

            Assert.Equal(
                CoreStrings.ShadowEntity("Order"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Can_create_One_to_Many_shadow_navigations_between_shadow_entity_types()
        {
            var modelBuilder = CreateModelBuilder();
            var foreignKey = modelBuilder.Entity("Order")
                .HasOne("Customer", "Customer")
                .WithMany("Orders")
                .Metadata;

            Assert.Equal("Customer", modelBuilder.Model.FindEntityType("Order")?.GetNavigations().Single().Name);
            Assert.Equal("Orders", modelBuilder.Model.FindEntityType("Customer")?.GetNavigations().Single().Name);
            Assert.False(foreignKey.IsUnique);

            Assert.Equal(
                CoreStrings.ShadowEntity("Customer"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        [ConditionalFact]
        public virtual void Cannot_create_navigation_on_non_shadow_entity_targeting_shadow_entity()
        {
            var modelBuilder = CreateModelBuilder();
            var orderEntityType = modelBuilder.Entity(typeof(Order));

            Assert.Equal(
                CoreStrings.NavigationToShadowEntity("Customer", typeof(Order).ShortDisplayName(), "Customer"),
                Assert.Throws<InvalidOperationException>(() => orderEntityType.HasOne("Customer", "Customer")).Message);
        }

        [ConditionalFact]
        public virtual void Cannot_create_shadow_navigation_between_non_shadow_entity_types()
        {
            var modelBuilder = CreateModelBuilder();
            var orderEntityType = modelBuilder.Entity(typeof(Order));

            Assert.Equal(
                CoreStrings.NoClrNavigation("CustomerNavigation", typeof(Order).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => orderEntityType.HasOne(typeof(Customer), "CustomerNavigation")).Message);
        }

        protected virtual ModelBuilder CreateModelBuilder()
            => InMemoryTestHelpers.Instance.CreateConventionBuilder();

        protected class Order
        {
            public int OrderId { get; set; }

            public int? CustomerId { get; set; }
            public Guid AnotherCustomerId { get; set; }
            public Customer Customer { get; set; }
        }

        protected class Customer
        {
            public int Id { get; set; }
            public Guid AlternateKey { get; set; }
            public string Name { get; set; }

            public IEnumerable<Order> Orders { get; set; }
        }
    }
}

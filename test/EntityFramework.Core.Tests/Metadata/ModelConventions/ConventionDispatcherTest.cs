// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.Conventions
{
    public class ConventionDispatcherTest
    {
        [Fact]
        public void OnEntityTypeAdded_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalEntityTypeBuilder entityTypeBuilder = null;
            var convention = new Mock<IEntityTypeConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>())).Returns<InternalEntityTypeBuilder>(b =>
                {
                    Assert.NotNull(b);
                    entityTypeBuilder = new InternalEntityTypeBuilder(b.Metadata, b.ModelBuilder);
                    return entityTypeBuilder;
                });
            conventions.EntityTypeAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IEntityTypeConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>())).Returns<InternalEntityTypeBuilder>(b =>
                {
                    Assert.Same(entityTypeBuilder, b);
                    return null;
                });
            conventions.EntityTypeAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IEntityTypeConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>())).Returns<InternalEntityTypeBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.EntityTypeAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            Assert.Null(builder.Entity(typeof(Order), ConfigurationSource.Convention));

            Assert.NotNull(entityTypeBuilder);
        }

        [Fact]
        public void OnBaseEntityTypeSet_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalEntityTypeBuilder entityTypeBuilder = null;
            var convention = new Mock<IBaseTypeConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<EntityType>()))
                .Returns<InternalEntityTypeBuilder, EntityType>((b, t) =>
                    {
                        Assert.NotNull(b);
                        Assert.Null(t);
                        entityTypeBuilder = b;
                        return true;
                    });
            conventions.BaseEntityTypeSetConventions.Add(convention.Object);

            var nullConvention = new Mock<IBaseTypeConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<EntityType>()))
                .Returns<InternalEntityTypeBuilder, EntityType>((b, t) =>
                    {
                        Assert.Null(t);
                        Assert.Same(entityTypeBuilder, b);
                        return false;
                    });
            conventions.BaseEntityTypeSetConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IBaseTypeConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<EntityType>()))
                .Returns<InternalEntityTypeBuilder, EntityType>((b, t) =>
                    {
                        Assert.False(true);
                        return false;
                    });
            conventions.BaseEntityTypeSetConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions)).Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            Assert.NotNull(builder.HasBaseType(typeof(Order), ConfigurationSource.Convention));

            Assert.NotNull(entityTypeBuilder);
        }

        [Fact]
        public void OnEntityTypeMemberIgnored_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalEntityTypeBuilder entityTypeBuilder = null;
            var convention = new Mock<IEntityTypeMemberIgnoredConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<string>()))
                .Returns<InternalEntityTypeBuilder, string>((b, t) =>
                    {
                        Assert.NotNull(b);
                        Assert.Equal("A", t);
                        entityTypeBuilder = b;
                        return true;
                    });
            conventions.EntityTypeMemberIgnoredConventions.Add(convention.Object);

            var nullConvention = new Mock<IEntityTypeMemberIgnoredConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<string>()))
                .Returns<InternalEntityTypeBuilder, string>((b, t) =>
                    {
                        Assert.Equal("A", t);
                        Assert.Same(entityTypeBuilder, b);
                        return false;
                    });
            conventions.EntityTypeMemberIgnoredConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IEntityTypeMemberIgnoredConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<string>()))
                .Returns<InternalEntityTypeBuilder, string>((b, t) =>
                    {
                        Assert.False(true);
                        return false;
                    });
            conventions.EntityTypeMemberIgnoredConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions)).Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            Assert.NotNull(builder.Ignore("A", ConfigurationSource.Convention));

            Assert.NotNull(entityTypeBuilder);
        }

        [Fact]
        public void OnPropertyAdded_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalPropertyBuilder propertyBuilder = null;
            var convention = new Mock<IPropertyConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.NotNull(b);
                    Assert.Equal("OrderId", b.Metadata.Name);
                    Assert.Equal(typeof(int), b.Metadata.ClrType);
                    propertyBuilder = new InternalPropertyBuilder(b.Metadata, b.ModelBuilder);
                    return propertyBuilder;
                });
            conventions.PropertyAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IPropertyConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.Same(propertyBuilder, b);
                    return null;
                });
            conventions.PropertyAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IPropertyConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.PropertyAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var explicitKeyBuilder = entityBuilder.Property("OrderId", typeof(int), ConfigurationSource.Convention);

            Assert.Null(explicitKeyBuilder);
            Assert.NotNull(propertyBuilder);
        }

        [Fact]
        public void OnPropertyAdded_calls_apply_on_conventions_in_order_for_non_shadow_property()
        {
            var conventions = new ConventionSet();

            InternalPropertyBuilder propertyBuilder = null;
            var convention = new Mock<IPropertyConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.NotNull(b);
                    Assert.Equal("OrderId", b.Metadata.Name);
                    Assert.Equal(typeof(int), b.Metadata.ClrType);
                    Assert.False(b.Metadata.IsShadowProperty);
                    propertyBuilder = new InternalPropertyBuilder(b.Metadata, b.ModelBuilder);
                    return propertyBuilder;
                });
            conventions.PropertyAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IPropertyConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.Same(propertyBuilder, b);
                    return null;
                });
            conventions.PropertyAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IPropertyConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalPropertyBuilder>())).Returns<InternalPropertyBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.PropertyAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var explicitKeyBuilder = entityBuilder.Property(Order.OrderIdProperty, ConfigurationSource.Convention);

            Assert.Null(explicitKeyBuilder);
            Assert.NotNull(propertyBuilder);
        }

        [Fact]
        public void OnForeignKeyAdded_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalRelationshipBuilder relationshipBuilder = null;
            var convention = new Mock<IForeignKeyConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.NotNull(b);
                    relationshipBuilder = new InternalRelationshipBuilder(b.Metadata, b.ModelBuilder);
                    return relationshipBuilder;
                });
            conventions.ForeignKeyAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IForeignKeyConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.Same(relationshipBuilder, b);
                    return null;
                });
            conventions.ForeignKeyAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IForeignKeyConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.ForeignKeyAddedConventions.Add(extraConvention.Object);

            var modelBuilder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention);
            Assert.Null(entityBuilder.Relationship(entityBuilder, ConfigurationSource.Convention));

            Assert.NotNull(relationshipBuilder);
        }

        [Fact]
        public void OnKeyAdded_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalKeyBuilder keyBuilder = null;
            var convention = new Mock<IKeyConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>())).Returns<InternalKeyBuilder>(b =>
                {
                    Assert.NotNull(b);
                    keyBuilder = new InternalKeyBuilder(b.Metadata, b.ModelBuilder);
                    return keyBuilder;
                });
            conventions.KeyAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IKeyConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>())).Returns<InternalKeyBuilder>(b =>
                {
                    Assert.Same(keyBuilder, b);
                    return null;
                });
            conventions.KeyAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IKeyConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>())).Returns<InternalKeyBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.KeyAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var explicitKeyBuilder = entityBuilder.HasKey(new List<string> { "OrderId" }, ConfigurationSource.Convention);

            Assert.Null(explicitKeyBuilder);
            Assert.NotNull(keyBuilder);
        }

        [Fact]
        public void OnPrimaryKeySet_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalKeyBuilder internalKeyBuilder = null;
            var convention = new Mock<IPrimaryKeyConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>(), It.IsAny<Key>()))
                .Returns<InternalKeyBuilder, Key>((b, t) =>
                    {
                        Assert.NotNull(b);
                        Assert.Null(t);
                        internalKeyBuilder = b;
                        return true;
                    });
            conventions.PrimaryKeySetConventions.Add(convention.Object);

            var nullConvention = new Mock<IPrimaryKeyConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>(), It.IsAny<Key>()))
                .Returns<InternalKeyBuilder, Key>((b, t) =>
                    {
                        Assert.Null(t);
                        Assert.Same(internalKeyBuilder, b);
                        return false;
                    });
            conventions.PrimaryKeySetConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IPrimaryKeyConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalKeyBuilder>(), It.IsAny<Key>()))
                .Returns<InternalKeyBuilder, Key>((b, t) =>
                    {
                        Assert.False(true);
                        return false;
                    });
            conventions.PrimaryKeySetConventions.Add(extraConvention.Object);

            var entityBuilder = new InternalModelBuilder(new Model(conventions))
                .Entity(typeof(Order), ConfigurationSource.Convention);

            entityBuilder.HasKey(new[] { "OrderId" }, ConfigurationSource.Convention);
            Assert.NotNull(entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention));

            Assert.NotNull(internalKeyBuilder);
        }

        [Fact]
        public void OnForeignKeyRemoved_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            var foreignKeyRemoved = false;

            var convention = new Mock<IForeignKeyRemovedConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<ForeignKey>()))
                .Callback(() => foreignKeyRemoved = true);
            conventions.ForeignKeyRemovedConventions.Add(convention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var foreignKey = entityBuilder.Metadata.AddForeignKey(
                new[] { entityBuilder.Property("FK", typeof(int), ConfigurationSource.Convention).Metadata },
                entityBuilder.HasKey(new[] { "OrderId" }, ConfigurationSource.Convention).Metadata,
                entityBuilder.Metadata);
            var conventionDispatcher = new ConventionDispatcher(conventions);
            conventionDispatcher.OnForeignKeyRemoved(entityBuilder, foreignKey);

            Assert.True(foreignKeyRemoved);
        }

        [Fact]
        public void OnNavigationAdded_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalRelationshipBuilder relationshipBuilder = null;
            var orderIgnored = false;
            var orderDetailsIgnored = false;
            var convention = new Mock<INavigationConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>(), It.IsAny<Navigation>())).Returns((InternalRelationshipBuilder b, Navigation n) =>
                {
                    Assert.NotNull(b);
                    relationshipBuilder = new InternalRelationshipBuilder(b.Metadata, b.ModelBuilder);
                    return relationshipBuilder;
                });
            conventions.NavigationAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<INavigationConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>(), It.IsAny<Navigation>())).Returns((InternalRelationshipBuilder b, Navigation n) =>
                {
                    Assert.Same(relationshipBuilder, b);
                    if (n.Name == "Order")
                    {
                        orderIgnored = true;
                    }
                    if (n.Name == "OrderDetails")
                    {
                        orderDetailsIgnored = true;
                    }
                    return null;
                });
            conventions.NavigationAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<INavigationConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>(), It.IsAny<Navigation>())).Returns((InternalRelationshipBuilder b, Navigation n) =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.NavigationAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);

            Assert.Null(dependentEntityBuilder.Relationship(principalEntityBuilder, nameof(OrderDetails.Order), nameof(Order.OrderDetails), ConfigurationSource.Convention));

            Assert.True(orderIgnored);
            Assert.False(orderDetailsIgnored);
            Assert.NotNull(relationshipBuilder);
        }

        [Fact]
        public void OnNavigationRemoved_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalEntityTypeBuilder dependentEntityTypeBuilderFromConvention = null;
            InternalEntityTypeBuilder principalEntityBuilderFromConvention = null;
            var convention = new Mock<INavigationRemovedConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<string>())).Returns((InternalEntityTypeBuilder s, InternalEntityTypeBuilder t, string n) =>
                {
                    dependentEntityTypeBuilderFromConvention = s;
                    principalEntityBuilderFromConvention = t;
                    Assert.Equal(nameof(OrderDetails.Order), n);
                    return false;
                });
            conventions.NavigationRemovedConventions.Add(convention.Object);

            var extraConvention = new Mock<INavigationRemovedConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<string>())).Returns((InternalEntityTypeBuilder s, InternalEntityTypeBuilder t, string n) =>
                {
                    Assert.False(true);
                    return false;
                });
            conventions.NavigationRemovedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(conventions));

            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityBuilder.Relationship(principalEntityBuilder, nameof(OrderDetails.Order), nameof(Order.OrderDetails), ConfigurationSource.Convention);
            relationshipBuilder.DependentToPrincipal(null, ConfigurationSource.Convention);

            Assert.NotNull(relationshipBuilder);
            Assert.Same(dependentEntityTypeBuilderFromConvention, dependentEntityBuilder);
            Assert.Same(principalEntityBuilderFromConvention, principalEntityBuilder);
        }

        [Fact]
        public void InitializingModel_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            var nullConventionCalled = false;

            InternalModelBuilder modelBuilder = null;
            var convention = new Mock<IModelConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    Assert.NotNull(b);
                    modelBuilder = new InternalModelBuilder(b.Metadata);
                    return b;
                });
            conventions.ModelInitializedConventions.Add(convention.Object);

            var nullConvention = new Mock<IModelConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    nullConventionCalled = true;
                    return null;
                });
            conventions.ModelInitializedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IModelConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.ModelInitializedConventions.Add(extraConvention.Object);

            var builder = new ModelBuilder(conventions);

            Assert.NotNull(builder);
            Assert.True(nullConventionCalled);
            Assert.NotNull(modelBuilder);
        }

        [Fact]
        public void ValidatingModel_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            var nullConventionCalled = false;

            InternalModelBuilder modelBuilder = null;
            var convention = new Mock<IModelConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    Assert.NotNull(b);
                    modelBuilder = new InternalModelBuilder(b.Metadata);
                    return b;
                });
            conventions.ModelBuiltConventions.Add(convention.Object);

            var nullConvention = new Mock<IModelConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    nullConventionCalled = true;
                    return null;
                });
            conventions.ModelBuiltConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IModelConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.ModelBuiltConventions.Add(extraConvention.Object);

            var builder = new ModelBuilder(conventions).Validate();

            Assert.NotNull(builder);
            Assert.True(nullConventionCalled);
            Assert.NotNull(modelBuilder);
        }

        [Fact]
        public void OnPropertyNullableChanged_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            var convention1 = new PropertyNullableConvention(false);
            var convention2 = new PropertyNullableConvention(true);
            var convention3 = new PropertyNullableConvention(false);

            conventions.PropertyNullableChangedConventions.Add(convention1);
            conventions.PropertyNullableChangedConventions.Add(convention2);
            conventions.PropertyNullableChangedConventions.Add(convention3);

            var builder = new ModelBuilder(conventions);

            builder.Entity<Order>().Property(e => e.Name).IsRequired();

            Assert.Equal(new bool?[] { false }, convention1.Calls);
            Assert.Equal(new bool?[] { false }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            builder.Entity<Order>().Property(e => e.Name).IsRequired(false);

            Assert.Equal(new bool?[] { false, true }, convention1.Calls);
            Assert.Equal(new bool?[] { false, true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            builder.Entity<Order>().Property(e => e.Name).IsRequired(false);

            Assert.Equal(new bool?[] { false, true }, convention1.Calls);
            Assert.Equal(new bool?[] { false, true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            builder.Entity<Order>().Property(e => e.Name).IsRequired();

            Assert.Equal(new bool?[] { false, true, false }, convention1.Calls);
            Assert.Equal(new bool?[] { false, true, false }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class PropertyNullableConvention : IPropertyNullableConvention
        {
            public readonly List<bool?> Calls = new List<bool?>();
            private readonly bool _terminate;

            public PropertyNullableConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public bool Apply(InternalPropertyBuilder propertyBuilder)
            {
                Calls.Add(propertyBuilder.Metadata.IsNullable);

                return !_terminate;
            }
        }

        private class Order
        {
            public static readonly PropertyInfo OrderIdProperty = typeof(Order).GetProperty("OrderId");

            public int OrderId { get; set; }

            public string Name { get; set; }

            public virtual OrderDetails OrderDetails { get; set; }
        }

        private class SpecialOrder : Order
        {
        }

        private class OrderDetails
        {
            public int Id { get; set; }
            public virtual Order Order { get; set; }
        }
    }
}

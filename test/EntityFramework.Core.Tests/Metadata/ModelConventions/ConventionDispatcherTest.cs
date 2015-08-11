// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
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

            var builder = new InternalModelBuilder(new Model(), conventions);

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

            var builder = new InternalModelBuilder(new Model(), conventions).Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            Assert.NotNull(builder.BaseType(typeof(Order), ConfigurationSource.Convention));

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

            var builder = new InternalModelBuilder(new Model(), conventions);

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var explicitKeyBuilder = entityBuilder.Property("OrderId", typeof(int), ConfigurationSource.Convention);

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
                    relationshipBuilder = new InternalRelationshipBuilder(b.Metadata, b.ModelBuilder, null);
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

            var builder = new InternalModelBuilder(new Model(), conventions);

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention);
            Assert.Null(entityBuilder.Relationship(typeof(Order), typeof(Order), null, null, ConfigurationSource.Convention));

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

            var builder = new InternalModelBuilder(new Model(), conventions);

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var explicitKeyBuilder = entityBuilder.Key(new List<string> { "OrderId" }, ConfigurationSource.Convention);

            Assert.Null(explicitKeyBuilder);
            Assert.NotNull(keyBuilder);
        }

        [Fact]
        public void OnForeignKeyRemoved_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            var foreignKeyRemoved = false;

            var convention = new Mock<IForeignKeyRemovedConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityTypeBuilder>(), It.IsAny<ForeignKey>())).Callback(() => foreignKeyRemoved = true);
            conventions.ForeignKeyRemovedConventions.Add(convention.Object);

            var builder = new InternalModelBuilder(new Model(), conventions);

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var foreignKey = new ForeignKey(
                new[] { entityBuilder.Property("FK", typeof(int), ConfigurationSource.Convention).Metadata },
                entityBuilder.Key(new[] { entityBuilder.Property("OrderId", typeof(int), ConfigurationSource.Convention).Metadata }, ConfigurationSource.Convention).Metadata,
                entityBuilder.Metadata,
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
                relationshipBuilder = new InternalRelationshipBuilder(b.Metadata, b.ModelBuilder, ConfigurationSource.Convention);
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

            var builder = new InternalModelBuilder(new Model(), conventions);

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention);

            entityBuilder.Relationship(typeof(Order), typeof(OrderDetails), "Order", "OrderDetails", ConfigurationSource.Convention, isUnique: true);

            Assert.True(orderIgnored);
            Assert.True(orderDetailsIgnored);
            Assert.NotNull(relationshipBuilder);
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
                modelBuilder = new InternalModelBuilder(b.Metadata, new ConventionSet());
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
                modelBuilder = new InternalModelBuilder(b.Metadata, new ConventionSet());
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

        private class Order
        {
            public int OrderId { get; set; }

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

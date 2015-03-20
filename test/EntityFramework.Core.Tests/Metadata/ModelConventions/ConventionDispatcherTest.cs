// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.ModelConventions
{
    public class ConventionDispatcherTest
    {
        [Fact]
        public void OnEntityTypeAdded_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalEntityBuilder entityBuilder = null;
            var convention = new Mock<IEntityTypeConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityBuilder>())).Returns<InternalEntityBuilder>(b =>
                {
                    Assert.NotNull(b);
                    entityBuilder = new InternalEntityBuilder(b.Metadata, b.ModelBuilder);
                    return entityBuilder;
                });
            conventions.EntityTypeAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IEntityTypeConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalEntityBuilder>())).Returns<InternalEntityBuilder>(b =>
                {
                    Assert.Same(entityBuilder, b);
                    return null;
                });
            conventions.EntityTypeAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IEntityTypeConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityBuilder>())).Returns<InternalEntityBuilder>(b =>
                {
                    Assert.False(true);
                    return null;
                });
            conventions.EntityTypeAddedConventions.Add(extraConvention.Object);

            var builder = new InternalModelBuilder(new Model(), conventions);

            Assert.Null(builder.Entity(typeof(Order), ConfigurationSource.Convention));

            Assert.NotNull(entityBuilder);
        }

        [Fact]
        public void OnRelationshipAdded_calls_apply_on_conventions_in_order()
        {
            var conventions = new ConventionSet();

            InternalRelationshipBuilder relationsipBuilder = null;
            var convention = new Mock<IRelationshipConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.NotNull(b);
                    relationsipBuilder = new InternalRelationshipBuilder(b.Metadata, b.ModelBuilder, null);
                    return relationsipBuilder;
                });
            conventions.ForeignKeyAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IRelationshipConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.Same(relationsipBuilder, b);
                    return null;
                });
            conventions.ForeignKeyAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IRelationshipConvention>();
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

            Assert.NotNull(relationsipBuilder);
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
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityBuilder>(), It.IsAny<ForeignKey>())).Callback(() => foreignKeyRemoved = true);
            conventions.ForeignKeyRemovedConventions.Add(convention.Object);

            var builder = new InternalModelBuilder(new Model(), conventions);

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var foreignKey = new ForeignKey(
                new[] { entityBuilder.Property(typeof(int), "FK", ConfigurationSource.Convention).Metadata },
                entityBuilder.Key(new[] { entityBuilder.Property(typeof(int), "OrderId", ConfigurationSource.Convention).Metadata }, ConfigurationSource.Convention).Metadata);
            var conventionDispatcher = new ConventionDispatcher(conventions);
            conventionDispatcher.OnForeignKeyRemoved(entityBuilder, foreignKey);

            Assert.True(foreignKeyRemoved);
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
            conventions.ModelConventions.Add(convention.Object);

            var nullConvention = new Mock<IModelConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
            {
                nullConventionCalled = true;
                return null;
            });
            conventions.ModelConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IModelConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalModelBuilder>())).Returns<InternalModelBuilder>(b =>
            {
                Assert.False(true);
                return null;
            });
            conventions.ModelConventions.Add(extraConvention.Object);

            var builder = new ModelBuilder(conventions);

            Assert.NotNull(builder);
            Assert.True(nullConventionCalled);
            Assert.NotNull(modelBuilder);
        }

        private class Order
        {
            public int OrderId { get; set; }
        }
    }
}

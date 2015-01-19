// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.ModelConventions
{
    public class ConventionsDispatcherTest
    {
        [Fact]
        public void OnEntityTypeAdded_calls_apply_on_conventions_in_order()
        {
            var conventionDispatcher = new ConventionsDispatcher();
            var builder = new InternalModelBuilder(new Model(), conventionDispatcher);

            InternalEntityBuilder entityBuilder = null;
            var convention = new Mock<IEntityTypeConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalEntityBuilder>())).Returns<InternalEntityBuilder>(b =>
                {
                    Assert.NotNull(b);
                    entityBuilder = new InternalEntityBuilder(b.Metadata, b.ModelBuilder);
                    return entityBuilder;
                });
            conventionDispatcher.EntityTypeAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IEntityTypeConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalEntityBuilder>())).Returns<InternalEntityBuilder>(b =>
                {
                    Assert.Same(entityBuilder, b);
                    return null;
                });
            conventionDispatcher.EntityTypeAddedConventions.Add(nullConvention.Object);

            var extraConvention = new Mock<IEntityTypeConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalEntityBuilder>())).Returns<InternalEntityBuilder>(b =>
            {
                Assert.False(true);
                return null;
            });

            conventionDispatcher.EntityTypeAddedConventions.Add(extraConvention.Object);

            Assert.Null(builder.Entity(typeof(Order), ConfigurationSource.Convention));

            Assert.NotNull(entityBuilder);
        }

        [Fact]
        public void OnForeignKeyAdded_calls_apply_on_conventions_in_order()
        {
            var conventionDispatcher = new ConventionsDispatcher();
            var builder = new InternalModelBuilder(new Model(), conventionDispatcher);

            InternalRelationshipBuilder relationsipBuilder = null;
            var convention = new Mock<IRelationshipConvention>();
            convention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.NotNull(b);
                    relationsipBuilder = new InternalRelationshipBuilder(b.Metadata, b.ModelBuilder, null);
                    return relationsipBuilder;
                });
            conventionDispatcher.ForeignKeyAddedConventions.Add(convention.Object);

            var nullConvention = new Mock<IRelationshipConvention>();
            nullConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
                {
                    Assert.Same(relationsipBuilder, b);
                    return null;
                });

            conventionDispatcher.ForeignKeyAddedConventions.Add(nullConvention.Object);
            
            var extraConvention = new Mock<IRelationshipConvention>();
            extraConvention.Setup(c => c.Apply(It.IsAny<InternalRelationshipBuilder>())).Returns<InternalRelationshipBuilder>(b =>
            {
                Assert.False(true);
                return null;
            });

            conventionDispatcher.ForeignKeyAddedConventions.Add(extraConvention.Object);

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention);
            Assert.Null(entityBuilder.Relationship(typeof(Order), typeof(Order), null, null, ConfigurationSource.Convention));

            Assert.NotNull(relationsipBuilder);
        }

        protected virtual ModelBuilder CreateModelBuilder()
        {
            return TestHelpers.CreateConventionBuilder();
        }

        private class Order
        {
            public int OrderId { get; set; }
        }
    }
}

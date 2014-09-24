// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalModelBuilderTest
    {
        [Fact]
        public void Entity_returns_same_instance_for_clr_type()
        {
            var model = new Model();
            var modelBuilder = new InternalModelBuilder(model, null);

            var entityBuilder = modelBuilder.Entity(typeof(Customer));

            Assert.NotNull(entityBuilder);
            Assert.Same(entityBuilder, modelBuilder.Entity(typeof(Customer).FullName));
        }

        [Fact]
        public void Entity_returns_same_instance_for_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = new InternalModelBuilder(model, null);

            var entityBuilder = modelBuilder.Entity(typeof(Customer).FullName);

            Assert.NotNull(entityBuilder);
            Assert.Same(entityBuilder, modelBuilder.Entity(typeof(Customer)));
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
